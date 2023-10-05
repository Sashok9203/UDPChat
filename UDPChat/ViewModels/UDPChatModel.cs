using FilesStatistic.Model;
using Microsoft.Win32;
using SharedData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace UDPChat.ViewModels
{
    internal class UDPChatModel : INotifyPropertyChanged
    {
        private const string serverIp = "127.0.0.1", everyone  = "Everyone";

        private const int port = 8080;

        private readonly UdpClient client;

        private IPEndPoint? server = null;

        private ServerMessage? serverMessage ;

        private byte[] avatarImage = Resource.no_avatar;

        private byte[] connectImage = Resource.connect;

        private bool isConnected = false;

        private bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged();
                ConnectImage = isConnected? Resource.disconnect: Resource.connect;
            }
        }

        private string contextMenuHeader = "Block",
                       selectedName = everyone,
                       message = string.Empty;

        private async Task sendMessageAsync(ClientToServerMessage message, object? content)
        {
            ClientMessage clientMessage = new()
            {
                Message = message,
                Content = content != null ? JsonSerializer.Serialize(content) : string.Empty
            };

            await client.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clientMessage)));
        }

        private async Task<ServerMessage?> resiveMessageAsync()  =>
            await Task<ServerMessage>.Run(async () => JsonSerializer.Deserialize<ServerMessage>(Encoding.UTF8.GetString((await client.ReceiveAsync()).Buffer)));
        
        private async Task listenServer()
        {
            do 
            {
                serverMessage = await resiveMessageAsync();
                switch (serverMessage.Message)
                {
                    case ServerToClientMessage.NewMemberConnected:
                        ClientInfo? clientInfo = JsonSerializer.Deserialize<ClientInfo>(serverMessage.Content);
                        ConnectedClients.Add(new() { ClientInfo = clientInfo });
                        ClientsNames.Add(clientInfo.Name);
                        SelectedName = everyone;
                        break;

                    case ServerToClientMessage.MemberDisconnected:
                        string? name = serverMessage.Content;
                        ConnectedClients.Remove(ConnectedClients.First(x=>x.ClientInfo?.Name == name));
                        ClientsNames.Remove(name);
                        SelectedName = everyone;
                        break;

                    case ServerToClientMessage.Disconnected:
                        string? message = serverMessage.Content;
                        IsConnected = false;
                        await Task.Delay(1000);
                        MessageBox.Show(message, serverMessage.Message.ToString());
                        break;

                    case ServerToClientMessage.Message:
                        ChatMessage? chatMessage = JsonSerializer.Deserialize<ChatMessage>(serverMessage.Content);
                        ClientInfo info = ConnectedClients.FirstOrDefault(x => x.ClientInfo.Name == chatMessage.Name).ClientInfo;
                        ChatMessages.Add(new()
                        {
                            Message = chatMessage.Message,
                            Time = DateTime.Now.ToShortTimeString(),
                            ClientInfo = info,

                            MessageAlignment = HorizontalAlignment.Left
                        }) ;
                        break;

                    case ServerToClientMessage.Connected:
                        IsConnected = true;
                        await sendMessageAsync(ClientToServerMessage.GetConnectedMembers, null);
                        break;

                    case ServerToClientMessage.Members:
                        ConnectedClients.Clear();
                        ClientsNames.Clear();
                        ClientsNames.Add(everyone);
                        SelectedName = everyone;
                        ClientInfo[]? connectedClients = JsonSerializer.Deserialize<ClientInfo[]>(serverMessage.Content);
                        foreach (var item in connectedClients)
                        {
                            ConnectedClients.Add(new() { ClientInfo = item });
                            ClientsNames.Add(item.Name);
                        }
                        break;

                    case ServerToClientMessage.NotConnected:
                        MessageBox.Show(serverMessage?.Content);
                        break;
                }

            }
            while (IsConnected);
        }

        private async Task connect()
        {
            ClientToServerMessage message = IsConnected ? ClientToServerMessage.Disconnect: ClientToServerMessage.Connect;

            ClientInfo? cli = null;

            if (message == ClientToServerMessage.Connect)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    MessageBox.Show("Enter youre name ! ! !");
                    return;
                }
                cli = new() 
                {
                    Name = Name,
                    AvatarImage = AvatarImage
                };
            }
           
            await sendMessageAsync(message,cli);

            await listenServer();
        }

        private void block(object o)
        {
            if (o is ClientInfoViewer viewer)
            {
                if (viewer.Blocked == Visibility.Visible)
                {
                    ContextMenuHeader = "Block";
                    viewer.Blocked = Visibility.Hidden;
                }
                else 
                {
                    ContextMenuHeader = "Unblock";
                    viewer.Blocked = Visibility.Visible;
                }
            }
        }

        private async void loadAvatar()
        {
            OpenFileDialog ofd = new ();
            if (ofd.ShowDialog() == true)
            {
                using MemoryStream ms = new( File.ReadAllBytes(ofd.FileName));
                Bitmap bitmap = new(new Bitmap(ms), 64, 64) ;
                ImageConverter imageConverter = new();

                AvatarImage = imageConverter.ConvertTo(bitmap, typeof(byte[])) as byte[];

            }
            else AvatarImage = Resource.no_avatar;
        }

        private async void sendChatMessageAsync()
        {
            ChatMessage chatMessage = new ()
            {
                Message = Message,
                PrivateName = SelectedName,
                Name = Name,
            };
            await sendMessageAsync(ClientToServerMessage.Message, chatMessage);
            ChatMessages.Add(new() 
            {
                Message = Message,
                Time = DateTime.Now.ToShortTimeString(),
                ClientInfo = new() { Name = SelectedName == everyone ? "You" : $"You for {SelectedName}" },
                
                MessageAlignment = HorizontalAlignment.Right
            });
            Message = string.Empty;
        }

        public UDPChatModel()
        {
            client = new ();
            client.Connect(new IPEndPoint(IPAddress.Parse(serverIp),port));
        }

        public string Name { get; set; } = string.Empty;

        public string Message
        {
            get => message;
            set 
            {
                message = value;
                OnPropertyChanged();
            }
        }


        public string SelectedName
        {
            get => selectedName;
            set 
            {
                selectedName = value;
                OnPropertyChanged();
            }
        }

        public string ContextMenuHeader
        {
            get => contextMenuHeader;
            set 
            {
                contextMenuHeader = value;
                OnPropertyChanged();
            }
        }

        public byte[] AvatarImage
        {
            get => avatarImage;
            set 
            {
                avatarImage = value;
                OnPropertyChanged();
            }
        }

        public byte[] ConnectImage
        {
            get => connectImage;
            set
            {
                connectImage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ChatMessageViewer> ChatMessages { get; set; } = new() ;

        public ObservableCollection<string> ClientsNames { get; set; } = new() { everyone };

        public ObservableCollection<ClientInfoViewer> ConnectedClients { get; set; } = new();

        public RelayCommand Connect => new(async (o)=> await connect());

        public RelayCommand Send => new((o) => sendChatMessageAsync(), (o) => !string.IsNullOrEmpty(Message) && IsConnected);

        public RelayCommand Block => new((o) => block(o));

        public RelayCommand GetAvatar => new((o) => loadAvatar(),(o) => !IsConnected);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

   
}
