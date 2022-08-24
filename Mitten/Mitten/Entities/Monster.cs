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
    public abstract class Monster : Shiftable, IEntity,IDamageble,IAttacker,IShadow
    {
        

        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool updatable = true;
        protected bool fl_attack = false; // indica se l' attacco è attivo
        protected bool active = false;
        protected bool corpse = false;

        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Circle[] damageCircles;
        protected Circle meleeCircle;
        protected Circle rangeCircle;
        protected Circle specialCircle;
        protected Circle seekingCircle;
        protected Circle inSightCircle;

        protected Color ent_color; //filtro entità
        protected Color weapon_color; //filtro arma

        //definizione gestore dei danni e parametri danni
        protected DamageManager damageManager;
        protected DamageData damageData;
        protected Damage damage;

        protected Dungeon currentDungeon; //referred dungeon

        //protected EntityAnimationManager animation; //animazione attualmente in corso
        //protected EntityStateManager status;
        protected EntityManager status;

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed; //velocità dell 'entità

        protected IEntity target; //obiettivo passato dall' IAManager

        protected float[] droppable;

        protected int cause_of_death = -1; //causa della morte
        protected int frame; //frame attualmente in corso
        protected int hand = (int)handleables.nothing;
        protected int id; //id unico dell entità
        public int genericAction; //verifica se si sta effetuando un azione
        //protected int idleTime; //tempo in cui l' entità è stata in idle
        protected int selectedSkill = (int)skills.none;
        protected int sheetIndex;

        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità

        protected OBB boundingBox;
        protected OBB[] boxList;
        protected Rectangle graphicOccupance;

        protected SpriteSheet[] handSheet; //referimento allo spritesheet delle armi
        protected SpriteSheet[] sheet; //riferiemento allo spritesheet

        private String[] debugString = new String[1];
        private String[] logString = new String[28];
        protected String name; //nome entità

        protected Vector2 direction; //direzione entità
        protected Vector2 oldPosition; //position to restore in case of collision
        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 pivot; // perno di rotazione entità
        protected Vector2 scale; //scala dimensionamento entità

        protected Vector2[] boxDistance;
        protected int[] effects;
        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        protected List<Collision> cData = new List<Collision>(); //lista delle collisioni
        protected List<DamageData> dData = new List<DamageData>(); // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell entità
        protected List<int> dIds = new List<int>(); //klista degli id unici dei danni
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

        protected List<Circle> pathToFollow;
        protected int w = 0;
        protected int factionId;
        protected VAxis axis;

        public Monster(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon, int factionId=(int)factions.foes)
        {

            droppable = new float[Globals.nItems];
            droppable.Initialize();

            
      

            this.position = position;
            this.oldPosition = position;
            this.direction = direction;
            this.speed = speed;
            this.origin = new Vector2(0, 0);
            this.depth = depth;
            this.sheet = sheet;
            this.scale = new Vector2(1, 1);
            this.damageManager = new DamageManager(health);
            this.type = type;
            this.damageCircles = new Circle[3];
            this.desiredAngle = 4f;
            this.id = Globals.AssignAnId();
            this.factionId = factionId;
            depth += (id / 32768f);  //profondità unica per l'algoritmo del pittore
            randMilliSec = new Random(id);
            ent_color = Color.White;
            weapon_color = Color.White;
            this.rotationAngle = rotation;
  
            currentDungeon = dungeon;

            cData = new List<Collision>();

        }


        public void graphicsOccupanceDraw()
        {
            using (System.Drawing.Bitmap debugMap = new System.Drawing.Bitmap("debugmap.bmp"))//new System.Drawing.Bitmap(currentDungeon.width * 32 + 1000, currentDungeon.height * 32 + 1000))
            {
                if (currentDungeon.getFloor == 2)
                {
                    for (int i = graphicOccupance.X; i < graphicOccupance.X + graphicOccupance.Width; i++)
                    {
                        for (int j = graphicOccupance.Y; j < graphicOccupance.Y + graphicOccupance.Height; j++)
                        {
                            debugMap.SetPixel(i, j, System.Drawing.Color.Yellow);
                        }
                    }
                    debugMap.SetPixel((int)position.X, (int)position.Y, System.Drawing.Color.Turquoise);
                    debugMap.Save("debugmap5.bmp");
                }
            }
        }

        public void AssignPath(List<Circle> path)
        {
            pathToFollow = path;
            w = 0;
        }
        
        /*public void AssignPath(Circle path)
        {
            pointToFollow = path;
            w = 0;
        }*/


        public void Drop()
        {
            Random r = new Random();
            for (int i = 0; i < droppable.Count();i++ )
            {
                if (r.NextDouble() < droppable[i])
                    spawned.Add(new Item(dic.itemIndex[i],position, direction, depth, rotationAngle, ref sheet, ref currentDungeon, true));
                
            }
        }

        public void SetCollisionData(Collision Data)
        {
            this.cData.Add(Data);
        }

        public void SetDamageData(DamageData Data)
        {
            
            this.dData.Add(Data);
        }

        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            l = spawned.GetRange(0, spawned.Count);
            spawned.Clear();
            return l;
        }

        public bool Is_in_camera(Rectangle camera)
        {
            if (camera.Contains(graphicOccupance) || camera.Intersects(graphicOccupance))
                return true;
            else
                return false;
        }

        public void LookAt(IEntity target)
        {
            this.target = target;
            if (target != null)
            {
                checkAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1);
                cCheckBox.Origin = position + (target.getPosition - position) / 2; ;
                cCheckBox.HalfWidths = new Vector2(((target.getPosition - position) / 2).Length(), cCheckBox.HalfWidths.Y);
            }
        }

        public void PickUp(Item item)
        {
            if (!item.alreadyPicked)
            {
                item.alreadyPicked = true;
                inventory.Add(item);
            }
        }


        public virtual void Update(GameTime gameTime)
        {
            
            //drop oggetti ed eliminazione ombre 
            if (status.Finished(0))
            {
                Drop();
                if (this is IShadow)
                {
                    shadow.Visible = false;
                    Globals.krypton.Hulls.Remove(shadow);
                    shadow = null;
                }
            }
            
             
            //impostare qui il damage manager e aggiugnere la rimazione delle ombre del motore krypton
            if (damageManager.health <= 0)
            {
            }
            if (target != null)
            {
                cCheckBox.UpdateAxis(checkAngle);
            }

            if (sensorBox != null)
            {
               // if (pathToFollow != null)
                {
                    sensorBox.UpdateAxis(desiredAngle);
                    sensorBox.Origin = position + new Vector2((float)Math.Cos(desiredAngle), (float)Math.Sin(desiredAngle))*58;
                    //sensorBox.HalfWidths = new Vector2(30, sensorBox.HalfWidths.Y);//allungare eventualmente il sensorBox
                }
                /*else
                {
                    sensorBox.UpdateAxis(rotationAngle);
                    sensorBox.Origin = position + new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle)) * 58;
                    //sensorBox.HalfWidths = new Vector2(30, sensorBox.HalfWidths.Y);//allungare eventualmente il sensorBox
                }*/
            }
            boundingCircle.Center = this.position;

            if (shadow != null)
            {
                shadow.Angle = -rotationAngle;
                shadow.Position = new Vector2(position.X - Globals.camera[0].Left, -(position.Y - Globals.camera[0].Top));
                shadow.Axis = axis;
            }
        }


        public virtual void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[this.sheetIndex].sourceBitmap, pos, sheet[this.sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[this.sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + (float)id / Globals.max_entities);
        }

        public virtual void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
            boxList[0].Draw(camera, Depths.boxes);
            cCheckBox.Draw(camera, Depths.boxes);
         //   if (sensorBox != null) sensorBox.Draw(camera, Depths.boxes);
            if (Globals.extremeDMode)
            {
                boundingCircle.Draw( camera);
                meleeCircle.Draw(camera);
                rangeCircle.Draw(camera);
                specialCircle.Draw(camera);
                seekingCircle.Draw(camera);
                inSightCircle.Draw(camera);
            }
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
        { get { return shadow; } set { shadow = value; } }

        /// <summary>
        /// Ottiene il valore che verifica se l' entità sta eseguendo un azione
        /// </summary>
        public bool generic
        {
            get { if (genericAction > 0) return true; else return false; }
        }

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
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

        public Circle getSeekingCircle
        {
            get { return this.seekingCircle; }
        }

        public Circle getInSightCircle
        {
            get { return this.inSightCircle; }
        }
     
        /// <summary>
        /// Ottiene il danno descritto nel damagedata
        /// </summary>
        public DamageData getDamageDealt
        {
            get { return damageData; }
        }

        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
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

        public int[] Effects
        {
            get { return effects; }
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
