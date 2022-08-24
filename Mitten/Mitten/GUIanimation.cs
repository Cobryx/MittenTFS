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
    public class GUIanimation
    {
        SpriteSheet[] sheet;
        public bool Stop {get; set;}
        int animation {get; set;}
        int frame = 0;
        int GUItime;
        int pace = 50;
        int sheetIndex;


        public GUIanimation(ref SpriteSheet[] sheet, int sheetIndex)
        {
            this.sheet = sheet;
            this.sheetIndex = sheetIndex;
        }

        public void Update(GameTime gameTime, int pace)
        {
            if (!Stop)
            {
                this.pace = pace;
                GUItime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (GUItime > pace)
                {
                    GUItime = 0;
                    frame++;
                    frame %= sheet[sheetIndex].GetFrameNumber(animation);
                }
            }
        }

        public void Draw(Vector2 offset, float scale)
        {
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, offset, sheet[sheetIndex].Frame(animation, frame), Color.White, 0.0f, new Vector2(0, 0), scale, SpriteEffects.None, Depths.playerGUI);
        }
    }
}
