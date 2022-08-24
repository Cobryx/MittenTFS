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
    public class EnemyseekingProjectile : Throwable
    {
        ItemInfo item;

        public EnemyseekingProjectile(Vector2 position, float radius, Vector2 direction, float speed, float depth, float health, float rotation, int factionId, int type, int subtype,
            ref SpriteSheet[] sheet, ref Dungeon dungeon, ItemInfo originalItem)
            : base(position, radius, direction, speed, depth, health, rotation, factionId, type, subtype, ref sheet, ref dungeon)
        {
            //boundingBox = new OBB(position, rotation, originalItem.shape);
            sprite = originalItem.sprite;
            this.damage = originalItem.maxDamage;
            this.direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            this.speed = speed;
            status = new EntityManager(Globals.th_states, Globals.th_animations, ref sheet[sheetIndex]);
            status.SetOn((int)th_states.launched,(int)th_animations.launched,false,true);
            item = originalItem;
            ent_color = originalItem.color.Value;
        }

        public override void Update(GameTime gameTime)
        {

            if (status.IsOn((int)th_states.launched))
            {
                if (!currentDungeon.WallContact(position + direction))
                {
                    oldPosition = position;
                    position += direction * speed;
                }
                else
                {
                    status.SetLock((int)th_states.launched);
                    status.SetOn((int)th_states.stuck,(int)th_animations.stuck,true,true);
                    //ficcarsi da qualche parte
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
            if (status.IsOn((int)th_states.landing))
            {
                status.SetOff((int)th_states.launched);
                speed *= 0.9f;

                if (speed < 0.01f)
                {
                    speed = 0;
                    status.SetOff((int)th_states.landing);
                    status.SetOn((int)th_states.inhert, (int)th_animations.inhert, true, true);
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