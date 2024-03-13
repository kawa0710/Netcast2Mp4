using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    [ModelMetadataType(typeof(FlimViewModelMetadata))]
    public class FlimViewModel : Models.Flim
    {
        /// <summary>
        /// 狀態
        /// </summary>
        [Display(Name = "狀態")]
        public string FlmStatusName
        {
            get
            {
                var s = "";
                switch (FlmStatus)
                {
                    case 0:
                        s = "未轉檔";
                        break;
                    case 1:
                        s = "轉檔中";
                        break;
                    case 2:
                        s = "轉檔失敗";
                        break;
                    case 3:
                        s = "未下載";
                        break;
                    case 4:
                        s = "已下載";
                        break;
                }

                return s;
            }
        }
    }

    public class FlimViewModelMetadata
    {
        /// <summary>
        /// 下載檔名
        /// </summary>
        [Display(Name = "下載檔名")]
        public string FlmFileName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Display(Name = "修改時間")]
        public string FlmMat { get; set; }

        /// <summary>
        /// SHA256檢查碼（16進位）
        /// </summary>
        [Display(Name = "SHA256檢查碼")]
        public string FlmSha256 { get; set; }
    }
}
