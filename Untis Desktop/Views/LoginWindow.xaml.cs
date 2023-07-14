﻿using System;
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
using System.Windows.Shapes;
using UntisDesktop.ViewModels;

namespace UntisDesktop.Views;
/// <summary>
/// Interaktionslogik für LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        PasswordBox pwdBox = (PasswordBox)sender;
        LoginWindowViewModel viewModel = (LoginWindowViewModel)DataContext;
        viewModel.Password = pwdBox.Password;
        
        e.Handled = true;
    }

    private void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            PasswordBox pwdBox = (PasswordBox)sender;
            LoginWindowViewModel viewModel = (LoginWindowViewModel)DataContext;
            pwdBox.Password = viewModel.Password;
        }
    }
}