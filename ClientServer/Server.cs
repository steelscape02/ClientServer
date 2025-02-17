namespace ClientServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public abstract class ServerSocket
{
    private const string EndMsg = "END_EXEC";
    private const string FileMsg = "START_FILE";
    
    public static void StartServer(int port)
    {
        var localIp = GetLocalIpAddress();
        Console.WriteLine($"Starting server... Your IP: {localIp}, Port: {port}");

        var server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine("Server started. Waiting for client...");

        var client = server.AcceptTcpClient();
        Console.WriteLine("Client connected!");

        var stream = client.GetStream();
        var receiveThread = new Thread(() => ReceiveMessages(stream, "Client"));
        receiveThread.Start();

        SendMessages(stream);
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

    private static void SendFile(string filePath, NetworkStream stream)
    {
        //TODO: Add file transfer and receive methods (with header msg's and in-terminal prompts - with approval?)
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        // Get file size
        var fileSize = fileStream.Length;
        var fileSizeBytes = BitConverter.GetBytes(fileSize);
        
        //send filesize banner first
        var exit = Encoding.UTF8.GetBytes(FileMsg);
        stream.Write(exit, 0, exit.Length);
        
        // Send file size
        stream.Write(fileSizeBytes, 0, fileSizeBytes.Length);

        // Send file data
        var buffer = new byte[4096]; // 4KB buffer
        int bytesRead;
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, bytesRead);
        }

        Console.WriteLine("File sent successfully!");
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