using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.ViewModels;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Views
{
    public sealed partial class TrailerScrapingPage : Page
    {
        private const int SingleMatchCount = 1;
        private const int FirstItemIndex = 0;
        private const string SelectedMovieFormat = "Selected: {0} ({1}) — {2}";

        public TrailerScrapingViewModel ViewModel { get; }

        public TrailerScrapingPage()
        {
            ViewModel = App.Services.GetRequiredService<TrailerScrapingViewModel>();
            this.InitializeComponent();

            // Populate MaxResults ComboBox
            MaxResultsCombo.ItemsSource = ViewModel.MaxResultsOptions;
            MaxResultsCombo.SelectedItem = ViewModel.MaxResults;

            MaxResultsCombo.SelectionChanged += (comboSender, selectionEventArgs) =>
            {
                if (MaxResultsCombo.SelectedItem is int selectedValue)
                {
                    ViewModel.MaxResults = selectedValue;
                }
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            await ViewModel.InitializeAsync();
        }

        /// <summary>
        /// Fires as the user types — queries the Movie table for matches.
        /// </summary>
        private async void MovieSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs textChangedEventArgs)
        {
            // Only query if the text was typed by the user (not set programmatically)
            if (textChangedEventArgs.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.SearchMoviesCommand.ExecuteAsync(sender.Text);

                // Set the suggestion list to the matching movies
                sender.ItemsSource = ViewModel.SuggestedMovies;

                // Show/hide warning
                NoMatchWarning.Visibility = ViewModel.NoMovieFound
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // Hide selected card when user is typing a new query
                SelectedMovieCard.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Fires when the user picks a suggestion from the dropdown.
        /// </summary>
        private void MovieSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs suggestionChosenEventArgs)
        {
            if (suggestionChosenEventArgs.SelectedItem is MovieCardModel movie)
            {
                sender.Text = movie.Title;
                ViewModel.SelectMovie(movie);

                // Show selected movie card
                SelectedMovieText.Text = string.Format(SelectedMovieFormat, movie.Title, movie.ReleaseYear, movie.Genre);
                SelectedMovieCard.Visibility = Visibility.Visible;
                NoMatchWarning.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Fires when the user presses Enter or clicks the query icon.
        /// If there's exactly one suggestion, select it automatically.
        /// </summary>
        private void MovieSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs querySubmittedEventArgs)
        {
            if (querySubmittedEventArgs.ChosenSuggestion is MovieCardModel movie)
            {
                ViewModel.SelectMovie(movie);

                SelectedMovieText.Text = string.Format(SelectedMovieFormat, movie.Title, movie.ReleaseYear, movie.Genre);
                SelectedMovieCard.Visibility = Visibility.Visible;
                NoMatchWarning.Visibility = Visibility.Collapsed;
            }
            else if (ViewModel.SuggestedMovies.Count == SingleMatchCount)
            {
                // Auto-select the only match
                var onlyMovie = ViewModel.SuggestedMovies[FirstItemIndex];
                sender.Text = onlyMovie.Title;
                ViewModel.SelectMovie(onlyMovie);

                SelectedMovieText.Text = string.Format(SelectedMovieFormat, onlyMovie.Title, onlyMovie.ReleaseYear, onlyMovie.Genre);
                SelectedMovieCard.Visibility = Visibility.Visible;
                NoMatchWarning.Visibility = Visibility.Collapsed;
            }
        }
    }
}