using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace Web.ViewModels
{
    /// <summary>
    /// 節目model
    /// </summary>
    /// <summary>
    /// 節目model
    /// </summary>
    public class ChannelModel
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 介紹
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 圖片
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 連結
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 媒體類型
        /// null:普通超連結
        /// "application/rss+xml": Rss feed)
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 平台來源
        /// </summary>
        public string Generator { get; set; }

        public List<NetcastModel> NetcastList { get; set; }
    }

    /// <summary>
    /// 音輯model
    /// </summary>
    public class NetcastModel
    {
        /// <summary>
        /// 主題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 介紹
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 發布日期
        /// </summary>
        public DateTime PublishDate { get; set; }

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
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 集數
        /// </summary>
        public string Episode { get; set; }

        /// <summary>
        /// 季數
        /// </summary>
        public string Season { get; set; }

        /// <summary>
        /// 關鍵字
        /// </summary>
        public string Keywords { get; set; }

        //public string EpisodeType { get; set; }

        /// <summary>
        /// 限制級(no, yes)
        /// </summary>
        public string Explicit { get; set; }

        /// <summary>
        /// 圖片
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 秒數
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }
    }
}
