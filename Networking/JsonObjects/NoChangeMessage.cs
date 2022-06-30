using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.JsonObjects
{
    public class NoChangeMessage : MessagePacket
    {
        public int tickNumber;
        public Vector2 direction;
        public NoChangeMessage()
        {
            msgType = "NO_CHANGE";
        }
    }
}
