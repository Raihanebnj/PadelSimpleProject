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
using System.Windows.Shapes;
using System.Linq;
using PadelSimple.Desktop.ViewModels;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Views;

public partial class MainWindow : Window
{
    public MainViewModel VM { get; }
    public MainWindow(MainViewModel vm, AuthService auth)
    {
        InitializeComponent();
        VM = vm; DataContext = VM;


        // Adjust UI by role
        if (auth.IsInRole("Admin")) UsersTab.Visibility = Visibility.Visible;
    }

    private void OnExit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void OnAbout(object sender, RoutedEventArgs e)
    => MessageBox.Show("PadelSimple\nWPF + EF Core + SQLite", "Over");

    private void OnNewCourt(object sender, RoutedEventArgs e) => VM.SelectedCourt = new();
    private void OnNewEquipment(object sender, RoutedEventArgs e) => VM.SelectedEquipment = new Equipment();
}