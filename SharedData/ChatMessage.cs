using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData
{
    [Serializable]
    public class ChatMessage
    {
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string PrivateName { get; set; } = string.Empty;
    }
}
