using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Models;
using Web.ViewModels;

namespace Web.Controllers
{
    public class FlimsController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Netcast2Mp4Context _context;
        private readonly IWebHostEnvironment _webhostEnvironment;

        public FlimsController(ILogger<HomeController> logger,
            Netcast2Mp4Context context,
            IWebHostEnvironment webhostEnvironment)
        {
            _logger = logger;
            _context = context;
            _webhostEnvironment = webhostEnvironment;
        }

        // GET: Flims
        public async Task<IActionResult> Index()
        {
            if (LoginSn <= 0)
                return NotFound();

            try
            {
                if (!(_context.User.Any(e => e.UsrSn == LoginSn)))
                {
                    return Json("-1");
                }
                else
                {
                    var q_flim = from a in _context.Flim
                                 join b in _context.Netcast on a.NtcSn equals b.NtcSn
                                 join c in _context.Channel on b.ChnSn equals c.ChnSn
                                 where c.UsrSn == LoginSn && a.FlmIsActive
                                 select new FlimViewModel
                                 {
                                     FlmSn = a.FlmSn,
                                     NtcSn = a.NtcSn,
                                     FlmFileName = a.FlmFileName,
                                     FlmStatus = a.FlmStatus,
                                     FlmCat = a.FlmCat,
                                     FlmMat = a.FlmMat,
                                     FlmIsActive = a.FlmIsActive,
                                     FlmSha256 = a.FlmSha256
                                 };
                    return View(await q_flim.ToListAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Json("-1");
            }
        }

        // GET: Flims/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flim = await _context.Flim.FirstOrDefaultAsync(m => m.FlmSn == id);
            if (flim == null)
            {
                return NotFound();
            }

            var path = Path.Combine(_webhostEnvironment.ContentRootPath, "UserData", id.Value.ToString() + ".mp4");
            if (!System.IO.File.Exists(path))
                return NotFound();
            else
            {
                byte[] b = System.IO.File.ReadAllBytes(path);
                flim.FlmStatus = 2; //已下載
                flim.FlmMat = DateTime.Now;
                _context.Flim.Update(flim);
                _context.SaveChanges();

                return new FileContentResult(b, "application/octet-stream")
                {
                    FileDownloadName = flim.FlmFileName
                };
            }
        }

        //// GET: Flims
        //public async Task<IActionResult> Index()
        //{
        //    var netcast2Mp4Context = _context.Flim.Include(f => f.NtcSnNavigation);
        //    return View(await netcast2Mp4Context.ToListAsync());
        //}

        //// GET: Flims/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var flim = await _context.Flim
        //        .Include(f => f.NtcSnNavigation)
        //        .FirstOrDefaultAsync(m => m.FlmSn == id);
        //    if (flim == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(flim);
        //}

        //// GET: Flims/Create
        //public IActionResult Create()
        //{
        //    ViewData["NtcSn"] = new SelectList(_context.Netcast, "NtcSn", "NtcInfo");
        //    return View();
        //}

        //// POST: Flims/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("FlmSn,NtcSn,FlmFileName,FlmStatus,FlmCat,FlmMat,FlmIsActive")] Flim flim)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(flim);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["NtcSn"] = new SelectList(_context.Netcast, "NtcSn", "NtcInfo", flim.NtcSn);
        //    return View(flim);
        //}

        //// GET: Flims/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var flim = await _context.Flim.FindAsync(id);
        //    if (flim == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["NtcSn"] = new SelectList(_context.Netcast, "NtcSn", "NtcInfo", flim.NtcSn);
        //    return View(flim);
        //}

        //// POST: Flims/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("FlmSn,NtcSn,FlmFileName,FlmStatus,FlmCat,FlmMat,FlmIsActive")] Flim flim)
        //{
        //    if (id != flim.FlmSn)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(flim);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!FlimExists(flim.FlmSn))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["NtcSn"] = new SelectList(_context.Netcast, "NtcSn", "NtcInfo", flim.NtcSn);
        //    return View(flim);
        //}

        //// GET: Flims/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var flim = await _context.Flim
        //        .Include(f => f.NtcSnNavigation)
        //        .FirstOrDefaultAsync(m => m.FlmSn == id);
        //    if (flim == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(flim);
        //}

        //// POST: Flims/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var flim = await _context.Flim.FindAsync(id);
        //    _context.Flim.Remove(flim);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool FlimExists(int id)
        {
            return _context.Flim.Any(e => e.FlmSn == id);
        }
    }
}
