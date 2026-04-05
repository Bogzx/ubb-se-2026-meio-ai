namespace Ubb_se_2026_meio_ai.Core.Services
{
    /// <summary>
    /// Provides a UI-agnostic way for ViewModels to show dialogs.
    /// </summary>
    public interface IDialogService
    {
        Task ShowMessageAsync(string title, string message);
        Task ShowErrorAsync(string title, string errorMessage);
    }
}
