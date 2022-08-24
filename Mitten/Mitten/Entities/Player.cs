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
using GameControllerState = SlimDX.DirectInput.JoystickState;

namespace Mitten
{
    /// <summary>
    /// Sottoclasse di Entity che racchiude i giocatori umani, siano essi locali o remoti.
    /// </summary>
    [Serializable]
    public class Player : Human//,IDamageble, IAttacker
    {
        bool waiting = true; //Indica che il giocatore è in attesa di essere posizionato nel dungeon
        int skillCategory;
        int skillNumber;
        int Number;
  
        //int destination = 0;
        [NonSerialized] KeyboardState keyState;

        kControls kControls;
        padControls gControls;
        gpadControls xControls;

        Vector2 camera_vector;
        [NonSerialized]public KeyPressed keyPressed = new KeyPressed();

        bool InventoryYetPressed = false;

        #region GUImembers
        float heartScale;
        [NonSerialized]GUIanimation heart;
        [NonSerialized]GUIanimation secondAnimation;   //da implementare, correntemente non istanziata
        Vector2 heartPos;
        Vector2 secondPos;//da implementare, correntemente non istanziata
        Vector2 namePos;
        Vector2 equipPos;
        Vector2 columnCenter;
        Color fontColor;
        #endregion


        [NonSerialized]GameController controller;
        [NonSerialized]GameControllerState controllerState;

        public delegate void UpdateDelegate(GameTime gameTime);
        public UpdateDelegate Update;
        
        /// <summary>
        /// Costruttore per la classe Player. Istanzia un giocatore umano fornendogli un numero identificativo e un nome.
        /// </summary>
        /// <param name="position">Posizione in coordinate del gioco.</param>
        /// <param name="sheet">Riferimento allo spritesheet del giocatore.</param>
        /// <param name="number">Numero identificativo del giocatore.</param>
        /// <param name="name">Nome del giocatore.</param>
        /// <param name="dungeon">Riferimento al livello di gioco corrente.</param>
        public Player(
            Vector2 position, 
            Vector2 direction, 
            float speed, 
            float depth, 
            float health, 
            float rotation,
            ref SpriteSheet[] sheet, 
            ref SpriteSheet[] handSheet,
            ref Dungeon dungeon,
            int number,
            String name,
            GameController controller = null
            )
            : base(
            position,
            40,
            direction,
            speed,
            depth,
            health,
            rotation,
            0,
            ref sheet,
            ref handSheet,
            ref dungeon
            )
        {
            this.name = name;
            Number = number;                                    //numero identificativo del giocatore
            kControls = Settings.Instance.getKControls(number);   //ottiene la lista dei controlli per il giocatore del numero corrispondente
            gControls = Settings.Instance.getGPControls(number);   //ottiene la lista dei controlli per il giocatore del numero corrispondente
            xControls = Settings.Instance.getXGPControls(number);   //ottiene la lista dei controlli per il giocatore del numero corrispondente

            inventory = new Inventory(ref sheet, this.Number, ref damageManager);
            axis = new VAxis(0, 115);
            switch (number)
            {
                case 1: sheetIndex = (int)sheetIndexes.human1; fontColor = Color.Red;
                    shadow = Krypton.ShadowHull.CreateCircle(14, 30); // debug: axis da eliminare
                    shadow.Axis = axis;
                    Globals.krypton.Hulls.Add(shadow);

                    inventory.Add(dic.itemIndex[0]);
                    inventory.equippedItems[(int)equipSlots.shield] = 0;
                    inventory.Add(dic.itemIndex[15]);
                    inventory.equippedItems[(int)equipSlots.ammoCrossbow] = 1;
                    inventory.Add(dic.itemIndex[25]);
                    inventory.equippedItems[(int)equipSlots.ring] = 2;
                    inventory.Add(dic.itemIndex[30]);
                    inventory.equippedItems[(int)equipSlots.ranged] = 3;
                    inventory.Add(dic.itemIndex[16]);
                    inventory.Add(dic.itemIndex[17]);
                    break;
                case 2: sheetIndex = (int)sheetIndexes.human2; fontColor = Color.Blue;
                    shadow = Krypton.ShadowHull.CreateCircle(14, 30); // debug: axis da eliminare
                    shadow.Axis = axis;
                    Globals.krypton.Hulls.Add(shadow);

                    inventory.Add(dic.itemIndex[1]);
                    inventory.equippedItems[(int)equipSlots.shield] = 0;
                    inventory.Add(dic.itemIndex[59]);
                    inventory.equippedItems[(int)equipSlots.melee] = 1;
                    inventory.Add(dic.itemIndex[78]);
                    inventory.equippedItems[(int)equipSlots.armor] = 2;


                    break;

                case 3: sheetIndex = (int)sheetIndexes.human3; fontColor = Color.Green;
                    shadow = Krypton.ShadowHull.CreateCircle(14, 30); // debug: axis da eliminare
                    shadow.Axis = axis;
                    Globals.krypton.Hulls.Add(shadow);

                    inventory.Add(dic.itemIndex[2]);
                    inventory.equippedItems[(int)equipSlots.shield] = 0;
                    inventory.Add(dic.itemIndex[89]);
                    inventory.equippedItems[(int)equipSlots.throwing] = 1;
                    inventory.Add(dic.itemIndex[79]);
                    inventory.equippedItems[(int)equipSlots.armor] = 2;
                    inventory.Add(dic.itemIndex[65]);
                    inventory.equippedItems[(int)equipSlots.amulet] = 3;
                    inventory.Add(dic.itemIndex[58]);
                    inventory.equippedItems[(int)equipSlots.melee] = 4;
                    inventory.Add(dic.itemIndex[91]);
                    inventory.Add(dic.itemIndex[92]);
                    break;
                case 4: sheetIndex = (int)sheetIndexes.human4; fontColor = Color.Yellow;
                    shadow = Krypton.ShadowHull.CreateCircle(14, 30); // debug: axis da eliminare
                    shadow.Axis = axis;
                    Globals.krypton.Hulls.Add(shadow);
                    inventory.Add(dic.itemIndex[3]);
                    inventory.equippedItems[(int)equipSlots.shield] = 0;
                    break;
                default: sheetIndex = (int)sheetIndexes.human1; fontColor = Color.Beige; break;
            }

            this.controller = controller;

            if (controller != null)
            {
                Update = this.UpdateX;

            }
            else
            {
                Update = this.UpdateK;
            }
        }

        /// <summary>
        /// Metodo per ottenere il rettangolo di visuale del giocatore.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns>Restituisce una camera di classe Rectangle per la visualizzazione.</returns>
        public Rectangle GetCamera(Rectangle camera)
        {
            camera_vector = Vector2.Subtract(base.position, new Vector2(camera.Center.X, camera.Center.Y));
            camera.X += (int)(camera_vector.X);
            camera.Y += (int)(camera_vector.Y);
            
            return camera;
        }

        public void Reset(ref Dungeon dungeon)
        {
            currentDungeon.FreeId(this.id);
            currentDungeon.UnsubmitPlayer(this.Number);
            currentDungeon = dungeon;
            this.id = Globals.AssignAnId();
            currentDungeon.SubmitPlayer(this.Number);
            position.X = currentDungeon.GetInitialPosition(destination).X;
            position.Y = currentDungeon.GetInitialPosition(destination).Y;
            destination = 0;
            waiting = true;
            rotationAngle = 0;
        }



        public void UpdateK(GameTime gameTime)
        {
            if (keyPressed.ChosenSkillTimer > 0 && keyPressed.ChosenCategoryYetPressed)
                keyPressed.ChosenSkillTimer -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (keyPressed.ChosenSkillTimer <= 0)
                keyPressed.ChosenCategoryYetPressed = false;
            if (this.alive)
            {
                keyState = Keyboard.GetState();

                if (keyState.IsKeyDown(kControls.Inventory) && !keyPressed.IYetPressed)
                {
                    inventory.Show();
                    keyPressed.IYetPressed = true;
                }
                if (keyState.IsKeyUp(kControls.Inventory))
                {
                    keyPressed.IYetPressed = false;
                }

                if (inventory.isVisible)
                {
                    {
                        if (keyState.IsKeyDown(kControls.Up) && !keyPressed.UpYetPressed)
                        {
                            inventory.Navigate("up");
                            keyPressed.UpYetPressed = true;

                        }
                        if (keyState.IsKeyUp(kControls.Up))
                        {
                            keyPressed.UpYetPressed = false;
                        }
                        if (keyState.IsKeyDown(kControls.Down) && !keyPressed.DownYetPressed)
                        {
                            inventory.Navigate("down");
                            keyPressed.DownYetPressed = true;
                        }
                        if (keyState.IsKeyUp(kControls.Down))
                        {
                            keyPressed.DownYetPressed = false;
                        }
                        if (keyState.IsKeyDown(kControls.Left) && keyState.IsKeyUp(kControls.Run) && !keyPressed.LeftYetPressed)
                        {
                            inventory.Navigate("left");
                            keyPressed.LeftYetPressed = true;
                        }
                        if (keyState.IsKeyUp(kControls.Left))
                        {
                            keyPressed.LeftYetPressed = false;
                        }
                        if (keyState.IsKeyDown(kControls.Right) && keyState.IsKeyUp(kControls.Run) && !keyPressed.RightYetPressed)
                        {
                            inventory.Navigate("right");
                            keyPressed.RightYetPressed = true;
                        }
                        if (keyState.IsKeyUp(kControls.Right))
                        {
                            keyPressed.RightYetPressed = false;
                        }
                        if (keyState.IsKeyDown(kControls.Left) && keyState.IsKeyDown(kControls.Run) && !keyPressed.LeftYetPressed)
                        {
                            inventory.Navigate("leftScroll");
                            keyPressed.LeftYetPressed = true;
                        }
                        /* if (keyState.IsKeyUp(kControls.Left))
                         {
                             keyPressed.LeftYetPressed = false;
                         }*/
                        if (keyState.IsKeyDown(kControls.Right) && keyState.IsKeyDown(kControls.Run) && !keyPressed.RightYetPressed)
                        {
                            inventory.Navigate("rightScroll");
                            keyPressed.RightYetPressed = true;
                        }
                        /*if (keyState.IsKeyUp(kControls.Right))
                        {
                            keyPressed.RightYetPressed = false;
                        }*/
                        if (keyState.IsKeyDown(kControls.Attack) && !keyPressed.AttackYetPressed)
                        {
                            inventory.Action();
                            keyPressed.AttackYetPressed = true;
                        }
                        if (keyState.IsKeyUp(kControls.Attack))
                        {
                            keyPressed.AttackYetPressed = false;
                        }
                        /*if (keyState.IsKeyDown(kControls.Skill))
                        {
                            inventory.Select("skill");
                        }*/
                        if (keyState.IsKeyDown(kControls.Action) && !keyPressed.ActionYetPressed)
                        {
                            Random rand = new Random();
                            Item i = inventory.DropSelected(this.position, this.direction, 0.9f, (float)(rand.NextDouble() * 2 * Math.PI));
                            if (i != null)
                                spawned.Add(i);
                            keyPressed.ActionYetPressed = true;
                        }
                        if (keyState.IsKeyUp(kControls.Action))
                        {
                            keyPressed.ActionYetPressed = false;
                        }
                    }
                }
                else
                {
                    if (keyState.IsKeyDown(kControls.Up) || (keyState.IsKeyDown(kControls.Down)))
                    {
                        if (keyState.IsKeyDown(kControls.Up))
                        {
                            if (keyState.IsKeyDown(kControls.Run))
                            {
                                base.Run(2);
                            }
                            else
                            {
                                base.Walk(1);
                            }

                        }

                        if (keyState.IsKeyDown(kControls.Down))
                        {
                            if (keyState.IsKeyDown(kControls.Run))
                            {
                                base.Walk(-2);
                            }
                            else
                            {
                                base.Walk(-1);
                            }
                        }
                    }
                    else
                        base.Nomovement();


                    if (keyState.IsKeyDown(kControls.Left) || keyState.IsKeyDown(kControls.Right))
                    {
                        if (keyState.IsKeyDown(kControls.Left))
                        {
                            if (keyState.IsKeyDown(kControls.Run))
                                base.Turn(-1);
                            else if (keyState.IsKeyUp(kControls.Run))
                                base.Turn(-0.6f);
                        }



                        if (keyState.IsKeyDown(kControls.Right))
                        {
                            if (keyState.IsKeyDown(kControls.Run))
                                base.Turn(+1);
                            else if (keyState.IsKeyUp(kControls.Run))
                                base.Turn(+0.6f);
                        }
                    }
                    else 
                    { DeTurn(); }

                    if (keyState.IsKeyDown(kControls.Jump))
                    {
                        base.Jump();
                    }

                    if (keyState.IsKeyDown(kControls.Skill) && !ActiveCaster)
                    {
                        base.Skill();
                        //keyPressed.SkillYetPressed = true;
                        ActiveCaster = true;
                    }

                    if (keyState.IsKeyUp(kControls.Skill) && ActiveCaster)
                    {
                        //keyPressed.SkillYetPressed = false;
                        ActiveCaster = false;

                    }

                    if (keyState.IsKeyDown(kControls.Action) && !keyPressed.ActionYetPressed)
                    {
                        this.Action();
                        keyPressed.ActionYetPressed = true;
                    }
                    if (keyState.IsKeyUp(kControls.Action))
                    {
                        keyPressed.ActionYetPressed = false;
                    }

                    if (keyState.IsKeyDown(kControls.Attack) && !keyPressed.AttackYetPressed)
                    {
                        if (inventory.melee != -1)
                        {
                            hand = (int)handleables.sword;
                            base.Attack();
                            keyPressed.AttackYetPressed = true;
                        }
                        else
                        {
                            base.Punch();
                        }
                    }
                    else if (keyState.IsKeyUp(kControls.Attack) && keyPressed.AttackYetPressed)
                    {
                        keyPressed.AttackYetPressed = false;
                    }
                    if (keyState.IsKeyDown(kControls.RangedAttack) && inventory.ranged != -1)
                    {
                        //correggere con la possibilità di equipaggiare l arco
                        if (keyState.IsKeyDown(kControls.Left) && !keyPressed.LeftYetPressed)
                        {
                            inventory.AmmoShift("left", ref hand);
                            keyPressed.LeftYetPressed = true;
                        }
                        else if (keyState.IsKeyDown(kControls.Right) && !keyPressed.RightYetPressed)
                        {
                            inventory.AmmoShift("right", ref hand);
                            keyPressed.RightYetPressed = true;
                        }
                        keyPressed.ShootingYetPressed = true;
                    }

                    if (keyState.IsKeyUp(kControls.RangedAttack) && inventory.ranged != -1 && keyPressed.ShootingYetPressed)
                    {
                        if (inventory.ammoCrossbow != -1 && hand == (int)handleables.crossbow && keyPressed.ShootingYetPressed)
                            base.Shoot();
                        else if (inventory.ranged != -1)
                            hand = (int)handleables.crossbow;
                        keyPressed.ShootingYetPressed = false;


                    }



                    if (keyState.IsKeyDown(kControls.Throw))
                    {
                        if (keyState.IsKeyDown(kControls.Left) && !keyPressed.LeftYetPressed)
                        {
                            inventory.AmmoShift("left");
                            keyPressed.LeftYetPressed = true;
                        }
                        if (keyState.IsKeyDown(kControls.Right) && !keyPressed.RightYetPressed)
                        {
                            inventory.AmmoShift("right");
                            keyPressed.RightYetPressed = true;
                        }
                        keyPressed.ThowingYetPressed = true;
                        if (power <= 10)
                            power += 0.15f;
                        //suono alla potenza massima raggiunta
                    }

                    if (keyState.IsKeyUp(kControls.Throw) && inventory.throwing != -1 && keyPressed.ThowingYetPressed)
                    {
                        base.power = power;
                        if (keyPressed.ThowingYetPressed && power > 3)
                        {
                            base.Throw();

                        }
                        keyPressed.ThowingYetPressed = false;
                    }

                    if (keyState.IsKeyUp(kControls.Left))
                    {
                        keyPressed.LeftYetPressed = false;
                    }
                    if (keyState.IsKeyUp(kControls.Right))
                    {
                        keyPressed.RightYetPressed = false;
                    }

                    if (keyState.IsKeyDown(kControls.Skill))
                    {
                        base.Skill();
                    }

                    if (keyState.IsKeyDown(kControls.Parry))
                    {
                        base.Parry();
                    }

                    if (keyState.IsKeyDown(kControls.Kick))
                    {
                        base.Kick();
                    }

                    if (keyState.IsKeyDown(kControls.Bt0) && !keyPressed.Bt0YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 0;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 0;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt0YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt0) && keyPressed.Bt0YetPressed)
                    {
                        keyPressed.Bt0YetPressed = false;

                    }
                    if (keyState.IsKeyDown(kControls.Bt1) && !keyPressed.Bt1YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt1YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt1) && keyPressed.Bt1YetPressed)
                    {
                        keyPressed.Bt1YetPressed = false;
                    }
                    if (keyState.IsKeyDown(kControls.Bt2) && !keyPressed.Bt2YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 2;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 2;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt2YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt2) && keyPressed.Bt2YetPressed)
                    {
                        keyPressed.Bt2YetPressed = false;

                    }
                    if (keyState.IsKeyDown(kControls.Bt3) && !keyPressed.Bt3YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 3;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 3;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt3YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt3) && keyPressed.Bt3YetPressed)
                    {
                        keyPressed.Bt3YetPressed = false;

                    }
                    if (keyState.IsKeyDown(kControls.Bt4) && !keyPressed.Bt4YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt4YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt4) && keyPressed.Bt4YetPressed)
                    {
                        keyPressed.Bt4YetPressed = false;

                    }
                   /* if (keyState.IsKeyDown(kControls.Bt5) && !keyPressed.Bt5YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt5YetPressed = true;
                    }
                    else if (keyState.IsKeyUp(kControls.Bt5) && keyPressed.Bt5YetPressed)
                    {
                        keyPressed.Bt5YetPressed = false;
                    
                    }*/
                    /*   if (keyPressed.Bt0YetPressed)
                       {
                           skillNumber = keyPressed.Bt0PressionCounter++;
                           skillNumber %= 5; //occorre tener conto del reale numero di skill di categoria 0
                       }
                       else
                       {
                           keyPressed.Bt0YetPressed = true;
                           skillCategory = 0;
                           keyPressed.Bt0PressionCounter = 0;
                            
                       }
                   }
                   else if (keyState.IsKeyUp(kControls.Bt0) && keyPressed.Bt0YetPressed)
                   {
                       keyPressed.Bt0YetPressed = false;
                       keyPressed.Bt0PressionCounter = 0;
                   }*/

                }

                //debug ambiente musicale
                if (keyState.IsKeyDown(kControls.Bt8) && !keyPressed.Bt8YetPressed)
                {
                    Globals.Frequencies.PlaySong(Globals.Frequencies.getCurrentSong - 1);
                    keyPressed.Bt8YetPressed = true;
                }
                else if (keyState.IsKeyUp(kControls.Bt8) && keyPressed.Bt8YetPressed)
                {
                    keyPressed.Bt8YetPressed = false;
                }
                if (keyState.IsKeyDown(kControls.Bt9) && !keyPressed.Bt9YetPressed)
                {
                    Globals.Frequencies.PlaySong(Globals.Frequencies.getCurrentSong + 1);
                    keyPressed.Bt9YetPressed = true;
                }
                else if (keyState.IsKeyUp(kControls.Bt9) && keyPressed.Bt9YetPressed)
                {
                    keyPressed.Bt9YetPressed = false;
                }

                if (inventory.isChanged)
                {
                    base.damage = inventory.GetStats((int)equipSlots.melee);
                    inventory.isChanged = false;
                }

                if (launched != -1)
                {
                    inventory.DecrementAmmo(launched);
                    launched = -1;
                }
                if (inventory.isChanged)
                    base.inventory = inventory;
            }
            
            heart.Stop = !alive;

            //aggiornamento animazioni della GUI
            heart.Update(gameTime, (int)(100 * damageManager.health / 100));

            skillManager.CurrentSkill = skillCategory * Globals.nSkill + skillNumber;

            base.Update(gameTime);
        }

        public void UpdateX(GameTime gameTime)
        {

            //int h = controllerState.GetPointOfViewControllers()[0];

            if (keyPressed.ChosenSkillTimer > 0 && keyPressed.ChosenCategoryYetPressed)
                keyPressed.ChosenSkillTimer -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (keyPressed.ChosenSkillTimer <= 0)
                keyPressed.ChosenCategoryYetPressed = false;
            if (this.alive)
            {
                controllerState = controller.GetState();

                if (controllerState.IsPressed(xControls.Inventory) && !keyPressed.IYetPressed)
                {
                    inventory.Show();
                    keyPressed.IYetPressed = true;
                }
                if (controllerState.IsReleased(xControls.Inventory))
                {
                    keyPressed.IYetPressed = false;
                }

                if (inventory.isVisible)
                {
                    if (!controllerState.IsPressed(xControls.Run))
                    {
                        if (controllerState.GetPointOfViewControllers()[0] == 0 && !keyPressed.UpYetPressed)
                        {
                            inventory.Navigate("up");
                            keyPressed.UpYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 0 && keyPressed.UpYetPressed)
                        {
                            keyPressed.UpYetPressed = false;
                        }
                        if (controllerState.GetPointOfViewControllers()[0] == 9000 && !keyPressed.RightYetPressed)
                        {
                            inventory.Navigate("right");
                            keyPressed.RightYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 9000 && keyPressed.RightYetPressed)
                        {
                            keyPressed.RightYetPressed = false;
                        }
                        if (controllerState.GetPointOfViewControllers()[0] == 18000 && !keyPressed.DownYetPressed)
                        {
                            inventory.Navigate("down");
                            keyPressed.DownYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 18000 && keyPressed.DownYetPressed)
                        {
                            keyPressed.DownYetPressed = false;
                        }
                        if (controllerState.GetPointOfViewControllers()[0] == 27000 && !keyPressed.LeftYetPressed)
                        {
                            inventory.Navigate("left");
                            keyPressed.LeftYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 27000 && keyPressed.LeftYetPressed)
                        {
                            keyPressed.LeftYetPressed = false;
                        }
                    }
                    else if (controllerState.IsPressed(xControls.Run))
                    {

                        if (controllerState.GetPointOfViewControllers()[0] == 9000 && !keyPressed.RightYetPressed)
                        {
                            inventory.Navigate("rightscroll");
                            keyPressed.RightYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 9000 && keyPressed.RightYetPressed)
                        {
                            keyPressed.RightYetPressed = false;
                        }
                        if (controllerState.GetPointOfViewControllers()[0] == 27000 && !keyPressed.LeftYetPressed)
                        {
                            inventory.Navigate("leftscroll");
                            keyPressed.LeftYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 27000 && keyPressed.LeftYetPressed)
                        {
                            keyPressed.LeftYetPressed = false;
                        }

                    }
                        if (controllerState.IsPressed(xControls.Attack) && !keyPressed.AttackYetPressed)
                        {
                            inventory.Action();
                            keyPressed.AttackYetPressed = true;
                        }
                        if (controllerState.IsReleased(xControls.Attack))
                        {
                            keyPressed.AttackYetPressed = false;
                        }
                        /*if (controllerState.IsPressed(xControls.Skill))
                        {
                            inventory.Select("skill");
                        }*/
                        if (controllerState.IsPressed(xControls.Action) && !keyPressed.ActionYetPressed)
                        {
                            Random rand = new Random();
                            Item i = inventory.DropSelected(this.position, this.direction, 0.9f, (float)(rand.NextDouble() * 2 * Math.PI));
                            if (i != null)
                                spawned.Add(i);
                            keyPressed.ActionYetPressed = true;
                        }
                        if (controllerState.IsReleased(xControls.Action))
                        {
                            keyPressed.ActionYetPressed = false;
                        }
                }
                else
                {
                    if (controllerState.Y < 0 || (controllerState.Y > 0))
                    {
                        if (controllerState.Y < 0)
                        {
                            if (!controllerState.IsPressed(xControls.Run))
                            {
                                base.Run(2);
                            }
                            else
                            {
                                base.Walk(1);
                            }
                        }
                        else if (controllerState.Y > 0)
                        {
                            if (!controllerState.IsPressed(xControls.Run))
                            {
                                base.Walk(-2);
                            }
                            else
                            {
                                base.Walk(-1);
                            }
                        }
                    }
                    else
                        base.Nomovement();

                    if (controllerState.Z != 1)
                    {
                        if (controllerState.Z > 1)
                        {
                            if (!controllerState.IsPressed(xControls.Run))
                            {
                                base.Turn(+1);
                            }
                            else
                            {
                                base.Turn(+0.6f);
                            }
                        }

                        if (controllerState.Z < 1)
                        {
                            if (!controllerState.IsPressed(xControls.Run))
                            {
                                base.Turn(-1);
                            }
                            else
                            {
                                base.Turn(-0.6f);
                            }
                        }
                    }
                    else
                    { DeTurn(); }

                    if (controllerState.IsPressed(xControls.Jump))
                        base.Jump();

                    if (controllerState.IsPressed(xControls.Skill) && !ActiveCaster)
                    {
                        base.Skill();
                        keyPressed.SkillYetPressed = true;
                        ActiveCaster = true;
                    }

                    if (controllerState.IsReleased(xControls.Skill) && ActiveCaster)
                    {
                        keyPressed.SkillYetPressed = false;
                        ActiveCaster = false;
                    }

                    if (controllerState.IsPressed(xControls.Action) && !keyPressed.ActionYetPressed)
                    {
                        this.Action();
                        keyPressed.ActionYetPressed = true;
                    }
                    if (controllerState.IsReleased(xControls.Action))
                        keyPressed.ActionYetPressed = false;

                    if (controllerState.IsPressed(xControls.Attack) && !keyPressed.AttackYetPressed)
                    {
                        if (inventory.melee != -1)
                        {
                            hand = (int)handleables.sword;
                            base.Attack();
                            keyPressed.AttackYetPressed = true;
                        }
                        else
                            base.Punch();
                    }
                    else if (controllerState.IsReleased(xControls.Attack) && keyPressed.AttackYetPressed)
                        keyPressed.AttackYetPressed = false;

                    if (controllerState.IsPressed(xControls.RangedAttack) && inventory.ranged != -1)
                    {
                        if (inventory.ammoCrossbow != -1 && hand == (int)handleables.crossbow && keyPressed.ShootingYetPressed)
                            base.Shoot();
                        else if (inventory.ranged != -1)
                            hand = (int)handleables.crossbow;
                        keyPressed.ShootingYetPressed = false;
                    }
                        //correggere con la possibilità di equipaggiare l arco
                        if (controllerState.GetPointOfViewControllers()[0] == 9000 && !keyPressed.RightYetPressed)
                        {
                            inventory.AmmoShift("right", ref hand);
                            keyPressed.RightYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 9000 && keyPressed.RightYetPressed)
                            keyPressed.RightYetPressed = false;


                        if (controllerState.GetPointOfViewControllers()[0] == 27000 && !keyPressed.LeftYetPressed)
                        {
                            inventory.AmmoShift("left", ref hand);
                            keyPressed.LeftYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 27000 && keyPressed.LeftYetPressed)
                            keyPressed.LeftYetPressed = false;

                        keyPressed.ShootingYetPressed = true;
                    

                    /*if (controllerState.IsPressed(xControls.RangedAttack) && inventory.ranged != -1 && keyPressed.ShootingYetPressed)
                    {
                        if (inventory.ammoCrossbow != -1 && hand == (int)handleables.crossbow && keyPressed.ShootingYetPressed)
                            base.Shoot();
                        else if (inventory.ranged != -1)
                            hand = (int)handleables.crossbow;
                        keyPressed.ShootingYetPressed = false;
                    }*/
                        if (controllerState.GetPointOfViewControllers()[0] == 0 && !keyPressed.UpYetPressed)
                        {
                            inventory.AmmoShift("left");
                            keyPressed.UpYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 0 && keyPressed.UpYetPressed)
                            keyPressed.UpYetPressed = false;

                        if (controllerState.GetPointOfViewControllers()[0] == 18000 && !keyPressed.DownYetPressed)
                        {
                            inventory.AmmoShift("right");
                            keyPressed.DownYetPressed = true;
                        }
                        else if (controllerState.GetPointOfViewControllers()[0] != 18000 && keyPressed.DownYetPressed)
                            keyPressed.DownYetPressed = false;

                    if (controllerState.IsPressed(xControls.Throw))
                    {
                        
                        keyPressed.ThowingYetPressed = true;
                        if (power <= 10)
                            power += 0.15f;
                        //suono alla potenza massima raggiunta
                    }

                    if (controllerState.IsReleased(xControls.Throw) && inventory.throwing != -1 && keyPressed.ThowingYetPressed)
                    {
                        base.power = power;
                        if (keyPressed.ThowingYetPressed && power > 3)
                            base.Throw();
                        keyPressed.ThowingYetPressed = false;
                    }

                    /*if (controllerState.IsReleased(xControls.Left))
                        keyPressed.LeftYetPressed = false;
                    if (controllerState.IsReleased(xControls.Right))
                        keyPressed.RightYetPressed = false;*/

                    if (controllerState.IsPressed(xControls.Skill))
                        base.Skill();

                    if (controllerState.IsPressed(xControls.Parry))
                        base.Parry();

                    if (controllerState.IsPressed(xControls.Kick))
                        base.Kick();

                    if (controllerState.X < 0 && !keyPressed.PreviousSkillYetPressed)
                    {
                        skillNumber--;
                        if (skillNumber < 0)
                            skillNumber = 0;
                        keyPressed.PreviousSkillYetPressed = true;
                        //keyPressed.NextSkillYetPressed = false;
                    }
                    else if (controllerState.X > 0 && !keyPressed.NextSkillYetPressed)
                    {
                        skillNumber++;
                        if (skillNumber > Globals.nSkill - 1)
                            skillNumber = Globals.nSkill - 1;
                      //  keyPressed.PreviousSkillYetPressed = false;
                        keyPressed.NextSkillYetPressed = true;
                    }
                    else if (controllerState.X ==0)
                    {
                        keyPressed.PreviousSkillYetPressed = false;
                        keyPressed.NextSkillYetPressed = false;
                    }
                    /*
                    if (controllerState.IsPressed(xControls.Bt0) && !keyPressed.Bt0YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 0;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 0;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt0YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt0) && keyPressed.Bt0YetPressed)
                    {
                        keyPressed.Bt0YetPressed = false;

                    }
                    if (controllerState.IsPressed(xControls.Bt1) && !keyPressed.Bt1YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt1YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt1) && keyPressed.Bt1YetPressed)
                    {
                        keyPressed.Bt1YetPressed = false;

                    }
                    if (controllerState.IsPressed(xControls.Bt1) && !keyPressed.Bt1YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }

                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt1YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt1) && keyPressed.Bt1YetPressed)
                    {
                        keyPressed.Bt1YetPressed = false;

                    }
                    if (controllerState.IsPressed(xControls.Bt3) && !keyPressed.Bt3YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt3YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt3) && keyPressed.Bt3YetPressed)
                    {
                        keyPressed.Bt3YetPressed = false;

                    }
                    if (controllerState.IsPressed(xControls.Bt4) && !keyPressed.Bt4YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt4YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt4) && keyPressed.Bt4YetPressed)
                    {
                        keyPressed.Bt4YetPressed = false;

                    }
                    if (controllerState.IsPressed(xControls.Bt5) && !keyPressed.Bt5YetPressed)
                    {
                        if (!keyPressed.ChosenCategoryYetPressed)
                        {
                            skillCategory = 1;
                            keyPressed.ChosenCategoryYetPressed = true;
                        }
                        else
                        {
                            skillNumber = 1;
                            keyPressed.ChosenCategoryYetPressed = false;
                        }

                        keyPressed.Bt5YetPressed = true;
                    }
                    else if (controllerState.IsReleased(xControls.Bt5) && keyPressed.Bt5YetPressed)
                    {
                        keyPressed.Bt5YetPressed = false;

                    }
                    /*   if (keyPressed.Bt0YetPressed)
                       {
                           skillNumber = keyPressed.Bt0PressionCounter++;
                           skillNumber %= 5; //occorre tener conto del reale numero di skill di categoria 0
                       }
                       else
                       {
                           keyPressed.Bt0YetPressed = true;
                           skillCategory = 0;
                           keyPressed.Bt0PressionCounter = 0;
                            
                       }
                   }
                   else if (controllerState.IsReleased(xControls.Bt0) && keyPressed.Bt0YetPressed)
                   {
                       keyPressed.Bt0YetPressed = false;
                       keyPressed.Bt0PressionCounter = 0;
                   }
                    */
                }

                //debug ambiente musicale
                /*
                if (controllerState.IsPressed(xControls.Bt8) && !keyPressed.Bt8YetPressed)
                {
                    Globals.Frequencies.PlaySong(Globals.Frequencies.getCurrentSong - 1);
                    keyPressed.Bt8YetPressed = true;
                }
                else if (controllerState.IsReleased(xControls.Bt8) && keyPressed.Bt8YetPressed)
                {
                    keyPressed.Bt8YetPressed = false;
                }
                if (controllerState.IsPressed(xControls.Bt9) && !keyPressed.Bt9YetPressed)
                {
                    Globals.Frequencies.PlaySong(Globals.Frequencies.getCurrentSong + 1);
                    keyPressed.Bt9YetPressed = true;
                }
                else if (controllerState.IsReleased(xControls.Bt9) && keyPressed.Bt9YetPressed)
                {
                    keyPressed.Bt9YetPressed = false;
                }
                 * */

                if (inventory.isChanged)
                {
                    base.damage = inventory.GetStats((int)equipSlots.melee);
                    inventory.isChanged = false;
                }

                if (launched != -1)
                {
                    inventory.DecrementAmmo(launched);
                    launched = -1;
                }
                if (inventory.isChanged)
                    base.inventory = inventory;
            }

            heart.Stop = !alive;
            //aggiornamento animazioni della GUI
            heart.Update(gameTime, (int)(100 * damageManager.health / 100));

            skillManager.CurrentSkill = skillCategory * Globals.nSkill + skillNumber;

            base.Update(gameTime);
        }

        /// <summary>
        /// Aggiorna i controlli recuperandoli di nuovo dai Settings.
        /// </summary>
        public void resetControls()
        {
            kControls = Settings.Instance.getKControls(Number);
            xControls = Settings.Instance.getXGPControls(Number);
        }

        /// <summary>
        /// Metodo per il disegno del giocatore su schermo.
        /// </summary>
        /// <param name="Globals.spriteBatch"></param>
        /// <param name="camera"></param>
        public void draw(Rectangle camera)
        {
            base.Draw(camera);
        }

        public void drawGUI(Viewport view)
        {
            //0.65f è un valore sperimentale, usato a causa della natura dello spriteFont. Può cessare di essere utile o cambiare in caso di cambiamento del font.
            
            Vector2 strlen;
            String dname = this.currentDungeon.getName;
            Globals.spriteBatch.DrawString(Globals.GUIFont, this.name, namePos, fontColor, 0.0f, new Vector2(0), 0.65f, SpriteEffects.None, Depths.playerGUI);
            heart.Draw(heartPos, heartScale);
            //secondaAnimation.Draw(ref Globals.spriteBatch, offset, 1.0f);
            strlen = Globals.GUIFont.MeasureString(dname) * 0.65f;
            Globals.spriteBatch.DrawString(Globals.GUIFont, dname, new Vector2((view.Width - (int)strlen.Length()) / 2, 10), Color.Gainsboro, 0.0f, new Vector2(0), 0.65f, SpriteEffects.None, Depths.playerGUI);
            Globals.spriteBatch.Draw(Globals.GUIborder, new Rectangle(view.X, view.Y, view.Width, 1), null, Color.Black, 0.0f, new Vector2(0, 0), SpriteEffects.None, Depths.playerGUI);
            Globals.spriteBatch.Draw(Globals.GUIborder, new Rectangle(view.X, view.Y, 1, view.Height), null, Color.Black, 0.0f, new Vector2(0, 0), SpriteEffects.None, Depths.playerGUI);
            Globals.spriteBatch.Draw(Globals.GUIborder, new Rectangle((view.X + view.Width - 1), view.Y, 1, view.Height), null, Color.Black, 0.0f, new Vector2(0, 0), SpriteEffects.None, Depths.playerGUI);
            Globals.spriteBatch.Draw(Globals.GUIborder, new Rectangle(view.X, view.Y + view.Height - 1, view.Width, 1), null, Color.Black, 0.0f, new Vector2(0, 0), SpriteEffects.None, Depths.playerGUI);
            inventory.Draw(equipPos);
        }

        public void CalculateGUI(Viewport view)
        {
            //da usare in caso di reset delle impostazioni
            if (Globals.players == 1)
            {
                heart = new GUIanimation(ref sheet, (int)sheetIndexes.GUIheart);
            }
            else
            {
                heart = new GUIanimation(ref sheet, (int)sheetIndexes.GUIheart);
            }
            if (Settings.Instance.camera_option == 1 || Settings.Instance.camera_option == 3)
            {
                columnCenter = new Vector2((Settings.Instance.HorizontalResolution * ((float)Number / Globals.players)) - (Settings.Instance.HorizontalResolution / (Globals.players * 2)), Settings.Instance.VerticalResolution / 2);
                heartPos = new Vector2(view.X + (Number - 1) * view.Width / Globals.players + 40, view.Height - 80);
                namePos = new Vector2(columnCenter.X-0.65f*Globals.GUIFont.MeasureString(name).X/2, view.Height - 120);
                equipPos = new Vector2(columnCenter.X-Globals.GUIinventorycase.Width*1.5f, view.Height - 65);
                
                heartScale = 1.0f;
            }
            else
            {
                columnCenter = new Vector2(view.Bounds.Center.X, view.Bounds.Center.Y);
                heartPos = new Vector2(view.X + (Number - 1) * view.Width / 4, view.Height - 120);
                namePos = new Vector2(view.X + (Number - 1) * view.Width / 4, view.Height - 150);
                equipPos = new Vector2(view.X + (Number - 1) * view.Width / 4, 50);
                
                heartScale = 1.0f;
            }
        }


        #region properties

        /// <summary>
        /// Ottiene lo stato del giocatore in relazione all'aggiornamento della camera e alla progressione dei livelli
        /// </summary>
        public bool ActivePlayer
        {
            get { return alive; }
        }

        /// <summary>
        /// Ottiene la destinazione del dungeon successivo o precedente del giocatore
        /// </summary>
        public int getDestination
        {
            get { return this.destination; }
        }

        /// <summary>
        /// Imposta il dungeon corrente del giocatore
        /// </summary>
        public Dungeon setCurrentDungeon
        {
            set { this.currentDungeon = value; }
        }
        #endregion
    }
}
