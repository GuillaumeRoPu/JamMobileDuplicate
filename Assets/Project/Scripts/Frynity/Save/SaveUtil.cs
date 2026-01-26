
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Frynity.Saves.Binary
{
    public static class SaveUtil
    {
        public static void OpenDirectory(string path)
        {
            if (Directory.Exists(path))
                Process.Start(path);
        }

        public static void SaveFile(this object data, string path, Action<bool> callback)
        {
            if (!(data is ISaveData))
            {
                callback?.Invoke(false);
                return;
            }    

            try
            {
                FileStream stream = new(path, FileMode.Create);

                new BinaryFormatter().Serialize(stream, data);
                stream.Close();

                callback?.Invoke(true);
            }
            catch
            {
                callback?.Invoke(false);
            }

        }

        public static object LoadFile(this string path, Action<bool> callback)
        {
            object data = null;
            if (!File.Exists(path))
            {
                callback?.Invoke(false);
                return data;
            }

            try
            {
                FileStream stream = new(path, FileMode.Open);

                data = new BinaryFormatter().Deserialize(stream);
                stream.Close();

                callback?.Invoke(true);
            }
            catch
            {
                callback?.Invoke(false);
            }

            return data;

        }

    }
}
