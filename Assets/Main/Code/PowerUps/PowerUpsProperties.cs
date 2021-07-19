using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: make this a scriptable or somethin
public class PowerUpsProperties : MonoBehaviour
{
    //TODO: give this a seperate file perhaps?
    [System.Serializable]
    public struct PowerUpPropertiesBlock
    {
        public PowerUp.Type type;
        public Sprite icon;
    }

    [SerializeField] private PowerUpPropertiesBlock[] powerUpPropertiesBlocks;
    //public static PowerUpPropertiesBlock[] PowerUpPropertiesBlocks => instance.powerUpPropertiesBlocks;

    private static PowerUpsProperties instance;
    private void Awake()
    {
        instance = this;
        SortPowerUpPropertiesBlocks();
    }

    private void SortPowerUpPropertiesBlocks()
    {
        PowerUpPropertiesBlock[] sortedPowerUpPropertiesBlocks = new PowerUpPropertiesBlock[(int)PowerUp.Type.Length];

        for (int i = 0; i < powerUpPropertiesBlocks.Length; i++)
        {
            int newIndex = (int)powerUpPropertiesBlocks[i].type;
            sortedPowerUpPropertiesBlocks[newIndex] = powerUpPropertiesBlocks[i];
        }
        powerUpPropertiesBlocks = sortedPowerUpPropertiesBlocks;
    }

    public static Sprite GetIcon(PowerUp.Type powerUpType)
    {
        return instance.powerUpPropertiesBlocks[(int)powerUpType].icon;
    }
}
