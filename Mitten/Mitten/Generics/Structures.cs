using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SlimDX.XInput;

namespace Mitten
{
    /// <summary>
    /// Collezione di parametri per lo scambio di dati fra entità coinvolte in una collisione.
    /// </summary>
    [Serializable]
    public struct Collision
    {
        public bool collided;
        public int type;                //type of the other entity with which an entity collides
        public String name;             //name of the entity with which it has been a collision
        public Vector2 position;        //posizione dell'entità collisa
        public Vector2 direction;       //direzione dell'entità collisa
        public float penetration_depth; //depth of the penetration
        public float distance;          //distanza fra le due entità
        public int factionId;
        public int id;                  //unique id
        [NonSerialized]public OBB boundingBox;
        public float depth;
        public int subtype;
        public bool generic;
        public bool damage_done;
        public IEntity entity;
        [NonSerialized]public VAxis axis;

        public Collision(bool collided, int type, VAxis axis, String name, Vector2 position, Vector2 direction,
    float penetration_depth, float distance, int factionId, int id, OBB boundingBox, OBB radar, float depth, int state, int subtype, bool generic, bool damage_done)
        {
            this.axis = axis;
            this.collided = collided;
            this.type = type;
            this.name = name;
            this.position = position;
            this.direction = direction;
            this.penetration_depth = penetration_depth;
            this.distance = distance;
            this.factionId = factionId;
            this.id = id;
            this.boundingBox = boundingBox;
            this.depth = depth;
            this.subtype = subtype;
            this.generic = generic;
            this.damage_done = damage_done;
            this.entity = null;
        }

        public Collision(bool collided, int type, VAxis axis, String name, Vector2 position, Vector2 direction,
            float penetration_depth, float distance, int factionId, int id, OBB boundingBox, float depth, int state, int subtype, bool generic, bool damage_done, ref IEntity entity)
        {
            this.axis = axis;
            this.collided = collided;
            this.type = type;
            this.name = name;
            this.position = position;
            this.direction = direction;
            this.penetration_depth = penetration_depth;
            this.distance = distance;
            this.factionId = factionId;
            this.id = id;
            this.boundingBox = boundingBox;
            this.depth = depth;
            this.subtype = subtype;
            this.generic = generic;
            this.damage_done = damage_done;
            this.entity = entity;
        }
    }

    public struct DamageData
    {
        public int factionId;
        public int id;
        public int dealerId;
        public int dealerType;
        public Vector2 dealerPosition;
        public Damage damage;
        public OBB oArea;
        public Circle cArea;
        //public OBB area;

        public DamageData(Vector2 dealerPosition, int factionId=-1, int id = -1, Damage damage = null, OBB oArea = null, Circle cArea = null, int dealerId = -1, int dealerType = -1)
        {
            this.factionId = factionId;
            this.id = id;
            this.damage = damage;
            this.oArea = oArea;
            this.cArea = cArea;
            this.dealerId = dealerId;
            this.dealerPosition = dealerPosition;
            this.dealerType = dealerType;
        }

        public void ResetOBB()
        {
            oArea = null;
        }
    }

    public struct ItemInfo
    {
        public bool equippable;
        public bool usable;
        public bool stashable;
        public bool unique;
        public int id;
        public int sprite;
        public Color? color;
        public float durability;
        public float maxDurability;
        //public float[] modifier;
        public float[] minResistenceDamage;
        public float[] maxResistenceDamage;
        public float[] minResistenceEffects;
        public float[] maxResistenceEffects;
        public Damage maxDamage;
        public Damage minDamage;
        public string name;
        public string description;
        public string otherP;
        public Vector2 shape;
        public int minQuantity;
        public int maxQuantity;

        public ItemInfo(bool equippable, bool usable, bool stashable, bool unique, float durability, float maxDurability, int sprite, Damage min, Damage max, float[] minResDam, float[] maxResDam, float[] minResEff, float[] maxResEff, Vector2 shape, string name, string description, string otherP, Color? color = null, int id = -1, int minQuantity = 1, int maxQuantity = 1)
        {
            this.equippable = equippable;
            this.usable = usable;
            this.stashable = stashable;
            this.unique = unique;
            this.durability = durability;
            this.maxDurability = maxDurability;
            this.id = id;
            this.sprite = sprite;
            this.color = color;
            this.maxDamage = max;
            this.minDamage = min;
            //this.modifier = modifier;
            this.minResistenceDamage = minResDam;
            this.maxResistenceDamage = maxResDam;
            this.minResistenceEffects = minResEff;
            this.maxResistenceEffects = maxResEff;
            this.shape = shape;
            this.name = name;
            this.otherP = otherP;
            this.description = description;
            if (stashable)
            {
                this.maxQuantity = maxQuantity;
                this.minQuantity = minQuantity;
            }
            else
            {
                this.maxQuantity = 1;
                this.minQuantity = 1;
            }
        }
    }

    public struct SkillInfo
    {
        public int duration;
        public int sprite;
        public Color? color;
        public float speed;
        public float[] minResistenceDamage;
        public float[] maxResistenceDamage;
        public float[] minResistenceEffects;
        public float[] maxResistenceEffects;
        public Damage maxDamage;
        public Damage minDamage;
        public string name;
        public string description;
        public string otherP;
        public Vector2 shape;

        public SkillInfo(int duration, int sprite, Damage min, Damage max, float[] minResDam, float[] maxResDam, float[] minResEff, float[] maxResEff, Vector2 shape, string name, string description, string otherP, float speed, Color? color = null)
        {
            this.duration = duration;
            this.sprite = sprite;
            this.color = color;
            this.maxDamage = max;
            this.minDamage = min;
            this.minResistenceDamage = minResDam;
            this.maxResistenceDamage = maxResDam;
            this.minResistenceEffects = minResEff;
            this.maxResistenceEffects = maxResEff;
            this.shape = shape;
            this.name = name;
            this.otherP = otherP;
            this.speed = speed;
            this.description = description;
        }
    }
    
    /// <summary>
    /// Collezione di parametri da passare al costruttore di una classe che implementa l'interfaccia IEntity.
    /// </summary>
    [Serializable]
    public struct spawningPlace         //punto di nascita di un'Entity
    {
        public Vector2 position;
        public float radius;
        public Vector2 direction;
        public float speed;
        public float depth;
        public float health;
        public int mana;
        public float rotation;
        public int type;
        public String name;
        public int subtype;
    }
    


    /// <summary>
    /// Segnaposto per la struttura del dungeon.
    /// </summary>
    [Serializable]
    public struct tilecoord
    {
        public Point bg;
        public Point fg;
        public bool steppable;
    }



    /// <summary>
    /// Comandi da tastiera.
    /// </summary>
    [Serializable]
    public struct kControls              //struttura che contiene i comandi di un giocatore (che verrano inizializzati nei settings)
    {
        public Keys Up;
        public Keys Down;
        public Keys Left;
        public Keys Right;
        public Keys Attack;
        public Keys RangedAttack;
        public Keys Throw;
        public Keys Skill;
        public Keys Jump;
        public Keys Run;
        public Keys Parry;
        public Keys Kick;
        public Keys Action;
        public Keys Bt1;
        public Keys Bt2;
        public Keys Bt3;
        public Keys Bt4;
        public Keys Bt5;
        public Keys Bt6;
        public Keys Bt7;
        public Keys Bt8;
        public Keys Bt9;
        public Keys Bt0;
        public Keys Next;
        public Keys Previous;
        public Keys Inventory;
        public Keys Menu;
    }

    /// <summary>
    /// Comandi da gamepad.
    /// </summary>
    [Serializable]
    public struct padControls
    {
        public Buttons Up;
        public Buttons Down;
        public Buttons Left;
        public Buttons Right;
        public Buttons Attack;
        public Buttons RangedAttack;
        public Buttons Throw;
        public Buttons Action;
        public Buttons Skill;
        public Buttons Jump;
        public Buttons Run;
        public Buttons Parry;
        public Buttons Kick;
        public Buttons Next;
        public Buttons Previous;
        public Buttons Inventory;
        public Buttons Menu;
        public Buttons NextCategory;
        public Buttons PreviousCategory;
    }

    /// <summary>
    /// Comandi da gamepad.
    /// </summary>
    [Serializable]
    public struct gpadControls
    {
        public int Up;
        public int Down;
        public int Left;
        public int Right;
        public int Attack;
        public int RangedAttack;
        public int Throw;
        public int Action;
        public int Skill;
        public int Jump;
        public int Run;
        public int Parry;
        public int Kick;
        public int Next;
        public int Previous;
        public int Inventory;
        public int Menu;
        public int NextCategory;
        public int PreviousCategory;
    }

    /// <summary>
    /// Asse verticale delle entità intesa come intervallo di altezze
    /// </summary>
    public struct VAxis
    {
        float bottom;
        float center;
        float top;
        float height;


        /// <summary>
        /// Costruisce un asse a partire dagli estremi
        /// </summary>
        /// <param name="bottom">Estremo inferiore</param>
        /// <param name="top">Estremo superiore</param>
        public VAxis(float bottom, float top)
        {
            if(bottom>top) throw new ArgumentException("'bottom' must be lower than 'top'", "bottom");
            this.bottom = bottom;
            this.top = top;
            height = top - bottom;
            center = (bottom + top) / 2;
        }

        /// <summary>
        /// Costruisce un asse a partire dal centro e dalla lunghezza complessiva
        /// </summary>
        /// <param name="center">Centro dell'asse</param>
        /// <param name="height">Lunghezza dell'asse verticale</param>
        /*
        public VAxis(float center, float height)
        {
            if(height<=0) throw new ArgumentOutOfRangeException("'height' must be positive", "height");
            this.center = center;
            this.height = height;
            bottom = center - height / 2;
            top = center + height / 2;
        }*/

        /// <summary>
        /// Fa partire l'asse dal livello del suolo
        /// </summary>
        public void Floor()
        {
            bottom = 0;
            top = height;
            center = height / 2;
        }

        /// <summary>
        /// Alza (o abbassa) il segmento di una data quantità, rispettando le proporzioni
        /// </summary>
        /// <param name="amount">Differenziale verticale, valori negativi abbassano il segmento</param>
        public void Rise(float amount)
        {
            bottom += amount;
            center += amount;
            top += amount;
        }

        public float Bottom
        {
            get { return bottom; }
            set
            {
                if (value >= top) throw new ArgumentException("'Bottom' must be lower than 'Top'");
                bottom = value;
                height = top - bottom;
                center = bottom + height / 2;
            }
        }

        public float Center
        {
            get { return center; }
            set { center = value; bottom = center - height / 2; top = center + height / 2; }
        }

        public float Top
        {
            get { return top; }
            set
            {
                if (value <= bottom) throw new ArgumentException("'Top' must be lower than 'Bottom'");
                top = value;
                height = top - bottom;
                center = bottom + height / 2;
            }
        }

        public float Height
        {
            get { return height; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("'Height' must be positive");
                height = value;
                bottom = center - height / 2;
                top = center + height / 2;
            }
        }
        /*
        public static float Overlap(VAxis a1, VAxis a2)
        {
            if (a1.bottom > a2.top)
            {
                return 0;
            }
            else if (a1.bottom < a2.top && a1.bottom > a2.bottom && a1.top < a2.top)
            {
                return a1.height;
            }
            return 0;
        }

        public float UpperTouch(VAxis axis)
        {
            if (this.Top > axis.Bottom && this.Top <= axis.Top)
            {
                return this.Top - axis.Bottom;
            }
            else return 
        }*/
    }
}


    

