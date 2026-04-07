using System.Diagnostics;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Downloads YouTube videos as MP4 files using yt-dlp.
    /// Stores them in a local folder and returns the file path.
    /// Calls yt-dlp directly via Process to avoid YoutubeDLSharp native crashes.
    /// Owner: Andrei
    /// </summary>
    public class VideoDownloadService : IVideoDownloadService
    {
        private const int ProcessTimeoutMinutes = 5;
        private const int DefaultMaxDurationSeconds = 60;
        private const int SuccessExitCode = 0;

        private const string YtDlpExecutableName = "yt-dlp.exe";
        private const string FfmpegExecutableName = "ffmpeg.exe";
        private const string AppDataFolderName = "MeioAI";
        private const string VideosFolderName = "Videos";
        private const string MicrosoftFolderName = "Microsoft";
        private const string WinGetFolderName = "WinGet";
        private const string LinksFolderName = "Links";
        private const string Mp4SearchPattern = "*.mp4";
        private const string Mp4FileFormat = "{0}.mp4";

        private const string OutputTemplateFormat = "%(id)s.%(ext)s";
        private const string YtDlpBaseArgumentsFormat = "--no-playlist --ffmpeg-location \"{0}\" -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" --merge-output-format mp4 -o \"{1}\" ";
        private const string YtDlpDurationArgumentFormat = "--postprocessor-args \"ffmpeg:-t {0}\" ";

        private const string ErrorProcessStartFailed = "Failed to start yt-dlp process";
        private const string ErrorProcessTimeout = "yt-dlp process timed out after 5 minutes";
        private const string ErrorExitCodeFormat = "yt-dlp exit code {0}: {1}";
        private const string ErrorNoMp4FoundFormat = "yt-dlp succeeded but no MP4 file found. standardOutput: {0}";
        private const string ErrorProcessExceptionFormat = "yt-dlp process error: {0}";

        private const string MergerLogPrefix = "[Merger]";
        private const string DownloadLogPrefix = "[download]";
        private const string DestinationLogPrefix = "Destination:";
        private const string AlreadyDownloadedLogSuffix = "has already been downloaded";
        private const string AlreadyDownloadedReplacement = " has already been downloaded";
        private const char LineBreakCharacter = '\n';
        private const char QuoteCharacter = '"';

        private readonly string downloadFolder;
        private string ytDlpPath = YtDlpExecutableName;
        private string ffmpegPath = FfmpegExecutableName;
        private bool isInitialized;

        public VideoDownloadService(string? downloadFolder = null)
        {
            this.downloadFolder = downloadFolder
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppDataFolderName, VideosFolderName);

            Directory.CreateDirectory(this.downloadFolder);
        }

        /// <summary>
        /// Ensures yt-dlp and ffmpeg binaries are available.
        /// </summary>
        public Task EnsureDependenciesAsync()
        {
            if (isInitialized)
            {
                return Task.CompletedTask;
            }

            // 1. Check WinGet links folder first (winget install yt-dlp / ffmpeg)
            string wingetLinks = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                MicrosoftFolderName, WinGetFolderName, LinksFolderName);
            string wingetYtDlp = Path.Combine(wingetLinks, YtDlpExecutableName);
            string wingetFfmpeg = Path.Combine(wingetLinks, FfmpegExecutableName);

            if (File.Exists(wingetYtDlp) && File.Exists(wingetFfmpeg))
            {
                ytDlpPath = wingetYtDlp;
                ffmpegPath = wingetFfmpeg;
                isInitialized = true;
                return Task.CompletedTask;
            }

            // 2. Check if binaries exist in the download folder
            string localYtDlp = Path.Combine(downloadFolder, YtDlpExecutableName);
            string localFfmpeg = Path.Combine(downloadFolder, FfmpegExecutableName);

            if (File.Exists(localYtDlp) && File.Exists(localFfmpeg))
            {
                ytDlpPath = localYtDlp;
                ffmpegPath = localFfmpeg;
                isInitialized = true;
                return Task.CompletedTask;
            }

            // 3. Fall back to PATH (set during constructor default)
            isInitialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Downloads a YouTube video as MP4 and returns the local file path.
        /// Returns null if the download fails.
        /// </summary>
        public async Task<string?> DownloadVideoAsMp4Async(string youtubeUrl, int maxDurationSeconds = DefaultMaxDurationSeconds)
        {
            await EnsureDependenciesAsync();

            string outputTemplate = Path.Combine(downloadFolder, OutputTemplateFormat);
            string processArguments = string.Format(YtDlpBaseArgumentsFormat, ffmpegPath, outputTemplate);

            if (maxDurationSeconds > 0)
            {
                processArguments += string.Format(YtDlpDurationArgumentFormat, maxDurationSeconds);
            }

            processArguments += $"\"{youtubeUrl}\"";

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = processArguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using var downloadProcess = Process.Start(processStartInfo);
                if (downloadProcess == null)
                {
                    LastError = ErrorProcessStartFailed;
                    return null;
                }

                var standardOutputTask = downloadProcess.StandardOutput.ReadToEndAsync();
                var standardErrorTask = downloadProcess.StandardError.ReadToEndAsync();

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(ProcessTimeoutMinutes));
                try
                {
                    await downloadProcess.WaitForExitAsync(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        downloadProcess.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                    }
                    LastError = ErrorProcessTimeout;
                    return null;
                }

                string standardOutput = await standardOutputTask;
                string standardError = await standardErrorTask;

                if (downloadProcess.ExitCode != SuccessExitCode)
                {
                    LastError = string.Format(ErrorExitCodeFormat, downloadProcess.ExitCode, standardError);
                    return null;
                }

                string? filePath = FindDownloadedFile(standardOutput);
                if (filePath != null && File.Exists(filePath))
                {
                    LastError = null;
                    return filePath;
                }

                LastError = string.Format(ErrorNoMp4FoundFormat, standardOutput);
                return null;
            }
            catch (Exception exception)
            {
                LastError = string.Format(ErrorProcessExceptionFormat, exception.Message);
                return null;
            }
        }

        /// <summary>
        /// Parses yt-dlp stdout to find the downloaded file path.
        /// Falls back to the newest .mp4 in the download folder.
        /// </summary>
        private string? FindDownloadedFile(string standardOutput)
        {
            foreach (string line in standardOutput.Split(LineBreakCharacter))
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith(MergerLogPrefix) && trimmedLine.Contains(QuoteCharacter))
                {
                    int firstQuoteIndex = trimmedLine.IndexOf(QuoteCharacter) + 1;
                    int lastQuoteIndex = trimmedLine.LastIndexOf(QuoteCharacter);
                    if (lastQuoteIndex > firstQuoteIndex)
                    {
                        string path = trimmedLine[firstQuoteIndex..lastQuoteIndex];
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }

                if (trimmedLine.StartsWith(DownloadLogPrefix))
                {
                    string remainingLine = trimmedLine[DownloadLogPrefix.Length..].Trim();

                    if (remainingLine.StartsWith(DestinationLogPrefix))
                    {
                        string path = remainingLine[DestinationLogPrefix.Length..].Trim();
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }

                    if (remainingLine.EndsWith(AlreadyDownloadedLogSuffix))
                    {
                        string path = remainingLine.Replace(AlreadyDownloadedReplacement, string.Empty).Trim();
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            try
            {
                var newestFile = new DirectoryInfo(downloadFolder)
                    .GetFiles(Mp4SearchPattern)
                    .OrderByDescending(file => file.LastWriteTimeUtc)
                    .FirstOrDefault();
                return newestFile?.FullName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Contains the error message from the last failed download, if any.
        /// </summary>
        public string? LastError { get; private set; }

        /// <summary>
        /// Gets the path where a video for a given YouTube video ID would be stored.
        /// </summary>
        public string GetExpectedFilePath(string videoId)
        {
            return Path.Combine(downloadFolder, string.Format(Mp4FileFormat, videoId));
        }
    }
}