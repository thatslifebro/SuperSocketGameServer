namespace GameServer;

class Program
{
    static void Main(string[] args)
    {
        var serverOption = ParseCommandLine(args);
        if (serverOption == null)
        {
            return;
        }

        var mainServer = new MainServer();

        mainServer.InitConfig(serverOption);

        mainServer.CreateStartServer();

        Console.WriteLine("Press any key to exit...");

        while (true)
        {
            System.Threading.Thread.Sleep(50);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey();
                if (key.KeyChar == 'q')
                {
                    Console.WriteLine("Server is shutting down...");

                    mainServer.ServerStop();
                }
            }
        }

    }

    static ServerOption? ParseCommandLine(string[] args)
    {
        var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

        if (result == null)
        {
            System.Console.WriteLine("Failed Command Line");
            return null;
        }

        return result.Value;
    }
}