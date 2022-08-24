using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    public class Damage// : IComparable
    {
        float[] damage = new float[Globals.ndamagetypes];
        int[] time = new int[Globals.ndamagetypes];
        float[] probability = new float[Globals.damage_effects];
        float[] amount = new float[Globals.damage_effects];
        int[] duration = new int[Globals.damage_effects];

        /// <summary>
        /// Istanzia un set di valori nulli.
        /// </summary>
        public Damage()
        {
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                amount[i] = 0;
                time[i] = 0;
            }
            for (int i = 0; i < Globals.damage_effects; i++)
            {
                probability[i] = 0;
                amount[i] = 0;
                duration[i] = 0;
            }
        }

        /// <summary>
        /// Costruttore ordinario per la classe Damage
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="time"></param>
        /// <param name="probability"></param>
        /// <param name="amount"></param>
        /// <param name="duration"></param>
        public Damage(float[] damage, int[] time, float[] probability, float[] amount, int[] duration)
        {
            Array.Copy(damage, this.damage, Globals.ndamagetypes);
            Array.Copy(time, this.time, Globals.ndamagetypes);
            Array.Copy(probability, this.probability, Globals.damage_effects);
            Array.Copy(amount, this.amount, Globals.damage_effects);
            Array.Copy(duration, this.duration, Globals.damage_effects);
        }

        public void Add(int type, float amount, int time)
        {
            this.damage[type] = amount;
            this.time[type] = time;
        }

        public void AddEffect(int type, float probability, float amount, int duration)
        {
            this.probability[type] = probability;
            this.amount[type] = amount;
            this.duration[type] = duration;
        }

        public float Total()
        {
            float tot = 0;
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                tot += damage[i];
            }
            return tot;
        }

        public static bool operator > (Damage d1, Damage d2)
        {
            if (d1.Total() > d2.Total())
                return true;
            else return false;
        }

        public static bool operator <(Damage d1, Damage d2)
        {
            if (d1.Total() < d2.Total())
                return true;
            else return false;
        }



        public static Damage operator + (Damage d1, Damage d2)
        {
            Damage result = new Damage();
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                result.damage[i]=d1.damage[i]+d2.damage[i];
                result.time[i] = d1.time[i] + d2.time[i];
            }
            for (int i = 0; i < Globals.damage_effects; i++)
            {
                result.amount[i] = d1.amount[i] + d2.amount[i];
                result.duration[i] = d1.duration[i] + d2.duration[i];
                result.probability[i] = d1.probability[i] + d2.probability[i];
            }
            return result;
        }

        public static Damage operator - (Damage d1, Damage d2)
        {
            Damage result = new Damage();
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                result.damage[i] = d1.damage[i] - d2.damage[i];
                result.time[i] = d1.time[i] - d2.time[i];
            }
            for (int i = 0; i < Globals.damage_effects; i++)
            {
                result.amount[i] = d1.amount[i] - d2.amount[i];
                result.duration[i] = d1.duration[i] - d2.duration[i];
                result.probability[i] = d1.probability[i] - d2.probability[i];
            }
            return result;
        }

        public static Damage Randomize(Damage min, Damage max)
        {
            Damage result = new Damage();
            Random rand = new Random();
            for (int i = 0; i < Globals.ndamagetypes; i++)
            {
                result.damage[i] = (float)rand.NextDouble()*(max.damage[i] - min.damage[i]) + min.damage[i];
                result.time[i] = rand.Next(min.time[i], max.time[i]);
            }
            for (int i = 0; i < Globals.damage_effects; i++)
            {
                result.amount[i] = (float)rand.NextDouble()*(max.amount[i] - min.amount[i]) + min.amount[i];
                result.duration[i] = min.duration[i] - max.duration[i];
                result.probability[i] = (float)rand.NextDouble()*(max.probability[i] - min.probability[i]) + min.probability[i];
            }
            return result;
        }

        

        #region properties

        public float[] getDamage
        {
            get { return damage; }
        }

        public int[] getDamageDuration
        {
            get { return time; }
        }

        public float[] getEffectDamage
        {
            get { return amount; }
        }

        public int[] getEffectDuration
        {
            get { return duration; }
        }

        public float[] getEffectProbability
        {
            get { return probability; }
        }

        #endregion

    }
}
