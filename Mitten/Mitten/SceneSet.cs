using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    public class SceneSet
    {
        Texture2D image;
        Rectangle[] sprites;
        Vector2[] pivots;

        public SceneSet(Texture2D image, StreamReader map)
        {
            String stringa;
            char[] vseparator = { ';' };
            char[] cseparator = { ':' };
            int lines = 0;

            while (!map.EndOfStream)
            {
                map.ReadLine();
                lines++;
            }

            map.BaseStream.Seek(0, SeekOrigin.Begin);
            this.image = image;
            sprites = new Rectangle[lines];
            pivots = new Vector2[lines];
            for (int i = 0; i < lines; i++)
            {
                stringa = map.ReadLine();
                //String[] vertex = stringa.Split(vseparator, StringSplitOptions.None);
                String[] coordinate = stringa.Split(cseparator, StringSplitOptions.None);
                sprites[i] = new Rectangle(Int32.Parse(coordinate[0]), Int32.Parse(coordinate[1]), Int32.Parse(coordinate[2]), Int32.Parse(coordinate[3]));
                pivots[i] = new Vector2(float.Parse(coordinate[4]), float.Parse(coordinate[5]));
            }
        }

        public Rectangle GetBounds(int n)
        {
            return sprites[n];
        }

        public Vector2 GetRotationCenter(int n)
        {
            return pivots[n];
        }

        public Texture2D SourceBitmap
        {
            get { return image; }
        }
    }
}
