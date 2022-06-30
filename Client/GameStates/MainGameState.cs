using Client.GameObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Networking.JsonObjects;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace GameStates
{
    public class MainGameState : GameObjectList
    {
        private int myId;
        private Paddle leftPaddle, rightPaddle, myPaddle, theirPaddle;
        private Arrow marker;
        private Ball ball;

        TextGameObject tickCounterText;

        BaseProject.Game1 main;

        int tickCounter = 0;
        //physics:
        Vector2 yIncr = new Vector2(0, 10);

        //variables used for checking dropped frames..
        Stopwatch stopWatch;
        readonly double accurateMs = 1000 / 60.0;
        readonly int MAX_ATTEMPTS = 3;
        //--------------------------------------------


        public MainGameState(BaseProject.Game1 mn)
        {
            main = mn;

            leftPaddle = new Paddle(new Vector2(100 - 12, 100));
            rightPaddle = new Paddle(new Vector2(700 - 12, 100));
            marker = new Arrow(new Vector2());
            ball = new Ball(400, 400, 2, -2);

            Add(leftPaddle);
            Add(rightPaddle);
            Add(marker);
            Add(ball);

            tickCounterText = new TextGameObject("Spritefont");
            tickCounterText.Text = "sds";
            tickCounterText.Position = new Vector2(350, 30);
            Add(tickCounterText);
        }

        public void StartGame(int id)
        {
            Debug.WriteLine("My ID is " + id);
            myId = id;
            if (myId == 0)
            {
                myPaddle = leftPaddle;
                theirPaddle = rightPaddle;
            }
            else
            {
                myPaddle = rightPaddle;
                theirPaddle = leftPaddle;
            }
            //Set Marker at the right spot!
            marker.Position = new Vector2(70 + myId * 600, 10);

            //Start a Stopwatch (necesary to check for frame drops)
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        /// <summary>
        /// Tick() is the main method for handling game events, physics, etc.
        /// </summary>
        private void Tick()
        {
            //update tick
            tickCounter++;
            tickCounterText.Text = "frame tick: " + tickCounter;

            //Ball collisions with Walls       
            if (ball.Position.X <= 0 || ball.Position.X >= 800 - ball.size)
            {
                ball.BounceHorizontal();
            }
            if (ball.Position.Y <= 0 || ball.Position.Y >= 600 - ball.size)
            {
                ball.BounceVertical();
            }

            // a (crude and simple) collisionCheck for Paddles
            if (ball.CollidesWith(myPaddle))
            {
                ball.BounceHorizontal();
                PaddleHitMessage message = new PaddleHitMessage()
                {
                    position = myPaddle.Position,
                    ballDirection = new Vector2()
                    {
                        X = ball.vx,
                        Y = ball.vy
                    },
                    ballPosition = new Vector2()
                    {
                        X = ball.x,
                        Y = ball.y
                    },
                    tickNumber = tickCounter
                };

                main.SendObject(message);
            }

            if (tickCounter - previousTick > 1)
                //theirPaddle.Position += yIncr * lastDirection;

                //Update ball (nb: DON'T replace this with MonoGame's Update; messes up the determinism of frames)
                ball.Tick();

        }

        Vector2 selfLastInputDirection;

        /// <summary>
        /// Use HandleInput for all the code when 'pressing keyboard buttons'
        /// </summary>
        public override void HandleInput(InputHelper inputHelper)
        {
            base.HandleInput(inputHelper);
            MessagePacket message = new MessagePacket();
            Vector2 playerDirection;

            if (inputHelper.IsKeyDown(Keys.W))
            {
                myPaddle.Position -= yIncr;
                playerDirection = -Vector2.UnitY;
            }
            else if (inputHelper.IsKeyDown(Keys.S))
            {
                myPaddle.Position += yIncr;
                playerDirection = Vector2.UnitY;
            }
            else
            {
                myPaddle.Velocity = Vector2.Zero;
                playerDirection = Vector2.Zero;
            }

            if (selfLastInputDirection == playerDirection)
            {
                message = new NoChangeMessage()
                {
                    direction = selfLastInputDirection,
                    tickNumber = tickCounter
                };
                main.SendObject(message);

            }
            else
            {
                message = new UpdatePaddleMessage()
                {
                    position = myPaddle.Position,
                    direction = playerDirection,
                    tickNumber = tickCounter
                };

                selfLastInputDirection = playerDirection;
                main.SendObject(message);

            }
            Debug.WriteLine(message.msgType);
        }

        int previousTick = 0;
        Vector2 lastDirection = Vector2.Zero;
        Vector2 lastCalculatedPosition = Vector2.Zero;

        /// <summary>
        /// HandleMessage is called when a network-message is received from the server
        /// </summary>
        public void HandleMessage(byte[] receiveBytes)
        {
            string returnData = Encoding.UTF8.GetString(receiveBytes);

            //When Other paddle is moved
            if (returnData.Contains("UPDATE_POS"))
            {
                UpdatePaddleMessage msg = JsonConvert.DeserializeObject<UpdatePaddleMessage>(returnData);

                lastDirection = msg.direction;
                theirPaddle.Position = msg.position + yIncr * lastDirection * (tickCounter - msg.tickNumber);

                previousTick = msg.tickNumber;
            }
            else if (returnData.Contains("NO_CHANGE"))
            {
                NoChangeMessage msg = JsonConvert.DeserializeObject<NoChangeMessage>(returnData);
                theirPaddle.Position += yIncr * msg.direction;
                previousTick = msg.tickNumber;
            }
            else if (returnData.Contains("HIT"))
            {
                PaddleHitMessage msg = JsonConvert.DeserializeObject<PaddleHitMessage>(returnData);
                theirPaddle.Position = msg.position;
                ball.x = (int)(msg.ballPosition.X + msg.ballDirection.X * (tickCounter - msg.tickNumber));
                ball.y = (int)(msg.ballPosition.Y + msg.ballDirection.Y * (tickCounter - msg.tickNumber));
                ball.vx = (int)msg.ballDirection.X;
                ball.vy = (int)msg.ballDirection.Y;
                previousTick = msg.tickNumber;
            }
        }
        //--------------------------------------------------------

        /// <summary>
        /// WARNING! Update() IS NOT used anymore for game physics etc; put all game logic in Tick() instead
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            /* NB:
             * voor het vak Infrastructure is de method "Update" NIET  handig om te gebruiken voor game-updates; 
             * GEBRUIK SVP method "Tick()" ... (Anders is de Game Loop niet deterministisch!)
             *
             * (verklaring: "Update()" in Monogame houdt blijkbaar zich niet netjes aan 60fps. 
             * Bijvoorbeeld: als je het window versleept, krijg je een flinke 'frame drop'.
             * Onderstaande code compenseert dit a.d.h.v. SystemTime, om daarmee verloren ticks in te halen)
             *             
            */

            int desiredTickCount, attempts = 0;
            desiredTickCount = (int)(stopWatch.ElapsedMilliseconds / accurateMs);

            // If there is a difference in ticks (desired vs actual): Do a tick MULTIPLE times until it's equal again..                        
            while (desiredTickCount > tickCounter)
            {
                Tick();

                //note: capped at maximum of [MAX_ATTEMPTS] ticks per update, to be safe.. Any remaining ticks will transfer to next frame.
                attempts++;
                if (attempts >= MAX_ATTEMPTS) break;
            }



            base.Update(gameTime);
        }

        public override void Reset()
        {
            base.Reset();
        }

        public void OnExiting()
        {

        }




    }
}
