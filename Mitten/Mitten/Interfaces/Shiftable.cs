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
    public abstract class Shiftable
    {
        
        
        protected Vector2 xDirection;     //direzione impostata esternamente
        protected Vector2 xDestination;   //destinazione impostata esternamente
        protected Vector2 xTranslation;
        protected bool xMovement = false; //flag relativa al moto subito da terze parti
        protected bool xRotation = false;
        protected bool xTranslate = false;
        protected float pace;
        protected float destinationAngle;
        protected float xSpeed;

        public void ExternalSpin(float angle, float pace)
        {
            if (!xRotation)
            {
                xRotation = true;
                destinationAngle = angle;
                this.pace = pace;
            }
        }

        /// <summary>
        /// Indica uno spostamento che l'entità subirà passivamente
        /// </summary>
        /// <param name="direction">Direzione dello spostamento</param>
        public void ExternalShift(Vector2 direction)
        {

            if (!xMovement)// && direction != Vector2.Zero)
            {
                xMovement = true;
                xDirection = direction;
                xTranslation = new Vector2(0);
                xDestination = new Vector2(0);
            }
            /*else
                xDirection *= 0;*/
        }

        /// <summary>
        /// Indica uno spostamento che l'entità subirà passivamente
        /// </summary>
        /// <param name="destination">Punto in cui l'entità dovrà ritrovarsi</param>
        /// <param name="speed">Velocità con cui raggiungere la destinazione</param>
        public void ExternalShift(Vector2 destination, float speed=1)
        {
            if (!xMovement)
            {
                xMovement = true;
                xDestination = destination;
                this.xSpeed = speed;
                xTranslation = new Vector2(0);
                xDirection = new Vector2(0);
            }
        }

        public void Translate(Vector2 direction)
        {
            if (!xTranslate)
            {
                xTranslate = true;
                xTranslation = direction;
            }
        }
    }
}
