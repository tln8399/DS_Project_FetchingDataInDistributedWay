using Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;

namespace DSProject
{
    class NodeHandler
    {
        string id;
        string remoteHostName;
        int port;
        //static DataManager dm = new DataManager();

        public NodeHandler(string id, string hostName, int port)
        {
            this.id = id;
            this.remoteHostName = hostName;
            this.port = port;
        }

        public void execute(object process)
        {
            int count = 0;
            Console.WriteLine(this.id + " connecting to fetch url");
            TcpClient conn = new TcpClient(remoteHostName, port);
            NetworkStream nwStream = conn.GetStream();
            //StreamWriter writer = new StreamWriter(nwStream);
            //StreamReader reader = new StreamReader(nwStream);

            IFormatter formatter = new BinaryFormatter(); 
            DataManager dm = (DataManager)process;
            //URL url = dm.getURL();


            //writer.WriteLine("Hello server.. I am " + this.id);
            //writer.Flush();

                while (true)
                {
                    Console.WriteLine(this.id + " waiting for mutex ..");
                    DataManager.urlMutex.WaitOne();

                    URL url = dm.getURL();
                    DataManager.urlMutex.ReleaseMutex();
                    Console.WriteLine(this.id + " releasing mutex ..");

                    if (url != null)
                    {
                        count++;
                        Console.WriteLine(this.id + " : URL: " + url.Url);
                        //writer.WriteLine(this.id + ": Using URL: " + url.url);
                        formatter.Serialize(nwStream, url);
                        
                        //writer.Flush();

                        //server will process data and pass it to the client
                        Output result = (Output)formatter.Deserialize(nwStream);

                        DataManager.outMutex.WaitOne();
                        if (result != null)
                        {
                           // dm.outputTray.Add(result);

                            //write in excel
                            dm.writeInFile(result);
                        }
                        DataManager.outMutex.ReleaseMutex();
                    }
                    else
                    {
                        //DataManager.myMutex.ReleaseMutex();
                        Console.WriteLine(this.id + " URL Tray empty. My URL count: "+count+" Elapsed time: "+ DataManager.watch.ElapsedMilliseconds + " . Bye...");
                        break;
                    }
                    
                    //Console.WriteLine("hit a key.." + this.remoteHostName);
                    //Console.ReadLine();

                   // Console.WriteLine(this.id + " releasing mutex ..");
                    //DataManager.myMutex.ReleaseMutex();
               }
            
        }
    }

   
}
