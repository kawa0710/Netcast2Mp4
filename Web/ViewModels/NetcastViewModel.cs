using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;

namespace Web.ViewModels
{
    public class NetcastViewModel : Netcast
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 介紹
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 連結
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 媒體類型
        /// null:普通超連結
        /// "audio/mpeg": 音檔
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 圖片
        /// </summary>
        public string ImgFilename { get; set; }
    }
}
