using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CityGeneratorWindow : EditorWindow
{
    public CityGenInfo cityGenInfo;

    [MenuItem("Window/CityGeneratorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<CityGeneratorWindow>();
    }

    private void OnGUI()
    {
        cityGenInfo = EditorGUILayout.ObjectField(cityGenInfo, typeof(CityGenInfo), true) as CityGenInfo;
        if (GUILayout.Button("Generate City"))
        {
            BuildCity();
        }
    }

    private void BuildCity()
    {
        GameObject city = new GameObject("City");
        uint rows = (uint)Mathf.Abs((cityGenInfo.startPos.z - cityGenInfo.endPos.z) / (cityGenInfo.buildingWidth + cityGenInfo.streetWidth) - cityGenInfo.streetWidth);
        uint columns = (uint)Mathf.Abs((cityGenInfo.startPos.x - cityGenInfo.endPos.x) / (cityGenInfo.buildingWidth + cityGenInfo.streetWidth) - cityGenInfo.streetWidth);
        Vector3 buildingPlacementCursor = cityGenInfo.startPos;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject newBuilding = Instantiate(cityGenInfo.buildingPrefab, buildingPlacementCursor, Quaternion.identity);
                newBuilding.transform.localScale = new Vector3(cityGenInfo.buildingWidth, Random.Range(cityGenInfo.buildingMinHeight, cityGenInfo.buildingMaxHeight), cityGenInfo.buildingWidth);
                newBuilding.transform.parent = city.transform;
                buildingPlacementCursor += new Vector3(cityGenInfo.buildingWidth + cityGenInfo.streetWidth, 0.0f, 0.0f);
            }
            buildingPlacementCursor = cityGenInfo.startPos + new Vector3(0.0f, 0.0f, (cityGenInfo.buildingWidth + cityGenInfo.streetWidth) * -(i + 1));
        }
    }
}
