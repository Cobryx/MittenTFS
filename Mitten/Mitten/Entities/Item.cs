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
    public class Item : IEntity
    {
        protected bool picked = false;
        protected bool stashable;
        protected bool equipped = false;
        protected bool equippable;
        protected bool usable;
        protected bool unique;
        bool xMovement = false;

        #region members
        protected bool alive = true; //determina se l' entità è viva o no
        private bool updatable = true;

        //definizione cerchi collidenti
        protected Circle boundingCircle;

        protected Color ent_color; //filtro entità

        //definizione gestore dei danni e parametri danni
        protected DamageData damageData;
        protected Damage damage;

        protected Dungeon currentDungeon; //referred dungeon

        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float maxDurability;
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed=0;
        protected float vSpeed;

        //protected float[] modifier;

        protected int id; //id unico dell entità
        protected int quantity;
        protected int type; //tipo di entità - uguale per tutti gli oggetti
        protected int sheetIndex;
        protected int sprite;
        protected int subtype;  //vero numero identificatore del tipo di oggetto

        protected OBB boundingBox;
        protected Rectangle graphicOccupance;

        protected SpriteSheet[] sheet; //riferiemento allo spritesheet

        private String[] debugString = new String[1];
        private String[] logString = new String[28];
        protected String name; //nome entità
        protected String description;   //descrizione
        protected String otherP;    //suffissi "tecnici"
        
        protected Vector2 direction; //direzione entità
        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 pivot; // perno di rotazione entità
        protected Vector2 scale = new Vector2(1, 1); //scala dimensionamento entità

        public float[] DamageModifier = new float[Globals.ndamagetypes];
        public float[] EffectModifier = new float[Globals.damage_effects];

        //istanziazione vettori per la definizione di damagedata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        protected List<Collision> cData; //lista delle collisioni
        protected List<DamageData> dData; // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create dell entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell'entità
        protected List<int> dIds = new List<int>(); //lista degli id unici dei danni
        private int factionId;
        VAxis axis;

        #endregion
        /// <summary>
        /// Costruttore che istanzia un item a partire da una struttura ItemInfo.
        /// </summary>
        /// <param name="source">Struttura di riferimento</param>
        /// <param name="position">Posizione in coordinate cartesiane</param>
        /// <param name="direction">Direzione iniziale (attualmente inutilizzato)</param>
        /// <param name="depth">Profondità per l'algoritmo del pittore</param>
        /// <param name="rotation">Angolo di rotazione</param>
        /// <param name="sheet">Riferimento allo spritesheet globale</param>
        /// <param name="dungeon">Dungeon corrente</param>
        /// <param name="box">Texture per il debug</param>
        /// <param name="quantity">Quantità di oggetti</param>
        public Item(ItemInfo source, Vector2 position, Vector2 direction, float depth, float rotation, ref SpriteSheet[] sheet, ref Dungeon dungeon, bool dropped, int quantity = 0)
        {
            axis = new VAxis(0, 0);

            this.boundingCircle = new Circle(position, 300);
            this.currentDungeon = dungeon;
            
            this.direction = direction;
            this.health = source.durability;
            this.maxDurability = source.maxDurability;
            if (source.id != -1)
                this.id = source.id;
            else
                id = Globals.AssignAnId();
            this.depth = depth+ id/Globals.max_entities;
            this.damage = Damage.Randomize(source.minDamage, source.maxDamage);
            this.description = source.description;
            //this.modifier = source.modifier;
            this.name = source.name;
            this.position = position;
            rotationAngle = rotation;
            this.sheet = sheet;
            sheetIndex = (int)sheetIndexes.item;
            sprite = source.sprite;
            subtype = -1;
            type = (int)entityTypes.item;
            unique = source.unique;
            description = source.description;
            stashable = source.stashable;
            equippable = source.equippable;
            usable = source.usable;
            DamageModifier = source.maxResistenceDamage;
            EffectModifier = source.maxResistenceEffects;
            otherP = source.otherP;
            //da verificare la coerenza
            if (!stashable)
                this.quantity = 1;
            else
            {
                if (quantity > 0)
                    this.quantity = quantity;
                else
                {
                    Random rand = new Random();
                    this.quantity = rand.Next(source.minQuantity, source.maxQuantity+1);
                }
            }
            //Random r = new Random();
            //this.quantity = r.Next(1, 55);
            boundingBox = new OBB(position, rotationAngle, source.shape);
            if (source.color != null)
            {
                ent_color = source.color.Value;
                boundingBox.DebugColor = ent_color;
            }
            else
                //Rosso per ragioni di debug
            {
                ent_color = Color.Red;
                boundingBox.DebugColor = Color.Red;
            }
            cData = new List<Collision>();
            dData = new List<DamageData>();

            if (dropped)
            {
                Random r = new Random();
                axis.Center = 64;
                //scale = new Vector2(1.3f, 1.3f);
                depth = Depths.middle_air;
                xMovement = true;
                vSpeed = -Globals.G;
                speed = 1;
                rotationAngle = (float)(r.NextDouble() * MathHelper.TwoPi);
                direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
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

        public void Update(GameTime gameTime)
        {
            Updatable = !alreadyPicked;
            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id))
                {
                    dIds.Add(d.id);
                    if (d.damage.getEffectProbability[(int)damageEffects.mechanical] > 0)
                    {
                        direction = d.oArea.Origin - position;
                        speed = d.damage.getEffectDamage[(int)damageEffects.mechanical];
                    }
                }
            }
            dIds.Clear();

            if (xMovement)
            {
                axis.Center += vSpeed / Globals.cycle;
                vSpeed -= Globals.G / Globals.cycle;
                scale.X = 1 + axis.Center * 0.00391f;
                scale.Y = 1 + axis.Center * 0.00391f;
                rotationAngle += 0.005f;
                if (axis.Bottom < 0)
                {
                    xMovement = false;
                    axis.Floor();
                    vSpeed = 0;
                    speed = 0;
                    depth = Depths.item;
                }
            }

            position += direction*speed;
            //speed *= 0.9f;

            if (speed < 0.01f) speed = 0;
            cData.Clear();
            dData.Clear();
        }

        public void Draw(Rectangle camera)
        {
            Globals.IAmanager.Intensity((IEntity)this);
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(sprite, 0), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(sprite, 0), scale, SpriteEffects.None, depth + (float)id / Globals.max_entities);
            
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            //boundingBox = new OBB(new Vector2(100, 100), 0f, new Vector2(100, 200));
            //boundingBox.Draw(box, camera, Globals.spriteBatch, depth);
            //immagine sorgente, posizione su schermo, rettangolo del frame desiderato, colore, angolo di rotazione, centro di rotazione, fattore di scala, effetti, profondità
            //boundingCircle.Draw(Globals.spriteBatch, camera, box);
        }


        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top + 15);

            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();

            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
            
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
        /// ottiene o imposta un valore che verifica se l' oggetto è già stato raccolto
        /// </summary>
        public bool alreadyPicked
        {
            get { return picked; }
            set { picked = value; }
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
        /// Statistiche dell'oggetto
        /// </summary>
        public Damage getDamage
        {
            get { return damage; }
        }

        /// <summary>
        /// Ottiene l'integrità dell'oggetto
        /// </summary>
        public float getDurability
        {
            get { return health; }
        }

        //proprietà per gli array modificatori

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
        /// Ottiene o imposta il dungeon corrente dell 'entità
        /// </summary>
        public Dungeon dungeon
        {
            get { return currentDungeon; }
            set { currentDungeon = value; }
        }


        /// <summary>
        /// Ottiene la profondità dell' entità
        /// </summary>
        public float getDepth
        {
            get { return depth; }
        }

        /// <summary>
        /// Ottiene l'id unico dell'entità
        /// </summary>
        public int getId
        {
            get { return this.id; }
        }

        /// <summary>
        /// Ottiene la quantità di oggetti (se accatastabili)
        /// </summary>
        public int getQuantity
        {
            get { return quantity; }
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
            get { return 1; }
            set { }
        }
        
        public ItemInfo ToItemInfo()
        {
            return new ItemInfo(equippable, usable, stashable, unique, health, maxDurability, sprite, damage, damage, DamageModifier, DamageModifier, EffectModifier, EffectModifier, boundingBox.HalfWidths, name, description, otherP, ent_color, id, quantity, quantity);
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
            get
            {
                Vector2 asd = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
                asd.Normalize();
                return asd;
            }
        }

        /// <summary>
        /// Restituisce lo sprite di quest'oggetto
        /// </summary>
        public int getSprite
        {
            get { return sprite; }
        }

        #endregion
    }
}
