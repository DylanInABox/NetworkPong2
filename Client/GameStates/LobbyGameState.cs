using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Networking.JsonObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameStates
{
    public class LobbyGameState : GameObjectList
    {
        private string playerNr = ""; 
        private TextGameObject nameInputField, startGameText;
        private List<Keys> supportedKeys = new List<Keys>();

        bool chosenPlayer = false;

        BaseProject.Game1 main;

        public LobbyGameState(BaseProject.Game1 mn)
        {
            main = mn;
            nameInputField = new TextGameObject("Spritefont");
            nameInputField.Text = "player ";

            for (int i = 48; i < 58; i++) //add numbers 0 - 9
            {
                supportedKeys.Add((Keys)i);
            }
            

            SetNameInputPosition();
            Add(nameInputField);

            startGameText = new TextGameObject("Spritefont") { Text = "Enter player (Press 1 or 2)"};
            startGameText.Position = new Vector2(GameEnvironment.Screen.X / 2 - startGameText.Size.X / 2, GameEnvironment.Screen.Y / 2 + 100);
            Add(startGameText);
        }

        public override void HandleInput(InputHelper inputHelper)
        {
            if (playerNr.Length <= 10)
            {
                foreach (Keys key in supportedKeys)
                {
                    if (inputHelper.KeyPressed(key) && !chosenPlayer)
                    {

                        playerNr += Encoding.ASCII.GetString(new byte[] { (byte)key });

                        int chosenId = int.Parse(playerNr) - 1;
                        main.StartConnection(chosenId);
                        chosenPlayer = true;
                        //set text
                        startGameText.Text = "Player set! waiting for second player...";
                    }
                }
            }

            

            nameInputField.Text = $"Player: {playerNr}";
            SetNameInputPosition();

            if (inputHelper.KeyPressed(Keys.Enter))
            {
                          
            }

            base.HandleInput(inputHelper);
        }
        
        public override void Update(GameTime gameTime)
        {
            Color newColor = startGameText.Color;
            byte alpha = (byte)(Math.Abs(Math.Sin(gameTime.TotalGameTime.TotalSeconds)) * 255);
            newColor.A = newColor.R = newColor.G = newColor.B = alpha;
            startGameText.Color = newColor;
            base.Update(gameTime);
        }

        private void SetNameInputPosition()
        {
            nameInputField.Position = new Vector2(GameEnvironment.Screen.X / 2 - nameInputField.Size.X / 2, GameEnvironment.Screen.Y / 2);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
