using UnityEngine;

//公扁 郴备档 包府

public class WeaponInstance : MonoBehaviour
{
    public WeaponData data;
    public int currentDurability;

    public WeaponInstance(WeaponData data)
    {
        this.data = data;
        currentDurability = data.maxDurability;
    }

    public bool IsBroken => currentDurability <= 0;

    public void ConsumeDurability(int amount = 1)
    {
        if (IsBroken) return;
        currentDurability -= amount;
        if (currentDurability < 0) currentDurability = 0;
    }
}

