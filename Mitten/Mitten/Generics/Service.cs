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
    public static class Service
    {
        //ricordati di spostarli in una classe ausiliare
        public static float CurveAngle(float from, float to, float step)
        {
            if (step == 0) return from;             //nessuna rotazione
            //if (from == to || step == 1) return to; //rotazione istantanea
            if (step >= Math.Abs(to - from)) return to; //rotazione istantanea

            //calcola i vettori corrispondenti alle semirette del lato mobile dell'angolo formato tra from e to
            Vector2 fromVector = new Vector2((float)Math.Cos(from), (float)Math.Sin(from));
            Vector2 toVector = new Vector2((float)Math.Cos(to), (float)Math.Sin(to));

            //step = step / (Math.Abs(to - from));
            Vector2 currentVector = Slerp(fromVector, toVector, step);

            //calcola 
            float ret = (float)Math.Atan2(currentVector.Y, currentVector.X);

            return ret;
        }

        public static Vector2 Slerp(Vector2 fromV, Vector2 toV, float step)
        {
            //come sopra
            if (step == 0) return fromV;
            if (fromV == toV || step == 1) return toV;


            float vangle = Vector2.Dot(fromV, toV);
            if (vangle > 1) vangle = 1;
            if (vangle < -1) vangle = -1;

            double theta = Math.Acos(vangle);
            if (theta == 0) return toV;

            double sinTheta = Math.Sin(theta);
            //step /= (float)sinTheta;    //da controllare: modula lo step in base all'ampiezza dell'angolo stesso
            step = step / (float)theta;
            if (step > 1) step = 1;
            Vector2 returnio = (float)(Math.Sin((1 - step) * theta) / sinTheta) * fromV + (float)(Math.Sin(step * theta) / sinTheta) * toV;
            if (float.IsNaN(returnio.X) || float.IsNaN(returnio.Y))
            { return toV; }
            return returnio;
        }





        public static Vector2 Perpendicular(Vector2 v)
        {
            v = new Vector2(-v.Y, v.X);
            return v;
        }
    }

    
}

namespace Extensions
{
    /// <summary>
    /// Adds the ability to index a Vector2 for cheeky code saving
    /// </summary>
    public static class Vector2Extensions
    {
        public static float Index(this Vector2 v, int i)
        {
            switch (i)
            {
                case 0:
                    return v.X;
                case 1:
                    return v.Y;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public static bool IsBetweenTwoPoints(this Vector2 t, Vector2 A, Vector2 B)
        {
            float deltaX = B.X / A.X;
            float deltaY = B.Y / A.Y;

            if (Math.Abs((t.X / A.X) - deltaX)<0.001f && Math.Abs((t.Y / A.Y) - deltaY)<0.001f)
                return true;
            else return false;
        }

        public static float DistanceToPoint(this Vector2 t, Vector2 v)
        {
            return (v - t).Length();
        }
    }
} 


