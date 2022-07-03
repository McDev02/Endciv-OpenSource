using UnityEngine;
namespace Endciv
{
    public class HumanColorSettings : ScriptableObject
    {
        [SerializeField] Color[] SkinTones;
        [SerializeField] Color[] HairTones;

        public Color GetRandomSkinTone()
        {
            return SkinTones[CivRandom.Range(0, SkinTones.Length )];
        }
        public Color GetRandomHairTone()
        {
            return HairTones[CivRandom.Range(0, HairTones.Length )];
        }
    }
}