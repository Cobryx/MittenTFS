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
    [Serializable]
    class Table : IEntity,IBypass,IDamageble,IAttacker,IMultiPart
    {
        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool updatable=true;

        //definizione cerchi collidenti
        protected Circle boundingCircle;

        protected Color ent_color; //filtro entità

        //definizione gestore dei danni e parametri danni
        [NonSerialized]protected DamageManager damageManager;
        [NonSerialized]protected DamageData damageData;
        [NonSerialized]protected Damage[] damage;

        protected Dungeon currentDungeon; //referred dungeon

        
        protected EntityManager status;

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float maxHealth; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed; //velocità dell 'entità

        protected int cause_of_death = -1; //causa della morte
        protected int id; //id unico dell entità
        //protected int idleTime; //tempo in cui l' entità è stata in idle
        protected int selectedSkill = (int)skills.none;
        protected int sheetIndex;
        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità

        [NonSerialized]protected OBB barrierBox; //definisce l'obb di schermatura del tavolo
        [NonSerialized]protected OBB boundingBox;
        [NonSerialized]protected OBB leftBox;
        [NonSerialized]protected OBB rightBox;
        protected Rectangle graphicOccupance;

        [NonSerialized]protected SpriteSheet[] sheet; //riferiemento allo spritesheet
        
        private String[] debugString = new String[1];
        private String[] logString = new String[28];
        protected String name; 
        
        protected Vector2 direction; //direzione entità
        protected Vector2 oldPosition; //position to restore in case of collision
        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 scale; //scala dimensionamento entità

        protected int[] effects;
        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        protected List<Collision> cData = new List<Collision>() ; //lista delle collisioni
        protected List<DamageData> dData = new List<DamageData>(); // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell entità
        protected List<int> dIds = new List<int>(); //klista degli id unici dei danni
        protected List<int> ids = new List<int>(); //registra gli id di collisione, lista da eliminare
        protected List<Item> inventory = new List<Item>();
        private int factionId;

        [NonSerialized] VAxis axis;

        private List<Waypoint> alternativePath;
        private bool jumpable = true;
        private bool goDown = true;
        private bool breakable = true;

        private SpriteEffects spreffects = SpriteEffects.None;

        List<OBB> boundingBoxes;
        public int ChildrenNumber { get; private set; }
        List<SubEntity> children;

        #endregion

        public Table(Vector2 position, float radius, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
        {
            //da vericare se al moemnti dell istnziazione ci sono wallcontacts
            alternativePath = new List<Waypoint>();
            Circle c = new Circle(new Vector2((float)(position.X + 20 + 16),(float)( position.Y + 26 + 16)),16);
            Waypoint w = new Waypoint(c);
            alternativePath.Add(w);
            c = new Circle(new Vector2((float)(position.X + 20 + 16), (float)(position.Y - 26 - 16)), 16);
            w = new Waypoint(c);
            alternativePath.Add(w);
            c = new Circle(new Vector2((float)(position.X - 20 - 16), (float)(position.Y + 26 + 16)), 16);
            w = new Waypoint(c);
            alternativePath.Add(w);
            c = new Circle(new Vector2((float)(position.X - 20 - 16), (float)(position.Y - 26 - 16)), 16);
            w = new Waypoint(c);
            alternativePath.Add(w);


            axis = new VAxis(0, 50);
            boundingCircle = new Circle(position, 100);
            boundingBoxes = new List<OBB>();
            boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(30, 48));
            boundingBoxes.Add(boundingBox);
            OBBindex = 0;
            OBBnumber = 1;

            barrierBox = new OBB(this.position + new Vector2(15,1), this.rotationAngle, new Vector2(5,50));
            boundingBox.DebugColor = new Color(0, 0, 255, 128);
            barrierBox.DebugColor = new Color(128, 0, 255, 128);
            currentDungeon = dungeon;
            this.position = position;
            this.oldPosition = position;
            this.origin = new Vector2(0, 0);
            maxHealth = health;
            this.health = float.PositiveInfinity; //debugw
            this.depth = depth;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.table;
            this.scale = new Vector2(1, 1);
            this.type = (int)entityTypes.table;
            this.rotationAngle = rotation;
            this.ent_color = Color.White;
            graphicOccupance = new Rectangle((int)position.X-32, (int)position.Y-32, 64, 64);
            this.factionId = (int)factions.ambient;

            leftBox = new OBB(new Vector2(position.X - boundingBox.HalfWidths.X, position.Y ), rotationAngle, new Vector2(2, 30));
            rightBox = new OBB (new Vector2(position.X + boundingBox.HalfWidths.X,position.Y),rotationAngle,new Vector2(2,30));
            leftBox.DebugColor = Color.Violet;
            rightBox.DebugColor = Color.Green;
            leftBox.DebugColor.A = 128;
            rightBox.DebugColor.A = 128;
            leftBox.Origin= Vector2.Transform(leftBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            rightBox.Origin = Vector2.Transform(rightBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;

            damageManager = new DamageManager(health);
            status = new EntityManager(Globals.tb_states, Globals.tb_animations, ref this.sheet[sheetIndex]);
            status.SetOn((int)h_states.idle, (int)h_states.idle,true,true);
            damageData = new DamageData(Vector2.Zero);
        }

        public void getCollisionData(Collision Data)
        {
            this.cData.Add(Data);
        }
        
        #region actions

        public void Break(int cause)
        {
            if (status.IsOff((int)tb_states.broken)&& alive)
            {
                this.alive = false;
                //this.depth *= 0.1f;
                cause_of_death = cause;
                status.SetOn((int)tb_states.broken, sheet[sheetIndex].GetTotalDuration((int)tb_animations.broken), (int)tb_animations.broken, false, true, null, true);

                //status.SetOn((int)tb_states.broken, (int)tb_animations.broken, false, true,true);
            }
        }

        public void Flip()
        {
            if(status.IsOff((int)tb_states.flipping))
            {
                status.SetOn((int)tb_states.flipping, sheet[sheetIndex].GetTotalDuration((int)tb_animations.flipping), (int)tb_animations.flipping, false, true);
                barrierBox.AngleInRadians = rotationAngle;
                
            }
            if (status.Finished((int)tb_states.flipping))
            {
                SubEntity a = new SubEntity(this, depth, null, new OBB(position + new Vector2(10, 10), rotationAngle + MathHelper.PiOver2, new Vector2(50, 10)), position + new Vector2(10, 10), direction, ref currentDungeon);
                children.Add(a);
                spawned.Add(a);
                SubEntity b = new SubEntity(this, depth, null, new OBB(position-new Vector2(10, 10), rotationAngle+MathHelper.PiOver2, new Vector2(50, 10)), position+new Vector2(10, 10), direction, ref currentDungeon);
                children.Add(b);
                spawned.Add(b);
            }
        }

        /*public void Idle()
        {
            if (!status.IsLocked((int)tb_states.idle))
            {
                status.SetOn((int)tb_states.idle);
                              
            }
        }*/

        #endregion

        public void Update(GameTime gameTime)
        {
            direction.X = (float)Math.Cos(rotationAngle);
            direction.Y = (float)Math.Sin(rotationAngle);
            direction *= speed;
            if (boundingBox != null)
            {
                boundingBox.Origin = this.position;
                boundingBox.UpdateAxis(rotationAngle);
            }
            barrierBox.UpdateAxis(rotationAngle); 

            
            damageManager.Update(gameTime);
            status.Update(gameTime);
            status.AutoOff();

            #region controlli di stato

            if (status.IsOn((int)tb_states.idle))
            {
                speed = 0;
                currentDungeon.FreeDamageId(damageData.id);
            }
            
            if(status.IsOn((int)tb_states.flipping))
            {
                if (status.GetCurrentFrame() == 3)
                {
                    axis.Height = 64;
                    axis.Floor();
                }
            }
            if (status.Finished((int)tb_states.flipping))
            {
                status.SetOn((int)tb_states.flipped, (int)tb_animations.flipped100, true, true, true);
            }

            if (status.IsOn((int)tb_states.flipped))
            {
                if (damageManager.health / maxHealth > 0.6f && damageManager.health / maxHealth <= 0.8f)
                {
                    status.SetOn((int)tb_states.flipped, (int)tb_animations.flipped80, true, true, false);
                }
                else if (damageManager.health / maxHealth > 0.4f && damageManager.health / maxHealth <= 0.6f)
                {
                    status.SetOn((int)tb_states.flipped, (int)tb_animations.flipped60, true, true, false);
                }
                else if (damageManager.health / maxHealth > 0.2f && damageManager.health / maxHealth <= 0.4f)
                {
                    status.SetOn((int)tb_states.flipped, (int)tb_animations.flipped40, true, true, false);
                }
                else if (damageManager.health / maxHealth > 0f && damageManager.health / maxHealth <= 0.2f)
                {
                    status.SetOn((int)tb_states.flipped, (int)tb_animations.flipped20, true, true, false);
                }
                else if (damageManager.health / maxHealth <= 0.0f)
                {
                    Break(0);
                }

                if (status.IsOn((int)tb_states.broken))
                {
                    if (status.GetCurrentFrame() == 2)
                        boundingBox = null;
                }

                if (status.Finished((int)tb_states.broken))
                {
                    status.SetOn((int)tb_states.breaked, (int)tb_animations.breaked, false, true, true);
                    depth = Depths.corpse;
                    axis.Height = 9;
                    axis.Floor();
                }
           
                if (rotationAngle == 0 && leftBox != null)
                    barrierBox.Origin = rightBox.Origin;
                else if (rotationAngle==(float) Math.PI/2 && leftBox != null)
                    barrierBox.Origin=leftBox.Origin;
                else if (rotationAngle == (float)Math.PI && leftBox != null)
                    barrierBox.Origin = leftBox.Origin;
                else if (rotationAngle == (float)Math.PI * 3/2 && leftBox!=null)
                    barrierBox.Origin = rightBox.Origin;
                
                leftBox = null;
                rightBox = null;
            }

            #endregion

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!ids.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        float d = Vector2.Subtract(position + direction, c.position).Length();
                        if (d < c.distance)
                            direction *= 0;
                        ids.Add(c.id);
                    }
                    //effects = damageManager.CalculateDamage(c.damage);//, id);
                    //AGGIUNGERE ALTRE REAZIONI PREVISTE
                    if (effects != null && effects[(int)damageEffects.mechanical] > 0)
                    {
                        //Flip();
                    }
                }
            }

            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id))
                {
                    dIds.Add(d.id);
                    damageManager.CalculateDamage(d.damage);
                    if (damageManager.Effects((int)damageEffects.mechanical) > 0)
                    {
                        if (OBB.Intersects(d.oArea, leftBox))
                        {
                            if (status.IsOn((int)tb_states.idle))
                            {
                                Flip();
                            }
                        }
                        else if (OBB.Intersects(d.oArea, rightBox))
                        {
                            if (status.IsOn((int)tb_states.idle))
                            {
                                rotationAngle += (float)Math.PI;
                                spreffects = SpriteEffects.FlipVertically;
                                Flip();
                            }
                        }
                    }
                }
            }

            dIds.Clear();
            dData.Clear();

            if (!currentDungeon.WallContact(position + direction))
            {
                position += direction;
                barrierBox.Origin += direction;
                graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
                graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;
                
            }

            oldPosition = position;     //salvataggio della vecchia posizione
            direction *= 0;
            cData.Clear();
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

        public void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, spreffects, depth);
        }

        public void DrawCollidedObjectDebug( Rectangle camera)
        {
            boundingCircle.Draw(camera);
            boundingBox.Draw(camera, 0f);
            if (leftBox != null)
            {
                leftBox.Draw(camera,  0f);
                rightBox.Draw(camera, 0f);
            }
            barrierBox.Draw(camera,  0.35f);
            foreach (Waypoint w in alternativePath)
                w.c.Draw(camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top + 15);
            String s;
            s = "HP: " + damageManager.health + "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1.5f, SpriteEffects.None, 0f);
        }

        #region properties
        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }

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

        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
        }

        /// <summary>
        /// Ottiene o imposta l'id di appartenenza di un'entità a una "fazione"
        /// </summary>
        public int Faction
        {
            get { return factionId; }
            set { factionId = value; }
        }


        public bool generic
        {
            get { return false; }
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
            get { return this.boundingBoxes[OBBindex]; }
        }

        public Circle getBoundingCircle
        {
            get { return this.boundingCircle; }
        }

        public DamageData getDamageDealt
        {
            get { return this.damageData; }
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
            get { return this.id; }
        }

        public String getName
        {
            get { return this.name; }
        }

        public Rectangle getOccupance
        {
            get { return graphicOccupance; }
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
            get { return -1; }
        }

        public int getType
        {
            get { return type; }
        }

        public int State
        {
            get { return state; }
            set { state = value; }
        }

        public bool Updatable
        {
            get { return updatable; }
        }

        public List<Waypoint> Alternative
        { get { return alternativePath; } }
        public bool Jumpable
        { get { return jumpable;} }
        public bool GoDown
        { get { return goDown;} }
        public bool Breakable
        { get { return breakable;} }
        public int OBBindex { get; set; }
        public int OBBnumber { get; private set; }
        public List<SubEntity> getChildren { get { return children; } }

        #endregion
    }
}
