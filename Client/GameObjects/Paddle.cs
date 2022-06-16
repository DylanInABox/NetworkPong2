using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.GameObjects
{
    internal class Paddle : RotatingSpriteGameObject
    {
        
        public Paddle(Vector2 startPosition) : base("spr_paddle")
        {
            this.position = startPosition;                
            
        }       
    }
}
