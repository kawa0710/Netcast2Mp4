using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class UserChangePwdViewModel
    {
        /// <summary>
        /// User序號
        /// </summary>
        [Required]
        public int UsrSn { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Required(ErrorMessage = "請填入密碼！")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0}長度最少限制{2}字元！", MinimumLength = 12)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$]).{12,}$", ErrorMessage = "密碼限長度12字元以上，且必須包含下列四種組合：英文大寫、英文小寫、數字及符號（!@#$任一）。")]
        [Display(Name = "密碼")]
        public string UpwPwd { get; set; }

        /// <summary>
        /// 確認密碼
        /// </summary>
        [Required(ErrorMessage = "請填入確認密碼！")]
        [DataType(DataType.Password)]
        [Compare("UpwPwd", ErrorMessage = "密碼與確認密碼不符！")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword  { get; set; }

        /// <summary>
        /// 是否變更成功
        /// </summary>
        public bool IsSuccess { get; set; } = false;

        /// <summary>
        /// 是否已登入
        /// </summary>
        public bool IsLogin { get; set; } = false;

        /// <summary>
        /// 註冊後訊息
        /// </summary>
        public string Msg { get; set; }
    }
}
