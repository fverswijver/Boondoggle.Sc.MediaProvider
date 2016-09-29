using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Xml;

namespace Boondoggle.Sc.MediaProvider.Media
{
    /// <summary>
    /// Custom MediaProvider which uses a separate set of encoding parameters for normal URLs and media urls
    /// </summary>
    public class MediaProvider : Sitecore.Resources.Media.MediaProvider
    {
        private const string _MEDIA_LIBRARY_PATH = "/sitecore/media library/";

        private static readonly object _lock = new object();
        private static string[] _encodeMediaReplacements;
        public static string[] EncodeMediaReplacements
        {
            get
            {
                if (_encodeMediaReplacements == null)
                {
                    lock (_lock)
                    {
                        if (_encodeMediaReplacements == null)
                        {
                            var xmlMediaReplacements = new List<string>();
                            foreach (XmlNode mediaReplacement in Factory.GetConfigNodes("encodeMediaReplacements/replace"))
                            {
                                if (XmlUtil.GetAttribute("mode", mediaReplacement) != "off")
                                {
                                    string findAttribute = XmlUtil.GetAttribute("find", mediaReplacement);
                                    string replaceAttribute = XmlUtil.GetAttribute("replaceWith", mediaReplacement);

                                    xmlMediaReplacements.Add(findAttribute);
                                    xmlMediaReplacements.Add(replaceAttribute);
                                }
                            }
                            _encodeMediaReplacements = xmlMediaReplacements.ToArray();
                        }
                    }
                }
                return _encodeMediaReplacements;
            }
        }

        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(options, "options");

            var flag = options.Thumbnail || HasMediaContent(item);
            if (!flag && item.InnerItem["path"].Length > 0)
            {
                if (!options.LowercaseUrls)
                    return item.InnerItem["path"];
                return item.InnerItem["path"].ToLowerInvariant();
            }
            if (options.UseDefaultIcon && !flag)
            {
                if (!options.LowercaseUrls)
                    return Themes.MapTheme(Settings.DefaultIcon);
                return Themes.MapTheme(Settings.DefaultIcon).ToLowerInvariant();
            }

            Assert.IsTrue(Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");

            var linkPrefix = MediaLinkPrefix;
            if (options.AbsolutePath)
                linkPrefix = options.VirtualFolder + linkPrefix;
            else if (linkPrefix.StartsWith("/", StringComparison.InvariantCulture))
                linkPrefix = StringUtil.Mid(linkPrefix, 1);

            var linkFirstPart = MainUtil.EncodePath(linkPrefix, '/');
            if (options.AlwaysIncludeServerUrl)
                linkFirstPart =
                    FileUtil.MakePath(
                        string.IsNullOrEmpty(options.MediaLinkServerUrl)
                            ? WebUtil.GetServerUrl()
                            : options.MediaLinkServerUrl, linkFirstPart, '/');

            var extension = StringUtil.EnsurePrefix('.',
                StringUtil.GetString(options.RequestExtension, item.Extension, "ashx"));
            var parameters = options.ToString();
            if (parameters.Length > 0)
                extension = extension + "?" + parameters;

            var path = item.InnerItem.Paths.Path;

            var imageUrlPath =
                CustomEncode(!options.UseItemPath || !path.StartsWith(_MEDIA_LIBRARY_PATH, StringComparison.OrdinalIgnoreCase)
                    ? item.ID.ToShortID().ToString()
                    : StringUtil.Mid(path, _MEDIA_LIBRARY_PATH.Length), '/');

            var fullImageUrl = linkFirstPart + imageUrlPath + (options.IncludeExtension ? extension : string.Empty);
            if (!options.LowercaseUrls)
                return fullImageUrl;

            return fullImageUrl.ToLowerInvariant();
        }

        private string CustomEncode(string path, char separator)
        {
            Assert.ArgumentNotNull(path, "path");
            if (EncodeMediaReplacements.Length == 0)
                return path;

            var returnValue = string.Empty;
            var chArray = new[] { separator };
            returnValue = path.Split(chArray).Aggregate(returnValue, (current, name) => current + EncodeName(name) + separator);

            return StringUtil.RemovePostfix(separator, returnValue);
        }

        private string EncodeName(string name)
        {
            var str = name;
            var nameReplacements = EncodeMediaReplacements;
            if (name.Length > 0)
            {
                var index = 0;
                while (index < nameReplacements.Length - 1)
                {
                    str = str.Replace(nameReplacements[index], nameReplacements[index + 1]);
                    index += 2;
                }
            }
            return str;
        }
    }
}