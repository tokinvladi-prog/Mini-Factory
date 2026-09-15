using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public float balance;
    public long lastSaveTime;
    public List<MachineSaveData> machines = new();
    public float boostTimeRemaining;
}
