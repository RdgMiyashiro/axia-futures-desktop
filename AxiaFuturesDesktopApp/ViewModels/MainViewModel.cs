using AxiaFuturesDesktopApp.Models;
using AxiaFuturesDesktopApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace AxiaFuturesDesktopApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WebSocketService _webSocketService = new WebSocketService();
        private readonly TtsService _ttsService = new TtsService();
        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
        private bool _isTtsEnabled = true;
        private int _volume = 50;
        private Message _currentMessage;

        public MainViewModel()
        {
            _webSocketService.OnMessageReceived += (message) =>
            {
                Messages.Add(message);
            };
            _webSocketService.ConnectAsync("wss://edge-api.axiafutures.com/ws/?token=U2FsdGVkX1+YcfF5A506hKmuKwlK2a4WErOATfH/Ek9GtuMmtY0FbGqnH892r4B8");
            PlayCommand = new RelayCommand(parameter => Play(parameter), parameter => CanPlay(parameter));
            PauseCommand = new RelayCommand(parameter => Pause(parameter), parameter => CanPause(parameter));
            TestVolumeCommand = new RelayCommand(parameter => TestVolume(parameter), parameter => CanTestVolume(parameter));
        }

        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set { _messages = value; OnPropertyChanged(nameof(Messages)); }
        }

        public bool IsTtsEnabled
        {
            get => _isTtsEnabled;
            set { _isTtsEnabled = value; OnPropertyChanged(nameof(IsTtsEnabled)); }
        }

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
                _ttsService.SetVolume(_volume);
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand TestVolumeCommand { get; }

        private bool CanPlay(object parameter)
        {
            return IsTtsEnabled && parameter is Message message && !string.IsNullOrEmpty(message.TtsText);
        }

        private void Play(object parameter)
        {
            if (parameter is Message message && !string.IsNullOrEmpty(message.TtsText))
            {
                _ttsService.Stop();
                _currentMessage = message;
                _ttsService.Speak(message.TtsText, Volume);
            }
        }

        private bool CanPause(object parameter)
        {
            return IsTtsEnabled && parameter is Message message && message == _currentMessage && _ttsService.IsPlaying();
        }

        private void Pause(object parameter)
        {
            if (parameter is Message message && message == _currentMessage)
            {
                _ttsService.Pause();
            }
        }

        private bool CanTestVolume(object parameter) => IsTtsEnabled;

        private void TestVolume(object parameter)
        {
            _ttsService.Stop();
            _currentMessage = null;
            _ttsService.Speak("Teste de volume", Volume);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}