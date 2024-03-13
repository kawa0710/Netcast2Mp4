using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.Helpers;
using Web.ViewModels;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 取得目前Controller名稱
        /// </summary>
        public string Controller { get => RouteData.Values["controller"].ToString(); }

        /// <summary>
        /// 取得目前Action名稱
        /// </summary>
        public string Action { get => RouteData.Values["action"].ToString(); }

        /// <summary>
        /// 取得使用者IP
        /// </summary>
        public string UserIP { get => WebHelper.GetRemoteIP; }

        /// <summary>
        /// UsrSn
        /// </summary>
        public int LoginSn
        {
            get
            {
                /*
                 * 在LoginController將
                 *                   UsrSn claim為UserId
                 *                   UsrEmail claim為Account
                 */
                int.TryParse(
                    User?.Claims?.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase))?.Value,
                    out int _sn);
                return _sn;
            }
        }

        /// <summary>
        /// 登入Email
        /// </summary>
        public string LoginEmail
        {
            get
            {
                /*
                 * 在LoginController將
                 *                   UsrSn claim為UserId
                 *                   UsrEmail claim為Account
                 */
                return User?.Claims?.FirstOrDefault(x => x.Type.Equals("Account", StringComparison.OrdinalIgnoreCase))?.Value;
            }
        }
    }
}
