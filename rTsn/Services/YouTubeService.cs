using rTsn.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Xml.Linq;

namespace rTsn.Services
{
    /// <summary>
    /// A simple Services that reads and parses YouTube feed 
    /// information from the web.
    /// 
    /// Note:
    ///     - For a more documented service implementation see <see cref="FeedService"/>.
    ///     - Use singelton `Instance` member to access the functionality.
    /// </summary>
    public sealed class YouTubeService
    {
        #region Public member

        /// <summary>
        /// Shared instance.
        /// </summary>
        public static YouTubeService Instance { get; } = new YouTubeService();

        #endregion

        #region Private constants 

        /// <summary>
        /// Feed endpoint.
        /// </summary>
        private const string FEED_ENDPOINT = "https://www.youtube.com/feeds/videos.xml?user=DrWindowsTV";

        #endregion

        #region Private member

        /// <summary>
        /// List of already requested posts.
        /// </summary>
        private List<Video> cachedVideos = new List<Video>();

        #endregion

        #region Constructor

        /// <summary>
        /// Hide statics inits from being called.
        /// </summary>
        static YouTubeService()
        {
        }

        private YouTubeService()
        {
        }

        #endregion

        #region Getter

        public List<Video> GetAll(bool forceReload = false)
        {
            if (forceReload == false && cachedVideos.Count > 0) return cachedVideos;

            WebClient client = new WebClient { CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore) };
            XNamespace xmlns = "http://www.w3.org/2005/Atom";
            XNamespace yt = "http://www.youtube.com/xml/schemas/2015";
            XNamespace media = "http://search.yahoo.com/mrss/";

            var videos = XDocument
                            .Parse(client.DownloadString(FEED_ENDPOINT))
                            .Root
                            .Elements(xmlns + "entry")
                            .Select(item => new Video
                            {
                                Id = item.Element(yt + "videoId").Value,
                                Title = item.Element(xmlns + "title").Value,
                                Description = item.Element(media + "group").Element(media + "description").Value,
                                VideoSource = item.Element(xmlns + "link").Attribute("href").Value.Replace("watch", "watch_popup"),
                                ThumbnailSource = item.Element(media + "group").Element(media + "thumbnail").Attribute("url").Value.Replace("hqdefault", "maxresdefault"),

                            })
                            .ToList();

            cachedVideos = videos;
            return videos;
        }

        #endregion
    }
}
