using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loadout/Loadout Manager")]
public class LoadoutManager : ScriptableObject
{
    public Loadout[] Loadouts;

    public int GetRandomLoadout(out Loadout loudout)
    {
        int loadoutIndex = Random.Range(0, Loadouts.Length);

        loudout = Loadouts[loadoutIndex];

        return loadoutIndex;
    }
}
