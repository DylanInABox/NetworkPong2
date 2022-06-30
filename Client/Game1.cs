using GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Networking;
using Networking.JsonObjects;
using System;
using System.Text;

namespace BaseProject
{
    public class Game1 : GameEnvironment
    {
        QuicknBriteUdpClient connection;
        MainGameState mainGameState;
        LobbyGameState lobbyGameState;

        // start up your UDP communicator...          
        string sendIp = "127.0.0.1";        
        int sendPort = 4000;
    

        enum State
        {
            WaitingForChoice,
            Connected,
            Started
        }
        State state = State.WaitingForChoice;

        protected override void LoadContent()
        {
            base.LoadContent();
            
            screen = new Point(800, 600);
            ApplyResolutionSettings();
            
            
            AssetManager.LoadSpriteSheet("spr_bullet");
            AssetManager.LoadSpriteSheet("spr_paddle");
            AssetManager.LoadSpriteSheet("spr_ball");
            

            lobbyGameState = new LobbyGameState(this);
            mainGameState = new MainGameState(this);
            GameStateManager.AddGameState("LobbyGameState", lobbyGameState);
            GameStateManager.AddGameState("MainGameState", mainGameState);

            GameStateManager.SwitchTo("LobbyGameState");

            //0. Make A CONNECTION-object (DO NOT start it yet):
            connection = new QuicknBriteUdpClient();


            
            
        }

        public void HandleMessage(int port, byte[] receiveBytes)
        {
            if (state == State.Connected)
            {
                //3.. wait for server to respond with "START"
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                if (returnData.Contains("START"))
                {
                    state = State.Started;
                    GameStateManager.SwitchTo("MainGameState");
                    mainGameState.StartGame(port - 4100);//TODO Magic nr
                   
                }
            } else if (state == State.Started)
            {
                //patch through to game:
                mainGameState.HandleMessage(receiveBytes);
            }
        }
       
        public void StartConnection(int i)
        {
            //1. pick a port and START connection
            connection.StartConnection(sendIp, sendPort + i, sendPort + 100 + i, this);

            //2. Send CONNECT to server
            connection.SendString("CONNECT");
            state = State.Connected;
        }

        public void SendObject(MessagePacket message)
        {
            //4. aand start echoing 
            connection.SendObject(message);
        }
       

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            mainGameState.OnExiting();
            //clientConnection.OnExiting();
        }
    }
}
