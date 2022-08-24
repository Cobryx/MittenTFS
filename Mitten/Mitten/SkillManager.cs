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
    public class SkillManager
    {
        bool skillFlag;
        int[] delay;
        int factionId;
        SpriteSheet[] sheet;
        ICaster caster;

        public SkillManager(int factionId, ref SpriteSheet[] sheet, ICaster caster)
        {
            skillFlag = false;
            delay = new int[Globals.nSkill];
            this.factionId = factionId;
            this.sheet = sheet;
            this.caster = caster;
        }

        public int getAnimation()
        {
            switch (CurrentSkill)
            {
                case (int)skills.firebolt: return (int)h_animations.magic1;
                case (int)skills.laser: return (int)h_animations.magic2;
                case (int)skills.blaze: return (int)h_animations.magic2;
                case (int)skills.shield: return (int)h_animations.magic1;
                case (int)skills.icewall: return (int)h_animations.magic1;
                case (int)skills.firewall: return (int)h_animations.magic1;
                default: return (int)h_animations.magic1;
            }
        }

        public int getEffect(int skill)
        {
            switch(skill)
            {
                default: return 0;
            }
        }

        
        public List<IEntity> createSkill(float power, float rotationAngle, Vector2 position, ref Dungeon currentDungeon, ICaster caster)
        {
            List<IEntity> magicEntities = new List<IEntity>();
            switch (CurrentSkill)
                {
                    case (int)skills.firebolt:
                        magicEntities.Add( new MagicProjectile((int)damageTypes.fire, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y)));
                        break;
                    case (int)skills.laser:
                        if (caster.ActiveCaster)
                            magicEntities.Add( new Laser((int)damageTypes.energy, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y), caster));
                        break;
                    case (int)skills.blaze:
                        magicEntities.Add( new Blaze(Color.Violet, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y), caster));
                        break;
                    case (int)skills.firewall:
                        magicEntities.Add(new Wall(power, Color.Violet, factionId, 0, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, position/*new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y)*/, caster, true));
                        break;
                    case (int)skills.icewall:
                        magicEntities.Add(new Wall(power, Color.Violet, factionId, 1, (int)entityTypes.magic, rotationAngle , ref currentDungeon, ref sheet, position,caster,true));
                        //new Wall(power, Color.Violet, factionId, 1, (int)entityTypes.magic, rotationAngle, ref currentDungeon, ref sheet, position, caster, true);
                            break;
                    case (int)skills.shield:
                        Shield skill = new Shield(power, (int)damageTypes.energy, factionId, 0, (int)entityTypes.magic, ref currentDungeon, ref sheet, caster);
                        magicEntities = skill.GetSpawningList();
                        break;
                }
            return magicEntities;
        }

        public int CurrentSkill { get; set; }

        public bool fl_skill
        {
            get { return skillFlag; }
            set { skillFlag = value; }
        }
    }
}
