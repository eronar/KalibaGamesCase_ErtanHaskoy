using UnityEngine;
using System.Collections;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.UI;
using System.Collections.Generic;

[CustomEditor(typeof(GridCreator))]
public class customButton : Editor
{

    private bool refresh_gui = false;

    public enum ObjectTypes
    {
        Empty = -1,
        ObstacleSize1 = 0,
        ObstacleSize2 = 1,
        Driver = 2,
        CarSize2 = 3,
        CarSize3 = 4
    }
    public ObjectTypes objectTypes;

    public enum ObjectColors
    {
        Grey = 0,
        Black = 1,
        Blue = 2,
        Green = 3,
        Red = 4,
        Yellow = 5,

    }
    public ObjectColors objectColors;

    public enum ObjectRotation
    {
        Down = 0,
        Left = 1,
        Up = 2,
        Right = 3,

    }
    public ObjectRotation objectRotation;


    public override bool RequiresConstantRepaint()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        GridCreator myScript = (GridCreator)target;
        if (GUILayout.Button("Create Level"))
        {
            myScript.CreateGrid();
        }

        if (GUILayout.Button("Clear Level"))
        {
            myScript.ClearLevel();
        }

        objectTypes = (ObjectTypes)EditorGUILayout.EnumPopup("Object Type To Spawn:", objectTypes);
        objectColors = (ObjectColors)EditorGUILayout.EnumPopup("Object Color:", objectColors);
        objectRotation = (ObjectRotation)EditorGUILayout.EnumPopup("Object Facing:", objectRotation);

        int gridsize_x = myScript.grid_x;
        int gridsize_y = myScript.grid_y;

        for (int i = 0, y = gridsize_y - 1; y >= 0; y = y - 1)
        {
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridsize_x; x = x + 1)
            {
                //Get current grid info before creating button color and texts
                string grid_type_name = myScript.GetCurrentGridButtonObjectType(x, y);
                Color grid_color = myScript.GetCurrentGridButtonColor(x, y);

                GUI.backgroundColor = grid_color;
                var created_button = GUILayout.Button(grid_type_name, GUILayout.Width(50), GUILayout.Height(50));
                //var created_button = GUILayout.Button(x.ToString() + "," + y.ToString(), GUILayout.Width(50), GUILayout.Height(50));
                if (created_button)
                {
                    GridButtonPressed();
                    myScript.UIGridButtonClicked(x, y);
                }
                i++;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
        }
    }

    int UseThisTypeOfObject(ObjectTypes objectTypes)
    {
        switch (objectTypes)
        {
            case ObjectTypes.Empty:
                //Debug.Log("-1");
                return -1;
            case ObjectTypes.ObstacleSize1:
                //Debug.Log("0");
                return 0;
            case ObjectTypes.ObstacleSize2:
                //Debug.Log("1");
                return 1;
            case ObjectTypes.Driver:
                //Debug.Log("2");
                return 2;
            case ObjectTypes.CarSize2:
                //Debug.Log("3");
                return 3;
            case ObjectTypes.CarSize3:
                //Debug.Log("4");
                return 4;
            default:
                Debug.LogError("No Type Selected");
                return -5;
        }
    }

    int UseThisTypeColor(ObjectColors objectColors)
    {
        switch (objectColors)
        {
            case ObjectColors.Grey:
                //Debug.Log("0Blue");
                return 0;
            case ObjectColors.Black:
                //Debug.Log("0Blue");
                return 1;
            case ObjectColors.Blue:
                //Debug.Log("0Blue");
                return 2;
            case ObjectColors.Green:
                //Debug.Log("1Green");
                return 3;
            case ObjectColors.Red:
                //Debug.Log("2Red");
                return 4;
            case ObjectColors.Yellow:
                //Debug.Log("3Yellow");
                return 5;
            default:
                Debug.LogError("No Color Selected");
                return -1;
        }
    }

    int UseThisFacing(ObjectRotation objectColors)
    {
        switch (objectColors)
        {
            case ObjectRotation.Down:
                //Debug.Log("0Down");
                return 0;
            case ObjectRotation.Left:
               // Debug.Log("1Left");
                return 1;
            case ObjectRotation.Up:
                //Debug.Log("2Up");
                return 2;
            case ObjectRotation.Right:
                //Debug.Log("3Right");
                return 3;
            default:
                Debug.LogError("No Rotation Selected");
                return -1;
        }
    }

    public void GridButtonPressed()
    {
        GridCreator myScript = (GridCreator)target;
        int type = UseThisTypeOfObject(objectTypes);
        int color = UseThisTypeColor(objectColors);
        int rotation = UseThisFacing(objectRotation);

        if (type == -1) //empty (empty slots can only have default grey color)
        {
            color = 0;
        }
        else if (type == 0 | type == 1) //obstacle (obstacle slots can only have default black color)
        {
            color = 1;
        }

        myScript.GetCurrentSelectedObjectOptions(type, color, rotation);
    }

}