using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;

//http://www.gutgames.com/post/Noise-Reduction-of-an-Image-in-C-using-Median-Filters.aspx

/*
 * 
 * One of the main issues when trying to do image processing is the simple fact that images usually contain some degree of noise. This noise can in turn cause issues. 
 * For instance if you're doing edge detection, a spot on the image may cause the algorithm to detect edges that it shouldn't. One of the easiest ways to fix this issue 
 * is to use a median filter on an image. Unlike box blurs and gaussian blurs, we're not looking for the average of the pixels. In the case of a median filter, we're looking 
 * for the median (sort the values, take the one in the middle). We're still looking at a set of pixels around each pixel but we're simply taking the median instead of the mean.  
 * The advantage of this approach is that it keeps the edges of an image but simply degrades the details a bit, unlike a box blur which will slowly bleed the edges.

Unfortunately in C# there is no built in function to do this (or at least none that I'm aware of) and as such I wrote code to accomplish it and as always I'm sharing it with you:
 * 
 * The code above takes in an image and the size of the aperture. The aperture is the number of pixels around that pixel that you want it to look at. 
 * The bigger the aperture, the more "blurred" the image will be. There is one issue with the code that I should bring up and it's one that many implementations have to deal with. 
 * The issue is what do we do with the edges? Edges have less pixels and therefore are going to have more detail than everything else. In my case, I'm ignoring this fact. 
 * If you wanted you could modify it, I just felt you should know prior to using the code. Anyway, I hope this helps someone out. So give it a try, leave feedback, and happy coding.
 * */

namespace ToolsImages.Filtres
{
    public static class Median
    {
        public static Bitmap MedianFilter(Bitmap Image, int Size)
        {
            Bitmap TempBitmap = Image;
            Bitmap NewBitmap = new System.Drawing.Bitmap(TempBitmap.Width, TempBitmap.Height);
            Graphics NewGraphics = System.Drawing.Graphics.FromImage(NewBitmap);
            NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), System.Drawing.GraphicsUnit.Pixel);
            NewGraphics.Dispose();

            Random TempRandom = new Random();
            int ApetureMin = -(Size / 2);
            int ApetureMax = (Size / 2);
    
            for (int x = 0; x < NewBitmap.Width; ++x)
            {
                for (int y = 0; y < NewBitmap.Height; ++y)
                {
                    List<int> RValues = new List<int>();
                    List<int> GValues = new List<int>();
                    List<int> BValues = new List<int>();
           
                    for (int x2 = ApetureMin; x2 < ApetureMax; ++x2)
                    {
                        int TempX = x + x2;
                        if (TempX >= 0 && TempX < NewBitmap.Width)
                        {
                            for (int y2 = ApetureMin; y2 < ApetureMax; ++y2)
                            {
                                int TempY = y + y2;
                                if (TempY >= 0 && TempY < NewBitmap.Height)
                                {
                                    Color TempColor = TempBitmap.GetPixel(TempX, TempY);
                                    RValues.Add(TempColor.R);
                                    GValues.Add(TempColor.G);
                                    BValues.Add(TempColor.B);
                                }
                            }
                        }
                    }
                RValues.Sort();
                GValues.Sort();
                BValues.Sort();
         
                Color MedianPixel = Color.FromArgb(RValues[RValues.Count / 2],
                GValues[GValues.Count / 2], 
                BValues[BValues.Count / 2]);
                NewBitmap.SetPixel(x, y, MedianPixel);
            }
        }
      return NewBitmap;
        }
    }
}
