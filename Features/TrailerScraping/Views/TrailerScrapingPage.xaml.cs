using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ubb_se_2026_meio_ai.Features.TrailerScraping.ViewModels;

namespace ubb_se_2026_meio_ai.Features.TrailerScraping.Views
{
    public sealed partial class TrailerScrapingPage : Page
    {
        public TrailerScrapingViewModel ViewModel { get; }

        public TrailerScrapingPage()
        {
            ViewModel = App.Services.GetRequiredService<TrailerScrapingViewModel>();
            this.InitializeComponent();

            // Populate MaxResults ComboBox
            MaxResultsCombo.ItemsSource = ViewModel.MaxResultsOptions;
            MaxResultsCombo.SelectedItem = ViewModel.MaxResults;
            MaxResultsCombo.SelectionChanged += (s, e) =>
            {
                if (MaxResultsCombo.SelectedItem is int val)
                {
                    ViewModel.MaxResults = val;
                }
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string preset)
            {
                ViewModel.ApplyPresetCommand.Execute(preset);
            }
        }
    }
}
