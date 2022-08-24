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
    public class Zombie : Monster ,IShadow,IAttacker,IDamageble
    {
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];
        public Zombie(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
        : base(position,radius,direction,speed,depth,health,rotation,type, ref sheet, ref dungeon)
        {
            randMilliSec = new Random(id);
            base.droppable[randMilliSec.Next(0, 90)] = 1f;
            /*base.droppable[dic.reverseItemIndex["Bag of coins"]] = 1;
            base.droppable[dic.reverseItemIndex["Magical ring"]] = 1;
            base.droppable[dic.reverseItemIndex["Sword of Dzigarbahad"]] = 1;*/
            axis = new VAxis(1, 100);

            shadow = Krypton.ShadowHull.CreateCircle(14, 10);
            shadow.Axis = axis;
            Globals.krypton.Hulls.Add(shadow);

            Random r = new Random(id);
            switch (r.Next(1, 4))
            {
                case 1: sheetIndex = (int)sheetIndexes.zombie1; break;
                case 2: sheetIndex = (int)sheetIndexes.zombie2; break;
                case 3: sheetIndex = (int)sheetIndexes.zombie3; break;
                default: sheetIndex = (int)sheetIndexes.zombie1; break;
            }
           

            boundingCircle = new Circle(position, 20);
            meleeCircle = new Circle(position, 50);
            rangeCircle = new Circle(position, 100);
            specialCircle = new Circle(position, 300);
            inSightCircle = new Circle(position, 3000);
            seekingCircle = new Circle(position, 2000);
            boundingBox = new OBB(position, rotationAngle, new Vector2(10, 16));
            boxList = new OBB[1];
            boxList[0] = new OBB(position + new Vector2(15,1), rotationAngle, new Vector2(5, 12));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            boxList[0].DebugColor = new Color(0, 255, 0, 128);

            //rototraslazione degli obb in base alla rotazione iniziale 
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle )) + position;
            boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle )) + position;

            damageManager = new DamageManager(health);
            status = new EntityManager(Globals.z_states, Globals.z_animations, ref sheet[sheetIndex]);
            status.SetOn((int)z_states.idle, (int)z_animations.idle,true,true);
            //instanziazione a 0 per la definizione di damagedata
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
            dam[(int)damageTypes.physical] = 1;
            pro[(int)damageEffects.block] = 1f;
            eff[(int)damageEffects.block] = 0;
            dur[(int)damageEffects.block] = 50;
            pro[(int)damageEffects.illness] = 0.1f;
            eff[(int)damageEffects.illness] = 0.002f;
            dur[(int)damageEffects.illness] = sizeof(int); //sostanzialmente infinito

            base.cCheckBox = new OBB(new Vector2(this.position.X + 95, this.position.Y), this.rotationAngle, new Vector2(100, 16));

            damage = new Damage(dam, tim, pro, eff, dur);
            damageData = new DamageData(position, factionId, -1, damage, boxList[0], dealerId: id, dealerType: type);
        }
        #region actions
        
        public void Attack()
        {
            if (status.IsOff((int)z_states.attacking))
            {
                speed = 0;
                List<int> l = new List<int>();
                status.SetOff((int)z_states.walking);
                status.SetOff((int)z_states.idle);
                status.SetOn((int)z_states.attacking, sheet[sheetIndex].GetTotalDuration((int)z_animations.attacking), (int)z_animations.attacking, false, true, l);
            }
        }

        public void Delay(int time)
        {
            if (status.IsOff((int)z_states.delayed))
            {
                List<int> l = new List<int>();
                l.Add((int)z_states.attacking);
                status.SetOn((int)z_states.delayed, time, (int)z_animations.idle, false, false, l);
            }
        }

        public void Die(int cause)
        {
            if (!status.IsLocked((int)z_states.dying))
            {
                alive = false;
                speed = 0;
                cause_of_death = cause;
                List<int> l = new List<int>();
                l.Add((int)z_states.attacking);
                l.Add((int)z_states.idle);
                l.Add((int)z_states.rotating);
                l.Add((int)z_states.walking);
                status.SetOn((int)z_states.dying, sheet[sheetIndex].GetTotalDuration((int)z_animations.dying), (int)z_animations.dying, true, true, l);
            }
        }

        public void Idle()
        {
            List<int> l = new List<int>();
            if (!status.IsOn((int)z_states.idle))
            {
                speed = 0;
                status.SetOff((int)z_states.walking);
                status.SetOff((int)z_states.attacking);
                l.Add((int)z_states.attacking);
                l.Add((int)z_states.idle);
                l.Add((int)z_states.rotating);
                l.Add((int)z_states.walking);
                status.SetOn((int)z_states.idle, (int)z_animations.idle, true, true);
            }
        }

        public void Turn(float from, float to, float step)
        {
            //fl_attack = false;
            if (!status.IsLocked((int)z_states.rotating))
            {
                status.SetOff((int)z_states.idle);
                rotationAngle = Service.CurveAngle(from, to, step);
                boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle-from)) + position;
                //status.SetOn((int)z_states.rotating,(int)z_animations.idle, true, false);
            }
        }

        public void Walk(int sign)
        {
            //fl_attack = false;
            if (!status.IsOn((int)z_states.walking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)z_states.idle);
                status.SetOn((int)z_states.walking, sheet[sheetIndex].GetTotalDuration((int)z_animations.walking), (int)z_animations.walking, false, true, l);
                speed = 0.5f * sign;
            }
        }

#endregion

        public override void Update(GameTime gameTime)
        {
            //da rivedere
            //if (!alive)
            //    updatable = false;

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



            if (alive)
            {
                if (target == null)
                {
                    Idle();
                }
                else if (target != null && Circle.intersect(target.getBoundingCircle, this.inSightCircle))
                {
                    desiredAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1);

                    if (Circle.intersect(target.getBoundingCircle, seekingCircle))
                    {
                        if (Circle.intersect(target.getBoundingCircle, meleeCircle))
                        {
                            if (OBB.Intersects(target.getBoundingBox, boxList[0]))
                                Attack();
                            else
                                Turn(rotationAngle, desiredAngle, 0.05f);
                        }
                        else
                        {
                            Turn(rotationAngle, desiredAngle, 0.05f);
                            Walk(1);
                        }
                    }
                    else
                        Turn(rotationAngle, desiredAngle, 0.05f);
                }
                else
                    Idle();
            }
            
            boundingBox.UpdateAxis(rotationAngle);
            boxList[0].UpdateAxis(rotationAngle); 

            damageManager.Update(gameTime);

            #region controlli di stato

            if (status.IsOn((int)z_states.delayed))
            {
            }
            else if(status.Finished((int)z_states.delayed))
            {
            }

            if (status.IsOn((int)z_states.idle))
            {
                speed = 0;
                currentDungeon.FreeDamageId(damageData.id);
                fl_attack = false;
                if (status.Old((int)z_animations.idle))
                {
                    status.Bind((int)z_states.idle, (int)z_animations.idle, true, true, false);
                }
            }

            if (status.IsOn((int)z_states.dead))
            {
                this.alive = false;
            }

            if(status.IsOn((int)z_states.dying))
            {

            }
            else if(status.Finished((int)z_states.dying))
            {
                status.SetOn((int)z_states.dead, (int)z_animations.dead, true, true);
                this.depth = Depths.corpse;
                
            }

            if(status.IsOn((int)z_states.attacking))
            {
                if (((IDamageble)target).DamageManager.Effects((int)damageEffects.block) > 0 && ((IDamageble)target).Alive)
                {
                    ((Human)target).Idle();
                    ((Human)target).ExternalShift(boxList[0].Origin, 5);
                    ((Human)target).ExternalSpin(boxList[0].AngleInRadians, (float)Math.PI / 32);
                }
                if (status.GetCurrentFrame() == 0 && !fl_attack)
                    damageData.id = id;
                else if (status.GetCurrentFrame() == 1)
                {
                    if (fl_attack)
                    {
                       
                        damage = new Damage(dam, tim, pro, eff, dur);
                        damageData = new DamageData(position, factionId, -1, damage, boxList[0], dealerId: id, dealerType: type);
                    }
                    fl_attack = true;
                    
                    dam[(int)damageTypes.physical] = 1;
                    pro[(int)damageEffects.block] = 1f;
                    eff[(int)damageEffects.block] = 0;
                    dur[(int)damageEffects.block] = 150;
                }
                else if (status.GetCurrentFrame() == 2)
                {
                    fl_attack = false;
                    dam[(int)damageTypes.physical] = 1;
                    damage = new Damage(dam, tim, pro, eff, dur);
                    damageData = new DamageData(position, factionId, -1, damage, boxList[0], dealerId: id, dealerType: type);
                    
                }
               // damageData.id = -1;
            }
            else if (status.Finished((int)z_states.attacking))
            {
                 dam[(int)damageTypes.physical] = 1;
                 pro[(int)damageEffects.block] = 1f;
                 eff[(int)damageEffects.block] = 0;
                 dur[(int)damageEffects.block] = 150;
                 damage = new Damage(dam, tim, pro, eff, dur);
                 damageData = new DamageData(position, factionId, -1, damage, boxList[0], dealerId: id, dealerType: type);
                 damageData.id = -1;
                 fl_attack = false;
                 Delay(300);
            }

            if (status.IsOn((int)z_states.rotating))
            {
                
            }

            if (status.IsOn((int)z_states.walking))
            {
                currentDungeon.FreeDamageId(damageData.id);
            }
            else if (status.Finished((int)z_states.walking))
            {
            
            }
            #endregion

            if (cData.Count > 0)
            {
                List<Vector2> translations = new List<Vector2>();
                Vector2 resultant = new Vector2();
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        if (c.collided)
                        {
                            Vector2 dVector = Vector2.Subtract(position + direction, c.position);
                            Vector2 distanceVector = Vector2.Subtract(position, c.position);
                            float d = dVector.Length();
                            //dVector.Normalize();
                            if (d < c.distance)
                            {
                                if (c.entity is Zombie)
                                {
                                    //direction *= Vector2.Dot(c.direction, direction);
                                    //direction = Vector2.Lerp(c.direction, direction, Vector2.Dot(direction, c.direction));
                                    double angleBetween = Math.Atan2(c.direction.Y - direction.Y, c.direction.X - direction.X);
                                    double offsetAngle = Math.Atan2(distanceVector.Y - direction.Y, distanceVector.X - direction.X);
                                    if (offsetAngle < Math.PI / 3)
                                    {
                                        direction = direction + (c.direction * (float)Math.Sin(angleBetween)) / (c.penetration_depth + c.distance);
                                        translations.Add((distanceVector * (float)Math.Cos(offsetAngle)) / (c.penetration_depth + c.distance));
                                    }
                                }
                                else
                                    direction *= 0;
                            }
                            cIds.Add(c.id);
                        }
                    }
                }
                foreach (Vector2 v in translations)
                    resultant += v;
                if (resultant != Vector2.Zero)
                {
                    resultant.Normalize();
                    Translate(resultant/3);
                }
                cIds.Clear();
            }
            foreach (DamageData d in dData.Where(d => d.id != -1))
            {
                if (!dIds.Contains(d.id))
                {
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

            direction = currentDungeon.WallContact(boundingCircle, direction);

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


            position += direction;
            boxList[0].Origin += direction;
            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;
            if (alive)
                boundingBox.Origin += direction;
            else
                boundingBox.Origin = Vector2.Zero;

            oldPosition = position;     //salvataggio della vecchia posizione
            //direction *= 0;

            int k = 0;
            for (int i = 0; i < Globals.z_states; i++)
            {
                if (status.IsOff(i))
                {
                    k++;
                }
            }
            if (k == Globals.z_states)
            {
                this.Idle();
            }
            
            if (damageManager.health <= 0 && !(status.IsOn((int)z_states.dying) || status.IsOn((int)z_states.dead)))
            {
                this.Die((int)deathCauses.generic);
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
            s += " Health " + damageManager.health;
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Black, 0.0f, new Vector2(0), 1.5f, SpriteEffects.None, 0f);
            base.DrawDebug(camera, ref debugFont);
        }
    }
}
