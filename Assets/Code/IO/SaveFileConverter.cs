using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
    public static class SaveFileConverter
	{
		public const int currentSaveVersion = 6;
		public const int lowestSupportedSaveVersion = 6;
		public delegate SavegameDataBase ConversionDelegate(string filePath);
        public static Dictionary<int, ConversionDelegate> converters = new Dictionary<int, ConversionDelegate>();
    
        public static SavegameDataBase UpdateSaveFile(string filePath)
        {
            int fileVersion = 0; //check file to get version
            if(fileVersion == currentSaveVersion)
            {
                Debug.Log("File already up to date.");
                //return existing save file

            }
            return null;
        }
    }
}