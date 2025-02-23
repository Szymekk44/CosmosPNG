/* 
* Cosmos PNG Decoder - Bitmap operations
* Created by Szymekk44
*/
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosPNG.GraphicsKit
{
    public static class BitmapOperations
    {
        public static void SetPixel(this Bitmap bmp, Color color, int X, int Y)
        {
            bmp.RawData[X + (Y * bmp.Width)] = color.ToArgb();
        }
        public static Color GetPixel(this Bitmap bmp, int X, int Y)
        {
            return Color.FromArgb(bmp.RawData[X + (Y * bmp.Width)]);
        }

        public static void SetRawPixel(this Bitmap bmp, int R, int G, int B, int X, int Y)
        {
            bmp.RawData[X + (Y * bmp.Width)] = (255 << 24) | (R << 16) | (G << 8) | B;
        }
        public static void SetRawPixel(this Bitmap bmp, int ARGB, int X, int Y)
        {
            bmp.RawData[X + (Y * bmp.Width)] = ARGB;
        }
        public static void SetRawPixel(this Bitmap bmp, int R, int G, int B, int A, int X, int Y)
        {
            bmp.RawData[X + (Y * bmp.Width)] = (A << 24) | (R << 16) | (G << 8) | B;
        }
    }
}
