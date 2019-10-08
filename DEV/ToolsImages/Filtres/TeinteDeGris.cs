using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

namespace ToolsImages.Filtres
{
    public class ColorMatrix
    {
         #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorMatrix()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Matrix containing the values of the ColorMatrix
        /// </summary>
        public float[][] Matrix { get; set; }

        #endregion

        #region Public Functions

        /// <summary>
        /// Applies the color matrix
        /// </summary>
        /// <param name="OriginalImage">Image sent in</param>
        /// <returns>An image with the color matrix applied</returns>
        private Bitmap Apply(Bitmap OriginalImage)
        {
            Bitmap NewBitmap = new Bitmap(OriginalImage.Width, OriginalImage.Height);
            using (Graphics NewGraphics = Graphics.FromImage(NewBitmap))
            {
                System.Drawing.Imaging.ColorMatrix NewColorMatrix = new System.Drawing.Imaging.ColorMatrix(Matrix);
                using (ImageAttributes Attributes = new ImageAttributes())
                {
                    Attributes.SetColorMatrix(NewColorMatrix);
                    NewGraphics.DrawImage(OriginalImage,
                         new System.Drawing.Rectangle(0, 0, OriginalImage.Width, OriginalImage.Height),
                           0, 0, OriginalImage.Width, OriginalImage.Height,
                           GraphicsUnit.Pixel,
                       Attributes);
                }
            }
            return NewBitmap;
        }

        /// <summary>
        /// Converts an image to black and white
        /// </summary>
        /// <param name="Image">Image to change</param>
        /// <returns>A bitmap object of the black and white image</returns>
        public static Bitmap ConvertBlackAndWhite(Bitmap Image)
        {
            ColorMatrix TempMatrix = new ColorMatrix();
            TempMatrix.Matrix = new float[][]{
                     new float[] {.3f, .3f, .3f, 0, 0},
                      new float[] {.59f, .59f, .59f, 0, 0},
                       new float[] {.11f, .11f, .11f, 0, 0},
                      new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                   };
            return TempMatrix.Apply(Image);
        }


        //float[][] FloatColorMatrix ={ 
        //new float[] {1, 0, 0, 0, 0}, 
        //new float[] {0, 1, 0, 0, 0}, 
        //new float[] {0, 0, 1, 0, 0}, 
        //new float[] {0, 0, 0, 1, 0}, 
        //new float[] {0, 0, 0, 0, 1} 
        //};


        #endregion
    }
}
