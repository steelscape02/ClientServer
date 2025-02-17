namespace ClientServer;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public abstract class ClientSocket
{
    private const string EndMsg = "END_EXEC";
    public static void StartClient(string ip, int port)
    {
        try
        {
            var client = new TcpClient(ip, port);
            Console.WriteLine("Connected to server!");
            var stream = client.GetStream();

            var receiveThread = new Thread(() => ReceiveMessages(stream, "Server"));
            receiveThread.Start();

            SendMessages(stream);
            receiveThread.Join();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
    private static void ReceiveMessages(NetworkStream stream, string sender)
    {
        var buffer = new byte[1024];
        while (true)
        {
            try
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message == EndMsg)
                {
                    Console.WriteLine($"\nReceived '{EndMsg}' request from {sender}. Press enter to end: ");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine($"\n{sender}: " + message);
            }
            catch
            {
                Console.WriteLine($"{sender} ended the chat.");
                break;
            }
        }
    }

    private static void SendMessages(NetworkStream stream)
    {
        while (true)
        {
            Console.Write("You ('exit' to end): ");
            var message = Console.ReadLine();
            if (message == null) continue;
            if (message.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                var exit = Encoding.UTF8.GetBytes(EndMsg);
                stream.Write(exit, 0, exit.Length);
                break;
            }
            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}
