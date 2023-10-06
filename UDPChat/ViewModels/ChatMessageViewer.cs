using SharedData;
using System.Windows;
using System.Windows.Media;

namespace UDPChat.ViewModels
{
    internal class ChatMessageViewer
    {
        public ClientInfo? ClientInfo {  get; set; }
        public string Message { get; set; } = string.Empty;
        public string PrivateName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public HorizontalAlignment MessageAlignment { get; set; }
        public Brush MessageBackground => PrivateName.Equals("Everyone") ? Brushes.LightSkyBlue: Brushes.LightGreen;
    }
}
