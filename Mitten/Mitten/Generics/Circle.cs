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
    [Serializable]
    public class Circle
    {
        Vector2 center;
        float radius;

        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Circle(Point center, float radius)
        {
            this.center = new Vector2((float)center.X, (float)center.Y);
            this.radius = radius;
        }

        public Circle(float x, float y, float radius)
        {
            this.center = new Vector2(x, y);
            this.radius = radius;
        }

        public float Area
        {
            get { return (radius * radius * (float)Math.PI); }
        }

        public Vector2 Center
        {
            get { return center; }
            set { center = value; }
        }

        public float Circumference
        {
            get { return (float)Math.PI * radius * 2; }
        }

        public float Diameter
        {
            get { return 2 * radius; }
        }

        public void Draw(Rectangle camera)
        {
            float theta = 0;
            for (int n = 0; n < (int)(this.radius * Math.PI * 2); n++)
            {
                Rectangle r = new Rectangle((int)(center.X + Math.Cos(theta) * radius), (int)(center.Y + Math.Sin(theta) * radius), 2, 2);
                r.Location = new Point(r.Location.X - camera.Location.X, r.Location.Y - camera.Location.Y);
                Globals.spriteBatch.Draw(Globals.Box, r, null, Color.Red, 0f, new Vector2(0, 0), SpriteEffects.None, 0);
                theta += (float)Math.PI/this.radius;
            }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public void Rotate(Vector2 rotationCenter, double rotationAngle)
        {
            Vector2 d = this.Center - rotationCenter;
            double x = d.X * Math.Cos(rotationAngle) - d.Y * Math.Sin(rotationAngle);
            double y = d.X * Math.Sin(rotationAngle) + d.Y * Math.Cos(rotationAngle);
            this.center = new Vector2(rotationCenter.X + (float)x, rotationCenter.Y + (float)y);
        }

        public float SectionArea(float angle)
        {
            return (angle / (2 * (float)Math.PI)) * radius * radius;
        }

        /* insert method here!
        public Color[,] DrawTexture()
        {
            Color[,] bitmap = new Color[(int)Diameter, (int)Diameter];



            return bitmap;
        }
        */

        public void VariateRadius(float multiplier)
        {
            this.radius *= multiplier;
        }

        public bool Contains(Circle circle)
        {
            if (Vector2.Distance(this.center, circle.center) <= (this.radius-circle.radius))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Verifica se due cerchi si intersecano.
        /// </summary>
        /// <param name="circle1">Cerchio 1</param>
        /// <param name="circle2">Cerchio 2</param>
        /// <returns></returns>
        public static bool intersect(Circle circle1, Circle circle2)
        {
            if (Vector2.Distance(circle1.Center, circle2.Center) < (circle1.Radius + circle2.Radius))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Restituisce la profondità di penetrazione fra i due cerchi
        /// </summary>
        /// <param name="circle1">Cerchio 1</param>
        /// <param name="circle2">Cerchio 2</param>
        /// <returns></returns>
        public static float penetration(Circle circle1, Circle circle2)
        {
            return (circle1.Radius + circle2.Radius) - Vector2.Distance(circle1.Center, circle2.Center);
        }
    }
}
