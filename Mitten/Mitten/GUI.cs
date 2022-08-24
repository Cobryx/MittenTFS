using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    public class GUI
    {
        SpriteFont font;
        Texture2D pixel;
        int pnumber;
        int cam_option;
        
        public GUI(SpriteFont font, int pnumber, int cam_option)
        {
            this.font = font;
            this.pnumber = pnumber;
            this.cam_option = cam_option;

        }
        
        public void Update()
        {
        }

        public void draw(ref SpriteBatch spriteBatch)
        {
            
            //spriteBatch.DrawString(font, "ABAB", new Vector2(0.0f), Color.Red, 0.0f, new Vector2(0.0f), 1.0f, SpriteEffects.None, 0.5f);
        }


        public void draw(ref SpriteBatch spriteBatch, Viewport viewport)
        {
            
        }
    }
}
