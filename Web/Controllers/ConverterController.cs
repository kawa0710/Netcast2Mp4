using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Web.Extensions;
using Web.Models;
using Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using RayITUtilityCore31;

namespace Web.Controllers
{
    public class ConverterController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Netcast2Mp4Context _context;
        private readonly IWebHostEnvironment _webhostEnvironment;

        private readonly string _folder = @".\UserData";
        private readonly int imgSizeLimit = 1 * (1024 * 1024); // 1MB

        public ConverterController(ILogger<HomeController> logger,
            Netcast2Mp4Context context,
            IWebHostEnvironment webhostEnvironment)
        {
            _logger = logger;
            _context = context;
            _webhostEnvironment = webhostEnvironment;
        }

        #region Parameters

        public class GetRssInfoParm
        {
            public string url { get; set; }
        }

        #endregion Parameters

        #region Funcions

        private async Task<bool> IsUserExist(int id)
        {
            return await _context.User.AnyAsync(e => e.UsrSn == id);
        }

        private async Task<Channel> GetChannelByUser(int usrSn)
        {
            return await _context.Channel
                .Include(ch => ch.Netcast)
                .ThenInclude(nc => nc.Image)
                .FirstOrDefaultAsync(m => m.UsrSn == LoginSn);
        }

        private async Task<bool> IsFlimsConverting()
        {
            var q_flim = from a in _context.Flim
                         join b in _context.Netcast on a.NtcSn equals b.NtcSn
                         join c in _context.Channel on b.ChnSn equals c.ChnSn
                         where c.UsrSn == LoginSn && a.FlmStatus == 0 && a.FlmIsActive
                         select a;

            return await q_flim.AnyAsync();
        }

        private async Task<bool> IsNetcastExist(string guid)
        {
            var q_netcast = from a in _context.Netcast
                         where a.NtcGuid == guid
                         select a;

            return await q_netcast.AnyAsync();
        }

        #endregion Funcions

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadData()
        {
            if (LoginSn <= 0)
                return NotFound();

            try
            {
                if (! (await IsUserExist(LoginSn)))
                {
                    return Json("-1");
                }
                else
                {
                    var channel = await GetChannelByUser(LoginSn);

                    if (channel == null || channel.Netcast.Count == 0)
                        return Json("0");

                    var netcasList = new List<NetcastViewModel>();
                    foreach (var item in channel.Netcast)
                    {
                        var netcastModel = JsonSerializer.Deserialize<NetcastModel>(item.NtcInfo);
                        var netcast = new NetcastViewModel()
                        {
                            NtcSn = item.NtcSn,
                            ChnSn = channel.ChnSn,
                            NtcInfo = JsonSerializer.Serialize(netcastModel),
                            Title = netcastModel.Title,
                            Summary = netcastModel.Summary,
                            Uri = netcastModel.Uri,
                            MediaType = netcastModel.MediaType,
                            ImgFilename = item.Image.FirstOrDefault()?.ImgGuid16
                        };
                        netcasList.Add(netcast);
                    }

                    if (netcasList.Count == 0)
                        return Json("0");

                    return Json(netcasList.Select(x => new
                    {
                        x.NtcSn,
                        x.Title,
                        Summary = x.Summary.Substring(0, Math.Min(x.Summary.Length, 150))
                            + (x.Summary.Length > 150 ? "..." : ""),
                        x.Uri,
                        x.MediaType,
                        x.ImgFilename
                    }).OrderByDescending(x => x.Title));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Json("-1");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRssInfo([FromBody] GetRssInfoParm param)
        {
            string s = Common.GetGuid16();
            SyndicationFeed feed = null;
            ChannelModel rssInfo = null;

            if (string.IsNullOrWhiteSpace(param.url))
                return Json("-2");

            try
            {
                XmlReader reader = XmlReader.Create(HttpUtility.UrlDecode(param.url));
                feed = SyndicationFeed.Load(reader);
                reader.Close();

                var links = feed.Links.Where(x => x.MediaType == "application/rss+xml").FirstOrDefault();
                rssInfo = new ChannelModel()
                {
                    Title = feed.Title.Text,
                    Description = feed.Description.Text,
                    ImageUrl = feed.ImageUrl.AbsoluteUri,
                    Uri = links?.Uri.AbsoluteUri,
                    MediaType = links?.MediaType,
                    Generator = feed.Generator
                };

                var channel = await GetChannelByUser(LoginSn);
                if (channel == null || channel.ChnSn == 0)
                {
                    channel = new Models.Channel()
                    {
                        UsrSn = LoginSn,
                        ChnStatus = 0,
                        ChnInfo = JsonSerializer.Serialize(rssInfo),
                        ChnCat = DateTime.Now
                    };
                    await _context.Channel.AddAsync(channel);
                    await _context.SaveChangesAsync();
                }                

                if (channel.ChnSn > 0)
                { //新增成功或已有資料
                    if (await IsFlimsConverting()) //轉檔中
                        return Json("-3");

                    var netcasList = new List<NetcastViewModel>();
                    foreach (var item in feed.Items)
                    {
                        if (! (await IsNetcastExist(item.Id)))
                        {
                            var links2 = item.Links.Where(x => x.MediaType == "audio/mpeg").FirstOrDefault();
                            var netcastModel = new NetcastModel()
                            {
                                Title = item.Title.Text,
                                Summary = item.Summary.Text,
                                PublishDate = item.PublishDate.DateTime,
                                Uri = links2?.Uri.AbsoluteUri,
                                MediaType = links2?.MediaType,
                                Author = item.GetAuthor(),
                                Episode = item.GetEpisode(),
                                Season = item.GetSeason(),
                                Keywords = item.GetKeywords(),
                                Explicit = item.GetExplicit(),
                                ImageUrl = item.GetImageUrl(),
                                Duration = item.GetDuration(),
                                Guid = item.Id,
                            };

                            var netcast = new NetcastViewModel()
                            {
                                ChnSn = channel.ChnSn,
                                NtcInfo = JsonSerializer.Serialize(netcastModel),
                                NtcGuid = netcastModel.Guid,
                                Title = netcastModel.Title,
                                Summary = netcastModel.Summary,
                                Uri = netcastModel.Uri,
                                MediaType = netcastModel.MediaType,
                            };

                            await _context.Netcast.AddAsync(netcast);
                            netcasList.Add(netcast);
                        }
                    }
                    await _context.SaveChangesAsync();

                    return Json(netcasList.Select(x => new
                    {
                        x.NtcSn,
                        x.Title,
                        Summary = x.Summary.Substring(0, Math.Min(x.Summary.Length, 150))
                            + (x.Summary.Length > 150 ? "..." : ""),
                        x.Uri,
                        x.MediaType,
                    }));
                }
                else
                    return Json("0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, param.url, feed);
                return Json("-1");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendData([FromForm] IFormFile pic, string ntcsn_jsonlist)
        {
            if (LoginSn <= 0)
                return NotFound();

            try
            {
                if (!(await IsUserExist(LoginSn)) || string.IsNullOrWhiteSpace(ntcsn_jsonlist) || pic.Length > imgSizeLimit)
                {
                    return Json("-1");
                }
                else
                {
                    int[] ntcsn_list = JsonSerializer.Deserialize<int[]>(ntcsn_jsonlist);
                    if (ntcsn_list.Length == 0)
                        return Json("-1");

                    var channel = await GetChannelByUser(LoginSn);
                    if (channel == null || channel.ChnSn == 0)
                        return Json("-1");

                    var netcast_list = channel.Netcast.Select(n => new Netcast { NtcSn = n.NtcSn, ChnSn = n.ChnSn, NtcInfo = "" }).ToList();
                    //避免一次取太多資料

                    #region 檢查要轉的檔案有沒有存在DB

                    var isNetcastExist = true;
                    foreach (var n in ntcsn_list)
                    {
                        if (!netcast_list.Any(x => x.NtcSn == n))
                            isNetcastExist = false;
                    }

                    if (!isNetcastExist)
                        return Json("-1");

                    #endregion 檢查要轉的檔案有沒有存在DB

                    var img_new_name = Common.GetGuid16();
                    var ext = Path.GetExtension(pic.FileName);
                    var path = $@"{_folder}\{img_new_name}{ext}";
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await pic.CopyToAsync(stream);
                    }

                    #region 寫入圖檔資料Image及Flim

                    var img_list = new List<Image>();
                    var flim_list = new List<Flim>();
                    netcast_list = await _context.Netcast.Where(x => ntcsn_list.Contains(x.NtcSn)).ToListAsync();

                    //要轉檔的netcast共用同一個圖檔
                    foreach (var n in ntcsn_list)
                    {
                        img_list.Add(new Image()
                        {
                            NtcSn = n,
                            ImgGuid16 = img_new_name,
                            ImgExt = ext
                        });

                        var flim_title = "";
                        var ntcInfo = netcast_list.Find(x => x.NtcSn == n).NtcInfo;
                        if (!string.IsNullOrWhiteSpace(ntcInfo))
                            flim_title = Common.RemoveIllegalFileNameChars((JsonSerializer.Deserialize<NetcastModel>(ntcInfo)).Title);
                        if (flim_title.Length > 96) //檔名長度100-副檔(.mp4)長度
                            flim_title = flim_title.Substring(0, 96);

                        flim_list.Add(new Flim()
                        {
                            NtcSn = n,
                            FlmFileName = $"{flim_title}.mp4",
                            FlmStatus = 0,
                            FlmCat = DateTime.Now,
                            FlmMat = DateTime.Now,
                            FlmIsActive = true,
                            FlmSha256 = "" //mp4檔產生時才寫入
                        });
                    }
                    await _context.AddRangeAsync(img_list);
                    await _context.AddRangeAsync(flim_list);

                    #endregion 寫入圖檔資料Image及Flim

                    await _context.SaveChangesAsync();

                    return Json("0");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Json("-1");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetImages()
        {
            if (LoginSn <= 0)
                return NotFound();

            try
            {
                if (! (await IsUserExist(LoginSn)))
                {
                    return Json("-1");
                }
                else
                {
                    var q_image = from a in _context.Image
                                  join b in _context.Netcast on a.NtcSn equals b.NtcSn
                                  join c in _context.Channel on b.ChnSn equals c.ChnSn
                                  where c.UsrSn == LoginSn
                                  select new ImageViewModel
                                  {
                                      ImgGuid16 = a.ImgGuid16,
                                      ImgExt = a.ImgExt
                                  };

                    if (q_image.Count() == 0)
                        return Json("0");
                    else
                    {
                        var image_list = await q_image.Distinct().ToListAsync();
                        foreach (var i in image_list)
                        {
                            var path = Path.Combine(_webhostEnvironment.ContentRootPath, "UserData", i.ImgGuid16 + i.ImgExt);
                            byte[] b = System.IO.File.ReadAllBytes(path);
                            i.Base64 = $"data:image/{i.ImgExt.Substring(1)};base64,{Convert.ToBase64String(b)}";
                        }
                        
                        return Json(image_list);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Json("-1");
            }
        }

    }
}