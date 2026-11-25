using System;
using System.IO;
using Game.Foundation;
using UnityEngine;

namespace Game.Platform.Android
{
    public class LocalJsonSaveService : ISaveService
    {
        private readonly string _savePath;
        private readonly string _tempPath;

        public LocalJsonSaveService(string fileName = "save.json")
        {
            _savePath = Path.Combine(Application.persistentDataPath, fileName);
            _tempPath = _savePath + ".tmp";
        }

        public bool HasSave() => File.Exists(_savePath);

        public GameSaveData Load()
        {
            if (!File.Exists(_savePath))
                return null;

            try
            {
                string json = File.ReadAllText(_savePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonSaveService] Failed to load save: {ex}");
                return null;
            }
        }

        public void Save(GameSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(_tempPath, json);

                if (File.Exists(_savePath))
                    File.Delete(_savePath);

                File.Move(_tempPath, _savePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalJsonSaveService] Failed to save: {ex}");
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(_savePath))
                File.Delete(_savePath);
        }
    }
}