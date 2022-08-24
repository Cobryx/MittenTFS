using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Drawing.Imaging;

namespace Mitten
{
    /// <summary>
    /// Dungeon, livello base del gioco.
    /// </summary>
    [Serializable]
    public class Dungeon
    {
        public int width { get; private set; }
        public int height { get; private set; }
        String name;
        int floor;
        int nrooms;
        float depth;
        float scale_factor;
        float rotation_angle;
        String[] logString = new String[8];
        String[] dString = new String[1];
        int type;
        int floorType;
        int wallType;


        int whileCounter = 0; //asd

        bool open;  //specifica se il dungeon è aperto (= vi sono ancora giocatori in esso)
        bool spawning = true;
        bool generated = false;

        //[NonSerialized] TileSet tileSet;
        tilecoord k;
        tilecoord[,] structure;
        Vector2 position = new Vector2(0, 0);
        Rectangle backgroundHue;
        Rectangle tile = new Rectangle(0, 0, 32, 32);
        Vector2 dummy_vector = new Vector2(0, 0);
        Vector2 sprite_origin = new Vector2(0, 0);

        List<SceneElement> scenography = new List<SceneElement>();

        List<Room> roomList = new List<Room>();

        List<spawningPlace> spawn; //vettore dei punti di spawn
        List<spawningPlace> playerSpawn;    //posizioni di nascita dei giocatori

        Vector2 entryPassage;
        Vector2 exitPassage;
        //List<passage> entries = new List<passage>();
        //List<passage> exits = new List<passage>();

        Zone[,] grid; //grid of control cells in which check for collisions
        int gZsize = 128; //size of each control cell

        bool[] id;
        bool[] damageId;

        int playerPopulation = 0;
        List<int> players = new List<int>();

        int i, j; //generic indexes for counters


        [NonSerialized] OBB fogBox;
        public Color lightColor
        {
            get;
            set;
        }

        //public UndirectedGraph<Waypoint, TaggedUndirectedEdge<Waypoint, float>> wpList = new UndirectedGraph<Waypoint, TaggedUndirectedEdge<Waypoint, float>>();
        //public AdjacencyGraph<Waypoint, Edge<Waypoint>> wpAList = new AdjacencyGraph<Waypoint, Edge<Waypoint>>();
        //public Dictionary<Edge<Waypoint>, double> wpCost = new Dictionary<Edge<Waypoint>, double>();
        [NonSerialized]public WGraph wGraph = new WGraph();
        private bool wpBool;
        private string wpFlag;
        private Point wpp;

        
        public List<Krypton.ShadowHull> shadows;
        List<Vector2> shadowVectors = new List<Vector2>();
        List<Rectangle> recs;

        List<Rectangle> dRecs = new List<Rectangle>();

        /// <summary>
        /// Costruttore per la generazione casuale del Dungeon.
        /// </summary>
        /// <param name="type">Tipo di Dungeon, corrispondente a un particolare stile e/o tileset.</param>
        /// <param name="floor">Piano in cui si trova il Dungeon rispetto alla struttura globale del gioco.</param>
        /// <param name="nEntries">Numero di entrate (correntemente inutilizzato)</param>
        /// <param name="lightColor">Colore della luce di fondo (parametro necessario al sistema di illuminazione</param>
        public Dungeon(int floorType, int wallType, int tileColor, int floor, int nEntries, Color lightColor)
        {
            #region common
            Random random1 = new Random();
            Random random2 = new Random(random1.Next(1, 204));
            this.width = random1.Next(50, 100);//40;
            this.height = random2.Next(50, 100);// width; //random.Next(50, 100);

            //this.type = type;
            name = "Piano " + floor.ToString();
            this.floor = floor;
            scale_factor = 1;
            rotation_angle = 0;
            depth = 0;

            this.structure = new tilecoord[width, height];
            this.grid = new Zone[(int)Math.Ceiling(32f * width / (float)gZsize), (int)Math.Ceiling(32f * height / (float)gZsize)];
            this.generated = true;

            id = new bool[Globals.max_entities];
            damageId = new bool[Globals.max_damages];
            int maxRooms = width * height / 500;

            playerSpawn = new List<spawningPlace>();
            spawn = new List<spawningPlace>();

            for (i = 0; i < Globals.max_damages; i++)
            {
                damageId[i] = false;
            }

            for (i = 0; i < (int)Math.Ceiling(32f * width / (float)gZsize); i++)
            {
                for (j = 0; j < (int)Math.Ceiling(32f * height / (float)gZsize); j++)
                {
                    grid[i, j] = new Zone(new Rectangle(i * gZsize, j * gZsize, gZsize, gZsize));
                }
            }
            #endregion

            //recs = new List<Rectangle>();
            this.floor = floor;
            this.floorType = floorType;
            this.wallType = wallType;
            this.lightColor = lightColor;

            int maxRoomTiles = width * height;
            int[,] matrix = new int[width, height]; //matrice dei blocchi liberi
            String[] tile_color = new String[4];
            tile_color[0]="green";
            tile_color[1]="black";
            tile_color[2]="red";
            tile_color[3]="gray";
            int tile_color_index;  //-1 per i livelli che non appartengono alla categoria "dungeon"
            bool color_randomize = false;
            bool chess = false;

            backgroundHue = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);

            if (floorType == (int)floorTypes.marble)
            {
                tile_color_index = 0;
                switch (tileColor)
                {
                    case (int)tileColors.green: tile_color_index = 0; break;
                    case (int)tileColors.black: tile_color_index = 1; break;
                    case (int)tileColors.red: tile_color_index = 2; break;
                    case (int)tileColors.gray: tile_color_index = 3; break;
                    case (int)tileColors.arlequin: color_randomize = true; break;
                    case (int)tileColors.chess: tile_color_index = 1; chess = true;  break;

                    default: break;
                }

                for (i = 0; i < width; i++)
                {
                    for (j = 0; j < height; j++)
                    {
                        //randomizzare le mattonelle
                        if (color_randomize)
                            tile_color_index = random1.Next(0, 4);
                        if (chess)
                            tile_color_index = 1 + (i + j) % 2;
                        int r = random1.Next(1, 1000);
                        if (r >= 0 && r < 330)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)[tile_color[tile_color_index] + "1"];
                        }
                        if (r >= 330 && r < 661)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)[tile_color[tile_color_index] + "2"];
                        }
                        if (r >= 661 && r <= 993)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)[tile_color[tile_color_index] + "3"];
                        }
                        if (r == 994)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["sunken_" + tile_color[tile_color_index]];
                        }
                        if (r == 995)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["cracked_" + tile_color[tile_color_index]];
                        }
                        if (r == 996)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["plain_" + tile_color[tile_color_index]];
                        }
                        if (r == 997)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["broken_" + tile_color[tile_color_index]];
                        }
                        if (r == 998)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["chipped_" + tile_color[tile_color_index]];
                        }
                        if (r == 999)
                        {
                            structure[i, j].bg = dic.floorTileIndex(floorType)["removed_tile"];
                        }

                        structure[i, j].fg = dic.floorTileIndex(floorType)["empty"];
                        structure[i, j].steppable = false;
                        //tutto il dungeon è vuoto e nero

                        matrix[i, j] = 0;
                        //tutti i blocchi sono liberi
                    }
                }
            }

            Point startingPoint = new Point(0, 0);
            Point maxDimensions = new Point(width - 1, height - 1);

            int n = 0;
            Room room;
            int dtilenumber = 0;
            int counter = 0;

            //fintanto che rimangono spazi liberi (soglia fissata a tre quinti del totale)
            while (counter < maxRooms)
            {

                do
                {
                    //istanzia le prime due stanze con la dimensione per contenere entrata e uscita
                    if (counter == 0 || counter == 1)
                    {
                        startingPoint.X = random1.Next(1, width - 1);
                        startingPoint.Y = random2.Next(1, height - 1);
                        maxDimensions.X = random1.Next(20, 25);
                        maxDimensions.Y = random2.Next(20, 25);
                    }
                    else
                    {
                        startingPoint.X = random1.Next(1, width - 1);
                        startingPoint.Y = random2.Next(1, height - 1);
                        maxDimensions.X = random1.Next(5, 16);
                        maxDimensions.Y = random2.Next(5, 16);
                    }
                }

                while (!CheckRoomDimensions(startingPoint, maxDimensions));

                maxRoomTiles -= (maxDimensions.X * maxDimensions.Y);
                dtilenumber += (maxDimensions.X * maxDimensions.Y);
                room = new Room(n, startingPoint, maxDimensions, this.floor, false, new List<int>());
                //debug, da tenere comunque
                bool intersect = false;
                foreach (Room r in roomList)
                {
                    if (room.getArea.Intersects(r.getArea))
                        intersect = true;
                }
                if (!intersect)
                {
                    room.AddWaypoint(wGraph.AddVertex(new Waypoint(room.getVCenter, 16, "center", room.getRoomNumber)), true);
                    roomList.Add(room);
                    for (i = 0; i < room.Width; i++)
                    {
                        for (j = 0; j < room.Height; j++)
                        {
                            matrix[room.getOrigin.X + i, room.getOrigin.Y + j] = 1;
                        }
                    }
                    n++;
                }

                counter++;
            }

            int nExits = random2.Next(1, 1); //ricalcolare i limiti inferiore e superiore in accordo al numero di piano
            int desiredExits = 0;
            int desiredEntries = 0;
            int exit = -1;
            int entry = -1;

            if (this.IsOk())
            {
                while (desiredExits < nExits)
                {
                    exit = random1.Next(0, roomList.Count);
                    if (roomList[exit].canHostAPassage())
                    {
                        exitPassage = roomList[exit].placePassage(true, ++desiredExits);
                        spawningPlace stair;
                        stair.depth = 0.97f;
                        stair.direction = new Vector2(0);
                        stair.health = 0;
                        stair.mana = 0;
                        stair.name = "exit";
                        stair.position = exitPassage;
                        stair.radius = 48;
                        stair.rotation = 0;
                        stair.speed = 0;
                        stair.subtype = this.type;
                        stair.type = (int)entityTypes.stairs;
                        spawn.Add(stair);
                    }
                }
                while (desiredEntries < nEntries)
                {
                    entry = random1.Next(0, roomList.Count);
                    if (roomList[entry].canHostAPassage())
                    {
                        entryPassage = roomList.ElementAt(entry).placePassage(false, (desiredEntries + 1) * -1);
                        spawningPlace stair;
                        stair.depth = 0.97f;
                        stair.direction = new Vector2(0);
                        stair.health = 0;
                        stair.mana = 0;
                        stair.name = "entry";
                        stair.position = entryPassage;
                        stair.radius = 48;
                        stair.rotation = 0;
                        stair.speed = 0;
                        stair.subtype = this.type;
                        stair.type = (int)entityTypes.stairs;
                        spawn.Add(stair);
                        desiredEntries++;

                        for (int z=0; z<Globals.players;z++)
                        {
                        spawningPlace psp;
                        psp.depth = 0.45f;
                        psp.direction = new Vector2(0, 0);
                        psp.health = 100f;
                        psp.mana = 100;
                        psp.position = new Vector2((entryPassage.X-64) + (128/(Globals.players ))* z, entryPassage.Y + 100);
                        psp.radius = 20;
                        psp.rotation = 1.0f;
                        psp.speed = 0.0f;
                        psp.type = 0;
                        psp.name = "Player " + (z + 1).ToString();
                        psp.subtype = -1;
                        playerSpawn.Add(psp);
                        }
                    }
                    

                }

                //nuovo algoritmo di collegamento stanze (tramite stanze corridoio)
                List<Room> supportRoomList = new List<Room>();

                //si copia la lista delle stanze in una lista di supporto
                foreach (Room r in roomList)
                {
                    supportRoomList.Add(r);
                }

                GenerateMap(ref wGraph, ref roomList, supportRoomList);
                //GenerateDBitmap("predMap");

                { }
                //inserire qui i controlli per le stanze non collegate
                supportRoomList.Clear();
                foreach (Room r in roomList)
                {
                    if(r.getLinkedRooms.Count()<2)
                        supportRoomList.Add(r);
                }

                if (supportRoomList.Count > 0)
                {
                    GenerateMap(ref wGraph, ref roomList, supportRoomList);
                }

                GraphCorrection();

                //trascrizione struttura stanze in quella di Dungeon
                foreach (Room r in roomList)
                {
                    for (i = 0; i < r.Width; i++)
                    {
                        for (j = 0; j < r.Height; j++)
                        {
                            structure[r.getOrigin.X + i, r.getOrigin.Y + j].fg = r.getStructure[i, j].fg;
                            structure[r.getOrigin.X + i, r.getOrigin.Y + j].steppable = r.getStructure[i, j].steppable;
                        }
                    }
                }

                foreach(Room r in roomList)
                {
                    //generazione torce - ATTENZIONE: rivedere gli offset relativi alle mattonelle:
                    //le torce vengono generate in coordinate tile non in coordinate pixel.
                    //Approfittare del foreach per generare altre entità scenografiche;
                    //la scenografia decorativa è generata altrove (vedi sotto).
                    if (r.Hallway)
                    {
                        if (r.Width >= r.Height)
                        {
                            int a = 0;
                            for (i = 2; i < r.Width - 2; i++)
                            {
                                a++;
                                a %= 2;
                                if ((r.getArea.Location.X + i) % 5 == 0 && !structure[r.getArea.Location.X + i, r.getArea.Top-1 + (r.getArea.Height+1) * a].steppable)
                                {
                                    spawningPlace sp;
                                    sp.depth = 0f;
                                    sp.direction = new Vector2(0, 0);
                                    sp.health = 100f;
                                    sp.mana = 0;
                                    sp.position = new Vector2((r.getArea.Location.X + i) * 32 + 16, (r.getArea.Top  + r.getArea.Height * a) * 32 -5 + 10*a);
                                    sp.radius = 100;
                                    sp.rotation = MathHelper.PiOver2 + MathHelper.Pi * a;
                                    sp.speed = 0.0f;
                                    sp.type = (int)entityTypes.torch;
                                    sp.name = "Edoardo Crisafulli";
                                    sp.subtype = -1;
                                    spawn.Add(sp);
                                }
                            }
                        }
                        else
                        {
                            int a = 0;
                            for (i = 2; i < r.Height - 2; i++)
                            {
                                a++;
                                a %= 2;
                                if ((r.getArea.Location.Y + i) % 5 == 0 && !structure[r.getArea.Left-1  + (r.getArea.Width+1) * a, r.getArea.Location.Y + i].steppable)
                                {
                                    spawningPlace sp;
                                    sp.depth = 0f;
                                    sp.direction = new Vector2(0, 0);
                                    sp.health = 100f;
                                    sp.mana = 0;
                                    sp.position = new Vector2((r.getArea.Left + r.getArea.Width * a) * 32 - 5 + 10 * a, (r.getArea.Location.Y + i) * 32 + 16);
                                    sp.radius = 100;
                                    sp.rotation = MathHelper.Pi * a;//MathHelper.PiOver2 + MathHelper.Pi*a;
                                    sp.speed = 0.0f;
                                    sp.type = (int)entityTypes.torch;
                                    sp.name = "Edoardo Crisafulli";
                                    sp.subtype = -1;
                                    spawn.Add(sp);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!structure[r.getArea.Left+r.getArea.Width/2, r.getArea.Top-1].steppable)   //parete nord
                        {
                            spawningPlace sp;
                            sp.depth = 0f;
                            sp.direction = new Vector2(0, 0);
                            sp.health = 100f;
                            sp.mana = 0;
                            sp.position = new Vector2((r.getArea.Left+r.getArea.Width/2) * 32 + 16, r.getArea.Top * 32 - 5);
                            sp.radius = 100;
                            sp.rotation = MathHelper.PiOver2;
                            sp.speed = 0.0f;
                            sp.type = (int)entityTypes.torch;
                            sp.name = "Edoardo Crisafulli";
                            sp.subtype = -1;
                            spawn.Add(sp);
                        }
                        if (!structure[r.getArea.Right, r.getArea.Top+r.getArea.Height/2].steppable)   //parete est
                        {
                            spawningPlace sp;
                            sp.depth = 0f;
                            sp.direction = new Vector2(0, 0);
                            sp.health = 100f;
                            sp.mana = 0;
                            sp.position = new Vector2(r.getArea.Right * 32 + 5, (r.getArea.Top + r.getArea.Height/2) * 32 + 16);
                            sp.radius = 100;
                            sp.rotation = MathHelper.Pi;
                            sp.speed = 0.0f;
                            sp.type = (int)entityTypes.torch;
                            sp.name = "Edoardo Crisafulli";
                            sp.subtype = -1;
                            spawn.Add(sp);
                        }
                        if (!structure[r.getArea.Left + r.getArea.Width / 2, r.getArea.Bottom].steppable)   //parete sud
                        {
                            spawningPlace sp;
                            sp.depth = 0f;
                            sp.direction = new Vector2(0, 0);
                            sp.health = 100f;
                            sp.mana = 0;
                            sp.position = new Vector2((r.getArea.Left + r.getArea.Width / 2) * 32 + 16, r.getArea.Bottom * 32 + 5);
                            sp.radius = 100;
                            sp.rotation = -MathHelper.PiOver2;
                            sp.speed = 0.0f;
                            sp.type = (int)entityTypes.torch;
                            sp.name = "Edoardo Crisafulli";
                            sp.subtype = -1;
                            spawn.Add(sp);
                        }
                        if (!structure[r.getArea.Left-1, r.getArea.Top + r.getArea.Height / 2].steppable)   //parete ovest
                        {
                            spawningPlace sp;
                            sp.depth = 0f;
                            sp.direction = new Vector2(0, 0);
                            sp.health = 100f;
                            sp.mana = 0;
                            sp.position = new Vector2(r.getArea.Left * 32 - 5, (r.getArea.Top + r.getArea.Height/2) * 32 + 16);
                            sp.radius = 100;
                            sp.rotation = 0.0f;
                            sp.speed = 0.0f;
                            sp.type = (int)entityTypes.torch;
                            sp.name = "Edoardo Crisafulli";
                            sp.subtype = -1;
                            spawn.Add(sp);
                        }
                    }
                }

                GenerateWallMap();

                //shadowVectors = new List<Vector2>();
                GenerateShadowMap();
                if (Globals.generateDMap)
                    GenerateDBitmap("dMap");

                #region muri predefiniti
                structure[0, 0].fg = dic.wallTileIndex(wallType)["empty"];
                structure[0, 0].steppable = false;
                structure[0, height - 1].fg = dic.wallTileIndex(wallType)["empty"];
                structure[0, height - 1].steppable = false;
                structure[width - 1, 0].fg = dic.wallTileIndex(wallType)["empty"];
                structure[width - 1, 0].steppable = false;
                structure[width - 1, height - 1].fg = dic.wallTileIndex(wallType)["empty"];
                structure[width - 1, height - 1].steppable = false;
                for (i = 1; i < width - 1; i++)
                {
                    structure[i, 0].fg = dic.wallTileIndex(wallType)["empty"];
                    structure[i, 0].steppable = false;
                }
                for (i = 1; i < width - 1; i++)
                {
                    structure[i, height - 1].fg = dic.wallTileIndex(wallType)["empty"];
                    structure[i, height - 1].steppable = false;
                }
                for (i = 1; i < height - 1; i++)
                {
                    structure[0, i].fg = dic.wallTileIndex(wallType)["empty"];
                    structure[0, i].steppable = false;
                }
                for (i = 1; i < height - 1; i++)
                {
                    structure[width - 1, i].fg = dic.wallTileIndex(wallType)["empty"];
                    structure[width - 1, i].steppable = false;
                }
                #endregion

                #region aggiustamento muri

                if (structure[1, 1].fg != dic.wallTileIndex(wallType)["blank"])
                    structure[0, 0].fg = dic.wallTileIndex(wallType)["black"];
                else structure[0, 0].fg = dic.wallTileIndex(wallType)["NW_corner_wall"];

                if (structure[width - 2, 1].fg != dic.wallTileIndex(wallType)["blank"])
                    structure[width - 1, 0].fg = dic.wallTileIndex(wallType)["black"];
                else structure[width - 1, 0].fg = dic.wallTileIndex(wallType)["NE_corner_wall"];

                if (structure[1, height - 2].fg != dic.wallTileIndex(wallType)["blank"])
                    structure[0, height - 1].fg = dic.wallTileIndex(wallType)["black"];
                else structure[0, height - 1].fg = dic.wallTileIndex(wallType)["SW_corner_wall"];

                if (structure[width - 2, height - 2].fg != dic.wallTileIndex(wallType)["blank"])
                    structure[width - 1, height - 1].fg = dic.wallTileIndex(wallType)["black"];
                else structure[width - 1, height - 1].fg = dic.wallTileIndex(wallType)["SE_corner_wall"];

                byte around = 0;

                //parete settentrionale
                for (i = 1; i < width - 1; i++)
                {
                    if (structure[i - 1, 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 32;
                    if (structure[i, 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 16;
                    if (structure[i + 1, 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 8;

                    switch (around)
                    {
                        case 0: structure[i, 0].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                        case 8: structure[i, 0].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                        case 16: structure[i, 0].fg = dic.wallTileIndex(wallType)["S_T-wall"]; break;
                        case 24: structure[i, 0].fg = dic.wallTileIndex(wallType)["NE_corner_wall"]; break;
                        case 32: structure[i, 0].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                        case 40: structure[i, 0].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                        case 48: structure[i, 0].fg = dic.wallTileIndex(wallType)["NW_corner_wall"]; break;
                        case 56: structure[i, 0].fg = dic.wallTileIndex(wallType)["black"]; break;
                    }

                    around = 0;
                }

                //parete occidentale
                for (j = 1; j < height - 1; j++)
                {
                    if (structure[1, j - 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 2;
                    if (structure[1, j].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 4;
                    if (structure[1, j + 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 8;

                    switch (around)
                    {
                        case 0: structure[0, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                        case 2: structure[0, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                        case 4: structure[0, j].fg = dic.wallTileIndex(wallType)["E_T-wall"]; break;
                        case 6: structure[0, j].fg = dic.wallTileIndex(wallType)["NW_corner_wall"]; break;
                        case 8: structure[0, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                        case 10: structure[0, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                        case 12: structure[0, j].fg = dic.wallTileIndex(wallType)["SW_corner_wall"]; break;
                        case 14: structure[0, j].fg = dic.wallTileIndex(wallType)["black"]; break;
                    }

                    around = 0;
                }

                //parete meridionale
                for (i = 1; i < width - 1; i++)
                {
                    if (structure[i - 1, height - 2].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 128;
                    if (structure[i, height - 2].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 1;
                    if (structure[i + 1, height - 2].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 2;

                    switch (around)
                    {
                        case 0: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                        case 1: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["N_T-wall"]; break;
                        case 2: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                        case 3: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["SE_corner_wall"]; break;
                        case 128: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                        case 129: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["SW_corner_wall"]; break;
                        case 130: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                        case 131: structure[i, height - 1].fg = dic.wallTileIndex(wallType)["black"]; break;

                    }

                    around = 0;
                }

                //parete orientale
                for (j = 1; j < height - 1; j++)
                {
                    if (structure[width - 2, j - 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 128;
                    if (structure[width - 2, j].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 64;
                    if (structure[width - 2, j + 1].fg != dic.wallTileIndex(wallType)["blank"])
                        around += 32;

                    switch (around)
                    {
                        case 0: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                        case 32: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                        case 64: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["W_T-wall"]; break;
                        case 96: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["SE_corner_wall"]; break;
                        case 128: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                        case 160: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                        case 192: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["NE_corner_wall"]; break;
                        case 224: structure[width - 1, j].fg = dic.wallTileIndex(wallType)["black"]; break;
                    }

                    around = 0;
                }

                for (i = 1; i < width - 1; i++)
                {
                    for (j = 1; j < height - 1; j++)
                    {
                        if (structure[i, j].fg == dic.wallTileIndex(wallType)["empty"] && structure[i, j].steppable == false)
                        {
                            if (structure[i - 1, j - 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 128;
                            if (structure[i - 1, j].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 64;
                            if (structure[i - 1, j + 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 32;
                            if (structure[i, j + 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 16;
                            if (structure[i + 1, j + 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 8;
                            if (structure[i + 1, j].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 4;
                            if (structure[i + 1, j - 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 2;
                            if (structure[i, j - 1].fg != dic.wallTileIndex(wallType)["blank"])
                                around += 1;


                            switch (around)
                            {
                                case 0: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 1: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 2: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 3: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 4: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 5: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 6: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 7: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 8: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 9: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 10: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 11: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 12: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 13: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 14: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 15: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 16: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 17: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 18: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 19: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 20: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 21: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_T-wall"]; break;
                                case 22: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 23: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_reverse_L-wall"]; break;
                                case 24: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 25: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 26: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 27: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 28: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 29: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_L-wall"]; break;
                                case 30: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 31: structure[i, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                                case 32: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 33: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 34: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 35: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 36: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 37: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 38: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 39: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 40: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 41: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 42: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 43: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 44: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 45: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 46: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 47: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 48: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 49: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 50: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 51: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 52: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 53: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_T-wall"]; break;
                                case 54: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 55: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_reverse_L-wall"]; break;
                                case 56: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 57: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 58: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 59: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 60: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 61: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_L-wall"]; break; //da controllare
                                case 62: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 63: structure[i, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                                case 64: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 65: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 66: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 67: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 68: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 69: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_T-wall"]; break;
                                case 70: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 71: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break; //da controllare
                                case 72: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 73: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 74: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 75: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 76: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 77: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_T-wall"]; break;
                                case 78: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 79: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break;
                                case 80: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 81: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_T-wall"]; break;
                                case 82: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 83: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_T-wall"]; break;
                                case 84: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_T-wall"]; break;
                                case 85: structure[i, j].fg = dic.wallTileIndex(wallType)["X-wall"]; break;
                                case 86: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_T-wall"]; break;
                                case 87: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_X-wall"]; break;
                                case 88: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 89: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_T-wall"]; break;
                                case 90: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 91: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_T-wall"]; break;
                                case 92: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_reverse_L-wall"]; break;
                                case 93: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_X-wall"]; break;
                                case 94: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_reverse_L-wall"]; break;
                                case 95: structure[i, j].fg = dic.wallTileIndex(wallType)["W_T-wall"]; break;
                                case 96: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 97: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 98: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 99: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 100: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 101: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_T-wall"]; break;
                                case 102: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 103: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break;
                                case 104: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 105: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 106: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 107: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SE_wall"]; break;
                                case 108: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 109: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break;
                                case 110: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 111: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break;
                                case 112: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 113: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_reverse_L-wall"]; break;
                                case 114: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 115: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_reverse_L-wall"]; break;
                                case 116: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_L-wall"]; break;
                                case 117: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_X-wall"]; break;
                                case 118: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_L-wall"]; break;
                                case 119: structure[i, j].fg = dic.wallTileIndex(wallType)["double_corner_NW_SE"]; break;
                                case 120: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 121: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_reverse_L-wall"]; break;
                                case 122: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 123: structure[i, j].fg = dic.wallTileIndex(wallType)["W_T-wall"]; break;
                                case 124: structure[i, j].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                                case 125: structure[i, j].fg = dic.wallTileIndex(wallType)["N_T-wall"]; break;
                                case 126: structure[i, j].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                                case 127: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_corner_wall"]; break;
                                case 128: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 129: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 130: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 131: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 132: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 133: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 134: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 135: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 136: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 137: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 138: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 139: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 140: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 141: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 142: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 143: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 144: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 145: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 146: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 147: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 148: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 149: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_T-wall"]; break;
                                case 150: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 151: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_reverse_L-wall"]; break;
                                case 152: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 153: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 154: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 155: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 156: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 157: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_L-wall"]; break;
                                case 158: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 159: structure[i, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                                case 160: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 161: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 162: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 163: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 164: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 165: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 166: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 167: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 168: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 169: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_L-wall"]; break;
                                case 170: structure[i, j].fg = dic.wallTileIndex(wallType)["wall"]; break;
                                case 171: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_wall"]; break;
                                case 172: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 173: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_SW_wall"]; break;
                                case 174: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_W_wall"]; break;
                                case 175: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_wall"]; break;
                                case 176: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 177: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 178: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 179: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 180: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 181: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_T-wall"]; break;
                                case 182: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NW_wall"]; break;
                                case 183: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_reverse_L-wall"]; break;
                                case 184: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 185: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 186: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_N_wall"]; break;
                                case 187: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_vertical_wall"]; break;
                                case 188: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 189: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_L-wall"]; break;
                                case 190: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_wall"]; break;
                                case 191: structure[i, j].fg = dic.wallTileIndex(wallType)["W_wall"]; break;
                                case 192: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 193: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 194: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 195: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 196: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 197: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_reverse_L-wall"]; break;
                                case 198: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 199: structure[i, j].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                                case 200: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 201: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 202: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 203: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 204: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 205: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_reverse_L-wall"]; break;
                                case 206: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 207: structure[i, j].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                                case 208: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 209: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_L-wall"]; break;
                                case 210: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 211: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_L-wall"]; break;
                                case 212: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_T-wall"]; break;
                                case 213: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_S_T-wall"]; break;
                                case 214: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_X-wall"]; break;
                                case 215: structure[i, j].fg = dic.wallTileIndex(wallType)["S_T-wall"]; break;
                                case 216: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 217: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_L-wall"]; break;
                                case 218: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_NE_wall"]; break;
                                case 219: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_L-wall"]; break;
                                case 220: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_reverse_L-wall"]; break;
                                case 221: structure[i, j].fg = dic.wallTileIndex(wallType)["double_corner_SW_NE"]; break;
                                case 222: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_reverse_L-wall"]; break;
                                case 223: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_corner_wall"]; break;
                                case 224: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 225: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 226: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 227: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 228: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 229: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_reverse_L-wall"]; break;
                                case 230: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 231: structure[i, j].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                                case 232: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 233: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 234: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_E_wall"]; break;
                                case 235: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_wall"]; break;
                                case 236: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 237: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_reverse_L-wall"]; break;
                                case 238: structure[i, j].fg = dic.wallTileIndex(wallType)["thin_horizontal_wall"]; break;
                                case 239: structure[i, j].fg = dic.wallTileIndex(wallType)["S_wall"]; break;
                                case 240: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 241: structure[i, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                                case 242: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 243: structure[i, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                                case 244: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_L-wall"]; break;
                                case 245: structure[i, j].fg = dic.wallTileIndex(wallType)["E_T-wall"]; break;
                                case 246: structure[i, j].fg = dic.wallTileIndex(wallType)["SE_L-wall"]; break;
                                case 247: structure[i, j].fg = dic.wallTileIndex(wallType)["NW_corner_wall"]; break;
                                case 248: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 249: structure[i, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                                case 250: structure[i, j].fg = dic.wallTileIndex(wallType)["NE_wall"]; break;
                                case 251: structure[i, j].fg = dic.wallTileIndex(wallType)["E_wall"]; break;
                                case 252: structure[i, j].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                                case 253: structure[i, j].fg = dic.wallTileIndex(wallType)["SW_corner_wall"]; break;
                                case 254: structure[i, j].fg = dic.wallTileIndex(wallType)["N_wall"]; break;
                                case 255: structure[i, j].fg = dic.wallTileIndex(wallType)["black"]; break;

                            }
                            around = 0;
                        }
                    }
                }
                #endregion

                GenerateScenography();

                #region istanziazione random elementi interattivi


                /*
                //istanziazione porte basata sui tile adiacenti
                for (i = 5; i < width - 5; i++)
                {
                    for (j = 5; j < height - 5; j++)
                    {
                        if (structure[i, j].steppable == true && (structure[i + 1, j].steppable == false && (structure[i - 1, j].steppable == false) || (structure[i, j + 1].steppable == false && structure[i, j - 1].steppable == false)))
                        {
                            spawningPlace sp;
                            sp.depth = 0.97f;
                            sp.direction = new Vector2(0, 0);
                            sp.health = 100f;
                            sp.mana = 100;
                            sp.position = new Vector2(i * 32 + 16, j * 32 + 16);
                            sp.radius = 10;
                            sp.speed = 0.0f;
                            sp.type = (int)entityTypes.door;
                            sp.name = "Edoardo Crisafulli";
                            sp.subtype = -1;
                            bool isNear = false;
                            foreach (spawningPlace d in spawn.OfType<spawningPlace>())
                            {

                                if (Math.Abs((int)sp.position.X - (int)d.position.X) < 64 && Math.Abs((int)sp.position.Y - (int)d.position.Y) < 64)
                                {
                                    isNear = true;
                                }
                                else
                                    isNear = false;
                            }
                            if (!isNear)
                            {
                                if (structure[i + 1, j + 1].steppable == true && structure[i - 1, j + 1].steppable == true && structure[i, j + 1].steppable == true)
                                {
                                    sp.rotation = (float)Math.PI;
                                    spawn.Add(sp);
                                }

                                else if (structure[i + 1, j + 1].steppable == true && structure[i + 1, j - 1].steppable == true && structure[i + 1, j].steppable == true)
                                {
                                    sp.rotation = (float)Math.PI / 2;
                                    spawn.Add(sp);
                                }

                                else if (structure[i - 1, j - 1].steppable == true && structure[i - 1, j + 1].steppable == true && structure[i - 1, j].steppable == true)
                                {
                                    sp.rotation = (float)Math.PI * 1.5f;
                                    spawn.Add(sp);
                                }

                                else if (structure[i - 1, j - 1].steppable == true && structure[i + 1, j - 1].steppable == true && structure[i, j - 1].steppable == true)
                                {
                                    sp.rotation = 0;
                                    spawn.Add(sp);
                                }
                            }
                        }
                    }
                }*/

                /*for (i = 0; i < floor; i++)
                {
                    spawningPlace sp;
                    sp.depth = 0f;
                    sp.direction = new Vector2(0, 0);
                    sp.health = 100f;
                    sp.mana = 0;
                    sp.position = new Vector2(520 + (60 * i), 200 + (60 * i));
                    sp.radius = 100;
                    sp.rotation = 0.0f;
                    sp.speed = 0.0f;
                    sp.type = (int)entityTypes.torch;
                    sp.name = "Edoardo Crisafulli";
                    sp.subtype = -1;
                    spawn.Add(sp);
                }*/

                //generazione casuale dei mostri
                //bool gen=true;
                Random s = new Random();
                for (i = 0; i < width /*&& gen*/; i++)
                {
                    for (j = 0; j < height/* && gen*/; j++)
                    {
                        if (structure[i, j].steppable && (s.NextDouble() > 0.99f))
                        {
                            
                            spawningPlace sp;
                            sp.depth = 0.45f;
                            sp.direction = new Vector2(0, 0);
                            sp.rotation = (float)(s.NextDouble() * 2 * Math.PI);
                            sp.speed = 0.0f;
                            sp.mana = 0;

                            
                            switch (s.Next(0, 4))
                            {
                                case 0: sp.type = (int)entityTypes.spiderbot;
                                    sp.name = "Zombie " + i.ToString();
                                    sp.health = 20f;
                                    sp.radius = 10;
                                    // gen = false;
                                    break;
                                case 1: sp.type = (int)entityTypes.zombie; 
                                    sp.name = "SpBot XZ " + i.ToString();
                                    sp.health = 100f;
                                    sp.radius = 20;
                                    break;
                                case 2: sp.type = (int)entityTypes.wizard;
                                    sp.name = "Wizzy " + i.ToString();
                                    sp.health = 5f;
                                    sp.radius = 10;
                                    break;
                                case 3: sp.type = (int)entityTypes.banshee;
                                    sp.name = "Banshee " + i.ToString();
                                    sp.health = 50f;
                                    sp.radius = 10;
                                    break;
                                default: sp.type = (int)entityTypes.banshee;
                                    sp.name = "Banshee " + i.ToString();
                                    sp.health = 50f;
                                    sp.radius = 10;
                                    break;
                            }
                            
                            
                            sp.subtype = -1;
                            sp.position = new Vector2(i * 32 + 16, j * 32 + 16);
                            spawn.Add(sp);
                            
                        }
                    }
                }

                /*spawningPlace qwe;
                qwe.type = (int)entityTypes.spiderbot;
                qwe.name = "Zombie " + i.ToString();
                qwe.health = 20f;
                qwe.radius = 10;
                qwe.depth = 0.45f;
                qwe.direction = new Vector2(0, 0);
                qwe.rotation = (float)(s.NextDouble() * 2 * Math.PI);
                qwe.speed = 0.0f;
                qwe.mana = 0;
                qwe.subtype = -1;
                qwe.position = roomList[0].getVCenter;
                spawn.Add(qwe);*/

                //for (int i = 0; i < roomList.Count; i++)
                //{
                    spawningPlace qwerty;
                    qwerty.depth = 0.45f;
                    qwerty.direction = new Vector2(0, 0);
                    qwerty.rotation = (float)(s.NextDouble() * 2 * Math.PI);
                    qwerty.speed = 0.0f;
                    qwerty.mana = 0;
                    qwerty.type = (int)entityTypes.wizard;
                    qwerty.name = "qwertyBot XZ " + i.ToString();
                    qwerty.health = 100f;
                    qwerty.radius = 20;
                    qwerty.subtype = -1;
                    qwerty.position = roomList[0].getVCenter;//new Vector2(roomList[i].getCenter.X*32, roomList[i].getCenter.Y*32);
                    spawn.Add(qwerty);

                    qwerty.depth = 0.45f;
                    qwerty.direction = new Vector2(0, 0);
                    qwerty.rotation = (float)(s.NextDouble() * 2 * Math.PI);
                    qwerty.speed = 0.0f;
                    qwerty.mana = 0;
                    qwerty.type = (int)entityTypes.table ;
                    qwerty.name = "qwertyBot XZ " + i.ToString();
                    qwerty.health = 100f;
                    qwerty.radius = 20;
                    qwerty.subtype = -1;
                    qwerty.position = roomList[0].getVCenter + new Vector2(80, 80);//new Vector2(roomList[i].getCenter.X*32, roomList[i].getCenter.Y*32);
                    spawn.Add(qwerty);
                //}

                #endregion
            }

            //registrazione delle stanze nella griglia di zone di collisione
            foreach (Room r in roomList)
            {
                for (int i = r.getArea.Left; i < r.getArea.Right; i++)
                {
                    for (int j = r.getArea.Top; j < r.getArea.Bottom; j++)
                    {
                        grid[i / 4, j / 4].RegisterRoom(r.getRoomNumber);
                    }
                }
                foreach (int w in r.waypoint)
                {
                    grid[(int)wGraph.getVertex(w).c.Center.X / 128, (int)wGraph.getVertex(w).c.Center.Y / 128].RegisterWaypoint(w);
                }
            }
            /*
            List<String> graphD = new List<String>();
            graphD.Add(wGraph.Vertices.Count().ToString() + " " + wGraph.Edges.Count().ToString() + " " + roomList.Count.ToString());

            foreach (Waypoint w in wGraph.Vertices)
            {
                graphD.Add(w.c.Center.ToString() + " " + w.direction);
            }

            foreach (Edge e in wGraph.Edges)
            {
                graphD.Add(wGraph.getVertex(e.from).c.Center.ToString() + " " + wGraph.getVertex(e.to).c.Center.ToString() + " " + e.weight.ToString());
            }
            System.IO.File.WriteAllLines(@"graph.txt", graphD);

            byte[,] pixelMatrix = new byte[width * 32, height * 32];
            Color[,] colorMatrix = new Color[width * 32, height * 32];

            shadowVectors = new List<Vector2>();
            //GenerateShadowMap();

            GenerateDBitmap("dMap");

            
            using (System.Drawing.Bitmap dMap = new System.Drawing.Bitmap(width * 32 + 1000, height * 32 + 1000))
            {
                foreach (Vector2 v in shadowVectors)
                {
                    dMap.SetPixel((int)v.X, (int)v.Y, System.Drawing.Color.Black);
                }
                dMap.Save("dmap8.bmp");
            }
            */


            /*playerSpawn = new List<spawningPlace>(); //debug solo se non ci sono scale
            for (int z = 0; z < Globals.players; z++)
            {
                spawningPlace psp;
                psp.depth = 0.45f;
                psp.direction = new Vector2(0, 0);
                psp.health = 100f;
                psp.mana = 100;
                psp.position = new Vector2(100 * (z + 1), 100);
                psp.radius = 20;
                psp.rotation = 1.0f;
                psp.speed = 0.0f;
                psp.type = 0;
                psp.name = "Player " + (z + 1).ToString();
                psp.subtype = -1;
                playerSpawn.Add(psp);
            }*/
 

            Thread.Sleep(20);
        }

        /// <summary>
        /// Costruttore per la generazione di un Dungeon standard di prova usato durante lo sviluppo.
        /// </summary>

        public Dungeon(String fileName,int floor)
        {
            //caricamente stringhe di lettura per il caricamento
            int n = 200;
            int m = 200;
            this.floor = floor; //temporaneo per la generazione delle entità
            #region common
            Random random1 = new Random();
            Random random2 = new Random(random1.Next(1, 204));
            this.width = n;//40;
            this.height = m;// width; //random.Next(50, 100);

          //  this.type = type;
            name = "Piano 0";// +floor.ToString();
           // this.floor = floor;
            scale_factor = 1;
            rotation_angle = 0;
            depth = 0;

            this.structure = new tilecoord[width, height];
            this.grid = new Zone[(int)Math.Ceiling(32f * width / (float)gZsize), (int)Math.Ceiling(32f * height / (float)gZsize)];
            this.generated = true;

            id = new bool[Globals.max_entities];
            damageId = new bool[Globals.max_damages];
            int maxRooms = width * height / 500;

            playerSpawn = new List<spawningPlace>();
            spawn = new List<spawningPlace>();

         /*   for (int i = 0; i < Globals.max_damages; i++)
            {
                damageId[i] = false;
            }

            for (i = 0; i < (int)Math.Ceiling(32f * width / (float)gZsize); i++)
            {
                for (j = 0; j < (int)Math.Ceiling(32f * height / (float)gZsize); j++)
                {
                    grid[i, j] = new Zone(new Rectangle(i * gZsize, j * gZsize, gZsize, gZsize));
                }
            }*/
            #endregion
            backgroundHue = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
            spawn = new List<spawningPlace>();
            spawningPlace sp;

            sp.depth = Depths.item;
            sp.direction = new Vector2(0, 0);
            sp.health = 100f;
            sp.mana = 100;
            sp.position = new Vector2(2963, 1725);
            sp.radius = 10;
            sp.rotation = 0.0f;
            sp.speed = 0.0f;
            sp.type = (int)entityTypes.item;
            sp.name = i.ToString();
            sp.subtype = 33;
            spawn.Add(sp);

            sp.depth = Depths.item;
            sp.direction = new Vector2(0, 0);
            sp.health = 100f;
            sp.mana = 100;
            sp.position = new Vector2(2785,1732);
            sp.radius = 10;
            sp.rotation = 0.0f;
            sp.speed = 0.0f;
            sp.type = (int)entityTypes.item;
            sp.name = i.ToString();
            sp.subtype = 86;
            spawn.Add(sp);

            sp.depth = Depths.item;
            sp.direction = new Vector2(0, 0);
            sp.health = 100f;
            sp.mana = 100;
            sp.position = new Vector2(2845,1630);
            sp.radius = 10;
            sp.rotation = 0.0f;
            sp.speed = 0.0f;
            sp.type = (int)entityTypes.item;
            sp.subtype = 44;
            spawn.Add(sp);



            //per testare gli oggetti
          /*  for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    
                    sp.depth = Depths.item;
                    sp.direction = new Vector2(0, 0);
                    sp.health = 100f;
                    sp.mana = 100;
                    sp.position = new Vector2(300 + 30 * ((i + 1)), 250 + 64 * j);
                    sp.radius = 10;
                    sp.rotation = 0.0f;
                    sp.speed = 0.0f;
                    sp.type = (int)entityTypes.item;
                    sp.name = i.ToString();
                    if (10 * j + i < 94)
                        sp.subtype = 10 * j + i;
                    else
                        sp.subtype = 1;
                    spawn.Add(sp);
                }
            }*/


            //exits.Add(new passage(new Point(11, 4), true, 1));
            //scenography.Add(new SceneElement(dic.sceneIndex["exit"], new Point(11*32, 4*32), 0f, new Rectangle(11*32+16, 4*32+16, 64, 64), 0.05f, Color.White));

            /*
            exitPassage = new Vector2(11 * 32, 4 * 32);
            spawningPlace stair;
            stair.depth = 0.97f;
            stair.direction = new Vector2(0);
            stair.health = 0;
            stair.mana = 0;
            stair.name = "exit";
            stair.position = exitPassage;
            stair.radius = 48;
            stair.rotation = 0;
            stair.speed = 0;
            stair.subtype = this.type;
            stair.type = (int)entityTypes.stairs;
            spawn.Add(stair);
            */
            //modifica di structure per il posizionamento di stairs
            /*structure[(int)(exitPassage.X / 32 - 2), (int)(exitPassage.Y / 32 - 2)].steppable = false;
            structure[(int)(exitPassage.X / 32 - 2), (int)(exitPassage.Y / 32 - 1)].steppable = false;
            structure[(int)(exitPassage.X / 32 - 2), (int)(exitPassage.Y / 32)].steppable = false;
            structure[(int)(exitPassage.X / 32 - 2), (int)(exitPassage.Y / 32 + 1)].steppable = false;

            structure[(int)(exitPassage.X / 32 - 1), (int)(exitPassage.Y / 32 - 2)].steppable = false;
            structure[(int)(exitPassage.X / 32 ), (int)(exitPassage.Y / 32 - 2)].steppable = false;
            structure[(int)(exitPassage.X / 32 + 1), (int)(exitPassage.Y / 32 - 2)].steppable = false;

            structure[(int)(exitPassage.X / 32 + 2), (int)(exitPassage.Y / 32 - 2)].steppable = false;
            structure[(int)(exitPassage.X / 32 + 2), (int)(exitPassage.Y / 32 - 1)].steppable = false;
            structure[(int)(exitPassage.X / 32 + 2), (int)(exitPassage.Y / 32)].steppable = false;
            structure[(int)(exitPassage.X / 32 + 2), (int)(exitPassage.Y / 32 + 1)].steppable = false;
            */


            for (i = 0; i < Globals.max_entities; i++)
            {
                id[i] = false;
            }
            for (i = 0; i < Globals.max_damages; i++)
            {
                damageId[i] = false;
            }

            for (i = 0; i < (int)Math.Ceiling(32f * width / (float)gZsize); i++)
            {
                for (j = 0; j < (int)Math.Ceiling(32f * height / (float)gZsize); j++)
                {
                    grid[i, j] = new Zone(new Rectangle(i * gZsize, j * gZsize, gZsize, gZsize));
                }
            }
            
            String[] structReader = new String[1];
            List<String> waypointReader = new List<String>();
            List<String> edgesReader = new List<String>();
            String[] entityReader = new String[n * m];
            //List<SceneElement> scenografyReader = new List<SceneElement>();
            String support;
            int nEOF = 0;

            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            i = 0;
            while ((support = file.ReadLine()) != null)
            {
                if (support != "EOF")
                {
                    if (nEOF == 0)
                    {
                        //dimensione dungeon
                        String[] dimDungeon = new String[2];
                        dimDungeon = support.Split('|');
                        n = Convert.ToInt32(dimDungeon[0]);
                        m = Convert.ToInt32(dimDungeon[1]);
                        structReader = new String[n * m + 100];
                        this.structure = new tilecoord[n, m];
                        this.name = dimDungeon[2];
                        spawningPlace spl = new spawningPlace() ;
                        if (Convert.ToInt32( dimDungeon[3]) != -1)
                        {
                            
                            spl.type = (int)entityTypes.stairs;
                            spl.subtype = 0;
                            spl.speed = 0;
                            spl.rotation = (float)Convert.ToDouble(dimDungeon[5]);
                            spl.radius = 160;
                            entryPassage = new Vector2((float)Convert.ToDouble(dimDungeon[3]), (float)Convert.ToDouble(dimDungeon[4]));
                            spl.position = entryPassage;
                            spl.name = "entry";
                            spl.mana = 0;
                            spl.health = 0;
                            spl.direction = new Vector2();
                            spl.depth = Depths.stairs;
                            AddSpawn(spl);
                        }
                       
                        if (Convert.ToInt32(dimDungeon[6]) != -1)
                        {
                            spl.type = (int)entityTypes.stairs;
                            spl.subtype = 1;
                            spl.name = "exit";
                            exitPassage = new Vector2((float)Convert.ToDouble(dimDungeon[6]), (float)Convert.ToDouble(dimDungeon[7]));
                            spl.position = exitPassage;
                            spl.rotation = (float)Convert.ToDouble(dimDungeon[8]);
                            AddSpawn(spl);
                        }
                            floorType = Convert.ToInt32(dimDungeon[9]);
                            wallType = Convert.ToInt32(dimDungeon[10]);
                        
                    }
                    //legge le riga prima della scritta "EOF" le quali determinano la struttura della matrice
                    else if (nEOF == 1)
                    {
                        structReader[i] = support;
                        i++;
                    }
                    else if (nEOF == 2)
                    {
                        //legge la terza parte del file suddiviso per EOF, che rappresenta i waypoint del grafo
                        waypointReader.Add(support);
                        i++;
                    }
                    else if (nEOF == 3)
                    {
                        //legge la quarte parte del file, che rappresenta gli edge del grafo
                        edgesReader.Add(support);
                        i++;
                    }
                    else if (nEOF == 4)
                    {
                        //legge la quinta parte del file, la quale indica gli elementi scenografici del dungeon
                        if (support != "")
                        {
                            String[] element = support.Split('|');
                            SceneElement scene = new SceneElement(Convert.ToInt32(element[0]), new Vector2(Convert.ToInt32(element[1]), Convert.ToInt32(element[2])), (float)Convert.ToDouble(element[3]), 46, Depths.scenography, Color.White);
                            //scene.number = Convert.ToInt32(element[0]);
                            //scene.position = new System.Drawing.Point(Convert.ToInt32(element[1]), Convert.ToInt32(element[2]));
                            //scene.rotation = Convert.ToDouble(element[3]);
                            //scene.color = new System.Drawing.Color();
                            scenography.Add(scene);
                            //scenografyReader.Add(scene);
                        }
                    }


                    else if (nEOF == 5)
                    {
                        //legge la quinta ed ultima parte del file, rappresentante la scenografia del dungeon
                    }
                    else if (nEOF == 6)
                    {
                        //legge la sesta ed ultima parte del file, rappresentante le entità presenti nel dungeon
                        if (support != "")
                        {
                            String[] element = support.Split('|');
                            spawningPlace esp = new spawningPlace();
                            //esp.number = Convert.ToInt32(element[0]);
                            esp.type = Convert.ToInt32(element[1]);
                            esp.depth = (float)(Convert.ToDouble(element[2]));
                            esp.health = (float)(Convert.ToDouble(element[3]));
                            esp.mana = Convert.ToInt32(element[4]);
                            esp.name = element[5];

                            esp.position = new Vector2(Convert.ToInt32(element[6]), Convert.ToInt32(element[7]));
                            esp.radius = Convert.ToInt32(element[8]);
                            esp.rotation = (float)(Convert.ToDouble(element[9]));
                            esp.speed = (float)(Convert.ToDouble(element[10]));
                            esp.subtype = Convert.ToInt32(element[11]);
                            
                            spawn.Add(esp);
                        }
                    }
                 //   else if (nEOF == 7)
                    {
                        //legge la sesta ed ultima parte del file, rappresentante i punti di spawn dei giocatori umani
                        playerSpawn = new List<spawningPlace>();
                        for (int z = 0; z < Globals.players; z++)
                        {

                            spawningPlace psp;
                            psp.depth = 0.45f;
                            psp.direction = new Vector2(0, 0);
                            psp.health = 100f;
                            psp.mana = 100;
                            psp.rotation = 1.0f;
                            psp.position = new Vector2(100 * (z + 1), 100);
                            switch (z)
                            {
                                case 0:
                                    psp.position = new Vector2(572, 675);
                                    psp.rotation = -MathHelper.PiOver2;
                                    break;
                                case 1:
                                    psp.position = new Vector2(555, 700);
                                    psp.rotation = ((float)MathHelper.Pi /6) * 5;
                                    break;
                                case 2:
                                    psp.position = new Vector2(590, 700);
                                    psp.rotation = (float)MathHelper.Pi / 6;
                                    
                                    break;
                               /* case 0:                             
                                    psp.position = new Vector2(572, 975);
                                    psp.rotation = 0;
                                    break;
                                case 1:
                                    psp.position = new Vector2(305, 1000);
                                    psp.rotation = 0;
                                    break;
                                case 2:
                                    psp.position = new Vector2(590, 1000);
                                    psp.rotation = 0;
                                    
                                    break;*/
                            }
                            psp.radius = 20;
                            
                            psp.speed = 0.0f;
                            psp.type = 0;
                            psp.name = "Player " + (z + 1).ToString();
                            psp.subtype = -1;
                            playerSpawn.Add(psp);
                        }
                    }

                }
                else
                {
                    nEOF++;
                    i = 0;
                }
            }
            file.Close();
            

            #region caricamento delle strutture del programma
            for (i = 0; i < n; i++)
            {
                //legge il reader una riga alla volta e ne fa lo split, estrapolando un singolo tile da ogni riga della matrice dei tile
                String[] riga = new String[n + 20];
                riga = structReader[i].Split(';');
                for (int j = 0; j < m; j++)
                {
                    //dopo aver estrapolato tutti gli elementi, questo vettore fa lo split dei singoli dati di ogni elemento
                    String[] elemento = new String[5];
                    elemento = riga[j].Split('|');

                    //istanziazione matrice dei tile
                    structure[i, j].bg = new Point(Convert.ToInt32(elemento[0]), Convert.ToInt32(elemento[1]));
                    structure[i, j].fg = new Point(Convert.ToInt32(elemento[2]), Convert.ToInt32(elemento[3]));
                    if (elemento[4].ToString() == "True ")
                        structure[i, j].steppable = true;
                    else if (elemento[4].ToString() == "False ")
                        structure[i, j].steppable = false;

                    if (Convert.ToInt32(elemento[0]) == -1 && Convert.ToInt32(elemento[1]) == -1)
                    {
                        structure[i, j].bg = new Point(1, 1);
                    }
                    if (Convert.ToInt32(elemento[2]) == -1 && Convert.ToInt32(elemento[3]) == -1)
                    {
                        structure[i, j].fg = new Point(1, 1);
                    }
                }
            }
            #endregion

            #region caricamento del grafo

            foreach (String g1 in waypointReader)
            {
                String[] element = new String[2];
                element = g1.Split('|');
                Waypoint w = new Waypoint(new Circle(Convert.ToInt32(element[0]), Convert.ToInt32(element[1]),16));
                wGraph.AddVertex(w);
            }

            foreach (String g2 in edgesReader)
            {
                String[] element = new String[4];
                element = g2.Split('|');
                Waypoint start = new Waypoint(new Circle(Convert.ToInt32(element[0])-16, Convert.ToInt32(element[1])-16, 16));
                Waypoint end = new Waypoint(new Circle(Convert.ToInt32(element[2])-16, Convert.ToInt32(element[3])-16, 16));
                int indexStart = -1;
                int indexEnd = -1;
                for (i = 0; i < wGraph.Vertices.Count; i++)
                {
                    if (start.c.Center.X == wGraph.Vertices[i].c.Center.X && start.c.Center.Y == wGraph.Vertices[i].c.Center.Y)
                    {
                        indexStart = i;
                    }
                    else if (end.c.Center.X == wGraph.Vertices[i].c.Center.X && end.c.Center.Y == wGraph.Vertices[i].c.Center.Y)
                    {
                        indexEnd = i;
                    }
                }
                Edge e = new Edge(indexStart,indexEnd, Vector2.Distance(wGraph.Vertices[indexStart].c.Center,wGraph.Vertices[indexEnd].c.Center));
                wGraph.AddEdge(e);
            }

            //correzione rotazione torce
            List<spawningPlace> corrective = new List<spawningPlace>();
            foreach (spawningPlace s in spawn)
            {
                spawningPlace esp = s;
                if (esp.type == (int)entityTypes.torch)
                {
                    if (!structure[(int)esp.position.X / 32 - 1, (int)esp.position.Y / 32].steppable)
                    {
                        esp.rotation = 0;
                        esp.position.X -= 20;
                    }
                    else if (!structure[(int)esp.position.X / 32, (int)esp.position.Y / 32 + 1].steppable)
                    {
                        esp.rotation = -(float)MathHelper.Pi / 2;
                        esp.position.Y += 20;
                    }
                    else if (!structure[(int)esp.position.X / 32 + 1, (int)esp.position.Y / 32].steppable)
                    {
                        esp.rotation = (float)MathHelper.Pi;
                        esp.position.X += 20;
                    }
                    else if (!structure[(int)esp.position.X / 32, (int)esp.position.Y / 32 - 1].steppable)
                    {
                        esp.rotation = (float)MathHelper.Pi / 2;
                        esp.position.Y -= 20;
                    }
                    corrective.Add(esp);
                }
                
            }
            for (int c = 0; c < spawn.Count;)
            {
                if (spawn[c].type == (int)entityTypes.torch)
                {
                    spawn.RemoveAt(c);
                    
                }
                else
                    c++;
            }
            foreach (spawningPlace c in corrective)
            {
                spawn.Add(c);
            }
            #endregion

            /*
            foreach (SceneElement s in scenografyReader)
            {
                scenography.Add(s);
            }
            */
            this.lightColor = Color.DarkGray;
        }
            
        
        

        public int AssignDamageId()
        {
            for (i = 0; damageId[i]; i++) ;
            damageId[i] = true;
            return i;
        }

        public bool AddSpawn(spawningPlace s)
        {
            int b = 0;
            foreach (spawningPlace sp in this.spawn)
            {
                if ((Vector2.Distance(sp.position, s.position) < (sp.radius + s.radius)))
                {
                    b++;
                }
            }
            foreach (IEntity e in grid[(int)s.position.X / this.gZsize, (int)s.position.Y / this.gZsize].Entities)
            {
                if ((Vector2.Distance(e.getPosition, s.position) < (e.getBoundingCircle.Radius + s.radius)))
                {
                    b++;
                }
            }
            if (b == 0)
            {
                this.spawn.Add(s);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Controlla che la stanza sia posizionabile all'interno del Dungeon
        /// </summary>
        /// <param name="origin">Angolo superiore sinistro della stanza</param>
        /// <param name="dim">Dimensioni della stanza</param>
        /// <returns>Restituisce true se la stanza rimane entro i limiti del dungeon, false altrimenti</returns>
        public bool CheckRoomDimensions(Point origin, Point dim)
        {
            if (origin.X >= this.width - 4 || origin.Y >= this.height - 4)
            {
                return false;
            }
            else
            {
                if (origin.X + dim.X >= width - 1 || origin.Y + dim.Y >= height - 1)
                {
                    return false;
                }
                else return true;
            }
        }

        /// <summary>
        /// Chiude il Dungeon.
        /// </summary>
        public void Close()
        {
            this.open = false;
        }

        /// <summary>
        /// Disegna su schermo la struttura del Dungeon, la scenografia e le entità scenografiche.
        /// </summary>
        /// <param name="Globals.spriteBatch">Globals.spriteBatch.</param>
        /// <param name="tileSet">TileSet di riferimento.</param>
        /// <param name="scene">Spritesheet contenente elementi scenografici.</param>
        /// <param name="camera">Rettangolo corrispondente alla visuale selezionata, come da Settings.</param>
        public void Draw(ref TileSet tileSet, ref SceneSet[] scene, Rectangle camera)
        {
            position.X = 0 - camera.X % 32;
            position.Y = 0 - camera.Y % 32;
            //tile.X=structure[camera.X/32,camera.Y/32].x1;
            //tile.Y=structure[camera.X/32,camera.Y/32].y1;
            for (int i = camera.Left / 32; i <= camera.Right / 32; i++)
            {
                if (i >= 0 && i < width)
                {
                    for (int j = camera.Top / 32; j <= camera.Bottom / 32; j++)
                    {
                        if (j >= 0 && j < height)
                        {
                            tile.X = structure[i, j].bg.X * 32;
                            tile.Y = structure[i, j].bg.Y * 32;

                            Globals.spriteBatch.Draw(tileSet.SourceBitmap, position, tile, Color.White, rotation_angle, sprite_origin, scale_factor, SpriteEffects.None, Depths.floor);

                            tile.X = structure[i, j].fg.X * 32;
                            tile.Y = structure[i, j].fg.Y * 32;

                            Globals.spriteBatch.Draw(tileSet.SourceBitmap, position, tile, Color.White, rotation_angle, sprite_origin, scale_factor, SpriteEffects.None, Depths.walls);
                        }
                        position.Y += 32;
                    }
                }
                position.Y = 0 - camera.Y % 32;
                position.X += 32;
                /*
                foreach (Waypoint w in wGraph.Vertices)
                {
                    w.Draw(camera);
                }
                */
                //sembra essere pesantissimo
                //Globals.spriteBatch.Draw(Globals.Box, backgroundHue, null, Color.Black, 0, new Vector2(0), SpriteEffects.None, 1f);

            }
            foreach (SceneElement e in scenography)
            {
                Rectangle destinationRectangle = e.getBorder;
                destinationRectangle.X = (int)e.getVPosition.X-camera.X;
                destinationRectangle.Y = (int)e.getVPosition.Y-camera.Y;
                Globals.spriteBatch.Draw(scene[this.type].SourceBitmap,destinationRectangle, scene[this.type].GetBounds(e.getNumber), e.getColor, e.getRotationAngle, scene[this.type].GetRotationCenter(e.getNumber), SpriteEffects.None, Depths.scenography);
            }
        }

        public void FreeDamageId(int index)
        {
            if (index == -1)
                return;
            else
                damageId[index] = false;
        }

        public void FreeId(int index)
        {
            id[index] = false;
        }

        public Vector2 GetInitialPosition(int origin)
        {
            Random r = new Random();
            Vector2 pos = new Vector2();
            Vector2 offset = new Vector2(r.Next(-100, +100), 110);//r.Next(0,2)-1, r.Next(0,2)-1);
            //Il segno di "origin" indica se occorre cercare fra le entrate o fra le uscite:
            //un intero negativo indica che il giocatore sta salendo, e dunque si affaccia da una delle USCITE
            //un intero positivo indica che il giocatore sta scendendo, e dunque si affaccia da una delle ENTRATE
            if (origin > 0)
            {
                pos = entryPassage + offset;
                //pos.X = entries.ElementAt(origin - 1).position.X*32;
                //pos.Y = entries.ElementAt(origin - 1).position.Y*32;
            }
            if (origin < 0)
            {
                pos = exitPassage + offset;
                //pos.X = exits.ElementAt(origin*-1 - 1).position.X*32;
                //pos.Y = exits.ElementAt(origin*-1 - 1).position.Y*32;
            }
            return pos;
        }

        /// <summary>
        /// Certifica che un Dungeon generato casualmente può contenere almeno un'entrata e un'uscita.
        /// </summary>
        /// <returns>Restituisce sempre true per i Dungeon notevoli.</returns>
        public bool IsOk()
        {
            int ok = 0;
            if (generated)
            {
                foreach (Room r in roomList)
                {
                    if (r.canHostAPassage())
                    {
                        ok++;
                    }
                }
                if (ok > 1) //da modificare, contato per 2 entrate e 2 uscite
                {
                    return true;
                }
                else return false;
            }
            else    //inserire appositi controlli per i dungeon notevoli
            {
                return true;
            }
        }

        /// <summary>
        /// Apre (o riapre) il Dungeon.
        /// </summary>
        public void Open()
        {
            this.open = true;
        }



        /// <summary>
        /// Salva un Dungeon su file binario.
        /// </summary>
        public void SaveDungeon()
        {
           /* IFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream("Dungeon.bin", FileMode.Create);//, FileAccess.Write, FileShare.Write);
            formatter.Serialize(stream, this);
            stream.Close();*/
        }

        /// <summary>
        /// Registra un'entità contenuta nel Dungeon presso il gestore delle collisioni.
        /// </summary>
        /// <param name="entity">Istanza di una classe che implementa l'interfaccia IEntity.</param>
        public void SubmitEntity(IEntity entity)
        {
            //h e k danno problemi per la divisione
            int h = (int)entity.getPosition.X / gZsize;// + 2;
            int k = (int)entity.getPosition.Y / gZsize;// + 2;
            if (!grid[h, k].Entities.Contains(entity))
            {
                grid[h, k].AddAnEntity(ref entity);
                if (grid[h, k].Area.Contains(entity.getOccupance))
                {
                    return;
                }
                else
                {
                    if (h > 0)
                    {
                        if (k > 0)
                        {
                            if (grid[h - 1, k - 1].Area.Intersects(entity.getOccupance))
                                grid[h - 1, k - 1].AddAnEntity(ref entity);
                        }
                        if (grid[h - 1, k].Area.Intersects(entity.getOccupance))
                            grid[h - 1, k].AddAnEntity(ref entity);
                        if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                        {
                            if (grid[h - 1, k + 1].Area.Intersects(entity.getOccupance))
                                grid[h - 1, k + 1].AddAnEntity(ref entity);
                        }
                    }
                    if (k > 0)
                    {
                        if (grid[h, k - 1].Area.Intersects(entity.getOccupance))
                            grid[h, k - 1].AddAnEntity(ref entity);
                    }
                    if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                    {
                        if (grid[h, k + 1].Area.Intersects(entity.getOccupance))
                            grid[h, k + 1].AddAnEntity(ref entity);
                    }
                    if (h < (int)Math.Ceiling((32f * width) / (float)gZsize) - 1)
                    {
                        if (k > 0)
                        {
                            if (grid[h + 1, k - 1].Area.Intersects(entity.getOccupance))
                                grid[h + 1, k - 1].AddAnEntity(ref entity);
                        }
                        if (grid[h + 1, k].Area.Intersects(entity.getOccupance))
                            grid[h + 1, k].AddAnEntity(ref entity);
                        if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                        {
                            if (grid[h + 1, k + 1].Area.Intersects(entity.getOccupance))
                                grid[h + 1, k + 1].AddAnEntity(ref entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registra un'entità contenuta nel Dungeon presso il gestore delle collisioni.
        /// </summary>
        /// <param name="entity">Istanza di una classe che implementa l'interfaccia IEntity/IMulticell.</param>
        public void SubmitMulticellEntity(IEntity entity)
        {
            Vector2 endA = entity.getBoundingBox.Origin - new Vector2((float)Math.Cos(entity.getBoundingBox.AngleInRadians), (float)Math.Sin(entity.getBoundingBox.AngleInRadians)) * entity.getBoundingBox.HalfWidths.X;
            Vector2 endB = entity.getBoundingBox.Origin + new Vector2((float)Math.Cos(entity.getBoundingBox.AngleInRadians), (float)Math.Sin(entity.getBoundingBox.AngleInRadians)) * entity.getBoundingBox.HalfWidths.X;
            float length = entity.getBoundingBox.HalfWidths.X*2;
            Vector2 d = endB - endA;
            float a = endB.X - endA.X;
            float b = endB.Y - endA.Y;
            if (length != 0)
            {
                a /= length/gZsize;
                b /= length/gZsize;
            }
            float i = endA.X;
            float j = endA.Y;
            while ((endB.X - i) > Math.Abs(a) || Math.Abs(endB.Y - j) > Math.Abs(b))
            {
                int h = (int)i / gZsize;
                int k = (int)j / gZsize;
                if (!grid[h, k].Entities.Contains(entity))
                {
                    grid[h, k].AddAnEntity(ref entity);
                    /*
                    if (grid[h, k].Area.Contains(entity.getOccupance))
                    {
                        continue;
                    }
                    else
                    {
                        if (h > 0) 
                        {
                            if (k > 0)
                            {
                                if (grid[h - 1, k - 1].Area.Intersects(entity.getOccupance) && !grid[h-1,k-1].Entities.Contains(entity)) //aggiunto in seguito onde evitare doppie registrazioni
                                    grid[h - 1, k - 1].AddAnEntity(ref entity);
                            }
                            if (grid[h - 1, k].Area.Intersects(entity.getOccupance) && !grid[h - 1, k - 1].Entities.Contains(entity))
                                grid[h - 1, k].AddAnEntity(ref entity);
                            if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                            {
                                if (grid[h - 1, k + 1].Area.Intersects(entity.getOccupance))
                                    grid[h - 1, k + 1].AddAnEntity(ref entity);
                            }
                        }
                        if (k > 0)
                        {
                            if (grid[h, k - 1].Area.Intersects(entity.getOccupance))
                                grid[h, k - 1].AddAnEntity(ref entity);
                        }
                        if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                        {
                            if (grid[h, k + 1].Area.Intersects(entity.getOccupance))
                                grid[h, k + 1].AddAnEntity(ref entity);
                        }
                        if (h < (int)Math.Ceiling((32f * width) / (float)gZsize) - 1)
                        {
                            if (k > 0)
                            {
                                if (grid[h + 1, k - 1].Area.Intersects(entity.getOccupance))
                                    grid[h + 1, k - 1].AddAnEntity(ref entity);
                            }
                            if (grid[h + 1, k].Area.Intersects(entity.getOccupance))
                                grid[h + 1, k].AddAnEntity(ref entity);
                            if (k < (int)Math.Ceiling((32f * height) / (float)gZsize) - 1)
                            {
                                if (grid[h + 1, k + 1].Area.Intersects(entity.getOccupance))
                                    grid[h + 1, k + 1].AddAnEntity(ref entity);
                            }
                        }
                    }*/
                }
                i += a;
                j += b;
            }
        }

        /// <summary>
        /// ATTENZIONE: metodo non implementato! - Registra un giocatore 
        /// </summary>
        public void SubmitPlayer(int playerNumber)
        {
            if (playerPopulation == 0)
            {
                this.Open();
            }
            playerPopulation++;
            players.Add(playerNumber);
        }

        /// <summary>
        /// /// ATTENZIONE: metodo non implementato! - De-registra un giocatore 
        /// </summary>
        public void UnsubmitPlayer(int playerNumber)
        {
            players.Remove(playerNumber);
            playerPopulation--;
            if (playerPopulation == 0)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Pulisce il gestore di collisioni.
        /// </summary>
        public void Update()
        {
            for (i = 0; i < (int)Math.Ceiling((32f * width) / (float)gZsize); i++)
            {
                for (j = 0; j < (int)Math.Ceiling((32f * height) / (float)gZsize); j++)
                {
                    grid[i, j].CollisionCheck();
                    grid[i, j].Clear();
                }
            }
            
            for (i = 0; i < dRecs.Count; i++)
            {
                shadows[i].Position = new Vector2(dRecs[i].X  + dRecs[i].Width /2 - Globals.camera[0].Left, -(dRecs[i].Y + dRecs[i].Height/2 - Globals.camera[0].Top));
            }
        }

        /// <summary>
        /// Verifica se una data posizione si trova su superficie calpestabile.
        /// </summary>
        /// <param name="position">Posizione per la quale effettuare il controllo.</param>
        /// <returns>True: superficie non calpestabile; False: superficie liberamente calpestabile.</returns>
        public bool WallContact(Vector2 position)
        {
            if (position.X < width * 32 && position.Y < height * 32)
            {
                if (structure[(int)(position.X / 32), (int)(position.Y / 32)].steppable)
                    return false;
                else return true;
            }
            else return true;
        }

        public Vector2 WallContact(Circle boundingCircle, Vector2 direction)
        {
            Vector2 pos;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 4)
            {
                pos = boundingCircle.Center + direction + new Vector2(boundingCircle.Radius * (float)Math.Cos(i), boundingCircle.Radius * (float)Math.Sin(i));
                if (!structure[(int)(pos.X / 32), (int)(pos.Y / 32)].steppable)
                {
                    direction.X += (float)-Math.Cos(i);
                    direction.Y += (float)-Math.Sin(i);
                }
            }
            return direction;
        }

        public bool WallContact(Circle boundingCircle)
        {
            Vector2 pos;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 4)
            {
                pos = boundingCircle.Center + new Vector2(boundingCircle.Radius * (float)Math.Cos(i), boundingCircle.Radius * (float)Math.Sin(i));
                if (!structure[(int)(pos.X / 32), (int)(pos.Y / 32)].steppable)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool WallContact(OBB box)
        {
            if (box.Origin.X < width * 32 && box.Origin.Y < height * 32)
            {
                
                foreach (Rectangle r in recs)
                {
                    if (box.Intersects(r))
                    {
                        return true;
                    }
                }
                return false;
            }
            else return true;
        }

        public int getFloor
        {
            get { return floor; }
        }

        public int getFloorType
        {
            get { return floorType; }
        }

        public int getWallType
        {
            get { return wallType; }
        }

        /// <summary>
        /// Ottiene il nome del Dungeon corrente.
        /// </summary>
        public String getName
        {
            get { return this.name; }
        }

        /// <summary>
        /// ATTENZIONE: non chiaro, forse da cancellare.
        /// </summary>
        public List<spawningPlace> getSpawn
        {
            get { return spawn; }
        }


        /// <summary>
        /// Fornisce una lista di entità da istanziare.
        /// </summary>
        /// <returns>Restituisce una lista di strutture spawningPlace per l'istanziazione.</returns>
        public List<spawningPlace> getSpawnList()
        {
            List<spawningPlace> list = spawn.GetRange(0, spawn.Count);
            spawn.Clear();
            spawning = false;
            return list;

        }

        /// <summary>
        /// Fornisce le posizioni iniziali dei giocatori sottoforma di segnaposto per l'istanziazione.
        /// </summary>
        public List<spawningPlace> getStartingPlaceList
        {
            get { return playerSpawn; }
        }

        /// <summary>
        /// Indica se il Dungeon è marcato come "aperto".
        /// </summary>
        public bool isOpen
        {
            get { return open; }
        }

        /// <summary>
        /// Indica se il Dungeon è stato generato casualmente.
        /// </summary>
        public bool isRandom
        {
            get { return generated; }
        }

        /// <summary>
        /// Indica se il Dungeon sta richiedendo l'istanziazione di nuove entità.
        /// </summary>
        public bool spawnActive
        {
            get { return spawning; }
        }

        public void GraphCorrection()
        {
            foreach (Room r in roomList)
            {
                //crea edge doppi, irrilevante per il risultato ma da ottimizzare
                foreach (int i in r.waypoint)
                {
                    foreach (int j in r.waypoint)
                    {
                        if (i != j)
                        {
                            wGraph.AddEdge(new Edge(i, j, Vector2.Distance(wGraph.getVertex(i).c.Center, wGraph.getVertex(j).c.Center)));
                            //GenerateDBitmap("dMap");
                        }
                    }
                }
            }
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {

        }

        public void DrawDebug(Rectangle rectangle, ref SpriteFont debugFont)
        {
            String s = "Piano " + this.floor.ToString() + "; "; //+ this.entries.Count.ToString() + " entrate, " + this.exits.Count.ToString() + " uscite;";
            s += "dimensioni: " + this.width.ToString() + " x " + this.height.ToString() + "; " + this.nrooms.ToString() + " stanze, tipo " + this.type.ToString();
            foreach (Room r in roomList)
            {

            }
            Globals.spriteBatch.DrawString(debugFont, s, new Vector2(20, 750), Color.Red, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0.0001f);
        }

        /// <summary>
        /// Trova il waypoint più vicino a una posizione data
        /// </summary>
        /// <param name="position">Posizione rispetto alla quale trovare il waypoint più vicino;
        /// se il valore startingNode è true viene restituito sempre il waypoint centrale della stanza corrispondente</param>
        /// <param name="startingNode">True: posizione di partenza; False: destinazione</param>
        /// <returns>Restituisce l'indice del waypoint trovato</returns>
        public int FindNearestWaypoint(Vector2 position)//, bool startingNode)
        {
            //va reimplementato per i dungeon notevoli
            if (!this.generated) return 0;
            int wayp = -1;
            float minimumDistance = 32 * (float)Math.Sqrt(width * width + height * height);
            //float distance;
            foreach (int num in grid[(int)position.X / 128, (int)position.Y / 128].Rooms)
            {
                if (roomList[num].getAbsoluteArea.Contains((int)position.X, (int)position.Y))
                {
                    wayp = roomList[num].centralWP;
                    /*
                    if (startingNode)
                    {
                        wayp = roomList[num].centralWP;
                    }
                    else
                    {
                        for (int i = 0; i < roomList[num].waypoint.Count(); i++)
                        {
                            distance = Vector2.Distance(wGraph.getVertex(roomList[num].waypoint[i]).c.Center, position);
                            if (distance < minimumDistance)
                            {
                                minimumDistance = distance;
                                wayp = roomList[num].waypoint[i];
                            }
                        }
                    }*/
                }
            }
            return wayp;
        }

        /*
        public int FindNearestWaypoint(Vector2 position) 
        {
            //da reimplementare - è loffio scorrere tutto il grafo
            int wayp = -1;
            float minimumDistance = (float)Math.Sqrt(width * width + height * height)*32;
            float distance;
            for (int i = 0; i < wGraph.Vertices.Count; i++)
            {
                distance = Vector2.Distance(wGraph.getVertex(i).c.Center, position);
                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    wayp = i;
                }
            }
            return wayp;
        }
        */


        public List<Circle> FindShortestPath(Vector2 start, Vector2 end)
        {
            //va implementato per i Dungeon notevoli
            if (!generated) return null;
            List<Circle> path;
            path = new List<Circle>();
            int startNode = FindNearestWaypoint(start);//, true);
            int endNode = FindNearestWaypoint(end);//, false);
            int counter = wGraph.Vertices.Count;
            if (startNode == endNode)
            {
                path.Add(new Circle(end, 16));
                return path;  //se il nodo di partenza e il nodo di arrivo sono gli stessi, non si deve calcolare alcun percorso
            }

            Dijkstra d = new Dijkstra(wGraph.BuildAdjacencyMatrix(), startNode);
            if (d.dist[endNode] == double.PositiveInfinity) return null;    //se il nodo destinazione è irraggiungibile, non restituire alcun percorso

            endNode = d.path[endNode];
            while (endNode != startNode && counter > 0)
            {

                counter--;
                path.Add(wGraph.getVertex(endNode).c);

                endNode = d.path[endNode];
                if (endNode == -1)
                {
                    path.Add(new Circle(end, 16));
                    return path;
                }
            }
            //debug 
            /*if (wGraph.getVertex(counter).direction != "center")
                path.Remove(wGraph.getVertex(endNode).c);
            */
            path.Reverse();
            path.Add(new Circle(end, 16));
            return path;
        }

        public void GenerateMap(ref WGraph wGraph, ref List<Room> roomList, List<Room> supportRoomList)
        {
            foreach (Room r in supportRoomList)
            {
                Rectangle projection;   //proizione dell'area di ricerca
                Rectangle intersection; //interesezione fra la stanza candidata al collegamento e la proiezione
                int[] freeSide;         //"orizzonte" di ricerca tile per tile
                int[] linkSide;         //collegamento a una stanza associato a quel tile
                int maxGirth = 0;
                int girth = 0;
                Random randSize = new Random();

                //controllo delle stanze poste più a nord
                projection = new Rectangle(r.getArea.Left, 0, r.Width, r.getArea.Top);  //fascia larga r.width che va dal bordo superiore della stanza al bordo superiore del dungeon
                freeSide = new int[r.Width];
                linkSide = new int[r.Width];
                for (i = 0; i < r.Width; i++)
                {
                    freeSide[i] = -1;    //l'altezza è massima per tutti
                    linkSide[i] = -1;   //-1 indica nessun collegamento
                }
                int max_Y = -1; //ordinata massima (in coordinate tile) della stanza trovata
                bool indirectLink = false;  //flag per evitare collegamenti a corridoi che portano a stanze già collegate con r
                foreach (Room rr in roomList)    //si scorre il vettore di tutte le stanze (corridoi compresi)
                {
                    if (r != rr)    //si evitano i collegamenti di r con se stessa
                    {
                        intersection = Rectangle.Intersect(projection, rr.getArea); //si controlla l'intersezione tra l'area di proiezione (che è una striscia) e l'area della stanza candidata
                        if (projection.Left == rr.getArea.Left && projection.Right == rr.getArea.Right || projection.Contains(rr.getArea)) intersection = rr.getArea;
                        if (!intersection.IsEmpty)
                        {
                            max_Y = intersection.Bottom;
                            if (max_Y >= r.getArea.Top) max_Y = r.getArea.Top; //in tal caso la stanza ispezionata compenetra questa stanza

                            for (i = intersection.Left; i < intersection.Right; i++)    //si scorre il lato nord tile per tile, assegnando a ogni porzione la Y della stanza più vicina
                            {
                                if (freeSide[i - r.getArea.Left] < max_Y)               //se a perpendicolo c'è una stanza più vicina
                                {
                                    freeSide[i - r.getArea.Left] = max_Y;               //assegna la nuova Y della stanza appena rilevata
                                    linkSide[i - r.getArea.Left] = rr.getRoomNumber;    //segna come da collegare la stanza rilevata
                                }
                            }
                        }
                    }
                }
                int roomN = linkSide[0];
                int leftStart = r.getArea.Left;
                bool only_one = true;   //nel caso si percorresse l'intero orizzonte senza incontrare più di una stanza questo booleano rimarrà true
                List<int> linkList;
                int newRoom = roomList.Count;
                for (i = 1; i < r.Width; i++)         //si collegano le stanze tile per tile
                {
                    if (linkSide[i] != roomN || (only_one && i==r.Width-1))
                    {
                        if (roomN >= 0)
                        {
                            if (!r.getLinkedRooms.Contains(roomList[roomN].getRoomNumber))
                            {
                                if (roomList[roomN].Hallway)
                                {
                                    foreach (int num in r.getLinkedRooms)
                                    {
                                        if (roomList[roomN].getLinkedRooms.Contains(num))
                                        {
                                            indirectLink = true;
                                        }
                                    }
                                    if (!indirectLink)
                                    {
                                        if (freeSide[i] - r.getArea.Top > 0)
                                        {
                                            maxGirth = r.getArea.Left + i - leftStart;
                                            girth = randSize.Next(1, maxGirth);
                                            roomList.Add(new Room(newRoom, new Rectangle(leftStart + (maxGirth - girth) / 2, freeSide[i - 1], girth, r.getArea.Top - freeSide[i - 1]), this.type, true));
                                            Waypoint w1 = new Waypoint(new Vector2(leftStart * 32 + maxGirth * 16, r.getArea.Top * 32 + 16), girth * 16, "north", r.getRoomNumber);
                                            Waypoint w2 = new Waypoint(new Vector2(leftStart * 32 + maxGirth * 16, roomList.ElementAt(roomN).getVCenter.Y), girth * 16, "crossing", roomN);
                                            Waypoint wc = new Waypoint(roomList[newRoom].getVCenter, girth * 16, "center", newRoom);
                                            int wn1 = wGraph.AddVertex(w1);
                                            int wn2 = wGraph.AddVertex(w2);
                                            int wnc = wGraph.AddVertex(wc);

                                            roomList[newRoom].linkARoom(roomN);
                                            roomList[newRoom].linkARoom(r.getRoomNumber);
                                            roomList[newRoom].AddWaypoint(wnc, true);
                                            roomList[newRoom].AddWaypoint(wn1, false);
                                            roomList[newRoom].AddWaypoint(wn2, false);

                                            roomList[roomN].linkARoom(newRoom);
                                            roomList[roomN].linkARoom(r.getRoomNumber);
                                            roomList[roomN].AddWaypoint(wn2, false);

                                            r.linkARoom(roomN);
                                            r.linkARoom(newRoom);
                                            r.AddWaypoint(wn1, false);

                                            newRoom++;
                                            only_one = false;   //segnala l'avvenuto collegamento con una stanza
                                        }
                                        else
                                        {
                                            //semplicemente non collegare la stanza
                                        }
                                    }
                                }
                                else
                                {
                                    linkList = new List<int>();
                                    linkList.Add(r.getRoomNumber);
                                    linkList.Add(roomN);
                                    maxGirth = r.getArea.Left + i - leftStart;
                                    girth = randSize.Next(1, maxGirth);
                                    try
                                    {
                                        roomList.Add(new Room(newRoom, new Rectangle(leftStart + (maxGirth - girth) / 2, freeSide[i - 1], girth, r.getArea.Top - freeSide[i - 1]), this.type, true));
                                    }
                                    catch (ArgumentException )
                                    {
                                    }
                                    //genera i Waypoint e li aggiunge al grafo
                                    Waypoint w1 = new Waypoint(new Vector2(roomList.ElementAt(newRoom).getVCenter.X, r.getArea.Top * 32 + 16), girth * 16, "north", r.getRoomNumber);
                                    Waypoint w2 = new Waypoint(new Vector2(roomList.ElementAt(newRoom).getVCenter.X, roomList.ElementAt(roomN).getArea.Bottom * 32 - 16), girth * 16, "south", roomN);
                                    Waypoint wc = new Waypoint(roomList.ElementAt(newRoom).getVCenter, girth * 16, "center", newRoom);
                                    int wn1 = wGraph.AddVertex(w1);
                                    int wn2 = wGraph.AddVertex(w2);
                                    int wnc = wGraph.AddVertex(wc);
                                    //linka le stanze l'una con l'altra, entrambe col corridoio e il corridoio con entrambe
                                    r.linkARoom(roomN);
                                    r.linkARoom(newRoom);
                                    roomList[roomN].linkARoom(r.getRoomNumber);
                                    roomList[roomN].linkARoom(newRoom);
                                    roomList[newRoom].linkARoom(r.getRoomNumber);
                                    roomList[newRoom].linkARoom(roomN);

                                    //segna i waypoint generati nell'elenco del corridoio appena creato
                                    roomList[newRoom].AddWaypoint(wnc, true); //prima il suo waypoint centrale
                                    roomList[newRoom].AddWaypoint(wn1, false);
                                    roomList[newRoom].AddWaypoint(wn2, false);
                                    roomList[roomN].AddWaypoint(wn2, false);
                                    r.AddWaypoint(wn1, false);

                                    newRoom++;  //incrementa il numero di stanze
                                    only_one = false;   //segnala l'avvenuto collegamento con una stanza
                                }
                            }
                            indirectLink = false;
                        }
                        roomN = linkSide[i];
                        leftStart = r.getArea.Left + i;
                    }
                }


                //controllo delle stanze poste più a est
                projection = new Rectangle(r.getArea.Right, r.getArea.Top, this.width - r.getArea.Right + 1, r.getArea.Height); //c'era un +1 accanto a .Right
                freeSide = new int[r.Height];
                linkSide = new int[r.Height];
                for (i = 0; i < r.Height; i++)
                {
                    freeSide[i] = width;    //l'altezza è massima per tutti
                    linkSide[i] = -1;   //-1 indica nessun collegamento
                }
                int min_X = width;
                foreach (Room rr in roomList)    //si scorre il vettore di tutte le stanze (corridoi compresi)
                {
                    if (r != rr)
                    {
                        intersection = Rectangle.Intersect(projection, rr.getArea); //si controlla l'intersezione tra l'area di proiezione (che è una striscia) e l'area della stanza candidata
                        if (projection.Top == rr.getArea.Top && projection.Bottom == rr.getArea.Bottom || projection.Contains(rr.getArea)) intersection = rr.getArea;
                        if (!intersection.IsEmpty)
                        {
                            min_X = intersection.Left;
                            if (min_X <= r.getArea.Right) min_X = r.getArea.Right; //in tal caso la stanza ispezionata compenetra questa stanza

                            for (i = intersection.Top; i < intersection.Bottom; i++)    //si scorre il lato nord tile per tile, assegnando a ogni porzione la Y della stanza più vicina
                            {
                                if (freeSide[i - r.getArea.Top] > min_X)               //se a perpendicolo c'è una stanza più vicina
                                {
                                    freeSide[i - r.getArea.Top] = min_X;               //assegna la nuova Y della stanza appena rilevata
                                    linkSide[i - r.getArea.Top] = rr.getRoomNumber;    //segna come da collegare la stanza rilevata
                                }
                            }
                        }
                    }
                }
                roomN = linkSide[0];
                int topStart = r.getArea.Top;
                newRoom = roomList.Count;
                only_one = true;   //nel caso si percorresse l'intero orizzonte senza incontrare più di una stanza questo booleano rimarrà true
                for (i = 1; i < r.Height; i++)         //si collegano le stanze tile per tile
                {
                    if (linkSide[i] != roomN || (only_one && i == r.Height-1))
                    {
                        if (roomN >= 0)
                        {
                            if (!r.getLinkedRooms.Contains(roomList.ElementAt(roomN).getRoomNumber))
                            {
                                if (roomList.ElementAt(roomN).Hallway)
                                {
                                    foreach (int num in r.getLinkedRooms)
                                    {
                                        if (roomList.ElementAt(roomN).getLinkedRooms.Contains(num))
                                        {
                                            indirectLink = true;
                                        }
                                    }
                                    if (!indirectLink)
                                    {
                                        if (freeSide[i] - r.getArea.Right > 0)
                                        {
                                            maxGirth = r.getArea.Top + i - topStart;
                                            girth = randSize.Next(1, maxGirth);
                                            roomList.Add(new Room(newRoom, new Rectangle(r.getArea.Right, topStart + (maxGirth - girth) / 2, freeSide[i - 1] - r.getArea.Right, girth), this.type, true));
                                            Waypoint w1 = new Waypoint(new Vector2(r.getArea.Right * 32 - 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "east", r.getRoomNumber);
                                            Waypoint w2 = new Waypoint(new Vector2(roomList[roomN].getVCenter.X, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "crossing", roomN);
                                            Waypoint wc = new Waypoint(roomList[newRoom].getVCenter, girth * 16, "center", newRoom);
                                            int wn1 = wGraph.AddVertex(w1);
                                            int wn2 = wGraph.AddVertex(w2);
                                            int wnc = wGraph.AddVertex(wc);

                                            roomList[newRoom].linkARoom(roomN);
                                            roomList[newRoom].linkARoom(r.getRoomNumber);
                                            roomList[newRoom].AddWaypoint(wnc, true);
                                            roomList[newRoom].AddWaypoint(wn1, false);
                                            roomList[newRoom].AddWaypoint(wn2, false);

                                            roomList[roomN].linkARoom(newRoom);
                                            roomList[roomN].linkARoom(r.getRoomNumber);
                                            roomList[roomN].AddWaypoint(wn2, false);

                                            r.linkARoom(roomN);
                                            r.linkARoom(newRoom);
                                            r.AddWaypoint(wn1, false);

                                            newRoom++;
                                            only_one = false;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                                else
                                {
                                    linkList = new List<int>();
                                    linkList.Add(r.getRoomNumber);
                                    linkList.Add(roomN);
                                    maxGirth = r.getArea.Top + i - topStart;
                                    girth = randSize.Next(1, maxGirth);
                                    try
                                    {
                                        roomList.Add(new Room(newRoom, new Rectangle(r.getArea.Right, topStart + (maxGirth - girth) / 2, freeSide[i - 1] - r.getArea.Right, girth), this.type, true));
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ArgumentException exception = e;
                                    }
                                    //genera i waypoint e li aggiunge al grafo
                                    Waypoint w1 = new Waypoint(new Vector2(r.getArea.Right * 32 - 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "east", r.getRoomNumber);
                                    Waypoint w2 = new Waypoint(new Vector2(roomList.ElementAt(roomN).getArea.Left * 32 + 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "west", roomN);
                                    Waypoint wc = new Waypoint(roomList.ElementAt(newRoom).getVCenter, girth * 16, "center", newRoom);
                                    int wnc = wGraph.AddVertex(wc);
                                    int wn1 = wGraph.AddVertex(w1);
                                    int wn2 = wGraph.AddVertex(w2);
                                    //linka le stanze l'una con l'altra, entrambe col corridoio e il corridoio con entrambe
                                    r.linkARoom(roomN);
                                    r.linkARoom(newRoom);
                                    roomList.ElementAt(newRoom).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(newRoom).linkARoom(roomN);
                                    roomList.ElementAt(roomN).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(roomN).linkARoom(newRoom);

                                    //ssegna i waypoint aggiunti nel corridoio appena generato
                                    roomList.ElementAt(newRoom).AddWaypoint(wnc, true);  //prima il suo waypoint centrale
                                    roomList.ElementAt(newRoom).AddWaypoint(wn1, false);
                                    roomList.ElementAt(newRoom).AddWaypoint(wn2, false);
                                    roomList[roomN].AddWaypoint(wn2, false);
                                    r.AddWaypoint(wn1, false);

                                    newRoom++;  //incrementa il numero di stanze
                                    only_one = false;
                                }
                            }
                            indirectLink = false;
                        }
                        roomN = linkSide[i];
                        topStart = r.getArea.Top + i;
                    }
                }

                //controllo delle stanze poste più a sud
                projection = new Rectangle(r.getArea.Left, r.getArea.Bottom, r.Width, this.height - r.getArea.Bottom + 1); //c'era un +1 accanto a .Bottom
                freeSide = new int[r.Width];
                linkSide = new int[r.Width];
                
                for (i = 0; i < r.Width; i++)
                {
                    freeSide[i] = height;    //l'altezza è massima per tutti
                    linkSide[i] = -1;   //-1 indica nessun collegamento
                }
                int min_Y = height;
                foreach (Room rr in roomList)    //si scorre il vettore di tutte le stanze (corridoi compresi)
                {
                    if (r != rr)
                    {
                        intersection = Rectangle.Intersect(projection, rr.getArea); //si controlla l'intersezione tra l'area di proiezione (che è una striscia) e l'area della stanza candidata
                        if (projection.Left == rr.getArea.Left && projection.Right == rr.getArea.Right || projection.Contains(rr.getArea)) intersection = rr.getArea;
                        if (!intersection.IsEmpty)
                        {
                            min_Y = intersection.Top;
                            if (min_Y <= r.getArea.Bottom) min_Y = r.getArea.Bottom; //in tal caso la stanza ispezionata compenetra questa stanza

                            for (i = intersection.Left; i < intersection.Right; i++)    //si scorre il lato nord tile per tile, assegnando a ogni porzione la Y della stanza più vicina
                            {
                                if (freeSide[i - r.getArea.Left] > min_Y)               //se a perpendicolo c'è una stanza più vicina
                                {
                                    freeSide[i - r.getArea.Left] = min_Y;               //assegna la nuova Y della stanza appena rilevata
                                    linkSide[i - r.getArea.Left] = rr.getRoomNumber;    //segna come da collegare la stanza rilevata
                                }
                            }
                        }
                    }
                }
                roomN = linkSide[0];
                leftStart = r.getArea.Left;
                newRoom = roomList.Count;
                only_one = true;
                for (i = 1; i < r.Width; i++)         //si collegano le stanze tile per tile
                {
                    if (linkSide[i] != roomN || (only_one && i==r.Width-1))
                    {
                        if (roomN >= 0)
                        {
                            if (!r.getLinkedRooms.Contains(roomList.ElementAt(roomN).getRoomNumber))// && !indirectLink)
                            {
                                if (roomList.ElementAt(roomN).Hallway)
                                {
                                    foreach (int num in r.getLinkedRooms)
                                    {
                                        if (roomList.ElementAt(roomN).getLinkedRooms.Contains(num))
                                        {
                                            indirectLink = true;
                                        }
                                    }
                                    if (!indirectLink)
                                    {
                                        if (r.getArea.Bottom - freeSide[i] > 0)
                                        {
                                            maxGirth = r.getArea.Left + i - leftStart;
                                            girth = randSize.Next(1, maxGirth);
                                            roomList.Add(new Room(newRoom, new Rectangle(leftStart + (maxGirth - girth) / 2, r.getArea.Bottom, girth, freeSide[i - 1] - r.getArea.Bottom), this.type, true));
                                            Waypoint w1 = new Waypoint(new Vector2(leftStart * 32 + maxGirth * 16, r.getArea.Top * 32 + 16), girth * 16, "north", r.getRoomNumber);
                                            Waypoint w2 = new Waypoint(new Vector2(leftStart * 32 + maxGirth * 16, roomList.ElementAt(roomN).getVCenter.Y), girth * 16, "crossing", roomN);
                                            Waypoint wc = new Waypoint(roomList[newRoom].getVCenter, girth * 16, "center", newRoom);
                                            int wn1 = wGraph.AddVertex(w1);
                                            int wn2 = wGraph.AddVertex(w2);
                                            int wnc = wGraph.AddVertex(wc);

                                            roomList[newRoom].linkARoom(roomN);
                                            roomList[newRoom].linkARoom(r.getRoomNumber);
                                            roomList[newRoom].AddWaypoint(wnc, true);
                                            roomList[newRoom].AddWaypoint(wn1, false);
                                            roomList[newRoom].AddWaypoint(wn2, false);

                                            roomList[roomN].linkARoom(newRoom);
                                            roomList[roomN].linkARoom(r.getRoomNumber);
                                            roomList[roomN].AddWaypoint(wn2, false);

                                            r.linkARoom(roomN);
                                            r.linkARoom(newRoom);
                                            r.AddWaypoint(wn1, false);

                                            newRoom++;
                                            only_one = false;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                                else
                                {
                                    linkList = new List<int>();
                                    linkList.Add(r.getRoomNumber);
                                    linkList.Add(roomN);
                                    maxGirth = r.getArea.Left + i - leftStart;
                                    girth = randSize.Next(1, maxGirth);
                                    try
                                    {
                                        roomList.Add(new Room(newRoom, new Rectangle(leftStart + (maxGirth - girth) / 2, r.getArea.Bottom, girth, freeSide[i - 1] - r.getArea.Bottom), this.type, true));
                                    }
                                    catch (ArgumentException )
                                    {
                                    }
                                    //genera i waypoint e li aggiunge al grafo
                                    Waypoint w1 = new Waypoint(new Vector2(roomList.ElementAt(newRoom).getVCenter.X, r.getArea.Bottom * 32 - 16), girth * 16, "south", r.getRoomNumber);
                                    Waypoint w2 = new Waypoint(new Vector2(roomList.ElementAt(newRoom).getVCenter.X, roomList.ElementAt(roomN).getArea.Top * 32 + 16), girth * 16, "north", roomN);
                                    Waypoint wc = new Waypoint(roomList.ElementAt(newRoom).getVCenter, girth * 16, "center", newRoom);
                                    int wn1 = wGraph.AddVertex(w1);
                                    int wn2 = wGraph.AddVertex(w2);
                                    int wnc = wGraph.AddVertex(wc);

                                    //linka le stanze l'una con l'altra, entrambe col corridoio e il corridoio con entrambe
                                    r.linkARoom(roomN);
                                    r.linkARoom(newRoom);

                                    roomList.ElementAt(roomN).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(roomN).linkARoom(newRoom);
                                    roomList.ElementAt(newRoom).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(newRoom).linkARoom(roomN);

                                    roomList.ElementAt(newRoom).AddWaypoint(wnc, true);
                                    roomList.ElementAt(newRoom).AddWaypoint(wn1, false);
                                    roomList.ElementAt(newRoom).AddWaypoint(wn2, false);
                                    roomList[roomN].AddWaypoint(wn2, false);
                                    r.AddWaypoint(wn1, false);

                                    newRoom++;  //incrementa il numero di stanze
                                    only_one = false;
                                }
                            }
                            indirectLink = false;
                        }
                        roomN = linkSide[i];
                        leftStart = r.getArea.Left + i;
                    }
                }

                //controllo delle stanze poste più a ovest
                projection = new Rectangle(0, r.getArea.Top, r.getArea.Left, r.Height);
                freeSide = new int[r.Height];
                linkSide = new int[r.Height];
                for (i = 0; i < r.Height; i++)
                {
                    freeSide[i] = -1;    //l'altezza è massima per tutti
                    linkSide[i] = -1;   //-1 indica nessun collegamento
                }
                int max_X = -1;
                foreach (Room rr in roomList)    //si scorre il vettore di tutte le stanze (corridoi compresi)
                {
                    if (r != rr)
                    {
                        intersection = Rectangle.Intersect(projection, rr.getArea); //si controlla l'intersezione tra l'area di proiezione (che è una striscia) e l'area della stanza candidata
                        if (projection.Top == rr.getArea.Top && projection.Bottom == rr.getArea.Bottom || projection.Contains(rr.getArea)) intersection = rr.getArea;
                        if (!intersection.IsEmpty)
                        {
                            max_X = intersection.Right;
                            if (max_X >= r.getArea.Left) max_X = r.getArea.Left; //in tal caso la stanza ispezionata compenetra questa stanza

                            for (i = intersection.Top; i < intersection.Bottom; i++)    //si scorre il lato nord tile per tile, assegnando a ogni porzione la X della stanza più vicina
                            {
                                if (freeSide[i - r.getArea.Top] < max_X)               //se a perpendicolo c'è una stanza più vicina
                                {
                                    freeSide[i - r.getArea.Top] = max_X;               //assegna la nuova X della stanza appena rilevata
                                    linkSide[i - r.getArea.Top] = rr.getRoomNumber;    //segna come da collegare la stanza rilevata
                                }
                            }
                        }
                    }
                }
                roomN = linkSide[0];
                topStart = r.getArea.Top;
                newRoom = roomList.Count;
                only_one = true;
                for (i = 1; i < r.Height; i++)         //si collegano le stanze tile per tile
                {
                    if (linkSide[i] != roomN || (only_one && i==r.Height-1))
                    {
                        if (roomN >= 0)
                        {
                            if (!r.getLinkedRooms.Contains(roomList.ElementAt(roomN).getRoomNumber))
                            {
                                if (roomList.ElementAt(roomN).Hallway)
                                {
                                    foreach (int num in r.getLinkedRooms)
                                    {
                                        if (roomList.ElementAt(roomN).getLinkedRooms.Contains(num))
                                        {
                                            indirectLink = true;
                                        }
                                    }
                                    if (!indirectLink)
                                    {
                                        if (freeSide[i] - r.getArea.Left > 0)
                                        {
                                            maxGirth = r.getArea.Top + i - topStart;
                                            girth = randSize.Next(1, maxGirth);
                                            try
                                            {
                                                roomList.Add(new Room(newRoom, new Rectangle(freeSide[i - 1], topStart + (maxGirth - girth) / 2, r.getArea.Left - freeSide[i - 1], girth), this.type, true));
                                            }
                                            catch (ArgumentException )
                                            {
                                            }
                                            Waypoint w1 = new Waypoint(new Vector2(r.getArea.Left * 32 + 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "west", r.getRoomNumber);
                                            Waypoint w2 = new Waypoint(new Vector2(roomList[roomN].getVCenter.X, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "crossing", roomN);
                                            Waypoint wc = new Waypoint(roomList[newRoom].getVCenter, girth * 16, "center", newRoom);
                                            int wn1 = wGraph.AddVertex(w1);
                                            int wn2 = wGraph.AddVertex(w2);
                                            int wnc = wGraph.AddVertex(wc);

                                            roomList[newRoom].linkARoom(roomN);
                                            roomList[newRoom].linkARoom(r.getRoomNumber);
                                            roomList[newRoom].AddWaypoint(wnc, true);
                                            roomList[newRoom].AddWaypoint(wn1, false);
                                            roomList[newRoom].AddWaypoint(wn2, false);

                                            roomList[roomN].linkARoom(newRoom);
                                            roomList[roomN].linkARoom(r.getRoomNumber);
                                            roomList[roomN].AddWaypoint(wn2, false);

                                            r.linkARoom(roomN);
                                            r.linkARoom(newRoom);
                                            r.AddWaypoint(wn1, false);

                                            newRoom++;
                                            only_one = false;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                                else
                                {
                                    linkList = new List<int>();
                                    linkList.Add(r.getRoomNumber);
                                    linkList.Add(roomN);
                                    maxGirth = r.getArea.Top + i - topStart;
                                    girth = randSize.Next(1, maxGirth);
                                    try
                                    {
                                        roomList.Add(new Room(newRoom, new Rectangle(freeSide[i - 1], topStart + (maxGirth - girth) / 2, r.getArea.Left - freeSide[i - 1], girth), this.type, true));
                                    }
                                    catch (ArgumentException )
                                    {
                                    }
                                    //linka le stanze l'una con l'altra, entrambe col corridoio e il corridoio con entrambe
                                    Waypoint w1 = new Waypoint(new Vector2(r.getArea.Left * 32 + 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "west", r.getRoomNumber);
                                    Waypoint w2 = new Waypoint(new Vector2(roomList.ElementAt(roomN).getArea.Right * 32 - 16, roomList.ElementAt(newRoom).getVCenter.Y), girth * 16, "east", roomN);
                                    Waypoint wc = new Waypoint(roomList.ElementAt(newRoom).getVCenter, girth * 16, "center", newRoom);

                                    int wn1 = wGraph.AddVertex(w1);
                                    int wn2 = wGraph.AddVertex(w2);
                                    int wnc = wGraph.AddVertex(wc);

                                    r.linkARoom(roomN);
                                    r.linkARoom(newRoom);
                                    roomList.ElementAt(roomN).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(roomN).linkARoom(newRoom);
                                    roomList.ElementAt(newRoom).linkARoom(r.getRoomNumber);
                                    roomList.ElementAt(newRoom).linkARoom(roomN);

                                    roomList.ElementAt(newRoom).AddWaypoint(wnc, true);
                                    roomList.ElementAt(newRoom).AddWaypoint(wn1, false);
                                    roomList.ElementAt(newRoom).AddWaypoint(wn2, false);
                                    roomList[roomN].AddWaypoint(wn2, false);
                                    r.AddWaypoint(wn1, false);

                                    newRoom++;  //incrementa il numero di stanze
                                    only_one = false;
                                }
                            }

                            indirectLink = false;
                        }
                        roomN = linkSide[i];
                        topStart = r.getArea.Top + i;
                    }
                }
            }
        }

        public void GenerateWallMap()
        {
            recs = new List<Rectangle>();
            for (int j = 0; j < height; j++)
            {
                Rectangle r = new Rectangle(0, j, 0, 1);
                for (int i = 0; i < width; i++)
                {
                    if (!structure[i, j].steppable)
                    {
                        r.Width++;
                    }
                    else
                    {
                        
                        recs.Add(r);
                        r = new Rectangle(i+1, j, 0, 1);
                    }
                }
                recs.Add(r);
            }

            recs.RemoveAll(x => x.Width == 0);
            for (int i = 1; i < recs.Count; i++)
            {
                int offset = 0;
                int x = recs[i].X;
                int y = recs[i].Y;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (recs[i].Width == recs[j].Width && recs[i].X == recs[j].X)
                    {
                        if (recs[j].Bottom == recs[i].Top - offset)
                        {
                            offset += recs[j].Height;
                            y = recs[j].Y;
                            recs[j] = new Rectangle(0, 0, 0, 0);
                        }
                    }
                }
                recs[i] = new Rectangle(x, y, recs[i].Width, recs[i].Height + offset);
            }
            recs.RemoveAll(x => x.Width == 0);
        }

        public void GenerateShadowMap()
        {
            dRecs.Clear();
            //istanzia la lista delle ombre
            shadows = new List<Krypton.ShadowHull>();


            foreach (Rectangle r in recs)
            {
                int up = 5;
                int down = 5;
                if (r.Top > 0)
                {
                    for (int i = r.Left; i < r.Right; i++)
                    {
                        if (structure[i, r.Top - 1].steppable)
                            up = -5;
                    }
                }
                else
                {
                    up = 0;
                }
                if (r.Bottom < height)
                {
                    for (int i = r.Left; i < r.Right; i++)
                    {
                        if (structure[i, r.Bottom].steppable)
                            down = -5;
                    }
                }
                else
                {
                    down = 0;
                }
                if (up + down == 0)
                { }

                Vector2 size = new Vector2(r.Width * 32 - 10, r.Height * 32 + up + down);
                Vector2 vec = new Vector2(r.X * 32 + 5, r.Y * 32 - up) + new Vector2(size.X * 0.5f, size.Y * 0.5f);
                dRecs.Add(new Rectangle(r.X * 32 + 5, r.Y * 32 - up, (int)size.X, (int)size.Y));
                shadowVectors.Add(vec);

                Krypton.ShadowHull shad = Krypton.ShadowHull.CreateRectangle(size);
                shad.Position = vec;
                shad.Scale = Vector2.One;
                shad.Angle = 0;
                shad.Visible = true;
                shad.Angle = MathHelper.Pi;
                //shad.Scale = new Vector2(1.5f, 1.5f);
                shadows.Add(shad);
            }
            foreach (Krypton.ShadowHull s in shadows)
            {
                //eventualmente modificare per tipologie d muri
                s.Axis = new VAxis(0, 500);
                //basarsi sull eventuale nuovo game o sul caricamento //debug
                if (floor == 1)
                    s.Visible = true;
                else
                    s.Visible = false;
                Globals.krypton.Hulls.Add(s);
            }
        }

        public void GenerateScenography()
        {
            Random p = new Random();

            switch (this.type)
            {
                default:
                    //accostamento mattonella con intonaco nudo (tileset ordinario)
                    for (i = 0; i < this.width; i++)
                    {
                        for (j = 0; j < this.height; j++)
                        {
                            if (structure[i, j].bg.X == dic.floorTileIndex(floorType)["removed_tile"].X && structure[i, j].bg.Y == dic.floorTileIndex(floorType)["removed_tile"].Y)
                            {
                                scenography.Add(new SceneElement(dic.sceneIndex["green_tile"], new Vector2(i * 32 + (float)p.NextDouble() * 32, j * 32 + (float)p.NextDouble() * 32), (float)(p.NextDouble() * Math.PI * 2), 46, Depths.scenography, Color.White));
                            }
                        }
                    }

                    //generazione macchie di sangue
                    for (i = 0; i < this.width; i++)
                    {
                        for (j = 0; j < this.height; j++)
                        {
                            if (structure[i, j].steppable && p.Next(0, 100) > 98)
                            {
                                //scenography.Add(new SceneElement(dic.sceneIndex["blood_" + String.Format("{0:00}", p.Next(0, 15).ToString())], new Vector2(i * 32 + (float)p.NextDouble() * 32, j * 32 + (float)p.NextDouble() * 32), (float)(p.NextDouble() * Math.PI * 2), 46, Depths.scenography, Color.White));
                                scenography.Add(new SceneElement(p.Next(0, 15), new Vector2(i * 32 + (float)p.NextDouble() * 32, j * 32 + (float)p.NextDouble() * 32), (float)(p.NextDouble() * Math.PI * 2), 46, Depths.scenography, Color.White));
                            }
                        }
                    }

                    //aggiunta sassolini
                    for (i = 0; i < this.width; i++)
                    {
                        for (j = 0; j < this.height; j++)
                        {
                            if (structure[i, j].steppable && p.Next(0, 100) > 98)
                            {
                                scenography.Add(new SceneElement(dic.sceneIndex["little_rock"], new Vector2(i * 32 + (float)p.NextDouble() * 32, j * 32 + (float)p.NextDouble() * 32), 0, 46, Depths.scenography, Color.White));
                            }
                        }
                    }
                    break;
            }
        }

        public void GenerateDBitmap(String bitmapName)
        {
            if (this.floor == 2) //mappatura di debug del secondo piano
            {

                using (System.Drawing.Bitmap debugMap = new System.Drawing.Bitmap(width * 32 + 1000, height * 32 + 1000))
                {


                    System.Drawing.Color col;
                    foreach (Room r in roomList)
                    {

                        if (r.Hallway) col = System.Drawing.Color.Blue;
                        else col = System.Drawing.Color.Orange;
                        for (i = r.getArea.Left; i < r.getArea.Right; i++)
                        {
                            for (j = r.getArea.Top; j < r.getArea.Bottom; j++)
                            {
                                for (int x = 0; x < 32; x++)
                                {
                                    for (int y = 0; y < 32; y++)
                                    {
                                        debugMap.SetPixel(i * 32 + x, j * 32 + y, col);
                                    }
                                }
                            }
                        }
                    }

                    foreach (Zone z in grid)
                    {
                        debugMap.SetPixel(z.Area.Left, z.Area.Top, System.Drawing.Color.Gold);
                        if (z.Rooms.Count > 0)
                        {
                            for (int i = z.Area.Location.X; i < z.Area.Location.X + z.Area.Width; i += 2)
                            {
                                for (int j = z.Area.Location.Y; j < z.Area.Location.Y + z.Area.Height; j += 2)
                                {
                                    debugMap.SetPixel(i, j, System.Drawing.Color.Green);
                                }
                            }
                        }

                    }

                    foreach (Edge t in wGraph.Edges)
                    {
                        bool control = false;
                        float length = Vector2.Distance(wGraph.getVertex(t.to).c.Center, wGraph.getVertex(t.from).c.Center);
                        Vector2 d = wGraph.getVertex(t.to).c.Center - wGraph.getVertex(t.from).c.Center;
                        float a = wGraph.getVertex(t.to).c.Center.X - wGraph.getVertex(t.from).c.Center.X;
                        float b = wGraph.getVertex(t.to).c.Center.Y - wGraph.getVertex(t.from).c.Center.Y;
                        //float c = -b / a;
                        if (length != 0)
                        {
                            a /= length;
                            b /= length;
                        }
                        float i = wGraph.getVertex(t.from).c.Center.X;
                        float j = wGraph.getVertex(t.from).c.Center.Y;
                        while (Math.Abs(wGraph.getVertex(t.to).c.Center.X - i) > Math.Abs(a) || Math.Abs(wGraph.getVertex(t.to).c.Center.Y - j) > Math.Abs(b))
                        {
                            control = true;
                            i += a;
                            j += b;
                            debugMap.SetPixel((int)i, (int)j, System.Drawing.Color.Violet);
                        }
                        if (control) whileCounter++;
                    }

                    foreach (Waypoint w in wGraph.Vertices)
                    {
                        debugMap.SetPixel((int)w.c.Center.X, (int)w.c.Center.Y, System.Drawing.Color.Red);
                    }

                    foreach (spawningPlace sp in spawn)
                    {
                        if (sp.type == (int)entityTypes.spiderbot)
                        {
                            debugMap.SetPixel((int)sp.position.X, (int)sp.position.Y, System.Drawing.Color.Black);
                        }
                    }
                    int xxx = 0;
                    int yyy = 50;
                    foreach (Rectangle rec in dRecs)
                    {
                        xxx+=10;
                        yyy += 50;
                        if (yyy >= 250)
                            yyy = 50;
                        if (xxx >= 250)
                            xxx = 10;
                        for (int x = rec.Left; x < rec.Right; x+=2)
                        {
                            for (int y = rec.Top; y < rec.Bottom; y+=2)
                            {
                                
                                //debugMap.SetPixel(i * 32 + x, j * 32 + y, col);
                                debugMap.SetPixel( x, y, System.Drawing.Color.FromArgb(255,xxx,yyy));
                            }
                        }
                     
                    }
                    col = System.Drawing.Color.Black;

                    foreach (Krypton.ShadowHull h in shadows)
                    {
                        debugMap.SetPixel((int)h.Position.X, (int)h.Position.Y,col);
                    }

                    debugMap.Save(bitmapName+".bmp");
                }
            }
            
        }


        /// <summary>
        /// Controlla collisioni fra un dato OBB e l'OBB delle entità registrate in Grid onde rilevarne la presenza.
        /// N.B.: questo metodo è computazionalmente oneroso e non dovrebbe essere chiamato troppo spesso.
        /// </summary>
        /// <param name="sensorBox">L'OBB sensore.</param>
        /// <returns></returns>
        public List<IEntity> Sense(OBB sensorBox)
        {
            List<IEntity> ret = new List<IEntity>();
            Vector2 endA = sensorBox.Origin + new Vector2((float)Math.Cos(sensorBox.AngleInRadians), (float)Math.Sin(sensorBox.AngleInRadians)) * sensorBox.HalfWidths.X;
            Vector2 endB = sensorBox.Origin - new Vector2((float)Math.Cos(sensorBox.AngleInRadians), (float)Math.Sin(sensorBox.AngleInRadians)) * sensorBox.HalfWidths.X;
            float length = sensorBox.HalfWidths.X;//Vector2.Distance(endA, endB);
            Vector2 d = endB - endA;
            float a = endB.X - endA.X;
            float b = endB.Y - endA.Y;
            //float c = -b / a;
            if (length != 0)
            {
                a /= length/gZsize;
                b /= length/gZsize;
            }
            float i = endA.X;
            float j = endA.Y;
            while ((endB.X - i) > Math.Abs(a) || Math.Abs(endB.Y - j) > Math.Abs(b))
            {
                i += a;
                j += b;
                ret=ret.Concat(grid[(int)i/32, (int)j/32].Entities).ToList();
            }
            return ret;
        }
    }
}


