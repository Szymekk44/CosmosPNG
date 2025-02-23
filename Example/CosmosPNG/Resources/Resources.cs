using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosPNG
{
    public static class Resources
    {
        [ManifestResourceStream(ResourceName = "CosmosPNG.Resources.CosmosLogo.png")] public static byte[] CosmosLogo;
        [ManifestResourceStream(ResourceName = "CosmosPNG.Resources.WallpaperMountain.png")] public static byte[] WallpaperMountain;
    }
}
