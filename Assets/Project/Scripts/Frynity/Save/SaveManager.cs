using Frynity.Saves.Binary;
using UnityEngine;

namespace Frynity.Saves.Binary
{
    public class SaveManager : MonoBehaviour
    {
        [SerializeField] bool _openDiretoryAtStart;

        SettingsData _settings = new SettingsData();
        public SettingsData Settings => _settings; 

        LevelGameData _level = new LevelGameData();
        public LevelGameData Level => _level;

        static SaveManager _instance;
        public static SaveManager Of => _instance;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            SaveSystem.InitDirectories();

            if (_openDiretoryAtStart)
            {
                SaveUtil.OpenDirectory(SaveSystem.DirectoryPath);
            }
        }

        public void LoadSettings()
        {
            if (SaveSystem.LoadDataProfile<SettingsData>("set", SaveSystem.Settings, (x) => { }) != null)
                _settings = SaveSystem.LoadDataProfile<SettingsData>("set", SaveSystem.Settings, (x) => { });
        }

        public void SaveSettings()
        {
            SaveSystem.SaveDataProfile(_settings, "set", SaveSystem.Settings, (x) => { });
        }

        public void LoadGame()
        {
            if (SaveSystem.LoadDataProfile<LevelGameData>("level", SaveSystem.Game, (x) => { }) != null)
                _level = SaveSystem.LoadDataProfile<LevelGameData>("level", SaveSystem.Game, (x) => { });
        }

        public void SaveGame()
        {
            SaveSystem.SaveDataProfile(_level, "level", SaveSystem.Game, (x) => { });
        }
    }
}