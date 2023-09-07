using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class csvReader : MonoBehaviour
{
    public TextAsset textAssetData;
    public ObjectInformation[] objectInformation;

    public class ObjectInformation
    {
        // all columns for file
        public string name;

        public int positionX;
        public int positionY;
        public int positionZ;

        public int rotationX;
        public int rotationY;
        public int rotationZ;
    }

    // Start is called before the first frame update
    void Start()
    {
        readCSV();
        //writeCSV("Tree2,1,2,3,4,5,6");
    }

    void readCSV()
    {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
        int colNum = 7;

        // table size is data length divided by number of columns, then -1 to ignore the first row of headings
        int tableSize = data.Length / colNum - 1;
        objectInformation = new ObjectInformation[tableSize];

        for (int i=0; i < tableSize; i++)
        {
            objectInformation[i] = new ObjectInformation();
            objectInformation[i].name = data[colNum * (i + 1)];

            objectInformation[i].positionX = Int32.Parse(data[colNum * (i + 1) + 1]);
            objectInformation[i].positionY = Int32.Parse(data[colNum * (i + 1) + 2]);
            objectInformation[i].positionZ = Int32.Parse(data[colNum * (i + 1) + 3]);

            objectInformation[i].rotationX = Int32.Parse(data[colNum * (i + 1) + 4]);
            objectInformation[i].rotationY = Int32.Parse(data[colNum * (i + 1) + 5]);
            objectInformation[i].rotationZ = Int32.Parse(data[colNum * (i + 1) + 6]);
        }
    }

    void writeCSV(string data)
    {
        string path = "Assets/Files/objectLocationsCSV 1.csv";
        using (var writer = new StreamWriter(path))
        {
            writer.WriteLine(data);
        } 
    }

}
