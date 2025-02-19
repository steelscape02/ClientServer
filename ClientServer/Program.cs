// See https://aka.ms/new-console-template for more information


namespace ClientServer;

public abstract class Program
{
    private const int Port = 8888;
    public static void Main()
    {
        Console.Write("Select role (1 - Server, 2 - Client): ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                ClientServer.StartServer(Port);
                break;
            case "2":
            {
                var serverIp = "";
                while (serverIp == "")
                {
                    Console.Write("Enter Server IP: ");
                    serverIp = Console.ReadLine();
                }
                
                if(serverIp != null) ClientServer.StartClient(serverIp,Port);
                else Console.WriteLine("Invalid server ip");
                break;
            }
            default:
                Console.WriteLine("Invalid choice. Restart the program.");
                break;
        }
    }

}