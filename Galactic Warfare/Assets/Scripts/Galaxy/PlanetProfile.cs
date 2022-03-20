using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Profile/Planet", fileName = "New Planet")]
public class PlanetProfile : ScriptableObject
{
    public string PlanetName = "Default Name";
    public int VictoryUnits = 800;
    public int BonusUnits = 50;
}