using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Extensions;

namespace Mitten
{
    public class OBB
    {
        #region members and properties
        Vector2 _origin;//centre point of the OBB
        Vector2[] _vertexes;    //vertexes of the OBB
        bool vertexesUpdated = false;

        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }
        Vector2[] _axis;//2d orientation matrix
        public Vector2[] Axis
        {
            get { return _axis; }
        }
        Vector2 _halfWidths;//the +ve extents along each axis
        public Vector2 HalfWidths
        {
            get { return _halfWidths; }
            set { _halfWidths = value; }
        }
        float _angleInRadians;//used for drawing a visuaisation of the OBB
        public float AngleInRadians
        {
            get { return _angleInRadians; }
            set
            {
                if (value < Math.PI && value > -Math.PI) //in the range [-PI..PI]
                    UpdateAxis(value);
                if (value >= Math.PI)
                    UpdateAxis((float)(Math.Abs((value % Math.PI * 2)) - Math.PI));
                if (value <= -Math.PI)
                    UpdateAxis((float)(Math.Abs((value % Math.PI * 2)) + Math.PI));
            }
        }

        //aggiunto da noi
        Vector2 _forceDirection;

        public Color DebugColor = new Color(1f, 1f, 1f, 0.5f);
        //an epsilon value to counter floating point errors in a parallel situation
        const float EPSILON = 0.00001f;
        #endregion

        /// <summary>
        /// Creates an oriented bounding box for collision detection
        /// </summary>
        /// <param name="Origin">The center of the box</param>
        /// <param name="AngleInRadians">The rotation of the box in the xy plane</param>
        /// <param name="HalfWidths">The half extents of the box in it's X and Y axis</param>
        public OBB(Vector2 Origin, float AngleInRadians, Vector2 HalfWidths)
        {
            _origin = Origin;
            _angleInRadians = AngleInRadians;

            _halfWidths = HalfWidths;

            _axis = new Vector2[2];
            _axis[0] = new Vector2();
            _axis[1] = new Vector2();

            _vertexes = new Vector2[4];

            UpdateAxis(AngleInRadians);

            _vertexes[0] = new Vector2(_halfWidths.X, _halfWidths.Y);
            _vertexes[1] = new Vector2(- _halfWidths.X, _halfWidths.Y);
            _vertexes[2] = new Vector2(- _halfWidths.X, - _halfWidths.Y);
            _vertexes[3] = new Vector2(_halfWidths.X, - _halfWidths.Y);
        }

        /// <summary>
        /// Returns whether this OBB is intersecting a second
        /// </summary>
        /// <param name="OtherOBB"></param>
        /// <returns></returns>
        public bool Intersects(OBB OtherOBB)
        {
            return OBB.Intersects(this, OtherOBB);
        }

        public bool Intersects(Circle circle)
        {
            if (Vector2.Distance(circle.Center, this.Origin) < (circle.Radius + Math.Min(this.HalfWidths.X, this.HalfWidths.Y)))
            {
                return true;
            }
            else
            {
                if (Vector2.Distance(circle.Center, this.Origin) > (circle.Radius + Math.Sqrt(HalfWidths.X * HalfWidths.X + HalfWidths.Y * HalfWidths.Y)))
                {
                    return false;
                }
                else
                {
                    float a, b, c;
                    Vector2 point;
                    point = this.Origin + this.Axis[0] * this._halfWidths.Y;
                    a = (float)Math.Tan(this._angleInRadians);
                    b = -1;
                    c = -a * point.X - b * point.Y;

                    if ((Math.Abs(a * circle.Center.X + b * circle.Center.Y + c) / Math.Sqrt(a * a + b * b)) < circle.Radius)
                    {
                        return true;
                    }

                    point = this.Origin - this.Axis[0] * this._halfWidths.Y;
                    a = (float)Math.Tan(this._angleInRadians + Math.PI / 2);
                    c = -a * point.X - b * point.Y;

                    if ((Math.Abs(a * circle.Center.X + b * circle.Center.Y + c) / Math.Sqrt(a * a + b * b)) < circle.Radius)
                    {
                        return true;
                    }

                    point = this.Origin + this.Axis[1] * this._halfWidths.X;
                    a = (float)Math.Tan(this._angleInRadians + Math.PI);
                    c = -a * point.X - b * point.Y;

                    if ((Math.Abs(a * circle.Center.X + b * circle.Center.Y + c) / Math.Sqrt(a * a + b * b)) < circle.Radius)
                    {
                        return true;
                    }

                    point = this.Origin - this.Axis[1] * this._halfWidths.X;
                    a = (float)Math.Tan(this._angleInRadians+3*Math.PI/2);
                    c = -a * point.X - b * point.Y;

                    if ((Math.Abs(a * circle.Center.X + b * circle.Center.Y + c) / Math.Sqrt(a * a + b * b)) < circle.Radius)
                    {
                        return true;
                    }
                    
                    return false;
                }
            }
        }

        /// <summary>
        /// Finds where we would draw our debug texture without rotation.
        /// </summary>
        /// <returns>A rectangle corresponging to the unrotated position of the AABB</returns>
        private Rectangle GetDestinationRect()
        {
            int x = (int)(_origin.X);
            int y = (int)(_origin.Y);
            int width = (int)(_halfWidths.X * 2);
            int height = (int)(_halfWidths.Y * 2);
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Updates the orientation of the OBB
        /// </summary>
        /// <param name="AngleInRadians">The new rotation in radians</param>
        public void UpdateAxis(float AngleInRadians)
        {
            //Standard rotation matrix equation
            _axis[0].X = (float)Math.Cos(AngleInRadians);
            _axis[0].Y = (float)Math.Sin(AngleInRadians);
            _axis[1].Y = (float)Math.Cos(AngleInRadians);
            _axis[1].X = -(float)Math.Sin(AngleInRadians);

            _angleInRadians = AngleInRadians;
        }

        /// <summary>
        /// Prints the properties of the OBB to the console
        /// </summary>
        public void Print()
        {
            Console.WriteLine("origin : " + _origin + "\nAngle : " + _angleInRadians +
            "\nX Axis : " + _axis[0] + "\nY Axis : " + _axis[1]);
        }

        /// <summary>
        /// Draws a visualisation of the OBB, useful for debug purposes
        /// </summary>
        /// <param name="sb">The Globals.spriteBatch instance to draw with</param>
        /// <param name="NullTexture">A reference to a 1x1 texture which will be draw over the OBB</param>
        public void Draw(Rectangle camera,float depth)
        {
            
            //OBB's are only ever going to be drawn in debug mode
            //so performance from repeated Begin() End() calls is acceptable
            Rectangle r = GetDestinationRect();
            r.Location = new Point(r.Location.X - camera.Location.X, r.Location.Y - camera.Location.Y);
            
            //sb.Begin();
            Vector2 g = _halfWidths;
            g.Normalize();
            /*
            sb.Draw(NullTexture, GetDestinationRect(), null, DebugColor,
            _angleInRadians,
            Vector2.One / 2,//the origin of the 1x1 texture i.e. (0.5f,0.5f)
            SpriteEffects.None, 0.5f);
            */
            Globals.spriteBatch.Draw(Globals.Box, r, null, DebugColor, _angleInRadians, Vector2.One / 2, SpriteEffects.None, depth);

            //sb.End();

        }

        public bool Intersects(Rectangle rect)
        {
            //da rifare, drammaticamente da rifare
            OBB b = new OBB(new Vector2(rect.X* 32+rect.Width*16, rect.Y*32+rect.Height*16), 0, new Vector2(rect.Width*32, rect.Height*32));
           // b.DebugColor = Color.White;
           
            
            //b.Draw(Globals.debugBox, Globals.camera[0], Globals.Globals.spriteBatch,0);
            return Intersects(this,b);
            //return Intersects(this, new OBB(new Vector2(rect.X * 32 + rect.Width * 16, rect.Y * 32 + rect.Height * 16), 0, new Vector2(rect.Width * 32, rect.Height * 32)));
        }

        /// <summary>
        /// Tests whether two OBBs intersect. Uses a separating axis implementation.
        /// </summary>
        /// <param name="First">The first OBB</param>
        /// <param name="Second">The second OBB</param>
        /// <returns></returns>
        public static bool Intersects(OBB First, OBB Second)
        {
            //aggiunto da noi
            if (First == null || Second == null)
            {
                return false;
            }

            if ( First.HalfWidths == Vector2.Zero || Second.HalfWidths == Vector2.Zero)
                return false;
            
            #region pre test calcs and declarations
            float rf, rs;
            float[,] R = new float[2, 2];
            float[,] AbsR = new float[2, 2];


            //compute rotation matrix by expressing second in terms of first
            //also create common sub expressions
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    R[i, j] = Vector2.Dot(First.Axis[i], Second.Axis[j]);
                    AbsR[i, j] = Math.Abs(R[i, j]) + EPSILON;   //non capisco perché sommare EPS qui - Carlo
                }

            //create translation vector
            Vector2 translation = Second.Origin - First.Origin; //possibile problema di segno (visto che non si fa first - second)?

            //bring translation into First's local coordinate system
            translation = new Vector2(Vector2.Dot(translation, First.Axis[0]),
            Vector2.Dot(translation, First.Axis[1]));
            #endregion

            //Test if axes FirstX or FirstY separate the OBBs
            for (int i = 0; i < 2; i++)
            {
                rf = First.HalfWidths.Index(i);
                rs = Second.HalfWidths.X * AbsR[i, 0] + Second.HalfWidths.Y * AbsR[i, 1];
                if (Math.Abs(translation.Index(i)) > (rf + rs))
                    return false;
            }

            //Test if axes SecondX or SecondY separate the OBBs
            for (int i = 0; i < 2; i++)
            {
                rf = First.HalfWidths.Index(0) * AbsR[0, i] + First.HalfWidths.Index(1) * AbsR[1, i];
                rs = Second.HalfWidths.Index(i);

                if (Math.Abs(translation.Index(0) * R[0, i] + translation.Index(1) * R[1, i]) > (rf + rs))
                    return false;
            }

            //no separating axis - OBBs must therefore be intersecting
            return true;
        }

        public bool Intersects(Vector2 axis, Vector2 origin)
        {
//            It'd be faster to use dot products here (let p be any point on the line, and v_i be the ith vertex of the rectangle; if dot_prod(p-v_0, p-v_i) < 0 for some i then the line is crossed)
//But I'm guessing pkt means 'line segment,' not line?
            Vector2 v = origin + axis;
            
            for(int i=0;i<3;i++)
            {
                if (Vector2.Dot(origin - Vertex(i), v - Vertex(i+1)) < 0)
                {
                    return true;
                }
            }
            if (Vector2.Dot(origin - Vertex(3), v - Vertex(0)) < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// DO NOT USE!
        /// </summary>
        private void Vertex()
        {
            _vertexes[0] = _origin + new Vector2(_halfWidths.X * (float)Math.Cos(_angleInRadians) - _halfWidths.X * (float)Math.Sin(_angleInRadians), _halfWidths.X * (float)Math.Sin(_angleInRadians) + _halfWidths.Y * (float)Math.Cos(_angleInRadians));
            _vertexes[1] = _origin + new Vector2(-_halfWidths.X * (float)Math.Cos(_angleInRadians) + _halfWidths.X * (float)Math.Sin(_angleInRadians), _halfWidths.X * (float)Math.Sin(_angleInRadians) + _halfWidths.Y * (float)Math.Cos(_angleInRadians));
            _vertexes[2] = _origin + new Vector2(-_halfWidths.X * (float)Math.Cos(_angleInRadians) + _halfWidths.X * (float)Math.Sin(_angleInRadians), -_halfWidths.X * (float)Math.Sin(_angleInRadians) - _halfWidths.Y * (float)Math.Cos(_angleInRadians));
            _vertexes[3] = _origin + new Vector2(_halfWidths.X * (float)Math.Cos(_angleInRadians) - _halfWidths.X * (float)Math.Sin(_angleInRadians), -_halfWidths.X * (float)Math.Sin(_angleInRadians) - _halfWidths.Y * (float)Math.Cos(_angleInRadians));
            vertexesUpdated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vector2 Vertex(int n)
        {
            if(n<0 || n>3)
                throw new ArgumentException("Vertex number must be within 0 and 3.");
            Vector2 r = Vector2.Transform(_vertexes[n], Matrix.CreateRotationZ(_angleInRadians)) + _origin;
            return r;
        }

        public LineEquation Edge(int n)
        {
            switch (n)
            {
                case 0: return new LineEquation(Vertex(0), Vertex(1));
                case 1: return new LineEquation(Vertex(1), Vertex(2));
                case 2: return new LineEquation(Vertex(2), Vertex(3));
                case 3: return new LineEquation(Vertex(3), Vertex(0));
                default: throw new ArgumentException("Edge number must be within 0 and 3.");
            }
        }
    }
}


