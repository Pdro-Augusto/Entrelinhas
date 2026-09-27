using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Entrelinhas.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _isLoading;
        public bool IsLoading
        {
            // Propriedade que indica se uma operação está em andamento
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Notifica a mudança de propriedade para a interface de usuário
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            // Se o valor não mudou, não faz nada
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            // Atualiza o valor e notifica a mudança
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string? _titulo;
        public string? Titulo
        {
            get => _titulo;
            set => SetProperty(ref _titulo, value);
        }
    }
}