using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.ReelsFeed.Services;
using Windows.Media.Core;
using System;

namespace ubb_se_2026_meio_ai.Features.ReelsFeed.Views
{
    public sealed partial class ReelItemView : UserControl
    {
        public static readonly DependencyProperty ReelProperty =
            DependencyProperty.Register("Reel", typeof(ReelModel), typeof(ReelItemView), new PropertyMetadata(null, OnReelChanged));

        public ReelModel Reel
        {
            get => (ReelModel)GetValue(ReelProperty);
            set => SetValue(ReelProperty, value);
        }

        private readonly IClipPlaybackService _playbackService;

        public ReelItemView()
        {
            this.InitializeComponent();
            _playbackService = App.Services.GetRequiredService<IClipPlaybackService>();
            
            // Unload the video and rigorously dispose the underlying COM objects when the control unloads to prevent Win32 exit crashes
            this.Unloaded += (s, e) => {
                if (ReelPlayer.MediaPlayer != null)
                {
                    ReelPlayer.MediaPlayer.Pause();
                    var oldSource = ReelPlayer.Source as IDisposable;
                    ReelPlayer.Source = null;
                    oldSource?.Dispose();
                }
            };
        }

        private static void OnReelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReelItemView view && e.NewValue is ReelModel reel)
            {
                if (!string.IsNullOrEmpty(reel.VideoUrl))
                {
                    // Create the raw primitive media source
                    var source = MediaSource.CreateFromUri(new Uri(reel.VideoUrl));
                    
                    // Wrap in a MediaPlaybackItem to strictly limit memory/network buffering to the first 60 seconds.
                    // Since FlipView natively keeps the 1 previous and 1 next container alive in the background,
                    // just assigning this source triggers the OS to seamlessly pre-buffer the adjacent reels automatically!
                    var playbackItem = new Windows.Media.Playback.MediaPlaybackItem(source, TimeSpan.Zero, TimeSpan.FromSeconds(60));

                    view.ReelPlayer.Source = playbackItem;
                }
            }
        }

        public void PlayVideo()
        {
            if (ReelPlayer.MediaPlayer != null)
            {
                ReelPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                ReelPlayer.MediaPlayer.Play();
            }
        }

        public void PauseVideo()
        {
            if (ReelPlayer.MediaPlayer != null)
            {
                ReelPlayer.MediaPlayer.Pause();
            }
        }
    }
}
