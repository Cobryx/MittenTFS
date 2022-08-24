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
    public class Banshee : Monster, ILightEntity,IDamageble,IAttacker
    {
        [NonSerialized] Krypton.Lights.Light2D light;
 
        //vettori per la definizione di damagedata
        float[] dam = new float[Globals.ndamagetypes]; //vettore danno
        int[] tim = new int[Globals.ndamagetypes]; // danno protratto nel tempo(zero per danni puntuali)
        float[] eff = new float[Globals.damage_effects]; //tipo di effetto
        float[] pro = new float[Globals.damage_effects]; //probabilità attivazione effetto
        int[] dur = new int[Globals.damage_effects]; // durata dell' effetto nel tempo
        public Banshee(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
            : base(position, radius, direction, speed, depth, health, rotation, type, ref sheet, ref dungeon)
        {
            Random r = new Random(id);

            axis = new VAxis(20, 128);
            
         
            switch (r.Next(1, 4))
            {
                case 1: sheetIndex = (int)sheetIndexes.banshee1; break;
                case 2: sheetIndex = (int)sheetIndexes.banshee2; break;
                case 3: sheetIndex = (int)sheetIndexes.banshee3; break;
                default: sheetIndex = (int)sheetIndexes.banshee1; break;
            }
            
            base.shadow = null;
            //istanziazione e definizione cerchi collidenti
            boundingCircle = new Circle(this.position, 20);
            meleeCircle = new Circle(this.position, 20);
            rangeCircle = new Circle(this.position, 100);
            specialCircle = new Circle(this.position, 300);
            inSightCircle = new Circle(this.position, 3000);
            seekingCircle = new Circle(this.position, 2000);

            
            this.boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(10, 16)); 
            boxList = new OBB[1]; //OBB per l' attivazione dell attacco
            boxList[0] = new OBB(this.position + new Vector2(18, 1), this.rotationAngle, new Vector2(5, 12));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            boxList[0].DebugColor = new Color(0, 255, 0, 128);

            //rototraslazione degli obb in base alla rotazione iniziale 
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;

            ent_color = new Color(160, 160, 160, 160);

            //gestione danni, stati e animazioni
            damageManager = new DamageManager(health); //gestione della vita dell entità affidata al damagemanager
            status = new EntityManager(Globals.b_states, Globals.b_animations, ref sheet[sheetIndex]); //gestione degli stati dell entità affidata all' entitystatemanager

            //istanziazione dei vettori per l'istanziazione del damagedata impostata a 0
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

            //definizione del damagedata del primo attacco
            dam[(int)damageTypes.spectral] = 5;
            pro[(int)damageEffects.fear] = 0.2f;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 3000;
            damage = new Damage(dam, tim, pro, eff, dur);
            damageData = new DamageData(position, factionId, -1, damage, boxList[0], dealerId: id, dealerType: type);

            //definizione resistenze e percentuale di resistenza al danno
            damageManager.ChangeModifier(0.6f, (int)damageTypes.air);
            damageManager.ChangeModifier(0f, (int)damageTypes.earth);
            damageManager.ChangeModifier(0.8f, (int)damageTypes.fire);
            damageManager.ChangeModifier(0.5f, (int)damageTypes.physical);
            damageManager.ChangeModifier(0f, (int)damageTypes.poison);

            base.cCheckBox = new OBB(new Vector2(this.position.X + 95, this.position.Y), this.rotationAngle, new Vector2(100, 16));

            light = new Krypton.Lights.Light2D();
            light.IsOn=true;
            light.Fov=MathHelper.TwoPi;
            light.Color=Color.BlueViolet;
            //light.Color = new Color(152, 102, 204);
            light.Angle=0;
            light.Intensity=0.6f;
            light.Texture = Globals.mLightTexture;
            light.Range = 100f;
            Globals.krypton.Lights.Add(light);
        }

        #region actions

        public void Attack()
        {
            if (status.IsOff((int)b_states.attacking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)b_states.walking);
                status.SetOff((int)b_states.idle);
                l.Add((int)b_states.rotating);
                l.Add((int)b_states.walking);
                speed = 0;
                status.SetOn((int)b_states.attacking, sheet[sheetIndex].GetTotalDuration((int)b_animations.attacking), (int)b_animations.attacking, false, true,l);
            }
        }

        public void Die(int cause)
        {
            if (!status.IsLocked((int)b_states.dying))
            {
                
                speed = 0;
                cause_of_death = cause;
                List<int> l = new List<int>();
                l.Add((int)b_states.attacking);
                l.Add((int)b_states.idle);
                l.Add((int)b_states.rotating);
                l.Add((int)b_states.walking);
                status.SetOn((int)b_states.dying, sheet[sheetIndex].GetTotalDuration((int)b_animations.dying), (int)b_animations.dying, false, true,l);
            }
        }

        public void Idle()
        {
            List<int> l = new List<int>();
            if (!status.IsOn((int)b_states.idle))
            {
                speed = 0;
                status.SetOff((int)b_states.walking);
                status.SetOff((int)b_states.attacking);
                status.SetOn((int)b_states.idle,(int)b_animations.idle,true,true);
                //l.Add((int)b_states.attacking);
                //l.Add((int)b_states.idle);
                //l.Add((int)b_states.rotating);
                //l.Add((int)b_states.walking);
                status.SetOn((int)b_states.idle, (int)b_animations.idle, true, false, false);
            }
        }

        public void Turn(float from, float to, float step)
        {
            //fl_attack = false;
            if (!status.IsLocked((int)b_states.rotating))
            {
                status.SetOn((int)b_states.rotating,(int)b_animations.idle,true,true);
                status.SetOff((int)b_states.idle);
                rotationAngle = Service.CurveAngle(from, to, step);
                boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle - from)) + position;
            }
        }

        public void Walk(int sign)
        {
            //fl_attack = false;
            if (!status.IsLocked((int)b_states.walking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)b_states.idle);
                status.SetOn((int)b_states.walking, (int)b_animations.walking, true, true, false);
                speed = 1.1f * sign;
            }
        }

        #endregion

        public override void Update(GameTime gameTime)
        {
            if (alive)
            {
                if (!currentDungeon.WallContact(position + direction))
                {
                    position += direction;
                    boxList[0].Origin += direction;
                    boundingBox.Origin += direction;
                }
                else
                {
                    position += direction / 2;
                    boxList[0].Origin += direction / 2;
                    boundingBox.Origin += direction / 2;
                }
            }
            else
            {
                //Updatable = false;
                Globals.krypton.Lights.Remove(light);
                light = null;
                boundingBox.Origin = Vector2.Zero;
                boundingBox.HalfWidths = Vector2.Zero;
            }

            status.Update(gameTime);
            status.AutoOff();

            meleeCircle.Center = position;
            rangeCircle.Center = position;
            specialCircle.Center = position;
            inSightCircle.Center = position;
            seekingCircle.Center = position;

            direction.X = (float)Math.Cos(rotationAngle);
            direction.Y = (float)Math.Sin(rotationAngle);
            direction *= speed;
            if (target == null)
            {
                Idle();
            }
            else if (target != null && alive)
            {
                if (Circle.intersect(target.getBoundingCircle, this.inSightCircle))
                {
                    desiredAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1);
                    this.Turn(rotationAngle, desiredAngle, 0.05f);

                    if (Circle.intersect(target.getBoundingCircle, this.seekingCircle))
                    {
                        if (Circle.intersect(target.getBoundingCircle, this.meleeCircle))
                        {
                            if (OBB.Intersects(target.getBoundingBox, boxList[0]))
                                Attack();
                        }
                        else
                            Walk(1);
                    }
                    else
                        Idle();
                }
            }
            else
                Idle();

            boundingBox.UpdateAxis(rotationAngle);
            boxList[0].UpdateAxis(rotationAngle);

            damageManager.Update(gameTime);

            #region controlli di stato

            if (status.IsOn((int)b_states.delayed))
            {

            }
            else if(status.Finished((int)b_states.delayed))
            {

            }

            if (status.IsOn((int)b_states.idle))
            {
                speed = 0;
                currentDungeon.FreeDamageId(damageData.id);
                fl_attack = false;
                //axis.Floor();
                //c'è roba da scrivere sul danno boh...
                if (status.Old((int)b_animations.idle))
                {
                    //status.StartAnimation((int)b_animations.idle, true, true, true);
                    status.Bind((int)b_states.idle, (int)b_animations.idle, true, true, false);
                }
            }

            if (status.IsOn((int)b_states.dead))
            {
                status.Bind((int)b_states.dead, (int)b_animations.dead, true, true);
                this.depth = Depths.corpse;
                axis.Height = 1;
                axis.Floor();
                corpse = true;
                
            }

            if (status.IsOn((int)b_states.dying))
            {
                
                ent_color.A = 255;
                light.Intensity = 0.65f - (float)status.GetCurrentFrame() / 20f;
            }
            if (status.Finished((int)b_states.dying))
            {
                status.SetLock((int)b_states.dying);
                status.SetOn((int)b_states.dead, (int)b_animations.dead, true, true);
                alive = false;
            }

            if (status.IsOn((int)b_states.attacking))
            {
                if (status.GetCurrentFrame() == 4 && !fl_attack)
                {
                    fl_attack = true;
                    damageData.id = id;
                }
                else
                {
                    damageData.id = -1;
                }
                speed = 0;
            }
            if (status.Finished((int)b_states.attacking))
            {
                fl_attack = false;
            }

            if (status.IsOn((int)b_states.rotating))
            {
                status.SetOff((int)b_states.rotating);
            }

            if (status.IsOn((int)b_states.walking))
            {
                //non ho idea del perché ci sia un freedamage qui
                currentDungeon.FreeDamageId(damageData.id);
            }

            #endregion

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        if (c.collided)
                        {
                            float d = Vector2.Subtract(position + direction, c.position).Length();
                            if (d < c.distance)
                            {
                                direction.X = (float)Math.Cos(rotationAngle);
                                direction.Y = (float)Math.Sin(rotationAngle);
                                direction *= speed/2;
                            }
                            cIds.Add(c.id);
                        }
                    }
                    if (c.damage_done && c.type == (int)entityTypes.human)
                    {
                        //this.Attack();
                    }
                }
                cIds.Clear();
            }
            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id))
                {
                    dIds.Add(d.id);
                    if (factionId!= d.factionId)
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

            //da ottimizzare - cercare di capire perché non è stato usato speed*=0.5f;
            

            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;

            oldPosition = position;     //salvataggio della vecchia posizione
            //direction *= 0;

            int k = 0;
            for (int i = 0; i < Globals.b_states; i++)
            {
                if (status.IsOff(i))
                    k++;
            }
            if (k == Globals.b_states)
                this.Idle();

            if (damageManager.health <= 0 && !(status.IsOn((int)b_states.dying) || status.IsOn((int)b_states.dead)))
            {
                
                this.Die((int)deathCauses.generic);
            }
            cData.Clear();
            dData.Clear();

            if (light != null)
            {
                light.X = position.X - Globals.camera[0].Left;
                light.Y = position.Y - Globals.camera[0].Top;
            }
            base.Update(gameTime);
        }

 

        public override void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 35);
            String s;
            s = "Rotazione desiderata: " + this.desiredAngle.ToString();
            s += " Health " + damageManager.health;
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1.5f, SpriteEffects.None,  Depths.entityImmaterial);
            base.DrawDebug(camera, ref debugFont);
        }



        public Krypton.Lights.Light2D Light
        {
            get { return light; }
            set { light = value; }
        }

    }
}

