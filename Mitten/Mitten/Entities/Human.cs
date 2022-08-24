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
    public class Human : Shiftable, IEntity, ICollector, IShadow, ICaster,IDamageble,IAttacker
    {
        #region general members
        protected bool alive = true; //determina se l' entità è viva o no
        protected bool corpse = false;
        private bool fl_attack = false; // indica se l' attacco è attivo
        protected bool combo = false;
        protected bool comboBreaker = false;

        //definizione cerchi collidenti
        protected Circle boundingCircle;
        protected Circle[] damageCircles;

        protected Color ent_color; //filtro entità
        protected Color weapon_color; //filtro arma

        //definizione gestore dei danni e parametri danni
        [NonSerialized] protected DamageManager damageManager;
        [NonSerialized] protected DamageData damageData;
        [NonSerialized] protected Damage damage;

        protected Dungeon currentDungeon; //referred dungeon

        [NonSerialized] protected EntityManager status;

        protected float angularSpeed;
        protected float depth; //profondità dell' entità
        protected float health; //vita dell' entità
        protected float rotationAngle; //angolo di rotazione dell entità
        protected float speed; //velocità dell 'entità

        protected int cause_of_death = -1; //causa della morte
        protected int frame; //frame attualmente in corso
        protected int hand = (int)handleables.nothing;
        protected int id; //id unico dell entità
        public int genericAction; //verifica se si sta effetuando un azione
        protected float power = 3;
        protected int selectedSkill = (int)skills.none;
        protected int type; //tipo di entità
        protected int destination = 0;  //a uso e consumo di player soltanto

        [NonSerialized]protected Inventory inventory;

        [NonSerialized] protected OBB boundingBox;
        [NonSerialized]protected OBB attackBox;
        protected Rectangle graphicOccupance;

        [NonSerialized] public SkillManager skillManager;

        [NonSerialized]protected SpriteSheet[] handSheet; //referimento allo spritesheet delle armi
        [NonSerialized]protected SpriteSheet[] sheet; //riferiemento allo spritesheet

        private String[] debugString = new String[1];
        private String[] logString = new String[28];
        protected String name; //nome entità
        
        protected Vector2 direction; //direzione entità
        protected Vector2 oldPosition; //position to restore in case of collision
        protected Vector2 origin; //origine sprite
        protected Vector2 position; //coordinate cartesiane
        protected Vector2 pivot; // perno di rotazione entità
        protected Vector2 scale; //scala dimensionamento entità

        [NonSerialized]protected VAxis axis;

        //coordinate (relative) del punto in cui vengono originate le entità lanciate dal giocatore
        protected Vector2 arrowOrigin;
        protected Vector2 dartOrigin = new Vector2(13, 5);

        protected Vector2 boxDistance;
        protected int[] effects;
        //istanziazione vettori per la definizione di damageData
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        protected List<Collision> cData; //lista delle collisioni
        protected List<DamageData> dData; // lista dei danni
        protected List<IEntity> spawned = new List<IEntity>(); //lista delle entità create da questa entità
        protected List<int> cIds = new List<int>(); //lista degli id unici dell'entità
        protected List<int> dIds = new List<int>(); //lista degli id unici dei danni
        protected int factionId;

        protected int projectile = -1;    
        protected int launched = -1;      
        protected int sheetIndex;         

        protected IRemoteEntity remote;

        protected int attackStage=0;

        [NonSerialized] protected Krypton.ShadowHull shadow;
      

        SpriteEffects spriteEffects = SpriteEffects.None;
        #endregion


        public Human(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref SpriteSheet[] handSheet, ref Dungeon dungeon)
        {
            axis = new VAxis(1, 115); //altezza
            sheetIndex = (int)sheetIndexes.human1; //+ r.Next(0, 3);

            /*axis = new VAxis(1, 1); //altezza
            shadow = Krypton.ShadowHull.CreateCircle(14, 3, axis); // debug: axis da eliminare
            shadow.Axis = axis;
            //shadow = Krypton.ShadowHull.CreateConvex(ref convexVectors); //definizione dell ombra, da rivedere con un poligono più dettagliato
            Globals.krypton.Hulls.Add(shadow);*/

            this.boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(10, 16));
            this.boundingCircle = new Circle(position, 10);
            //   Vector2[] convexVectors = new Vector2[43];
           // for (int i =0 ; i<convexVectors.Length;i++)
            {
             /*   if (i%3==0)
                    convexVectors[i]=new Vector2(position.X,position.Y);*/
            }
            #region convexVectors vari
            /* convexVectors[1] = new Vector2(position.X, position.Y - 14);
                    convexVectors[2] = new Vector2(position.X + 2, position.Y - 13);
                    convexVectors[4] = new Vector2(position.X + 3, position.Y - 12);
                    convexVectors[5] = new Vector2(position.X + 4, position.Y - 11);
                    convexVectors[7] = new Vector2(position.X + 4, position.Y - 9);
                    convexVectors[8] = new Vector2(position.X + 3, position.Y - 8);
                    convexVectors[10] = new Vector2(position.X + 1, position.Y - 7);
                    convexVectors[11] = new Vector2(position.X + 3, position.Y - 6);
                    convexVectors[13] = new Vector2(position.X + 4, position.Y - 5);
                    convexVectors[14] = new Vector2(position.X + 5, position.Y - 4);
                    convexVectors[16] = new Vector2(position.X + 6, position.Y - 3);
                    convexVectors[17] = new Vector2(position.X + 6, position.Y - 2);
                    convexVectors[19] = new Vector2(position.X + 7, position.Y - 1);
                    convexVectors[20] = new Vector2(position.X + 7, position.Y);
                    convexVectors[22] = new Vector2(position.X + 7, position.Y + 1);
                    convexVectors[23]=new Vector2(position.X+6, position.Y+2);
                    convexVectors[25] = new Vector2(position.X + 6, position.Y + 3);
                    convexVectors[26] = new Vector2(position.X + 5, position.Y + 4);
                    convexVectors[28] = new Vector2(position.X + 4, position.Y + 5);
                    convexVectors[29] = new Vector2(position.X + 3, position.Y + 6);
                    convexVectors[31] = new Vector2(position.X + 1, position.Y + 8);
                    convexVectors[32] = new Vector2(position.X + 3, position.Y + 9);
                    convexVectors[34] = new Vector2(position.X + 4, position.Y + 10);
                    convexVectors[35] = new Vector2(position.X + 4, position.Y + 12);
                    convexVectors[37] = new Vector2(position.X + 3, position.Y + 13);
                    convexVectors[38] = new Vector2(position.X + 2, position.Y + 14);
                    convexVectors[40] = new Vector2(position.X, position.Y - 14);
                    convexVectors[41] = new Vector2(position.X - 1, position.Y - 14);
                    convexVectors[43] = new Vector2(position.X - 3, position.Y - 13);
                    convexVectors[44] = new Vector2(position.X - 5, position.Y - 12);
                    convexVectors[46] = new Vector2(position.X - 6, position.Y - 10);
                    convexVectors[47] = new Vector2(position.X - 7, position.Y - 7);
                    convexVectors[49] = new Vector2(position.X - 8, position.Y - 4);
                    convexVectors[50] = new Vector2(position.X - 7, position.Y - 2);
                    convexVectors[52] = new Vector2(position.X - 8, position.Y);
                    convexVectors[53] = new Vector2(position.X - 7, position.Y + 2);
                    convexVectors[55] = new Vector2(position.X - 8, position.Y + 4);
                    convexVectors[56] = new Vector2(position.X - 7, position.Y + 7);
                    convexVectors[58] = new Vector2(position.X - 6, position.Y + 10);
                    convexVectors[59] = new Vector2(position.X - 5, position.Y + 12);
                    convexVectors[61] = new Vector2(position.X - 3, position.Y + 13);
                    convexVectors[62] = new Vector2(position.X - 1, position.Y + 14);*/
            /*
                    convexVectors[0] = new Vector2(position.X, position.Y );
                    convexVectors[1] = new Vector2(position.X, position.Y - 14);
                    convexVectors[2] = new Vector2(position.X + 2, position.Y - 13);
                    convexVectors[3] = new Vector2(position.X + 3, position.Y - 12);
                    convexVectors[4] = new Vector2(position.X + 4, position.Y - 11);
                    convexVectors[5] = new Vector2(position.X + 4, position.Y - 9);
                    convexVectors[6] = new Vector2(position.X + 3, position.Y - 8);
                    convexVectors[7] = new Vector2(position.X + 1, position.Y - 7);
                    convexVectors[8] = new Vector2(position.X + 3, position.Y - 6);
                    convexVectors[9] = new Vector2(position.X + 4, position.Y - 5);
                    convexVectors[10] = new Vector2(position.X + 5, position.Y - 4);
                    convexVectors[11] = new Vector2(position.X + 6, position.Y - 3);
                    convexVectors[12] = new Vector2(position.X + 6, position.Y - 2);
                    convexVectors[13] = new Vector2(position.X + 7, position.Y - 1);
                    convexVectors[14] = new Vector2(position.X + 7, position.Y);
                    convexVectors[15] = new Vector2(position.X + 7, position.Y + 1);
                    convexVectors[16] = new Vector2(position.X + 6, position.Y + 2);
                    convexVectors[17] = new Vector2(position.X + 6, position.Y + 3);
                    convexVectors[18] = new Vector2(position.X + 5, position.Y + 4);
                    convexVectors[19] = new Vector2(position.X + 4, position.Y + 5);
                    convexVectors[20] = new Vector2(position.X + 3, position.Y + 6);
                    convexVectors[21] = new Vector2(position.X + 1, position.Y + 8);
                    convexVectors[22] = new Vector2(position.X + 3, position.Y + 9);
                    convexVectors[23] = new Vector2(position.X + 4, position.Y + 10);
                    convexVectors[24] = new Vector2(position.X + 4, position.Y + 12);
                    convexVectors[25] = new Vector2(position.X + 3, position.Y + 13);
                    convexVectors[26] = new Vector2(position.X + 2, position.Y + 14);
                    convexVectors[27] = new Vector2(position.X, position.Y + 14);
                    convexVectors[28] = new Vector2(position.X - 1, position.Y + 14);
                    convexVectors[29] = new Vector2(position.X - 3, position.Y + 13);
                    convexVectors[30] = new Vector2(position.X - 5, position.Y + 12);
                    convexVectors[31] = new Vector2(position.X - 6, position.Y + 10);
                    convexVectors[32] = new Vector2(position.X - 7, position.Y + 7);
                    convexVectors[33] = new Vector2(position.X - 8, position.Y + 4);
                    convexVectors[34] = new Vector2(position.X - 7, position.Y + 2);
                    convexVectors[35] = new Vector2(position.X - 8, position.Y);
                    convexVectors[36] = new Vector2(position.X - 7, position.Y - 2);
                    convexVectors[37] = new Vector2(position.X - 8, position.Y - 4);
                    convexVectors[38] = new Vector2(position.X - 7, position.Y - 7);
                    convexVectors[39] = new Vector2(position.X - 6, position.Y - 10);
                    convexVectors[40] = new Vector2(position.X - 5, position.Y - 12);
                    convexVectors[41] = new Vector2(position.X - 3, position.Y - 13);
                    convexVectors[42] = new Vector2(position.X - 1, position.Y - 14);
            */
            /* convexVectors[0] = new Vector2(position.X, position.Y);
             convexVectors[1] = new Vector2(position.X, position.Y - 14);
             convexVectors[2] = new Vector2(position.X + 2, position.Y - 13);
             convexVectors[3] = new Vector2(position.X + 3, position.Y - 12);
             convexVectors[4] = new Vector2(position.X + 4, position.Y - 11);
             convexVectors[5] = new Vector2(position.X + 4, position.Y - 9);
             convexVectors[6] = new Vector2(position.X + 3, position.Y - 8);
             convexVectors[7] = new Vector2(position.X + 1, position.Y - 7);
             convexVectors[8] = new Vector2(position.X + 3, position.Y - 6);
             convexVectors[9] = new Vector2(position.X + 4, position.Y - 5);
             convexVectors[10] = new Vector2(position.X + 5, position.Y - 4);
             convexVectors[11] = new Vector2(position.X + 6, position.Y - 3);
             convexVectors[12] = new Vector2(position.X + 6, position.Y - 2);
             convexVectors[13] = new Vector2(position.X + 7, position.Y - 1);
             convexVectors[14] = new Vector2(position.X + 7, position.Y);
             convexVectors[15] = new Vector2(position.X + 7, position.Y + 1);
             convexVectors[16] = new Vector2(position.X + 6, position.Y + 2);
             convexVectors[17] = new Vector2(position.X + 6, position.Y + 3);
             convexVectors[18] = new Vector2(position.X + 5, position.Y + 4);
             convexVectors[19] = new Vector2(position.X + 4, position.Y + 5);
             convexVectors[20] = new Vector2(position.X + 3, position.Y + 6);
             convexVectors[21] = new Vector2(position.X + 1, position.Y + 8);
             convexVectors[22] = new Vector2(position.X + 3, position.Y + 9);
             convexVectors[23] = new Vector2(position.X + 4, position.Y + 10);
             convexVectors[24] = new Vector2(position.X + 4, position.Y + 12);
             convexVectors[25] = new Vector2(position.X + 3, position.Y + 13);
             convexVectors[26] = new Vector2(position.X + 2, position.Y + 14);
             convexVectors[27] = new Vector2(position.X, position.Y + 14);
             convexVectors[28] = new Vector2(position.X - 1, position.Y + 14);
             convexVectors[29] = new Vector2(position.X - 3, position.Y + 13);
             convexVectors[30] = new Vector2(position.X - 5, position.Y + 12);
             convexVectors[31] = new Vector2(position.X - 6, position.Y + 10);
             convexVectors[32] = new Vector2(position.X - 7, position.Y + 7);
             convexVectors[33] = new Vector2(position.X - 8, position.Y + 4);
             convexVectors[34] = new Vector2(position.X - 7, position.Y + 2);
             convexVectors[35] = new Vector2(position.X - 8, position.Y);
             convexVectors[36] = new Vector2(position.X - 7, position.Y - 2);
             convexVectors[37] = new Vector2(position.X - 8, position.Y - 4);
             convexVectors[38] = new Vector2(position.X - 7, position.Y - 7);
             convexVectors[39] = new Vector2(position.X - 6, position.Y - 10);
             convexVectors[40] = new Vector2(position.X - 5, position.Y - 12);
             convexVectors[41] = new Vector2(position.X - 3, position.Y - 13);
             convexVectors[42] = new Vector2(position.X - 1, position.Y - 14);
     */
            #endregion

           
         
            angularSpeed = MathHelper.Pi / 32;
            this.boxDistance = new Vector2();
           
            this.damageManager = new DamageManager(health);
            this.damageCircles = new Circle[3];
            
            this.direction = direction;
            this.handSheet = handSheet;
            this.id = Globals.AssignAnId();
            this.depth = depth+id/Globals.max_entities;
            this.factionId = id;
            this.oldPosition = position;
            this.origin = new Vector2(0, 0);
            this.position = position;
            this.rotationAngle = rotation;
            this.scale = new Vector2(1, 1);
            this.sheet = sheet;
            this.speed = speed;
            this.type = type;
            graphicOccupance = new Rectangle((int)position.X-32, (int)position.Y-32, 64, 64);

            effects = new int[Globals.damage_effects];

            skillManager = new SkillManager(factionId, ref sheet, ((ICaster)this));
            skillManager.CurrentSkill=(int)skills.laser;

            //vettore della distanza del box di attacco
            boxDistance = new Vector2(1, 1);

            attackBox = new OBB(this.position + boxDistance, this.rotationAngle, new Vector2(3, 6));
            attackBox.DebugColor = new Color(255, 0, 0, 128);

            //rototraslazione degli obb in base alla rotazione iniziale 
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            attackBox.Origin = Vector2.Transform(attackBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;

            status = new EntityManager(Globals.h_states, Globals.h_animations, ref this.sheet[sheetIndex]);
            status.SetOn((int)h_states.idle,(int)h_animations.idle, true, true, false);
            status = new EntityManager(status);
            cData = new List<Collision>();
            currentDungeon = dungeon;
            damage = new Damage();
            dData = new List<DamageData>();
            ent_color = Color.White; //filtro entità
            weapon_color = Color.White; //filtro arma dell' entità

            magicOrigin1 = new Vector2(40, 14);
            magicOrigin2 = new Vector2(30, 14);

            //definizione dei vettori per la definizione dei damagedata inizializzato a 0
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

            //definizione di tutti i damage e relativi damagedata
            damage = new Damage(dam, tim, pro, eff, dur);
            damage = new Damage(dam, tim, pro, eff, dur);
            
            //definizione danni ed effetti pugno
            dam[(int)damageTypes.physical] = 3;
            damage = new Damage(dam, tim, pro, eff, dur);

            //definizione danni ed effetti calcio debug
            dam[(int)damageTypes.physical] = 1;
            pro[(int)damageEffects.mechanical] = 1;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 0;
           
            damage = new Damage(dam, tim, pro, eff, dur);
        }

        public void Drop()
        {
            
        }

        protected void Action()
        {
            if (status.IsOff((int)h_states.action))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.running);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.running);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                status.SetOff((int)h_states.idle);
                status.SetOn((int)h_states.action, 100, (int)h_animations.idle, true, true,l);
                genericAction = 2;
                speed = 0;
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

        public void PickUp(Item item)
        {
            if (!item.alreadyPicked)
            {
                item.alreadyPicked = true;
                inventory.Add(ref item);
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

        public void Shrink(float shrink_factor)
        {
            scale.X *= shrink_factor;
            scale.Y *= shrink_factor;
        }

        public void Shrink(Vector2 shrink_vector)
        {
            scale = shrink_vector;
        }

        public void Update(GameTime gameTime)
        {
            status.Update(gameTime);
            status.AutoOff();
            
            
            
            if (corpse)
            {
                boundingBox.Origin = Vector2.Zero;
                boundingBox.HalfWidths = Vector2.Zero;
            }
            else 
            {
                direction.X = (float)Math.Cos(rotationAngle);
                direction.Y = (float)Math.Sin(rotationAngle);
                direction *= speed;
                boundingCircle.Center = position;

                //origini dei box
                boundingBox.Origin = this.position;


                //aggiornamento degli axis dei box
                boundingBox.UpdateAxis(rotationAngle);

                attackBox.UpdateAxis(rotationAngle);
                attackBox.Origin = position + boxDistance;
                attackBox.Origin = Vector2.Transform(attackBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
                damageManager.Update(gameTime);
            }

            damageData.ResetOBB();

            #region controlli di stato
            if (status.IsOn((int)h_states.action))
            {
                axis.Floor();
                if (genericAction > 0) genericAction--;
            }
            else
            {
                genericAction = 0;
            }

            if (status.IsOn((int)h_states.attacking))
            {
                axis.Floor();
                if (status.GetCurrentFrame() == 3 && !fl_attack)
                {
                    damageData = new DamageData(this.position, factionId, id, damage, attackBox, null, id, type);
                    fl_attack = true;
                }
                
                if (status.CurrentAnimation == (int)h_animations.attacking)
                {
                    if (status.GetCurrentFrame() == 3 && !fl_attack)
                    {
                        damageData = new DamageData(this.position, factionId, id, damage, attackBox, null, id, type);
                        fl_attack = true;
                    }
                    else if (status.GetCurrentFrame() == 4 && !((Player)this).keyPressed.AttackYetPressed)
                        comboBreaker = true;
                    else if (status.GetCurrentFrame() == 5 && ((Player)this).keyPressed.AttackYetPressed && comboBreaker)
                    {
                        combo = true;
                        Attack();
                    }
                }
                else if (status.CurrentAnimation == (int)h_animations.attacking2)
                {
                    if (status.GetCurrentFrame() == 3 && !fl_attack)
                    {
                        damageData = new DamageData(this.position, factionId, id, damage, attackBox, null, id, type);
                        fl_attack = true;
                    }
                    else if (status.GetCurrentFrame() == 5 && !((Player)this).keyPressed.AttackYetPressed)
                        comboBreaker = true;
                    else if (status.GetCurrentFrame() == 6 && ((Player)this).keyPressed.AttackYetPressed && comboBreaker)
                    {
                        combo = true;
                        Attack();
                    }
                }

                else if (status.CurrentAnimation == (int)h_animations.attacking3)
                {
                    if (status.GetCurrentFrame() == 3 && !fl_attack)
                    {
                        damageData = new DamageData(this.position, factionId, id, damage, attackBox, null, id, type);
                        fl_attack = true;
                    }
                    else if (status.GetCurrentFrame() == 8 && !((Player)this).keyPressed.AttackYetPressed)
                        comboBreaker = true;
                    else if (status.GetCurrentFrame() == 9 && ((Player)this).keyPressed.AttackYetPressed && comboBreaker)
                    {
                        combo = true;
                        Attack();
                    }
                }
            }
            else if(status.Finished((int)h_states.attacking))
            {
                fl_attack=false;
                
                attackStage = 0;
            }

            if (status.IsOn((int)h_states.dead))
            {
                //da regolare
                axis.Height = 25;
                boundingBox.HalfWidths = new Vector2(32, boundingBox.HalfWidths.Y);
                boundingBox.Origin = position - new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle))*28;
            }

            if (status.IsOn((int)h_states.dying))
            {
            }
            else if (status.Finished((int)h_states.dying))
            {
                Corpsify();
            }

            if (status.IsOn((int)h_states.floating))
            {
                //speed = -3f;
                xMovement = true;
                axis.Height = 80;
                depth = Depths.floating;
            }
            else if (status.Finished((int)h_states.floating))
            {
                xMovement = false;
                axis.Height = 115;
                axis.Floor();
                spriteEffects = SpriteEffects.None;
            }

            if (status.IsOn((int)h_states.idle))
            {
                speed = 0;
                axis.Floor();
                depth = Depths.entity_idle;
                currentDungeon.FreeDamageId(damageData.id);
                //damageData.Reset();
                fl_attack = false;
                //c'è roba da scrivere sul danno boh...
                if (status.Old((int)h_states.idle))
                    status.StartAnimation((int)h_animations.idle, true, true, true);
                if (status.TimeOn((int)h_states.idle) % 15000 > 2000 && status.TimeOn((int)h_states.idle) % 15000 < 10000)
                    status.Bind((int)h_states.idle, (int)h_animations.idle3, true, true, false);
                if (status.TimeOn((int)h_states.idle) % 15000 > 10000 )
                    status.Bind((int)h_states.idle, (int)h_animations.idle2, false, true, false);
            }

            if (status.IsOn((int)h_states.jumping))
            {
                speed = 2;
                depth = Depths.floating;
                
                    switch (status.GetCurrentFrame())
                    {
                        case 2:
                            axis.Bottom = 10;
                            axis.Top=125;
                            break;
                        case 3:
                            axis.Bottom = 20;
                            axis.Top=135;
                            break;
                        case 4:
                            axis.Bottom = 30;
                            axis.Top=145;
                            break;
                        case 5:
                            axis.Bottom = 40;
                            axis.Top=155;
                            break;
                        case 6:
                            axis.Bottom = 50;
                            axis.Top=165;
                            break;
                        /*case 7:
                            axis.Bottom = 60;
                            axis.Top=175;
                            break;*/
                        case 8:
                            axis.Bottom = 40;
                            axis.Top=155;
                            break;
                        case 9:
                            axis.Bottom = 30;
                            axis.Top=145;
                            break;
                        case 10:
                            axis.Bottom = 20;
                            axis.Top=135;
                            break;
                        case 11:
                            axis.Bottom = 10;
                            axis.Top=125;
                            break;
                        case 12:
                            axis.Bottom = 0;
                            axis.Top = 115;
                            break;
                    }
                
            }
            else if (status.Finished((int)h_states.jumping))
            {
                axis.Floor();
                depth = Depths.entity_idle;
            }

            if (status.IsOn((int)h_states.kicking))
            {
                if (status.GetCurrentFrame() == 2 && !fl_attack)
                {
                    damageData = new DamageData(this.position, factionId, id, damage, attackBox, dealerId: id, dealerType: type);
                    fl_attack = true;
                }
                if (status.GetCurrentFrame() == 3)
                    fl_attack = false;
            }
            else if (status.Finished((int)h_states.kicking))
            {
                fl_attack = false;
            }

            if (status.IsOn((int)h_states.magic))
            {
                
                if (!ActiveCaster && skillManager.CurrentSkill==(int)skills.laser)
                    status.SetOff((int)h_states.magic);
            }
            else if (status.Finished((int)h_states.magic))
            {
                ActiveCaster = false;
            }

            if (status.IsOn((int)h_states.parrying))
            { }                

            if (status.IsOn((int)h_states.punching))
            {
                if (status.GetCurrentFrame() == 3 && !fl_attack)
                {
                    damageData = new DamageData(this.position, factionId, id, damage, attackBox, dealerId: id, dealerType: type);
                    fl_attack = true;
                }
            }
            else if (status.Finished((int)h_states.punching))
            {
                fl_attack = false;
            }

            if (status.IsOn((int)h_states.rotating))
            {
            }

            if (status.IsOn((int)h_states.running))
            {
            }

            if (status.IsOn((int)h_states.shooting))
            {
                if (status.GetCurrentFrame() == 3 && !fl_attack )
                {
                    if (hand == (int)handleables.crossbow)
                    {
                        Vector2 orig = new Vector2(position.X + (float)Math.Cos(rotationAngle) * dartOrigin.X - (float)Math.Sin(rotationAngle) * dartOrigin.Y, position.Y + (float)Math.Sin(rotationAngle) * dartOrigin.X + (float)Math.Cos(rotationAngle) * dartOrigin.Y);
                        if (inventory.getItemList[inventory.ammoCrossbow].otherP.Contains("seeking"))
                            spawned.Add(new EnemyseekingProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.enemyseeking, ref sheet, ref currentDungeon,  inventory.getItemList[inventory.ammoCrossbow]));
                        else if (inventory.getItemList[inventory.ammoCrossbow].otherP.Contains("parabolic"))
                            spawned.Add(new ParabolicProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.parabolic, ref sheet, ref currentDungeon,  inventory.getItemList[inventory.ammoCrossbow]));
                        else if (inventory.getItemList[inventory.ammoCrossbow].otherP.Contains("rotational"))
                            spawned.Add(new RotationalProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.rotational, ref sheet, ref currentDungeon, inventory.getItemList[inventory.ammoCrossbow]));
                        else
                            spawned.Add(new LinearProjectile(orig, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.linear, ref sheet, ref currentDungeon, inventory.getItemList[inventory.ammoCrossbow]));
                    }
                    else if (hand == (int)handleables.bow)
                    {
                        Vector2 orig = new Vector2(position.X + (float)Math.Cos(rotationAngle) * arrowOrigin.X - (float)Math.Sin(rotationAngle) * arrowOrigin.Y, position.Y + (float)Math.Sin(rotationAngle) * arrowOrigin.X + (float)Math.Cos(rotationAngle) * arrowOrigin.Y);
                        if (inventory.getItemList[inventory.ammoBow].otherP.Contains("seeking"))
                            spawned.Add(new EnemyseekingProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.enemyseeking, ref sheet, ref currentDungeon,  inventory.getItemList[inventory.ammoBow]));
                        else if (inventory.getItemList[inventory.ammoBow].otherP.Contains("parabolic"))
                            spawned.Add(new ParabolicProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.parabolic, ref sheet, ref currentDungeon, inventory.getItemList[inventory.ammoBow]));
                        else if (inventory.getItemList[inventory.ammoBow].otherP.Contains("rotational"))
                            spawned.Add(new RotationalProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.rotational, ref sheet, ref currentDungeon, inventory.getItemList[inventory.ammoBow]));
                        else
                            spawned.Add(new LinearProjectile(orig, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.linear, ref sheet, ref currentDungeon, inventory.getItemList[inventory.ammoBow]));
                    }
                    
                    if (hand == (int)handleables.bow)
                    {
                    launched = (int)equipSlots.ammoBow;
                    }
                    else if (hand == (int)handleables.crossbow)
                    {
                        launched = (int)equipSlots.ammoCrossbow;
                    }
                    
                    fl_attack = true;
                }
            }
            else if (status.Finished((int)h_states.shooting))
            {
                fl_attack = false;
            }

            if (status.IsOn((int)h_states.throwing))
            {
                if (status.GetCurrentFrame() == 3 && !fl_attack )
                {
                    if (inventory.getItemList[inventory.throwing].otherP.Contains("seeking"))
                        spawned.Add(new EnemyseekingProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.enemyseeking, ref sheet, ref currentDungeon, inventory.getItemList[inventory.throwing]));
                    else if (inventory.getItemList[inventory.throwing].otherP.Contains("parabolic"))
                        spawned.Add(new ParabolicProjectile(position, 10, direction, power*102.4f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.parabolic, ref sheet, ref currentDungeon,  inventory.getItemList[inventory.throwing]));
                    else if (inventory.getItemList[inventory.throwing].otherP.Contains("rotational"))
                        spawned.Add(new RotationalProjectile(position, 10, direction, power, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.rotational, ref sheet, ref currentDungeon,  inventory.getItemList[inventory.throwing]));
                    else
                        spawned.Add(new LinearProjectile(position, 10, direction, 10f, Depths.floating, 10f, rotationAngle, factionId, (int)entityTypes.throwable, (int)projectileType.linear, ref sheet, ref currentDungeon, inventory.getItemList[inventory.throwing]));
                  
                    launched = (int)equipSlots.throwing;
                    power = 3;
                    fl_attack = true;
                }
            }
            else if(status.Finished((int)h_states.throwing))
            {
                fl_attack = false;
            }

            if (status.IsOn((int)h_states.walking))
            {
            }

            #endregion

            foreach (Collision c in cData.Where(c => c.id != -1))
            {
                if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                {
                    if (c.collided)
                    {
                        float d = Vector2.Subtract(position + direction, c.position).Length();
                        if (d < c.distance && axis.Bottom + 10 <= c.axis.Top && axis.Top >= c.axis.Bottom)
                            direction *= 0;
                        if (c.type == (int)entityTypes.stairs)
                            destination = c.subtype;
                        if (c.type == (int)entityTypes.item && generic)
                            PickUp((Item)c.entity);
                        cIds.Add(c.id);
                    }
                }
            }
            cIds.Clear();
            

            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id) && d.factionId!=factionId)
                {
                    dIds.Add(d.id);
                    damageManager.CalculateDamage(d.damage);
                    
                    if (alive)
                    {
                        if (damageManager.Effects((int)damageEffects.block) > 0)
                        {
                            speed = 0;
                        }
                        if (damageManager.Effects((int)damageEffects.burn) > 0)
                        {
                            ent_color = new Color(255, 0, 0, 128);
                        }
                        if (damageManager.Effects((int)damageEffects.mechanical) > 0)
                        {
                            //this.Float(new Vector2((float)Math.Cos(d.oArea.AngleInRadians), (float)Math.Sin(d.oArea.AngleInRadians)), damageManager.Effects((int)damageEffects.mechanical));
                            this.Float(d.oArea.AngleInRadians, damageManager.Effects((int)damageEffects.mechanical));
                        }
                        if (damageManager.Effects((int)damageEffects.poison) > 0)
                        {
                            //damageManager.health -= dData.damage;
                            //damageManager.Reactions(effects[(int)damageEffects.poison);
                        }
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
            }
            dIds.Clear();

            if (xMovement)
            {
                if (xDirection != Vector2.Zero)
                {
                    direction += xDirection;
                    xMovement = false;
                }
                else if (xDestination != Vector2.Zero)
                {
                    direction = (xDestination - position);
                    if ((xDestination - position).Length() > xSpeed)
                    {
                        direction.Normalize();
                        direction *= xSpeed;
                    }
                }

                if (position == xDestination)
                {
                    xMovement = false;
                }
            }

            if (xTranslate)
            {
                position += xTranslation;
                xTranslate = false;
            }

            if (xRotation)
            {
                int sign;
                if (Math.Abs(destinationAngle - rotationAngle) < (float)Math.PI)
                {
                    sign = -Math.Sign(destinationAngle - rotationAngle);
                }
                else sign = -Math.Sign(destinationAngle - rotationAngle);
                if (Math.Abs(destinationAngle - rotationAngle) <= pace)
                {
                    rotationAngle = destinationAngle;
                    xRotation = false;
                }
                else
                {
                    rotationAngle = Service.CurveAngle(rotationAngle, destinationAngle, pace);
                }
            }

            if (!corpse)
            {
                direction = currentDungeon.WallContact(boundingCircle, direction);
                position += direction;
                graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
                graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;

                oldPosition = position;     //salvataggio della vecchia posizione
                direction *= 0;
            }

            int k = 0;
            for (int i = 0; i < Globals.h_states; i++)
            {
                if (status.IsOff(i))
                {
                    k++;
                }
            }
            if (k == Globals.h_states)
            {
                Idle();
            }

            if (damageManager.health <= 0 && !(status.IsOn((int)h_states.dying) || status.IsOn((int)h_states.dead)))
            {
                this.Die((int)deathCauses.generic);
            }
            cData.Clear();
            dData.Clear();

            if (shadow != null)
            {
                shadow.Angle = -rotationAngle;
                shadow.Position = new Vector2(position.X - Globals.camera[0].Left, -(position.Y - Globals.camera[0].Top));
                shadow.Axis = axis;
            }
        }

        public void Draw(Rectangle camera)
        {
           // attackBox.Draw(camera, Depths.boxes);
           // boundingCircle.Draw(camera);
            //calcolare il colore dell entità in base alla colorazione delle torce
            Vector2 pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
            Globals.IAmanager.Intensity((IEntity)this);
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, spriteEffects, depth + (float)id / Globals.max_entities);
            if( hand == (int)handleables.sword )
                Globals.spriteBatch.Draw(handSheet[this.hand].sourceBitmap, pos, handSheet[this.hand].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, handSheet[this.hand].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, spriteEffects, depth-0.0001f);
            //immagine sorgente, posizione su schermo, rettangolo del frame desiderato, colore, angolo di rotazione, centro di rotazione, fattore di scala, effetti, profondità
            if (hand == (int)handleables.crossbow)
                Globals.spriteBatch.Draw(handSheet[this.hand].sourceBitmap, pos, handSheet[this.hand].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle, handSheet[this.hand].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, spriteEffects, depth-0.0001f);
            if (status.CurrentAnimation==(int)h_animations.magic1)
            {
                Globals.spriteBatch.Draw(sheet[(int)sheetIndexes.magictrail].sourceBitmap, pos, sheet[(int)sheetIndexes.magictrail].Frame(0, status.GetCurrentFrame()), Color.Red, rotationAngle, sheet[(int)sheetIndexes.magictrail].GetRotationCenter(0, status.GetCurrentFrame()), scale, spriteEffects, depth);
            }
            

            if (this.hand != (int)handleables.nothing)
            {
                //Globals.spriteBatch.Draw(handSheet[this.hand].SourceBitmap, pos, handSheet[this.hand].frame(animation, frame[animation]), weapon_color, rotationAngle, handSheet[this.hand].getRotationCenter(animation, frame[animation]), scale, SpriteEffects.None, depth + 0.1f);
            }
        }
        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
            attackBox.Draw(camera, Depths.boxes);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 25);

            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString() + "danno da taglio: " + damage.getDamage[1].ToString();

            //String dd;
            //dd = "damageData: " + "damageData[0]: " + this.damage[0].Total().ToString() + " damageData[1]: " + this.damage[1].Total().ToString() + " damageData[2]: " + this.damage[2].Total().ToString() + " damageData[3]: " + this.damage[3].Total().ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
            //Globals.spriteBatch.DrawString(debugFont, dd, pos+new Vector2(0, 20), Color.Black, 0.0f, new Vector2(0), 2.5f, SpriteEffects.None, 1f);
            //Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Red, 0f, new Vector2(0), 2f, SpriteEffects.None, 1f);
            
        }

        #region actions

        public void Attack()
        {
            List<int> l = new List<int>();
            if (status.IsOff((int)h_states.attacking))
            {
                boxDistance = new Vector2(20, 0);
                attackBox.HalfWidths = new Vector2(5,14);

                fl_attack = false;
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.kicking);
                //l.Add((int)h_states.rotating);
                l.Add((int)h_states.running);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.action);
            }
            switch (attackStage)
            {
                case 0:
                    if ((status.CurrentAnimation != (int)h_animations.attacking && status.CurrentAnimation != (int)h_animations.attacking2 && status.CurrentAnimation != (int)h_animations.attacking3) || (status.CurrentAnimation == (int)h_animations.attacking3 && status.GetCurrentFrame() == 9))
                    {
                        int i = status.GetCurrentFrame();
                        status.SetOn((int)h_states.attacking, sheet[sheetIndex].GetTotalDuration((int)h_animations.attacking), (int)h_animations.attacking, false, true, l);
                        attackStage++;
                        attackStage %= 3;
                    }
                    break;
                case 1:
                    if (status.GetCurrentFrame() == 5)
                    {
                        status.SetOff((int)h_states.attacking);
                        status.SetOn((int)h_states.attacking, sheet[sheetIndex].GetTotalDuration((int)h_animations.attacking2), (int)h_animations.attacking2, false, true, l);
                        attackStage++;
                        attackStage %= 3;
                    }
                    break;
                case 2:
                    if (status.GetCurrentFrame() == 6)
                    {
                        status.SetOff((int)h_states.attacking);
                        status.SetOn((int)h_states.attacking, sheet[sheetIndex].GetTotalDuration((int)h_animations.attacking3), (int)h_animations.attacking3, false, true, l);
                        attackStage++;
                        attackStage %= 3;
                    }
                    break;
               /* default:
                    status.SetOn((int)h_states.attacking, sheet[sheetIndex].GetTotalDuration((int)h_animations.attacking), (int)h_animations.attacking, false, true,l);
                    break;*/
            }
            
            
        }

        public void Die(int cause)
        {
            if (!status.IsLocked((int)h_states.dying))
            {
                speed = 0;
                cause_of_death = cause;
                depth = Depths.dying;
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.attacking);
                status.SetOff((int)h_states.kicking);
                status.SetOff((int)h_states.parrying);
                status.SetOff((int)h_states.shooting);
                status.SetOff((int)h_states.throwing);
                status.SetOff((int)h_states.jumping);
                status.SetOff((int)h_states.magic);
                status.SetOff((int)h_states.punching);
                status.SetOff((int)h_states.rotating);
                status.SetOff((int)h_states.floating);

                l.Add((int)h_states.action);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.idle);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.floating);
                status.SetOn((int)h_states.dying, sheet[sheetIndex].GetTotalDuration((int)h_animations.dying), (int)h_animations.dying, false, true,l);
            }
        }

        public void Corpsify()
        {
            if (!status.IsLocked((int)h_states.dead))
            {
                axis.Floor();
                this.alive = false;
                this.depth = Depths.corpse;
                corpse = true;
                boundingBox.HalfWidths = new Vector2(32, boundingBox.HalfWidths.Y);
                boundingBox.Origin = position - new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle)) * 28;
                

                List<int> l = new List<int>();
                status.SetLock((int)h_states.walking);
                status.SetLock((int)h_states.idle);
                status.SetLock((int)h_states.running);
                status.SetLock((int)h_states.attacking);
                status.SetLock((int)h_states.kicking);
                status.SetLock((int)h_states.parrying);
                status.SetLock((int)h_states.shooting);
                status.SetLock((int)h_states.throwing);
                status.SetLock((int)h_states.jumping);
                status.SetLock((int)h_states.magic);
                status.SetLock((int)h_states.punching);
                status.SetLock((int)h_states.rotating);
                status.SetLock((int)h_states.floating);
                status.SetOn((int)h_states.dead, (int)h_animations.dead, true, true);
            }
        }

        public void Float(float angle, int duration)
        {
            if (status.IsOff((int)h_states.floating))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.attacking);
                status.SetOff((int)h_states.kicking);
                status.SetOff((int)h_states.parrying);
                status.SetOff((int)h_states.shooting);
                status.SetOff((int)h_states.throwing);
                status.SetOff((int)h_states.jumping);
                status.SetOff((int)h_states.magic);
                status.SetOff((int)h_states.punching);
                status.SetOff((int)h_states.rotating);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.idle);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.dying);
                l.Add((int)h_states.dead);
                status.SetOn((int)h_states.floating, duration, (int)h_animations.floating, true, true, l);
                speed = 2f;
                rotationAngle = angle;
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
        }

        public void Float(Vector2 direction, int duration)
        {
            if (status.IsOff((int)h_states.floating))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.attacking);
                status.SetOff((int)h_states.kicking);
                status.SetOff((int)h_states.parrying);
                status.SetOff((int)h_states.shooting);
                status.SetOff((int)h_states.throwing);
                status.SetOff((int)h_states.jumping);
                status.SetOff((int)h_states.magic);
                status.SetOff((int)h_states.punching);
                status.SetOff((int)h_states.rotating);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.idle);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.dying);
                l.Add((int)h_states.dead);
                status.SetOn((int)h_states.floating, duration, (int)h_animations.floating, true, true, l);
                speed = 2f;
                Vector2 d = direction;
                d.Normalize();
                rotationAngle = (float)(Math.Atan2(d.Y, d.X)+Math.PI);
                ExternalShift(direction);
            }
        }

        public void Jump()
        {
            if (status.IsOff((int)h_states.jumping))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.action);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);


                status.SetOn((int)h_states.jumping, sheet[sheetIndex].GetTotalDuration((int)h_animations.jumping), (int)h_animations.jumping, false, true, l,true);
                
            }
        }

        public void Kick()
        {
            if (status.IsOff((int)h_states.kicking))
            {
                speed = 0;
                dam[(int)damageTypes.physical] = 3;
                damage = new Damage(dam, tim, pro, eff, dur);

                //definizione danni ed effetti calcio debug
                dam[(int)damageTypes.physical] = 5;
                pro[(int)damageEffects.mechanical] = 1;
                eff[(int)damageEffects.mechanical] = 0;
                dur[(int)damageEffects.mechanical] = 500;

                damage = new Damage(dam, tim, pro, eff, dur);

                boxDistance = new Vector2(16, 7);
                attackBox.HalfWidths = new Vector2(10, 4);

                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.parrying);
                
                l.Add((int)h_states.magic);
                l.Add((int)h_states.action);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.rotating);

                status.SetOn((int)h_states.kicking, sheet[sheetIndex].GetTotalDuration((int)h_animations.kicking), (int)h_animations.kicking, false, true,l);
            }
        }

        public void Idle()
        {
            if (!status.IsOn((int)h_states.idle) || !status.IsLocked((int)h_states.idle))
            {
                for (int i = 0; i < Globals.h_states;i++)
                {
                        status.SetOff(i);
                }
                status.SetOn((int)h_states.idle, (int)h_animations.idle, true, true, false);
            }
            speed = 0;
            angularSpeed = MathHelper.Pi / 32;
            axis.Floor();
        }

        public void Parry()
        {
            if (!status.IsLocked((int)h_states.parrying))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.idle);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.action);
                status.SetOn((int)h_states.parrying, sheet[sheetIndex].GetTotalDuration((int)h_animations.parrying), (int)h_animations.parrying, false, true,l);
            }
        }

        public void Punch()
        {
            if (status.IsOff((int)h_states.punching))
            {
                speed = 0;
                dam[(int)damageTypes.physical] = 3;
                damage = new Damage(dam, tim, pro, eff, dur);

                //definizione danni ed effetti calcio debug
                dam[(int)damageTypes.physical] = 205;
                pro[(int)damageEffects.mechanical] = 1;
                eff[(int)damageEffects.mechanical] = 0;
                dur[(int)damageEffects.mechanical] = 500;

                damage = new Damage(dam, tim, pro, eff, dur);

                boxDistance = new Vector2(18,11);
                attackBox.HalfWidths = new Vector2(16, 5);

                List<int> l = new List<int>();
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.walking);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.magic);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.action);

                status.SetOn((int)h_states.punching, sheet[sheetIndex].GetTotalDuration((int)h_animations.punching), (int)h_animations.punching, false, true,l);
                //damageData = new DamageData(currentDungeon.AssignDamageId(), damage, this.boxList[(int)h_damage.punch]);
            }
        }
        
        public void Run(int sign)
        {
            if (!status.IsLocked((int)h_states.running) && !status.IsOn((int)h_states.running))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.idle);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.parrying);
                status.SetOn((int)h_states.running, sheet[sheetIndex].GetTotalDuration((int)h_animations.running), (int)h_animations.running,false, true, l);
                speed = sign;
            }
        }

        public void Shoot()
        {
            if (!status.IsOn((int)h_states.shooting) && !status.IsLocked((int)h_states.shooting))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.walking);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.rotating);
                l.Add((int)h_states.running);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.action);
                l.Add((int)h_states.magic);
                speed = 0;
                status.SetOn((int)h_states.shooting, sheet[sheetIndex].GetTotalDuration((int)h_animations.shooting), (int)h_animations.shooting, false, true,l);
            }
        }

        public void Skill(float power=0)
        {
            if (!status.IsOn((int)h_states.magic) && !status.IsLocked((int)h_states.magic))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.idle);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.walking);
                l.Add((int)h_states.attacking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.throwing);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.action);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.action);
                l.Add((int)h_states.idle);
                ActiveCaster = true;
                speed = 0;
                int anim = skillManager.getAnimation();
                switch (skillManager.CurrentSkill)
                {
                    case (int)skills.laser: 
                        status.SetOn((int)h_states.magic, -1/*sheet[sheetIndex].GetTotalDuration(anim)*/,  anim, true, true,l);
                        angularSpeed = MathHelper.Pi / 256;
                        break;
                    case (int)skills.firebolt:
                        status.SetOn((int)h_states.magic, sheet[sheetIndex].GetTotalDuration(anim),  anim, true, true, l);
                        break;
                    case (int)skills.shield:
                        status.SetOn((int)h_states.magic, sheet[sheetIndex].GetTotalDuration(anim),  anim, false, true, l);
                        break;
                    default:
                        status.SetOn((int)h_states.magic, sheet[sheetIndex].GetTotalDuration(anim),  anim, false, true, l);
                        break;
                }
                List<IEntity> magic = new List<IEntity>();
                
                magic = skillManager.createSkill(power,rotationAngle, position, ref currentDungeon, ((ICaster)this));
                foreach (IEntity e in magic)
                {
                    spawned.Add( e );
                    if (e is IRemoteEntity)
                       remote = (IRemoteEntity)magic;
                }
            }
        }

        public void Throw()
        {
            if (status.IsOff((int)h_states.throwing))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.walking);
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.rotating);
                status.SetOff((int)h_states.idle);
        

                l.Add((int)h_states.attacking);
                l.Add((int)h_states.jumping);
                l.Add((int)h_states.kicking);
                l.Add((int)h_states.punching);
                l.Add((int)h_states.running);
                l.Add((int)h_states.shooting);
                l.Add((int)h_states.action);
                l.Add((int)h_states.parrying);
                l.Add((int)h_states.walking);
                l.Add((int)h_states.action);
                l.Add((int)h_states.magic);
                speed = 0;
                status.SetOn((int)h_states.throwing, sheet[sheetIndex].GetTotalDuration((int)h_animations.throwing), (int)h_animations.throwing, false, true,l);
            }

        }

        public void Turn(float sign)
        {
                status.SetOn((int)h_states.rotating,(int)h_animations.idle,false,false);
                rotationAngle += angularSpeed * sign;
        }

        public void DeTurn()
        {
            if (status.IsOn((int)h_states.rotating))
            {
                status.SetOff((int)h_states.rotating);
            }
        }

        public void Nomovement()
        {
            status.SetOff((int)h_states.walking);
            status.SetOff((int)h_states.running);
            //da rivedere
            if(status.IsOff((int)h_states.floating))
                speed = 0;
        }



        public void Walk(int sign)
        {
            if (!status.IsOn((int)h_states.walking) && !status.IsLocked((int)h_states.walking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)h_states.running);
                status.SetOff((int)h_states.idle);
                
                status.SetOn((int)h_states.walking, sheet[sheetIndex].GetTotalDuration((int)h_animations.walking), (int)h_animations.walking, true, true,l);
                speed = sign;
            }
        }

        #endregion

        #region reactions

        #endregion
        #region properties

        public bool ActiveCaster
        {
            get;
            protected set;
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

        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
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

        public float getHealth
        {
            get { return health; }
        }

        /// <summary>
        /// Ottiene l'angolo di rotazione rispetto al versore (1,0)
        /// </summary>
        public float getRotationAngle
        {
            get { return rotationAngle; }
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
            get { return -1; }
        }

        /// <summary>
        /// Ottiene il tipo dell' entità
        /// </summary>
        public int getType
        {
            get { return type; }
        }

        public Krypton.ShadowHull Shadow
        { get { return shadow; } set { shadow = value; } }

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

        public Vector2 magicOrigin1
        {
            get;
            private set;
        }

        public Vector2 magicOrigin2
        {
            get;
            private set;
        }
        #endregion
     
    }
}
