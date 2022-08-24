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
    public class Shield
    {
        Krypton.Lights.Light2D lightLaser;
        Krypton.Lights.Light2D lightSource;

        int lightSourceIndex;
        bool existence;
        bool updatable = true;
        Circle boundingCircle;
        Color ent_color;
        Damage damage;
        DamageData damageData;
        Dungeon currentDungeon;
        int currentAnimation;
        int factionId;
        int id;
        protected int sheetIndex;
        int state;
        int subtype;
        int type;
        float depth;
        
        float rotationAngle;
        float speed;
        float thickness=0;
        float maxThickness =2 ;

        ICaster caster;
        OBB boundingBox;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 initialPosition;
        Vector2 length;
        Vector2 destination;

        Vector2 offset; // serve per posizionare i vari raggi

        Vector2 oldPosition;
        Vector2 origin;
        Vector2 scale;

        List<int> cIds;
        List<Collision> cData;
        List<DamageData> dData;
        List<IEntity> spawned;

        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        IEntity[] orbs;
        VAxis axis;

        public Shield(float power, int element, int factionId, int subtype, int type,  ref Dungeon currentDungeon, ref SpriteSheet[] sheet, ICaster caster)
        {
            spawned = new List<IEntity>();
            if (subtype == 0)
            {
                //if (power) // da implementare
                orbs = new Orb[3];
                for (int i = 0; i < 3; i++)
                {
                    orbs[i] = new Orb(factionId, 0, (int)entityTypes.magic, ref currentDungeon, ref sheet, caster, (MathHelper.TwoPi / 3) * i);
                    spawned.Add(orbs[i]);
                }

            }
            
        }

        #region properties
        public Krypton.Lights.Light2D Light
        {
            get { return lightLaser; }
            set { lightLaser = value; }
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
            //List<IEntity> l = new List<IEntity>();

            return spawned ;
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
            get { return initialPosition; }
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
