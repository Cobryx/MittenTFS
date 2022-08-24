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
    public class Blaze : IEntity,IAttacker
    {
        bool existence;
        bool updatable = true;
        Circle boundingCircle;
        Color ent_color;
        Damage damage;
        DamageData damageData;
        Dungeon currentDungeon;
        int currentAnimation;
        int factionId;
        int id;
        protected int sheetIndex;
        int state;
        int subtype;
        int type;
        float depth;
        Vector2 length;
        Vector2[] boxDistance;
        float rotationAngle;
        float speed;
        float thickness = 10;
        ICaster maker;
        OBB boundingBox;
        protected OBB[] boxList;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 initialPosition;
        Vector2 oldPosition;
        Vector2 origin;
        Vector2 scale;

        List<int> cIds;
        List<Collision> cData;
        List<DamageData> dData;
        List<IEntity> spawned;
        VAxis axis;

        public Blaze(Color color, int factionId, int subtype, int type, float rotationAngle, ref Dungeon currentDungeon, ref SpriteSheet[] sheet, Vector2 position, ICaster maker)
        {
            this.maker = maker;
            axis = new VAxis(0, 192);   //le fiammate dovrebbero essere alte...
            sheetIndex = (int)sheetIndexes.blaze;
            boundingBox = new OBB(position, rotationAngle, new Vector2(100, thickness));
            //boundingBox.DebugColor = Color.Black;
            this.direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));

            //boxList[3] = new OBB(position, rotationAngle, new Vector2(100, thickness));
            //existence = fl_skill;
            boundingCircle = new Circle(position, 100); //100 è un valore provvisorio
            ent_color = color;
            this.damage = new Damage();
            this.currentDungeon = currentDungeon;
            currentAnimation = 0;
            this.factionId = factionId;
            id = Globals.AssignAnId();
            state = 0;
            this.subtype = subtype;
            this.type = type;
            this.rotationAngle = rotationAngle;
            graphicOccupance = new Rectangle((int)position.X - 3, (int)position.Y - 3, 6, 6);
            this.sheet = sheet;
            name = "Blaze";
            this.direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            oldPosition = position;
            initialPosition = position;
            getLength();
            scale = new Vector2(1, 1);

            cData = new List<Collision>();
            cIds = new List<int>();
            dData = new List<DamageData>();
            damageData = new DamageData(position, factionId, id, damage, boundingBox, boundingCircle, id, type);
            spawned = new List<IEntity>();
        }

        public bool Is_in_camera(Rectangle camera)
        {
            return true;
        }

        public void SetCollisionData(Collision Data)
        {
            this.cData.Add(Data);
        }

        public void SetDamageData(DamageData Data)
        {
            this.dData.Add(Data);
        }

        public void Update(GameTime gameTime)
        {
            if (!maker.ActiveCaster)
            {
                updatable = false;
            }

            damageData.dealerPosition = origin;
            damageData.oArea = boundingBox;

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        if (c.collided)
                        {
                            switch (c.type)
                            {
                                case (int)entityTypes.item: break;
                                case (int)entityTypes.throwable: break;
                                case (int)entityTypes.dead: break;
                                case (int)entityTypes.explosion: break;
                                default:
                                    if (c.factionId != factionId)   //tale effetto è MOLTO da rivedere
                                    {
                                        // currentAnimation = 1; //il proiettile dovrebbe dissolversi
                                    }
                                    break;
                            }
                        }
                    }
                }

            }

            /*   if (currentAnimation == 0)
               {
                   if (!currentDungeon.WallContact(position + direction))
                   {
                       oldPosition = position;
                       position += direction * speed;
                   }
                   else
                   {
                       currentAnimation = 1;
                       animation.SetCurrentAnimation(currentAnimation);
                   }
           //        boundingBox.Origin = position;
             //      boundingCircle.Center = position;
               }*/

            getLength();

            cIds.Clear();
            cData.Clear();
            dData.Clear();
        }

        void getLength()
        {
            length = Vector2.Zero; //initialPosition;
            while (!currentDungeon.WallContact(length + initialPosition + direction))
            {
                length += direction;
            }
            origin = initialPosition + length / 2;

            boundingBox = new OBB(origin, rotationAngle, new Vector2(length.Length() / 2, thickness));
        }


        public void Draw(Rectangle camera)
        {
            Vector2 pos = new Vector2(origin.X - camera.Left, origin.Y - camera.Top);
            //Globals.spriteBatch.Draw(sheet[this.subtype].sourceBitmap, pos, sheet[this.subtype].Frame(currentAnimation, animation.GetCurrentFrame()), ent_color, rotationAngle, sheet[this.subtype].GetRotationCenter(currentAnimation, animation.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
        }

        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            for (int i = 0; i < 3; i++)
            {
                boxList[i].Draw(camera, Depths.boxes); 
            }
            //boundingBox.Draw(Globals.debugBox, camera, Globals.spriteBatch, Depths.boxes);
            boundingCircle.Draw(camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(origin.X - camera.Left - 40, origin.Y - camera.Top + 25);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
        }
        #region properties

        public Color Color
        {
            get { return ent_color; }
            set { ent_color = value; }
        }

        /// <summary>
        /// Verifica se l'entità è "morta" e ridotta a un cadavere statico
        /// </summary>
        public bool Corpse
        {
            get { return false; }
        }

        /// <summary>
        /// Ottiene o imposta l'id di appartenenza di un'entità a una "fazione"
        /// </summary>
        public int Faction
        {
            get { return factionId; }
            set { factionId = value; }
        }

        /// <summary>
        /// Ottiene il valore che verifica se l' entità sta eseguendo un azione
        /// </summary>
        public bool generic
        {
            get { return false; }
        }

        /// <summary>
        /// Ottiene o imposta un valore che determina se l' entità verrà aggiornata, se questo valore è impostato su false l' entità verrà rimossa della lista dlle entità
        /// </summary>
        public bool Updatable
        {
            get { return updatable; }
            set { updatable = value; }
        }

        /// <summary>
        /// Ottiene l'asse verticale dell'oggetto
        /// </summary>
        public VAxis getAxis
        {
            get { return this.axis; }
        }

        /// <summary>
        /// Ottiene il cerchio di collisione principale
        /// </summary>
        public Circle getBoundingCircle
        {
            get { return this.boundingCircle; }
        }

        /// <summary>
        /// Ottiene il danno descritto nel damagedata
        /// </summary>
        public DamageData getDamageDealt
        {
            get { return damageData; }
        }

        /// <summary>
        /// Ottiene la profondità dell' entità
        /// </summary>
        public float getDepth
        {
            get { return depth; }
        }

        /// <summary>
        /// Ottiene l' id unico dell'entità
        /// </summary>
        public int getId
        {
            get { return this.id; }
        }

        /// <summary>
        /// Ottiene il sottotipo dell'entità
        /// </summary>
        public int getSubtype
        {
            get { return subtype; }
        }

        /// <summary>
        /// Ottiene il tipo dell' entità
        /// </summary>
        public int getType
        {
            get { return type; }
        }

        /// <summary>
        /// Ottiene lo stato dell' entità
        /// </summary>
        public int State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// Ottiene il boundingBox dell' Entità
        /// </summary>
        public OBB getBoundingBox
        {
            get { return this.boundingBox; }
        }

        /// <summary>
        /// Ottiene l occupazione sullo schermo dell ' entità
        /// </summary>
        public Rectangle getOccupance
        {
            get { return graphicOccupance; }
        }

        public List<IEntity> GetSpawningList()
        {
            List<IEntity> l = new List<IEntity>();
            return l;
        }

        /// <summary>
        /// Ottiene il nome dell' entità
        /// </summary>
        public String getName
        {
            get { return this.name; }
        }

        /// <summary>
        /// Ottiene la posizione dell' entità
        /// </summary>
        public Vector2 getPosition
        {
            get { return origin; }
        }

        /// <summary>
        /// Ottiene la direzione dell' entità
        /// </summary>
        public Vector2 getDirection
        {
            get { return direction; }
        }
        #endregion
    }
}
