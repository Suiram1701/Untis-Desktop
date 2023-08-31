using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UntisDesktop.Localization;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls;

public partial class FilterBox : UserControl
{
    public string Type { get; set; }

    public FilterItem[] Items { get; set; }

    public static readonly DependencyProperty ViewCancelProperty = DependencyProperty.Register("ViewCancel", typeof(bool), typeof(FilterBox), new(false));
    public bool ViewCancel
    {
        get => (bool)GetValue(ViewCancelProperty);
        set => SetValue(ViewCancelProperty, value);
    }

    public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register("IsExpand", typeof(bool), typeof(FilterBox), new(false));
    public bool IsExpand
    {
        get => (bool)GetValue(IsExpandProperty);
        set => SetValue(IsExpandProperty, value);
    }

    public static readonly DependencyProperty DisplayedTextProperty = DependencyProperty.Register("DisplayedText", typeof(string), typeof(FilterBox), new(string.Empty));
    public string DisplayedText
    {
        get => (string)GetValue(DisplayedTextProperty);
        set => SetValue(DisplayedTextProperty, value);
    }

    public static RoutedEvent SelectionChanged = EventManager.RegisterRoutedEvent("OnSelectionChanged", RoutingStrategy.Bubble, typeof(EventHandler<UpdateEventArgs>), typeof(FilterBox));
    public event EventHandler<UpdateEventArgs> SelectionChangedEvent
    {
        add => AddHandler(SelectionChanged, value);
        remove => RemoveHandler(SelectionChanged, value);
    }

    public FilterBox(string type, FilterItem[] items)
    {
        Type = type;
        Items = items;

        InitializeComponent();

        DisplayedText = Type;
        RenderItems();
    }

    private void RenderItems()
    {
        foreach (FilterItem item in Items)
        {
            ToggleButton toggleButton = new()
            {
                Name = "_" + item.ReferenceId,
                Content = item.Name,
                ToolTip = LangHelper.GetString("FilterBox.S"),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Template = (ControlTemplate)Application.Current.FindResource("CheckBtn")
            };
            toggleButton.Click += ToggleItem_Click;

            ItemContainer.Children.Add(toggleButton);
        }
    }

    private void UpdateDisplayText()
    {
        int count = ItemContainer.Children
            .OfType<ToggleButton>()
            .Where(i => i.IsChecked ?? false)
            .Count();

        if (count == 0)
            DisplayedText = Type;
        else
        {
            string displayItemName = ItemContainer.Children
                .OfType<ToggleButton>()
                .Where(t => t.IsChecked ?? false)
                .First().Name;
            string displayName = Items.First(i => i.ReferenceId == int.Parse(displayItemName[1..])).Name;

            if (count == 1)
                DisplayedText = displayName;
            else
                DisplayedText = LangHelper.GetString("FilterBox.FN", displayName, (count - 1).ToString());
        }

        // Update cancel view
        if (count >= 1)
            ViewCancel = true;
        else
            ViewCancel = false;
    }

    private void ToggleItem_Click(object sender, RoutedEventArgs e)
    {
        string name = ((FrameworkElement)sender).Name;
        int id = int.Parse(name[1..]);

        UpdateDisplayText();

        RaiseEvent(new UpdateEventArgs(id, SelectionChanged));

        e.Handled = true;
    }

    private void BorderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        Storyboard animation = (Storyboard)ExpansionImg.FindResource("RotateAnimation");

        if (IsExpand)
        {
            animation.AutoReverse = true;
            animation.Begin();
            animation.Pause();
            animation.Seek(TimeSpan.FromSeconds(0.2));
            animation.Resume();
        }
        else
        {
            animation.AutoReverse = false;
            animation.Begin();
        }

        IsExpand = !IsExpand;
        e.Handled = true;
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        // Deselect all selected items
        foreach (ToggleButton btn in ItemContainer.Children
            .OfType<ToggleButton>()
            .Where(t => t.IsChecked ?? false))
        {
            int id = int.Parse(btn.Name[1..]);

            btn.IsChecked = false;
            RaiseEvent(new UpdateEventArgs(id, SelectionChanged));
        }

        UpdateDisplayText();
    }
}
