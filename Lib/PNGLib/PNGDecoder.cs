/* 
* Cosmos PNG Decoder
* Created by Szymekk44
* PNG Library used: https://github.com/EliotJones/BigGustave
* DeflateStream: https://github.com/jstedfast/Ionic.Zlib/tree/master
*/
using BigGustave;
using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using System;
using System.IO;
using CosmosPNG.GraphicsKit;

namespace CosmosPNG.PNGLib.Decoders.PNG
{
    /// <summary>
    /// Provides methods to decode PNG images and convert them into <see cref="Bitmap"/> objects.
    /// </summary>
    public class PNGDecoder
    {
        /// <summary>
        /// Decodes a PNG image from the specified file path and returns it as a <see cref="Bitmap"/> object.
        /// </summary>
        /// <param name="path">The file path to the PNG image.</param>
        /// <returns>A <see cref="Bitmap"/> representation of the PNG image.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file path is not found.</exception>
        /// <exception cref="IOException">Thrown when there is an I/O error while reading the file.</exception>
        public Bitmap GetBitmap(string path, bool returnErrors = true)
        {
            Bitmap buffer;
            if (!File.Exists(path))
            {
                if (returnErrors)
                    throw new Exception($"File not found but open requested.\n{path}");
            }

            using (var stream = new MemoryStream(File.ReadAllBytes(path)))
            {
                Png image = Png.Open(stream);

                buffer = new Bitmap((uint)image.Width, (uint)image.Height, ColorDepth.ColorDepth32);
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Pixel pixel = image.GetPixel(x, y);
                        buffer.SetRawPixel(pixel.R, pixel.G, pixel.B, pixel.A, x, y);
                    }
                }
                image = null;
            }
            Heap.Collect();
            return buffer;
        }

        /// <summary>
        /// Decodes a PNG image from the specified byte array and returns it as a <see cref="Bitmap"/> object.
        /// </summary>
        /// <param name="bytes">The byte array containing the PNG image data.</param>
        /// <returns>A <see cref="Bitmap"/> representation of the PNG image.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bytes"/> is null.</exception>
        /// <exception cref="InvalidDataException">Thrown when the byte array does not represent a valid PNG image.</exception>
        public Bitmap GetBitmap(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "The byte array cannot be null.");
            }

            Bitmap buffer;
            using (var stream = new MemoryStream(bytes))
            {
                Png image = Png.Open(stream);

                buffer = new Bitmap((uint)image.Width, (uint)image.Height, ColorDepth.ColorDepth32);
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Pixel pixel = image.GetPixel(x, y);
                        buffer.SetRawPixel(pixel.R, pixel.G, pixel.B, pixel.A, x, y);
                    }
                }
                image = null;
            }
            Heap.Collect();
            return buffer;
        }
    }
}
