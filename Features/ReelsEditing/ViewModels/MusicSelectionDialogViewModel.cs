// <copyright file="MusicSelectionDialogViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Ubb_se_2026_meio_ai.Core.Models;
    using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

    /// <summary>
    /// ViewModel for the music selection dialog.
    /// </summary>
    public partial class MusicSelectionDialogViewModel : ObservableObject
    {
        private readonly IAudioLibraryService audioLibrary;

        [ObservableProperty]
        private ObservableCollection<MusicTrackModel> availableTracks = new ();

        [ObservableProperty]
        private MusicTrackModel? selectedTrack;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicSelectionDialogViewModel"/> class.
        /// </summary>
        /// <param name="audioLibrary">The audio library service used to fetch tracks.</param>
        public MusicSelectionDialogViewModel(IAudioLibraryService audioLibrary)
        {
            this.audioLibrary = audioLibrary;
        }

        /// <summary>
        /// Loads the available music tracks from the audio library asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [RelayCommand]
        private async Task LoadTracksAsync()
        {
            var tracks = await this.audioLibrary.GetAllTracksAsync();
            this.AvailableTracks.Clear();

            foreach (var musicTrack in tracks)
            {
                this.AvailableTracks.Add(musicTrack);
            }
        }

        /// <summary>
        /// Selects a specific music track.
        /// </summary>
        /// <param name="track">The music track to select.</param>
        [RelayCommand]
        private void SelectTrack(MusicTrackModel track)
        {
            this.SelectedTrack = track;
        }
    }
}