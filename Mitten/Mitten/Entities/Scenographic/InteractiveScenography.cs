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
    public abstract class InteractiveScenography : IEntity
    {
        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool updatable = true;
        protected bool fl_attack = false; // indica se l' attacco è attivo
        protected bool active = false;
        protected bool corpse = false;

        //definizione cerchi collidenti
        protected Circle boundingCircle;

        protected Color ent_color; 

        protected Dungeon currentDungeon; //referred dungeon

        protected EntityManager status;

        protected float depth; //profondità dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed; //velocità dell 'entità

        protected IEntity target; //obiettivo passato dall' IAManager

        protected float[] droppable;

        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        public int genericAction; //verifica se si sta effetuando un azione
        protected int sheetIndex;

        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità

        protected OBB boundingBox;
        protected Rectangle graphicOccupance;

        protected SpriteSheet[] sheet; //riferiemento allo spritesheet

        protected String name; //nome entità

        protected Vector2 direction; //direzione entità
        protected Vector2 oldPosition; //position to restore in case of collision
        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 pivot; // perno di rotazione entità
        protected Vector2 scale; //scala dimensionamento entità

        protected List<Collision> cData = new List<Collision>(); //lista delle collisioni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell entità
        protected List<Item> inventory = new List<Item>();

        #endregion

        public bool requiredPath = false;
        public bool requiredAlternativePath = false;
        public IEntity obstructive;

        public OBB cCheckBox;
        public OBB sensorBox;
        public float checkAngle;

        protected Krypton.ShadowHull shadow;

        protected float desiredAngle;
        protected Vector2[] targetPosition;

        protected Random randMilliSec;

        protected Vector2 targetToLook = new Vector2();
        protected List<Circle> pathToFollow;
        protected int w = 0;
        protected int factionId;
        protected VAxis axis;

        public InteractiveScenography(Vector2 position, float rotation, int type, int subtype, ref SpriteSheet[] sheet, ref Dungeon dungeon)
        {
           
            ent_color = Color.White;
            scale = new Vector2(1,1);
            this.sheet = sheet;

            boundingCircle = new Circle(position, 10);
           
            this.position = position;
            this.rotationAngle = rotation;
        }


        public void SetCollisionData(Collision Data)
        {
            this.cData.Add(Data);
        }

        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            l = spawned.GetRange(0, spawned.Count);
            spawned.Clear();
            return l;
        }

        public virtual void Update(GameTime gameTime)
        {
            status.Update(gameTime);
            status.AutoOff();

           /* boundingBox = new OBB(position, rotationAngle, new Vector2(sheet[sheetIndex].Frame(0, 0).Width / 2, sheet[sheetIndex].Frame(0, 0).Height / 2));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;*/
            
        }

        public bool Is_in_camera(Rectangle camera)
        {
            if (camera.Contains(graphicOccupance) || camera.Intersects(graphicOccupance))
                return true;
            else
                return false;
        }

        public virtual void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[this.sheetIndex].sourceBitmap, pos, sheet[this.sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[this.sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, SpriteEffects.None, Depths.InteractiveScenography + (float)id / Globals.max_entities);
            //Globals.spriteBatch.Draw(sheet[sheetIndex].SourceBitmap, pos, sheet[sheetIndex].Frame(currentAnimation, animation.GetFrame()), ent_color, rotationAngle, sheet[sheetIndex].getRotationCenter(currentAnimation , animation.GetFrame(currentAnimation)), scale, SpriteEffects.None, depth);
            //immagine sorgente, posizione su schermo, rettangolo del frame desiderato, colore, angolo di rotazione, centro di rotazione, fattore di scala, effetti, profondità
        }

        public virtual void DrawCollidedObjectDebug(Rectangle camera)
        {
            //boundingBox.Draw(camera, Depths.boxes);
            if (sensorBox != null) sensorBox.Draw(camera, Depths.boxes);
            if (Globals.extremeDMode)
                boundingCircle.Draw(camera);
        }

        public virtual void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 25);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
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
            get { return corpse; }
        }

        /// <summary>
        /// Ottiene o imposta l'id di appartenenza di un'entità a una "fazione"
        /// </summary>
        public int Faction
        {
            get { return factionId; }
            set { factionId = value; }
        }

        public Krypton.ShadowHull Shadow
        { 
            get { return shadow; } set { shadow = value; } }

        /// <summary>
        /// Ottiene il valore che verifica se l' entità sta eseguendo un azione
        /// </summary>
        public bool generic
        {
            get { if (genericAction > 0) return true; else return false; }
        }

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l'entità verrà rimossa della lista delle entità
        /// </summary>
        public bool Updatable
        {
            get { return updatable; }
            set { updatable = value; }
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
            get { return this.boundingCircle; }
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
            get { return depth; }
        }

        /// <summary>
        /// Ottiene l' id unico dell'entità
        /// </summary>
        public int getId
        {
            get { return this.id; }
        }

        /// <summary>
        /// Ottiene l'angolo di rotazione rispetto al versore (1,0)
        /// </summary>
        public float getRotationAngle
        {
            get { return rotationAngle; }
        }

        /// <summary>
        /// Ottiene il sottotipo dell'entità
        /// </summary>
        public int getSubtype
        {
            get { return -1; }  //ci servirà il tipo di mostro (esempio, varianti di mostro)
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
            get { return state; }
            set { state = value; }
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
                Vector2 asd = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
                asd.Normalize();
                return asd;
            }
        }
        #endregion
    }
}