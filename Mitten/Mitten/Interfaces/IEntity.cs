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
    public interface IEntity
    {
        void SetCollisionData(Collision Data);
        
        List<IEntity> GetSpawningList();
        
        void Draw(Rectangle camera);
        void DrawCollidedObjectDebug(Rectangle camera);
        void DrawDebug(Rectangle camera, ref SpriteFont debugFont);
        bool Is_in_camera(Rectangle camera);
        void Update(GameTime gameTime);

        bool Corpse { get; }
        bool generic { get; }
        Circle getBoundingCircle { get; }
        Color Color { get; set; }
        float getDepth { get; }
        int getId { get; }
        OBB getBoundingBox { get; }
        Vector2 getDirection { get; }


        bool Updatable { get; }
        int Faction { get; set; }
        int getSubtype { get; }
        int getType { get; }
        Rectangle getOccupance { get; }
        String getName { get; }
        VAxis getAxis { get; }
        Vector2 getPosition { get; }
    }
}
