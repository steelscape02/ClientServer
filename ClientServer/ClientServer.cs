namespace ClientServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public abstract class ClientServer
{
    private const string EndMsg = "END_EXEC";
    
    public static void StartClient(string ip, int port)
    {
        try
        {
            var client = new TcpClient(ip, port);
            Console.WriteLine("Connected to server!");
            var stream = client.GetStream();
            string? name;
            Console.WriteLine("Enter client name: ");
            do
            {
                name = Console.ReadLine();
            }while(string.IsNullOrEmpty(name));
            var receiveThread = new Thread(() => ReceiveMessages(stream, name));
            receiveThread.Start();

            while (client.Connected)
            {
                SendMessages(stream);
            }
            receiveThread.Join();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
    public static void StartServer(int port)
    {
        var name = "Server";
        var localIp = GetLocalIpAddress();
        Console.WriteLine($"Starting {name.ToLower()}... Your IP: {localIp}, Port: {port}");

        var server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine("Server started. Waiting for client...");

        var client = server.AcceptTcpClient();
        Console.WriteLine("Client connected!");

        var stream = client.GetStream();
        var receiveThread = new Thread(() => ReceiveMessages(stream, "Server"));
        receiveThread.Start();

        while (client.Connected)
        {
            SendMessages(stream);
        }
        receiveThread.Join();
        Console.WriteLine("Client disconnected.");
    }

    private static void ReceiveMessages(NetworkStream stream, string sender)
    {
        var buffer = new byte[1024];
        while (true)
        {
            try
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Connection closed
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message.Contains(EndMsg))
                {
                    Console.WriteLine($"{sender} ended the session.");
                    return;
                }

                Console.WriteLine($"\n{sender}: {message}");
            }
            catch
            {
                Console.WriteLine($"{sender} disconnected.");
                break;
            }
        }
    }
    
    private static void SendMessages(NetworkStream stream)
    {
        while (true)
        {
            Console.Write("You (special: exit, send): ");
            var message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;

            if (message.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                var exit = Encoding.UTF8.GetBytes(EndMsg);
                stream.Write(exit, 0, exit.Length);
                stream.Close();
                break;
            }

            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }


    private static string GetLocalIpAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1"; //default IP
    }
}