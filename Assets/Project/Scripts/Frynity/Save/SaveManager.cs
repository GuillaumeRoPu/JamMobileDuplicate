
using Frynity.Saves.Binary;
using UnityEngine;

namespace Frynity.Saves.Binary
{
    public class SaveManager : MonoBehaviour
    {

        static SaveManager _instance;
        public static SaveManager Of => _instance;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            SaveSystem.InitDirectories();
        }
    }
}
