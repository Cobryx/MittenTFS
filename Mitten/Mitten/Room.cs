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
    public class Room
    {
        int roomNumber;
        int width;
        int height;
        Point origin;
        Point center;
        int nexit;
        Rectangle area;
        tilecoord[,] structure;
        Vector2 vCenter;
        public List<int> waypoint { get; private set; }
        public int centralWP { get; private set; }

        public bool Hallway { get { return hallway; } }
        public int Width { get { return width; } }//set { width = value; } }
        public int getRoomNumber { get { return roomNumber; } }
        public int Height { get { return height; } }//set { height = value; } }
        public List<int> getLinkedRooms { get { return linkedRooms; } }
        public Point getOrigin { get { return origin; } }
        public Point getCenter { get { return center; } }
        public Point getDimension { get { return new Point(width, height); } }
        public Rectangle getArea { get { return area; } }//new Rectangle(origin.X, origin.Y, width, height); } }
        public Rectangle getAbsoluteArea { get { return new Rectangle (area.X*32,area.Y*32,Width*32,Height*32); } }//new Rectangle(origin.X, origin.Y, width, height); } }
        public tilecoord[,] getStructure { get { return structure; } set { structure = value; } }
        //public int Waypoint { get { return waypoint; } set { waypoint = value; } }
        public Vector2 getVCenter { get { return vCenter; } }
        

        Random random = new Random(); 			//definizione funzione random
        int rtile;
        int i, j;

        String name;
        int type;
        bool passage = false; //flag che indica se la stanza contiene un passaggio (sia esso un'uscita o un'entrata)
        bool hallway = false; //flag se indica se la stanza è un corridoio di collegmaento generato successivamente rispetto alle stanze principali
        List<int> linkedRooms = new List<int>();


        /// <summary>
        /// Istanzia una stanza intesa come unità spaziale del Dungeon
        /// </summary>
        /// <param name="roomNumber">Numero di stanza</param>
        /// <param name="origin">Coordinate (del Dungeon) dell'angolo superiore sinistro della stanza</param>
        /// <param name="dimensions">Dimensioni (larghezza e altezza) della stanza</param>
        /// <param name="walls">Numero di muri che la stanza deve presentare</param>
        /// <param name="floor">Piano di riferimento</param>
        public Room(int roomNumber, Point origin, Point dimensions, int type, bool hallway, List<int> linkedRooms)
        {
            if (roomNumber < 0) throw new ArgumentOutOfRangeException("roomNumber", "roomNumber must be a valid index (greater or equal than 0)");
            if (type < 0) throw new ArgumentOutOfRangeException("type", "type must be a valid type number (greater or equal than 0)");
            this.roomNumber = roomNumber;
            linkedRooms.Add(roomNumber);
            this.origin = origin;
            area = new Rectangle(origin.X, origin.Y, dimensions.X, dimensions.Y);
            width = dimensions.X;
            height = dimensions.Y;
            center = new Point(origin.X + width / 2, origin.Y + height / 2);
            vCenter = new Vector2(origin.X * 32 + width * 16, origin.Y * 32 + height * 16);
            structure = new tilecoord[width, height];
            nexit = random.Next(1, 5);
            this.type = type;
            this.hallway = hallway;
            waypoint = new List<int>();
            centralWP = -1;
            foreach (int l in linkedRooms)
            {
                this.linkedRooms.Add(l);
            }
            
            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    structure[i, j].fg = dic.wallTileIndex(type)["blank"];
                    structure[i, j].steppable = true;
                }
            }
        }

        /// <summary>
        /// Istanzia una stanza intesa come unità spaziale del Dungeon
        /// </summary>
        /// <param name="roomNumber">Numero della stanza</param>
        /// <param name="area">Rettangolo che descriva la superficie (in tiles) della stanza</param>
        /// <param name="type">Tipo di stanza (correntemente inutilizzato)</param>
        /// <param name="hallway">Flag che specifica se la stanza viene genrata come corridoio (true) o meno (false)</param>
        /// <param name="linkedRooms">Lista de numeri identificativi delle stanze adiacenti</param>
        public Room(int roomNumber, Rectangle area, int type, bool hallway)//, List<int> linkedRooms)
        {
            if (roomNumber < 0) throw new ArgumentOutOfRangeException("roomNumber", "roomNumber must be a valid index (greater or equal than 0)");
            if (type < 0) throw new ArgumentOutOfRangeException("type", "type must be a valid type number (greater or equal than 0)");
            if (area.Width < 0) throw new ArgumentException("area", "area must have a width greater or equal than 0");
            if (area.Height < 0) throw new ArgumentException("area", "area must have a height greater or equal than 0");
            this.roomNumber = roomNumber;
            linkedRooms.Add(roomNumber);
            this.area = area;
            origin = area.Location;
            width = area.Width;
            height = area.Height;
            center = area.Center;
            vCenter = new Vector2(area.Left*32 + area.Width * 16, area.Top *32 + area.Height * 16);
            structure = new tilecoord[width, height];
            nexit = random.Next(1, 5);
            this.type = type;
            this.hallway = hallway;
            waypoint = new List<int>();
            centralWP = -1;

            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    structure[i, j].fg = dic.wallTileIndex(type)["blank"];
                    structure[i, j].steppable = true;
                }
            }
        }

        public void AddWaypoint(int waypointIndex, bool center)
        {
            if (center)
            {
                if (centralWP == -1)
                {
                    waypoint.Add(waypointIndex);
                    centralWP = waypointIndex;
                }
                else
                {
                    throw new ArgumentException("L'indice del waypoint centrale della stanza è già stato impostato", "waypointIndex");
                }
            }
            else
            {
                waypoint.Add(waypointIndex);
            }
        }

        public bool canHostAPassage()
        {
            if (height > 9 && width > 9 && !passage)
                return true;
            else return false;
        }

        public bool IsOnlyWall()
        {
            if (structure[width/2, height/2].steppable == true )
                return false;
            else
                return true;
        }

        /// <summary>
        /// verifica se ci sono più di due aperture nei muri di una stanza
        /// </summary>
        /// <returns>restituisce true se le aperture sono più di due</returns>
        public bool IsOpen()
        {
            int n=0;
            for (int i=0; i<=width;i++)
            {
               if (structure[i,0].steppable==true)
               {
                   n++;
               }
               if (structure[i, height].steppable == true)
               {
                   n++;
               }
            }
            for (int i = 0; i <= height; i++)
            {
                if (structure[0,i].steppable == true)
                {
                    n++;
                }
                if (structure[width, i].steppable == true)
                {
                    n++;
                }
            }
            if (n >= 2)
                return true;
            else
                return false;
        }

        public void linkARoom(int roomNumber)
        {
            linkedRooms.Add(roomNumber);
        }
        
        public Vector2 placePassage(bool exit, int destination)
        {
            this.passage = true;
            Vector2 position = new Vector2(random.Next(3, this.width - 3)*32, random.Next(3, this.height - 3)*32);

            //structure[p.position.X, p.position.Y].steppable = true;

            position.X += this.origin.X*32;
            position.Y += this.origin.Y*32;
            return position;
        }

        /*
        public passage placePassage(bool exit, int destination)
        {
            this.passage = true;
            passage p = new passage();
            p.destination = destination;
            p.exit = exit;
            p.position = new Point(random.Next(3, this.width - 3), random.Next(3, this.height - 3));
            //p.position = new Point(random.Next(this.Center.X+3, this.Center.X+this.width-3), random.Next(this.Center.Y+3, this.Center.Y+this.height-3));
            
            structure[p.position.X, p.position.Y].steppable = true;

            if (p.exit)
                structure[p.position.X, p.position.Y].bg = dic.tileIndex(type)["empty"];
            else
                structure[p.position.X, p.position.Y].bg = dic.tileIndex(type)["removed_tile"];
            structure[p.position.X, p.position.Y].fg = dic.tileIndex(type)["blank"];

            p.position.X += this.origin.X;
            p.position.Y += this.origin.Y;
            return p;
        }
        */
    }
}
