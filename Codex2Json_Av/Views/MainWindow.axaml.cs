using Avalonia.Controls;
using Codex2Json_Av.ViewModels;

namespace Codex2Json_Av.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();  // Ensure ViewModel is set

    }
}