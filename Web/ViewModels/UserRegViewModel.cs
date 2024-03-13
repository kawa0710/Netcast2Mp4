using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    [ModelMetadataType(typeof(UserRegViewModelMetadata))]
    public class UserRegViewModel : Models.User
    {
        /// <summary>
        /// 驗證碼
        /// </summary>
        [Required(ErrorMessage = "請填入驗證碼！")]
        [Display(Name = "驗證碼")]
        public string VCode { get; set; }

        /// <summary>
        /// 是否註冊成功
        /// </summary>
        public bool IsReg { get; set; } = false;

        /// <summary>
        /// 註冊後訊息
        /// </summary>
        public string Msg { get; set; }
    }

    public class UserRegViewModelMetadata
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
