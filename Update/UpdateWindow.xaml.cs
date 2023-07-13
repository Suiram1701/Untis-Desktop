using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Update.Localization;

namespace Update;
/// <summary>
/// Interaktionslogik für Update.xaml
/// </summary>
public partial class UpdateWindow : Window
{
    private string UpdateState
    {
        set => State.Content = value;
    }

    private readonly CancellationTokenSource _cts = new();
    private readonly Uri _url;

    private bool _isDownloading = true;
    private bool _successful = false;

    public UpdateWindow(Uri downloadUrl)
    {
        _url = downloadUrl;
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_successful)
            return;

        e.Cancel = true;
        CancelBtn_Click(this, (RoutedEventArgs)EventArgs.Empty);
    }

    private async void Window_InitializedAsync(object sender, EventArgs e)
    {
        IProgress<double> progress = new Progress<double>(progress => Progress.Value = progress);

        // Download
        Logger.LogInformation("Start downloading update");
        UpdateState = LangHelper.GetString("UpdateWin.State.D");

        using HttpClient client = new();
        using HttpResponseMessage response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);

        if (_cts.IsCancellationRequested)
        {
            Logger.LogInformation("Update cancelled!");
            Application.Current.Shutdown(0);
            Close();
            return;
        }

        response.EnsureSuccessStatusCode();

        long totalBytes = response.Content.Headers.ContentLength ?? -1;
        long receivedBytes = 0;

        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        byte[] buffer = new byte[8192];
        int bytesRead = 0;

        using MemoryStream zipStream = new();
        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)     // Read the buffer with progress bar updates
        {
            receivedBytes += bytesRead;
            zipStream.Write(buffer, 0, bytesRead);
            progress.Report((double)receivedBytes / totalBytes * 100);
        }

        Logger.LogInformation($"Update download was successful");
        _isDownloading = false;

        progress.Report(0);
        Logger.LogInformation("Starting moving files");
        UpdateState = LangHelper.GetString("UpdateWin.State.F");

        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Read))
        {
            double readFiles = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                await Task.Delay(10000);
                if (Regex.IsMatch(entry.Name, @"Update[^\\]*\.(?:dll|exe)"))
                    continue;


                if (File.Exists(entry.FullName))     // Replace a exist file
                {
                    File.Delete(entry.FullName);
                    entry.ExtractToFile(entry.FullName);
                    Logger.LogInformation($"File '{entry.FullName}' was replaced");
                }
                else     // Create a new file
                {
                    int index = entry.FullName.LastIndexOf('/');
                    if (index != -1)     // Ensure that parts of the path exists
                    {
                        string pathPart = entry.FullName[..index];
                        if (!Directory.Exists(pathPart))
                            Directory.CreateDirectory(pathPart);
                    }

                    entry.ExtractToFile(entry.FullName);
                    Logger.LogInformation($"File '{entry.FullName}' was created");
                }

                readFiles++;
                progress.Report(readFiles / archive.Entries.Count * 100d);
            }
        }
        Logger.LogInformation("Moving files was successful");

        Logger.LogInformation("Update was installed successful");
        _successful = true;
        Close();

        MessageBox.Show(LangHelper.GetString("UpdateWin.Inf.Success"), LangHelper.GetString("UpdateWin.Inf.SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);

        Application.Current.Shutdown(0);
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!_isDownloading)
        {
            MessageBox.Show(this, LangHelper.GetString("UpdateWin.Warn.DCancel"), LangHelper.GetString("UpdateWin.Warn.DCancel.Title"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        _cts.Cancel();
    }
}
