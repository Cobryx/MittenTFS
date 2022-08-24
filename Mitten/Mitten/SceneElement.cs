using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    class SceneElement
    {
        int number;
        Color color;
        Point position;
        Vector2 vPosition;
        float rotation;
        float depth;
        Rectangle border;
        bool tileRelative = false;

   /*     public SceneElement(int number, Point position, float angle, Rectangle border, float depth, Color color)
        {
            this.number = number;
            this.position = position;
            this.rotation = angle;
            this.color = color;
            this.border = border;
            this.depth = depth;
            this.vPosition = new Vector2(position.X*32, 32*position.Y);
            tileRelative = true;
        }*/

        public SceneElement(int number, Vector2 position, float angle, int rectangleSize, float depth, Color color)
        {
            this.number = number;
            this.position = new Point((int)position.X/32, (int)position.Y/32);
            this.rotation = angle;
            this.color = color;
            this.border = new Rectangle((int)position.X-rectangleSize/2, (int)position.Y-rectangleSize/2, rectangleSize, rectangleSize);
            this.depth = depth;
            this.vPosition = position;
            tileRelative = false;
        }

        public bool IsVisible(Rectangle camera)
        {
            if(camera.Contains(border) || camera.Intersects(border))
                return true;
            else return false;
        }

        public Rectangle getBorder
        {
            get { return border; }
        }
        
        public Color getColor
        {
            get { return color; }
        }

        public float getDepth
        {
            get { return depth; }
        }

        public int getNumber
        {
            get { return number; }
        }

        public Point getPosition
        {
            get { return position; }
        }

        public Vector2 getVPosition
        {
            get { return vPosition; }
        }

        public float getRotationAngle
        {
            get { return rotation; }
        }

        public bool TileAligned
        {
            get { return tileRelative; }
        }
    }
}
