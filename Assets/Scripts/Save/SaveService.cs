using System;
using UnityEngine;

    public class SaveService
    {
        private const string PrefsKey = "factory.save.v1";

        public bool HasSave() => PlayerPrefs.HasKey(PrefsKey);

        public void Save(GameSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string json = JsonUtility.ToJson(data);

            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        public GameSaveData Load()
        {
            if (!HasSave()) return null;

            string json = PlayerPrefs.GetString(PrefsKey);
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Corrupted save, ignoring. {e.Message}");
                return null;
            }
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }
    }
    