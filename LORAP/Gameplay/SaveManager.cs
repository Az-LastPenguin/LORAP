using Archipelago.MultiClient.Net.Enums;
using GameSave;
using LORAP.Archipelago;
using LORAP.Playthru;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UI;
using UnityEngine;

namespace LORAP.Gameplay
{
    internal static class SaveManager // TODO: Maybe save version number to make migrating possible in case i change saving?
    {
        internal static string CompressString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return Convert.ToBase64String(mso.ToArray());
            }
        }
        internal static string DecompressString(string data)
        {
            using (var msi = new MemoryStream(Convert.FromBase64String(data)))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        internal static void SaveGame()
        {
            Debug.Log("[LORAP] Saving AP Run");

            // Save Last Session Data
            SaveLastSessionData();

            // Some vanilla shenanigans
            GameSave.SaveManager.Instance._packageIdTable = new Dictionary<string, int>();
            GameSave.SaveManager.Instance._packageIdIndex = 1;

            // Get SaveData
            SaveData saveData = new SaveData();

            saveData.AddData("inventory", InventoryModel.Instance.GetSaveData()); // Combat Pages
            saveData.AddData("bookInventory", BookInventoryModel.Instance.GetSaveData()); // Key Pages
            saveData.AddData("usingBookInventory", DropBookInventoryModel.Instance.GetSaveData()); // Books //GetDropBookData());
            saveData.AddData("deckList", DeckListModel.Instance.GetSaveData()); // Decks
            saveData.AddData("customStorage", LibraryModel.Instance._customStorage.GetSaveData()); // Custom storage for mods(?)
            saveData.AddData("floorData", GetFloorData()); // Floor units
            saveData.AddData("playthrough", PlaythruManager.GetSaveData()); // Playthrough data
            saveData.AddData("itemManager", ItemManager.GetSaveData()); // Item Manager

            // Save package ids (for modded books & other stuff)
            SaveData packageSaveData = new SaveData();
            foreach (KeyValuePair<string, int> item in GameSave.SaveManager.Instance._packageIdTable)
            {
                packageSaveData.AddData(item.Key, new SaveData(item.Value));
            }
            saveData.AddData("packageIdTable", packageSaveData);

            // Save to server
            SessionManager.DataStorage[Scope.Slot, "SaveData"] = CompressString(JsonConvert.SerializeObject(saveData.CustomGetSerializedData()));
        }

        internal static void LoadGame()
        {
            Debug.Log("[LORAP] Loading AP Run");

            // Get the save file
            // Init as empty object if there is no save file yet so that game knows
            SessionManager.DataStorage[Scope.Slot, "SaveData"].Initialize("");

            string CompressedSaveData = SessionManager.DataStorage[Scope.Slot, "SaveData"];

            // If object is empty, it means save file is empty, skip loading
            if (CompressedSaveData == "")
                return;

            // Decompress and Deserealize data
            SaveData SaveData = new SaveData();
            SaveData.CustomLoadFromSerializedData(JsonConvert.DeserializeObject<JToken>(DecompressString(CompressedSaveData)));

            // Load package ids (for modded books & other stuff)
            SaveData packageSaveData = SaveData.GetData("packageIdTable");
            GameSave.SaveManager.Instance._packageIdTableReverse = new Dictionary<int, string>();
            if (packageSaveData != null)
            {
                foreach (KeyValuePair<string, SaveData> item in packageSaveData.GetDictionarySelf())
                {
                    int intSelf = item.Value.GetIntSelf();
                    if (intSelf != 0 && !GameSave.SaveManager.Instance._packageIdTableReverse.ContainsKey(intSelf))
                    {
                        GameSave.SaveManager.Instance._packageIdTableReverse.Add(intSelf, item.Key);
                    }
                }
            }

            // Load everything
            InventoryModel.Instance.LoadFromSaveData(SaveData.GetData("inventory"));
            BookInventoryModel.Instance.LoadFromSaveData(SaveData.GetData("bookInventory"));
            DropBookInventoryModel.Instance.LoadFromSaveData(SaveData.GetData("usingBookInventory"));
            DeckListModel.Instance.LoadFromSaveData(SaveData.GetData("deckList"));
            LibraryModel.Instance._customStorage.LoadFromSaveData(SaveData.GetData("customStorage"));
            PlaythruManager.LoadFromSaveData(SaveData.GetData("playthrough"));
            ItemManager.LoadFromSaveData(SaveData.GetData("itemManager"));

            SaveData data = SaveData.GetData("floorData");
            foreach (LibraryFloorModel floor in LibraryModel.Instance._floorList)
            {
                SaveData _data = data.GetData(SephirahName.GetSephirahNameByType(floor.Sephirah));
                if (_data == null) continue;

                int num = 0;
                foreach (SaveData dat in _data)
                {
                    floor._unitDataList[num].LoadFromSaveData(dat);
                    num++;
                }
            }

            // if (PlaythruManager.BinahUnlocked)
            //     PlaythruManager.UnlockBinah(true);
            // 
            // if (PlaythruManager.BlackSilenceUnlocked)
            //     PlaythruManager.UnlockBlackSilence(true);
        }


        /*private static SaveData GetDropBookData()
        {
            SaveData saveData = new SaveData();
            SaveData saveData2 = new SaveData();
            foreach (OwnDropBookModel book in DropBookInventoryModel.Instance._bookList)
            {
                SaveData saveData3 = new SaveData();
                saveData3.AddData("id", new SaveData(book.XmlInfo.id.id));
                saveData3.AddData("pkg", new SaveData(book.XmlInfo.id.packageId));
                saveData3.AddData("num", new SaveData(book.num));
                saveData2.AddToList(saveData3);
            }
            saveData.AddData("bookList", saveData2);

            return saveData;
        }*/

        private static SaveData GetFloorData()
        {
            SaveData saveData = new SaveData();
            foreach (LibraryFloorModel floor in LibraryModel.Instance._floorList)
            {
                SaveData unitInfoData = new SaveData();
                foreach (UnitDataModel unitData in floor._unitDataList)
                {
                    unitInfoData.AddToList(unitData.GetSaveData());
                }

                saveData.AddData(SephirahName.GetSephirahNameByType(floor.Sephirah), unitInfoData);
            }

            return saveData;
        }


        internal static SessionData LoadLastSessionData() // TODO: Possibly make shorter 
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            if (!File.Exists($"{Application.persistentDataPath}/Archipelago/LastSession"))
            {
                SessionManager.sessionData = new SessionData();
                return SessionManager.sessionData;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try
            {
                SessionData Data;
                using (FileStream fileStream = File.Open($"{Application.persistentDataPath}/Archipelago/LastSession", FileMode.Open))
                {
                    Data = binaryFormatter.Deserialize(fileStream) as SessionData;
                }
                if (Data == null)
                {
                    throw new Exception();
                }

                return Data;
            }
            catch (Exception)
            {
                return new SessionData() { IP = "", SlotName = "", Password = "", Progress = 0f };
            }
        }

        internal static void SaveLastSessionData() // TODO: Make this shorter too
        {
            if (SessionManager.sessionData == null)
                SessionManager.sessionData = new SessionData();

            int allLocations = LocationManager.AllLocations.Count;
            int checkedLocations = LocationManager.CheckedLocations.Count;
            SessionManager.sessionData.Progress = allLocations > 0 ? (float)checkedLocations / allLocations : SessionManager.sessionData.Progress;
            SessionManager.sessionData.Password = "";

            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            using (FileStream serializationStream = File.Create($"{Application.persistentDataPath}/Archipelago/LastSession"))
            {
                new BinaryFormatter().Serialize(serializationStream, SessionManager.sessionData);
            }
        }

    }

    internal static class SaveDataExtension
    {
        internal static object CustomGetSerializedData(this SaveData saveData)
        {
            switch (saveData._type)
            {
                case SaveDataType.Int:
                    return saveData._pdi;
                case SaveDataType.UnsignedLong:
                    return saveData._pdul;
                case SaveDataType.String:
                    return saveData._pds;
                case SaveDataType.Dictionary:
                    Dictionary<string, object> dict = new Dictionary<string, object>();

                    foreach (var item in saveData._dic)
                    {
                        dict[item.Key] = item.Value.CustomGetSerializedData();
                    }

                    return dict;
                case SaveDataType.List:
                    List<object> list = new List<object>();

                    foreach (SaveData item in saveData._list)
                    {
                        list.Add(item.CustomGetSerializedData());
                    }

                    return list;
                default:
                    return null;
            }
        }

        internal static void CustomLoadFromSerializedData(this SaveData saveData, JToken serialized)
        {
            if (serialized.Type == JTokenType.Object)
            {
                saveData._type = SaveDataType.Dictionary;

                saveData._dic = (serialized as JObject).ToObject<Dictionary<string, JToken>>().ToDictionary(p => p.Key, p =>
                {
                    SaveData pairSaveData = new SaveData();
                    pairSaveData.CustomLoadFromSerializedData(p.Value);
                    return pairSaveData;
                });
                return;
            }
            else if (serialized.Type == JTokenType.Array)
            {
                saveData._type = SaveDataType.List;

                saveData._list = (serialized as JArray).Select(d =>
                {
                    SaveData listSaveData = new SaveData();
                    listSaveData.CustomLoadFromSerializedData(d);
                    return listSaveData;
                }).ToList();
            }
            else if (serialized.Type == JTokenType.Integer)
            {
                saveData._type = SaveDataType.Int;
                saveData._pdi = serialized.Value<int>();
            }
            /*else if (serialized is ulong)
            {
                saveData._type = SaveDataType.UnsignedLong;
                saveData._pdul = (ulong)serialized;
            }*/
            else if (serialized.Type == JTokenType.String)
            {
                saveData._type = SaveDataType.String;
                saveData._pds = serialized.Value<string>();
            }
            else if (serialized.Type == JTokenType.Null)
            {
                saveData._type = SaveDataType.None;
            }
            else
            {
                Debug.LogError("invalid SaveData");
            }
        }
    }
}
