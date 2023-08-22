using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für DraftControl.xaml
    /// </summary>
    public partial class DraftControl : UserControl
    {
        public DraftPreview Draft { get; set; }

        public DraftControl(DraftPreview draft)
        {
            Draft = draft;
            InitializeComponent();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
