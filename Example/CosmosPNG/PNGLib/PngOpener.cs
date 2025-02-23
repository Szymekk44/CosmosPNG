namespace BigGustave
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text;

    internal static class PngOpener
    {
        public static Png Open(Stream stream, IChunkVisitor chunkVisitor = null) => Open(stream, new PngOpenerSettings
        {
            ChunkVisitor = chunkVisitor
        });

        public static Png Open(Stream stream, PngOpenerSettings settings)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                var validHeader = HasValidHeader(stream);

                if (!validHeader.IsValid)
                {
                    throw new ArgumentException($"The provided stream did not start with the PNG header. Got {validHeader}.");
                }

                var crc = new byte[4];
                var imageHeader = ReadImageHeader(stream, crc);


                var hasEncounteredImageEnd = false;

                Palette palette = null;

                using (var output = new MemoryStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        while (TryReadChunkHeader(stream, out var header))
                        {
                            if (hasEncounteredImageEnd)
                            {
                                if (settings?.DisallowTrailingData == true)
                                {
                                    throw new InvalidOperationException($"Found another chunk {header} after already reading the IEND chunk.");
                                }

                                break;
                            }

                            var bytes = new byte[header.Length];
                            var read = stream.Read(bytes, 0, bytes.Length);
                            if (read != bytes.Length)
                            {
                                throw new InvalidOperationException($"Did not read {header.Length} bytes for the {header} header, only found: {read}.");
                            }

                            if (header.IsCritical)
                            {
                                switch (header.Name)
                                {
                                    case "PLTE":
                                        if (header.Length % 3 != 0)
                                        {
                                            throw new InvalidOperationException($"Palette data must be multiple of 3, got {header.Length}.");
                                        }

                                        // Ignore palette data unless the header.ColorType indicates that the image is paletted.
                                        if ((imageHeader.ColorType & ColorType.PaletteUsed) == ColorType.PaletteUsed)
                                        {
                                            palette = new Palette(bytes);
                                        }


                                        break;

                                    case "IDAT":
                                        memoryStream.Write(bytes, 0, bytes.Length);
                                        break;
                                    case "IEND":
                                        hasEncounteredImageEnd = true;
                                        break;
                                    default:
                                        throw new NotSupportedException($"Encountered critical header {header} which was not recognised.");
                                }
                            }
                            else
                            {
                                switch (header.Name)
                                {
                                    case "tRNS":
                                        // Add transparency to palette, if the PLTE chunk has been read.
                                        if (palette != null)
                                        {
                                            palette.SetAlphaValues(bytes);
                                        }
                                        break;
                                }
                            }

                            read = stream.Read(crc, 0, crc.Length);
                            if (read != 4)
                            {
                                throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
                            }

                            var result = (int)Crc32.Calculate(Encoding.ASCII.GetBytes(header.Name), bytes);
                            var crcActual = (crc[0] << 24) + (crc[1] << 16) + (crc[2] << 8) + crc[3];

                            if (result != crcActual)
                            {
                                throw new InvalidOperationException($"CRC calculated {result} did not match file {crcActual} for chunk: {header.Name}.");
                            }

                            settings?.ChunkVisitor?.Visit(stream, imageHeader, header, bytes, crc);

                        }

                        memoryStream.Flush();
                        memoryStream.Seek(2, SeekOrigin.Begin);

                        using (var deflateStream = new Ionic.Zlib.DeflateStream(memoryStream, Ionic.Zlib.CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(output);
                            deflateStream.Close();
                        }

                    }

                    var bytesOut = output.ToArray();

                    var (bytesPerPixel, samplesPerPixel) = Decoder.GetBytesAndSamplesPerPixel(imageHeader);

                    bytesOut = Decoder.Decode(bytesOut, imageHeader, bytesPerPixel, samplesPerPixel);

                    return new Png(imageHeader, new RawPngData(bytesOut, bytesPerPixel, palette, imageHeader), palette?.HasAlphaValues ?? false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open PNG File! " + ex.Message);
            }
            return null;
        }

        private static HeaderValidationResult HasValidHeader(Stream stream)
        {
            return new HeaderValidationResult(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte(),
                stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
        }

        private static bool TryReadChunkHeader(Stream stream, out ChunkHeader chunkHeader)
        {
            chunkHeader = default;

            var position = stream.Position;
            if (!StreamHelper.TryReadHeaderBytes(stream, out var headerBytes))
            {
                return false;
            }

            var length = StreamHelper.ReadBigEndianInt32(headerBytes, 0);

            var name = Encoding.ASCII.GetString(headerBytes, 4, 4);

            chunkHeader = new ChunkHeader(position, length, name);

            return true;
        }

        private static ImageHeader ReadImageHeader(Stream stream, byte[] crc)
        {
            if (!TryReadChunkHeader(stream, out var header))
            {
                throw new ArgumentException("The provided stream did not contain a single chunk.");
            }

            if (header.Name != "IHDR")
            {
                throw new ArgumentException($"The first chunk was not the IHDR chunk: {header}.");
            }

            if (header.Length != 13)
            {
                throw new ArgumentException($"The first chunk did not have a length of 13 bytes: {header}.");
            }

            var ihdrBytes = new byte[13];
            var read = stream.Read(ihdrBytes, 0, ihdrBytes.Length);

            if (read != 13)
            {
                throw new InvalidOperationException($"Did not read 13 bytes for the IHDR, only found: {read}.");
            }

            read = stream.Read(crc, 0, crc.Length);
            if (read != 4)
            {
                throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
            }

            var width = StreamHelper.ReadBigEndianInt32(ihdrBytes, 0);
            var height = StreamHelper.ReadBigEndianInt32(ihdrBytes, 4);

            byte bitDepth = ihdrBytes[8];
            byte colorType = ihdrBytes[9];
            byte compressionMethod = ihdrBytes[10];
            byte filterMethod = ihdrBytes[11];
            byte interlaceMethod = ihdrBytes[12];

            //cosmos hates enums...
            ColorType colorTypeEnum = new ColorType();
            switch (colorType)
            {
                case 0:
                    colorTypeEnum = ColorType.None;
                    break;
                case 1:
                    colorTypeEnum = ColorType.PaletteUsed;
                    break;
                case 2:
                    colorTypeEnum = ColorType.ColorUsed;
                    break;
                case 4:
                    colorTypeEnum = ColorType.AlphaChannelUsed;
                    break;
                case 6:
                    colorTypeEnum = ColorType.GrayscaleWithAlpha;
                    break;
                default:
                    throw new Exception("Unsupported color type! " + colorType);
            }
            CompressionMethod compressionMethodEnum = new CompressionMethod();
            switch (compressionMethod)
            {
                case 0:
                    compressionMethodEnum = CompressionMethod.DeflateWithSlidingWindow;
                    break;
                default:
                    throw new Exception("Unsupported compression method! " + compressionMethod);
            }
            FilterMethod filterMethodEnum = new FilterMethod();
            switch (filterMethod)
            {
                case 0:
                    filterMethodEnum = FilterMethod.AdaptiveFiltering;
                    break;
                default:
                    throw new Exception("Unsupported filter method! " + filterMethod);
            }
            InterlaceMethod interlaceMethodEnum = new InterlaceMethod();
            switch (interlaceMethod)
            {
                case 0:
                    interlaceMethodEnum = InterlaceMethod.None;
                    break;
                case 1:
                    interlaceMethodEnum = InterlaceMethod.Adam7;
                    break;
                default:
                    throw new Exception("Unsupported interlace method! " + interlaceMethod);
            }

            ImageHeader imgheader = new ImageHeader
            {
                Width = width,
                Height = height,
                BitDepth = bitDepth,
                ColorType = colorTypeEnum,
                CompressionMethod = compressionMethodEnum,
                FilterMethod = filterMethodEnum,
                InterlaceMethod = interlaceMethodEnum
            };

            //InitializePermittedBitDepthsManually();
            return imgheader;
        }

        public static void InitializePermittedBitDepthsManually()
        {
            var tempDict = new Dictionary<ColorType, byte[]>();

            try
            {
                // Initialize the dictionary manually using arrays
                tempDict.Add(ColorType.None, new byte[] { 1, 2, 4, 8, 16 });
                tempDict.Add(ColorType.ColorUsed, new byte[] { 8, 16 });
                tempDict.Add(ColorType.PaletteUsed | ColorType.ColorUsed, new byte[] { 1, 2, 4, 8 });
                tempDict.Add(ColorType.AlphaChannelUsed, new byte[] { 8, 16 });
                tempDict.Add(ColorType.AlphaChannelUsed | ColorType.ColorUsed, new byte[] { 8, 16 });

                // Assigning the completed dictionary
                ImageHeader.PermittedBitDepths = tempDict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while initializing PermittedBitDepths: {ex.Message}");
                throw;
            }
        }

    }
}
