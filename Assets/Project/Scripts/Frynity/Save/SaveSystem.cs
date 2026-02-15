using Frynity.Saves;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using UnityEngine;

namespace Frynity.Saves.Binary
{
    public static class SaveSystem
    {
        public const string Profil = "Profil";
        public const string Game = "Game";
        public const string Settings = "Settings";

        const string extension = ".save";
        const string directoryName = "/Saves";
        static string fileName = "";

        static string DirectoryPath => Application.persistentDataPath + directoryName;
        static string ProfilDirectoryPath(string profilName) => $"{DirectoryPath}/{profilName}";
        static string SavePath(string path) => path + "/" + fileName + extension;

        public static void InitDirectories()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            SaveUtil.OpenDirectory(DirectoryPath);
        }

        public static void CreateProfile(string profilName)
        {
            if (!Directory.Exists(ProfilDirectoryPath(profilName)))
            {
                Directory.CreateDirectory(ProfilDirectoryPath(profilName));
            }
        }

        public static void SaveDataProfile(ISaveData data, string dataName, string profilName, Action<bool> callback)
        {
            CreateProfile(profilName);

            fileName = dataName;

            data.SaveFile(SavePath(ProfilDirectoryPath(profilName)), callback);
        }

       
        public static T LoadDataProfile<T>(string dataName, string profilName, Action<bool> callback) where T : ISaveData
        {
            fileName = dataName;

            object data = SavePath(ProfilDirectoryPath(profilName)).LoadFile(callback);

            return data == null ? default : (T)data;
        }

        public static T ResetDataProfile<T>(object data, string dataName, string profilName, Action<bool> callback) where T : ISaveData
        {
            fileName = dataName;

            if (File.Exists(ProfilDirectoryPath(profilName)))
            {
                data = new();
                (data as ISaveData).SaveFile(SavePath(ProfilDirectoryPath(profilName)), callback);
            }
            
            return (T)data;
        }

        public static void DeleteDataProfile(string dataName, string profilName, Action<bool> callback)
        {
            fileName = dataName;
            
            if (Directory.Exists(ProfilDirectoryPath(profilName)))
                Directory.Delete(ProfilDirectoryPath(profilName), true);
        }
    }
}
