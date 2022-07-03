using UnityEngine;
using System.Collections;

namespace Endciv
{
    public abstract class TerrainGenerator
    {
        public TerrainGenerationSurfaceExtended heightmap;
        public LoadingState loadingState;

        public abstract void TerrainGeneration(TerrainSettings settings);
        
        public static void ExportTexture(string name, int size, Color32[] colors)
        {
            if (System.IO.File.Exists(name + ".png"))
            {
                System.IO.File.Delete(name + ".old.png");
                System.IO.FileInfo inf = new System.IO.FileInfo(name + ".png");
                inf.MoveTo(name + ".old.png");
            }
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            tex.SetPixels32(colors);
            System.IO.File.WriteAllBytes(name + ".png", tex.EncodeToPNG());
            GameObject.DestroyImmediate(tex);
        }
    }
}