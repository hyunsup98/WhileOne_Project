using System.Collections.Generic;
using UnityEngine;

public class DataManager_SFX : MonoBehaviour
{
    [field: SerializeField] public SFXDatabaseSO SFXDatabase { get; private set; }

    public string[] FindWeaponSound(string str)
    {
        List<string> sfxList = new List<string>();

        foreach(var data in SFXDatabase.datas)
        {
            if(data.sfxID.Contains(str))
            {
                sfxList.Add(data.sfxID);
            }
        }

        return sfxList.ToArray();
    }
}
