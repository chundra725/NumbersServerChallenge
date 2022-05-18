using NumbersServerChallenge;
using System;
using System.Collections.Concurrent;
using System.Threading;

public class Program
{
    static void Main(string[] args)
    {
        try
        {
            List<string> ips = new List<string>();

            System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (System.Net.IPAddress ip in entry.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ips.Add(ip.ToString());

            var port = 4000;
           
            // create the tcpServer object with params used to create TCPListener
            var tcpServer = new TCPServer(ips.First(), port);

            // seperate thread used to StartEventProcessing
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                tcpServer.StartEventProcessing();

            }).Start();

            // start TCPListener and ParameterizedThread used to processing incoming TCPClients
            tcpServer.Run().Wait(); 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }

    }
}