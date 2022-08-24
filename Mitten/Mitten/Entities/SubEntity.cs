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
    class SubEntity : IEntity
    {
        IEntity parent;

        float depth;
        float rotationAngle;

        Circle boundingCircle;
        Dungeon currentDungeon;
        OBB boundingBox;
        VAxis axis;
        Vector2 direction;
        Vector2 position;

        public SubEntity(IEntity parent, float depth, Circle boundingCircle, OBB boundingBox, Vector2 position, Vector2 direction, ref Dungeon dungeon)
        {
            this.parent = parent;
            currentDungeon = dungeon;
            this.boundingCircle = boundingCircle;
            this.boundingBox = boundingBox;
            this.position = position;
            this.direction = direction;
        }

        public List<IEntity> GetSpawningList()
        {
            return null;
        }

        public bool Is_in_camera(Rectangle camera)
        {
            //da rivedere, come tutti gli altri metodi
            return false;
        }

        public void SetCollisionData(Collision Data)
        {
            parent.SetCollisionData(Data);
        }

        public void SetDungeon(ref Dungeon dungeon)
        {
            currentDungeon = dungeon;
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Update(float depth, float rotationAngle, Vector2 position, Vector2 direction)
        {
            this.depth = depth;
            boundingBox.UpdateAxis(rotationAngle);
            boundingBox.Origin = position;
            this.position = position;
            this.direction = direction;
        }

        public void Draw(Rectangle camera)
        {
            boundingBox.Draw(camera, depth);
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingCircle.Draw(camera);
            //boundingBox.Draw(BoxTexture, camera, Globals.spriteBatch, 0f);
            boundingBox.Draw(camera, depth);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top + 15);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1.5f, SpriteEffects.None, 0f);
        }

        #region properties
        /// <summary>
        /// Ottiene o imposta il colore di un'entità
        /// </summary>
        public Color Color
        {
            get { return parent.Color; }
            set { }
        }

        /// <summary>
        /// Verifica se l'entità è "morta" e ridotta a un cadavere statico
        /// </summary>
        public bool Corpse
        {
            get { return parent.Corpse; }
        }

        /// <summary>
        /// Ottiene o imposta l'id di appartenenza di un'entità a una "fazione"
        /// </summary>
        public int Faction
        {
            get { return parent.Faction; }
            set { parent.Faction = value; }
        }


        public bool generic
        {
            get { return parent.generic; }
        }

        /// <summary>
        /// Ottiene l'asse verticale dell'oggetto
        /// </summary>
        public VAxis getAxis
        {
            get { return this.axis; }
        }

        public OBB getBoundingBox
        {
            get { return this.boundingBox; }
        }

        public Circle getBoundingCircle
        {
            get { return this.boundingCircle; }
        }

        public float getDepth
        {
            get { return depth; }
        }

        public Vector2 getDirection
        {
            get { return this.direction; }
        }

        public int getId
        {
            get { return parent.getId; }
        }

        public String getName
        {
            get { return parent.getName; }
        }

        public Rectangle getOccupance
        {
            get { return parent.getOccupance; }
        }

        public Vector2 getPosition
        {
            get { return position; }
        }

        /// <summary>
        /// Ottiene il sottotipo dell'entità
        /// </summary>
        public int getSubtype
        {
            get { return parent.getSubtype; }
        }

        public int getType
        {
            get { return parent.getType; }
        }

        public bool Updatable
        {
            get { return parent.Updatable; }
        }

        public IEntity getParent
        {
            get { return parent; }
        }
        #endregion
    }
}
