using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO; // For reading file contents

//! Written fully by ChatGPT, this should be right
[ScriptedImporter(1, "xyz")] // Register this importer for the .xyz file extension
public class XYZImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // Get the path of the .xyz file
        var filePath = ctx.assetPath;

        // Read the content of the .xyz file
        string fileContent;
        try
        {
            fileContent = File.ReadAllText(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read .xyz file: {e.Message}");
            fileContent = "Error reading file.";
        }

        // Create a TextAsset to store the text data
        TextAsset textData = new TextAsset(fileContent);

        // Add the TextAsset to the AssetDatabase so it appears in Unity
        ctx.AddObjectToAsset("main", textData); // "main" is the identifier for this asset
        ctx.SetMainObject(textData); // Set the first/main object in the imported asset

        Debug.Log($"Successfully imported .xyz file: {filePath}");
    }
}