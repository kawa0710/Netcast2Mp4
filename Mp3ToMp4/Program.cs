using Newtonsoft.Json;
using RayITUtilityNet472;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Mp3ToMp4
{
    class Program
    {
        //apng會被當成普通png。目前.net套件無法判斷apng
        static void Main(string[] args)
        {
            #region 測試Code
            //var str = Common.AESDecrypt(ConfigurationManager.ConnectionStrings["Netcast2Mp4Entities"].ToString(), Common.AES_KEY, Common.AES_IV);
            //Console.WriteLine(str);
            //Console.ReadKey();

            //    var img_path = @"C:\Users\Kawa\Downloads\1093.png";

            //    var music_path = @"C:\Users\Kawa\Downloads\Tones And I - Dance Monkey myfreemp3.vip .mp3";
            //    var ffmpeg_path = @"ffmpeg\ffmpeg.exe";
            //    var app_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            //                                , ffmpeg_path);
            //    var output_path = @"D:\test.mp4";
            //    var param = $" -y -ignore_loop 0 -i \"{img_path}\" -i \"{music_path}\" -shortest \"{output_path}\"";
            //    /*
            //     -y: 覆蓋檔案
            //     -ignore_loop 0: apng或gif重覆複放
            //     -shortest: 合併input檔案時以長度最短為基準
            //     */
            //    RunMyProcess(app_path, param);
            //    //Console.ReadKey(); 
            #endregion

            //利用IsRunning.txt供後續判斷本程式的執行狀態
            var path_IsRunning = "IsRunning.txt";
            bool flag_IsRunning = false;
            if (File.Exists(path_IsRunning))
            {
                string IsRunning = File.ReadAllText(path_IsRunning);
                if (IsRunning.Trim() != "0")
                    flag_IsRunning = true;
            }
            else
                File.WriteAllLines("IsRunning.txt", new string[] { "0" });

            string UserDataDrive = ConfigurationManager.AppSettings["UserDataDrive"];
            if (!string.IsNullOrWhiteSpace(UserDataDrive) && !flag_IsRunning)
            { //檢查 Config有無設定資料暫存的磁碟機&&本程式是不是【執行中】
                File.WriteAllLines("IsRunning.txt", new string[] { "1" });

                Netcast2Mp4Entities db = null;
                try
                {
                    db = new Netcast2Mp4Entities(Conn);

                    FlimViewModel f = GetWaitingFlim(db);
                    while (f != null && f.flim.FLM_SN > 0)
                    {
                        f.flim.FLM_Status = (byte)FlmStatusEnum.轉檔中;
                        SetFlimValue(f.flim);

                        var netcastModel = JsonConvert.DeserializeObject<NetcastModel>(f.ntc_info);
                        var uri = netcastModel.Uri;

                        var mp3_path = $@"{UserDataDrive}:\UserData\{f.flim.FLM_SN}.mp3";
                        if (!File.Exists(mp3_path))
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(uri, mp3_path);
                            }
                        }

                        var img_path = $@"{UserDataDrive}:\UserData\{f.img_guid16}{f.img_ext}";
                        var flim_path = $@"{UserDataDrive}:\UserData\{f.flim.FLM_SN}.mp4";

                        var ffmpeg_path = @"ffmpeg\ffmpeg.exe";
                        var app_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ffmpeg_path);
                        var param = "";
                        var loop_param = $" -y -ignore_loop 0 -i \"{img_path}\" -i \"{mp3_path}\" -shortest \"{flim_path}\"";
                        var noloop_param = $" -y -i \"{img_path}\" -i \"{mp3_path}\" \"{flim_path}\"";

                        if (f.img_ext == ".apng")
                            param = loop_param;
                        else
                        {
                            if (f.img_ext != ".gif")
                                param = noloop_param;
                            else
                            {
                                var checkIsAnimate = CheckIsAnimate(img_path);
                                if (checkIsAnimate != null)
                                {
                                    if (checkIsAnimate == true)
                                        param = loop_param;
                                    else
                                        param = noloop_param;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(param))
                        {
                            //Console.WriteLine("Start...");
                            RunMyProcess(app_path, param);
                            //Console.WriteLine("Completed.");
                            f.flim.FLM_Status = (byte)FlmStatusEnum.未下載;
                            f.flim.FLM_Sha256 = Common.GetCheckSum_SHA256(flim_path);
                            SetFlimValue(f.flim);
                        }
                        else
                        {
                            f.flim.FLM_Status = (byte)FlmStatusEnum.轉檔失敗;
                            SetFlimValue(f.flim);
                        }

                        f = GetWaitingFlim(db);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (db != null)
                    {
                        db.Dispose();
                        db = null;
                    }
                }
            }

            File.WriteAllLines("IsRunning.txt", new string[] { "0" });
        }

        static void RunMyProcess(string FFmpegPath, string Parameters)
        {
            var p = new Process();
            p.StartInfo.FileName = FFmpegPath;
            p.StartInfo.Arguments = Parameters;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            //Console.WriteLine("Start...");
            p.WaitForExit();
            //Console.WriteLine("Completed.");
            p.Close();
        }

        static string Conn
        {
            get
            {
                var conn = Common.AESDecrypt(ConfigurationManager.ConnectionStrings["Netcast2Mp4Entities"].ToString(), Common.AES_KEY, Common.AES_IV);
                return conn;
            }
        }

        static FlimViewModel GetWaitingFlim(Netcast2Mp4Entities db)
        {            
            FlimViewModel f = null;
            if (db == null)
                return f;

            var q = from a in db.Flims
                    join b in db.Netcasts on a.NTC_SN equals b.NTC_SN
                    join c in db.Images on b.NTC_SN equals c.NTC_SN
                    where a.FLM_Status == 0
                    select new FlimViewModel
                    {
                        flim = a,
                        ntc_info = b.NTC_Info,
                        img_guid16 = c.IMG_Guid16,
                        img_ext = c.IMG_Ext
                    };
            f = q.FirstOrDefault();

            return f;
        }

        enum FlmStatusEnum
        {
            未轉檔,
            轉檔中,
            轉檔失敗,
            未下載,
            已下載
        }

        static void SetFlimValue(Flim flim)
        {
            using (var db = new Netcast2Mp4Entities(Conn))
            {
                flim.FLM_MAt = DateTime.Now;
                var old_flim = db.Flims.Find(flim.FLM_SN);
                db.Entry(old_flim).CurrentValues.SetValues(flim);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 檢查圖檔是不是動態
        /// (無法檢查apng)
        /// </summary>
        /// <param name="ImgPath"></param>
        /// <returns>null：無法檢查檔案或檔案不存在/true/false</returns>
        static bool? CheckIsAnimate(string ImgPath)
        {
            if (File.Exists(ImgPath))
            {
                try
                {
                    var i = System.Drawing.Image.FromFile(ImgPath);
                    var frameDim = new System.Drawing.Imaging.FrameDimension(i.FrameDimensionsList[0]);
                    int frames = i.GetFrameCount(frameDim);
                    if (frames > 1)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }
    }
}
