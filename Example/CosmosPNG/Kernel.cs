using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using CosmosPNG.GraphicsKit;
using System.Drawing;
using Cosmos.System.Graphics.Fonts;
using Cosmos.Core.Memory;
using CosmosPNG.PNGLib.Decoders.PNG;

namespace CosmosPNG
{
    public class Kernel : Sys.Kernel
    {
        public static Canvas MainCanvas { get; set; }
        public static uint ScreenWidth { get; set; } = 1920;
        public static uint ScreenHeight { get; set; } = 1080;
        protected override void BeforeRun()
        {
            MainCanvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));
            /* Decode and draw PNG wallpaper */
            Heap.Collect();
            try
            {
                Bitmap Wallpaper = new PNGDecoder().GetBitmap(Resources.WallpaperMountain);
                MainCanvas.DrawImage(Wallpaper, 0, 0);
            }
            catch (Exception ex)
            {
                PCScreenFont font = PCScreenFont.Default;
                MainCanvas.DrawString(ex.Message, font, Color.Red, 0, 0);
            }
            MainCanvas.Display();
            /* Decode and draw PNG with transparency */
            Heap.Collect();
            try
            {
                Bitmap Logo = new PNGDecoder().GetBitmap(Resources.CosmosLogo);
                int CenterX = (int)(ScreenWidth - Logo.Width) / 2;
                int CenterY = (int)(ScreenHeight - Logo.Height) / 2;
                MainCanvas.DrawImageAlpha(Logo, CenterX, CenterY);
            }
            catch (Exception ex)
            {
                PCScreenFont font = PCScreenFont.Default;
                MainCanvas.DrawString(ex.Message, font, Color.Red, 0, 0);
            }
            MainCanvas.Display();
        }

        protected override void Run()
        {

        }
    }
}
