namespace BigGustave
{
    using System.Collections.Generic;

    /// <summary>
    /// The high-level information about the image.
    /// </summary>
    public class ImageHeader
    {
        internal static byte[] HeaderBytes = {
            73, 72, 68, 82
        };
        public static Dictionary<ColorType, byte[]> PermittedBitDepths;

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The bit depth of the image.
        /// </summary>
        public byte BitDepth { get; set; }

        /// <summary>
        /// The color type of the image.
        /// </summary>
        public ColorType ColorType { get; set; }

        /// <summary>
        /// The compression method used for the image.
        /// </summary>
        public CompressionMethod CompressionMethod { get; set; }

        /// <summary>
        /// The filter method used for the image.
        /// </summary>
        public FilterMethod FilterMethod { get; set; }

        /// <summary>
        /// The interlace method used by the image.
        /// </summary>
        public InterlaceMethod InterlaceMethod { get; set; }
    }
}
