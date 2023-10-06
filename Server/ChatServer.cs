using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    internal class ChatServer 
    {
        private int clientCount,port;
        private string ip;
        private Dictionary<IPEndPoint,ClientInfo> clients = new ();
        private UdpClient? listener;
        private bool started = false;
        
        public ChatServer(int clientCount, int port,string ip)
        {
            this.clientCount = clientCount;
            this.port = port;
            this.ip = ip;
        }

        public void Start()
        {
            IPEndPoint? clientEndPoint = null;

            try { listener = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            };
            Console.WriteLine($"Chat server {listener.Client.LocalEndPoint} started...");
            started = true;
            while (true)
            {
                try
                {
                    byte[] data = listener.Receive(ref clientEndPoint);
                    ClientMessage clientMessage = JsonSerializer.Deserialize<ClientMessage>(Encoding.UTF8.GetString(data)) ?? new();
                    Console.WriteLine($"Chat server resive message [{clientMessage.Message}] from client [{clientEndPoint}]...");
                    switch (clientMessage.Message)
                    {
                        case ClientToServerMessage.Connect:
                            if (!clients.ContainsKey(clientEndPoint))
                            {
                                string message;
                                ClientInfo? clientInfo = JsonSerializer.Deserialize<ClientInfo>(clientMessage.Content);
                                if (clients.Count >= clientCount)
                                    message = "User limit exceeded ! ! !";
                                else if (clients.Any(x => x.Value.Name == clientInfo?.Name))
                                    message = "This name is already in use ! ! !";
                                else
                                {
                                    sendMessage(clientEndPoint, ServerToClientMessage.Connected, string.Empty, false);
                                    foreach (var client in clients)
                                        sendMessage(client.Key, ServerToClientMessage.NewMemberConnected, clientInfo);
                                    clients.Add(clientEndPoint, clientInfo);
                                    break;
                                }
                                sendMessage(clientEndPoint, ServerToClientMessage.NotConnected, message,false);
                                Console.WriteLine($"Client [{clientEndPoint}] not connected...{message}");
                            }
                            break;

                        case ClientToServerMessage.Disconnect:

                            if (!clientConnectedCheck(clientEndPoint)) break;
                            sendMessage(clientEndPoint, ServerToClientMessage.Disconnected, "Connection was terminated by the user", false);
                            string disconnectedClientName = clients[clientEndPoint].Name;
                            clients.Remove(clientEndPoint);
                            foreach (var client in clients)
                                sendMessage(client.Key, ServerToClientMessage.MemberDisconnected, disconnectedClientName,false);

                            break;

                        case ClientToServerMessage.GetConnectedMembers:

                            if (!clientConnectedCheck(clientEndPoint)) break;
                            if (clients.Count - 1 == 0)
                            {
                                sendMessage(clientEndPoint, ServerToClientMessage.NoMembers, string.Empty, false);
                                Console.WriteLine($"Client [{clientEndPoint}] try get members...no members");
                            }
                            else
                            {
                                ClientInfo[] clientsInfo = clients.Values.Where(x => x.Name != clients[clientEndPoint].Name).ToArray();
                                sendMessage(clientEndPoint, ServerToClientMessage.Members, clientsInfo);
                                Console.WriteLine($"Client [{clientEndPoint}] try get members...{clientsInfo.Length} members info sended");
                            }

                            break;

                        case ClientToServerMessage.Message:
                            if (!clientConnectedCheck(clientEndPoint)) break;
                            ChatMessage? chatMessage = JsonSerializer.Deserialize<ChatMessage>(clientMessage.Content);
                            if (chatMessage.PrivateName != "Everyone")
                            {
                                IPEndPoint? pClient = clients.FirstOrDefault(x => x.Value.Name == chatMessage.PrivateName).Key;
                                sendMessage(pClient, ServerToClientMessage.Message, clientMessage.Content,false);
                                Console.WriteLine($"Client [{clientEndPoint}] send private message...");
                            }
                            else
                            {
                                Console.WriteLine($"Client [{clientEndPoint}] send message...");
                                foreach (var client in clients.Keys)
                                {
                                    if (!client.Equals(clientEndPoint))
                                        sendMessage(client, ServerToClientMessage.Message, clientMessage.Content,false);
                                }
                            }
                     
                            break;
                    }
                    clientEndPoint = null;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }

        public void Stop() 
        {
            if (!started) return;
            foreach (var client in clients)
                sendMessage(client.Key, ServerToClientMessage.Disconnected, "Server stoped", false);
            clients.Clear();
            listener?.Close();
            Console.WriteLine("Chat server stoped...");
        }

        private void sendMessage(IPEndPoint endPoint,ServerToClientMessage message, object? content ,bool serialize = true)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ServerMessage()
            {
                Message = message,
                Content = serialize ? JsonSerializer.Serialize(content): content as string
                }));
            listener?.SendAsync(data, endPoint);
            Console.WriteLine($"Chat server send message [{message}] to client [{endPoint}]...");
        }

        private bool clientConnectedCheck(IPEndPoint clientEndPoint)
        {
            if (clients.ContainsKey(clientEndPoint)) return true;
            else
            {
                sendMessage(clientEndPoint, ServerToClientMessage.NotConnected, "Not connected",false);
                Console.WriteLine($"Client [{clientEndPoint}] not connected...");
                return false;
            }
        }
    }
}
