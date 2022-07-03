using UnityEngine;
namespace Endciv
{
    public class CombineCharacterSet : ScriptableObject
    {
        public bool MaleOrFemale;

        public Texture2D bodyTexture;
        public Texture2D headTexture;
        public Texture2D[] headHairTextures;
        public Texture2D[] shirtsTexture;
        public Texture2D[] pantsTexture;
        public Texture2D hairTexture;
        public Texture2D miscTexture;

        public Mesh bodyMesh;
        public Mesh headMesh;
        public Mesh hairMesh;
        public Mesh miscMesh;
    }
}