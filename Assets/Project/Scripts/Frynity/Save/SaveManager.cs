
using Frynity.Saves.Binary;
using UnityEngine;

namespace Frynity.Saves.Binary
{
    public class SaveManager : MonoBehaviour
    {

        SettingSave _settings = new SettingSave();
        public SettingSave Settings => _settings;

        static SaveManager _instance;
        public static SaveManager Of => _instance;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            SaveSystem.InitDirectories();
        }


        public void LoadSettings()
        {
            if (SaveSystem.LoadDataProfile<SettingSave>("set", SaveSystem.Settings, (x) => { }) != null)
                _settings = SaveSystem.LoadDataProfile<SettingSave>("set", SaveSystem.Settings, (x) => { });
        }

        public void SaveSettings()
        {
            SaveSystem.SaveDataProfile(_settings, "set", SaveSystem.Settings, (x) => { });
        }
    }
}
