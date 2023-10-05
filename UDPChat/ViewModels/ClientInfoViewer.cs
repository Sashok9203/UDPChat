using SharedData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UDPChat.ViewModels
{
    internal class ClientInfoViewer : INotifyPropertyChanged
    {
        private Visibility  blocked =  Visibility.Hidden;

        public Visibility Blocked
        {
            get => blocked;
            set 
            {
                blocked = value;
                OnPropertyChanged();
            }
        }

        public ClientInfo? ClientInfo { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
