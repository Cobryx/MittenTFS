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
    /// Collezione di parametri globali sottoforma di interi statici.
    /// </summary>
    public class Globals
    {
        public const bool dMode = false;
        public const bool advancedDMode = false;
        public const bool extremeDMode = false;
        public const bool generateDMap = false;
        public const bool AIActivity = false;
        public static bool showGUI = true;
        public const int b_animations = 5;
        public const int b_states = 7;
        public const int ex_animations = 4;
        public const int ex_states = 4;
        public const int h_damage = 6; //numero di array Damage per la classe Human
        public const int h_animations = 20;
        public const int h_states = 19;
        public const int sp_animations = 6;
        public const int sp_states = 7;
        public const int tb_animations = 9;
        public const int tb_states = 9;
        public const int th_animations = 5;
        public const int th_states = 5;
        public const int w_animations = 7;
        public const int w_states = 8;
        public const int z_animations = 5;
        public const int z_states = 7;
        public const int damage_effects = 17;
        public const int ndamagetypes = 8;
        public const int nhandleables = 6;
        public const int nsceneset = 1;
        public const int nsheets = 100; //24
        public const int nsounds = 100; //valore assolutamente privo di significato
        public const int nsongs = 10;
        public const int ntypes = 16;
        public const int max_entities = 32768;
        public const int max_damages = 32768;
        public const int maximumLocalPlayers = 4;
        public const int maximumGamers = 31;
        public const int multiPlayerDungeon = 4096;
        public const int singlePlayerDungeons = 2;
        public static bool[] id = new bool[max_entities];
        public static int players;
        public static MarkupTextEngine GUIengine;
        public const int nSkill = 4;
        public static int nItems;
        public const float G = 627f;
        public const float cycle = 60f;

        public static SoundManager Frequencies;

        public static IAManager IAmanager;

        public static Rectangle[] camera; //globalizzare le camere

        public static Krypton.KryptonEngine krypton;

        public static SpriteBatch spriteBatch;
        public static SpriteFont GUIFont;

        public static Texture2D Box;
        public static Texture2D GUIheart;
        public static Texture2D GUIborder;
        public static Texture2D GUIinventorybg;
        public static Texture2D GUIinventorycase;
        public static Texture2D GUIinventoryselect;

        public static Texture2D mLightTexture;
      

        public static void InitializeIdArray()
        {
            for (int i = 0; i < max_entities; i++)
            {
                id[i] = false;
            }
        }

        /// <summary>
        /// Assegna un id unico all'entità.
        /// </summary>
        /// <returns>Restituisce l'id assegnato.</returns>
        public static int AssignAnId()
        {
            int i;
            
            for (i = 0; id[i]; i++) ;
            id[i] = true;
            return i;
        }

        /// <summary>
        /// Richiede un id unico specifico (piuttosto che il primo disponibile).
        /// </summary>
        /// <param name="i">L'id richiesto</param>
        /// <returns>L'id richiesto se l'assegnazione è andata a buon fine, -1 se è già assegnato</returns>
        public static int AssignAnId(int i)
        {
            if (id[i])
            {
                return -1;
            }
            else
            {
                id[i] = true;
                return i;
            }
        }

        /// <summary>
        /// Libera un id precedentemente assegnato al termine del ciclo vitale dell'entità da esso identificato.
        /// Dovrebbe essere chiamato una e una sola volta per ogni entità.
        /// </summary>
        /// <param name="i"></param>
        public static void FreeId(int i)
        {
            if (id[i])
            {
                id[i] = false;
            }
            else
            {
                //throw new InvalidIdException("The selected id was not assigned.");
            }
        }

        
    }
}
