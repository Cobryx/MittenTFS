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
    public class Corpse : InteractiveScenography
    {
        Random r;
        public Corpse(Vector2 position, float rotation, int type, int subtype, ref SpriteSheet[] sheet, ref Dungeon dungeon)
            : base(position, rotation, type, subtype, ref sheet, ref dungeon)
        {
            r = new Random(id);
            this.position = position;
            this.rotationAngle = rotationAngle;

            sheetIndex = (int)sheetIndexes.corpse1;
            if (subtype == -1)
                throw new InvalidOperationException("Questo oggetto necessita di un subtype");

            if (subtype != 0)
            {
                switch (subtype)
                {
                    case 1: sheetIndex = (int)sheetIndexes.corpse1; break;
                    case 2: sheetIndex = (int)sheetIndexes.corpse2; break;
                    case 3: sheetIndex = (int)sheetIndexes.corpse3; break;
                }
            }
            else
                switch (r.Next(1, 4))
                {
                    case 1: sheetIndex = (int)sheetIndexes.corpse1; break;
                    case 2: sheetIndex = (int)sheetIndexes.corpse2; break;
                    case 3: sheetIndex = (int)sheetIndexes.corpse3; break;
                }
            boundingBox = new OBB(position, rotationAngle, new Vector2(sheet[sheetIndex].Frame(0, 0).Width / 2, sheet[sheetIndex].Frame(0, 0).Height / 2));
            boundingBox.DebugColor = new Color(128, 128, 20, 128);
            boundingBox.Origin = Vector2.Transform(boundingBox.Origin - position, Matrix.CreateRotationZ(rotationAngle)) + position;

            axis = new VAxis(0, 10);

            shadow = Krypton.ShadowHull.CreateRectangle(new Vector2(boundingBox.HalfWidths.X * 2, boundingBox.HalfWidths.Y * 2));//.CreateRectangle(new Vector2(1, 28)); //definizione dell ombra, da rivedere con un poligono più dettagliato
            shadow.Axis = axis;
            shadow.Position = new Vector2(this.position.X, this.position.Y);
            Globals.krypton.Hulls.Add(shadow);

            status = new EntityManager(1, 1, ref sheet[sheetIndex]);
            status.SetOn(0, 0, true, true);
        }

        public override void Update(GameTime gametime)
        {
            if (!alive)
                updatable = false;

            boundingCircle.Center = this.position;
            boundingBox.Origin = this.position;

            //da eseguire solo se la scenografia esegue rotazioni a tempo di compilazione
            //boundingBox.UpdateAxis(rotationAngle);

            if (cData.Count > 0)
            {
                foreach (Collision c in cData.Where(c => c.id != -1))
                {
                    if (!cIds.Contains(c.id)) //avoid calculating twice the same collision
                    {
                        if (c.collided)
                        {
                            float d = Vector2.Subtract(position + direction, c.position).Length();
                            if (d < c.distance)
                                direction *= 0;
                            cIds.Add(c.id);
                        }
                    }
                }
                cIds.Clear();
            }
            cData.Clear();
            base.Update(gametime);
        }
    }
}
