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
    public class Stairs : IEntity
    {
        bool exit;
        Circle boundingCircle;
        Color ent_color;
        Dungeon currentDungeon;
        float rotation;
        int id;
        protected int sheetIndex;
        int style;
        int subtype;
        int type;
        OBB boundingBox;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Texture2D debugBox;
        Vector2 position;

        int animation;
        VAxis axis;

        protected List<Collision> cData = new List<Collision>(); //lista delle collisioni
        protected List<int> cIds = new List<int>(); //lista degli id unici dell'entità

        public Stairs(bool exit, int style, Vector2 position, float rotation, int type, int subtype, ref SpriteSheet[] sheet, ref Dungeon dungeon)
        {
            axis = new VAxis(-128, 256);
            boundingBox=new OBB(new Vector2( position.X,position.Y+32), rotation, new Vector2(48, 20));
            boundingBox.DebugColor = new Color(0, 20, 0, 0);
             boundingCircle = new Circle(position, 20);
            currentDungeon = dungeon;
            debugBox = Globals.Box;
            id = Globals.AssignAnId();
            this.position = position;
            this.rotation = rotation;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.stairs;
            this.subtype = subtype;
            this.type = type;
            this.exit = exit;
            this.style = style;

            graphicOccupance = new Rectangle((int)position.X, (int)position.Y, 160, 160);

            if (exit)
            {
                animation = 0;
                name = "To floor " + (currentDungeon.getFloor+1).ToString();
            }
            else
            {
                animation = 1;
                name = "To floor " + (currentDungeon.getFloor-1).ToString();
            }
        }


        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            return l;
        }

        public bool Is_in_camera(Rectangle camera)
        {
            if (camera.Contains(graphicOccupance) || camera.Intersects(graphicOccupance))
                return true;
            else
                return false;
        }

        public void SetCollisionData(Collision Data)
        {
        }

        public void SetDamageData(DamageData Data)
        {
        }

        public virtual void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(style, animation), ent_color, rotation, sheet[sheetIndex].GetRotationCenter(style, animation), 1, SpriteEffects.None, Depths.stairs); //controllare la profondità
            //immagine sorgente, posizione su schermo, rettangolo del frame desiderato, colore, angolo di rotazione, centro di rotazione, fattore di scala, effetti, profondità
        }

        public virtual void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.foreBoxes);
            getBoundingCircle.Draw(camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 35);
            String s;
            s = "scale";
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
        }

        public void Update(GameTime gameTime)
        {
            //tendenzialmente inutile ma lasciato in vista di improbabili usi futuri
            foreach (Collision c in cData.Where(c => c.id != -1))
            {
                if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                {
                    if (c.collided)
                    {
                        cIds.Add(c.id);
                    }
                }
            }
            cIds.Clear();

            cData.Clear();
        }


        #region properties

        /// <summary>
        /// Ottiene o imposta il colore di un'entità
        /// </summary>
        public Color Color
        {
            get { return ent_color; }
            set { ent_color = value; }
        }

        /// <summary>
        /// Verifica se l'entità è "morta" e ridotta a un cadavere statico
        /// </summary>
        public bool Corpse
        {
            get { return false; }
        }

        /// <summary>
        /// Ottiene o imposta l'id di appartenenza di un'entità a una "fazione"
        /// </summary>
        public int Faction
        {
            get { return factionId; }
            set { factionId = value; }
        }


        /// <summary>
        /// Ottiene il valore che verifica se l'entità sta eseguendo un azione
        /// </summary>
        public bool generic
        {
            get { return false; }
        }

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l' entità verrà rimossa della lista dlle entità
        /// </summary>
        public bool Updatable
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// Ottiene l'asse verticale dell'oggetto
        /// </summary>
        public VAxis getAxis
        {
            get { return this.axis; }
        }

        /// <summary>
        /// Ottiene il cerchio di collisione principale
        /// </summary>
        public Circle getBoundingCircle
        {
            get { return boundingCircle; }
        }

        public Circle getInSightCircle
        {
            get { return null; }
        }

        /// <summary>
        /// Ottiene il danno descritto nel damagedata
        /// </summary>
        public DamageData getDamageDealt
        {
            get { return new DamageData(); }
        }

        /// <summary>
        /// Ottiene o imposta il dungeon corrente dell'entità
        /// </summary>
        public Dungeon dungeon
        {
            get { return currentDungeon; }
            set { currentDungeon = value; }
        }


        /// <summary>
        /// Ottiene la profondità dell'entità
        /// </summary>
        public float getDepth
        {
            get { return 0.97f; }
        }

        /// <summary>
        /// Ottiene l' id unico dell'entità
        /// </summary>
        public int getId
        {
            get { return this.id; }
        }

        /// <summary>
        /// Ottiene il sottotipo dell'entità
        /// </summary>
        public int getSubtype
        {
            get { if (exit) return 1; else return -1; } //scorretto, da modificare: restituisce la destinazione (il sottotipo originariamente indicava il tileset di riferimento)
        }

        /// <summary>
        /// Ottiene il tipo dell'entità
        /// </summary>
        public int getType
        {
            get { return type; }
        }

        /// <summary>
        /// Ottiene lo stato dell'entità
        /// </summary>
        public int State
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Ottiene il boundingBox dell'entità
        /// </summary>
        public OBB getBoundingBox
        {
            get { return this.boundingBox; }
        }

        /// <summary>
        /// Ottiene l'occupazione sullo schermo dell'entità
        /// </summary>

        public Rectangle getOccupance
        {
            get { return graphicOccupance; }
        }

        /// <summary>
        /// Ottiene il nome dell'entità
        /// </summary>
        public String getName
        {
            get { return this.name; }
        }

        /// <summary>
        /// Ottiene la posizione dell'entità
        /// </summary>
        public Vector2 getPosition
        {
            get { return position; }
        }

        /// <summary>
        /// Ottiene la direzione dell'entità
        /// </summary>
        public Vector2 getDirection
        {
            get
            {
                Vector2 asd = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                asd.Normalize();
                return asd;
            }
        }
        #endregion


        public int factionId { get; set; }
    }
}
