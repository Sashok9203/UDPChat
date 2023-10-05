using System;

namespace Server
{
    internal class Program
    {
        private static ChatServer? chat;
        private static readonly string ip = "127.0.0.1";
        private static readonly int port = 8080;
        private static readonly int clientCount = 3;
        static void Main(string[] args)
        {
            string mutexName = $"{ip}:{port}";
            if (!Mutex.TryOpenExisting(mutexName, out Mutex? mutex))
                mutex = new Mutex(false, mutexName);
            else
            {
                Console.WriteLine("Another instance of this program was opened with current network address!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            mutex?.WaitOne();
               AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
               chat = new(clientCount, port,ip);
               chat?.Start();
            mutex?.ReleaseMutex();
        }
        static void ProcessExit(object sender, EventArgs e) => chat?.Stop();
    }
}