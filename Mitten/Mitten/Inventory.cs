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
    public class Inventory
    {
        public bool isChanged { get; set; }
        bool visible = false;
        Dungeon currentDungeon;
        ItemInfo[] quickBar = new ItemInfo[10];
        int element_distance;
        int playerNumber;
        int itemIndex = 0;
        Vector2 GUIspacing;
        int width = 5;
        int x = 0;
        int y = 0;
        float GUIscaling;
        OBB background;
        OBB casing;
        Vector2 description_position;
        Vector2 equipOffset=new Vector2(0,0);
        Vector2 outer_square_origin;
        Vector2 square_origin;
        

        
        float inventoryScale;
        public int[] equippedItems = new int[11];

        //vettori oggetti equippaggiabili stashable
        int[] ammoTypeBow = new int[10];  //vettore di ammo arco
        int[] ammoTypeCrossbow = new int[10];  //vettore di ammo balestra
        int[] ammoTypeThrowable = new int[10];  //vettore di armi da lancio
        

        SpriteSheet[] spriteSheet;
        Texture2D debugBox;
        Vector2 equipPos;
        
        Vector2 offset;
        Vector2 inventoryPosition;

        List<ItemInfo> items;
        int n=1; //numero di pagina nell'interfaccia visiva
        int m=1; //numero di pagine

        string text;

        CompiledMarkup cm;
        DamageManager damageManager;

        public Inventory(ref SpriteSheet[] sheet, int playerNumber, ref DamageManager damageManager)
        {
            for (int i = 0; i < ammoTypeBow.Length; i++)
            {
                ammoTypeBow[i] = -1;
                ammoTypeCrossbow[i] = -1;
                ammoTypeThrowable[i] = -1;
            }
            this.damageManager = damageManager;
            items = new List<ItemInfo>();
            spriteSheet = sheet;
            debugBox = Globals.Box;
            isChanged = false;
            //equipPos = new Vector2(40, 40);
            for (int i = 0; i < equippedItems.Length; i++)
            {
                equippedItems[i] = -1;  //nessun oggetto equipaggiato
            }

            if (Globals.players == 1)
            {
                GUIscaling = 1f;
                GUIspacing = new Vector2(0, 68);
                equipPos = new Vector2(0, Settings.Instance.VerticalResolution-68);
                outer_square_origin = new Vector2(36, 36);
                square_origin = new Vector2(34, 34);
                element_distance = 80;
                description_position = new Vector2(Settings.Instance.HorizontalResolution / 2 + 64, Settings.Instance.VerticalResolution / 8);
            }
            else
            {
                GUIscaling = 0.5f;
                GUIspacing = new Vector2(34, 0);
                if (Settings.Instance.cameraOption == 2)
                {
                    //equipPos = new Vector2((playerNumber - 1) * Settings.Instance.HorizontalResolution / Globals.players, Settings.Instance.VerticalResolution - 34);
                }
                else
                {
                    equipPos = new Vector2((playerNumber - 1) * Settings.Instance.HorizontalResolution / 4, Settings.Instance.VerticalResolution - 34);
                }
                description_position = new Vector2(100, 2 *Settings.Instance.VerticalResolution / 3);
                outer_square_origin = new Vector2(36, 36);
                square_origin = new Vector2(34, 34);
                element_distance = 40;
            }
            
            //cm.Start();

            this.playerNumber = playerNumber;
            if (Globals.players == 1)
            {
                offset = new Vector2(Settings.Instance.h_res / 2, Settings.Instance.v_res / 2);
                inventoryScale = 1f;
            }
            else inventoryScale = 0.5f;
            switch (Settings.Instance.cameraOption)
            {
                case 1: offset = new Vector2(Settings.Instance.v_res / 8 + Settings.Instance.v_res / 4 * (playerNumber-1), Settings.Instance.v_res / 4); break;
                case 2: offset = new Vector2(50, 50); break;
                case 3: offset = new Vector2(playerNumber * 50, Settings.Instance.v_res / 4);  break;    
            }
            background = new OBB(new Vector2(offset.X+84, offset.Y+69), 0, new Vector2(100, 100));
            inventoryPosition = new Vector2(offset.X-20, offset.Y-20);
            casing = new OBB(new Vector2(), 0, new Vector2(16, 16));
            casing.DebugColor = new Color(32, 32, 32, 224);
        }

        public void Add(ItemInfo item)
        {
            //rimane da mettere un oggetto stashable nell'inventario
            //e modificare il metodo Drop() per rilasciare un oggetto di quantità 1 alla volta
            if (item.stashable)
            {
                //inserimento degli indici oggetti equipaggiabili e stashabili quali ammo in un apposito vettore
                //debug sostituire ammoCrossbow con ammobow appena lo spritesheet e stato rifatto
                if (dic.equipmentType[item.sprite / 5] == (int)equipSlots.ammoBow)
                {
                    for (int ind = 0; ind < ammoTypeBow.Length; ind++)
                    {
                        if (ammoTypeBow[ind] == -1)
                        {
                            ammoTypeBow[ind] = items.Count;
                            break;
                        }
                    }
                }

                else if (dic.equipmentType[item.sprite / 5] == (int)equipSlots.ammoCrossbow)
                {
                    for (int ind = 0; ind < ammoTypeCrossbow.Length; ind++)
                    {
                        if (ammoTypeCrossbow[ind] == -1)
                        {
                            ammoTypeCrossbow[ind] = items.Count;
                            break;
                        }
                    }
                }
                else if (dic.equipmentType[item.sprite / 5] == (int)equipSlots.throwing)
                {
                    for (int ind = 0; ind < ammoTypeThrowable.Length; ind++)
                    {
                        if (ammoTypeThrowable[ind] == -1)
                        {
                            ammoTypeThrowable[ind] = items.Count;
                            break;
                        }
                    }
                }
                bool newItem = true;
                int index = 0;
                foreach (ItemInfo i in items)
                {
                    if (i.name == item.name)
                    {
                        item.minQuantity+=i.minQuantity;
                        item.maxQuantity += i.maxQuantity;
                        items.RemoveAt(index);
                        items.Insert(index, item);
                        newItem = false;
                        break;
                    }
                    else index++;
                }
                if (newItem)
                {
                    items.Add(item);
                }
            }
            else items.Add(item);
        }

        public void Add(ref Item item)
        {
            this.Add(item.ToItemInfo());
            //items.Add(item.ToItemInfo());
            //item.Updatable = false;
        }

        public List<Item> DropAll(Vector2 position, float depth, float angle,ref Dungeon dungeon)
        {
            List<Item> items2 = new List<Item>();
            foreach(ItemInfo i in items)
            {
                items2.Add(new Item(i, position, new Vector2(0), depth, 0, ref this.spriteSheet, ref dungeon, false));
            }
            for (int i = 0; i < ammoTypeBow.Length; i++)
            {
                ammoTypeBow[i] = -1;
                ammoTypeCrossbow[i] = -1;
                ammoTypeThrowable[i] = -1;

            }

            items.Clear();
            isChanged = true;
            return items2;
        }

        public Item Drop(int index, Vector2 position, Vector2 direction, float depth, float angle)
        {
            Item it = new Item(items.ElementAt(index), position, direction, 0.45f, angle, ref spriteSheet, ref currentDungeon, true);
            for (int i =0 ; i<ammoTypeBow.Count();i++)
            {
                if (ammoTypeBow[i] == index)
                {
                    ammoTypeBow[i] = ammoTypeBow[i + 1];
                    ammoTypeBow[i + 1] = -1;
                }
                if (ammoTypeBow[i] == index)
                {
                    ammoTypeCrossbow[i] = ammoTypeCrossbow[i + 1];
                    ammoTypeCrossbow[i + 1] = -1;
                }
                if (ammoTypeThrowable[i] == index)
                {
                    ammoTypeThrowable[i] = ammoTypeThrowable[i + 1];
                    ammoTypeThrowable[i + 1] = -1;
                }
            }
            items.RemoveAt(index);
          


            isChanged = true;
            return it;
        }

        public Item Drop(ref ItemInfo item, Vector2 position, Vector2 direction, float depth, float angle)
        {
            Item item2 = new Item(item, position, direction, depth, angle, ref spriteSheet, ref currentDungeon, false);
            items.Remove(item);


            isChanged = true;
            return item2;
        }

        public void Action()
        {
            if (items.Count <= itemIndex || items.Count < 1)
                return;
            
            ItemInfo ii = items[itemIndex];
            if (ii.equippable)
            {
                if (equippedItems[dic.equipmentType[ii.sprite / 5]] == itemIndex)
                {
                    equippedItems[dic.equipmentType[ii.sprite / 5]] = -1;

                }
                else 
                    equippedItems[dic.equipmentType[ii.sprite/5]]=itemIndex;
                isChanged = true;

                
            }
            if (ii.usable)
            {
                if(ii.stashable)
                {
                    if (ii.maxQuantity > 1)
                    {
                        

                        ii.maxQuantity--;
                        ii.minQuantity--;
                        items[itemIndex] = ii;
                    }
                    else if (items[itemIndex].maxQuantity == 1)
                    {
                        equippedItems[itemIndex] = -1;
                        items.RemoveAt(itemIndex);
                    }
                }
                if (ii.name == "Health potion")
                {
                    damageManager.health += 10;
                }

            }
        }

        public List<int> GetProperties()
        {
            List<int> prop = new List<int>();
            foreach (int i in equippedItems)
            {
                //da implementare
            }
            return prop;
        }

        public void DecrementAmmo(int ammo)
        {
            if (items[equippedItems[ammo]].maxQuantity > 1)
            {
                ItemInfo ii = items[equippedItems[ammo]];
                ii.maxQuantity--;
                ii.minQuantity--;
                items[equippedItems[ammo]] = ii;
            }
            else
            {
                switch (ammo)
                {
                    case (int)equipSlots.ammoBow:
                        for (int i = 0; i < ammoTypeBow.Count(); i++)
                        {
                            if (ammoTypeBow[i] == equippedItems[ammo])
                            {
                                if (i < ammoTypeBow.Count())
                                {
                                    ammoTypeBow[i] = ammoTypeBow[i + 1];
                                    ammoTypeBow[i + 1] = -1;
                                }
                                else
                                    ammoTypeBow[i] = -1;
                            }
                        }
                        break;
                    case (int)equipSlots.ammoCrossbow:
                        for (int i = 0; i < ammoTypeCrossbow.Count(); i++)
                        {
                            if (i < ammoTypeCrossbow.Count())
                            {
                                if (ammoTypeCrossbow[i] == equippedItems[ammo])
                                {
                                    ammoTypeCrossbow[i] = ammoTypeCrossbow[i + 1];
                                    ammoTypeCrossbow[i + 1] = -1;
                                }
                            }
                            else
                                ammoTypeCrossbow[i] = -1;
                        }
                        break;
                    case (int)equipSlots.throwing:
                        for (int i = 0; i < ammoTypeThrowable.Count(); i++)
                        {
                            if (ammoTypeThrowable[i] == equippedItems[ammo])
                            {
                                if (i < ammoTypeThrowable.Count())
                                {
                                    ammoTypeThrowable[i] = ammoTypeThrowable[i + 1];
                                    ammoTypeThrowable[i + 1] = -1;
                                }
                                else
                                    ammoTypeThrowable[i] = -1;

                            }
                        }
                        break;
                }

                //riordino degli indici del vettore degli oggetti equipaggiati
                int sortElement = equippedItems[ammo];

                for (int i = 0; i < equippedItems.Length; i++)
                {
                    if (equippedItems[i] > sortElement)
                    {
                        equippedItems[i] -= 1;
                    }
                }


                items.RemoveAt(equippedItems[ammo]);
                equippedItems[ammo] = -1;

            }
        }

        public void Show()
        {
            if (visible)
            {
                visible = false;
            }
            else visible = true;
        }


        public void OrderBy()
        {
        }

        public void Draw(Vector2 equipPos)
        {
            if (visible)
            {
                ItemInfo ii;
                Vector2 screen_pos = new Vector2(offset.X+20, offset.Y+20);
                //background.Draw(debugBox, new Rectangle(), Globals.spriteBatch, Depths.inventory_background);
                //Globals.spriteBatch.Draw(Globals.GUIinventorybg, inventoryPosition, null, Color.White, 0, Vector2.Zero, inventoryScale, SpriteEffects.None, Depths.inventory_background);

                for (int j = 0; j < 4; j++)
                {
                    
                    for (int i = 0; i < 5; i++)
                    {
                        int index=i + j * 5+20*(n-1);
                        //casing.Origin = screen_pos;
                        //casing.Draw(debugBox, new Rectangle(), Globals.spriteBatch, Depths.inventory_casing);
                        Globals.spriteBatch.Draw(Globals.GUIinventorycase, screen_pos, null, Color.White, 0, square_origin, inventoryScale, SpriteEffects.None, Depths.inventory_casing);

                        if (items.Count>index)
                        {
                            ii = items[index];
                            Globals.spriteBatch.Draw(spriteSheet[(int)sheetIndexes.item].sourceBitmap, screen_pos, spriteSheet[(int)sheetIndexes.item].Frame(ii.sprite, 0), Color.White, 0, spriteSheet[(int)sheetIndexes.item].GetRotationCenter(ii.sprite, 0), inventoryScale, SpriteEffects.None, Depths.inventory_item);
                            bool eq=false;
                            for (int z = 0; z < 10; z++)
                            {
                                if (equippedItems[z] == index)
                                {
                                    eq = true;
                                }
                            }
                            if(eq)
                                Globals.spriteBatch.DrawString(Globals.GUIFont, "E", screen_pos + new Vector2(-16, -16), Color.Gold, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, Depths.inventory_letters);
                            if(ii.stashable)
                                Globals.spriteBatch.DrawString(Globals.GUIFont, ii.maxQuantity.ToString(), screen_pos + new Vector2(4, 0), Color.Gold, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, Depths.inventory_letters);
                        }
                        //screen_pos.Y = offset.Y - 16;
                        screen_pos.X += element_distance;
                    }
                    screen_pos.Y += element_distance;
                    screen_pos.X = offset.X+20;
                }
                screen_pos = new Vector2(offset.X+20, offset.Y+20);// + 150);

                screen_pos += new Vector2(x * element_distance, y * element_distance);
                Globals.spriteBatch.Draw(Globals.GUIinventoryselect, screen_pos, null, Color.White, 0, outer_square_origin, inventoryScale, SpriteEffects.None, Depths.inventory_selector);

                String itemName = "";

                if (itemIndex >=0 && itemIndex < items.Count)
                {
                    ii = items[itemIndex];
                    itemName = ii.name;
                    String dur = "";
                    String durCol;
                    if (ii.durability / ii.maxDurability >= 0.9)
                    {
                        dur = "Intact";
                        durCol="#0000ff";
                    }
                    else if (ii.durability / ii.maxDurability > 0.7f)
                    {
                        dur = "Slightly damaged";
                        durCol = "#00ff00";
                    }
                    else if (ii.durability / ii.maxDurability > 0.5f)
                    {
                        dur = "Damaged";
                        durCol = "#ffff00";
                    }
                    else if (ii.durability / ii.maxDurability > 0.3f)
                    {
                        dur = "Seriously damaged";
                        durCol = "#ff8800";
                    }
                    else if (ii.durability / ii.maxDurability > 0.1f)
                    {
                        dur = "Almost broken";
                        durCol = "#ff0000";
                    }
                    else
                    {
                        dur = "Unusable";
                        durCol = "#888888";
                    }
                    String quant = "";
                    if (ii.stashable)
                        quant = " x" + ii.maxQuantity.ToString();

                    text = @"
<text font='Arial' color='#ffffff'>
    
  <text font='Arial' color='#00ff00'>" + itemName + quant + @"</text>
  <br/>";
                    if (ii.equippable)
                        text += @"
  <text font='Arial' color='" + durCol + @"'>" + dur + @"</text>
  <br/>";
                    text +=
  ii.description  +
  @"<br/>
</text>";
                    Globals.spriteBatch.Draw(Globals.GUIinventorybg, description_position/*offset+new Vector2(500, 200)*/, null, Color.White, 0, Vector2.Zero, inventoryScale, SpriteEffects.None, Depths.inventory_background);
                    CompiledMarkup cm = Globals.GUIengine.Compile(text, 200);
                    cm.Draw(description_position);//offset+new Vector2(10, 200));
                }
            }


            //draw del rettangolo oggetti equipaggiati
            
            for (int i=0; i < 6; i++)
            {
                Globals.spriteBatch.Draw(Globals.GUIinventorycase, equipPos + equipOffset, null, Color.White, 0, new Vector2(34, 34), inventoryScale, SpriteEffects.None, Depths.inventory_casing);
                if (equippedItems[i] != -1)
                {
                    Globals.spriteBatch.Draw(spriteSheet[(int)sheetIndexes.item].sourceBitmap, equipPos + equipOffset, spriteSheet[(int)sheetIndexes.item].Frame(items[equippedItems[i]].sprite, 0), Color.White, 0, spriteSheet[(int)sheetIndexes.item].GetRotationCenter(items[equippedItems[i]].sprite, 0), 0.5f, SpriteEffects.None, Depths.inventory_item);

                    if (items[equippedItems[i]].stashable)
                        Globals.spriteBatch.DrawString(Globals.GUIFont, items[equippedItems[i]].maxQuantity.ToString(), equipPos + equipOffset + new Vector2(4, 0), Color.Gold, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, Depths.inventory_letters);
                }
                equipOffset += GUIspacing;
            }
            equipOffset = new Vector2(34, 34);

            for (int i=6; i < 8; i++)
            {
                Globals.spriteBatch.Draw(Globals.GUIinventorycase, equipPos + equipOffset, null, Color.White, 0, new Vector2(34, 34), inventoryScale, SpriteEffects.None, Depths.inventory_casing);
                if (equippedItems[i] != -1)
                {
                    Globals.spriteBatch.Draw(spriteSheet[(int)sheetIndexes.item].sourceBitmap, equipPos + equipOffset, spriteSheet[(int)sheetIndexes.item].Frame(items[equippedItems[i]].sprite, 0), Color.White, 0, spriteSheet[(int)sheetIndexes.item].GetRotationCenter(items[equippedItems[i]].sprite, 0), 0.5f, SpriteEffects.None, Depths.inventory_item);

                    if (items[equippedItems[i]].stashable)
                        Globals.spriteBatch.DrawString(Globals.GUIFont, items[equippedItems[i]].maxQuantity.ToString(), equipPos + equipOffset + new Vector2(4, 0), Color.Gold, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, Depths.inventory_letters);
                }
                equipOffset += GUIspacing;
            }
            equipOffset = new Vector2(136, 34);

            for (int i=8; i < 11; i++)
            {
                Globals.spriteBatch.Draw(Globals.GUIinventorycase, equipPos + equipOffset, null, Color.White, 0, new Vector2(34, 34), inventoryScale, SpriteEffects.None, Depths.inventory_casing);
                if (equippedItems[i] != -1)
                {
                    Globals.spriteBatch.Draw(spriteSheet[(int)sheetIndexes.item].sourceBitmap, equipPos + equipOffset, spriteSheet[(int)sheetIndexes.item].Frame(items[equippedItems[i]].sprite, 0), Color.White, 0, spriteSheet[(int)sheetIndexes.item].GetRotationCenter(items[equippedItems[i]].sprite, 0), 0.5f, SpriteEffects.None, Depths.inventory_item);

                    if (items[equippedItems[i]].stashable)
                        Globals.spriteBatch.DrawString(Globals.GUIFont, items[equippedItems[i]].maxQuantity.ToString(), equipPos + equipOffset + new Vector2(4, 0), Color.Gold, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, Depths.inventory_letters);
                }
                equipOffset += GUIspacing;
            }
            equipOffset = new Vector2(34, 0);
            // draw degli oggetti equipaggiati e se stashable del loro numerico
        }

        public void AmmoShift(String direction, ref int hand)
        {
            if (direction == "left")
            {
                if (hand == (int)handleables.bow)
                {
                    for (int i = 0; i < ammoTypeBow.Length; i++)
                        if (ammoTypeBow[i] == equippedItems[(int)equipSlots.ammoBow] && equippedItems[(int)equipSlots.ammoCrossbow] != -1) //ultimo debug
                            if (ammoTypeBow[i + 1] != -1)
                                equippedItems[(int)equipSlots.ammoBow] = ammoTypeBow[i + 1];
                            else if(ammoTypeBow[0] != -1)
                                equippedItems[(int)equipSlots.ammoBow] = ammoTypeBow[0];

                }
                if (hand == (int)handleables.crossbow)
                {
                    for (int i = 0; i < ammoTypeCrossbow.Length; i++)
                        if (ammoTypeCrossbow[i] == equippedItems[(int)equipSlots.ammoCrossbow] && equippedItems[(int)equipSlots.ammoCrossbow]!=-1)
                            if (ammoTypeCrossbow[i + 1] != -1)
                                equippedItems[(int)equipSlots.ammoCrossbow] = ammoTypeCrossbow[i + 1];
                            else if (ammoTypeCrossbow[0] != -1)
                                equippedItems[(int)equipSlots.ammoCrossbow] = ammoTypeCrossbow[0];

                }
            }
            if (direction == "right")
            {
                if (hand == (int)handleables.bow)
                {
                    for (int i = 0; i < ammoTypeBow.Length; i++)
                        if (ammoTypeBow[i] == equippedItems[(int)equipSlots.ammoBow])
                            if (ammoTypeBow[i + 1] != -1)
                            {
                                equippedItems[(int)equipSlots.ammoBow] = ammoTypeBow[i + 1];
                                break;
                            }
                            else //if (ammoTypeBow[ammoTypeBow.Count() - 1] != -1)
                                equippedItems[(int)equipSlots.ammoBow] = ammoTypeBow[0];
                }
                if (hand == (int)handleables.crossbow)
                {
                    for (int i = 0; i < ammoTypeCrossbow.Count(); i++)
                        if (ammoTypeCrossbow[i] == equippedItems[(int)equipSlots.ammoCrossbow])
                            if (ammoTypeCrossbow[i + 1] != -1)
                            {
                                equippedItems[(int)equipSlots.ammoCrossbow] = ammoTypeCrossbow[i + 1];
                                break;
                            }
                            else //if (ammoTypeCrossbow[ammoTypeCrossbow.Count()-1] != -1)
                            {
                                equippedItems[(int)equipSlots.ammoCrossbow] = ammoTypeCrossbow[0];
                                break;
                            }

                }
            }
                
        }

        public void AmmoShift(String direction)
        {
            if (direction == "left")
            {
                {
                    for (int i = 0; i < ammoTypeThrowable.Length; i++)
                        if (ammoTypeThrowable[i] == equippedItems[(int)equipSlots.throwing ])
                            if (i - 1 >= 0)
                                equippedItems[(int)equipSlots.throwing] = ammoTypeThrowable[i - 1];
                            else 
                                while (ammoTypeThrowable[ammoTypeThrowable.Count()-1-i] == -1 && i!=0)
                                    equippedItems[(int)equipSlots.throwing] = ammoTypeThrowable[ammoTypeThrowable.Count() -1- i];
                }
            }
            if (direction == "right" )
            {
                {
                    for (int i = 0; i+1 < ammoTypeThrowable.Length; i++)
                        if (ammoTypeThrowable[i] == equippedItems[(int)equipSlots.throwing])
                            if (ammoTypeThrowable[i + 1] != -1)
                            {
                                equippedItems[(int)equipSlots.throwing] = ammoTypeThrowable[i + 1];
                                break;
                            }
                            else //if (ammoTypeThrowable[ammoTypeThrowable.Count() - 1] != -1)
                                equippedItems[(int)equipSlots.throwing] = ammoTypeThrowable[0];
                }
            }
        }

        public List<ItemInfo> getItemList
        {
            get { return this.items; }
        }

        public bool isVisible
        {
            get { return this.visible; }
        }

        public void Navigate(string dir)
        {
            switch (dir)
            {
                case "down":
                    if (y < 3) y++;
                    break;
                case "up":
                    if (y > 0) y--;
                    break;
                case "left":
                    if (x > 0)
                    {
                        x--;
                    }
                    else
                    {
                        if (n > 1)
                            n--;
                        else n = m;
                    }
                    break;
                case "right":
                    if (x < 4)
                    {
                        x++;
                    }
                    else
                    {
                        if (n < m)
                            n++;
                        else n = 1;
                    }
                    break;
                case "rightScroll":
                    if (n != m)
                        n++;
                    else
                        n = 1;
                    break;
                case "leftScroll":
                    if (n == 1)
                        n = m;
                    else
                        n--;
                    break;
            }
            m = items.Count / 20;
            if (items.Count % 20 != 0 || m == 0)
            {
                m++;
            }

            itemIndex = y * width + x + 20 * (n - 1);
        }

        public void SetQuickBar(int slot)
        {
            quickBar[slot] = items[y * width + x];
        }

        public Item DropSelected(Vector2 pos, Vector2 dir, float depth, float angle)
        {
            Item ii;
            for (int i = 0; i < ammoTypeBow.Length; i++)
            {
                if (ammoTypeBow[i] == itemIndex)
                {
                    ammoTypeBow[i] = ammoTypeBow[i + 1];
                    ammoTypeBow[i + 1] = -1;
                }
                if (ammoTypeBow[i] == itemIndex)
                {
                    ammoTypeCrossbow[i] = ammoTypeCrossbow[i + 1];
                    ammoTypeCrossbow[i + 1] = -1;
                }
                if (ammoTypeThrowable[i] == itemIndex)
                {
                    ammoTypeThrowable[i] = ammoTypeThrowable[i + 1];
                    ammoTypeThrowable[i + 1] = -1;
                }
            }
            //equippedItems[dic.equipmentType[ii.sprite/5]]=itemIndex;
            if (/*itemIndex >= 0 &&*/ itemIndex < items.Count)
            {
                ii = new Item(items[itemIndex], pos, dir, depth, angle, ref spriteSheet, ref currentDungeon, true);
                // "se l'oggetto è equipaggiato, non è possibile dropparlo"
                int search=0;
                for (; search < equippedItems.Count(); )
                {
                    if (equippedItems[search] == itemIndex)
                        break;
                    search++;
                }
           //     if (equippedItems[itemIndex] != itemIndex)
               // {
                    // Riordino degli indici degli oggetti equipaggiati nel caso in cui si droppi un oggetto non equipaggiabile posto prima di un qualsiasi indice rispetto a tutti gli oggetti equipaggiati
                    for (int e = 0; e < equippedItems.Count(); e++)
                    {
                        if (equippedItems[e] > search)
                            equippedItems[e]--;
                    }
                    items.RemoveAt(itemIndex);
              //  }
             //   else
                 //   ii = null;
            }
            else ii = null;
            isChanged = true;
            return ii;
        }

        /// <summary>
        /// Ottiene le statistiche di un oggetto equipaggiato.
        /// </summary>
        /// <param name="equip">Slot desiderato</param>
        /// <returns>Restituisce un'istanza di classe Damage.</returns>
        public Damage GetStats(int equip)
        {
            if (equippedItems[equip] != -1)
            {
                return items[equippedItems[equip]].maxDamage;
            }
            else
            {
                return new Damage();
            }
        }

        public int ammoBow
        {
            get { return equippedItems[(int)equipSlots.ammoBow]; }
        }

        public int ammoCrossbow
        {
            get { return equippedItems[(int)equipSlots.ammoCrossbow]; }
        }

        public int melee
        {
            get { return equippedItems[(int)equipSlots.melee]; }
        }

        public int throwing
        {
            get { return equippedItems[(int)equipSlots.throwing];}
        }

        public int ranged
        {
            get { return equippedItems[(int)equipSlots.ranged]; }
        }
    }
}

