using System.Text;
using System.Net.Sockets;
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
           
            Random random = new Random();
            var randNum = 0;
            Parallel.For(0, 10000, i =>
            {
                Thread.Sleep(1000);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    randNum = random.Next(000000000, 99999999);
                    Connect(ips.First(), randNum.ToString("D9"));
                }).Start();

            });

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
        }

    }

    static async void Connect(String server, String message)
    {

        try
        {
            var port = 4000;
            var client = new TcpClient(server, port);
            var stream = client.GetStream();
            Thread.Sleep(10000);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            // Send the message to the connected TcpServer. 
            await stream.WriteAsync(data, 0, data.Length);
            stream.Close();
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
           
        }
    }

}