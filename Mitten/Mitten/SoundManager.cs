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
    public class SoundManager
    {
        int currentDungeonIndex;
        SoundEffect[] VSound;
        SoundEffect[] VSong;
        SoundEffectInstance currentSongInPlay;
        int currentSongInPlayIndex;
        int[] soundInPlay;
        int[] songInPlay;

        public SoundManager(SoundEffect[] VSong, SoundEffect[] VSound) 
        {
             this.VSound = VSound;
             this.VSong=VSong;
             songInPlay = new Int32[Globals.nsongs];
             soundInPlay = new Int32[Globals.nsounds];
             songInPlay.Initialize();
             soundInPlay.Initialize();
        }

   

        /// <summary>
        /// Riproduce un suono udibile in ogni punto
        /// </summary>
        /// <param name="soundEffect">Indice del suono da riprodurre</param>
        public void PlaySound(int soundEffect)
        {
            VSound[soundEffect].Play();
            soundInPlay[soundEffect] = 1;
        }

        /// <summary>
        /// Riproduce un suono alla locazione desiderata nel dungeon corrente
        /// </summary>
        /// <param name="soundEffect">Indice del suono da riprodurre</param>
        public void PlaySound(int soundEffect, Vector2 location)
        {
            VSound[soundEffect].Play();
            soundInPlay[soundEffect] = 1;
        }

        /// <summary>
        /// Riproduce un suono udibile alla locazione desiderata nel dungeon specificato
        /// </summary>
        /// <param name="soundEffect">Indice del suono da riprodurre</param>
        public void PlaySound(int soundEffect, Vector2 location, int dungeon)
        {
            VSound[soundEffect].Play();
            soundInPlay[soundEffect] = 1;
        }

        public void PlaySong(int song)
        {
            if (song >= 0 && song < 4)
            {
                if (currentSongInPlay != null)
                {
                    currentSongInPlay.Stop();
                }
                    currentSongInPlayIndex = song;
                    currentSongInPlay = VSong[song].CreateInstance();
                    songInPlay[song] = 1;
                    currentSongInPlay.Play();
                
            }
        }
         


        public void Update(GameTime gametime)
        {
         /*       for (int i = 0; i < songInPlay.Length; i++)
                {
                    songInPlay[i] = 0;
                    currentSongInPlay = -1;
                }*/
        }

        public int getCurrentSong
        {
            get { return currentSongInPlayIndex; }
        }

    }
}
