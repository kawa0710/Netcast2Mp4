using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    [ModelMetadataType(typeof(UserLoginViewModelMetadata))]
    public class UserLoginViewModel : Models.User
    {
        /// <summary>
        /// 密碼
        /// </summary>
        [Required(ErrorMessage = "請輸入密碼！")]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        [Required(ErrorMessage = "請填入驗證碼！")]
        [Display(Name = "驗證碼")]
        public string VCode { get; set; }

        /// <summary>
        /// 是否登入成功
        /// </summary>
        public bool IsLogin { get; set; } = false;

        /// <summary>
        /// 登入後訊息
        /// </summary>
        public string Msg { get; set; }
    }

    public class UserLoginViewModelMetadata
    {
        /// <summary>
        /// 電子郵件
        /// </summary>
        [Required(ErrorMessage = "請填入電子郵件！")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確！")]
        [StringLength(100, ErrorMessage = "電子郵件限100字元！")]
        [Display(Name = "電子郵件")]
        public string UsrEmail { get; set; }
    }
}
