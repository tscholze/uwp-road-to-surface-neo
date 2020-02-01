using rTsn.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace rTsn.Services
{
    /// <summary>
    /// A simple that reads and parses feed 
    /// information from the web.
    /// 
    /// Note:
    ///     - Use singelton `Instance` member to access the functionality.
    /// </summary>
    public sealed class FeedService
    {
        #region Public member

        /// <summary>
        /// Shared instance.
        /// </summary>
        public static FeedService Instance { get; } = new FeedService();

        #endregion

        #region Private constants 

        /// <summary>
        /// Feed endpoint.
        /// </summary>
        private const string FEED_ENDPOINT = "https://www.drwindows.de/news/feed";

        /// <summary>
        /// Fallback for the image source of a post.
        /// </summary>
        private const string IMAGESOURCE_FALLBACK = "https://www.drwindows.de/news/wp-content/themes/drwindows_theme/img/DrWindows-Windows-News.png";

        #endregion

        #region Private member

        /// <summary>
        /// List of already requested posts.
        /// </summary>
        private List<Post> cachedPosts = new List<Post>();

        #endregion

        #region Constructor

        /// <summary>
        /// Hide statics inits from being called.
        /// </summary>
        static FeedService()
        {
        }

        private FeedService()
        {
        }

        #endregion

        #region Getter

        /// <summary>
        /// Gets list of post items from the webserver.
        /// </summary>
        /// <param name="forceReload">True if no cached data should be used.</param>
        /// <returns>List of feed post items.</returns>
        public List<Post> GetAll(bool forceReload = false)
        {
            // If no reload is forced and cached posts are available,
            // return cached posts.
            // Else load posts from web service
            if (forceReload == false && cachedPosts.Count > 0)
            {
                return cachedPosts;
            }

            // Setup web client.
            WebClient client = new WebClient
            {
                // Ignore chaching.
                // Always load data from the server instead from the cache.
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            };

            // Download rss file.
            var rssString = client.DownloadString(FEED_ENDPOINT);

            // Setup xml document parser.
            XDocument doc = XDocument.Parse(rssString);
            XNamespace content = "http://purl.org/rss/1.0/modules/content/";

            // Example data structure
            //
            //<xml>
            //  <channel>
            //      <item>
            //          <title>Entry #1</title>
            //      </item>
            //      <item>
            //          n times ...
            //      </item>
            //  </channel>

            // Get channels from the document.
            var channel = doc.Root.Element("channel");

            // Get all `item` entries from the channel.
            var items = channel.Elements("item");

            // Convert found item xml entries into post objects.
            // By mapping element tags to object members.
            // E.g. item.guid -> object.id
            var posts = items.Select(item => new Post
            {
                Id = item.Element("guid").Value.Replace("https://www.drwindows.de/news/?p=", string.Empty),
                Title = item.Element("title").Value,
                LinkSource = item.Element("link").Value,
                Content = item.Element(content + "encoded").Value,
                ImageSource = GetImageSourceOutOfContent(item.Element("description").Value)
            }).ToList();

            // Dispose web client.
            client.Dispose();

            // Store locally
            cachedPosts = posts;

            // Return created posts from parsed document.
            return posts;
        }

        #endregion

        #region Private helper 

        /// <summary>
        /// Gets the image source out of a content string.
        /// </summary>
        /// <param name="content">Content string of a post.</param>
        /// <returns>Found or fallback image source.</returns>
        private string GetImageSourceOutOfContent(string content)
        {
            // Try to extract the first `<img src=".." /> out of the content string 
            // to use as image source.
            var source = Regex.Match(content, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

            // If no image found in content, use fallback.
            return source ?? IMAGESOURCE_FALLBACK;
        }

        #endregion
    }
}
