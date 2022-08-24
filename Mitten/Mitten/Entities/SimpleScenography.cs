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
    public class SimpleScenography : IEntity
    {
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool corpse = false;
        protected bool updatable = true;
        
        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Color ent_color; //filtro entità

        //definizione gestore dei danni e parametri danni

        protected Dungeon currentDungeon; //referred dungeon

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità

        protected int currentAnimation; //animazione corrente
        protected int cause_of_death = -1; //causa della morte
        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        //protected int idleTime; //tempo in cui l' entità è stata in idle
        protected int sheetIndex;
        protected int state; //stato dell entità attualmente in corso
        protected int subtype;
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

        VAxis axis;

        protected List<int> propertyList = new List<int>(); //da implementare


        protected List<Collision> cData; //lista delle collisioni
        protected List<DamageData> dData; // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell'entità
        protected List<int> dIds = new List<int>(); //lista degli id unici dei danni
        protected int factionId;

        protected int projectile = -1;      //se nessun proiettile è impostato
        protected int launched = -1;        //nessuna munizione spesa nel ciclo corrente

        public SimpleScenography(Vector2 position, Vector2 dimensions, float height, float depth, float rotation, int subtype, ref SpriteSheet[] sheet, ref Dungeon dungeon, int id=-1)
        {
            this.position = position;
            this.depth = depth;
            this.rotationAngle = rotation;
            this.type = (int)entityTypes.simplescenography;
            this.subtype = subtype;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.simplescenography;
            currentDungeon = dungeon;

            if (id == -1)
            {
                this.id = Globals.AssignAnId();
            }
            else
            {
                this.id = Globals.AssignAnId(id);
            }

            boundingBox = new OBB(position, rotation, dimensions * 0.5f);
            axis = new VAxis(0, height);

            graphicOccupance = new Rectangle((int)(position.X - 32), (int)(position.Y - 32), 64, 64);
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
            this.cData.Add(Data);
        }

        public void SetDamageData(DamageData Data)
        {
            this.dData.Add(Data);
        }

        public void Update(GameTime gameTime)
        {
            //sostanzialmente inutile, aggiungere qui nel caso di interazioni con lo script
            cData.Clear();
            dData.Clear();
        }

        public void Draw(Rectangle camera)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(subtype, 0), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(subtype, 0), scale, SpriteEffects.None, depth);
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            this.boundingBox.Draw(camera, Depths.boxes);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 25);

            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + ";";

            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
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
        /// Ottiene il valore che verifica se l' entità sta eseguendo un azione
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
        /// Ottiene il danno descritto nel damagedata
        /// </summary>
        public DamageData getDamageDealt
        {
            get { return new DamageData(); }
        }

        /// <summary>
        /// Ottiene la profondità dell' entità
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
        /// Ottiene il sottotipo dell'entità
        /// </summary>
        public int getSubtype
        {
            get { return subtype; }
        }

        /// <summary>
        /// Ottiene il tipo dell' entità
        /// </summary>
        public int getType
        {
            get { return type; }
        }

        /// <summary>
        /// Ottiene lo stato dell' entità
        /// </summary>
        public int State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// Ottiene il boundingBox dell' Entità
        /// </summary>
        public OBB getBoundingBox
        {
            get { return this.boundingBox; }
        }

        /// <summary>
        /// Ottiene l occupazione sullo schermo dell ' entità
        /// </summary>
        public Rectangle getOccupance
        {
            get { return graphicOccupance; }
        }

        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            l = spawned.GetRange(0, spawned.Count);
            spawned.Clear();
            return l;
        }

        /// <summary>
        /// Ottiene il nome dell' entità
        /// </summary>
        public String getName
        {
            get { return this.name; }
        }

        /// <summary>
        /// Ottiene la posizione dell' entità
        /// </summary>
        public Vector2 getPosition
        {
            get { return position; }
        }

        /// <summary>
        /// Ottiene la direzione dell' entità
        /// </summary>
        public Vector2 getDirection
        {
            get { return direction; }
        }
        #endregion
    }
}
