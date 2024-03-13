using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Helpers
{
    /// <summary>
    /// 取得瀏覽器端資訊class
    /// </summary>
    public static class WebHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 取得Client IP
        /// </summary>
        public static string GetRemoteIP
        {
            get { return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(); }
        }

        /// <summary>
        /// 取得瀏覽器資訊
        /// </summary>
        public static string GetUserAgent
        {
            get { return _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(); }
        }

        /// <summary>
        /// 取得通訊協定
        /// </summary>
        public static string GetScheme
        {
            get { return _httpContextAccessor.HttpContext.Request.Scheme; }
        }
    }
}