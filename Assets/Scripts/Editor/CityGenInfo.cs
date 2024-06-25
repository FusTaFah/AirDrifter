using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AirDrifter Assets/City Gen Info")]
public class CityGenInfo : ScriptableObject
{
    public float buildingWidth;
    public float streetWidth;
    public float buildingMinHeight;
    public float buildingMaxHeight;
    public Vector3 startPos;
    public Vector3 endPos;
    public GameObject buildingPrefab;
}
