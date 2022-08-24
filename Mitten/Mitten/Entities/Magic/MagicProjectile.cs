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
    public class MagicProjectile  : IEntity, ILightEntity,IAttacker
    {
        Krypton.Lights.Light2D light;
        protected int[] effects;
        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        

        bool updatable=true;
        bool alive = true;
        Circle boundingCircle;
        Color ent_color;
        Damage damage;
        DamageData damageData;
        Dungeon currentDungeon;
        EntityManager status;
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
        OBB boundingBox;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 oldPosition;
        Vector2 position;
        Vector2 scale;

        List<int> cIds;
        List<Collision> cData;
        List<DamageData> dData;
        List<IEntity> spawned;

        VAxis axis;

        public MagicProjectile(int element, int factionId, int subtype, int type, float rotationAngle, ref Dungeon currentDungeon, ref SpriteSheet[] sheet, Vector2 position)//depth
        {
            Color color;
            switch (element)
            {
                case (int)damageTypes.air: color = Color.BlueViolet; break;
                case (int)damageTypes.earth: color = Color.Brown; break;
                case (int)damageTypes.energy: color = Color.Yellow; break;
                case (int)damageTypes.fire: color = Color.OrangeRed; break;
                case (int)damageTypes.physical: color = Color.White; break;
                case (int)damageTypes.poison: color = Color.Green; break;
                case (int)damageTypes.spectral: color = Color.DarkGray; break;
                case (int)damageTypes.water: color = Color.Blue; break;
                default: color = Color.White; break;
            }
            dam[(int)damageTypes.energy] = 5;
            pro[(int)damageEffects.mechanical] = 0;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 0;
            boundingCircle = new Circle(position, 10);
            damage = new Damage(dam, tim, pro, eff, dur);
            axis = new VAxis(63, 65);
            
            ent_color = color;
            
            this.currentDungeon = currentDungeon;
            sheetIndex = (int)sheetIndexes.magicbolt;
            status = new EntityManager(2, 2, ref sheet[sheetIndex]);
            status.SetOn(0, 0, true, true); 
            this.factionId = factionId;
            id = Globals.AssignAnId();
            state = 0;
            this.subtype = subtype;
            this.type = type;
            this.rotationAngle = rotationAngle;
            this.speed = 10f;  
            boundingBox = new OBB(position, rotationAngle, new Vector2(3, 3));
            graphicOccupance = new Rectangle((int)position.X-3, (int)position.Y-3, 6, 6);
            this.sheet = sheet;
            name = "Magic projectile";
            this.direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            oldPosition = position;
            this.position = position;
            scale = new Vector2(1, 1);
            depth = Depths.foreSkill;

            cData = new List<Collision>();
            cIds = new List<int>();
            dData = new List<DamageData>();
            damageData = new DamageData(position, factionId, id, damage, boundingBox, boundingCircle, id, type);
            spawned = new List<IEntity>();

            light = new Krypton.Lights.Light2D();
            light.IsOn = true;
            light.Fov = MathHelper.TwoPi;
            light.Color = color;
            light.Angle = 0;
            light.Intensity = 0.6f;
            light.Texture = Globals.mLightTexture;
            light.Range = 100f;
            Globals.krypton.Lights.Add(light);
            
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

        /*public void SetDamageData(DamageData Data)
        {
            this.dData.Add(Data);
        }*/

        public void Update(GameTime gameTime)
        {
            damageData.dealerPosition = position;
            damageData.cArea = boundingCircle;
            damageData.oArea = boundingBox;

            status.Update(gameTime);
            status.AutoOff();


            if (status.IsOn(0))
            {
                oldPosition = position;
                position += direction * speed;
                //damageData.ResetOBB();

                boundingBox.Origin = position;
                boundingCircle.Center = position;

                if (currentDungeon.WallContact(position + new Vector2(0, direction.Y * speed )) || currentDungeon.WallContact(position + new Vector2(0, -direction.Y * speed )))
                {
                    oldPosition = position;
                    rotationAngle = ((float)Math.PI / 2) * Math.Sign(direction.Y);
                    position.Y = ((position.Y + (direction.Y * speed)) / 32) * 32 -3 * Math.Sign(direction.Y);
                    light.Y = (((position.Y + (direction.Y * speed)) / 32) * 32 - 12 * Math.Sign(direction.Y)) - Globals.camera[0].Top;
                    direction = Vector2.Zero;
                    Explode();
                }
                else if (currentDungeon.WallContact(position + new Vector2(direction.X * speed , 0)) || currentDungeon.WallContact(position + new Vector2(-direction.X * speed , 0)))
                {
                    oldPosition = position;
                    if (Math.Sign(direction.X) == -1)
                        rotationAngle = (float)Math.PI;
                    else
                        rotationAngle = 0;
                    position.X = ((position.X + (direction.X * speed)) / 32) * 32 - 3 * Math.Sign(direction.X);
                    
                    light.X = (((position.X + (direction.X * speed)) / 32) * 32 - 12 * Math.Sign(direction.X)) -Globals.camera[0].Left;

                    direction = Vector2.Zero;
                    
                    Explode();
                }
            }
            else if (status.Finished(0))
            {

            }

            if (status.IsOn(1))
            {
                damage = new Damage(dam, tim, pro, eff, dur);
                damageData = new DamageData(this.position, factionId, id, damage, null, boundingCircle, id, type);
                light.Intensity = 0.65f - (float)status.GetCurrentFrame()/15f;
            }
            else if(status.Finished(1))
            {
                Globals.krypton.Lights.Remove(light);
                light = null;
                Updatable = false;
                alive = false;
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
                                case (int)entityTypes.throwable: break;
                                case (int)entityTypes.dead: break;
                                case (int)entityTypes.explosion: break;
                                case (int)entityTypes.icewall: Explode(); break;
                                default:
                                    if (c.factionId != factionId && axis.Bottom <= c.axis.Top && axis.Top >= c.axis.Bottom)
                                    {
                                        //lo stato 0 non termina mai autonomamente giacché la durata è indeterminata;
                                        //in un'entità normale ci sarebbe stato un metodo che avrebbe provveduto a settare a off gli stati da escludere
                                        Explode();
                                    }
                                    break;
                            }
                        }
                    }
                }
                cIds.Clear();
            }


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
            if (updatable) //c'è da regolare la cosa a livello globale
            {
                Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
            }
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
            if ( Globals.extremeDMode)
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

        private void Explode()
        {
            if(!status.IsOn(1) && alive)
            {
                alive = false;
                status.SetOn(1, sheet[sheetIndex].GetTotalDuration(1), 1, false, true);
                speed = 0;
            }
        }


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
