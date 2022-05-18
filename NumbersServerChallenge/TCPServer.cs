
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace NumbersServerChallenge
{
    public class TCPServer
    {
        private IPAddress ipAddress;
        private int port = 0;
        private static ConcurrentBag<string> concurrentBag = new ConcurrentBag<string>();
        private static int duplicateCount = 0;
        private static int totalUniqueCount = 0;
        private static int lastDuplicateCount = 0;
        private static int lastTotalUniqueCount = 0;

        // We have a parameterized constructor taking IP & Port as 
        // parameters to instantiate a TcpListener object
        public TCPServer(string ip, int port1)
        {
            port = port1;
            IPAddress localAddr = IPAddress.Parse(ip);
            ipAddress = localAddr;
        }

        public async Task Run()
        {
            // TcpListener keeps listening for incoming connections
            // For every new client which gets connected; a new thread for the Process Method is created

            var listener = new TcpListener(this.ipAddress, this.port);
            listener.Start();
            while (true)
            {
                try
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    var t = new Thread(new ParameterizedThreadStart(Process));
                    t.Start(tcpClient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                finally
                {
                    //TODO: Shut down gracefully 
                }
            }
        }

        // Process method receive bytes stream from the TCP Client
        // Adds the 9 digit number to thread safe ConcurrentBag collection 
        

        private void Process(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            try
            {
                var data = string.Empty;
                var bytes = new Byte[256];
                int i;
                using (NetworkStream stream = new NetworkStream(tcpClient.GetStream().Socket))
                {
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);

                        // logic used to manage count statistics should be refactered out to its own method
                        // this logic is not correct for more then the first few outputs, once there are many threads
                        // the values used to present to the console view is more incorrect as the total 
                        // connections increase.  
                        // used Mutex, and Lock but was unsuccessful in getting a accuate count
                        // this is probaby due to my lack of using Mutx or Locks.  I would need furter time to evalute
                        // a solution which uses Locking for optimistic concurrency.
                        
                        if (!concurrentBag.Contains(data))
                        {
                            concurrentBag.Add(data);
                            totalUniqueCount = concurrentBag.Count();
                            lastTotalUniqueCount = totalUniqueCount - lastTotalUniqueCount;
                        }
                        else
                        {
                            ++duplicateCount;
                            lastDuplicateCount = duplicateCount - lastDuplicateCount;
                        }
                    }
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
                throw;
            }
            finally
            {
                tcpClient.Close();
            }
        }
        // WriteToLog used by the EventProcessor to log 9 digit unique numbers using async 
        private async Task WriteToLog()
        {
            var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter writer = new StreamWriter(Path.Combine(docPath, "numbers.log.txt"), true))
            {
                writer.AutoFlush = true;
                var result = string.Join(Environment.NewLine, concurrentBag);
                await writer.WriteLineAsync(result);
            }
        }
        // StartEventProcessing used as timer event to push unique, duplicate and total numbers out to console
        public void StartEventProcessing()
        {
            // Create a timer with a 10 second interval.
            var timer = new System.Timers.Timer(10000);
            timer.AutoReset = true;
            timer.Enabled = true;
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += async (s, e) =>
            {
                Console.Clear();
                totalUniqueCount = concurrentBag.Count();
                lastTotalUniqueCount = totalUniqueCount - lastTotalUniqueCount;
                lastDuplicateCount = duplicateCount - lastDuplicateCount;
                Console.WriteLine(" {0} unique numbers, {1} duplicates. Unique total: {2}", lastTotalUniqueCount, lastDuplicateCount, concurrentBag.Count());
                await WriteToLog();
            };

        }

    }
}
