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
    public interface ICaster
    {
        int getId { get; }
        float getRotationAngle { get; }
        Vector2 getDirection { get; }
        Vector2 getPosition { get; }
        Vector2 magicOrigin1 { get; }
        Vector2 magicOrigin2 { get; }
        bool ActiveCaster { get; }
    }
}
