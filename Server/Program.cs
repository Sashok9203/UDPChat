using System;

namespace Server
{
    internal class Program
    {
        private static ChatServer? chat;
        static void Main(string[] args)
        {
            int port = 0,
                clientCount = 0;
            if (args.Length < 3 || !int.TryParse(args[1], out port) || !int.TryParse(args[2], out clientCount)) exit("Invalid command parameters...");
            string ip = args[0];
            string mutexName = $"{ip}:{port}";
            if (!Mutex.TryOpenExisting(mutexName, out Mutex? mutex))
                mutex = new Mutex(false, mutexName);
            else exit("Another instance of this program was opened with current network address!");
            
            mutex?.WaitOne();
               AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
               chat = new(clientCount, port,ip);
               chat?.Start();
            mutex?.ReleaseMutex();
        }
        static void ProcessExit(object sender, EventArgs e) => chat?.Stop();

        static void exit(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}