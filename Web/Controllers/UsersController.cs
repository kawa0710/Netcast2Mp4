using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Models;
using Web.ViewModels;

namespace Web.Controllers
{
    public class UsersController : BaseController
    {
        private readonly ILogger<LoginController> _logger;
        private readonly Netcast2Mp4Context _context;

        public UsersController(ILogger<LoginController> logger,
            Netcast2Mp4Context context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Users/ChangePwd/5
        public async Task<IActionResult> ChangePwd(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userPwd = await _context.UserPassword.FindAsync(id);
            if (userPwd == null)
            {
                return NotFound();
            }

            var user_vm = new UserChangePwdViewModel();
            user_vm.UsrSn = userPwd.UsrSn;

            return View(user_vm);
        }

        // POST: Users/ChangePwd/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePwd(int id, [Bind("UsrSn,UpwPwd")] UserChangePwdViewModel userChPwdVm)
        {
            if (id != userChPwdVm.UsrSn)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var userPwd = await _context.UserPassword.FindAsync(id);
                    if (userPwd == null)
                    {
                        return NotFound();
                    }

                    var user = await _context.User.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    userPwd.UpwPwd = userChPwdVm.UpwPwd;
                    userPwd.UpwCat = DateTime.Now;
                    _context.Update(userPwd);

                    userChPwdVm.IsLogin = true;
                    if (!user.UsrIsActive)
                    {
                        userChPwdVm.IsLogin = false;
                        user.UsrIsActive = true;
                        user.UsrMat = DateTime.Now;
                        _context.Update(user);
                    }

                    await _context.SaveChangesAsync();

                    userChPwdVm.UpwPwd = null;
                    userChPwdVm.ConfirmPassword = null;
                    userChPwdVm.IsSuccess = true;
                    if (userChPwdVm.IsLogin)
                        userChPwdVm.Msg = "以後請用新密碼登入！";
                    else
                        userChPwdVm.Msg = "您的帳號已啟用，以後請用新密碼登入！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    userChPwdVm.Msg = "請聯絡客服處理。";
                    if (!UserExists(userChPwdVm.UsrSn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));
            }
            else
                userChPwdVm.Msg = "密碼格式不正確或與確認密碼不一致！";

            return View(userChPwdVm);
        }

        //// GET: Users
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.User.ToListAsync());
        //}

        //// GET: Users/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.User
        //        .FirstOrDefaultAsync(m => m.UsrSn == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        //// GET: Users/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Users/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("UsrSn,UsrEmail,UsrGuid,UsrCat,UsrCuser,UsrMat,UsrMuser,UsrIsActive")] User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(user);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(user);
        //}

        //// GET: Users/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.User.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(user);
        //}

        //// POST: Users/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("UsrSn,UsrEmail,UsrGuid,UsrCat,UsrCuser,UsrMat,UsrMuser,UsrIsActive")] User user)
        //{
        //    if (id != user.UsrSn)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(user);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UserExists(user.UsrSn))
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
        //    return View(user);
        //}

        //// GET: Users/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.User
        //        .FirstOrDefaultAsync(m => m.UsrSn == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        //// POST: Users/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var user = await _context.User.FindAsync(id);
        //    _context.User.Remove(user);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UsrSn == id);
        }
    }
}
