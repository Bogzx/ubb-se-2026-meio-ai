using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels;
using Windows.Media.Core;

namespace ubb_se_2026_meio_ai.Features.ReelsEditing.Views
{
    public sealed partial class ReelsEditingPage : Page
    {
        public ReelsEditingViewModel ViewModel { get; }
        public ReelGalleryViewModel GalleryViewModel { get; }
        public MusicSelectionDialogViewModel MusicDialogViewModel { get; }

        public ReelsEditingPage()
        {
            ViewModel = App.Services.GetRequiredService<ReelsEditingViewModel>();
            GalleryViewModel = App.Services.GetRequiredService<ReelGalleryViewModel>();
            MusicDialogViewModel = App.Services.GetRequiredService<MusicSelectionDialogViewModel>();
            this.InitializeComponent();

            // Listen for IsEditing changes to toggle panels
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.IsEditing))
                    UpdatePanelVisibility();
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GalleryViewModel.EnsureLoadedAsync();
            UpdatePanelVisibility();
        }

        private void UpdatePanelVisibility()
        {
            if (ViewModel.IsEditing)
            {
                GalleryPanel.Visibility = Visibility.Collapsed;
                EditorPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StopVideo();
                GalleryPanel.Visibility = Visibility.Visible;
                EditorPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ReelGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection tracked by TwoWay binding
        }

        private void ReelGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ubb_se_2026_meio_ai.Core.Models.ReelModel reel)
            {
                GalleryViewModel.SelectedReel = reel;
                ViewModel.LoadReel(reel);
                LoadVideo(reel.VideoUrl);
            }
        }

        private void LoadVideo(string videoUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(videoUrl) && Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri))
                {
                    ReelPlayer.Source = MediaSource.CreateFromUri(uri);
                }
            }
            catch
            {
                // Video URL may be invalid; player will show empty
            }
        }

        private void StopVideo()
        {
            try
            {
                ReelPlayer.Source = null;
            }
            catch { }
        }

        private async void ChooseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            await MusicDialogViewModel.LoadTracksCommand.ExecuteAsync(null);

            var dialog = new ContentDialog
            {
                Title = "Choose Background Music",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Confirm",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
            };

            var listView = new ListView
            {
                ItemsSource = MusicDialogViewModel.AvailableTracks,
                Height = 300,
            };
            listView.ItemTemplate = (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                "<TextBlock Text=\"{Binding TrackName}\" Padding=\"8,4\"/>" +
                "</DataTemplate>");

            listView.SelectionChanged += (s, args) =>
            {
                if (listView.SelectedItem is ubb_se_2026_meio_ai.Core.Models.MusicTrackModel track)
                    MusicDialogViewModel.SelectedTrack = track;
            };

            dialog.Content = listView;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && MusicDialogViewModel.SelectedTrack != null)
                ViewModel.ApplyMusicSelection(MusicDialogViewModel.SelectedTrack);
        }
    }
}
