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
    public class Explosion : IEntity,IAttacker
    {
        #region members

        protected bool alive = true; //determina se l' entità è viva o no
        protected bool updatable = true;
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
        protected float expansionSpeed; // velocità di espansione dell' esplosione
        protected float collapseSpeed; // velocità di collasso dell' esplosione

        protected int cause_of_death = -1; //causa della morte
        protected int currentstatus; //animazione corrente
        protected int frame; //frame attualmente in corso
        protected int id; //id unico dell entità
        protected int exp_type;
        protected int sheetIndex;
        protected int state; //stato dell entità attualmente in corso
        protected int type; //tipo di entità

        protected OBB boundingBox;
        protected OBB boxList;
        protected Rectangle graphicOccupance;
        
        protected SpriteSheet[] sheet; //riferiemento allo spritesheet
        protected String name; //nome entità
        
        protected Vector2 direction; //direzione entità

        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
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

        EntityManager status;

        VAxis axis;
        #endregion

        public Explosion(Vector2 position, float radius, float depth, float violence, int exp_type, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon, int factionId=(int)factions.explosions)
        {
            
            this.boundingBox = new OBB(position, 0f, new Vector2(radius));
            this.boundingBox.DebugColor = new Color(255, 0, 255, 128);
            this.boundingCircle = new Circle(position, radius);
            this.depth = depth;
            this.exp_type = exp_type;
            this.factionId = factionId;
            this.id = Globals.AssignAnId();
            this.origin = new Vector2(0, 0);
            this.position = position;
            this.rotationAngle = rotation;
            this.scale = new Vector2(1, 1);
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.explosion;
            expansionSpeed = 0.2f;
            collapseSpeed = 0.2f;
            this.type = type;
            //da contemplare l'ipotesi che le esplosioni siano molto più grandi
            graphicOccupance = new Rectangle((int)position.X - 32, (int)position.Y - 32, 64, 64);



            status = new EntityManager(Globals.ex_states, Globals.ex_animations, ref sheet[sheetIndex]);
            cData = new List<Collision>();
            dData = new List<DamageData>();
            currentDungeon = dungeon;
            depth += (id / 32768f);  //profondità unica dell' entità
            ent_color = Color.White;
            status.SetOn(0, sheet[sheetIndex].GetTotalDuration(0), 0,false, true);

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

            //selezione dei danni in base al tipo di esplosione
            switch (exp_type)
            {
                case (int)explosion_types.gas:
                    dam[(int)damageTypes.poison] = 0.005f;
                    pro[(int)damageEffects.poison] = 1;
                    eff[(int)damageEffects.poison] = 0.00005f;
                    dur[(int)damageEffects.poison] = 5000;
                    break;
                case (int)explosion_types.typical:  
                    dam[(int)damageTypes.physical] = 10;
                    dam[(int)damageTypes.fire] = 10;
                    pro[(int)damageEffects.mechanical] = 1;
                    eff[(int)damageEffects.mechanical] = 0.005f;
                    dur[(int)damageEffects.mechanical] = 1;
                    break;
                case (int)explosion_types.napalm: 
                    dam[(int)damageTypes.fire] = 20;
                    pro[(int)damageEffects.burn] = 1;
                    eff[(int)damageEffects.burn] = 0.01f;
                    dur[(int)damageEffects.burn] = 5000;
                    break;
            }
            damage = new Damage(dam, tim, pro, eff, dur);
            damageData = new DamageData(position, factionId, id, damage,null,boundingCircle); 
        }

        private void Collapse()
        {
            if (!status.IsLocked((int)ex_states.collapsing))
            {
                status.SetOff((int)ex_states.expanding);
                status.SetOn(1, sheet[sheetIndex].GetTotalDuration(1), 1, false, true);
            }
        }

        private void Exhaust()
        {
            if (!status.IsLocked((int)ex_states.exhausted))
            {
                status.SetOff((int)ex_states.collapsing);
                status.SetOn((int)ex_states.exhausted,sheet[sheetIndex].GetTotalDuration(1),(int)ex_animations.exhausted,false,true);
            }
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
            status.Update(gameTime);
            status.AutoOff();

            if (status.IsOn((int)ex_states.expanding))
                boundingCircle.Radius += expansionSpeed;

            if (status.Finished((int)ex_states.expanding))
                this.Collapse();

            if (status.IsOn((int)ex_states.collapsing))
            {
                boundingCircle.Radius -= collapseSpeed;
                
            }

            if (status.Finished((int)ex_states.collapsing))
                updatable = false;
            /*if(status.IsOn((int)ex_states.collapsing))
            {
                boundingCircle.Radius = 0;
            }*/

            if (boundingCircle.Radius <= 300f)
            {
                
                boundingBox.HalfWidths = new Vector2(boundingBox.HalfWidths.X + expansionSpeed, boundingBox.HalfWidths.Y + expansionSpeed);
            }

            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;

           
            
        }

        public void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);

            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(currentstatus, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(currentstatus, status.GetCurrentFrame()), scale, SpriteEffects.None, Depths.explosions);
            //boundingBox.Draw(BoxTexture, camera, Globals.spriteBatch,0.3f);
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            //boundingBox.Draw(BoxTexture, camera, Globals.spriteBatch,0.3f);
            if (Globals.extremeDMode)
                boundingCircle.Draw(camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 50, position.Y - camera.Top);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; velocitate di espansione: " + this.expansionSpeed.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
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

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l' entità verrà rimossa della lista dlle entità
        /// </summary>
        public bool Updatable
        {
            get { return updatable; }
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
            get { return exp_type; }
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

        public int factionId { get; set; }
    }
}
