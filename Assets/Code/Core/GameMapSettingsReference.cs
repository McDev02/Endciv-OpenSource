using System.Linq;
using UnityEngine;

namespace Endciv
{
    public class GameMapSettingsReference : ScriptableObject
    {
        [SerializeField] public GameMapSettings[] gameMapSettings;

        public GameMapSettings GetSettingsByID(string id)
        {
            var settings = gameMapSettings.FirstOrDefault(x => x.ID == id);
            if (settings == null)
                return null;
            return settings.Clone();
        }
    }
}

