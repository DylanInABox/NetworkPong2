using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class BasicServer
    {      
        int nPlayers = 2;
        int connectedPlayers = 0;
        int receivePort = 4000;
        int sendPortOffset = 100;
        string ipAddress = "127.0.0.1";

        enum State
        {
            Connecting,            
            Started
        }
        State state = State.Connecting;

        QuicknBriteUdpServer[] connection;

        public BasicServer()
        {                                    
            connection = new QuicknBriteUdpServer[nPlayers];
            
            for (int i = 0; i < nPlayers; i++)
            {
                connection[i] = new QuicknBriteUdpServer();
                connection[i].StartConnection(ipAddress, receivePort + sendPortOffset + i, receivePort + i, this);
            }            
        }

        public void HandleMessage(int port, byte[] receiveBytes)
        {
            // if msg = "CONNECT" {
            string returnData = Encoding.UTF8.GetString(receiveBytes);
            
            switch (state)
            {
                case State.Connecting:
                    connectedPlayers++;
                    Console.WriteLine("Client Connected on port " + port);
                    if (connectedPlayers == 2)
                    {
                        state = State.Started;
                        //START both clients
                        for (int i = 0; i < nPlayers; i++)
                        {
                            connection[i].SendString("START");
                        }
                        Console.WriteLine("Starting both clients..");
                    }
                    break;
                case State.Started:
                    if (returnData.Contains("UPDATE_POS"))
                        SendAllExceptPort(port, receiveBytes);
                    //else if (etc)
                    break;
            }
            
            
        }

        public void SendAllExceptPort(int incPort, byte[] receiveBytes) {
            for (int i = 0; i < nPlayers; i++)
            {
                if (i != (incPort - receivePort))
                {
                    connection[i].SendBytes(receiveBytes);
                }
            }
        }
    }
}
