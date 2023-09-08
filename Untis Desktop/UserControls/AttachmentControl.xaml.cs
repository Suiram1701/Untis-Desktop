using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls;

public partial class AttachmentControl : UserControl
{
    public string FileName { get; set; }

    public bool DownloadAble { get; }

    public bool DeleteAble { get; }

    public MemoryStream? Stream;

    public Attachment? Attachment;

    public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent("OnDeletion", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AttachmentControl));
    public event RoutedEventHandler DeleteEventHandler
    {
        add => AddHandler(DeleteEvent, value);
        remove => RemoveHandler(DeleteEvent, value);
    }

    private AttachmentControl(string fileName, bool downloadAble, bool deleteAble)
    {
        FileName = fileName;
        DownloadAble = downloadAble;
        DeleteAble = deleteAble;
        InitializeComponent();
    }

    public AttachmentControl(string fileName, Stream content) : this(fileName, false, true)
    {
        Stream = new(0);
        content.CopyTo(Stream);
    }

    public AttachmentControl(Attachment attachment) : this(attachment.Name, true, false)
    {
        Attachment = attachment;
    }

    public AttachmentControl(Attachment attachment, bool deleteAble) : this(attachment.Name, true, deleteAble)
    {
        Attachment = attachment;
    }

    private async void Download_ClickAsync(object sender, RoutedEventArgs e)
    {
        // Get the description for the file to download
        string extension = Path.GetExtension(FileName);
        string description = LangHelper.GetString("AttachmentControl.DFE", extension[1..]);

        using RegistryKey? key = Registry.ClassesRoot.OpenSubKey(extension);
        if (key is not null)
        {
            using RegistryKey? subKey = Registry.ClassesRoot.OpenSubKey((key.GetValue(null) as string) ?? string.Empty);
            if (subKey is not null)
                description = (subKey.GetValue(null) as string) ?? description;
        }

        SaveFileDialog dialog = new()
        {
            Title = LangHelper.GetString("AttachmentControl.DDT"),
            CheckPathExists = true,
            FileName = FileName,
            Filter = $"{description} (*{extension})|*{extension}"
        };
        if (dialog.ShowDialog() ?? false)
        {
            try
            {
                using FileStream stream = new(dialog.FileName, FileMode.OpenOrCreate, FileAccess.Write);

                // Progress bar
                DownloadState.Text = "0%";
                DownloadImg.Visibility = Visibility.Hidden;
                IProgress<double> progress = new Progress<double>(value => DownloadState.Text = Math.Round(value, 0) + "%");

                await Attachment?.DownloadContentAsStreamAsync(App.Client, stream, TimeSpan.FromSeconds(10), progress)!;

                DownloadState.Text = string.Empty;
                DownloadImg.Visibility = Visibility.Visible;
                DownloadImg.LoadImage(new Uri("pack://application:,,,/Untis Desktop;component/Assets/download_done.png"));
            }
            catch (Exception ex)
            {
                MessageWindowViewModel viewModel = (MessageWindowViewModel)Window.GetWindow(this).DataContext;
                ex.HandleWithDefaultHandler(viewModel, "Download attachment");
            }
        }

        e.Handled = true;
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        Stream?.Dispose();
        RaiseEvent(new(DeleteEvent));
    }
}