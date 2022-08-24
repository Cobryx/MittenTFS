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
    public class Laser : IEntity, ILightEntity, IMulticell,IAttacker
    {
        Krypton.Lights.Light2D lightLaser;
        Krypton.Lights.Light2D lightSource;
   
        int lightSourceIndex;
        bool existence;
        bool updatable = true;
        Circle boundingCircle;
        Color ent_color;
        Damage damage;
        DamageData damageData;
        Dungeon currentDungeon;
        int currentstatus;
        int factionId;
        int id;
        protected int sheetIndex;
        int state;
        int subtype;
        int type;
        float depth;
        
        float rotationAngle;
        float speed;
        float thickness=0;
        float maxThickness =1.8f ;

        ICaster caster;
        OBB boundingBox;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 initialPosition;
        Vector2 length;
        Vector2 destination;

        Vector2 offset; // serve per posizionare i vari raggi

        Vector2 oldPosition;
        Vector2 origin;
        Vector2 scale;

        List<int> cIds;
        List<Collision> cData;
        List<DamageData> dData;
        List<IEntity> spawned;
        List<Vector2> lengths = new List<Vector2>();

        EntityManager status;


        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];

        VAxis axis;

        public Laser(int element, int factionId, int subtype, int type, object rotationAngle, ref Dungeon currentDungeon, ref SpriteSheet[] sheet, Vector2 position, ICaster caster)
        {
            //istanziazione vettori per la definizione di damagadata
            

            Color color;

            dam[(int)damageTypes.energy] = 0.02f;
            pro[(int)damageEffects.mechanical] = 0;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 0;
            damage = new Damage(dam, tim, pro, eff, dur);

            lightLaser = new Krypton.Lights.Light2D();
            lightLaser.IsOn = true;
            lightLaser.Fov = MathHelper.TwoPi/100;
            //light.Color=Color.BlueViolet;
            switch (element)
            {
                case (int)damageTypes.air: color = Color.BlueViolet; break;
                case (int)damageTypes.earth: color = Color.Brown; break;
                case (int)damageTypes.energy: color = Color.Yellow; break;
                case (int)damageTypes.fire: color = Color.Red; break;
                case (int)damageTypes.physical: color = Color.White; break;
                case (int)damageTypes.poison: color = Color.Green; break;
                case (int)damageTypes.spectral: color = Color.DarkGray; break;
                case (int)damageTypes.water: color = Color.Blue; break;
                default: color = Color.White; break;
            }
            lightLaser.Color = color;//new Color(152, 102, 204);
            lightLaser.Angle = 0;
            lightLaser.Intensity = 0.60f;
            lightLaser.Texture = Globals.mLightTexture;
            lightLaser.X = initialPosition.X - Globals.camera[0].Left;
            lightLaser.Y = initialPosition.Y - Globals.camera[0].Top;

            lightSource = new Krypton.Lights.Light2D();
            lightSource.IsOn = true;
            lightSource.Fov = MathHelper.TwoPi;
            //light.Color=Color.BlueViolet;
            lightSource.Color = color;//new Color(152, 102, 204);
            lightSource.Angle = 0;
            lightSource.Intensity = 0.00f;
            lightSource.Range = 50;
            lightSource.Texture = Globals.mLightTexture;
            lightSource.X = initialPosition.X - Globals.camera[0].Left;
            lightSource.Y = initialPosition.Y - Globals.camera[0].Top;

            Globals.krypton.Lights.Add(lightLaser);


            Globals.krypton.Lights.Add(lightSource);
            lightSourceIndex = Globals.krypton.Lights.Count - 1;

            this.caster = caster;
            axis = new VAxis(63, 65);
            sheetIndex = (int)sheetIndexes.laser;
            status = new EntityManager(1, 1, ref sheet[sheetIndex]);
            boundingBox = new OBB(position, this.rotationAngle, new Vector2(100, thickness*3));
            boundingBox.DebugColor = Color.Black;
            //existence = fl_skill;
            boundingCircle = new Circle(position, 100); //100 è un valore provvisorio
            ent_color = color;
            
            this.currentDungeon = currentDungeon;
            
            this.factionId = factionId;
            id = Globals.AssignAnId();
            this.subtype = subtype;
            this.type = type;
            //this.speed = 12f;   //valore di default
            graphicOccupance = new Rectangle((int)position.X - 3, (int)position.Y - 3, 6, 6);
            this.sheet = sheet;
            name = "Laser";
            this.direction = new Vector2((float)Math.Cos(this.rotationAngle), (float)Math.Sin(this.rotationAngle));
            oldPosition = position;
            initialPosition = position;
            offset = Vector2.Zero;
            getLength();
            scale = new Vector2(1, thickness);

            cData = new List<Collision>();
            cIds = new List<int>();
            dData = new List<DamageData>();
            damageData = new DamageData(position, factionId, id, damage, boundingBox, null, id, type);
            spawned = new List<IEntity>();
            status.SetOn(0, 0, true, true);
        }

        public bool Is_in_camera(Rectangle camera)
        {
            return true;
        }

        public void SetCollisionData(Collision Data)
        {
            this.cData.Add(Data);
        }

        /*public void SetDamageData(DamageData Data)
        {
            this.dData.Add(Data);
        }*/

        public void Update(GameTime gameTime)
        {
            status.Update(gameTime);
            status.AutoOff();


            if (thickness < maxThickness)
            {
                thickness += 0.005f;
                scale.Y = thickness;
                dam[(int)damageTypes.energy] = 0.10f * ((float)Math.Pow(thickness,2));
                damage = new Damage(dam, tim, pro, eff, dur);
                damageData = new DamageData(initialPosition, factionId, id, damage, boundingBox, null, id, type);
            }
            if (!caster.ActiveCaster)
            {
                updatable = false;
                Globals.krypton.Lights.Remove(lightSource);
                Globals.krypton.Lights.Remove(lightLaser);
                lightLaser = null;
                lightSource = null;
                
            }
            initialPosition = new Vector2(caster.getPosition.X  + (float)Math.Cos(caster.getRotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(caster.getRotationAngle) * caster.magicOrigin1.Y, caster.getPosition.Y  + (float)Math.Sin(caster.getRotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(caster.getRotationAngle) * caster.magicOrigin1.Y);   //oldPosition /*+ maker.getDirection*/;

            damageData.dealerPosition = origin;
            damageData.oArea = boundingBox;
            getLength();
            damageData = new DamageData(initialPosition, factionId, id, damage, boundingBox, null, id, type);

            if (cData.Count > 0)
            {
                LineEquation eq = new LineEquation(initialPosition, initialPosition + length);
                Vector2 intersectionPoint;
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
                                    //if (c.factionId == factionId)   //tale effetto è MOLTO da rivedere
                                    if (c.id != id && c.id != caster.getId)
                                    {
                                        for(int i=0;i<4;i++)
                                        {
                                            if (eq.IntersectWithSegmentOfLine(c.boundingBox.Edge(i), out intersectionPoint))
                                                setLength(intersectionPoint);
                                                //lengths.Add(intersectionPoint - initialPosition);
                                        }
                                        //lengths.Add(new Vector2(Math.Abs(c.position.X - initialPosition.X), Math.Abs(c.position.Y - initialPosition.Y)));
                                        //getLength(new Vector2(Math.Abs(c.position.X - initialPosition.X), Math.Abs(c.position.Y - initialPosition.Y)));
                                    }
                                    break;
                            }
                            //getLength(new Vector2(Math.Abs(c.position.X - initialPosition.X), Math.Abs(c.position.Y - initialPosition.Y)));
                        }
                    }
                }
                setLength();
                lengths.Clear();
            }
            cIds.Clear();

            cData.Clear();
            dData.Clear();
        }

        void getLength()
        {
            //reimpostarlo per laser "onda energetica
            length = Vector2.Zero; //initialPosition;
            rotationAngle = caster.getRotationAngle;
            direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            while (!currentDungeon.WallContact(length + initialPosition + direction))
            {
                length += direction;
            }
            origin = initialPosition + length / 2;
            boundingBox = new OBB(origin, rotationAngle, new Vector2(length.Length() / 2, thickness*3));
            length -= direction*8;
            destination = initialPosition + length;
            if (lightLaser != null)
            {
                lightLaser.X = initialPosition.X - Globals.camera[0].Left;
                lightLaser.Y = initialPosition.Y - Globals.camera[0].Top;
                lightLaser.Range = length.Length() * 2;
                lightLaser.Angle = -rotationAngle;
                lightLaser.Fov = (MathHelper.TwoPi / 150) * thickness;
            }
            if (lightSource != null)
            {
                lightSource.X = initialPosition.X - Globals.camera[0].Left;
                lightSource.Y = initialPosition.Y - Globals.camera[0].Top;
                lightSource.Intensity = 0.20f + thickness / 3.5f;
            }
        }
        
        void setLength()
        {
            //reimpostarlo per laser "onda energetica
            //initialPosition;
            float d = length.Length();
            float l = d;
            foreach (Vector2 v in lengths)
            {
                l = v.Length();
                if (l < d)
                    d = l;
            }
            direction = new Vector2((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle));
            length = l * direction;
            origin = initialPosition + length / 2;
            //boundingBox = new OBB(origin, rotationAngle, new Vector2(length.Length() / 2, thickness / 2/** 3*/));
            //boundingBox.HalfWidths= new Vector2(length.Length() / 2, thickness * 3);
            destination = initialPosition + length;
            if (lightLaser!=null)
                lightLaser.Range = length.Length() * 2;
        }

        void setLength(Vector2 v)
        {
            length = v - initialPosition;
            origin = initialPosition + length / 2;
            destination = v;
            if (lightLaser != null)
                lightLaser.Range = length.Length() * 2;
        }

        public void Draw(Rectangle camera)
        {
            Vector2 pos = new Vector2(initialPosition.X - camera.Left, initialPosition.Y - camera.Top);
            offset = direction * 106;
            float dimension = 2180;
            Vector2 l;
            
            
            for (l = Vector2.Zero; Vector2.Distance(initialPosition+l, destination) >= 106f; l += offset)
            {
                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos + l, sheet[sheetIndex].Frame(currentstatus, (status.GetCurrentFrame() + (int)(dimension / 106)) %20), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(currentstatus, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
                dimension -= 106;
                if (dimension < 0)
                {
                }
            }
            //calcola la dimensione  dell'ultimo tratto di laser e la disegna
            dimension = Vector2.Distance(destination, initialPosition + l);//valore correttivo
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos + l, sheet[sheetIndex].Frame(currentstatus, status.GetCurrentFrame(), dimension), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(currentstatus, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
            
            //disegna l'inizio del laser
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos + direction*2 , sheet[sheetIndex].Frame(currentstatus + 1, status.GetCurrentFrame()), ent_color, rotationAngle-(float)Math.PI, sheet[sheetIndex].GetRotationCenter(currentstatus + 1, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);
            dimension -= 1;//valore correttivo
            //disegna fine del laser
            Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos + l + new Vector2(dimension * direction.X, dimension * direction.Y), sheet[sheetIndex].Frame(currentstatus + 2, status.GetCurrentFrame()), ent_color, rotationAngle, sheet[sheetIndex].GetRotationCenter(currentstatus + 2, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + id / Globals.max_entities);

            //boundingBox.Draw(camera, Depths.boxes);
        }

        public void DrawCollidedObjectDebug( Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
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
        public Krypton.Lights.Light2D Light
        {
            get { return lightLaser; }
            set { lightLaser = value; }
        }

        /// <summary>
        /// Ottiene o imposta il colore di un'entità
        /// </summary>
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
            get { return initialPosition; }
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
