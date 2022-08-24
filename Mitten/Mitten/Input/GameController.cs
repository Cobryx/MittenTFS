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
using SlimDX.DirectInput;

namespace Mitten
{
    public class GameController
    {
        private Joystick joystick;

        private JoystickState state = new JoystickState();

        DirectInput input = new DirectInput();

        public GameController(DirectInput directInput, Game game, int number)
        {
            // Geräte suchen
            var devices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            if (devices.Count == 0 || devices[number] == null)
            {
                // Kein Gamepad vorhanden
                return;
            }

            // Gamepad erstellen
            joystick = new Joystick(directInput, devices[number].InstanceGuid);

            // Das GamePad soll nur reagieren, wenn sich unser Spiel(-fenster) im Vordergrund befindet
            joystick.SetCooperativeLevel(game.Window.Handle, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);

            // Den Zahlenbereich der Achsen auf -1000 bis 1000 setzen
            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

            }

            joystick.Acquire();
        }

        public JoystickState GetState()
        {
            if (joystick != null)
            {
                if (joystick.Acquire().IsFailure || joystick.Poll().IsFailure)
                {
                    // Wenn das GamePad nicht erreichbar ist, leeren Status zurückgeben.
                    state = new JoystickState();
                    return state;
                }

                state = joystick.GetCurrentState();

                return state;
            }
            else
                return null;
        }

        public void Release()
        {
            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            joystick = null;
        }
    }
}
