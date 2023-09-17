using Licenses;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UntisDesktop.Extensions;

internal static class LicenceInformationExtensions
{
    public static void AddToPanel(this LicenceInformation information, Panel panel)
    {
        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock
                {
                        Text = information.LibraryName,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold
                }
            }
        };
        if (information.Version is not null)
        {
            stackPanel.Children.Add(new TextBlock
            {
                Text = 'v' + information.Version.ToString(),
                FontSize = 16,
                Margin = new(30, 0, 0, 0)
            });
        }

        panel.Children.Add(stackPanel);
        panel.Children.Add(new TextBlock
        {
            Text = information.Content,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        });
    }
}
