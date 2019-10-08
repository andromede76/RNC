using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ToolsImages.Conversion 
{
    public static class Conversion
    {
        /// <summary>
        /// Transforme une image 32 bits en Teinte de Gris 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Bitmap Transforme32En8(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            Bitmap bitmap8bits = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            //table de pre-calcul
            Byte[] canalTab = new byte[256];
            int m;
            for (int i = 0; i < 256; i++)
            {
                canalTab[i] = (byte)(i / 51);
            }

            unsafe
            {
                BitmapData bmpDataOld = bitmap.LockBits(new Rectangle(0, 0,
                width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                BitmapData bmpDataNew = bitmap8bits.LockBits(new Rectangle(0,
                0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
                byte* oldPixel = (byte*)(void*)bmpDataOld.Scan0;
                byte* newPixel = (byte*)(void*)bmpDataNew.Scan0;
                int offsetNew = bmpDataNew.Stride - (width);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newPixel[0] = (byte)((36 * canalTab[oldPixel[2]]) +
                        (6 * canalTab[oldPixel[1]]) + canalTab[oldPixel[0]]);
                        oldPixel += 4;
                        newPixel++;
                    }
                    newPixel += offsetNew;
                }
                bitmap8bits.UnlockBits(bmpDataNew);
                bitmap.UnlockBits(bmpDataOld);

                return bitmap8bits;


            }

        }

        public static byte[] ConvertGrayscaleBitmaptoBytes(Bitmap original)
        {
            Bitmap gsBitmap = (Bitmap)original.Clone();
            if (original.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                gsBitmap = ColorToIndexedGrayscale(original);
            }

            byte[] grayscalebytes = new byte[gsBitmap.Width * gsBitmap.Height];
            BitmapData bmpData = gsBitmap.LockBits(new Rectangle(0, 0, gsBitmap.Width, gsBitmap.Height),
                                                ImageLockMode.ReadOnly,
                                                gsBitmap.PixelFormat);
            int bytes = Math.Abs(bmpData.Stride) * gsBitmap.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
            gsBitmap.UnlockBits(bmpData);
            //
            for (int h = 0; h < gsBitmap.Height; h++)
            {
                for (int w = 0; w < gsBitmap.Width; w++)
                {
                    grayscalebytes[h * gsBitmap.Width + w] = rgbValues[h * bmpData.Stride + w];
                }
            }

            return grayscalebytes;

        }

        public static Bitmap ColorToGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static Bitmap ColorToIndexedGrayscale(Bitmap original)
        {
            int w = original.Width,
                h = original.Height,
                r, ic, oc, bmpStride, outputStride, bytesPerPixel;
            PixelFormat pfIn = original.PixelFormat;
            ColorPalette palette;
            Bitmap output;
            BitmapData bmpData, outputData;

            //Create the new bitmap
            output = new Bitmap(w, h, PixelFormat.Format8bppIndexed);

            //Build a grayscale color Palette
            palette = output.Palette;
            for (int i = 0; i < 256; i++)
            {
                Color tmp = Color.FromArgb(255, i, i, i);
                palette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            output.Palette = palette;

            //No need to convert formats if already in 8 bit
            if (pfIn == PixelFormat.Format8bppIndexed)
            {
                output = (Bitmap)original.Clone();

                //Make sure the palette is a grayscale palette and not some other
                //8-bit indexed palette
                output.Palette = palette;

                return output;
            }

            //Get the number of bytes per pixel
            switch (pfIn)
            {
                case PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                case PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                case PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                default: throw new InvalidOperationException("Image format not supported");
            }

            //Lock the images
            bmpData = original.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
                                   pfIn);
            outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
                                         PixelFormat.Format8bppIndexed);
            bmpStride = bmpData.Stride;
            outputStride = outputData.Stride;

            int bmpBytes = Math.Abs(bmpData.Stride) * original.Height;
            byte[] rgbValues = new byte[bmpBytes];
            int indexdedBytes = Math.Abs(outputStride) * h;
            byte[] indexedValues = new byte[indexdedBytes];
            //Traverse each pixel of the image

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bmpBytes);

            if (bytesPerPixel == 3)
            {
                //Convert the pixel to it's luminance using the formula:
                // L = .299*R + .587*G + .114*B
                //Note that ic is the input column and oc is the output column
                for (r = 0; r < h; r++)
                    for (ic = oc = 0; oc < w; ic += 3, ++oc)
                        indexedValues[r * outputStride + oc] = (byte)(int)
                            (0.299f * rgbValues[r * bmpStride + ic] +
                                0.587f * rgbValues[r * bmpStride + ic + 1] +
                                0.114f * rgbValues[r * bmpStride + ic + 2]);
            }
            else //bytesPerPixel == 4
            {
                //Convert the pixel to it's luminance using the formula:
                // L = alpha * (.299*R + .587*G + .114*B)
                //Note that ic is the input column and oc is the output column
                for (r = 0; r < h; r++)
                    for (ic = oc = 0; oc < w; ic += 4, ++oc)
                        indexedValues[r * outputStride + oc] = (byte)(int)
                            ((rgbValues[r * bmpStride + ic] / 255.0f) *
                            (0.299f * rgbValues[r * bmpStride + ic + 1] +
                                0.587f * rgbValues[r * bmpStride + ic + 2] +
                                0.114f * rgbValues[r * bmpStride + ic + 3]));
            }

            //Copy the data from the byte array into BitmapData.Scan0
            System.Runtime.InteropServices.Marshal.Copy(indexedValues, 0, outputData.Scan0, indexedValues.Length);
            //Unlock the images
            original.UnlockBits(bmpData);
            output.UnlockBits(outputData);

            return output;
        }
        public static Bitmap ResizeBitmap(Bitmap original, int newWidth, int newHeight)
        {
            //a holder for the result
            Bitmap result = new Bitmap(newWidth, newHeight);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(original, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }
        public static bool IsGrayscale(Bitmap original)
        {
            bool ret = false;

            // check pixel format
            if (original.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ret = true;
                // check palette
                ColorPalette cp = original.Palette;
                Color c;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    c = cp.Entries[i];
                    if ((c.R != i) || (c.G != i) || (c.B != i))
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }

    }
}
