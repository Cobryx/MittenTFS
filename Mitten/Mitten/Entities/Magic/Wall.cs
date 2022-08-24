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
    public class Wall : IEntity,IDamageble
    {
        Krypton.Lights.Light2D light;
        protected int[] effects;
        //istanziazione vettori per la definizione di damagadata
        float[] dam = new float[Globals.ndamagetypes];
        int[] tim = new int[Globals.ndamagetypes];
        float[] eff = new float[Globals.damage_effects];
        float[] pro = new float[Globals.damage_effects];
        int[] dur = new int[Globals.damage_effects];


        bool mainBlock;
        bool updatable = true;
        public bool alive = true;
        Circle boundingCircle;
        Color ent_color;
        Damage damage;
        DamageData damageData;
        DamageManager damageManager;
        Dungeon currentDungeon;
        EntityManager status;
        int factionId;
        int id;
        ICaster caster;
        protected int sheetIndex;
        
        int state;
        int subtype;
        int blockNumber;

        int type;
        float depth=0;
        float health;
        float power;
        float rotationAngle;
        OBB boundingBox;
        Rectangle graphicOccupance;
        SpriteSheet[] sheet;
        String name;
        Vector2 direction;
        Vector2 position;
        Vector2 distance;
        Vector2 scale;
        Vector2 start;

        List<int> cIds;
        List<int> dIds;
        List<Collision> cData;
        List<DamageData> dData;
        List<IEntity> spawned;
        List<IEntity> walls;

        VAxis axis;

        public Wall(float power, Color color, int factionId, int subtype, int type, float rotationAngle, ref Dungeon currentDungeon, ref SpriteSheet[] sheet, Vector2 position, ICaster caster,bool mainBlock,int blockNumber=0)
        {
            this.caster = caster;
            this.mainBlock=mainBlock;

            health = 50; //basarlo su power
            damageManager = new DamageManager(health);

            dam[(int)damageTypes.energy] = 0;
            pro[(int)damageEffects.mechanical] = 0;
            eff[(int)damageEffects.mechanical] = 0;
            dur[(int)damageEffects.mechanical] = 0;
            power = 100;//debug
            this.rotationAngle = rotationAngle ;
            this.blockNumber = blockNumber;
            this.direction = new Vector2((float)Math.Cos(this.rotationAngle), (float)Math.Sin(this.rotationAngle));

            if (mainBlock)
            {
                this.position = position + this.direction * power;
                this.direction = Service.Perpendicular(direction);
                start = this.position+(new Vector2(this.direction.X * 28 * 2.5f, direction.Y * 28 * 2.5f));//bug:centrare l oggetto
                this.position = start;
            }
            else
                this.position = position;

            axis = new VAxis(0, 512); //i muri magici dovrebbero essere alti
            
            spawned = new List<IEntity>();
            
            this.power = power;
            
            switch (subtype)
            {
                case 0: //muro di fuoco
                    break;
                case 1: //muro di ghiaccio
                    sheetIndex = (int)sheetIndexes.icewall;
                    type = (int)entityTypes.icewall;
                    break;
                default: throw new InvalidOperationException("errore nello sheet delle animazioni");
            }

            boundingBox = new OBB(this.position, rotationAngle, new Vector2(13, 13));
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - this.position, Matrix.CreateRotationZ(rotationAngle)) + this.position;
            
            if (mainBlock)
            {
                walls = new List<IEntity>();
                walls.Add(this);
                int nblocks = 1;
                
                while (nblocks < 5)
                {
                    /*Vector2 nextBlock = this.position + (new Vector2(this.direction.X * 28 * (nblocks / 2), direction.Y * 28 * (nblocks / 2))) * offsetsign;
                        walls.Add(new Wall(power, Color.White, factionId, 1, (int)entityTypes.magic, this.rotationAngle, ref currentDungeon, ref sheet, nextBlock, caster, false, nblocks - 1));//*new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y)*//*, caster, false));*/
                    /*offsetsign *= -1;
                    nblocks++;*/
                    Vector2 nextBlock = start - (new Vector2(this.direction.X * 28 * (nblocks), direction.Y * 28 * (nblocks )));
                        walls.Add(new Wall(power, Color.White, factionId, 1, (int)entityTypes.magic, this.rotationAngle, ref currentDungeon, ref sheet, nextBlock, caster, false, nblocks));//*new Vector2(position.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.X - (float)Math.Sin(rotationAngle) * caster.magicOrigin1.Y, position.Y + (float)Math.Sin(rotationAngle) * caster.magicOrigin1.X + (float)Math.Cos(rotationAngle) * caster.magicOrigin1.Y)*/, caster, false));
                    nblocks++;
                }
                foreach (Wall w in walls)
                    spawned.Add(w);
                }


            if (currentDungeon.WallContact(new Vector2(this.position.X, this.position.Y)))
                alive = false;
            else
            {
                light = new Krypton.Lights.Light2D();

                light.IsOn = false;
                light.Fov = MathHelper.TwoPi;
                light.Color = Color.LightSkyBlue;
                light.Angle = 0;
                light.Intensity = 0.6f;
                light.Texture = Globals.mLightTexture;
                light.Range = 100f;
                Globals.krypton.Lights.Add(light);
            }

            boundingCircle = new Circle(-this.position, 1); 
            ent_color = Color.White;
            this.damage = new Damage();
            this.currentDungeon = currentDungeon;
            
            this.factionId = factionId;
            id = Globals.AssignAnId();
            state = 0;
            this.subtype = subtype;
            this.type = type;
           
            graphicOccupance = new Rectangle((int)this.position.X - 3, (int)this.position.Y - 3, 6, 6);
            this.sheet = sheet;
            name = "Wall";
            
            scale = new Vector2(1, 1);

            cData = new List<Collision>();
            cIds = new List<int>();
            dData = new List<DamageData>();
            dIds = new List<int>();
            damageData = new DamageData(this.position, factionId, id, damage, boundingBox, boundingCircle, id, type);
            status = new EntityManager(3, 3, ref sheet[sheetIndex]);
            status.SetOn(0, sheet[sheetIndex].GetTotalDuration(0), 0, false, true);


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

        private void Explode()
        {
            if (status.IsOff(2))
                status.SetOn(2, sheet[sheetIndex].GetTotalDuration(2), 2, false, true);
        }

        public void Update(GameTime gameTime)
        {
            if (mainBlock)
            {
                bool wallDown= true;
                foreach (Wall w in walls)
                {
                    if (w.alive && !w.status.IsOn(2) && w.status.GetCurrentFrame() != 0)
                        wallDown = false;
                }

                
                //cancellazione delle entità figlie e di quella principlae se tutti i muri sono stati abbattuti

                if (wallDown && !alive)
                {
                    foreach (Wall w in walls)
                        w.updatable = false;
                    updatable = false;
                }
            }
                status.Update(gameTime);
                status.AutoOff();

                damageManager.Update(gameTime);
                if (damageManager.health <= 0 && alive)
                    Explode();

                damageData.dealerPosition = caster.getPosition;
                damageData.cArea = boundingCircle;

                if (status.IsOn(0))
                {
                    //damageData.ResetOBB();
                    boundingCircle.Center = position;
                    if (status.GetCurrentFrame() > 1  && light!=null)
                    {
                        light.IsOn = true;
                        light.Intensity = 0.55f - (float)status.GetCurrentFrame() / 20f; // da sistemare
                    }

                }
                else if (status.Finished(0))
                {
                    status.SetOn(1, sheet[sheetIndex].GetTotalDuration(1), 1, true, true);
                }

                if (status.IsOn(1))
                {
                    Globals.krypton.Lights.Remove(light);
                    light = null;
                }

                if (status.IsOn(2))
                {
                    depth = +0.01f;
                    if (status.GetCurrentFrame() == 3) 
                    {
                        boundingBox.HalfWidths = new Vector2(0, 0); //evita la collisione 
                        boundingBox.Origin = new Vector2(0, 0);
                    }

                }
            if (status.Finished(2))
                {
                alive = false;
                

                if ( mainBlock && blockNumber!= -1)
                  ((Wall)walls[blockNumber]).alive = false;
                }

                if (cData.Count > 0)
                {
                    foreach (Collision c in cData.Where(c => c.id != -1))
                    {
                        if (!cIds.Contains(c.id))
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
                                        if (status.GetCurrentFrame() == 1 && status.IsOn(0))
                                        {
                                            
                                            //Explode();
                                            Globals.krypton.Lights.Remove(light);
                                            light = null;
                                            alive = false;
                                        }
                                        if (c.factionId != factionId && axis.Bottom <= c.axis.Top && axis.Top >= c.axis.Bottom)
                                        {
                                            //lo stato 0 non termina mai autonomamente giacché la durata è indeterminata;
                                            //in un'entità normale ci sarebbe stato un metodo che avrebbe provveduto a settare a off gli stati da escludere
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    cIds.Clear();

                }

                foreach (DamageData d in dData.Where(d => d.id != -1))
                {
                if (!dIds.Contains(d.id))
                    {
                        dIds.Add(d.id);
                        damageManager.CalculateDamage(d.damage);

                        //AGGIUNGERE ALTRE REAZIONI PREVISTE
                        if (damageManager.Effects((int)damageEffects.blind) > 0)
                        {
                            //aggiungere cecità
                        }
                        if (damageManager.Effects((int)damageEffects.buff) > 0)
                        {
                            //aggiungere dettagli buff
                        }
                        if (damageManager.Effects((int)damageEffects.debuff) > 0)
                        {
                        }
                        if (damageManager.Effects((int)damageEffects.freeze) > 0)
                        {
                        }
                    }
                }
                dIds.Clear();


                if (light != null)
                {
                    light.X = position.X - Globals.camera[0].Left;
                    light.Y = position.Y - Globals.camera[0].Top;
                }
                cData.Clear();
                dData.Clear();

        }


        public void Draw(Rectangle camera)
        {
            Vector2 pos;
            if (updatable) //c'è da regolare la cosa a livello globale
            {
                
                if (alive)
                {
                    Globals.IAmanager.Intensity((IEntity)this);
                    pos = new Vector2(position.X - camera.Left, position.Y - camera.Top);
                    Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos, sheet[sheetIndex].Frame(status.CurrentAnimation, status.GetCurrentFrame()), ent_color, rotationAngle - (float)Math.PI / 2, sheet[sheetIndex].GetRotationCenter(status.CurrentAnimation, status.GetCurrentFrame()), scale, SpriteEffects.None, depth + Depths.foreSkill + (float)id / Globals.max_entities);
                }
                if (mainBlock)
                {
                    for (int i= 0; i < walls.Count-1; i++)
                    {
                        pos = new Vector2(walls[i].getPosition.X - camera.Left, walls[i].getPosition.Y - camera.Top);
                        if (((Wall)walls[i]).alive && ((Wall)walls[i+1]).alive)
                        {
                            if(((Wall)walls[i]).status.IsOn(2))
                                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos - (direction * 14) , sheet[sheetIndex].Frame(((Wall)walls[i]).status.CurrentAnimation + 3, ((Wall)walls[i]).status.GetCurrentFrame()), ent_color, rotationAngle - (float)Math.PI / 2, sheet[sheetIndex].GetRotationCenter(((Wall)walls[i]).status.CurrentAnimation + 3, ((Wall)walls[i]).status.GetCurrentFrame()), scale, SpriteEffects.None, Depths.overForeSkill + (float)id / Globals.max_entities);
                            else if (((Wall)walls[i+1]).status.IsOn(2))
                                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos - (direction * 14), sheet[sheetIndex].Frame(((Wall)walls[i+1]).status.CurrentAnimation + 3, ((Wall)walls[i+1]).status.GetCurrentFrame()), ent_color, rotationAngle - (float)Math.PI / 2, sheet[sheetIndex].GetRotationCenter(((Wall)walls[i]).status.CurrentAnimation + 3, ((Wall)walls[i]).status.GetCurrentFrame()), scale, SpriteEffects.None, Depths.overForeSkill + (float)id / Globals.max_entities);
                            else
                                Globals.spriteBatch.Draw(sheet[sheetIndex].sourceBitmap, pos - (direction * 14), sheet[sheetIndex].Frame(((Wall)walls[i]).status.CurrentAnimation + 3, ((Wall)walls[i]).status.GetCurrentFrame()), ent_color, rotationAngle - (float)Math.PI / 2, sheet[sheetIndex].GetRotationCenter(((Wall)walls[i]).status.CurrentAnimation + 3, ((Wall)walls[i]).status.GetCurrentFrame()), scale, SpriteEffects.None, Depths.overForeSkill + (float)id / Globals.max_entities);
                        }
                    }
                }

            }
            // boundingBox.Draw(camera, 0);
        }


        public void DrawCollidedObjectDebug(Rectangle camera)
        {
            boundingBox.Draw(camera, Depths.boxes);
            //boundingCircle.Draw(Globals.spriteBatch, camera);
        }

        public void DrawDebug(Rectangle camera, ref SpriteFont debugFont)
        {
            Vector2 pos = new Vector2(position.X - camera.Left - 40, position.Y - camera.Top + 25);
            String s;
            s = "Id: " + this.getId.ToString() + "; nome: " + this.getName + "; inclinazione in radianti: " + this.rotationAngle.ToString();
            Globals.spriteBatch.DrawString(debugFont, s, pos, Color.Blue, 0.0f, new Vector2(0), 1f, SpriteEffects.None, 0f);
        }
        #region properties

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

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
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

        public DamageManager DamageManager
        {
            get { return damageManager; }
            set { damageManager = value; }
        }

        /// <summary>
        /// Ottiene la profondità dell' entità
        /// </summary>
        public float getDepth
        {
            get { return depth + Depths.foreSkill + (float)id / Globals.max_entities; }
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
            l = spawned.GetRange(0, spawned.Count);
            spawned.Clear();
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
            get { return position; }
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
