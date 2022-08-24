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
    public class ParabolicProjectile : Throwable
    {
       
        ItemInfo item;
        float initialSpeed;
        //float h = 115f;
        float angle = (float)Math.PI / 2.5f;
        float vSpeed;
                
        int nBounce =1;
        float rAngle = 1.5f; //divisore del random direzionale
        Random r;
        Vector2 checkingDirection;
        


        public ParabolicProjectile(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int factionId, int type, int subtype,
            ref SpriteSheet[] sheet, ref Dungeon dungeon, ItemInfo originalItem)
            : base(position, radius, direction, speed, depth, health, rotation, factionId, type, subtype, ref sheet, ref dungeon)
        {
            axis = new VAxis(112, 116);
            r = new Random();
           // boundingBox = new OBB(position, rotation, originalItem.shape);
            sprite = originalItem.sprite;
            damage = originalItem.maxDamage;
            this.direction =new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            initialSpeed = speed;
            this.speed = speed*(float)Math.Cos(angle);
            vSpeed = (speed*(float)Math.Sin(angle) - Globals.G);
            scale = new Vector2(1.45f, 1.45f);
            status = new EntityManager(Globals.th_states, Globals.th_animations, ref sheet[sheetIndex]);
            status.SetOn((int)th_states.launched, (int)th_animations.launched, false, true);
            item = originalItem;
            ent_color = originalItem.color.Value;
            damageData = new DamageData(position, factionId, id, damage, boundingBox, null, id, type);
        }

        public override void Update(GameTime gameTime)
        {
            checkingDirection = direction * 2;
            if (status.IsOn((int)th_states.launched) && item.otherP.Contains("bouncing"))
            {
             
                if (!currentDungeon.WallContact(position + direction) )
                {
                    oldPosition = position;
                    position += direction * speed/Globals.cycle;
                    axis.Center += vSpeed/Globals.cycle;
                    vSpeed -= Globals.G/Globals.cycle;
                    scale.X=1+axis.Center*0.00391f;
                    scale.Y=1+axis.Center*0.00391f;

                    if (vSpeed < 0)
                    {

                    }
                    
                    if (axis.Bottom <= 0)
                    {
                        status.SetOff((int)th_states.launched);
                        status.SetOn((int)th_states.landing,(int)th_states.landing,true,true);
                        vSpeed = (speed * (float)Math.Sin(angle) - Globals.G) / 4;
                        axis.Center = 40f;
                    }
                    
                }

               
                if (currentDungeon.WallContact(position + new Vector2(0, direction.Y * speed / Globals.cycle)) || currentDungeon.WallContact(position + new Vector2(0, -direction.Y * speed / Globals.cycle)))
                {
                    direction = new Vector2(direction.X, -direction.Y +(float)( r.NextDouble()*direction.Y/rAngle));
                    oldPosition = position;
                    position += direction * speed / Globals.cycle;
                    axis.Center += vSpeed / Globals.cycle;
                    vSpeed -= Globals.G / Globals.cycle;
                    scale.X = 1 + axis.Center * 0.00391f;
                    scale.Y = 1 + axis.Center * 0.00391f;
                }

                else if (currentDungeon.WallContact(position + new Vector2(direction.X * speed / Globals.cycle, 0)) || currentDungeon.WallContact(position + new Vector2(-direction.X * speed / Globals.cycle, 0)))
                {
                    direction = new Vector2(-direction.X+(float)( r.NextDouble()*direction.X/rAngle) , direction.Y);
                    oldPosition = position;
                    position += direction * speed / Globals.cycle;
                    axis.Center += vSpeed / Globals.cycle;
                    vSpeed -= Globals.G / Globals.cycle;
                    scale.X = 1 + axis.Center * 0.00391f;
                    scale.Y = 1 + axis.Center * 0.00391f;
                }

                boundingBox.Origin = position;
            }

            else if (status.IsOn((int)th_states.launched) && !item.otherP.Contains("bouncing"))
            {
             
                if (!currentDungeon.WallContact(position + direction) )
                {
                    oldPosition = position;
                    position += direction * speed/Globals.cycle;
                    axis.Center += vSpeed/Globals.cycle;
                    vSpeed -= Globals.G/Globals.cycle;

                    scale.X=1+axis.Center*0.00391f;
                    scale.Y=1+axis.Center*0.00391f;

                    if (vSpeed < 0)
                    {

                    }
                    
                    if (axis.Bottom <= 0)
                    {
                        axis.Floor();
                        status.SetOff((int)th_states.launched);
                        status.SetOn((int)th_states.landing, (int)th_states.landing, true, true);
                    }
                    
                }
                else
                {
                    status.SetLock((int)th_states.launched);
                    status.SetOn((int)th_states.idle,(int)th_animations.idle,true,true);
                }
                boundingBox.Origin = position;
            }
            if (status.IsOn((int)th_states.stuck))
            {
                if (item.otherP.Contains("typical"))
                {
                    spawned.Add(new Explosion(position, 50, Depths.explosions, 100, (int)explosion_types.typical, rotationAngle, (int)entityTypes.explosion, ref sheet, ref currentDungeon));
                }
                else
                {

                    damageData.ResetOBB();
                    speed = 0;
                    spawned.Add(new Item(item, position, direction, Depths.item, rotationAngle, ref sheet, ref currentDungeon, false, 1));
                    Updatable = false;
                }
            }
            if (status.IsOn((int)th_states.landing) && item.otherP.Contains("bouncing"))
            {
                if (nBounce <= 2 && axis.Bottom<=0)
                {
                    vSpeed = (speed * (float)Math.Sin(angle) - Globals.G) / 4;
                    axis.Center = (115 / (4 * nBounce));
                    nBounce++;
                }
                //if (nBouce<3)
                oldPosition = position;
                position += direction * speed / Globals.cycle;
                axis.Center += vSpeed / Globals.cycle;
                vSpeed -= Globals.G / Globals.cycle;

                scale.X = 1 + axis.Center * 0.00391f;
                scale.Y = 1 + axis.Center * 0.00391f;

                damageData.ResetOBB();
                status.SetOff((int)th_states.launched);
                speed *= 0.9f;
               
                if (nBounce ==3 && axis.Center<=0)// && speed < 0.1f*cycle)
                {
                    speed = 0;
                    status.SetOff((int)th_states.landing);
                    status.SetOn((int)th_states.inhert, (int)th_animations.inhert, true, true); ;
                }
            }
            if (status.IsOn((int)th_states.landing) && !item.otherP.Contains("bouncing"))
            {
              
                oldPosition = position;
                position += direction * speed / Globals.cycle;
               
                damageData.ResetOBB();
                status.SetOff((int)th_states.launched);
                speed *= 0.9f;

                if (speed < 0.1f*Globals.cycle)
                {
                    speed = 0;
                    status.SetOff((int)th_states.landing);
                    status.SetOn((int)th_states.inhert, (int)th_animations.inhert, true, true); ;
                }
            }
            if (status.IsOn((int)th_states.inhert))
            {
                spawned.Add(new Item(item, position, direction, Depths.item, rotationAngle, ref sheet, ref currentDungeon, false, 1));
                Updatable = false;
            }

            status.Update(gameTime);
            damageManager.Update(gameTime);

            base.Update(gameTime);
        }
    }
}