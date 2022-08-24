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
    /// <summary>
    /// Spritesheet contenente tutti i frame di tutte le animazioni di una categoria di entità (classe che implementa l'interfaccia IEntity).
    /// Ogni categoria di entità (e.g.: giocatori umani, non-morti, trappole, oggetti a terra) si riferisce a uno spritesheet diverso.</para>
    /// N.B.: Entità appartenenti alla stessa categoria si riferiscono alla medesima istanza di uno stesso SpriteSheet.</para>
    /// </summary>
    public class SpriteSheet
    {
        Texture2D image;
        Rectangle[][] frames;
        Vector2[][] pivots;
        int[][] durations;

        /// <summary>
        /// Costruttore che istanzia lo Spritesheet fornendogli un'immagine di base e una mappa di coordinate per la suddivisione in frame caricata da file.
        /// </summary>
        /// <param name="image">Immagine di base per lo Spritesheet.</param>
        /// <param name="map">Istanza di un file di testo che funge da mappa dei singoli frame suddivisi in animazioni.</param>
        public SpriteSheet(Texture2D image, StreamReader map)
        {
            String stringa;
            char[] vseparator = { ';' };
            char[] cseparator = { ':' };
            int lines=0;
            
            while (!map.EndOfStream)
            {
                map.ReadLine();
                lines++;
            }

            map.BaseStream.Seek(0, SeekOrigin.Begin);

            this.image = image;
            frames = new Rectangle[lines][];
            pivots = new Vector2[lines][];
            durations = new int[lines][];
            for (int i = 0; i<lines; i++)
            {
                stringa = map.ReadLine();
                String[] vertex = stringa.Split(vseparator, StringSplitOptions.None);
                frames[i] = new Rectangle[vertex.Length-1];
                pivots[i] = new Vector2[vertex.Length-1];
                durations[i] = new int[vertex.Length-1];
                for (int j = 0; j < vertex.Length-1; j++)
                {
                    String[] coordinate = vertex[j].Split(cseparator, StringSplitOptions.None);
                    frames[i][j] = new Rectangle(Int32.Parse(coordinate[0]), Int32.Parse(coordinate[1]), Int32.Parse(coordinate[2]), Int32.Parse(coordinate[3]));
                    pivots[i][j] = new Vector2(float.Parse(coordinate[4]), float.Parse(coordinate[5]));
                    durations[i][j] = int.Parse(coordinate[6]);
                }

            }

            map.Close();
        }

        /// <summary>
        /// Costruttore speciale usato solo per lo spritesheet degli oggetti dell'inventario
        /// </summary>
        /// <param name="image">Spritesheet degli oggetti</param>
        public SpriteSheet(Texture2D image)
        {
            this.image = image;
            frames = new Rectangle[95][];
            pivots = new Vector2[95][];
            durations = new int[95][];

            for (int i = 0; i < 95; i++)
            {
                frames[i] = new Rectangle[1];
                frames[i][0] = new Rectangle(64*(i%5), 64*(i/5), 64, 64);
                pivots[i] = new Vector2[1];
                pivots[i][0] = new Vector2(32, 32);
                durations[i] = new int[1];
                durations[i][0] = 50;
            }
        }

        /// <summary>
        /// ATTENZIONE: attualmente non più usato.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        /// <summary>
        /// Restituisce il frame specificato per l'animazione specificata.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <param name="frame_index">Indice del frame.</param>
        /// <returns></returns>
        public Rectangle Frame(int animation, int frame_index)
        {
            if (this.frames[animation].Length > frame_index)
                return this.frames[animation][frame_index];
            else return this.frames[animation][0];
        }

        /// <summary>
        /// Restituisce il frame specificato per l'animazione specificata, debitamente accorciato.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <param name="frame_index">Indice del frame.</param>
        /// <param name="dimension">Dimensione ridotta specificata</param>
        /// <returns></returns>
        public Rectangle Frame(int animation, int frame_index,float dimension)
        {
            if (frames[animation].Length > frame_index)
                return new Rectangle(frames[animation][frame_index].X, frames[animation][frame_index].Y, (int)dimension, frames[animation][frame_index].Height);
            else return new Rectangle(frames[animation][0].X, frames[animation][0].Y, (int)dimension, frames[animation][0].Height);
        }

        /// <summary>
        /// Restituisce il numero del frame corrente sulla base del tempo trascorso per l'animazione specificata.
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public int GetFrame(int animation, int milliSeconds)
        {
            if (milliSeconds == 0)
            {
                return 0;
            }
            int total = this.GetTotalDuration(animation);
            while (milliSeconds > total)
            {
                milliSeconds -= total;
            }

            int dur = this.durations[animation][0];
            int i = 0;


            while (dur < milliSeconds)
            {
                 dur += this.durations[animation][i++];
            }

            return i;
        }

        /// <summary>
        /// Restituisce il numero del frame previsto al tempo specificato per l'animazione specificata.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <param name="frame">Indice del frame.</param>
        /// <param name="milliSeconds">Istante inteso in millisecondi (interi).</param>
        /// <returns></returns>
        public int GetFrame(int animation, int frame, int milliSeconds)
        {
            if (durations[animation][frame] < milliSeconds)
            {
                return (frame + 1)%durations[animation].Count();
            }
            else return frame;
        }

        /*
        public int GetFrame(int animation, int milliSeconds)
        {
            if (animation != 0)
            {
                if (time == 0)
                {
                    this.animation = animation;
                    nFrame = 0;
                    total = this.getTotalDuration(animation);
                    partial = this.getFrameDuration(animation, 0);
                    time = 1;
                    timer = new Timer(partial);
                    timer.Enabled = true;
                    timer.Elapsed += new ElapsedEventHandler(this.Elapsed);
                }
                else
                {
                }
            }
            return nFrame;
        }
        */


        /// <summary>
        /// Restituisce la durata del frame specificato per una data animazione.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <param name="frame_index">Indice del frame.</param>
        /// <returns>Durata del frame in millisecondi (interi).</returns>
        public int GetFrameDuration(int animation, int frame_index)
        {
            if (this.durations[animation].Length > frame_index)
                return this.durations[animation][frame_index];
            else return this.durations[animation][0];
        }

        /// <summary>
        /// Ottiene il numero di frame che compongono una data animazione.
        /// </summary>
        /// <param name="animation">Numero identificativo dell'animazione.</param>
        /// <returns>Lunghezza dell'animazione in numero di frame.</returns>
        public int GetFrameNumber(int animation)
        {
            return this.frames[animation].Length;
        }

        /// <summary>
        /// Restituisce il centro di rotazione per il frame specificato.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <param name="frame_index">Indice del frame.</param>
        /// <returns>Coordinate del centro di rotazione dello sprite corrispondente al frame specificato.</returns>
        public Vector2 GetRotationCenter(int animation, int frame_index)
        {
            if (this.pivots[animation].Length > frame_index)
                return this.pivots[animation][frame_index];
            else return this.pivots[animation][0];
        }

        /// <summary>
        /// Restituisce la durata totale dell'animazione specificata.
        /// </summary>
        /// <param name="animation">Indice dell'animazione.</param>
        /// <returns>Durata in millisecondi (interi) dell'animazione.</returns>
        public int GetTotalDuration(int animation)
        {
            int dur=0;
            for (int z = 0; z < this.durations[animation].Length; z++)
            {
                dur += this.durations[animation][z];
            }
            return dur;
        }

        public Rectangle[][] getFrame
        {
            get { return this.frames; }
        }

        public Texture2D sourceBitmap
        {
            get { return this.image; }
        }
    }
}
