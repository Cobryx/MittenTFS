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

    public class Spiderbot : Monster, IShadow,IDamageble
    {
        bool gas = false;
        float from;
        float to;
        float step;
        bool meanwhileWalk;
        //istanziazione dei vettori per la definizione di damagedata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];
        public Spiderbot(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
             : base(position,radius,direction,speed,depth,health,rotation,type, ref sheet, ref dungeon)
        {
            //base.shadow = Krypton.ShadowHull.CreateCircle(14, 10); //definizione dell ombra, da rivedere con un poligono più dettagliato
            this.boundingBox = new OBB(new Vector2(this.position.X - 5, this.position.Y), this.rotationAngle, new Vector2(18, 13));
            base.cCheckBox = new OBB(new Vector2(this.position.X + 95, this.position.Y), this.rotationAngle, new Vector2(100, 13));

            base.sheetIndex = (int)sheetIndexes.spiderbot;
            axis = new VAxis(1, 48);

            shadow = Krypton.ShadowHull.CreateCircle(14, 10);
            shadow.Axis = axis;
            Globals.krypton.Hulls.Add(shadow);

            //definizione dei cerchi collidenti
            boundingCircle = new Circle(this.position, 5);
            meleeCircle = new Circle(this.position, 20);
            rangeCircle = new Circle(this.position, 100);
            specialCircle = new Circle(this.position, 300);
            inSightCircle = new Circle(this.position, 3000);
            seekingCircle = new Circle(this.position, 2000);
          
            boxList = new OBB[1];
            boxList[0] = new OBB(this.position + new Vector2(30,1), this.rotationAngle, new Vector2(6, 6));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            boxList[0].DebugColor = new Color(0, 255, 0, 128);
            cCheckBox.DebugColor = Color.Yellow;

            //rototraslazione degli obb in base alla rotazione iniziale 
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            //cCheckBox.Origin = Vector2.Transform(cCheckBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;

            damageManager = new DamageManager(health);
            status = new EntityManager(Globals.sp_states, Globals.sp_animations, ref sheet[sheetIndex]);
            
            //Istanziazione a 0 per tutti i vettori che definiscono damagedata
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

            graphicOccupance = new Rectangle((int)position.X - 32, (int)position.Y - 32, 64, 64);

            //graphicsOccupanceDraw();
            //status.SetOn(0, 0, true, true);
            status.SetOn((int)sp_states.idle, (int)sp_animations.idle, true, true);
        }

        #region actions

        public void Attack()
        {
            if (status.IsOff((int)sp_states.attacking))
            {
                status.SetOff((int)sp_states.walking);
                status.SetOff((int)sp_states.idle);
                status.SetOff((int)sp_states.rotating);
                status.SetOn((int)sp_states.attacking, sheet[sheetIndex].GetTotalDuration((int)sp_animations.attacking), (int)sp_animations.attacking, false, true,null);
            }
        }

        public void Delay(int time)
        {
            if (status.IsOff((int)sp_states.delayed))
            {
                List<int> l = new List<int>();
                l.Add((int)sp_states.walking);
                l.Add((int)sp_states.attacking);
                status.SetOff((int)sp_states.attacking);
                status.SetOn((int)sp_states.delayed, time, (int)sp_animations.idle, false, true,l);
            }
        }

        public void Die(int cause)
        {
            if (status.IsOff((int)sp_states.dying))
            {

                speed = 0;
                cause_of_death = cause;
                alive = false;
                List<int> l = new List<int>();
                l.Add((int)sp_states.attacking);
                l.Add((int)sp_states.idle);
                l.Add((int)sp_states.rotating);
                l.Add((int)sp_states.walking);
                status.SetOn((int)sp_states.dying, sheet[sheetIndex].GetTotalDuration((int)sp_animations.dying), (int)sp_animations.dying, false, true,l);
            }
        }

        public void Idle()
        {
            if (!status.IsOn((int)sp_states.idle))
            {
                status.SetOff((int)sp_states.walking);
                status.SetOff((int)sp_states.attacking);
                status.SetOn((int)sp_states.idle, (int)sp_animations.idle, true, true);
                speed = 0;
            }
        }

        public void Turn(float from, float to, float step, bool meanwhileWalk)
        {
            this.from = from;
            this.to = to;
            this.step = step;
            this.meanwhileWalk = meanwhileWalk;
            rotationAngle = Service.CurveAngle(from, to, step);
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle - from)) + position;
            boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle - from)) + position;
            speed = 0f;



            if (!status.IsOn((int)sp_states.rotating) && !status.IsOn((int)sp_states.walking))
                {
                    status.SetOff((int)sp_states.idle);
                    status.SetOff((int)sp_states.attacking);
                    status.SetOff((int)sp_states.walking);
                    status.SetOn((int)sp_states.rotating, sheet[sheetIndex].GetTotalDuration((int)sp_animations.walking), (int)sp_animations.walking, false, true);

                    //cCheckBox.Origin = Vector2.Transform(cCheckBox.Origin - position, Matrix.CreateRotationZ(rotationAngle - from)) + position;
                    //if (meanwhileWalk)
                    // speed = 0.4f;
                    //  else

                
            }
        }

        public void Walk(int sign)
        {
            if (!status.IsOn((int)sp_states.rotating) && !status.IsOn((int)sp_states.walking))
            {
                status.SetOff((int)sp_states.idle);
                status.SetOff((int)sp_states.rotating);
                status.SetOn((int)sp_states.walking, sheet[sheetIndex].GetTotalDuration((int)sp_animations.walking), (int)sp_animations.walking, false, true);
                speed = 0.7f * sign;
            }
        }

        #endregion

        public override void Update(GameTime gameTime)
        {
            if (alive)
            {

            }
            else
            {
                //updatable = false;
                boundingBox.Origin = Vector2.Zero;
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

            if (currentDungeon.WallContact(cCheckBox))
                requiredPath = true;
            else
            {
                requiredPath = false;
            }

            if (target == null && alive)
                Idle();
            else if (target != null && pathToFollow==null && status.IsOff((int)sp_states.delayed) && alive)
            {
                if (OBB.Intersects(target.getBoundingBox, boxList[0]))
                    Attack();
              
                else if (Circle.intersect(target.getBoundingCircle, this.seekingCircle))
                {
                    desiredAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1);
                    if (Math.Abs(desiredAngle - rotationAngle) > Math.PI / 50)
                    {
                        if (Math.Abs(desiredAngle - rotationAngle) < Math.PI )
                            Turn(rotationAngle, desiredAngle, 0.01f,true);
                        else
                            Turn(rotationAngle, desiredAngle, 0.01f, false);
                    }
                    else if (Math.Abs(rotationAngle - desiredAngle) > Math.PI / 50)
                    {
                        if (Math.Abs(desiredAngle - rotationAngle) < Math.PI)
                            Turn(desiredAngle, rotationAngle, 0.01f, true);
                        else
                            Turn(desiredAngle, rotationAngle, 0.01f, false);
                    }
                    else if (Math.Abs(desiredAngle - rotationAngle) < Math.PI / 50 || Math.Abs(rotationAngle - desiredAngle) < Math.PI / 50)
                    {
                           Walk(1);
                    }
                }
                
            }
            else if (pathToFollow != null && status.IsOff((int)sp_states.delayed) && alive)
            {
                if (OBB.Intersects(target.getBoundingBox, boxList[0]))
                {
                    Attack();
                }
                int wOffset=0;
                if (Circle.intersect(boundingCircle, pathToFollow[w]))
                {
                    wOffset = 1;
                }
                //else if (!Circle.intersect(target.getBoundingCircle, this.seekingCircle))
                {
                    //turnica solo all assegnazione di un nuovo waypoint ossia quando waypoint [0] viene eliminato
                    desiredAngle = (float)Math.Atan2((position.Y - pathToFollow[w + wOffset].Center.Y) * -1, (position.X - pathToFollow[w + wOffset].Center.X) * -1);
                    if (Math.Abs(desiredAngle - rotationAngle) > Math.PI / 50)
                    {
                            Turn(rotationAngle, desiredAngle, 0.01f, false);
                    }
                    else if (Math.Abs(rotationAngle - desiredAngle) > Math.PI / 50)
                    {
                            Turn(rotationAngle, desiredAngle, 0.01f, false);
                    }
                    else if (Math.Abs(desiredAngle - rotationAngle) < Math.PI / 50 || Math.Abs(rotationAngle - desiredAngle) < Math.PI / 50)
                    {
                        if (status.IsOff((int)sp_states.delayed))
                        {
                            Walk(1);
                        }
                    }
                }
                if (pathToFollow[w].Contains(this.boundingCircle))
                {
                    if (w != pathToFollow.Count - 1)
                    {
                        w++;
                    }
                    else pathToFollow = null;
                }
            }
     
           

           
            
            boundingBox.UpdateAxis(rotationAngle);
            boxList[0].UpdateAxis(rotationAngle);

            damageManager.Update(gameTime);

            #region controlli di stato

            if (status.IsOn((int)sp_states.delayed))
            {
            }
            else if (status.Finished((int)sp_states.delayed))
            {
            }

            if (status.IsOn((int)sp_states.idle))
            {
                speed = 0;
                currentDungeon.FreeDamageId(damageData.id);
            }
            else if (status.Finished((int)sp_states.idle))
            {
            }

            if (status.IsOn((int)sp_states.dead))
            {
                alive = false;
            }
            else if (status.Finished((int)sp_states.dead))
            {
                updatable = false;
            }

            if (status.IsOn((int)sp_states.dying))
            {
            }
            if (status.Finished((int)sp_states.dying))
            {
                depth = Depths.corpse;
                status.SetLock((int)sp_states.dying);
                status.SetOn((int)sp_states.dead, (int)sp_animations.dead, true, true, true);
            }

            if (status.IsOn((int)sp_states.attacking))
            {
                speed = 0;
                if (!gas && status.GetCurrentFrame() == 2)
                {
                    spawned.Add(new Explosion(this.boxList[0].Origin, 10, this.depth, 0.02f, (int)explosion_types.gas, this.rotationAngle, (int)entityTypes.explosion, ref sheet, ref currentDungeon));  //debug:rimettere tipo gas
                    gas = true;
                }
            }
            else if (status.Finished((int)sp_states.attacking))
            {
                gas = false;
                Delay(1000);
            }

            if (status.IsOn((int)sp_states.rotating))
            {
                
            }

            if (status.IsOn((int)sp_states.walking))
            {
                //status.CurrentAnimation=(int)sp_animations.walking;
                currentDungeon.FreeDamageId(damageData.id);
                //damage data
            }
            else if (status.Finished((int)sp_states.walking))
            {

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
                            if (d < c.distance && axis.Bottom <= c.axis.Top && axis.Top >= c.axis.Bottom)
                                direction *= 0;
                            cIds.Add(c.id);
                        }
                    }
                }
                cIds.Clear();

                foreach (DamageData d in dData.Where(d => d.id != -1))
                {
                    if (!dIds.Contains(d.id))
                    {
                        dIds.Add(d.id);
                        if (factionId != d.factionId)
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
            }

            direction = currentDungeon.WallContact(boundingCircle, direction);
            position += direction;
            if (alive)
            {
            boundingBox.Origin += direction;
            boxList[0].Origin += direction;
            //cCheckBox.Origin += direction;
            }

            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;
            

            oldPosition = position;     //salvataggio della vecchia posizione
            //direction *= 0;

            int k = 0;
            for (int i = 0; i < Globals.sp_states; i++)
            {
                if (status.IsOff(i))
                {
                    k++;
                }
            }
            if (k == Globals.sp_states)
            {
                this.Idle();
            }

            if (damageManager.health <= 0 && alive)
            {
                Die((int)deathCauses.generic);
            }

            cData.Clear();
            dData.Clear();

            base.Update(gameTime);
        }

     

        public override void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 35);
            String s;
            s = "Rotazione desiderata: " + this.desiredAngle.ToString();
            String q;
            q=status.IsOn((int)sp_states.attacking).ToString();
            q+=status.IsOn((int)sp_states.dead).ToString();
            q+=status.IsOn((int)sp_states.dying).ToString();
            q+=status.IsOn((int)sp_states.idle).ToString();
            q+=status.IsOn((int)sp_states.rotating).ToString();
            q+=status.IsOn((int)sp_states.walking).ToString();
            q += " Health " + damageManager.health;
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1.5f, SpriteEffects.None, 0f);
            Globals.spriteBatch.DrawString(debugFont, q, pos+new Vector2(0, 10), Color.Black, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
            base.DrawDebug(camera, ref debugFont);
        }
    }
}
