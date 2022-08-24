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
    
    public class DamageManager
    {
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];    //il danno si intende per millisecondo
        int[] dur  = new int[Globals.damage_effects];  //espresse in millisecondi
        Random r = new Random();

        bool fl_effect = false;

        float[] modifier = new float[Globals.ndamagetypes];
        float hp;

        int i = 0;

        int timeCheck = 0;
        int timeInterval = 100;

        /// <summary>
        /// Costruttore.
        /// </summary>
        /// <param name="healthPoints">Punti salute iniziali dell'entità.</param>
        public DamageManager(float healthPoints)
        {
            for (i = 0; i < Globals.ndamagetypes; i++)
            {
                modifier[i] = 1;
            }

            this.hp = healthPoints;
        }

        /// <summary>
        /// Calcola i danni derivanti da una collisione.
        /// </summary>
        /// <param name="d">Struttura indicante i dannni.</param>
        public void CalculateDamage(Damage d)
        {
            //gestione dei danni semplici
            for (i = 0; i < Globals.ndamagetypes; i++)
            {
                //decrementa immediatamente la vita in base ai danni puntuali
                if (tim[i] == 0)
                {
                    hp -= d.getDamage[i] * modifier[i];
                }
                else //aggiunge danni distribuiti nel tempo che verranno trattati nell'Update
                {
                    dam[i] += (d.getDamage[i] * modifier[i]);
                    tim[i] += d.getDamageDuration[i];
                }
            }
            

            //gestione degli effetti
            for (i = 0; i < Globals.damage_effects; i++)
            {
                if ((float)r.NextDouble() <= d.getEffectProbability[i])
                {
                    fl_effect = true;
                    eff[i] = eff[i] * dur[i] + d.getEffectDamage[i] * d.getEffectDuration[i];
                    if (d.getEffectDuration[i] > dur[i])
                    {
                        dur[i] = d.getEffectDuration[i];
                    }
                    eff[i] = eff[i] / dur[i];
                }

            }

        }

        public void ChangeModifier(float[] modifier)
        {
            Array.Copy(modifier, this.modifier, Globals.ndamagetypes);
        }

        public void ChangeModifier(float modifier, int type)
        {
            this.modifier[type] = modifier;
        }

        public void Update(GameTime gameTime)
        {
            timeCheck += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            for (i = 0; i < Globals.ndamagetypes; i++)
            {
                tim[i] -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (tim[i] < 0)
                {
                    tim[i] = 0;
                    dam[i] = 0;
                }
            }

            for (i = 0; i < Globals.damage_effects; i++)
            {
                dur[i] -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (dur[i] < 0)
                {
                    dur[i] = 0;
                    eff[i] = 0;
                }
            }

            if (timeCheck > timeInterval)
            {
                timeCheck %= timeInterval;

                for (i = 0; i < Globals.ndamagetypes; i++)
                {
                    hp -= (dam[i] * (float)timeInterval);
                }
                
                for (i = 0; i < Globals.damage_effects; i++)
                {
                    hp -= (eff[i] * (float)timeInterval);
                }
            }

            fl_effect = false;
        }

        public int Effects(int i)
        {
            return dur[i];
        }

        public float health
        {
            get { return hp; }
            set { hp = value;}
        }

        public bool getFlag
        {
            get { return fl_effect; }
        }
    }
}
