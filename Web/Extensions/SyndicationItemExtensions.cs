using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Web.Extensions
{
    public static class SyndicationItemExtensions
    {
        //public static string GetCreator(this SyndicationItem item)
        //{
        //    var value = item.GetElementExtensionValueByOuterName("creator");
        //    return value;
        //}

        public static string GetAuthor(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("author");
            return value;
        }

        public static string GetEpisode(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("episode");
            return value;
        }

        public static string GetSeason(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("season");
            return value;
        }

        public static string GetKeywords(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("keywords");
            return value;
        }

        //public static string GetEpisodeType(this SyndicationItem item)
        //{
        //    var value = item.GetElementExtensionValueByOuterName("episodeType");
        //    return value;
        //}

        public static string GetExplicit(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("explicit");
            return value;
        }

        public static string GetImageUrl(this SyndicationItem item)
        {
            var value = item.GetElementAttributeValueByOuterName("image", "href");
            return value;
        }

        public static int GetDuration(this SyndicationItem item)
        {
            var value = item.GetElementExtensionValueByOuterName("duration");
            int.TryParse(value, out int result); ;
            return result;
        }

        private static string GetElementExtensionValueByOuterName(this SyndicationItem item, string outerName)
        {
            if (item.ElementExtensions.All(x => x.OuterName != outerName)) return null;
            return item.ElementExtensions.Single(x => x.OuterName == outerName).GetObject<XElement>().Value;
        }

        private static string GetElementAttributeValueByOuterName(this SyndicationItem item, string outerName, string attrName)
        {
            if (item.ElementExtensions.All(x => x.OuterName != outerName)) return null;
            return item.ElementExtensions.Single(x => x.OuterName == outerName).GetObject<XElement>().Attribute(attrName).Value;
        }
    }
}
