using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    /// <summary>
    /// Collezione di valori costanti di profondità per l'algoritmo del pittore; 0=foreground, 1=background
    /// </summary>
    public class Depths
    {
        //in foreground
        public static float foreBoxes = 0.005f;
        public static float Igame = 0.01f;
        public static float inventory_selector = 0.025f;
        public static float inventory_letters = 0.026f;
        public static float inventory_item = 0.027f;
        public static float inventory_casing = 0.028f;
        public static float inventory_background = 0.029f;
        public static float fog = 0.020f;
        public static float entityImmaterial = 0.03f;
        public static float walls = 0.04f;

        public static float overForeSkill = 0.08f;
        public static float foreSkill = 0.09f;

        //GUI
        public static float playerGUI = 0.001f;

        //nel mezzo
        public static float floating = 0.1f;
        public static float doors = 0.2f;
        public static float table = 0.35f;
        public static float explosions = 0.30f;
        public static float entity_idle = 0.45f;
        public static float middle_air = 0.5f;
        public static float backSkill = 0.6f;
        public static float InteractiveScenography = 0.65f;

        //in background
        public static float dying = 0.905f;
        public static float item = 0.91f;
        public static float stairs = 0.94f;
        public static float boxes = 0.95f;
        public static float scenography = 0.96f;
        public static float floor = 0.97f;
        public static float corpse = 0.98f;
        
    }
}

