using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Mitten
{
    public class Settings
    {
        private static Settings instance;
        
        public bool default_settings;
        public int h_res;
        public int v_res;
        public Rectangle camera;
        public short camera_option; //1 = single camera; 2 = split camera; 3 = shared camera;
        public bool windowed;

        public Keys Menu=Keys.Escape;
        public Keys Enter=Keys.Enter;
        public kControls singlePlayerKeyboardControls;
        public padControls singlePlayerGamepadControls;
        public gpadControls singlePlayerXGamepadControls;
        public String singlePlayerName;
        public Color singlePlayerColor;
        public kControls[] keyboardControls = new kControls[4];
        public padControls[] gamepadControls = new padControls[4];
        public gpadControls[] xgamepadControls = new gpadControls[4];
        public String[] player_names = new String[4];
        public Color[] player_colors = new Color[4];

        //audio settings;
        public int effects_volume;
        public int music_volume;
        public bool no_effects;
        public bool no_music;

        //network settings
        public int show_messages;
                
        private Settings()
        {
            default_settings = true;
            windowed = true;
            h_res = 1900;
            v_res = 1000;
            camera_option = 2;
            singlePlayerName = "The Prisoner";
            player_names[0] = "Player 1";
            player_names[1] = "Player 2";
            player_names[2] = "Player 3";
            player_names[3] = "Player 4";
            singlePlayerColor = Color.White;
            player_colors[0] = Color.Red;
            player_colors[1] = Color.Blue;
            player_colors[2] = Color.Green;
            player_colors[3] = Color.Yellow;
            show_messages = 0;

            singlePlayerKeyboardControls.Attack = Keys.A;
            singlePlayerKeyboardControls.RangedAttack = Keys.W;
            singlePlayerKeyboardControls.Throw = Keys.Q;
            singlePlayerKeyboardControls.Skill = Keys.S;
            singlePlayerKeyboardControls.Bt0 = Keys.D0;
            singlePlayerKeyboardControls.Bt1 = Keys.D1;
            singlePlayerKeyboardControls.Bt2 = Keys.D2;
            singlePlayerKeyboardControls.Bt3 = Keys.D3;
            singlePlayerKeyboardControls.Bt4 = Keys.D4;
            singlePlayerKeyboardControls.Bt5 = Keys.D5;
            singlePlayerKeyboardControls.Bt6 = Keys.D6;
            singlePlayerKeyboardControls.Bt7 = Keys.D7;
            singlePlayerKeyboardControls.Bt8 = Keys.D8;
            singlePlayerKeyboardControls.Bt9 = Keys.D9;
            singlePlayerKeyboardControls.Right = Keys.Right;
            singlePlayerKeyboardControls.Left = Keys.Left;
            singlePlayerKeyboardControls.Up = Keys.Up;
            singlePlayerKeyboardControls.Down = Keys.Down;
            singlePlayerKeyboardControls.Jump = Keys.Space;
            singlePlayerKeyboardControls.Run = Keys.LeftShift;
            singlePlayerKeyboardControls.Action = Keys.LeftControl;
            singlePlayerKeyboardControls.Parry = Keys.D;
            singlePlayerKeyboardControls.Kick = Keys.F;
            singlePlayerKeyboardControls.Next = Keys.X;
            singlePlayerKeyboardControls.Previous = Keys.Z;
            singlePlayerKeyboardControls.Inventory = Keys.I;
            singlePlayerKeyboardControls.Menu = Keys.Escape;

            singlePlayerGamepadControls.Attack = Buttons.A;
            singlePlayerGamepadControls.RangedAttack = Buttons.X;
            singlePlayerGamepadControls.Throw = Buttons.Y;
            singlePlayerGamepadControls.Skill = Buttons.B;
            singlePlayerGamepadControls.Up = Buttons.LeftThumbstickUp;
            singlePlayerGamepadControls.Down = Buttons.LeftThumbstickDown;
            singlePlayerGamepadControls.Left = Buttons.LeftThumbstickLeft;
            singlePlayerGamepadControls.Right = Buttons.LeftThumbstickRight;
            singlePlayerGamepadControls.Run = Buttons.LeftStick;
            singlePlayerGamepadControls.Jump = Buttons.LeftTrigger;
            singlePlayerGamepadControls.Parry = Buttons.RightTrigger;
            singlePlayerGamepadControls.Kick = Buttons.RightShoulder;
            singlePlayerGamepadControls.Action = Buttons.LeftShoulder;
            singlePlayerGamepadControls.Next = Buttons.DPadRight;
            singlePlayerGamepadControls.Previous = Buttons.DPadLeft;
            singlePlayerGamepadControls.NextCategory = Buttons.DPadUp;
            singlePlayerGamepadControls.PreviousCategory = Buttons.DPadDown;
            singlePlayerGamepadControls.Inventory = Buttons.Start;
            singlePlayerGamepadControls.Menu = Buttons.Back;

            singlePlayerXGamepadControls.Attack = (int)xgbuttons.A;
            singlePlayerXGamepadControls.Skill = (int)xgbuttons.B;
            //singlePlayerXGamepadControls.Up = (int)xgbuttons.LeftThumbUp;
            //singlePlayerXGamepadControls.Down = (int)xgbuttons.LeftThumbDown;
            //singlePlayerXGamepadControls.Left = (int)xgbuttons.LeftThumbLeft;
            //singlePlayerXGamepadControls.Right = (int)xgbuttons.LeftThumbRight;
            singlePlayerXGamepadControls.Run = (int)xgbuttons.LeftThumbPress;
            singlePlayerXGamepadControls.Jump = (int)xgbuttons.LeftTrigger;
            singlePlayerXGamepadControls.Parry = (int)xgbuttons.RightTrigger;
            singlePlayerXGamepadControls.Kick = (int)xgbuttons.RightShoulder;
            singlePlayerXGamepadControls.Action = (int)xgbuttons.LeftShoulder;
            singlePlayerXGamepadControls.Next = (int)xgbuttons.DPadRight;
            singlePlayerXGamepadControls.Previous = (int)xgbuttons.DPadLeft;
            singlePlayerXGamepadControls.Inventory = (int)xgbuttons.DPadDown;
            singlePlayerXGamepadControls.Menu = (int)xgbuttons.DPadUp;
            singlePlayerXGamepadControls.Throw = (int)xgbuttons.X;
            singlePlayerXGamepadControls.RangedAttack = (int)xgbuttons.Y;
            //singlePlayerXGamepadControls.NextCategory = (int)xgbuttons.RightThumbRight;
            //singlePlayerXGamepadControls.PreviousCategory = (int)xgbuttons.RightThumbLeft;


            keyboardControls[0].Right = Keys.Right;
            keyboardControls[0].Left = Keys.Left;
            keyboardControls[0].Up = Keys.Up;
            keyboardControls[0].Down = Keys.Down;
            keyboardControls[0].Attack = Keys.OemMinus;
            keyboardControls[0].RangedAttack = Keys.J;
            keyboardControls[0].Throw = Keys.K;
            keyboardControls[0].Skill = Keys.OemPeriod;
            keyboardControls[0].Jump = Keys.RightControl;
            keyboardControls[0].Run = Keys.RightShift;
            keyboardControls[0].Parry = Keys.OemComma;
            keyboardControls[0].Kick = Keys.M;
            keyboardControls[0].Action = Keys.N;
            keyboardControls[0].Bt0 = Keys.NumPad0;
            keyboardControls[0].Bt1 = Keys.NumPad1;
            keyboardControls[0].Bt2 = Keys.NumPad2;
            keyboardControls[0].Bt3 = Keys.NumPad3;
            keyboardControls[0].Bt4 = Keys.NumPad4;
            keyboardControls[0].Bt5 = Keys.NumPad5;
            keyboardControls[0].Bt6 = Keys.NumPad6;
            keyboardControls[0].Bt7 = Keys.NumPad7;
            keyboardControls[0].Bt8 = Keys.NumPad8;
            keyboardControls[0].Bt9 = Keys.NumPad9;
            keyboardControls[0].Next = Keys.P;
            keyboardControls[0].Previous = Keys.O;
            keyboardControls[0].Inventory = Keys.I;
            keyboardControls[0].Menu = Keys.L;


            keyboardControls[1].Right = Keys.F;
            keyboardControls[1].Left = Keys.S;
            keyboardControls[1].Up = Keys.E;
            keyboardControls[1].Down = Keys.D;
            keyboardControls[1].Attack = Keys.D1;
            keyboardControls[1].RangedAttack = Keys.D2 ;
            keyboardControls[1].Throw = Keys.D3;
            keyboardControls[1].Skill = Keys.D4;
            keyboardControls[1].Jump = Keys.Q;
            keyboardControls[1].Run = Keys.W;
            keyboardControls[1].Parry = Keys.A;
            keyboardControls[1].Kick = Keys.Z;
            keyboardControls[1].Action = Keys.R;
            keyboardControls[1].Bt0 = Keys.D5;
            keyboardControls[1].Bt1 = Keys.D6;
            keyboardControls[1].Bt2 = Keys.D7;
            keyboardControls[1].Bt3 = Keys.D8;
            keyboardControls[1].Bt4 = Keys.D9;
            /*keyboardControls[1].Bt5 = Keys.D0;
            keyboardControls[1].Bt6 = Keys.D0;
            keyboardControls[1].Bt7 = Keys.D0;
            keyboardControls[1].Bt8 = Keys.D0;
            keyboardControls[1].Bt9 = Keys.D0;*/
            keyboardControls[1].Previous = Keys.H;
            keyboardControls[1].Inventory = Keys.Y;
            keyboardControls[1].Menu = Keys.B;

            keyboardControls[2].Right = Keys.F;
            keyboardControls[2].Left = Keys.S;
            keyboardControls[2].Up = Keys.E;
            keyboardControls[2].Down = Keys.D;
            keyboardControls[2].Attack = Keys.OemPipe;
            keyboardControls[2].RangedAttack = Keys.G;
            keyboardControls[2].Skill = Keys.Tab;
            keyboardControls[2].Jump = Keys.Q;
            keyboardControls[2].Run = Keys.W;
            keyboardControls[2].Parry = Keys.A;
            keyboardControls[2].Kick = Keys.Z;
            keyboardControls[2].Action = Keys.X;
            keyboardControls[2].Bt0 = Keys.D0;
            keyboardControls[2].Bt1 = Keys.D1;
            keyboardControls[2].Bt2 = Keys.D2;
            keyboardControls[2].Bt3 = Keys.D3;
            keyboardControls[2].Bt4 = Keys.D4;
            keyboardControls[2].Bt5 = Keys.D5;
            keyboardControls[2].Bt6 = Keys.D6;
            keyboardControls[2].Bt7 = Keys.D7;
            keyboardControls[2].Bt8 = Keys.D8;
            keyboardControls[2].Bt9 = Keys.D9;
            keyboardControls[2].Inventory = Keys.T;
            keyboardControls[2].Menu = Keys.Y;


            keyboardControls[3].Right = Keys.F;
            keyboardControls[3].Left = Keys.S;
            keyboardControls[3].Up = Keys.E;
            keyboardControls[3].Down = Keys.D;
            keyboardControls[3].Attack = Keys.OemPipe;
            keyboardControls[3].Skill = Keys.Tab;
            keyboardControls[3].Jump = Keys.Q;
            keyboardControls[3].Run = Keys.W;
            keyboardControls[3].Parry = Keys.A;
            keyboardControls[3].Kick = Keys.Z;
            keyboardControls[3].Action = Keys.X;
            keyboardControls[3].Bt0 = Keys.D0;
            keyboardControls[3].Bt1 = Keys.D1;
            keyboardControls[3].Bt2 = Keys.D2;
            keyboardControls[3].Bt3 = Keys.D3;
            keyboardControls[3].Bt4 = Keys.D4;
            keyboardControls[3].Bt5 = Keys.D5;
            keyboardControls[3].Bt6 = Keys.D6;
            keyboardControls[3].Bt7 = Keys.D7;
            keyboardControls[3].Bt8 = Keys.D8;
            keyboardControls[3].Bt9 = Keys.D9;
            keyboardControls[3].Inventory = Keys.T;
            keyboardControls[3].Menu = Keys.Y;

            gamepadControls[0].Attack = Buttons.A;
            gamepadControls[0].Skill = Buttons.B;
            gamepadControls[0].Up = Buttons.LeftThumbstickUp;
            gamepadControls[0].Down = Buttons.LeftThumbstickDown;
            gamepadControls[0].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[0].Right = Buttons.LeftThumbstickRight;
            gamepadControls[0].Run = Buttons.LeftStick;
            gamepadControls[0].Jump = Buttons.LeftTrigger;
            gamepadControls[0].Parry = Buttons.RightTrigger;
            gamepadControls[0].Kick = Buttons.RightShoulder;
            gamepadControls[0].Action = Buttons.LeftShoulder;
            gamepadControls[0].Next = Buttons.DPadRight;
            gamepadControls[0].Previous = Buttons.DPadLeft;
            gamepadControls[0].Inventory = Buttons.DPadDown;
            gamepadControls[0].Menu = Buttons.DPadUp;

            gamepadControls[1].Attack = Buttons.A;
            gamepadControls[1].Skill = Buttons.B;
            gamepadControls[1].Up = Buttons.LeftThumbstickUp;
            gamepadControls[1].Down = Buttons.LeftThumbstickDown;
            gamepadControls[1].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[1].Right = Buttons.LeftThumbstickRight;
            gamepadControls[1].Run = Buttons.LeftStick;
            gamepadControls[1].Parry = Buttons.RightTrigger;
            gamepadControls[1].Jump = Buttons.LeftTrigger;
            gamepadControls[1].Kick = Buttons.RightShoulder;
            gamepadControls[1].Action = Buttons.LeftShoulder;
            gamepadControls[1].Next = Buttons.DPadRight;
            gamepadControls[1].Previous = Buttons.DPadLeft;
            gamepadControls[1].Inventory = Buttons.DPadDown;
            gamepadControls[1].Menu = Buttons.DPadUp;

            gamepadControls[2].Attack = Buttons.A;
            gamepadControls[2].Skill = Buttons.B;
            gamepadControls[2].Up = Buttons.LeftThumbstickUp;
            gamepadControls[2].Down = Buttons.LeftThumbstickDown;
            gamepadControls[2].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[2].Right = Buttons.LeftThumbstickRight;
            gamepadControls[2].Run = Buttons.LeftStick;
            gamepadControls[2].Parry = Buttons.RightTrigger;
            gamepadControls[2].Jump = Buttons.LeftTrigger;
            gamepadControls[2].Kick = Buttons.RightShoulder;
            gamepadControls[2].Action = Buttons.LeftShoulder;
            gamepadControls[2].Next = Buttons.DPadRight;
            gamepadControls[2].Previous = Buttons.DPadLeft;
            gamepadControls[2].Inventory = Buttons.DPadDown;
            gamepadControls[2].Menu = Buttons.DPadUp;

            gamepadControls[3].Attack = Buttons.A;
            gamepadControls[3].Skill = Buttons.B;
            gamepadControls[3].Up = Buttons.LeftThumbstickUp;
            gamepadControls[3].Down = Buttons.LeftThumbstickDown;
            gamepadControls[3].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[3].Right = Buttons.LeftThumbstickRight;
            gamepadControls[3].Parry = Buttons.RightTrigger;
            gamepadControls[3].Run = Buttons.LeftStick;
            gamepadControls[3].Jump = Buttons.LeftTrigger;
            gamepadControls[3].Kick = Buttons.RightShoulder;
            gamepadControls[3].Action = Buttons.LeftShoulder;
            gamepadControls[3].Next = Buttons.DPadRight;
            gamepadControls[3].Previous = Buttons.DPadLeft;
            gamepadControls[3].Inventory = Buttons.DPadDown;
            gamepadControls[3].Menu = Buttons.DPadUp;


            xgamepadControls[0].Attack = (int)xgbuttons.LeftShoulder;
            xgamepadControls[0].Skill = (int)xgbuttons.RightTrigger;
            xgamepadControls[0].Throw = (int)xgbuttons.LeftTrigger;
            xgamepadControls[0].RangedAttack = (int)xgbuttons.RightShoulder;
            //xgamepadControls[0].Up = (int)xgbuttons.LeftThumbUp;
            //xgamepadControls[0].Down = (int)xgbuttons.LeftThumbDown;
            //xgamepadControls[0].Left = (int)xgbuttons.LeftThumbLeft;
            //xgamepadControls[0].Right = (int)xgbuttons.LeftThumbRight;
            xgamepadControls[0].Action = (int)xgbuttons.A;
            xgamepadControls[0].Inventory = (int)xgbuttons.X;
            xgamepadControls[0].Parry = (int)xgbuttons.B;
            xgamepadControls[0].Kick = (int)xgbuttons.Y;
            //xgamepadControls[0].Next = (int)xgbuttons.DPadRight;
            //xgamepadControls[0].Previous = (int)xgbuttons.DPadLeft;
            xgamepadControls[0].Jump = (int)xgbuttons.RightThumbPress;
            xgamepadControls[0].Run = (int)xgbuttons.LeftThumbPress;
            xgamepadControls[0].Menu = (int)xgbuttons.Home;
            //xgamepadControls[0].NextCategory = (int)xgbuttons.RightThumbRight;
            //xgamepadControls[0].PreviousCategory = (int)xgbuttons.RightThumbLeft;

            xgamepadControls[1].Attack = (int)xgbuttons.LeftShoulder;
            xgamepadControls[1].Skill = (int)xgbuttons.RightTrigger;
            xgamepadControls[1].Throw = (int)xgbuttons.LeftTrigger;
            xgamepadControls[1].RangedAttack = (int)xgbuttons.RightShoulder;
            //xgamepadControls[1].Up = (int)xgbuttons.LeftThumbUp;
            //xgamepadControls[1].Down = (int)xgbuttons.LeftThumbDown;
            //xgamepadControls[1].Left = (int)xgbuttons.LeftThumbLeft;
            //xgamepadControls[1].Right = (int)xgbuttons.LeftThumbRight;
            xgamepadControls[1].Action = (int)xgbuttons.A;
            xgamepadControls[1].Inventory = (int)xgbuttons.X;
            xgamepadControls[1].Parry = (int)xgbuttons.B;
            xgamepadControls[1].Kick = (int)xgbuttons.Y;
            //xgamepadControls[1].Next = (int)xgbuttons.DPadRight;
            //xgamepadControls[1].Previous = (int)xgbuttons.DPadLeft;
            xgamepadControls[1].Jump = (int)xgbuttons.RightThumbPress;
            xgamepadControls[1].Run = (int)xgbuttons.LeftThumbPress;
            xgamepadControls[1].Menu = (int)xgbuttons.Home;
            //xgamepadControls[1].NextCategory = (int)xgbuttons.RightThumbRight;
            //xgamepadControls[1].PreviousCategory = (int)xgbuttons.RightThumbLeft;

            xgamepadControls[2].Attack = (int)xgbuttons.LeftShoulder;
            xgamepadControls[2].Skill = (int)xgbuttons.RightTrigger;
            xgamepadControls[2].Throw = (int)xgbuttons.LeftTrigger;
            xgamepadControls[2].RangedAttack = (int)xgbuttons.RightShoulder;
            //xgamepadControls[2].Up = (int)xgbuttons.LeftThumbUp;
            //xgamepadControls[2].Down = (int)xgbuttons.LeftThumbDown;
            //xgamepadControls[2].Left = (int)xgbuttons.LeftThumbLeft;
            //xgamepadControls[2].Right = (int)xgbuttons.LeftThumbRight;
            xgamepadControls[2].Action = (int)xgbuttons.A;
            xgamepadControls[2].Inventory = (int)xgbuttons.X;
            xgamepadControls[2].Parry = (int)xgbuttons.B;
            xgamepadControls[2].Kick = (int)xgbuttons.Y;
            //xgamepadControls[2].Next = (int)xgbuttons.DPadRight;
            //xgamepadControls[2].Previous = (int)xgbuttons.DPadLeft;
            xgamepadControls[2].Jump = (int)xgbuttons.RightThumbPress;
            xgamepadControls[2].Run = (int)xgbuttons.LeftThumbPress;
            xgamepadControls[2].Menu = (int)xgbuttons.Home;
            //xgamepadControls[2].NextCategory = (int)xgbuttons.RightThumbRight;
            //xgamepadControls[2].PreviousCategory = (int)xgbuttons.RightThumbLeft;

            xgamepadControls[3].Attack = (int)xgbuttons.LeftShoulder;
            xgamepadControls[3].Skill = (int)xgbuttons.RightTrigger;
            xgamepadControls[3].Throw = (int)xgbuttons.LeftTrigger;
            xgamepadControls[3].RangedAttack = (int)xgbuttons.RightShoulder;
            //xgamepadControls[3].Up = (int)xgbuttons.LeftThumbUp;
            //xgamepadControls[3].Down = (int)xgbuttons.LeftThumbDown;
            //xgamepadControls[3].Left = (int)xgbuttons.LeftThumbLeft;
            //xgamepadControls[3].Right = (int)xgbuttons.LeftThumbRight;
            xgamepadControls[3].Action = (int)xgbuttons.A;
            xgamepadControls[3].Inventory = (int)xgbuttons.X;
            xgamepadControls[3].Parry = (int)xgbuttons.B;
            xgamepadControls[3].Kick = (int)xgbuttons.Y ;
            //xgamepadControls[3].Next = (int)xgbuttons.DPadRight;
            //xgamepadControls[3].Previous = (int)xgbuttons.DPadLeft;
            xgamepadControls[3].Jump = (int)xgbuttons.RightThumbPress;
            xgamepadControls[3].Run = (int)xgbuttons.LeftThumbPress;
            xgamepadControls[3].Menu = (int)xgbuttons.Home;
            //xgamepadControls[3].NextCategory = (int)xgbuttons.RightThumbRight;
            //xgamepadControls[3].PreviousCategory = (int)xgbuttons.RightThumbLeft;
        }

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    if (File.Exists(@"settings.xml"))
                    {
                        XmlSerializer xmlSer = new XmlSerializer(typeof(Settings));
                        FileStream strrd = new FileStream(@"settings.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                        instance = (Settings)xmlSer.Deserialize(strrd);
                        strrd.Close();
                    }
                    else instance = new Settings();
                }
                return instance;
            }
        }

        /*
        public void Default()
        {
            default_settings = true;
            windowed=true;
            h_res = 1024;
            v_res = 768;
            camera_option=2;
            singlePlayerName = "The Prisoner";
            player_names[0] = "Player 1";
            player_names[1] = "Player 2";
            player_names[2] = "Player 3";
            player_names[3] = "Player 4";
            singlePlayerColor = Color.White;
            player_colors[0] = Color.Red;
            player_colors[1] = Color.Blue;
            player_colors[2] = Color.Green;
            player_colors[3] = Color.Yellow;
            show_messages = 0;

            singlePlayerKeyboardControls.Attack = Keys.LeftControl;
            singlePlayerKeyboardControls.Skill = Keys.LeftAlt;
            singlePlayerKeyboardControls.Bt0 = Keys.D0;
            singlePlayerKeyboardControls.Bt1 = Keys.D1;
            singlePlayerKeyboardControls.Bt2 = Keys.D2;
            singlePlayerKeyboardControls.Bt3 = Keys.D3;
            singlePlayerKeyboardControls.Bt4 = Keys.D4;
            singlePlayerKeyboardControls.Bt5 = Keys.D5;
            singlePlayerKeyboardControls.Bt6 = Keys.D6;
            singlePlayerKeyboardControls.Bt7 = Keys.D7;
            singlePlayerKeyboardControls.Bt8 = Keys.D8;
            singlePlayerKeyboardControls.Bt9 = Keys.D9;
            singlePlayerKeyboardControls.Left = Keys.Left;
            singlePlayerKeyboardControls.Right = Keys.Right;
            singlePlayerKeyboardControls.Up = Keys.Up;
            singlePlayerKeyboardControls.Down = Keys.Down;
            singlePlayerKeyboardControls.Jump = Keys.Space;
            singlePlayerKeyboardControls.Run = Keys.LeftShift;

            singlePlayerGamepadControls.Attack = Buttons.A;
            singlePlayerGamepadControls.Skill = Buttons.B;
            singlePlayerGamepadControls.Up = Buttons.LeftThumbstickUp;
            singlePlayerGamepadControls.Down = Buttons.LeftThumbstickDown;
            singlePlayerGamepadControls.Left = Buttons.LeftThumbstickLeft;
            singlePlayerGamepadControls.Right = Buttons.LeftThumbstickRight;
            singlePlayerGamepadControls.Run = Buttons.LeftStick;
            singlePlayerGamepadControls.Jump = Buttons.LeftTrigger;
            singlePlayerGamepadControls.Next = Buttons.DPadRight;
            singlePlayerGamepadControls.Previous = Buttons.DPadLeft;


            keyboardControls[0].Right = Keys.Right;
            keyboardControls[0].Left = Keys.Left;
            keyboardControls[0].Up = Keys.Up;
            keyboardControls[0].Down = Keys.Down;
            keyboardControls[0].Attack = Keys.L;
            keyboardControls[0].Skill = Keys.K;
            keyboardControls[0].Jump = Keys.RightControl;
            keyboardControls[0].Run = Keys.RightShift;
            keyboardControls[0].Bt0 = Keys.NumPad0;
            keyboardControls[0].Bt1 = Keys.NumPad1;
            keyboardControls[0].Bt2 = Keys.NumPad2;
            keyboardControls[0].Bt3 = Keys.NumPad3;
            keyboardControls[0].Bt4 = Keys.NumPad4;
            keyboardControls[0].Bt5 = Keys.NumPad5;
            keyboardControls[0].Bt6 = Keys.NumPad6;
            keyboardControls[0].Bt7 = Keys.NumPad7;
            keyboardControls[0].Bt8 = Keys.NumPad8;
            keyboardControls[0].Bt9 = Keys.NumPad9;

            keyboardControls[1].Right = Keys.D;
            keyboardControls[1].Left = Keys.A;
            keyboardControls[1].Up = Keys.W;
            keyboardControls[1].Down = Keys.S;
            keyboardControls[1].Attack = Keys.Q;
            keyboardControls[1].Skill = Keys.E;
            keyboardControls[1].Jump = Keys.Z;
            keyboardControls[1].Run = Keys.LeftShift;
            keyboardControls[1].Bt0 = Keys.D0;
            keyboardControls[1].Bt1 = Keys.D1;
            keyboardControls[1].Bt2 = Keys.D2;
            keyboardControls[1].Bt3 = Keys.D3;
            keyboardControls[1].Bt4 = Keys.D4;
            keyboardControls[1].Bt5 = Keys.D5;
            keyboardControls[1].Bt6 = Keys.D6;
            keyboardControls[1].Bt7 = Keys.D7;
            keyboardControls[1].Bt8 = Keys.D8;
            keyboardControls[1].Bt9 = Keys.D9;

            keyboardControls[2].Right = Keys.D;
            keyboardControls[2].Left = Keys.A;
            keyboardControls[2].Up = Keys.W;
            keyboardControls[2].Down = Keys.S;
            keyboardControls[2].Attack = Keys.OemPipe;
            keyboardControls[2].Skill = Keys.Tab;
            keyboardControls[2].Jump = Keys.Q;
            keyboardControls[2].Run = Keys.LeftShift;
            keyboardControls[2].Bt0 = Keys.D0;
            keyboardControls[2].Bt1 = Keys.D1;
            keyboardControls[2].Bt2 = Keys.D2;
            keyboardControls[2].Bt3 = Keys.D3;
            keyboardControls[2].Bt4 = Keys.D4;
            keyboardControls[2].Bt5 = Keys.D5;
            keyboardControls[2].Bt6 = Keys.D6;
            keyboardControls[2].Bt7 = Keys.D7;
            keyboardControls[2].Bt8 = Keys.D8;
            keyboardControls[2].Bt9 = Keys.D9;


            keyboardControls[3].Right = Keys.D;
            keyboardControls[3].Left = Keys.A;
            keyboardControls[3].Up = Keys.W;
            keyboardControls[3].Down = Keys.S;
            keyboardControls[3].Attack = Keys.OemPipe;
            keyboardControls[3].Skill = Keys.Tab;
            keyboardControls[3].Jump = Keys.Q;
            keyboardControls[3].Run = Keys.LeftShift;
            keyboardControls[3].Bt0 = Keys.D0;
            keyboardControls[3].Bt1 = Keys.D1;
            keyboardControls[3].Bt2 = Keys.D2;
            keyboardControls[3].Bt3 = Keys.D3;
            keyboardControls[3].Bt4 = Keys.D4;
            keyboardControls[3].Bt5 = Keys.D5;
            keyboardControls[3].Bt6 = Keys.D6;
            keyboardControls[3].Bt7 = Keys.D7;
            keyboardControls[3].Bt8 = Keys.D8;
            keyboardControls[3].Bt9 = Keys.D9;

            gamepadControls[0].Attack = Buttons.A;
            gamepadControls[0].Skill = Buttons.B;
            gamepadControls[0].Up = Buttons.LeftThumbstickUp;
            gamepadControls[0].Down = Buttons.LeftThumbstickDown;
            gamepadControls[0].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[0].Right = Buttons.LeftThumbstickRight;
            gamepadControls[0].Run = Buttons.LeftStick;
            gamepadControls[0].Jump = Buttons.LeftTrigger;
            gamepadControls[0].Next = Buttons.DPadRight;
            gamepadControls[0].Previous = Buttons.DPadLeft;

            gamepadControls[1].Attack = Buttons.A;
            gamepadControls[1].Skill = Buttons.B;
            gamepadControls[1].Up = Buttons.LeftThumbstickUp;
            gamepadControls[1].Down = Buttons.LeftThumbstickDown;
            gamepadControls[1].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[1].Right = Buttons.LeftThumbstickRight;
            gamepadControls[1].Run = Buttons.LeftStick;
            gamepadControls[1].Jump = Buttons.LeftTrigger;
            gamepadControls[1].Next = Buttons.DPadRight;
            gamepadControls[1].Previous = Buttons.DPadLeft;

            gamepadControls[2].Attack = Buttons.A;
            gamepadControls[2].Skill = Buttons.B;
            gamepadControls[2].Up = Buttons.LeftThumbstickUp;
            gamepadControls[2].Down = Buttons.LeftThumbstickDown;
            gamepadControls[2].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[2].Right = Buttons.LeftThumbstickRight;
            gamepadControls[2].Run = Buttons.LeftStick;
            gamepadControls[2].Jump = Buttons.LeftTrigger;
            gamepadControls[2].Next = Buttons.DPadRight;
            gamepadControls[2].Previous = Buttons.DPadLeft;

            gamepadControls[3].Attack = Buttons.A;
            gamepadControls[3].Skill = Buttons.B;
            gamepadControls[3].Up = Buttons.LeftThumbstickUp;
            gamepadControls[3].Down = Buttons.LeftThumbstickDown;
            gamepadControls[3].Left = Buttons.LeftThumbstickLeft;
            gamepadControls[3].Right = Buttons.LeftThumbstickRight;
            gamepadControls[3].Run = Buttons.LeftStick;
            gamepadControls[3].Jump = Buttons.LeftTrigger;
            gamepadControls[3].Next = Buttons.DPadRight;
            gamepadControls[3].Previous = Buttons.DPadLeft;
        }
        */

        public void load()
        {
            XmlSerializer xmlSer = new XmlSerializer(typeof(Settings));
            FileStream strrd = new FileStream(@"settings.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
            instance = (Settings)xmlSer.Deserialize(strrd);
            strrd.Close();
        }

        public void save()
        {
            XmlSerializer xmlSer = new XmlSerializer(typeof(Settings));
            FileStream strwr = new FileStream(@"settings.xml", FileMode.Create, FileAccess.Write, FileShare.Read);
            xmlSer.Serialize(strwr, instance);
            strwr.Close();
        }

        public int cameraOption
        {
            get { return camera_option; }
        }

        public bool Windowed
        {
            get { return windowed; }
        }

        public int HorizontalResolution
        {
            get { return h_res; }
        }

        public int VerticalResolution
        {
            get { return v_res; }
        }

        public kControls getKControls(int playerNumber)
        {
            if (Globals.players > 1)
                return keyboardControls[playerNumber - 1];
            else return singlePlayerKeyboardControls;
        }

        public padControls getGPControls(int playerNumber)
        {
            if (Globals.players > 1)
                return gamepadControls[playerNumber - 1];
            else return singlePlayerGamepadControls;
        }

        public gpadControls getXGPControls(int playerNumber)
        {
            if (Globals.players > 1)
                return xgamepadControls[playerNumber - 1];
            else return singlePlayerXGamepadControls;
        }
    }
}
