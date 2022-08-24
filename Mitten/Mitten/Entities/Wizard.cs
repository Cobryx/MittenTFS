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
    public class Wizard : Monster, IShadow, ICaster,IDamageble,IAttacker
    {
        
        float rofTeleport; //timer per l' attivazione dell' abilità di teleport

        //vettori per la definizione di damagedata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        private Vector2 magicOrigin = new Vector2(20, -12);
        private int currentSkill = (int)skills.firebolt;
        private int element;
        Random r;
       
        public Wizard(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int type,
            ref SpriteSheet[] sheet, ref Dungeon dungeon)
        : base(position, radius, direction, speed, depth, health, rotation, type, ref sheet, ref dungeon)
        {
            axis = new VAxis(0, 100);

            shadow = Krypton.ShadowHull.CreateCircle(14, 10);
            shadow.Axis = axis;
            Globals.krypton.Hulls.Add(shadow);
          
            this.boundingBox = new OBB(this.position, this.rotationAngle, new Vector2(10, 16));
           
            boundingCircle = new Circle(this.position, 10);
            meleeCircle = new Circle(this.position, 20);
            rangeCircle = new Circle(this.position, 100);
            specialCircle = new Circle(this.position, 300);
            inSightCircle = new Circle(this.position, 300);
            seekingCircle = new Circle(this.position, 200);

            boxList = new OBB[1];
            boxList[0] = new OBB(this.position + new Vector2(15,1), this.rotationAngle, new Vector2(5, 12));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            
            boxList[0].DebugColor = new Color(0, 255, 0, 128);

            //rototraslazione degli obb in base alla rotazione iniziale 
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;
            boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;


            damageManager = new DamageManager(health);

            r = new Random(id);
            rofTeleport = r.Next(5000, 15000);
            currentSkill = r.Next(0, 1);
            switch (r.Next(1, 9))
            {
                case 1: sheetIndex = (int)sheetIndexes.wizard1; element = (int)damageTypes.air; break;
                case 2: sheetIndex = (int)sheetIndexes.wizard2; element = (int)damageTypes.spectral; break;
                case 3: sheetIndex = (int)sheetIndexes.wizard3; element = (int)damageTypes.water; break;
                case 4: sheetIndex = (int)sheetIndexes.wizard4; element = (int)damageTypes.energy; break;
                case 5: sheetIndex = (int)sheetIndexes.wizard5; element = (int)damageTypes.poison; break;
                case 6: sheetIndex = (int)sheetIndexes.wizard6; element = (int)damageTypes.physical; break;
                case 7: sheetIndex = (int)sheetIndexes.wizard7; element = (int)damageTypes.fire; break;
                case 8: sheetIndex = (int)sheetIndexes.wizard8; element = (int)damageTypes.earth; break;
                default: sheetIndex = (int)sheetIndexes.wizard2; element = (int)damageTypes.spectral; break;
            }

            status = new EntityManager(Globals.w_states, Globals.w_animations, ref sheet[sheetIndex]);
            status = new EntityManager(status);
            
            //instanziazione a 0 di tutti i vettori per la definizione di damagedata
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

            magicOrigin1 = new Vector2(40, 14);
            magicOrigin2 = new Vector2(30, 14);

            base.cCheckBox = new OBB(new Vector2(this.position.X +95, this.position.Y), this.rotationAngle, new Vector2(100, 16));
            base.cCheckBox.DebugColor = new Color(0, 255, 0, 128);
            base.sensorBox = new OBB(new Vector2(this.position.X, this.position.Y), this.rotationAngle, new Vector2(100, 16));
            base.sensorBox.DebugColor = new Color(255, 0, 0, 128);
        }

        #region actions

        public void Attack()
        {
            if (status.IsOff((int)w_states.attacking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)w_states.walking);
                status.SetOff((int)w_states.idle);
                l.Add((int)w_states.rotating);
                l.Add((int)w_states.walking);
                status.SetOn((int)w_states.attacking, sheet[sheetIndex].GetTotalDuration((int)w_animations.attacking), (int)w_animations.attacking, true, true,l);
                ActiveCaster = true;
                IEntity magic;
                switch (currentSkill)
                {
                    case (int)skills.firebolt:
                        if (Circle.intersect(target.getBoundingCircle, specialCircle))
                        {
                            magic = new MagicProjectile((int)damageTypes.fire, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, new Vector2(position.X + (float)Math.Cos(rotationAngle) * magicOrigin.X - (float)Math.Sin(rotationAngle) * magicOrigin.Y, position.Y + (float)Math.Sin(rotationAngle) * magicOrigin.X + (float)Math.Cos(rotationAngle) * magicOrigin.Y));
                        }
                        else magic = null;
                        break;

                    /*case (int)skills.laser:
                        magic = new Laser((int)damageTypes.energy, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, new Vector2(position.X + (float)Math.Cos(rotationAngle) * magicOrigin.X - (float)Math.Sin(rotationAngle) * magicOrigin.Y, position.Y + (float)Math.Sin(rotationAngle) * magicOrigin.X + (float)Math.Cos(rotationAngle) * magicOrigin.Y), (ICaster)this);
                        break;*/
                    /*case (int)skills.blaze:
                        magic =  new Blaze(Color.Violet, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, position, ref skillFlag);
                    case (int)skills.firewall:
                        magic = new Wall(power, Color.Violet, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, position, ref skillFlag);
                     */
                    default:
                        magic = null;
                        break;
                }
                if (magic != null)
                {
                    spawned.Add(magic);
                }

                speed = 0;
            }
            else
            {
                //da regolare
                if (currentSkill == (int)skills.laser)
                {
                    status.Prolongate((int)w_states.attacking, 50, (int)w_animations.attacking, true, true);
                }
            }
        }

        public void Die(int cause)
        {
            if (status.IsOff((int)w_states.dying))
            {
                status.SetOff((int)w_states.idle);
                status.SetOff((int)w_states.walking);
                status.SetOff((int)w_states.attacking);
                alive = false;
                speed = 0;
                cause_of_death = cause;
                List<int> l = new List<int>();
                l.Add((int)w_states.attacking);
                l.Add((int)w_states.idle);
                l.Add((int)w_states.rotating);
                l.Add((int)w_states.walking);
                
                status.SetOn((int)w_states.dying, sheet[sheetIndex].GetTotalDuration((int)w_animations.dying), (int)w_animations.dying, false, true,l);
            }
        }

        public void Idle()
        {
            
            if (status.IsOff((int)w_states.idle))
            {
                List<int> l = new List<int>();
                status.SetOff((int)w_states.walking);
                status.SetOff((int)w_states.attacking);
                l.Add((int)w_states.attacking);
                l.Add((int)w_states.idle);
                l.Add((int)w_states.rotating);
                l.Add((int)w_states.walking);
                status.SetOn((int)w_states.idle, (int)w_animations.idle, true, true);
                //damage data
            }
        }

        public void Turn(float from, float to, float step)
        {
            if (status.IsOff((int)w_states.rotating))
            {
                status.SetOff((int)w_states.idle);
                status.SetOn((int)w_states.rotating, (int)w_animations.idle, true, true);
                rotationAngle = Service.CurveAngle(from, to, step);
          
                boxList[0].Origin = Vector2.Transform(boxList[0].Origin - position, Matrix.CreateRotationZ(rotationAngle-from)) + position;
            }
        }

        public void Teleport()
        {
            if (rofTeleport <= 0)
            {
                if (status.IsOff((int)w_states.teleport))
                {
                    List<int> l = new List<int>();
                    status.SetOff((int)w_states.idle);
                    status.SetOff((int)w_states.walking);
                    status.SetOff((int)w_states.attacking);
                    status.SetOn((int)w_states.teleport, sheet[sheetIndex].GetTotalDuration((int)w_animations.teleport), (int)w_animations.teleport, false, true,l);
                    speed = 0;
                }
                rofTeleport = r.Next(5000, 15000);
            }
        }


        public void Deteleport()
        {
                if (status.IsOff((int)w_states.deteleport))
                {
                    List<int> l = new List<int>();
                    status.SetOff((int)w_states.idle);
                    status.SetOff((int)w_states.walking);
                    status.SetOff((int)w_states.attacking);
                    status.SetOn((int)w_states.deteleport, sheet[sheetIndex].GetTotalDuration((int)w_animations.deteleport), (int)w_animations.deteleport, false, true,l);
                    speed = 0;
                }
        }

         public void Walk(int sign)
        {
            if (status.IsOff((int)w_states.walking))
            {
                List<int> l = new List<int>();
                status.SetOff((int)w_states.idle);
                status.SetOn((int)w_states.walking, sheet[sheetIndex].GetTotalDuration((int)w_animations.walking), (int)w_animations.walking, true, true,l);
                speed = 0.6f * sign;
            }
        }

#endregion

        public override void Update(GameTime gameTime)
        {
            status.Update(gameTime);
            status.AutoOff();
            //if (alive)
            {

             //   if (currentDungeon.WallContact(cCheckBox))
               //   requiredPath = true;
             //   else
                {
                    pathToFollow = null;
                    requiredPath = false;
                    requiredAlternativePath = false;
                }
                requiredAlternativePath = false; //debug
                //attiva l entità se sei nel suo raggio visivo
                //if (target!=null && Circle.intersect(target.getBoundingCircle, this.inSightCircle)) // l assegnazione del target dovrebbe avenire tramite IAmanager, il controllo dell intersecanza dei cerchi non dovrebbe essere quindi usato
                active = true;

                //appena attivata, l entità comincia il suo conto alla rovescia per il proprio teletrasporto
                if (rofTeleport > 0 && active == true)
                    rofTeleport -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                //aggiornamento dei cerchi intersecanti
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


                if (target == null)
                {
                    Idle();
                }
                else if (pathToFollow == null && target != null && active)
                {
                    desiredAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1) 
                        + MathHelper.Pi / 50; //correzione empirica


                    if (status.IsOff((int)w_states.attacking) && rofTeleport <= 0)
                    {
                        Teleport();
                    }
                    else if (status.IsOff((int)w_states.teleport) && status.IsOff((int)w_states.deteleport))
                    {
                        if (Math.Abs(desiredAngle - rotationAngle) > Math.PI / 50)
                        {
                            Turn(rotationAngle, desiredAngle, (float)Math.PI / 100);
                        }
                        else if (Math.Abs(rotationAngle - desiredAngle) > Math.PI / 50)
                        {
                            Turn(rotationAngle, desiredAngle, (float)Math.PI / 100);
                        }
                        else if (Math.Abs(desiredAngle - rotationAngle) < Math.PI / 50 || Math.Abs(rotationAngle - desiredAngle) < Math.PI / 50)
                        {

                            if (Circle.intersect(inSightCircle, target.getBoundingCircle))
                            {
                               Attack();
                            }
                            else
                            {
                               Walk(1);
                            }

                        }
                    }
                }
                else if (pathToFollow != null && active)
                {
                    if (rofTeleport <= 0 && pathToFollow.Count>=1)  // da rivedere
                    {
                        if (Vector2.Distance( pathToFollow[pathToFollow.Count()-1].Center,pathToFollow[0].Center)> 200)
                            Teleport();
                    }
                    int wOffset=0;
                    if (Circle.intersect(boundingCircle, pathToFollow[w]))
                    {
                        wOffset = 1;
                    }
                    else
                    {
                        //turnica solo all assegnazione di un nuovo waypoint ossia quando waypoint [0] viene eliminato
                        desiredAngle = (float)Math.Atan2((position.Y - pathToFollow[w+wOffset].Center.Y) * -1, (position.X - pathToFollow[w+wOffset].Center.X) * -1);
                        if (Math.Abs(desiredAngle - rotationAngle) > Math.PI / 50)
                        {
                            Turn(rotationAngle, desiredAngle, (float)Math.PI / 64);
                        }
                        else if (Math.Abs(rotationAngle - desiredAngle) > Math.PI / 50)
                        {
                            Turn(rotationAngle, desiredAngle, (float)Math.PI/100);
                        }
                        else if (Math.Abs(desiredAngle - rotationAngle) < Math.PI / 50 || Math.Abs(rotationAngle - desiredAngle) < Math.PI / 50)
                        {
                            //if (status.IsOff((int)w_states.delayed))
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
            }
            #region controlli di stato

            if (status.IsOn((int)w_states.idle))
            {
                speed = 0;
                currentDungeon.FreeDamageId(damageData.id);
                //damageData.Reset();
                if (status.Old((int)w_animations.idle))
                {
                    //status.Bind((int)w_states.idle, (int)w_animations.idle, true, true, false);
                }
            }

            if (status.IsOn((int)w_states.dead))
            {
               
            }
 
            if(status.IsOn((int)w_states.dying))
            {
                
            }
            else if (status.Finished((int)w_states.dying))
            {
                //this.depth = Depths.corpse;
                status.SetLock((int)w_states.attacking);
                status.SetLock((int)w_states.deteleport);
                status.SetLock((int)w_states.idle);
                status.SetLock((int)w_states.rotating);
                status.SetLock((int)w_states.teleport);
                status.SetLock((int)w_states.walking);
                status.SetLock((int)w_states.dying);
                status.SetOn((int)w_states.dead, (int)w_animations.dead, true, true);
            }

            if(status.IsOn((int)w_states.attacking))
            {
                ActiveCaster = true;
            }
            else if (status.Finished((int)w_states.attacking))
            {
                ActiveCaster = false;
            }

            if (status.IsOn((int)w_states.teleport))
            {
                status.SetOff((int)w_states.idle);
                status.SetOff((int)w_states.walking);
                status.SetOff((int)w_states.rotating);
                status.SetOff((int)w_states.attacking);
            }
            else if (status.Finished((int)w_states.teleport))
            {
                status.SetOff((int)w_states.teleport);
                //status.CurrentAnimation=(int)w_animations.deteleport);
                Random r = new Random();
                Random r2 = new Random(r.Next(1, 7));

                //Controllo di verifica posizione post teleport
                Vector2 deteleportPos = new Vector2();
                deteleportPos = Vector2.Zero;

                int dim1 = 64;
                int xFactor = 2;

                int teleportCont = 0;
                Vector2 oldteleportPos = Vector2.Zero;
                Vector2 deteleportRadius;
                do
                {
                    if (teleportCont % 10 == 9)
                    {
                        xFactor++;
                    }

                    r = new Random(teleportCont + id);
                    r2 = new Random(teleportCont + r.Next() + id);


                    deteleportRadius = new Vector2(r.Next(-1, xFactor), r2.Next(-1, xFactor));
                    if (pathToFollow == null)
                        deteleportPos = position + deteleportRadius * dim1;
                    else
                        deteleportPos = pathToFollow[w].Center + deteleportRadius * dim1;

                    deteleportPos.X = ((deteleportPos.X / 32) * 32) + 16;
                    deteleportPos.Y = ((deteleportPos.Y / 32) * 32) + 16;
                    teleportCont++;
                }
                while (Vector2.Distance(deteleportPos, position) <= dim1 || deteleportPos.X > currentDungeon.width * 32 - dim1 || deteleportPos.Y > currentDungeon.height * 32 - dim1 || deteleportPos.X < dim1 || deteleportPos.Y < dim1 || Vector2.Distance(deteleportPos, oldteleportPos) < dim1 || (currentDungeon.WallContact(deteleportPos)));
                rofTeleport = r.Next(5000, 15000);
                oldteleportPos = deteleportPos;
                position = deteleportPos;

                meleeCircle.Center = position;
                rangeCircle.Center = position;
                specialCircle.Center = position;
                inSightCircle.Center = position;
                seekingCircle.Center = position;
                boundingBox.Origin = position;
                boxList[0].Origin = this.position;
                if (target!=null)
                    desiredAngle = (float)Math.Atan2((position.Y - target.getPosition.Y) * -1, (position.X - target.getPosition.X) * -1);
                rotationAngle = desiredAngle;

                //this.Turn(rotationAngle, desiredAngle, 0.05f);

                Deteleport();
            }

            if (status.IsOn((int)w_states.deteleport))
            {

            }
            else if (status.Finished((int)w_states.deteleport))
            {
                Idle();
            }

            if (status.IsOn((int)w_states.rotating))
            {
                status.SetOff((int)w_states.rotating);
            }

            if(status.IsOn((int)w_states.walking))
            {
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
                            if (status.IsOn((int)w_states.deteleport))
                            {
                                rofTeleport = 0;
                                Teleport();
                            }
                            float d = Vector2.Subtract(position + direction, c.position).Length();
                            if (d < c.distance && axis.Bottom + 10 <= c.axis.Top && axis.Top >= c.axis.Bottom)
                                direction *= 0;
                            cIds.Add(c.id);
                        }
                        //debug
                       // if (this.sensorBox.Intersects(c.boundingBox) && c.type == (int)entityTypes.table)
                        {
                            // requiredAlternativePath = true;
                            //obstructive = c.entity;
                        }
                    }
                    /*if (c.damage_done && c.type == (int)entityTypes.human && !status.IsOn((int)w_states.teleport))
                    {
                        this.Attack();
                    }*/
                }

                List<IEntity> sensed = currentDungeon.Sense(sensorBox);
                cIds.Clear();
            }

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
         
            direction = currentDungeon.WallContact(boundingCircle, direction);
            position += direction;
            boxList[0].Origin += direction;
            graphicOccupance.X = (int)position.X - graphicOccupance.Width / 2;
            graphicOccupance.Y = (int)position.Y - graphicOccupance.Height / 2;
            boundingBox.Origin += direction;

            oldPosition = position;     //salvataggio della vecchia posizione
            //direction *= 0;

            int k = 0;
            for (int i = 0; i < Globals.w_states; i++)
            {
                if (status.IsOff(i))
                {
                    k++;
                }
            }

            if (k == Globals.w_states)
            {
                this.Idle();
            }
            
            if (damageManager.health <= 0 && alive)
            {
                Die((int)deathCauses.generic); 
                //updatable = false; //debug
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

        public bool ActiveCaster
        {
            get;
            private set;
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
    }
}
