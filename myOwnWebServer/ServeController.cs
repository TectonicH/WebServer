using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Mime;
using System.Web;
using System.Collections.Generic;
using System.Text;



namespace myOwnWebServer
{

    public class ServeController
    {
        private const int kBufferSize = 8192;

        private ResponseHeader Head = new ResponseHeader();

        private string Root;
        private IPAddress IP;
        private int Port = 0;

        /* 
           Name	    : Server
           Purpose  : To instantiate a new server object to establish communication between the browser and the server
           Inputs	:	    string webRoot: Folder that contains all the files the server has "access"
                            IPAddress: IP address of the server
                            int webPort: port number the server lsitens to
           Outputs	:	NONE
           Returns	:	Nothing
        */

        public ServeController(string webRoot, IPAddress webIP, int webPort)
        {
            Root = webRoot;
            IP = webIP;
            Port = webPort;
        }

        // Name: StartListen
        // Purpose: Starts server to begin listening for browser requests, receives them, processes a response complete with header and body and then sends the response back to the requesting browser
        public void StartListen()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                TcpListener listener = new TcpListener(IP, Port);
                listener.Start();


                Logger.Log("[Begin] - webRoot=" + Root + ", webIP=" + IP.ToString() + ", -port=" + Port.ToString());

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    using (var networkStream = client.GetStream())
                    {
                        using (var streamReader = new StreamReader(networkStream))
                        {
                            byte[] data = new byte[8192];
                            int i = networkStream.Read(data, 0, data.Length);
                            string requests = Encoding.ASCII.GetString(data, 0, i);
                            if (requests == "")
                            {
                                client.Close();
                                continue;
                            }            
                            int posToGetRequest = requests.IndexOf("HTTP");
                            string logRequest = requests.Substring(0, posToGetRequest - 1);
                            Logger.Log(logRequest);
                            Root = Root.Replace("\\", "/");
                            Root = Root.TrimEnd('/');
                            string response = Head.GenerateHeader(requests, Root);
                            int indexNum = response.IndexOf("\r\nContent-Type");
                            string errorMessage = response.Substring(0, indexNum);
                            if (errorMessage != "HTTP/1.1 200 OK")
                            {
                                Logger.Log(errorMessage);
                            }
                            data = Encoding.ASCII.GetBytes(response);
                            data = AddContent(data, Head.ContentPath);
                            networkStream.Write(data, 0, data.Length);
                            Head.ValidRequest = false;
                        }
                    }

                    client.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                throw;
            }

        }
        // Name: AddContent
        // Purpose: Used to construct body data, whether it's an image or not. Reads all the data into a string, encodes it into bytes, appends it to existing data and returns compiled data
        // Inputs: byte[] data - Our encode header data
        //         string path - Path to get the file the browser needs to display

        private byte[] AddContent(byte[] data, string path)
        {
            List<byte> byteList = new List<byte>(data);

            if (Head.ValidRequest == false) // If invalid extension/request
            {
                byteList.AddRange(Encoding.ASCII.GetBytes(ResponseHeader.GetHTML(Head.StatusCode))); // Displays the correct HTTP status code
            }
            else
            {
                try
                {
                    if (Head.ContentType.StartsWith("image")) /// Do we need to read an image/gif?
                    {
                        byteList.AddRange(File.ReadAllBytes(path));

                    }
                    else // The rest of the valid extensions
                    {
                        byteList.AddRange(Encoding.ASCII.GetBytes(File.ReadAllText(path)));
                    }
                }
                catch (IOException ie)
                {
                    Logger.Log("An Exception Occured:" + ie.ToString());
                    byteList.AddRange(Encoding.ASCII.GetBytes(ResponseHeader.GetHTML(Head.StatusCode)));
                }

                // A 200 OK is log
                string okLog = "\r\nContent-Type: " + Head.ContentType + "\r\nContent-Length: " + Head.ContentLength + "\r\nServer: " + Head.ServerDetails + "\r\nDate: " + Head.ReplyDate;
                Logger.Log(okLog);

            }
            return byteList.ToArray();
        }
            
        
    }
    


        
    
}