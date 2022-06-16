using Networking.JsonObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


class QuicknBriteUdpServer
{
    private UdpClient udpClient;

    private readonly Queue<string> incomingQueue = new Queue<string>();
    Thread receiveThread;
    private bool threadRunning = false;
    private string ip;
   
    private int sendingPort, receivingPort;
    Server.BasicServer server;

    IPEndPoint serverEndpoint;

    public void StartConnection(string ip, int sendPort, int receivePort, Server.BasicServer serv)
    {
        this.ip = ip;
        sendingPort = sendPort;
        receivingPort = receivePort;
        server = serv; //Used for echo'ing to other connections

        try { 
            udpClient = new UdpClient(receivePort);            
        } catch (Exception e) {
            Debug.WriteLine("Failed to listen for UDP at port " + receivePort + ": " + e.Message);
            return;
        }

        Debug.WriteLine("Created receiving client at ip  and port " + receivePort);       
        Debug.WriteLine("Set sendee at ip " + ip + " and port " + sendPort);

        //make the endpoints
        serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), sendingPort);

        //start thread
        receiveThread = new Thread(() => ListenForMessages(udpClient));
        receiveThread.IsBackground = true;        
        receiveThread.Start();

        threadRunning = true;
    }


    private void ListenForMessages(UdpClient client)
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (threadRunning)
        {
            try
            {
                Byte[] receiveBytes = client.Receive(ref remoteIpEndPoint); // Blocks until a message returns on this socket from a remote host.                
                      
                //Tell server: handle that message
                server.HandleMessage(receivingPort, receiveBytes);
                
            }
            catch (SocketException e)
            {
                // 10004 thrown when socket is closed                    
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error receiving data from udp client: " + e.Message);
            }
            Thread.Sleep(1);
        }
    }
    
    public void SendObject(MessagePacket msg)
    {
        string str = JsonConvert.SerializeObject(msg);
        SendBytes(Encoding.UTF8.GetBytes(str));
    }
    public void SendString(string message)
    {           
        SendBytes(Encoding.UTF8.GetBytes(message));
    }
    public void SendBytes(byte[] sendBytes)
    {        
        udpClient.Send(sendBytes, sendBytes.Length, serverEndpoint);
    }

    public void Stop()
    {
        threadRunning = false;
        receiveThread.Abort();
        udpClient.Close();
    }
}

