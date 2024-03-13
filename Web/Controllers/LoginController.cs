using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;
using RayITUtilityCore31;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Web.Controllers
{
    public class LoginController : BaseController
    {
        private readonly ILogger<LoginController> _logger;
        private IConfiguration _config;
        private readonly Netcast2Mp4Context _context;

        public LoginController(ILogger<LoginController> logger,
            IConfiguration config,
            Netcast2Mp4Context context)
        {
            _logger = logger;
            _config = config;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("UsrEmail, VCode, Password")] UserLoginViewModel user)
        {
            if (ModelState.IsValid && user.VCode == HttpContext.Session.GetString("VCode"))
            {
                try
                {
                    var oldUser = await _context.User
                        .FirstOrDefaultAsync(m => m.UsrEmail == user.UsrEmail);
                    if (oldUser == null)
                    {
                        user.Msg = "找不到此帳號！請註冊帳號。";
                        _logger.LogWarning($"登入失敗!無帳號:{user.UsrEmail}@{UserIP}");
                    }
                    else
                    {
                        var oldPwd = await _context.UserPassword
                            .FirstOrDefaultAsync(m => m.UsrSn == oldUser.UsrSn);
                        if (oldPwd == null)
                        {
                            user.Msg = "請聯絡客服處理。";
                            _logger.LogError($"登入失敗!找不到密碼:{oldUser.UsrSn},{oldUser.UsrEmail}");
                        }
                        else
                        {
                            if (oldPwd.UpwPwd == Common.GetSaltedHashCode(user.Password, oldUser.UsrGuid.ToString()))
                            {
                                if (oldUser.UsrIsActive)
                                { //帳號已啟用

                                    user.IsLogin = true;
                                    //建立 Claim，也就是要寫到 Cookie 的內容
                                    var claims = new[]
                                    {
                                        new Claim("UserId", oldUser.UsrSn.ToString()),
                                        new Claim("Account", oldUser.UsrEmail)
                                    };
                                    //建立證件，類似你的駕照或護照
                                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                    //將 ClaimsIdentity 設定給 ClaimsPrincipal (持有者) 
                                    ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

                                    //登入動作
                                    await HttpContext.SignInAsync(principal, new AuthenticationProperties()
                                    {
                                        //是否可以被刷新
                                        AllowRefresh = true,
                                        // 設置了一個 有效期的持久化 cookie，期限與Startup.cs相同
                                        IsPersistent = true,
                                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
                                    });

                                    _logger.LogInformation($"登入:{user.UsrEmail}@{UserIP}");
                                    return RedirectToAction("Index", "Flims");
                                }
                                else
                                { //帳號未啟用，強制改密碼後才能啟用
                                    return RedirectToAction("ChangePwd", "Users", new { id = oldUser.UsrSn });
                                }
                            }
                            else
                            {
                                user.Msg = "帳號或密碼錯誤！請重新登入。";
                                _logger.LogWarning($"登入失敗!帳密錯誤:{user.UsrEmail}@{UserIP}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"{Controller}/{Action}", user);
                }
                //return RedirectToAction(nameof(Index));
            }

            user.VCode = null;
            user.Password = null;
            ModelState.Clear();
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <returns></returns>
        public IActionResult Reg()
        {
            return View();
        }

        /// <summary>
        /// 取得/更新驗證碼
        /// </summary>
        /// <returns>image/jpeg</returns>
        public IActionResult GetValidateCode()
        {
            var vcode = Common.GetValidateCode();
            HttpContext.Session.SetString("VCode", vcode.Code);
            return File(vcode.Bytes, @"image/jpeg");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reg([Bind("UsrEmail, VCode")] UserRegViewModel user)
        {
            if (ModelState.IsValid && user.VCode == HttpContext.Session.GetString("VCode"))
            {
                try
                {
                    if (!UserExists(user.UsrEmail))
                    {
                        user.UsrGuid = Guid.NewGuid();
                        user.UsrCat = DateTime.Now;
                        user.UsrCuser = "sys";
                        user.UsrMat = DateTime.Now;
                        user.UsrMuser = "sys";
                        user.UsrIsActive = false;
                        _context.Add(user);
                        await _context.SaveChangesAsync();

                        if (user.UsrSn > 0)
                        { //新增成功
                            var pwd = Common.GetRandomPassword();

                            var userPwd = new UserPassword()
                            {
                                UsrSn = user.UsrSn,
                                UpwPwd = Common.GetSaltedHashCode(pwd, user.UsrGuid.ToString()),
                                UpwCat = DateTime.Now
                            };
                            _context.Add(userPwd);

                            #region 註解--直接註冊成功&顯示密碼(下版改【密碼寄email不要直接顯示並驗證email】)

                            //user.UsrIsActive = true;
                            //_context.Update(user);

                            //await _context.SaveChangesAsync();

                            //user.Msg = $"您的登入密碼為：{pwd}";
                            //user.IsReg = true;

                            #endregion

                            #region 驗證Email及寄密碼

                            var email_template = _config.GetValue<string>("RegEmail");
                            var sendMail = new SendMail()
                            {
                                SmlSubject = "[NoReply]啟用您在Raycast的帳號",
                                SmlBody = email_template.Replace("{email}", user.UsrEmail).Replace("{pwd}", pwd),
                                SmlTo = user.UsrEmail,
                                SmlCat = DateTime.Now,
                                SmlSendAt = new DateTime(1900, 1, 1)
                            };
                            _context.Add(sendMail);
                            await _context.SaveChangesAsync();

                            user.Msg = $"請至您的Email：{user.UsrEmail}收取信件，完成啟用手續並收取登入密碼。";
                            user.IsReg = true;

                            #endregion
                        }
                        else
                        {
                            user.Msg = "不能註冊帳號！請聯絡客服處理。";
                            _logger.LogError($"註冊失敗!已寫入User資料表但取得序號卻小於1:{user.UsrSn},{user.UsrEmail}");
                        }
                    }
                    else
                    {
                        user.Msg = "不能註冊帳號！此帳號已被使用。";
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex, $"{Controller}/{Action}", user);
                }
                //return RedirectToAction(nameof(Index));
            }

            user.VCode = null;
            ModelState.Clear();
            return View(user);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UsrSn == id);
        }

        private bool UserExists(string email)
        {
            return _context.User.Any(m => m.UsrEmail == email);
        }
    }
}
