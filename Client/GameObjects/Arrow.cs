using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.GameObjects
{
    internal class Arrow : RotatingSpriteGameObject
    {
        
        public Arrow(Vector2 startPosition) : base("spr_arrow")
        {
            this.position = startPosition;                  
            
        }       
    }
}
