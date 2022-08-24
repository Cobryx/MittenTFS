using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    /// <summary>
    /// La classe TileSet implementa un classico tileset per videogiochi bidimensionali con visuale dall'alto.
    /// </summary>
    public class TileSet
    {
        Texture2D image;
        int tile_width;
        int tile_height;
        int htilenum;
        int vtilenum;
        
        /// <summary>
        /// Costruttore che istanzia TileSet fornendo le dimensioni dei singoli tile e il numero di tile in cui suddivedere l'immagine base.
        /// </summary>
        /// <param name="image">Immagine base del tileset.</param>
        /// <param name="width">Larghezza del singolo tile.</param>
        /// <param name="height">Altezza del singolo tile.</param>
        /// <param name="hnum">Numero di tile nel tileset in senso orizzontale.</param>
        /// <param name="vnum">Numero di tile nel tileset in senso verticale.</param>
        public TileSet(Texture2D image, int width, int height, int hnum, int vnum)
        {
            this.image = image;
            this.tile_width = width;
            this.tile_height = height;
            this.htilenum = hnum;
            this.vtilenum = vnum;
        }

        /// <summary>
        /// Costruttore che istanzia un TileSet di tile quadrati della dimensione specificata.
        /// </summary>
        /// <param name="image">Immagine base del tileset.</param>
        /// <param name="dimension">Lato del singolo tile (quadrato).</param>
        public TileSet(Texture2D image, int dimension)
        {
            if (image.Width % dimension != 0 || image.Height % dimension != 0)
            {
                throw new System.ArgumentException("Image size is not a multiple of specified tile size.");
            }
            else
            {
                this.image = image;
                this.tile_height = dimension;
                this.tile_width = dimension;
                this.htilenum = image.Width / dimension;
                this.vtilenum = image.Height / dimension;
            }
        }

        #region properties

        public Texture2D SourceBitmap
        {
            get { return this.image; }
        }

        public int TileWidth
        {
            get { return this.tile_width; }
        }

        public int TileHeight
        {
            get { return this.tile_height; }
        }

        public int HorizontalTiles
        {
            get { return htilenum; }
        }

        public int VerticalTiles
        {
            get { return vtilenum; }
        }

        #endregion
    }
}
