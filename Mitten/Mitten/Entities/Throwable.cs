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
    public abstract class Throwable : IEntity,IAttacker
    {
        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool lookingAt = false;
        protected bool updatable = false;
        protected Color ent_color = Color.White; //filtro entità

        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Circle[] damageCircles;
        protected Circle meleeCircle;
        protected Circle rangeCircle;
        protected Circle specialCircle;
        protected Circle seekingCircle;
        protected Circle inSightCircle;

        protected DamageManager damageManager;
        protected DamageData damageData;
        protected Damage damage;

        protected Dungeon currentDungeon;

        protected EntityManager status;

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed; //velocità dell 'entità

        protected IEntity target; //obiettivo passato dall' IAManager

        protected int currentAnimation; //animazione corrente
        protected int cause_of_death = -1; //causa della morte
        protected int factionId;
        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        public int genericAction; //verifica se si sta effetuando un azione
        protected int sheetIndex;
        protected int sprite;
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
        protected List<int> dIds = new List<int>(); //lista degli id unici dei danni
        
        protected float desiredAngle;
        protected Vector2[] targetPosition;
                
        protected Random randMilliSec;

        protected Vector2 targetToLook = new Vector2();
        protected List<Circle> pathToFollow;
        protected int w = 0;//pathProgression = 0;
        protected VAxis axis;
        #endregion


        public Throwable(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int factionId, int type, int subtype,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
        {
            boundingBox = new OBB(position, rotation, new Vector2(3, 3));
            this.position = position;
            this.oldPosition = position;
            //this.direction = direction;
            //this.speed = speed;
            this.origin = new Vector2(0, 0);
            this.factionId = factionId;
            this.id = Globals.AssignAnId();
            this.depth = depth + id / Globals.max_entities;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.item;
            //this.scale = new Vector2(1.3f, 1.3f);
            this.damageManager = new DamageManager(health);
            this.subtype = subtype;
            this.type = type;
            this.desiredAngle = 0f;
            this.currentDungeon = dungeon;
            this.rotationAngle = rotation;
            

            boundingCircle = new Circle(position, radius);
            seekingCircle= new Circle (this.position,100);
            inSightCircle = new Circle(this.position, 100);
            
        }

   
        public void AssignPath(List<Circle> path)
        {
            pathToFollow = path;
            w = 0;
        }

        public void Chase(IEntity target)
        {
            if (target != null)
            {
                this.target = target;
                targetToLook = target.getPosition;
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

        public void LookAt(Vector2 target)
        {
            lookingAt = true;
            targetToLook = target;
        }

        public void LookAtTarget()
        {
            lookingAt = true;
            targetToLook = target.getPosition;
        }

        public void PickUp(Item item)
        {
            if (!item.alreadyPicked)
            {
                item.alreadyPicked = true;
               // inventory.Add(item);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            
            damageData.dealerPosition = position;
            damageData.oArea = boundingBox;
            damageData.cArea = boundingCircle;
            
            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        if (c.collided)
                        {
                            switch (c.type)
                            {
                                case (int)entityTypes.item: break;
                                case (int)entityTypes.throwable: break;
                                case (int)entityTypes.dead: break;
                                case (int)entityTypes.explosion: break;
                                //case (int)entityTypes.banshee: speed *= 0.75f; break;
                                default: if(c.factionId!=factionId) Updatable = false; break;
                            }
                        }
                    }
                }
                cIds.Clear();
            }

            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id))
                {
                    dIds.Add(d.id);
                    damageManager.CalculateDamage(d.damage);

                    //AGGIUNGERE ALTRE REAZIONI PREVISTE
                    if (damageManager.Effects((int)damageEffects.blind) > 0)
                    {
                        //aggiungere cecità
                    }
                    if (damageManager.Effects((int)damageEffects.buff) > 0)
                    {
                        //aggiungere dettagli buff
                    }
                    if (damageManager.Effects((int)damageEffects.debuff) > 0)
                    {
                    }
                    if (damageManager.Effects((int)damageEffects.freeze) > 0)
                    {
                    }
                }
            }
            dIds.Clear();
            
            /*
            direction = currentDungeon.WallContact(boundingCircle, direction);
            position += direction;
            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;
            boundingBox.Origin += direction;

            int k = 0;
            for (int i = 0; i < Globals.th_states; i++)
            {
                if (status.IsOff(i))
                {
                    k++;
                }
            }
            if (k == Globals.th_states)
            {
            //    this.Idle();
            }

            if (damageManager.health <= 0)
            {
                //this.Die((int)deathCauses.generic);
            }
             * */

            cData.Clear();
            dData.Clear();

        }

        public virtual void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[(int)sheetIndexes.item].sourceBitmap, pos, sheet[(int)sheetIndexes.item].Frame(sprite, 0), ent_color, rotationAngle, sheet[(int)sheetIndexes.item].GetRotationCenter(sprite, 0), scale, SpriteEffects.None, depth);
            if (sprite == 90)
            { }
            //immagine sorgente, posizione su schermo, rettangolo del frame desiderato, colore, angolo di rotazione, centro di rotazione, fattore di scala, effetti, profondità
        }

        public virtual void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
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
            get { if (genericAction > 0) return true; else return false; }
        }

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l' entità verrà rimossa della lista dlle entità
        /// </summary>
        public bool Updatable
        {
            get { return alive; }
            set { alive = value; }
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
