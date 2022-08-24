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
    public class Orb : IEntity, IDamageble
    {
        Krypton.Lights.Light2D light;
        protected int[] effects;
        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        //definizione gestore dei danni e parametri danni
        [NonSerialized]protected DamageManager damageManager;
        [NonSerialized]protected DamageData damageData;
        [NonSerialized]protected Damage damage;

        protected List<DamageData> dData; // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create da questa entità
        protected List<int> dIds = new List<int>(); //lista degli id unici dei danni

        bool updatable = true;
        bool alive = true;
        Circle boundingCircle;
        Color ent_color;
        
        Dungeon currentDungeon;
        EntityManager status;
        
        int factionId;
        int id;
        protected int sheetIndex;
        int state;
        int subtype;
        int type;
        float angle;
        float depth;
        float health;
        float rotationAngle;
        float speed;
        OBB boundingBox;
        Random r;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 oldPosition;
        Vector2 position;
        Vector2 scale;
        ICaster caster;
        List<int> cIds;
        List<Collision> cData;

        VAxis axis;

        public Orb(int factionId, int subtype, int type, ref Dungeon currentDungeon, ref SpriteSheet[] sheet, ICaster caster, float angle)
        {
            this.factionId = factionId;
            id = Globals.AssignAnId();

            r = new Random(id);
            rotationAngle = (float)Math.PI/2;
            this.caster = caster;
            this.angle = angle;
            this.direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            health = 100; //basarlo su power
            damageManager = new DamageManager(health);
                        
            dam[(int)damageTypes.energy] = 0;
            pro[(int)damageEffects.mechanical] = 0;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 0;

            oldPosition = position;
            position = caster.getPosition + direction *30;
            boundingCircle = new Circle(position, 10);

            damage = new Damage(dam, tim, pro, eff, dur);
            damageData = new DamageData(this.position, factionId, id, damage, null, boundingCircle, id, type);
            axis = new VAxis(63, 65);

            
            ent_color = Color.Orange ;

            this.currentDungeon = currentDungeon;
            sheetIndex = (int)sheetIndexes.orb;
            this.sheet = sheet;
            status = new EntityManager(5, 5, ref sheet[sheetIndex]);
            
            //state = 0;
            this.subtype = subtype;
            this.type = type;
            this.speed = (float)Math.PI / 100;
            boundingBox = new OBB(position, rotationAngle, new Vector2(3, 3));
            graphicOccupance = new Rectangle((int)position.X - 3, (int)position.Y - 3, 6, 6);
            
            name = "Orb";
            
           
            scale = new Vector2(1, 1);
            depth = Depths.foreSkill;

            cData = new List<Collision>();
            cIds = new List<int>();
            dData = new List<DamageData>();
            spawned = new List<IEntity>();

            light = new Krypton.Lights.Light2D();
            light.IsOn = false;
            light.Fov = MathHelper.TwoPi;
            light.Color = Color.White;
            light.Angle = 0;
            light.Intensity = 0.3f;
            light.Texture = Globals.mLightTexture;
            light.Range = 100f;
            Globals.krypton.Lights.Add(light);
            
            status.SetOn(0, sheet[sheetIndex].GetTotalDuration(0), 0, false, true);
            status = new EntityManager(status);
        }

        private void Explode()
        {
            if ( !status.IsOn(3) && !status.IsOn(4) && alive)
            {
                damageManager.health = 0;
                alive = false;
                List<int> l = new List<int>();
                status.SetOff(0);
                status.SetOff(1);
                status.SetOff(2);
                status.SetOff(3);
                l.Add(0);
                l.Add(1);
                l.Add(2);
                l.Add(3);
                status.SetOn(4, sheet[sheetIndex].GetTotalDuration(4), 4, false, true, l);
                speed = 0.25f;
                    
            }
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
            position = caster.getPosition + direction * 30;

            boundingCircle.Center = position;
           
            damageManager.Update(gameTime);
            if (damageManager.health <= 0 || currentDungeon.WallContact(boundingCircle))
                Explode();

            damageData.dealerPosition = caster.getPosition;
            damageData.cArea = boundingCircle;

            if (status.IsOn(0)) //nascita
            {
                oldPosition = position;
                //damageData.ResetOBB();
                //boundingCircle.Center = position;
            }
            if (status.Finished(0))
            {
                status.SetOn(1,1, true, true, false);
            }
            
            
            if (status.IsOn(1)) //idle
            {
                angle += speed;
                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                light.IsOn = false;
                position = caster.getPosition + direction * 30;
                boundingBox.Origin = position;

                if (status.GetCurrentFrame() == 10 && r.NextDouble() > 0.90f)
                {
                    status.SetOff(1);
                    status.SetOn(2, sheet[sheetIndex].GetTotalDuration(2), 2, false, true);
                }
            }

            if (status.IsOn(2)) //idle2
            {
                
                angle += speed;
                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                light.IsOn = false;
                position = caster.getPosition + direction * 30;
                boundingBox.Origin = position;
                //damage = new Damage(dam, tim, pro, eff, dur);
                //damageData = new DamageData(this.position, factionId, id, damage, null, boundingCircle, id, type);
            }
            else if (status.Finished(2))
            {
                status.SetOn(1, 1, true, true, false);
            }

            if (status.IsOn(3))  //damaged
            {
                angle += speed;
                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                position = caster.getPosition + direction * 30;
                boundingBox.Origin = position;
            }
            else if (status.Finished(3))
            {
                status.SetOn(1, 1, true, true, false);
            }

            if (status.IsOn(4))  //die
            {
                angle += speed;
                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                position = caster.getPosition + direction * 30 *  ((((float)sheet[sheetIndex].GetTotalDuration(4) - (float)status.TimeLeft(4))/100) +1);
                boundingBox.Origin = position;
                light.IsOn = true;
                light.Intensity = 0.65f - (float)status.GetCurrentFrame() / 10f;
            }
            else if (status.Finished(4))
            {
                Globals.krypton.Lights.Remove(light);
                light = null;
                updatable = false;
            }

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id))
                    {
                        if (c.collided)
                        {
                            switch (c.type)
                            {
                                case (int)entityTypes.item: break;
                                case (int)entityTypes.dead: break;
                                default:
                                    if (c.factionId != factionId && axis.Bottom <= c.axis.Top && axis.Top >= c.axis.Bottom)
                                    {
                                        if (factionId!=c.factionId)
                                            damageManager.health=-1;
                                        if (!status.IsOn(3) && !status.IsOn(4))
                                        {
                                            status.SetOff(1);
                                            status.SetOff(2);
                                            status.SetOn(3, sheet[sheetIndex].GetTotalDuration(3), 3, false, true, null,true);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }


                cIds.Clear();
            }
            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id) && d.factionId != factionId)
                {
                    status.SetOn(3, sheet[sheetIndex].GetTotalDuration(3), 3, false, true);
                    dIds.Add(d.id);
                    if (factionId!=d.factionId)
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

            if (light != null)
            {
                light.X = position.X - Globals.camera[0].Left;
                light.Y = position.Y - Globals.camera[0].Top;
            }
            cData.Clear();
            dData.Clear();
            
        }

        public void Draw(Rectangle camera)
        {

            if (updatable)
            {
                Globals.IAmanager.Intensity((IEntity)this);
                Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale*1, SpriteEffects.None, depth + id / Globals.max_entities);
                //this.boundingCircle.Draw(camera);
            }
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            if (Globals.extremeDMode)
                this.boundingCircle.Draw(camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 25);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
        }

        #region action

        #endregion

        #region properties
        public Krypton.Lights.Light2D Light
        {
            get { return light; }
            set { light = value; }
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

        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
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
            get { return boundingBox; }
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
