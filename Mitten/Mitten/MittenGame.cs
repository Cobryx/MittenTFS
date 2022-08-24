using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
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
    /// Questo è il tipo principale per il gioco
    /// </summary>
    public class MittenGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;         //dichirazione standard
        Player[] localPlayer;                   //vettore dei giocatori locali
        Player[] externalPlayer;                //vettore dei giocatori remoti
        Dungeon currentDungeon;                 //riferimento al dungeon corrente
        int currentDungeonIndex;                //indice corrente del vettore dei dungeons
        KeyPressed keypressed;
        SceneSet[] sceneSet;          //vettore degli oggetti scenografici
        SpriteSheet[] sheet;                    //vettore degli spritesheet
        SpriteSheet[] handSheet;                //vettore degli oggetti impugnabili
        SpriteSortMode sortMode;                //set di opzioni per il rendering degli sprite, una dichiarazione standard
        SpriteFont mainSpriteFont;
        SpriteFont debugFont;
        TileSet[] dTileSet;                     //vettore dei tileset; notare che mainTileSet è esterno a questo vettore
        TileSet mainTileSet;                    //istanza di un tileset provvisoriamente utilizzata come unica fonte di tile
        Viewport[] screen;                      //vettore di viewport: una viewport in XNA è un "sottoschermo", si utilizza nella modalità splitscreen

        Dungeon[] dungeons = new Dungeon[Globals.singlePlayerDungeons];   //vettore che contiene tutti i livelli della modalità single-player
        SoundEffect[] VSongs;  //vettore delle tracce audio
        SoundEffect[] VSounds; // vettore degli effetti sonori

        List<IEntity>[] entities;                  //vettore delle entità presenti nel gioco, provvisoriamente lasciato come attributo di TheExitGame
        List<IEntity> spawningList = new List<IEntity>();
        List<IEntity> supportSpawningList = new List<IEntity>();
        List<spawningPlace> spawner;            //vettore dei punti di spawn

        SlimDX.DirectInput.DirectInput directInput;
        GameController[] controller;

        Random mRandom = new Random();

        float plaX, plaY, plaN;

        /// <summary>
        /// Questo costruttore istanzia il gioco specificando essenzialmente il solo numero di giocatori, la directory di default e la modalità schermo intero
        /// </summary>
        /// <param name="player_number">Numero dei giocatori locali.</param>
        public MittenGame()
        {
            //istanziazione di krypton
            Globals.krypton = new Krypton.KryptonEngine(this, "KryptonEffect");
            Globals.players = 3;            //dichiarazione del numero di giocatori, successivamente impostabile inGame
            controller = new GameController[Globals.players];
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = Settings.Instance.HorizontalResolution;
            graphics.PreferredBackBufferHeight = Settings.Instance.VerticalResolution;
            if (!Settings.Instance.Windowed)
            {
                graphics.PreferredBackBufferWidth = 1920;
                graphics.PreferredBackBufferHeight = 1080;
                graphics.ToggleFullScreen();
                graphics.IsFullScreen = true;
            }
            this.IsFixedTimeStep = false;
        }

        /// <summary>
        /// Consente al gioco di eseguire tutte le operazioni di inizializzazione necessarie prima di iniziare l'esecuzione.
        /// È possibile richiedere qualunque servizio necessario e caricare eventuali
        /// contenuti non grafici correlati. Quando si chiama base.Initialize, tutti i componenti vengono enumerati
        /// e inizializzati.
        /// </summary>
        protected override void Initialize()
        {
            localPlayer = new Player[Globals.players];
            Globals.InitializeIdArray();

            keypressed = new KeyPressed();

            VSongs = new SoundEffect[Globals.nsongs];
            VSounds = new SoundEffect[Globals.nsounds];

            //inizializzazione di krYpton
            Globals.krypton.Initialize();

            //inizializzazione controller
            directInput = new SlimDX.DirectInput.DirectInput();

            //parte di codice che inizializza viewport e camere.
            #region cameras and screen
            switch (Globals.players)
            {
                //Single player: one camera, one screen region, options for the camera ignored;
                case 1:
                    Globals.camera = new Rectangle[1];
                    screen = new Viewport[1];
                    Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                    screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                    break;
                //Two local players
                case 2:
                    switch (Settings.Instance.cameraOption)
                    {
                        //one fixed camera
                        case 1:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        //splitted camera: vertically splitted if hres is way largher than vres, horizontally splitted otherwise;
                        case 2:
                            Globals.camera = new Rectangle[2];
                            screen = new Viewport[2];
                            if ((Settings.Instance.HorizontalResolution / Settings.Instance.VerticalResolution) >= (16 / 9))
                            {
                                Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution / 2);
                                Globals.camera[1] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution / 2);
                                screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution / 2);
                                screen[1] = new Viewport(0, Settings.Instance.VerticalResolution / 2, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution / 2);
                            }
                            else
                            {
                                Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution);
                                Globals.camera[1] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution);
                                screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution);
                                screen[1] = new Viewport(Settings.Instance.HorizontalResolution / 2, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution);
                            }
                            break;
                        //single camera to be scaled around the center by the distance between the two players
                        case 3:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        default: break;
                    }
                    break;
                //Three local players
                case 3:
                    switch (Settings.Instance.cameraOption)
                    {
                        //one fixed camera
                        case 1:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        //splitted camera
                        case 2:
                            Globals.camera = new Rectangle[3];
                            screen = new Viewport[3];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            Globals.camera[1] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            Globals.camera[2] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            screen[1] = new Viewport(Settings.Instance.HorizontalResolution / 3, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            screen[2] = new Viewport(Settings.Instance.HorizontalResolution * 2 / 3, 0, Settings.Instance.HorizontalResolution / 3, Settings.Instance.VerticalResolution);
                            break;
                        //single camera to be adapted around the triangle made from each player's own position
                        case 3:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        default: break;
                    }
                    break;
                case 4:
                    switch (Settings.Instance.cameraOption)
                    {
                        //one fixed camera
                        case 1:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        //splitted camera
                        case 2:
                            Globals.camera = new Rectangle[4];
                            screen = new Viewport[4];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            Globals.camera[1] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            Globals.camera[2] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            Globals.camera[3] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            screen[1] = new Viewport(Settings.Instance.HorizontalResolution / 2, 0, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            screen[2] = new Viewport(0, Settings.Instance.VerticalResolution / 2, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            screen[3] = new Viewport(Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2, Settings.Instance.HorizontalResolution / 2, Settings.Instance.VerticalResolution / 2);
                            break;
                        //single camera to be adapted around the triangle made from each player's own position
                        case 3:
                            Globals.camera = new Rectangle[1];
                            screen = new Viewport[1];
                            Globals.camera[0] = new Rectangle(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            screen[0] = new Viewport(0, 0, Settings.Instance.HorizontalResolution, Settings.Instance.VerticalResolution);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            #endregion
            sortMode = new SpriteSortMode();        //istanziazione necessaria ma senza significato particolare
            sheet = new SpriteSheet[Globals.nsheets];     //quantità di spritesheet e tileset da utilizzare nel gioco indicata da variabili globali (vedi param)
            handSheet = new SpriteSheet[Globals.nhandleables];
            //interactiveObjectSheet = new SpriteSheet[Globals.ninteractiveObject];
            dTileSet = new TileSet[Globals.singlePlayerDungeons];
            sceneSet = new SceneSet[Globals.nsceneset];

            entities = new List<IEntity>[Globals.singlePlayerDungeons];          //istanziazione del vettore di entità
            for (int i = 0; i < Globals.singlePlayerDungeons; i++)
                entities[i] = new List<IEntity>();

            Globals.IAmanager = new IAManager(ref localPlayer, ref entities, ref dungeons);

            Globals.GUIborder = new Texture2D(this.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Globals.GUIborder.SetData(new[] { Color.White }); 
            base.Initialize();   
        }

        /// <summary>
        /// LoadContent verrà chiamato una volta per gioco e costituisce il punto in cui caricare tutto il contenuto.
        /// </summary>
        protected override void LoadContent()
        {
            Globals.spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.GUIFont = Content.Load<SpriteFont>(@"Font/arial");

            //caricamento del tileset principale da file. 32*32 è la dimensione standard dei tile, 8*24 è il numero di tile del tileset
            mainTileSet = new TileSet(Content.Load<Texture2D>(@"Tiles/tileset"), 32, 32, 8, 34);
            mainSpriteFont = Content.Load<SpriteFont>(@"Font/arial");
            debugFont = Content.Load<SpriteFont>(@"Font/minimal");
            Globals.Box = Content.Load<Texture2D>(@"Animations/box");
            Globals.GUIheart = Content.Load<Texture2D>(@"GUI/heart");
            Globals.GUIinventorybg = Content.Load<Texture2D>(@"GUI/inventory_bg");
            Globals.GUIinventorycase = Content.Load<Texture2D>(@"GUI/inventory_case"); ;
            Globals.GUIinventoryselect = Content.Load<Texture2D>(@"GUI/inventory_select");

            //setup the font, image, video and condition resolvers for the engine
            //the resolvers are simple lambdas that map a string to its corresponding data
            //e.g. an image resolver maps a string to a Texture2D
            var fonts = new Dictionary<string, SpriteFont>();
            Func<string, SpriteFont> fontResolver = f => fonts[f];
            var buttons = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> imageResolver = b => buttons[b];
            var conditions = new Dictionary<string, bool>();
            Func<string, bool> conditionalResolver = c => conditions[c];
            var videos = new Dictionary<string, Video>();
            Func<string, Video> videoResolver = v => videos[v];

            fonts.Add("Debug", Content.Load<SpriteFont>(@"Font/minimal"));
            fonts.Add("Arial", Content.Load<SpriteFont>(@"Font/Arial"));

            buttons.Add("heart", Content.Load<Texture2D>(@"GUI/heart"));
            //buttons.Add("base_background", Content.Load<Texture2D>(@"GUI/"));

            conditions.Add("IsWindows", true);
            conditions.Add("IsXbox", false);

            //videos.Add("Movie", Content.Load<Video>("Movie"));
            Globals.GUIengine = new MarkupTextEngine(fontResolver, imageResolver, conditionalResolver, videoResolver);

            //aggiunta delle tracce audio
            VSongs[(int)songs.environment1] = Content.Load<SoundEffect>(@"Song/environment1");
            VSongs[(int)songs.environment2] = Content.Load<SoundEffect>(@"Song/environment2");
            VSongs[(int)songs.environment3] = Content.Load<SoundEffect>(@"Song/environment3");
            VSongs[(int)songs.environment4] = Content.Load<SoundEffect>(@"Song/environment4");
            //aggiunta degli effetti sonori
            Globals.Frequencies = new SoundManager(VSongs, VSounds);
            //Globals.Frequencies.PlaySong(0);

            sheet[(int)sheetIndexes.human1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/human1"), File.OpenText(@"mmp\human.mmp")); //caricamento spritesheet principale
            sheet[(int)sheetIndexes.human2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/human2"), File.OpenText(@"mmp\human.mmp"));
            sheet[(int)sheetIndexes.human3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/human3"), File.OpenText(@"mmp\human.mmp"));
            sheet[(int)sheetIndexes.human4] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/human4"), File.OpenText(@"mmp\human.mmp"));
            sheet[(int)sheetIndexes.door] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/door"), File.OpenText(@"mmp\door.mmp")); //caricamento spritesheet Porta in sovraimpressione
            sheet[(int)sheetIndexes.torch] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/torch"), File.OpenText(@"mmp\torch.mmp")); //caricamento spritesheet Porta in sovraimpressione
            sheet[(int)sheetIndexes.table] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/table"), File.OpenText(@"mmp\table.mmp")); //caricamento spritesheet table in sovraimpressione
            sheet[(int)sheetIndexes.zombie1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/zombie1"), File.OpenText(@"mmp\zombie.mmp"));
            sheet[(int)sheetIndexes.zombie2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/zombie2"), File.OpenText(@"mmp\zombie.mmp"));
            sheet[(int)sheetIndexes.zombie3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/zombie3"), File.OpenText(@"mmp\zombie.mmp"));
            sheet[(int)sheetIndexes.spiderbot] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/spiderbot"), File.OpenText(@"mmp\spiderbot.mmp"));
            sheet[(int)sheetIndexes.wizard1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard1"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard2"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard3"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard4] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard4"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard5] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard5"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard6] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard6"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard7] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard7"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.wizard8] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/wizard8"), File.OpenText(@"mmp\wizard.mmp"));
            sheet[(int)sheetIndexes.banshee1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/banshee1"), File.OpenText(@"mmp\banshee.mmp"));
            sheet[(int)sheetIndexes.banshee2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/banshee2"), File.OpenText(@"mmp\banshee.mmp"));
            sheet[(int)sheetIndexes.banshee3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/banshee3"), File.OpenText(@"mmp\banshee.mmp"));
            sheet[(int)sheetIndexes.explosion] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/explosion"), File.OpenText(@"mmp\explosion.mmp"));
            sheet[(int)sheetIndexes.item] = new SpriteSheet(Content.Load<Texture2D>(@"Items/items"));//, File.OpenText(@"mmp\item.mmp")); 
            sheet[(int)sheetIndexes.stairs] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/stairs"), File.OpenText(@"mmp\stairs.mmp"));
            sheet[(int)sheetIndexes.magicbolt] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/magicbolt"), File.OpenText(@"mmp\magicbolt.mmp"));
            sheet[(int)sheetIndexes.laser] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/laser"), File.OpenText(@"mmp\laser.mmp"));
            sheet[(int)sheetIndexes.magictrail] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/magictrail"), File.OpenText(@"mmp\magictrail.mmp"));
            sheet[(int)sheetIndexes.GUIheart] = new SpriteSheet(Content.Load<Texture2D>(@"GUI/heart"), File.OpenText(@"mmp\guiheart.mmp")); //caricamento spritesheet della GUI
            sheet[(int)sheetIndexes.orb] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/orb"), File.OpenText(@"mmp\orb.mmp"));
            sheet[(int)sheetIndexes.icewall] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/icewall"), File.OpenText(@"mmp\icewall.mmp"));

            sheet[(int)sheetIndexes.altar] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/altar"), File.OpenText(@"mmp\Scenographic\altar.mmp"));
            sheet[(int)sheetIndexes.altardecoration] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/altardecoration"), File.OpenText(@"mmp\Scenographic\altar.mmp"));
            sheet[(int)sheetIndexes.anvil] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/anvil"), File.OpenText(@"mmp\Scenographic\anvil.mmp"));
            sheet[(int)sheetIndexes.bigbook] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/bigbook"), File.OpenText(@"mmp\Scenographic\bigbook.mmp"));
            sheet[(int)sheetIndexes.column] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/column"), File.OpenText(@"mmp\Scenographic\column.mmp"));
            sheet[(int)sheetIndexes.charredwood] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/charredWood"), File.OpenText(@"mmp\Scenographic\charredwood.mmp"));
            sheet[(int)sheetIndexes.columnbasements1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/columnbasement"), File.OpenText(@"mmp\Scenographic\columnbasements.mmp"));
            sheet[(int)sheetIndexes.columnbasements2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/columnbasement2"), File.OpenText(@"mmp\Scenographic\columnbasements.mmp"));
            sheet[(int)sheetIndexes.columnbasements3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/columnbasement3"), File.OpenText(@"mmp\Scenographic\columnbasements.mmp"));
            sheet[(int)sheetIndexes.gargoyle] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/gargoyle"), File.OpenText(@"mmp\Scenographic\gargoyle.mmp"));
            sheet[(int)sheetIndexes.hammer] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/hammer"), File.OpenText(@"mmp\Scenographic\hammer.mmp"));
            sheet[(int)sheetIndexes.pot1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/pot1"), File.OpenText(@"mmp\Scenographic\pot1.mmp"));
            sheet[(int)sheetIndexes.pot2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/pot2"), File.OpenText(@"mmp\Scenographic\pot2.mmp"));
            sheet[(int)sheetIndexes.throne1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/throne1"), File.OpenText(@"mmp\Scenographic\throne.mmp"));
            sheet[(int)sheetIndexes.throne2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/throne2"), File.OpenText(@"mmp\Scenographic\throne.mmp"));
            sheet[(int)sheetIndexes.bloodfountain] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/bloodfountain"), File.OpenText(@"mmp\Scenographic\bloodfountain.mmp"));
            sheet[(int)sheetIndexes.corpse1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/corpse1"), File.OpenText(@"mmp\Scenographic\corpse1.mmp"));
            sheet[(int)sheetIndexes.corpse2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/corpse2"), File.OpenText(@"mmp\Scenographic\corpse2.mmp"));
            sheet[(int)sheetIndexes.corpse3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/corpse3"), File.OpenText(@"mmp\Scenographic\corpse3.mmp"));
            sheet[(int)sheetIndexes.corpsestack] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/corpsestack"), File.OpenText(@"mmp\Scenographic\corpseStack.mmp"));
            sheet[(int)sheetIndexes.head] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/head"), File.OpenText(@"mmp\Scenographic\head.mmp"));


            sheet[(int)sheetIndexes.manhole1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/manhole1"), File.OpenText(@"mmp\Scenographic\manhole1.mmp"));
            sheet[(int)sheetIndexes.manhole2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/manhole2"), File.OpenText(@"mmp\Scenographic\manhole2.mmp"));
            sheet[(int)sheetIndexes.manhole3] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/manhole3"), File.OpenText(@"mmp\Scenographic\manhole3.mmp"));

            sheet[(int)sheetIndexes.sleepbag1] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/sleepbag1"), File.OpenText(@"mmp\Scenographic\sleepbag1.mmp"));
            sheet[(int)sheetIndexes.sleepbag2] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/Scenographic/sleepbag2"), File.OpenText(@"mmp\Scenographic\sleepbag2.mmp"));

            handSheet[(int)handleables.crossbow] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/crossbow"), File.OpenText(@"mmp\crossbow.mmp")); //caricamento spritesheet arma in sovraimpressione
            handSheet[(int)handleables.mace] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/commonsword"), File.OpenText(@"mmp\commonsword.mmp"));
            handSheet[(int)handleables.nothing] = null;
            handSheet[(int)handleables.sword] = new SpriteSheet(Content.Load<Texture2D>(@"Animations/commonsword"), File.OpenText(@"mmp\commonsword.mmp"));

            dic.loadItemList(File.OpenText(@"items.idf"));
            dic.reverseItemIndex = new Dictionary<String, int>();
            foreach (KeyValuePair<int, ItemInfo> k in dic.itemIndex)
                dic.reverseItemIndex.Add(k.Value.name, k.Key);
            Globals.nItems = dic.itemIndex.Count;
        
            sceneSet[0] = new SceneSet(Content.Load<Texture2D>(@"Scenography/scenography"), File.OpenText(@"mmp\scenography.mmp"));

            // currentDungeon = (Dungeon)formatter.Deserialize(str);
            // currentDungeon = new Dungeon();
            Random r = new Random();

            dungeons[0] = new Dungeon("Room4.bin", 1);
            dungeons[0].GenerateWallMap();
            dungeons[0].GenerateShadowMap();
            //dungeons[0].SaveDungeon();

            for (int i = 1; i < Globals.singlePlayerDungeons; i++)
            {
                dungeons[i] = new Dungeon((int)floorTypes.marble, (int)wallTypes.standard, (int)tileColors.gray, i + 1, 1, Color.DarkGray);
                while (!dungeons[i].IsOk())
                {
                    //if (i == 0) { dungeons[i] = new Dungeon((int)floorTypes.marble, (int)wallTypes.standard, (int)tileColors.gray, i + 1, 1, Color.Gray); }
                    if (i == 1) { dungeons[i] = new Dungeon((int)floorTypes.marble, (int)wallTypes.standard, (int)tileColors.gray, i + 1, 1, Color.DarkGray); }
                    if (i == 2) { dungeons[i] = new Dungeon((int)floorTypes.marble, (int)wallTypes.standard, (int)tileColors.chess, i + 1, 1, Color.DarkGray); }
                    if (i == 3) { dungeons[i] = new Dungeon((int)floorTypes.marble, (int)wallTypes.standard, (int)tileColors.chess, i + 1, 1, Color.DarkGray); }
                }
                //dungeons[i].SaveDungeon();
            }
        
            IFormatter formatter = new BinaryFormatter();
            //Stream str = new FileStream("Dungeon.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            // dungeons[2] = (Dungeon)formatter.Deserialize(str);
            //int d = r.Next(1, Globals.singlePlayerDungeons);    //
            currentDungeon = dungeons[0];
            // str.Close();
            //currentDungeon.SaveDungeon();
            Settings.Instance.save();

            List<spawningPlace> playerStart = currentDungeon.getStartingPlaceList;

            for (int i = 0; i < directInput.GetDevices(SlimDX.DirectInput.DeviceClass.GameController, SlimDX.DirectInput.DeviceEnumerationFlags.AttachedOnly).Count; i++)
            {
                if (i<Globals.players)
                    controller[i] = new GameController(directInput, this, i);
            }
            spawningPlace pl = playerStart.ElementAt(0);
            localPlayer[0] = new Player(pl.position, pl.direction, pl.speed, pl.depth, pl.health, pl.rotation, ref sheet, ref handSheet, ref currentDungeon, 1, "Carl the Evil");
            if (Globals.players > 1)
            {
                pl = playerStart.ElementAt(1);
                localPlayer[1] = new Player(pl.position, pl.direction, pl.speed, pl.depth, pl.health, pl.rotation, ref sheet, ref handSheet, ref currentDungeon, 2, "Keldric Malek");
            }
            if (Globals.players > 2)
            {
                pl = playerStart.ElementAt(2);
                localPlayer[2] = new Player(pl.position, pl.direction, pl.speed, pl.depth, pl.health, pl.rotation, ref sheet, ref handSheet, ref currentDungeon, 3, "Lord of Shadows", controller[0]);
            }
            if (Globals.players > 3)
            {
                pl = playerStart.ElementAt(3);
                localPlayer[3] = new Player(pl.position, pl.direction, pl.speed, pl.depth, pl.health, pl.rotation, ref sheet, ref handSheet, ref currentDungeon, 4, "Liseah", controller[1]);
            }
            for (int i = 0; i < Globals.players; i++)
            {
                if (Settings.Instance.cameraOption == 2)
                    localPlayer[i].CalculateGUI(screen[i]);
                else localPlayer[i].CalculateGUI(screen[0]);
            }

            Globals.mLightTexture = Krypton.LightTextureBuilder.CreatePointLight(this.GraphicsDevice, 512);
            Globals.krypton.AmbientColor = new Color(50,50,50);
            Globals.krypton.Bluriness = 3;
        }

        /// <summary>
        /// UnloadContent verrà chiamato una volta per gioco e costituisce il punto in cui scaricare
        /// tutto il contenuto.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: scaricare qui tutto il contenuto non gestito da ContentManager
            for (int i = 0; i < Globals.players-2; i++)
            {
                //controller[i].Release();
            }
            directInput.Dispose();

            foreach (GameController gc in controller.Where(x => x != null))
            {
                gc.Release();
            }
        }

        /// <summary>
        /// Consente al gioco di eseguire la logica per, ad esempio, aggiornare il mondo,controllare l'esistenza di conflitti, raccogliere l'input e riprodurre l'audio.
        /// </summary>
        /// <param name="gameTime">Fornisce uno snapshot dei valori di temporizzazione.</param>
        protected override void Update(GameTime gameTime)
        {
            #region tasti generici

            if (Keyboard.GetState().IsKeyDown(Keys.F10) && !keypressed.GuiYetPressed)
            {
                Globals.showGUI = !Globals.showGUI;
                keypressed.GuiYetPressed = true;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.F10) && keypressed.GuiYetPressed)
                keypressed.GuiYetPressed = false;
            #endregion

            if (currentDungeon.isOpen)
            { }

            if (Settings.Instance.cameraOption == 1)
            {
                int dest = localPlayer[0].getDestination;
                bool proceed = true;

                for (int i = 1; i < Globals.players; i++)
                {
                    if (dest == localPlayer[i].getDestination && proceed)
                    {
                        proceed = true;
                    }
                    else
                    {
                        proceed = false;
                    }
                }

                if (proceed && dest != 0)
                {
                    //spegnimento ombre e luci del livello corrente
                    foreach (IEntity e in entities[currentDungeonIndex])
                    {
                        if (e is Krypton.Lights.ILight2D)
                            ((Krypton.Lights.ILight2D)e).IsOn = false;
                        else if (e is ILightEntity)
                            ((ILightEntity)e).Light.IsOn = false;
                        else if (e is IShadow)
                            ((IShadow)e).Shadow.Visible = false;
                    }

                    foreach (Krypton.ShadowHull s in dungeons[currentDungeonIndex].shadows)
                        s.Visible = false;

                    if (dest < 0)
                    {
                        currentDungeonIndex = currentDungeon.getFloor - 2;
                        currentDungeon = dungeons[currentDungeonIndex]; //floor è il numero del piano, che parte da 1, pertanto corrisponde all'indirizzo logico del piano successivo
                    }
                    else
                    {
                        currentDungeonIndex = currentDungeon.getFloor;
                        currentDungeon = dungeons[currentDungeonIndex]; //floor è il numero del piano, che parte da 1, pertanto corrisponde all'indirizzo logico del piano successivo
                    }

                    foreach (IEntity e in entities[currentDungeonIndex])
                    {
                        if (e is Krypton.Lights.ILight2D)
                            ((Krypton.Lights.ILight2D)e).IsOn = true;
                        else if (e is ILightEntity)
                            ((ILightEntity)e).Light.IsOn = true;
                        else if (e is IShadow && e.Updatable)
                            ((IShadow)e).Shadow.Visible = true;
                    }

                    foreach (Krypton.ShadowHull s in dungeons[currentDungeonIndex].shadows)
                        s.Visible = true;

                    Globals.krypton.AmbientColor = currentDungeon.lightColor;

                    foreach (Player p in localPlayer)
                        p.Reset(ref currentDungeon);
                }
            }

            if (currentDungeon.spawnActive)
            {
                Random r = new Random();
                spawner = currentDungeon.getSpawnList();
                foreach (spawningPlace spl in spawner)
                {
                    IEntity e;
                    switch (spl.type)
                    {
                        case (int)entityTypes.human:
                            e = new Human(spl.position, spl.radius, spl.direction, spl.speed, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref handSheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.zombie:
                            e = new Zombie(spl.position, spl.radius, spl.direction, spl.speed, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.spiderbot:
                            e = new Spiderbot(spl.position, spl.radius, spl.direction, spl.speed, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.door:
                            e = new Door(spl.position, spl.radius, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.torch:
                            e = new Torch(spl.position, spl.radius, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon, (float)r.NextDouble() * 3f, (float)r.NextDouble() * 10 + 5f);  //ridefinizione in base alle viewport
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.table:
                            e = new Table(spl.position, spl.radius, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.explosion:
                            e = new Explosion(spl.position, spl.radius, spl.depth, spl.health, spl.subtype, 0, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.wizard:
                            e = new Wizard(spl.position, spl.radius, spl.direction, spl.speed, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;

                        case (int)entityTypes.banshee:
                            e = new Banshee(spl.position, spl.radius, spl.direction, spl.speed, spl.depth, spl.health, spl.rotation, spl.type, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;

                        case (int)entityTypes.item:
                            e = new Item(dic.itemIndex[spl.subtype], spl.position, spl.direction, spl.depth, spl.rotation, ref sheet, ref currentDungeon, false); //quantity è attualmente omesso
                            entities[currentDungeonIndex].Add(e);
                            break;

                        case (int)entityTypes.stairs:
                            bool exit;
                            if (spl.name.Contains("exit"))
                                exit = true;
                            else exit = false;
                            e = new Stairs(exit, currentDungeon.getFloorType, spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.altar:
                            e = new Altar(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.anvil:
                            e = new Anvil(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.bigbook:
                            e = new Bigbook(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.column:
                            e = new Column(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.charredwood:
                            e = new Charredwood(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.columnbasement1:
                            e = new Columnbasements(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.columnbasement2:
                            e = new Columnbasements(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.columnbasement3:
                            e = new Columnbasements(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.gargoyle:
                            e = new Gargoyle(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.pot1:
                           // e = new Pot(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                           // entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.pot2:
                           // e = new Pot(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                           // entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.throne1:
                            e = new Throne(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.throne2:
                            e = new Throne(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.bloodfountain:
                            e = new BloodFountain(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.corpse1:
                            e = new Corpse(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.corpse2:
                            e = new Corpse(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.corpse3:
                            e = new Corpse(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.head:
                            e = new Head(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.corpsestack:
                            e = new CorpseStack(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.manhole1:
                            e = new Manhole(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.manhole2:
                            e = new Manhole(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.manhole3:
                            e = new Manhole(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.sleepbag1:
                            e = new SleepBag(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                        case (int)entityTypes.sleepbag2:
                            e = new SleepBag(spl.position, spl.rotation, spl.type, spl.subtype, ref sheet, ref currentDungeon);
                            entities[currentDungeonIndex].Add(e);
                            break;
                    }
                }
            }

            for (int i = 0; i < localPlayer.Length; i++)
            {
                currentDungeon.SubmitEntity(localPlayer[i]);
                spawningList = localPlayer[i].GetSpawningList();
                foreach (IEntity e2 in spawningList)
                    supportSpawningList.Add(e2);
            }

            foreach (IEntity e in entities[currentDungeonIndex])
            {
                if (e is IMulticell)
                    currentDungeon.SubmitMulticellEntity(e);
                else
                    currentDungeon.SubmitEntity(e);
                spawningList = e.GetSpawningList();
                

                foreach (IEntity e2 in spawningList)
                    supportSpawningList.Add(e2);
            }
            foreach (IEntity e in supportSpawningList)
                entities[currentDungeonIndex].Add(e);
            supportSpawningList.Clear();
            if (Globals.AIActivity)
                Globals.IAmanager.Update(gameTime, currentDungeonIndex);
            currentDungeon.Update();

            for (int i = 0; i < localPlayer.Length; i++)
            {
                localPlayer[i].Shadow.Position = new Vector2(Globals.camera[0].Width / 2, -Globals.camera[0].Height / 2);
                localPlayer[i].Update(gameTime);
            }

            foreach (IEntity e in entities[currentDungeonIndex])
            {
                if (e.Updatable)
                {
                    e.Update(gameTime);
                  
                }
                else
                {
                    if (e is IShadow)
                        Globals.krypton.Hulls.Remove(((IShadow)e).Shadow);
                    else if (e is ILightEntity && ((ILightEntity)e).Light != null)
                        Globals.krypton.Lights[Globals.krypton.Lights.IndexOf(((ILightEntity)e).Light)].IsOn = false;
                    Globals.FreeId(e.getId);
                }
            }
            entities[currentDungeonIndex].RemoveAll(x => x.Updatable == false); //debug, da rivedere

            base.Update(gameTime);
        }

        /// <summary>
        /// Viene chiamato quando il gioco deve disegnarsi.
        /// </summary>
        /// <param name="gameTime">Fornisce uno snapshot dei valori di temporizzazione.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Create a world view projection matrix to use with krypton
            Matrix world = Matrix.Identity;
            //Matrix view = Matrix.CreateTranslation(new Vector3(0, 0, 0) * -1f);
            //Matrix projection = Matrix.CreateOrthographic(this.mVerticalUnits * this.GraphicsDevice.Viewport.AspectRatio, this.mVerticalUnits, 0, 1);
            Matrix view = Matrix.CreateTranslation(new Vector3(graphics.PreferredBackBufferWidth / 2, -graphics.PreferredBackBufferHeight / 2, 0) * -1f);
            Matrix projection = Matrix.CreateOrthographic(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, 0, 1);

            // Assign the matrix and pre-render the lightmap.
            // Make sure not to change the position of any lights or shadow hulls after this call, as it won't take effect till the next frame!
            Globals.krypton.Matrix = world * view * projection;
            Globals.krypton.Bluriness = 3;
            Globals.krypton.LightMapPrepare();
            GraphicsDevice.Clear(Color.Black); //debug potrebbe non essere necessario, doppia cancellatura

            // Make sure we clear the backbuffer *after* Krypton is done pre-rendering

            // ----- DRAW STUFF HERE ----- //
            // By drawing here, you ensure that your scene is properly lit by krypton.
            // Drawing after KryptonEngine.Draw will cause you objects to be drawn on top of the lightmap (can be useful, fyi)
            // ----- DRAW STUFF HERE ----- //

            // Draw krypton (This can be omited if krypton is in the Component list. It will simply draw krypton when base.Draw is called

            if (Settings.Instance.camera_option == 1)
            {
                
                Globals.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateScale(1.0f));
                graphics.GraphicsDevice.Viewport = screen[0];
                Globals.camera[0] = localPlayer[0].GetCamera(Globals.camera[0]);

                plaX = plaY = plaN = 0;
                foreach (Player p in localPlayer.Where(x => x.ActivePlayer))
                {
                    plaX += p.getPosition.X;
                    plaY += p.getPosition.Y;
                    plaN++;
                }
                Vector2 cameraVec = new Vector2(plaX / plaN, plaY / plaN);
                float minDis = float.PositiveInfinity;
                foreach (Player p in localPlayer.Where(x => x.ActivePlayer))
                {
                    if (Vector2.Distance(cameraVec, p.getPosition) < minDis)
                        minDis = Vector2.Distance(cameraVec, p.getPosition);
                }

                currentDungeon.Draw(ref mainTileSet, ref sceneSet, Globals.camera[0]);
                if (Globals.dMode) { currentDungeon.DrawCollidedObjectDebug(Globals.camera[0]); };
                if (Globals.advancedDMode) { currentDungeon.DrawDebug(Globals.camera[0], ref debugFont); }
                Globals.spriteBatch.End();
                Globals.krypton.Draw(gameTime);
                Globals.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateScale(1.0f));
                foreach (Player p in localPlayer)
                {
                    p.draw(Globals.camera[0]);
                    if (Globals.showGUI)
                        p.drawGUI(screen[0]);
                    if (Globals.dMode)  p.DrawCollidedObjectDebug(Globals.camera[0]); ;
                    if (Globals.advancedDMode)  p.DrawDebug(Globals.camera[0], ref debugFont); 
                }

                foreach (IEntity e in entities[currentDungeonIndex])
                {
                    //if (e.Is_in_camera(camera[0]))
                    e.Draw(Globals.camera[0]);
                    if (Globals.dMode)  e.DrawCollidedObjectDebug(Globals.camera[0]); 
                    if (Globals.advancedDMode)  e.DrawDebug(Globals.camera[0], ref debugFont); 
                }
                Globals.spriteBatch.End();
                

            }
            if (Settings.Instance.camera_option == 2)
            {
                for (int i = 0; i < Globals.camera.Length; i++)
                {
                    Globals.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateScale(1.0f));
                    graphics.GraphicsDevice.Viewport = screen[i];
                    Globals.camera[i] = localPlayer[i].GetCamera(Globals.camera[i]);
                    currentDungeon.Draw(ref mainTileSet, ref sceneSet, Globals.camera[i]);
                    if (Globals.dMode) { currentDungeon.DrawCollidedObjectDebug(Globals.camera[i]); }
                    if (Globals.advancedDMode) { currentDungeon.DrawDebug(Globals.camera[i], ref debugFont); }

                    foreach (Player p in localPlayer)
                    {
                        p.draw(Globals.camera[i]);
                        if (Globals.dMode) { p.DrawCollidedObjectDebug(Globals.camera[i]); };
                        if (Globals.advancedDMode) { p.DrawDebug(Globals.camera[i], ref debugFont); }
                    }
                    localPlayer[i].drawGUI(screen[0]);
                    if (Globals.dMode) { localPlayer[i].DrawCollidedObjectDebug(Globals.camera[i]); };
                    if (Globals.advancedDMode) { localPlayer[i].DrawDebug(Globals.camera[i], ref debugFont); }
                    //space for external player to fill in a later time
                    foreach (IEntity e in entities[currentDungeonIndex])
                    {
                        if (e.Is_in_camera(Globals.camera[i]))
                        {
                            e.Draw(Globals.camera[i]);
                            if (Globals.dMode) { e.DrawCollidedObjectDebug(Globals.camera[i]); };
                            if (Globals.advancedDMode) { e.DrawDebug(Globals.camera[i], ref debugFont); }
                        }
                    }
                    Globals.spriteBatch.End();
                }
            }
            if (Settings.Instance.camera_option == 3)
            {
                Globals.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateScale(1.0f));
                graphics.GraphicsDevice.Viewport = screen[0];
                //la camera è centrata sul baricentro delle posizioni dei giocatori
                //minX = maxX = minY = maxY = 0;
                /*foreach (Player p in localPlayer.Where(x => x.ActivePlayer))
                {
                    if (p.getPosition.X < minX)
                        minX = p.getPosition.X;
                    if (p.getPosition.X > maxX)
                        maxX = p.getPosition.X;
                    if (p.getPosition.Y < minY)
                        minY = p.getPosition.Y;
                    if (p.getPosition.Y > maxY)
                        maxY = p.getPosition.Y;
                }*/

                //centra la camera sul baricentro delle posizioni dei giocatori attivi
                plaX = plaY = plaN = 0;
                foreach (Player p in localPlayer.Where(x => x.ActivePlayer))
                {
                    plaX += p.getPosition.X;
                    plaY += p.getPosition.Y;
                    plaN++;
                }

                Globals.camera[0] = new Rectangle((int)(plaX / plaN) - Globals.camera[0].Width / 2, (int)(plaY / plaN) - Globals.camera[0].Height / 2, Globals.camera[0].Width, Globals.camera[0].Height);

                //Globals.camera[0] = new Rectangle((int)(maxX + minX - Globals.camera[0].Width)/2, (int)(maxY + minY - Globals.camera[0].Height)/2, Globals.camera[0].Width, Globals.camera[0].Height);
                //Globals.camera[0] = new Rectangle((int)(maxX + minX) / 2, (int)(maxY + minY) / 2, Globals.camera[0].Width, Globals.camera[0].Height);

                currentDungeon.Draw(ref mainTileSet, ref sceneSet, Globals.camera[0]);
                if (Globals.dMode) { currentDungeon.DrawCollidedObjectDebug(Globals.camera[0]); };
                if (Globals.advancedDMode) { currentDungeon.DrawDebug(Globals.camera[0], ref debugFont); }

                foreach (IEntity e in entities[currentDungeonIndex])
                {
                    //if (e.Is_in_camera(camera[0]))
                    e.Draw(Globals.camera[0]);
                    if (Globals.dMode) { e.DrawCollidedObjectDebug(Globals.camera[0]); }
                    if (Globals.advancedDMode) { e.DrawDebug(Globals.camera[0], ref debugFont); }
                }
                Globals.spriteBatch.End();
                Globals.krypton.Draw(gameTime);
                foreach (Player p in localPlayer)
                {
                    Globals.spriteBatch.Begin();
                    p.draw(Globals.camera[0]);
                    p.drawGUI(screen[0]);
                    if (Globals.dMode) { p.DrawCollidedObjectDebug(Globals.camera[0]); };
                    if (Globals.advancedDMode) { p.DrawDebug(Globals.camera[0], ref debugFont); }
                    Globals.spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        public void spawnEntity(IEntity e)
        {
            entities[currentDungeonIndex].Add(e);
        }
    }
}

