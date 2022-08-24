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
    public class KeyPressed
    {
        public bool ChosenCategoryYetPressed = false;
        public int ChosenSkillTimer = 3000;
        public bool IYetPressed = false;
        public bool LeftYetPressed = false;
        public bool RightYetPressed = false;
        public bool UpYetPressed = false;
        public bool DownYetPressed = false;
        public bool ActionYetPressed = false;
        public bool AttackYetPressed = false;
        public bool ShootingYetPressed = false;
        public bool ThowingYetPressed = false;
        public bool SkillYetPressed = false;
        public bool GuiYetPressed = false;
        public bool Bt0YetPressed = false;
        public bool Bt1YetPressed = false;
        public bool Bt2YetPressed = false;
        public bool Bt3YetPressed = false;
        public bool Bt4YetPressed = false;
        public bool Bt5YetPressed = false;
        public bool Bt6YetPressed = false;
        public bool Bt7YetPressed = false;
        public bool Bt8YetPressed = false;
        public bool Bt9YetPressed = false;
        public bool PreviousSkillYetPressed = false;
        public bool NextSkillYetPressed = false;
        public bool PreviousAmmoYetPressed = false;
        public bool NextAmmoYetPressed = false;
        public bool PreviousThrowYetPressed = false;
        public bool NextThrowYetPressed = false;
        public int Bt0PressionCounter = 0;
        public int Bt1PressionCounter = 0;
        public int Bt2PressionCounter = 0;
        public int Bt3PressionCounter = 0;
        public int Bt4PressionCounter = 0;
        public int Bt5PressionCounter = 0;
        public int Bt6PressionCounter = 0;
        public int Bt7PressionCounter = 0;
        public int Bt8PressionCounter = 0;
        public int Bt9PressionCounter = 0;


        public KeyPressed()
        {
            IYetPressed = false;
            LeftYetPressed = false;
            RightYetPressed = false;
            UpYetPressed = false;
            DownYetPressed = false;
            ActionYetPressed = false;
            AttackYetPressed = false;
            GuiYetPressed = false;
            Bt0YetPressed = false;
            Bt1YetPressed = false;
            Bt2YetPressed = false;
            Bt3YetPressed = false;
            Bt4YetPressed = false;
            Bt5YetPressed = false;
            Bt6YetPressed = false;
            Bt7YetPressed = false;
            Bt8YetPressed = false;
            Bt9YetPressed = false;
        }
   
    }
}


