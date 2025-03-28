
using UnityEngine;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using System;

public class TransmuteManager : MonoBehaviour
{
    public static TransmuteManager _instance { get; private set; }
    [SerializeField] string fileName = string.Empty;
    [SerializeField] public static FamiliarItem tempFamiliarItem { get; private set; }
    [ReadOnly,SerializeField] string filePath;

    private void Awake()
    {
        _instance = this;
        filePath = Path.Combine(Application.persistentDataPath, $"{((fileName == string.Empty) ? "FamiliarItemsData_LOCAL" : fileName)}.json");
    }

    // Method to save FamiliarItem to JSON file
    private void SaveFamiliarItemToJson(FamiliarItem item)
    {
        string jsonData = JsonUtility.ToJson(item);
        //string filePath = Path.Combine(Application.persistentDataPath, "FamiliarItems.json");
#if UNITY_EDITOR
        Debug.Log($"writing at : {filePath}");
#endif
        if (File.Exists(filePath))
        {
            // Read existing content
            string existingContent = File.ReadAllText(filePath);

            if (existingContent.Length > 2)
            {
                existingContent = existingContent.TrimEnd(']');
                existingContent += ",";
            }
            else
            {
                existingContent = existingContent.TrimEnd(']');
            }

            string newContent = existingContent + jsonData + "]";
            File.WriteAllText(filePath, newContent);
        }
        else
        {
            // Create new file with the item as first element in an array
            File.WriteAllText(filePath, "[" + jsonData + "]");
        }
    }

    /// <summary>
    /// Creates a new FamiliarItem with the given name and icon ID, using statistical data from Table_Control_Panel.
    /// </summary>
    public void TransmuteMakeNew(string name = "No name", string iconID = "defaultFamiliarIcon" )
    {
        //   FamiliarItem tempFamiliarItem = new FamiliarItem("Player ID here","");
        //Debug.LogWarning(string.Join('_', Table_Control_Panel.instance.numbers_list.ToArray()));
        tempFamiliarItem = new FamiliarItem(0, name, string.Join('_', Table_Control_Panel.instance.numbers_list.ToArray()), DateTime.Now.ToString(), iconID, Table_Control_Panel.instance.mean, Table_Control_Panel.instance.sd, Table_Control_Panel.instance.skew);
    }

    public void TransmuteEraseNew()
    {
        if (tempFamiliarItem != null)
        {
            tempFamiliarItem = null;
            return;
        }
        //Anything else??

    }

    public void TransmuteAddNew()
    {
        if (tempFamiliarItem != null)
        {
            SaveFamiliarItemToJson(tempFamiliarItem);
#if UNITY_EDITOR
            Debug.Log("Saved FItem");
#endif
            return;
        }
    }
}


