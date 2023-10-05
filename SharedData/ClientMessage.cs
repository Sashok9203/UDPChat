using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData
{
    public enum ClientToServerMessage
    {
        Non,
        GetConnectedMembers,
        Disconnect,
        Connect,
        Message
    }

    [Serializable]
    public class ClientMessage
    {
        public ClientToServerMessage Message { get; set; }
        public string? Content { get; set; } = string.Empty;
    }
}
