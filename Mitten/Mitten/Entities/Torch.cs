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
using Krypton.Common;



namespace Mitten
{
    public class Torch : Krypton.Lights.Light2D,IEntity
    {
        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        public bool genericAction; //verifica se si sta effetuando un azione



        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Circle inSightCircle;

        protected Color ent_color; //chromatic filter - actually unused

        //definizione gestore dei danni e parametri danni
        protected DamageManager damageManager;
        protected DamageData damageData;
        protected Damage damage;

        protected Dungeon currentDungeon; //referred dungeon

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità

        protected int cause_of_death = -1; //causa della morte
        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        //protected int idleTime; //tempo in cui l' entità è stata in idle
        protected int sheetIndex;
        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità
        protected int vh;   

        protected Rectangle graphics_occupance;

        protected SpriteSheet[] sheet; //riferiemento allo spritesheet
        protected String name; //nome entità
        
        protected Vector2 direction; //direzione entità

        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 offset;
        protected Vector2 pivot; // perno di rotazione entità
        protected Vector2 scale; //scala dimensionamento entità

        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        protected List<Collision> cData; //lista delle collisioni
        protected List<DamageData> dData; // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell entità
        protected List<int> dIds = new List<int>(); //klista degli id unici dei danni

        //protected int[] damageType;
        #endregion
        float radius;
        int factionId;
        int k; //nukero di vertici
        int subtype;
        
        float initialRotationAngle;
        EntityManager status;
        Color c1;
        Color c2;
        int colorChange;
          Random r;
        Vector2 originalPos;

        public Torch(Vector2 position, float radius, float depth, float health, float rotation, int type, ref SpriteSheet[] sheet, ref Dungeon dungeon, float intensity, float lightRadius)
        {
            
            axis = new VAxis(128, 160);
            //paramentri passati dal costruttore
            this.radius = radius;
            this.depth = depth;
            this.health = health;
            this.origin = new Vector2(16, 16);
            this.position = position;
            this.originalPos = position;
            this.rotationAngle = rotation;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.torch;
            status = new EntityManager(1,1, ref sheet[sheetIndex]);
            this.scale = new Vector2(1, 1);
            id = Globals.AssignAnId();
            factionId = (int)factions.ambient;
           
            this.type = type;
            r = new Random(id);
            initialRotationAngle = rotationAngle;

            //boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(16, 8));
            //boundingBox.DebugColor = Color.Red;

            boundingCircle = new Circle(position, radius);

            colorChange = 20;

            base.Position = position;
            base.Texture = Globals.mLightTexture;
            base.Color = new Color(255, r.Next(100, 153), 0);
            base.Range = 350f;
            base.Intensity = 0.65f; //valore consigliato 0.65f
            base.Angle = 0;
            base.Fov = MathHelper.TwoPi;
            base.IsOn = true;

            c1 = new Color(255, 0, 0);
            c2 = new Color(255, 255, 0);
            
            Globals.krypton.Lights.Add(this);

            currentDungeon = dungeon;
            cData = new List<Collision>();
            damageManager = new DamageManager(health);
            dData = new List<DamageData>();
            
            ent_color = Color.White;
            graphics_occupance.X = (int)position.X;     //aggiornamento del bounding box (per usi futuri)
            graphics_occupance.Y = (int)position.Y;
            status.SetOn(0, 0, true, true);
        }

      
        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            l = spawned.GetRange(0, spawned.Count);
            return spawned;
        }

        public bool Is_in_camera(Rectangle camera)
        {
            if (camera.Contains(graphics_occupance) || camera.Intersects(graphics_occupance))
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
            Vector2 pos = new Vector2(position.X - Globals.camera[0].Left, position.Y - Globals.camera[0].Top);
            base.Position = pos;
            
            colorChange--;
            if (colorChange%7 == 0)
            {
                base.Color = new Color(255, r.Next(120,150), 0);
                base.Intensity = 0.60f + r.Next(2, 7) * 0.012f; //* colorChange;    //precedente valore 0.018f
                colorChange = 0;
            }

           // base.Intensity = 3;
            //base.Color = Color.White;

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //evita di ricolcolare due volte la stessa collisione
                    {
                        
                        cIds.Add(c.id);     //aggiunge un id di collisione
                    }
                }
                cIds.Clear(); //ripulisce gli id di collisione
            }

            foreach (DamageData d in dData.Where(d => d.id != -1)) //per ogni dato presente nella lista dei danni
            {
                dIds.Add(d.id);
                damageManager.CalculateDamage(d.damage); //il gestore dei danni calcola il danno e lo sotrae agli hp
            }

            graphics_occupance.X = (int)position.X;     //aggiornamento del bounding box (per usi futuri)
            graphics_occupance.Y = (int)position.Y;

            status.Update(gameTime);
            status.AutoOff();
            
            dData.Clear();
            cData.Clear();
            
        }

        public virtual void Draw(Rectangle camera)
        {

            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);

            Globals.spriteBatch.Draw(sheet[this.sheetIndex].sourceBitmap, pos, sheet[this.sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[this.sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
           // boundingCircle.Draw(Globals.spriteBatch, camera);
        }


        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top + 15);
     
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: -";
            //boundingCircle.Draw(Globals.spriteBatch, camera, box);
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 10f);
        }

        #region properties

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
        /// Ottiene il valore che verifica se l' entità sta eseguendo un azione
        /// </summary>
        public bool generic
        {
            get { return this.genericAction; }
        }

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l' entità verrà rimossa della lista dlle entità
        /// </summary>
        public bool Updatable
        {
            get { return true; }
            set { Updatable = value; }
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
            get { return damageData; }
        }

        /// <summary>
        /// Ottiene la profondità dell' entità
        /// </summary>
        public float getDepth
        {
            get { return depth; }
        }

        public int Faction
        {
            get { return factionId; }
            set { factionId = value; }
        }

        /// <summary>
        /// Ottiene l' id unico dell'entità
        /// </summary>
        public int getId
        {
            get { return this.id; }
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

        public int getSubtype
        {
            get { return -1; }
        }

        /// <summary>
        /// Ottiene il boundingBox dell' Entità
        /// </summary>
        public OBB getBoundingBox
        {
            get { return null; }
        }

        /// <summary>
        /// Ottiene l occupazione sullo schermo dell ' entità
        /// </summary>
        public Rectangle getOccupance
        {
            get { return graphics_occupance; }
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
