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
    public class Door : IEntity,IDamageble
    {
        #region members

        protected bool alive = true; //determina se l' entità è viva o no
        public bool genericAction; //verifica se si sta effetuando un azione

        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Circle inSightCircle;

        protected Color ent_color; //chromatic filter - actually unused

        //definizione gestore dei danni e parametri danni
        [NonSerialized]protected DamageManager damageManager;
        [NonSerialized]protected DamageData damageData;
        [NonSerialized]protected Damage damage;

        protected Dungeon currentDungeon; //referred dungeon

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità

        protected int animation; //animazione attualmente in corso
        protected int cause_of_death = -1; //causa della morte
        protected int factionId;
        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        //protected int idleTime; //tempo in cui l' entità è stata in idle
        protected int sheetIndex;
        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità
        protected int vh;   //

        [NonSerialized] protected OBB boundingBox;
        [NonSerialized] protected OBB detection;

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

        [NonSerialized] VAxis axis;

        Circle pivotCircle;
        float initialRotationAngle;

        public Door(Vector2 position, float radius, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet,  ref Dungeon dungeon)
        {
            axis = new VAxis(0, 999);   //idealmente ogni porta arriva fino al soffitto
            //paramentri passati dal costruttore
            this.depth = depth;
            this.health = health;
            this.origin = new Vector2(16, 16);
            this.position = position;
            this.rotationAngle = rotation;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.door;
            this.scale = new Vector2(1, 1);
            this.type = (int)entityTypes.door;
            this.factionId = (int)factions.ambient;

            initialRotationAngle = rotationAngle;

            boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(16, 8));

            if (rotationAngle == 0)
            {
                vh = 0; //orizzontale
                pivot = new Vector2(position.X - boundingBox.HalfWidths.X, position.Y);
                pivotCircle = new Circle(pivot, 10);
                offset = new Vector2(-20, -2);
                
            }
            else if (rotationAngle == (float)Math.PI/2)
            {
                vh = 1; // verticale
                pivot = new Vector2(position.X, position.Y - boundingBox.HalfWidths.X);
                pivotCircle = new Circle(pivot, 10);
                offset = new Vector2(2, -20);
            }
            else if (rotationAngle == (float)Math.PI )
            {
                vh = 0; // orizzontale
                pivot = new Vector2(position.X + boundingBox.HalfWidths.X, position.Y);
                pivotCircle = new Circle(pivot, 10);
                offset = new Vector2(+20,2);
            }
            else if (rotationAngle == (float)Math.PI*3/2) 
            {
                vh = 1; // verticale
                pivot = new Vector2(position.X, position.Y + boundingBox.HalfWidths.X);
                pivotCircle = new Circle(pivot, 10);
                offset = new Vector2(-2, 20);
            }

            boundingCircle = new Circle(position, 20);
            
            animation = 0;
            
            currentDungeon = dungeon;
            cData = new List<Collision>();
            damageManager = new DamageManager(health);
            dData = new List<DamageData>();
            detection = new OBB(this.position, this.rotationAngle, new Vector2(16, 20));
            detection.DebugColor = new Color(0, 0.5f, 0.75f, 0.5f);
            ent_color = Color.White;
            //pivot = sheet[sheetIndex].GetRotationCenter(0, 0);    //centro di rotazione dell'OBB ottenuto dallo sprite per comodità
            
            //definizione a 0 per tutti i vettori utilizzati per la definizione di damagedata
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                dam[i] = 0;
                tim[i] = 0;
            }
            for (int i = 0; i < Globals.damage_effects; i++)
            {
                pro[i] = 0;
                eff[i] = 0;
                dur[i] = 0;
            }
            damage = new Damage(dam, tim, pro, eff, dur);
            damageData = new DamageData(position, factionId, -1, damage, detection);
        }

        public void Break()
        {
            state = 2;
            animation = 2;
        }
        public void ChangeStatus(Vector2 cposition)
        {
            if (vh == 0) //se la porta è orizzontale
            {
                if (animation == 0) // se è chiusa
                {
                    if (cposition.Y > position.Y && initialRotationAngle == 0)
                    {
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.Y < position.Y && initialRotationAngle == 0)
                    {
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                    //aggiungere un or che comprenda tra inizial position e +-pigreco  /2
                    else if (cposition.Y > position.Y && initialRotationAngle == (float)Math.PI)
                    {
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.Y < position.Y && initialRotationAngle == (float)Math.PI)
                    {
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    animation = 1;
                }
                else if (animation == 1) //se è aperta
                {
                    rotationAngle = initialRotationAngle;
                    boundingBox.AngleInRadians = initialRotationAngle;
                    boundingBox.Origin = position;
                    animation = 0;
                }
            }
            else if (vh == 1) //se la porta è verticale
            {
                if (animation == 0) // se è chiusa
                {
                    //aggiungere un or che comprenda tra inizial position e +-pigreco  /2
                    if(cposition.X < position.X && initialRotationAngle == (float)Math.PI / 2)
                    {
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.X > position.X && initialRotationAngle == (float)Math.PI / 2)
                    {
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.X < position.X && initialRotationAngle == (float)Math.PI * 3 / 2)
                    {
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                        //ERR
                    else if (cposition.X > position.X && initialRotationAngle == (float)Math.PI * 3/ 2)
                    {
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    animation = 1;
                }
                else if (animation == 1) //se è aperta
                {
                    rotationAngle = initialRotationAngle;
                    boundingBox.AngleInRadians = initialRotationAngle;
                    boundingBox.Origin = position;
                    animation = 0;
                }
            }
        }
        /*public void ChangeStatus(Vector2 cposition)
        {
            if (animation == 1)
            {
                if (vh == 0)
                {
                    //aggiungere un or che comprenda tra inizial position e +-pigreco  /2
                    if (cposition.Y > position.Y && rotationAngle != initialRotationAngle-Math.PI/2)//&& rotationAngle > initialRotationAngle-Math.PI/2 && rotationAngle < initialRotationAngle+Math.PI/2)
                    {
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        rotationAngle -= (float)Math.PI/ 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.Y <= position.Y && rotationAngle != initialRotationAngle+Math.PI/2) //&& rotationAngle == initialRotationAngle+Math.PI/2)//&& rotationAngle > initialRotationAngle-Math.PI/2 && rotationAngle < initialRotationAngle+Math.PI/2)
                    {
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                }
                else if(vh == 1)
                {
                    if (cposition.X > position.X && rotationAngle != initialRotationAngle + Math.PI / 2)//&& rotationAngle > initialRotationAngle-Math.PI/2 && rotationAngle < initialRotationAngle+Math.PI/2)
                    {

                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.X <= position.X && rotationAngle != initialRotationAngle - Math.PI / 2)//&& rotationAngle == initialRotationAngle + Math.PI / 2)//&& rotationAngle > initialRotationAngle-Math.PI/2 && rotationAngle < initialRotationAngle+Math.PI/2)
                    {
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                }
                animation = 0;
            }
            if (animation == 0)
            {
                if (vh == 0)
                {
                    if (cposition.Y > position.Y && rotationAngle != initialRotationAngle - Math.PI / 2)
                    {
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.Y < position.Y && rotationAngle != initialRotationAngle - Math.PI / 2)
                    {
                        rotationAngle -= (float)Math.PI/2 ;
                        boundingBox.AngleInRadians -= (float)Math.PI/ 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                }
                else if (vh == 1)
                {
                    if (cposition.X > position.X && rotationAngle != initialRotationAngle - Math.PI / 2)
                    {
                        boundingBox.AngleInRadians -= (float)Math.PI / 2;
                        rotationAngle -= (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(-(float)Math.PI / 2)) + pivot;
                    }
                    else if (cposition.X <= position.X && rotationAngle != initialRotationAngle + Math.PI / 2)
                    {
                        rotationAngle += (float)Math.PI / 2;
                        boundingBox.AngleInRadians += (float)Math.PI / 2;
                        boundingBox.Origin = Vector2.Transform(boundingBox.Origin - pivot, Matrix.CreateRotationZ(+(float)Math.PI / 2)) + pivot;
                    }
                }
                animation = 1;
            }
        }*/

        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            l = spawned.GetRange(0, spawned.Count);
            return spawned;
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

            #region controlli di stato
            if (this.alive == false)
            {
                this.depth = 0.9f;
            }
            if (health <= 0)
            {
                this.Break();
            }
            #endregion

            damageManager.Update(gameTime);

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //evita di ricolcolare due volte la stessa collisione
                    {
                        if (c.generic && detection.Intersects(c.boundingBox)) //verifica se l entità collidente sta effetuando un azione
                        {
                            float v = Vector2.Distance(position, c.position); 
                            if (Vector2.Distance(position, c.position + c.direction) < v) //verifica che l entità che sta efffettuando l azione sia abbastanza vicina da poter interagire
                            {
                                this.ChangeStatus(c.position); //apre o chiude la porta
                            }
                        }
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

            //sistemare graphicOccupance

            if (damageManager.health < 0) // se gli hp dell entità vanno sotto 0
            {
                this.Break(); // la porta si rompe
            }
            //frame = 0;
            cData.Clear(); // ripulisce il vettore di collisioni
        }


        public void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos + offset, sheet[sheetIndex].Frame(animation, frame), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(animation, frame), scale, SpriteEffects.None, Depths.doors );
        }

        public void DrawCollidedObjectDebug( Rectangle camera)
        {
            boundingBox.Draw(camera, 1.0f);
            pivotCircle.Draw(camera);
            detection.Draw(camera, 1.0f);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top + 15);

            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: -";
            //boundingCircle.Draw(Globals.spriteBatch, camera, box);
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);

        }

        #region properties

        public Color Color
        {
            get { return ent_color; }
            set { ent_color = value; }
        }


        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
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
            get { return this.genericAction; }
        }

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
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
            get { return -1; } //ci servirà il tipo di porta?
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
            get { return state;  }
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

        public Rectangle graphicOccupance { get; set; }
    }
}
