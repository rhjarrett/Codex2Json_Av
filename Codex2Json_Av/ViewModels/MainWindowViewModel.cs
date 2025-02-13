using Avalonia.Controls;
using Codex2Json_Av.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Codex2Json_Av.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private string _directoryPath = "";
        private string _resultMessage = "";
        private readonly CodexGenerator _codexGenerator;

        public string DirectoryPath
        {
            get => _directoryPath;
            set
            {
                _directoryPath = value;
                OnPropertyChanged(nameof(DirectoryPath));
            }
        }

        public string ResultMessage
        {
            get => _resultMessage;
            set
            {
                _resultMessage = value;
                OnPropertyChanged(nameof(ResultMessage));
            }
        }

        public ICommand BrowseCommand { get; private set; }
        public ICommand ProcessCommand { get; private set; }

        public MainWindowViewModel()
        {
            _codexGenerator = new CodexGenerator();
            BrowseCommand = new RelayCommand(async () => await OpenFolderDialog());
            ProcessCommand = new RelayCommand(ProcessDirectory);
        }

        private async Task OpenFolderDialog()
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(new Window());

            if ( !string.IsNullOrEmpty(result) )
            {
                DirectoryPath = result;
            }
        }

        private async Task ProcessDirectory()
        {
            if ( string.IsNullOrWhiteSpace(DirectoryPath) || !Directory.Exists(DirectoryPath) )
            {
                ResultMessage = "Invalid directory. Please select a valid folder.";
                return;
            }

            try
            {
                string outputPath = await _codexGenerator.Process(DirectoryPath, new Window());
                ResultMessage = $"JSON generated at: {outputPath}";
            }
            catch ( Exception ex )
            {
                ResultMessage = $"Error: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task>? _executeAsync;
        private readonly Action? _execute;

        public RelayCommand(Func<Task> executeAsync)
        {
            _executeAsync = executeAsync;
        }

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public async void Execute(object? parameter)
        {
            if ( _executeAsync != null ) await _executeAsync();
            _execute?.Invoke();
        }
    }
}
