using rTsn.Models;
using rTsn.Services;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace rTsn
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadInformation();
        }

        void LoadInformation()
        {
            FeedListView.ItemsSource = FeedService.Instance.GetAll(true);
            VideoListView.ItemsSource = YouTubeService.Instance.GetAll(true);
        }

        private void FeedListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var post = (Post)e.ClickedItem;
            System.Diagnostics.Debug.WriteLine(post.Title);
        }

        private void VideoListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
