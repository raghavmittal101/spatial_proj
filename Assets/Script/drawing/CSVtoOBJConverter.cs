using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVtoOBJConverter : MonoBehaviour
{

    public static void Convert(string inputFilePath, string outputFilePath)
    {

        List<string> objLines = ConvertCSVtoOBJ(inputFilePath);
        File.WriteAllLines(outputFilePath, objLines);

         Debug.Log("OBJ file generated: " + outputFilePath);
    }

    static List<string> ConvertCSVtoOBJ(string filePath)
    {
        List<string> objLines = new List<string>();
        List<string> vertices = new List<string>();
        List<string> lines = new List<string>();
        Dictionary<int, List<int>> idToVertexIndices = new Dictionary<int, List<int>>();
        int vertexIndex = 1;

        foreach (string line in File.ReadLines(filePath))
        {
            if (line.StartsWith("sketch_id")) continue; // Skip header

            string[] parts = line.Split(',');
            Debug.Log(line);
            int id = int.Parse(parts[0]);
            float x = float.Parse(parts[1]);
            float y = float.Parse(parts[2]);
            float z = float.Parse(parts[3]);

            vertices.Add($"v {x} {y} {z}");

            if (!idToVertexIndices.ContainsKey(id))
                idToVertexIndices[id] = new List<int>();

            idToVertexIndices[id].Add(vertexIndex);
            vertexIndex++;
        }

        foreach (var kvp in idToVertexIndices)
        {
            List<int> indices = kvp.Value;
            if (indices.Count == 1)
            {
                lines.Add($"l {indices[0]} {indices[0]}"); // Self-loop if only one vertex
            }
            else
            {
                for (int i = 0; i < indices.Count - 1; i++)
                {
                    lines.Add($"l {indices[i]} {indices[i + 1]}");
                }
            }
            lines.Add(""); // Blank line to separate groups
        }

        objLines.AddRange(vertices);
        objLines.AddRange(lines);

        return objLines;
    }
}
