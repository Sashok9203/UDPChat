namespace SharedData
{
    public enum ServerToClientMessage
    {
        Non,
        Members,
        NoMembers,
        NewMemberConnected,
        MemberDisconnected,
        Disconnected,
        Connected,
        NotConnected,
        Message
      
    }
    [Serializable]
    public class ServerMessage
    {
        public ServerToClientMessage Message { get; set; }
        public string? Content { get; set; }
    }
}