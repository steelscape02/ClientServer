namespace ClientServer;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public abstract class ClientSocket
{
    private const string EndMsg = "END_EXEC";
    private const string FileMsg = "START_FILE";
    private const string ReadyMsg = "READY";

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
                switch (message)
                {
                    case EndMsg:
                        Console.WriteLine($"\nReceived '{EndMsg}' request from {sender}. Press enter to end: ");
                        Console.ReadLine();
                        return;
                    case FileMsg:
                        Console.WriteLine($"\nReceived '{FileMsg}' request from {sender}.");
                        var filename = "";
                        while(string.IsNullOrEmpty(filename))
                        {
                            Console.Write("Enter desired receive file name: ");
                            filename = Console.ReadLine();
                        }
                        var readyMessage = Encoding.UTF8.GetBytes(ReadyMsg);
                        stream.Write(readyMessage, 0, readyMessage.Length);
                        ReceiveFile(filename,stream);
                        break;
                    default:
                        Console.WriteLine($"\n{sender}: " + message);
                        break;
                }

                
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
            Console.Write("You (Special: exit, send): ");
            var message = Console.ReadLine();
            if (message == null) continue;
            if (message.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                var exit = Encoding.UTF8.GetBytes(EndMsg);
                stream.Write(exit, 0, exit.Length);
                break;
            }

            if (message.Equals("send", StringComparison.CurrentCultureIgnoreCase))
            {
                var filename = "";
                while (string.IsNullOrEmpty(filename))
                {
                    Console.Write("Enter filename: ");
                    filename = Console.ReadLine();
                }
                SendFile(filename, stream);
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
        
        //wait for confirmation
        var confirmBuffer = new byte[5]; // "READY" is 5 bytes
        stream.ReadExactly(confirmBuffer, 0, confirmBuffer.Length);
        var confirmation = Encoding.UTF8.GetString(confirmBuffer);
        if (confirmation != ReadyMsg)
        {
            Console.WriteLine("Client did not confirm. Aborting transfer.");
            return;
        }
        // Send file size
        stream.Write(fileSizeBytes, 0, fileSizeBytes.Length);

        // Send file data
        var buffer = new byte[4096]; // 4KB buffer
        long totalBytesSent = 0;
        int bytesRead;
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, bytesRead);
            totalBytesSent += bytesRead;

            // Display progress
            Console.WriteLine($"Progress: {totalBytesSent * 100 / fileSize}%");
        }

        Console.WriteLine("File sent successfully.");
    }
    
    private static void ReceiveFile(string receivePath, NetworkStream stream)
    {
        using (stream)
        {
            // Receive file size (8 bytes for long)
            var fileSizeBytes = new byte[8];
            stream.ReadExactly(fileSizeBytes, 0, fileSizeBytes.Length);
            var fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

            Console.WriteLine($"Receiving file of size: {fileSize} bytes");

            using (var fs = new FileStream(receivePath, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[4096]; // Match server buffer size
                int bytesRead;
                long totalBytesReceived = 0;

                while (totalBytesReceived < fileSize && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    totalBytesReceived += bytesRead;

                    // Display progress
                    Console.WriteLine($"Progress: {totalBytesReceived * 100 / fileSize}%");
                }
            }
        }

        Console.WriteLine("File received successfully.");
    }
}
