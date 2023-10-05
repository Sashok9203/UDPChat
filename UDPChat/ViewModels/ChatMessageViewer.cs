using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UDPChat.ViewModels
{
    internal class ChatMessageViewer
    {
        public ClientInfo? ClientInfo {  get; set; }
        public string Message { get; set; } = string.Empty;
        public string PrivateName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public HorizontalAlignment MessageAlignment { get; set; }
    }
}
