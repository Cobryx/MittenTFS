using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Mitten
{
    public class Waypoint
    {
        public Circle c;
        public OBB obb;
        public String direction;
        public int room;

        public Waypoint(Circle c)
        {
            this.c = c;
        }

        public Waypoint(Vector2 v, float radius, String dir, int room)
        {
            c = new Circle(v, radius);
            obb = new OBB(v, 0, new Vector2(2, 2));
            obb.DebugColor = Color.Navy;
            direction = dir;
            this.room = room;
        }

        public void Draw(Rectangle camera)
        {
            obb.Draw(camera, 1.0f);
        }
    }
}
