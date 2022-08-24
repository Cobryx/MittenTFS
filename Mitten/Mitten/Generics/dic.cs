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
    public class dic
    {
        private static Dictionary<string, Point> marbleTileIndex = new Dictionary<string, Point>()
        {
            {"black", new Point(3, 4)},
            {"blank", new Point(1, 1)},
            {"empty", new Point(2, 1)},
            {"removed_tile", new Point(0, 1)},
            {"plain_green", new Point(0, 0)}, 
            {"green1", new Point(1, 0)},
            {"green2", new Point(2, 0)},
            {"green3", new Point(3, 0)},
            {"broken_green", new Point(4, 0)},
            {"chipped_green", new Point(5, 0)},
            {"sunken_green", new Point(6, 0)},
            {"cracked_green", new Point(7, 0)},
            {"plain_black", new Point(8, 5)}, 
            {"black1", new Point(9, 5)},
            {"black2", new Point(10, 5)},
            {"black3", new Point(11, 5)},
            {"broken_black", new Point(12, 5)},
            {"chipped_black", new Point(13, 5)},
            {"sunken_black", new Point(14, 5)},
            {"cracked_black", new Point(15, 5)},
            {"plain_red", new Point(8, 6)}, 
            {"red1", new Point(9, 6)},
            {"red2", new Point(10, 6)},
            {"red3", new Point(11, 6)},
            {"broken_red", new Point(12, 6)},
            {"chipped_red", new Point(13, 6)},
            {"sunken_red", new Point(14, 6)},
            {"cracked_red", new Point(15, 6)},
            {"plain_gray", new Point(8, 7)}, 
            {"gray1", new Point(9, 7)},
            {"gray2", new Point(10, 7)},
            {"gray3", new Point(11, 7)},
            {"broken_gray", new Point(12, 7)},
            {"chipped_gray", new Point(13, 7)},
            {"sunken_gray", new Point(14, 7)},
            {"cracked_gray", new Point(15, 7)}
        };

        private static Dictionary<string, Point> wallTileIndex1 = new Dictionary<string, Point>()
        {
            {"blank", new Point(1, 1)},
            {"black", new Point(3, 4)},
            {"empty", new Point(2, 1)},
            {"NW_wall", new Point(0,2)},
            {"N_wall", new Point(1,2)},
            {"NE_wall", new Point(2,2)}, 
            {"NW_corner_wall", new Point(3,2)},
            {"NE_corner_wall", new Point(4,2)},
            {"W_wall", new Point(0,3)}, 
            {"E_wall", new Point(2,3)},
            {"SW_corner_wall", new Point(3,3)},
            {"SE_corner_wall", new Point(4,3)}, 
            {"SW_wall", new Point(0,4)},
            {"S_wall", new Point(1,4)},
            {"SE_wall", new Point(2,4)},
            {"N_T-wall", new Point(8, 0)},
            {"E_T-wall", new Point(9, 0)},
            {"W_T-wall", new Point(8, 1)},
            {"S_T-wall", new Point(9, 1)},
            {"SW_X-wall", new Point(10, 0)},
            {"NW_X-wall", new Point(11, 0)},
            {"SE_X-wall", new Point(10, 1)},
            {"NE_X-wall", new Point(11, 1)},
            {"wall", new Point(9, 3)},
            {"thin_horizontal_wall", new Point(9, 2)},
            {"thin_vertical_wall", new Point(8, 3)},
            {"thin_NW_wall", new Point(8, 2)},
            {"thin_NE_wall", new Point(10, 2)},
            {"thin_SE_wall", new Point(10, 4)},
            {"thin_SW_wall", new Point(8, 4)},
            {"double_corner_SW_NE", new Point(9, 4)},
            {"double_corner_NW_SE", new Point(10, 3)},
            {"X-wall", new Point(6, 3)},
            {"thin_E_wall", new Point(7, 2)},
            {"thin_S_wall", new Point(7, 4)},
            {"thin_W_wall", new Point(5, 4)},
            {"thin_N_wall", new Point(5, 2)},
            {"thin_N_T-wall", new Point(6, 4)},
            {"thin_E_T-wall", new Point(5, 3)},
            {"thin_W_T-wall", new Point(7, 3)},
            {"thin_S_T-wall", new Point(6, 2)},
            {"NE_reverse_L-wall", new Point(12, 0)},
            {"SE_reverse_L-wall", new Point(13, 0)},
            {"NW_reverse_L-wall", new Point(12, 1)},
            {"SW_reverse_L-wall", new Point(13, 1)},
            {"NE_L-wall", new Point(14, 0)},
            {"SE_L-wall", new Point(15, 0)},
            {"NW_L-wall", new Point(14, 1)},
            {"SW_L-wall", new Point(15, 1)}
        };


        private static Dictionary<string, Point> wallTileIndex2 = new Dictionary<string, Point>()
        {
            {"blank", new Point(1, 1)},
            {"black", new Point(3, 4)},
            {"empty", new Point(2, 1)},
            {"NW_wall", new Point(0,23)},
            {"N_wall", new Point(1,23)},
            {"NE_wall", new Point(2,23)}, 
            {"NW_corner_wall", new Point(3,23)},
            {"NE_corner_wall", new Point(4,23)},
            {"W_wall", new Point(0,24)}, 
            {"E_wall", new Point(2,24)},
            {"SW_corner_wall", new Point(3,24)},
            {"SE_corner_wall", new Point(4,24)}, 
            {"SW_wall", new Point(0,25)},
            {"S_wall", new Point(1,25)},
            {"SE_wall", new Point(2,25)},
            {"N_T-wall", new Point(8, 26)},
            {"E_T-wall", new Point(9, 26)},
            {"W_T-wall", new Point(8, 27)},
            {"S_T-wall", new Point(9, 27)},
            {"SW_X-wall", new Point(10, 26)},
            {"NW_X-wall", new Point(11, 26)},
            {"SE_X-wall", new Point(10, 27)},
            {"NE_X-wall", new Point(11, 27)},
            {"wall", new Point(6, 24)},
            {"thin_horizontal_wall", new Point(6, 23)},
            {"thin_vertical_wall", new Point(5, 24)},
            {"thin_NW_wall", new Point(5, 23)},
            {"thin_NE_wall", new Point(7, 23)},
            {"thin_SE_wall", new Point(7, 25)},
            {"thin_SW_wall", new Point(5, 25)},
            {"double_corner_SW_NE", new Point(6, 25)},
            {"double_corner_NW_SE", new Point(7, 24)},
            {"X-wall", new Point(9, 24)},
            {"thin_E_wall", new Point(10, 23)},
            {"thin_S_wall", new Point(10, 25)},
            {"thin_W_wall", new Point(8, 25)},
            {"thin_N_wall", new Point(8, 23)},
            {"thin_N_T-wall", new Point(9, 25)},
            {"thin_E_T-wall", new Point(10, 24)},
            {"thin_W_T-wall", new Point(8, 24)},
            {"thin_S_T-wall", new Point(9, 23)},
            {"NE_reverse_L-wall", new Point(12, 26)},
            {"SE_reverse_L-wall", new Point(13, 26)},
            {"NW_reverse_L-wall", new Point(12, 27)},
            {"SW_reverse_L-wall", new Point(13, 27)},
            {"NE_L-wall", new Point(14, 26)},
            {"SE_L-wall", new Point(15, 26)},
            {"NW_L-wall", new Point(14, 27)},
            {"SW_L-wall", new Point(15, 27)}
        };

        public static Dictionary<int, ItemInfo> itemIndex = new Dictionary<int, ItemInfo>();
        public static Dictionary<String, int> reverseItemIndex = new Dictionary<String, int>();
        

        public static void loadItemList(StreamReader itemData)
        {
            int lines = 0;
            String stringa;

            Char[] splitters = new Char[1];
            splitters[0] = ',';
            String[] Matrix;

            while (!itemData.EndOfStream)
            {
                itemData.ReadLine();
                lines++;
                
                

            }

            itemIndex = new Dictionary<int, ItemInfo>(lines);

            itemData.BaseStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < lines; i++)
            {
                stringa = itemData.ReadLine();
                Matrix = stringa.Split(splitters);

                ItemInfo InputItem = new ItemInfo();
                InputItem.minDamage = new Damage();
                InputItem.maxDamage = new Damage();

                float[] mindamage = new float[Globals.ndamagetypes];
                int[] mintime = new int[Globals.ndamagetypes];
                float[] minamount = new float[Globals.damage_effects];
                float[] minprobability = new float[Globals.damage_effects];
                int[] minduration = new int[Globals.damage_effects];

                float[] maxdamage = new float[Globals.ndamagetypes];
                int[] maxtime = new int[Globals.ndamagetypes];
                float[] maxamount = new float[Globals.damage_effects];
                float[] maxprobability = new float[Globals.damage_effects];
                int[] maxduration = new int[Globals.damage_effects];

                InputItem.minResistenceDamage = new float[Globals.ndamagetypes];
                InputItem.maxResistenceDamage = new float[Globals.ndamagetypes];
                InputItem.minResistenceEffects = new float[Globals.damage_effects];
                InputItem.maxResistenceEffects = new float[Globals.damage_effects];

                int cont = 0;
                //assegnazioni dei valori della matrice alle textbox
                InputItem.id = -1;
                        InputItem.name = Matrix[1];
                        if (Matrix[2] == "True")
                            InputItem.unique = true;
                        if (Matrix[3] == "True")
                            InputItem.stashable = true;
                        if (Matrix[4] == "True")
                            InputItem.equippable = true;
                        if (Matrix[5] == "True")
                            InputItem.usable = true;

                    InputItem.durability = (float)Convert.ToDouble(Matrix[204]);
                    InputItem.maxDurability = InputItem.durability;
                    InputItem.description = Matrix[205];
                    InputItem.otherP = Matrix[206];
                    InputItem.color = new Color(Convert.ToInt32(Matrix[207].Substring(1, 2), 16), Convert.ToInt32(Matrix[207].Substring(3, 2), 16), Convert.ToInt32(Matrix[207].Substring(5, 2), 16), Convert.ToInt32(Matrix[207].Substring(7, 2), 16));
                    InputItem.sprite = (Convert.ToInt32(Matrix[208])/64) + (Convert.ToInt32(Matrix[209])/64)*5; //valori costanti varianti assieme allo spritesheet
                    if (InputItem.stashable)
                    {
                        InputItem.minQuantity = Convert.ToInt32(Matrix[210]);
                        InputItem.maxQuantity = Convert.ToInt32(Matrix[211]);
                    }
                    for (int l = 6; l < 211; l++) //da verificare
                        {

                            if (cont < Globals.ndamagetypes)
                                mindamage[cont] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= Globals.ndamagetypes && cont < Globals.ndamagetypes * 2)
                                mintime[cont - Globals.ndamagetypes] = Convert.ToInt32(Matrix[l]);
                            if (cont >= Globals.ndamagetypes * 2 && cont < Globals.ndamagetypes * 2 + Globals.damage_effects)
                                minamount[cont - Globals.ndamagetypes * 2] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= Globals.ndamagetypes * 2 + Globals.damage_effects && cont < Globals.ndamagetypes * 2 + Globals.damage_effects * 2)
                                minprobability[cont - Globals.ndamagetypes * 2 - Globals.damage_effects] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= Globals.ndamagetypes * 2 + Globals.damage_effects * 2 && cont < Globals.ndamagetypes * 2 + Globals.damage_effects * 3)
                                minduration[cont - Globals.ndamagetypes * 2 - Globals.damage_effects * 2] = Convert.ToInt32(Matrix[l]);

                            int half = Globals.ndamagetypes * 2 + Globals.damage_effects * 3;

                            if (cont >= half && cont < half + Globals.ndamagetypes)
                                maxdamage[cont - half] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half + Globals.ndamagetypes && cont < half + Globals.ndamagetypes * 2)
                                maxtime[cont - half - Globals.ndamagetypes] = Convert.ToInt32(Matrix[l]);
                            if (cont >= half + Globals.ndamagetypes * 2 && cont < half + Globals.ndamagetypes * 2 + Globals.damage_effects)
                                maxamount[cont - half - Globals.ndamagetypes * 2] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half + Globals.ndamagetypes * 2 + Globals.damage_effects && cont < half + Globals.ndamagetypes * 2 + Globals.damage_effects * 2)
                                maxprobability[cont - half - Globals.ndamagetypes * 2 - Globals.damage_effects] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half + Globals.ndamagetypes * 2 + Globals.damage_effects * 2 && cont < half + Globals.ndamagetypes * 2 + Globals.damage_effects * 3)
                                maxduration[cont - half - Globals.ndamagetypes * 2 - Globals.damage_effects * 2] = Convert.ToInt32(Matrix[l]);


                            //aggiunta dei vettori per i modificatori dell'oggetto
                            if (cont >= half * 2 && cont < half * 2 + Globals.ndamagetypes)
                                InputItem.minResistenceDamage[cont - half * 2] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half * 2 + Globals.ndamagetypes && cont < half * 2 + Globals.ndamagetypes * 2)
                                InputItem.maxResistenceDamage[cont - half * 2 - Globals.ndamagetypes] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half * 2 + Globals.ndamagetypes * 2 && cont < half * 2 + Globals.damage_effects + Globals.ndamagetypes * 2)
                                InputItem.minResistenceEffects[cont - half * 2 - Globals.ndamagetypes * 2] = (float)Convert.ToDouble(Matrix[l]);
                            if (cont >= half * 2 + Globals.damage_effects + Globals.ndamagetypes * 2 && cont < half * 2 + Globals.damage_effects * 2 + Globals.ndamagetypes * 2)
                                InputItem.maxResistenceEffects[cont - half * 2 - Globals.damage_effects - Globals.ndamagetypes * 2] = (float)Convert.ToDouble(Matrix[l]);
                            cont++;
                        }
                        InputItem.maxDamage=new Damage(maxdamage, maxtime, maxprobability, maxamount, maxduration);
                        InputItem.minDamage=new Damage(mindamage, mintime, minprobability, minamount, minduration);

                        //itemIndex.Add(InputItem.id, InputItem);
                        itemIndex.Add(i, InputItem);
                }
                
                
            }

        public static Dictionary<int, SkillInfo> skillIndex = new Dictionary<int, SkillInfo>()
        {
            {0,
                new SkillInfo(1000, 0, new Damage(
                    new float[8]{3f,0,0,0,0,0,0,0},
                    new int[8]{10,0,0,0,0,0,0,0},
                    new float[17]{1f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                    new float[17]{7f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                    new int[17]{10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
                    ), new Damage(
                        new float[8]{3f,0,0,0,0,0,0,0},
                    new int[8]{10,0,0,0,0,0,0,0},
                    new float[17]{1f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                    new float[17]{7f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                    new int[17]{10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
                        ), new float[8], new float[8], new float[17], new float[17], new Vector2(10, 10), "asd", "asd2", "asd3", 5.0f)}
        };


        public static void loadSkillList(StreamReader skillData)
        {
        }

        /// <summary>
        /// Restituisce il dizionario di riferimento per il TileSet desiderato.
        /// </summary>
        /// <param name="n">Numero del set di tile desiderato</param>
        /// <returns></returns>
        public static Dictionary<String, Point> floorTileIndex(int n)
        {
            /*
            marble,
            cave,
            parquet,
            stone
            */
            switch (n)
            {
                case 0: return marbleTileIndex;
                //case 1: return marbleTileIndex;
                //case 2: return marbleTileIndex;
                //case 3: return marbleTileIndex;
                default: return marbleTileIndex;
            }
        }

        /// <summary>
        /// Restituisce il dizionario di riferimento per il TileSet desiderato.
        /// </summary>
        /// <param name="n">Numero del set di tile desiderato</param>
        /// <returns></returns>
        public static Dictionary<String, Point> wallTileIndex(int n)
        {
            /*
            standard,
            alternative,
            rock,
            wood,
            greek
             */
            switch (n)
            {
                case 0: return wallTileIndex1;
                case 1: return wallTileIndex2;
                default: return wallTileIndex1;
            }
        }

        public static Dictionary<String, int> sceneIndex = new Dictionary<string, int>()
        {
            {"blood_00", 0},
            {"blood_01", 1},
            {"blood_02", 2},
            {"blood_03", 3},
            {"blood_04", 4},
            {"blood_05", 5},
            {"blood_06", 6},
            {"blood_07", 7},
            {"blood_08", 8},
            {"blood_09", 9},
            {"blood_10", 10},
            {"blood_11", 11},
            {"blood_12", 12},
            {"blood_13", 13},
            {"blood_14", 14},
            {"green_tile", 15},
            {"little_rock", 16},
            {"grey_tile", 17},
            {"mosaic_NW", 18},
            {"mosaic_NE", 19},
            {"mosaic_SW", 20},
            {"mosaic_SE", 21},
            {"black_tile", 22},
            {"red_tile", 23},
            {"golden_rag", 24},
            {"golden_rag2", 25},
            {"golden_rag_end", 26},
            {"damasque_rag", 27},
            {"damasque_rag2", 28},
            {"damasque_rag_end", 29},
            {"red_rag", 30},
            {"red_rag2", 31},
            {"red_rag3", 32},
            {"red_rag_end", 33},
            {"chains", 34},
        };

        public static Dictionary<int, int> equipmentType = new Dictionary<int, int>()
        {
            {0, (int)equipSlots.shield},
            {1, -1},
            {2, (int)equipSlots.ammoBow},
            {3, (int)equipSlots.ammoCrossbow},
            {4, -1},
            {5, (int)equipSlots.ring},
            {6, (int)equipSlots.ranged},
            {7, -1},
            {8, -1},
            {9, -1},
            {10, (int)equipSlots.melee},
            {11, (int)equipSlots.melee},
            {12, (int)equipSlots.amulet},
            {13, -1},
            {14, (int)equipSlots.belt},
            {15, -1},
            {16, (int)equipSlots.armor},
            {17, (int)equipSlots.melee},
            {18, (int)equipSlots.throwing}

        };
    }
}
