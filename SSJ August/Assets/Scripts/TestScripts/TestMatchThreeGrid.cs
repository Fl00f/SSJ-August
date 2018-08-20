using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = System.Random;
using System.Linq;

public class TestMatchThreeGrid : MonoBehaviour
{
    public static int RowCutOff;
    public Transform[] gridColumns;
    private int ColumnLength => gridColumns.Length;

    public Vector2[,] gridImagePositions;
    public Vector2[,] translationImageStartPositions;

    public float translationTime => doingSwap ? swapTime : normalTranslationTime;

    public float normalTranslationTime;
    public float swapTime;
    public bool doingSwap;

    private Action OnTranslationEnd;

    #region Debug

    public bool doImageTranslations;
    public float translationStartTime;

    #endregion

    // Use this for initialization
    void Start()
    {
        RowCutOff = 10;
        SetInitialGridPositions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //reset
            ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest>();

            foreach (var objectDragTest in allObjects)
            {
                objectDragTest.IsConnected = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest>();

            foreach (var objectDragTest in allObjects.Where(a => a.IsConnected))
            {
                objectDragTest.transform.SetSiblingIndex(0);
                Vector3 pos = objectDragTest.transform.position;
                pos.y = 1000;
                objectDragTest.transform.position = pos;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            translationImageStartPositions = getCurrentImagePositions();
            translationStartTime = Time.time;
            doImageTranslations = true;
        }

        if (doImageTranslations && translationImageStartPositions != null)
        {
            float percentage = (Time.time - translationStartTime) / translationTime;
            float percentageClamp = Mathf.Clamp(percentage, 0, 1);
            translateImagesToGridPositions(percentageClamp);

            if (percentageClamp >= 1)
            {
                doImageTranslations = false;
                translationImageStartPositions = null;
                doingSwap = false;
                OnTranslationEnd?.Invoke();
            }
        }
    }

    private void translateImagesToGridPositions(float translationPercentage)
    {
        for (int i = 0; i < ColumnLength; i++)
        {
            for (int j = 0; j < gridColumns[0].childCount; j++)
            {
                Transform imageTransform = gridColumns[i].GetChild(j);
                
                Vector3 lerpPos = Vector3.Lerp(translationImageStartPositions[i, j], gridImagePositions[i, j],
                    translationPercentage);

                imageTransform.position = lerpPos;
            }
        }
    }

    private Vector2[,] getCurrentImagePositions()
    {
        Vector2[,] currentImageStartPositions = new Vector2[ColumnLength, gridColumns[0].childCount];

        for (int i = 0; i < ColumnLength; i++)
        {
            for (int j = 0; j < gridColumns[0].childCount; j++)
            {
                Vector3 pos = gridColumns[i].GetChild(j).position;

                currentImageStartPositions[i, j] = new Vector2Int((int) pos.x, (int) pos.y);
            }
        }

        return currentImageStartPositions;
    }

    private void SetInitialGridPositions()
    {
        //using the pre established positions from prefab, record initial positions into grid
        gridImagePositions = new Vector2[ColumnLength, gridColumns[0].childCount];
        int tileTypeMax = Enum.GetValues(typeof(TileType)).Length;
        Random ran = new Random();
        ;
        for (int i = 0; i < ColumnLength; i++)
        {
            for (int j = 0; j < gridColumns[0].childCount; j++)
            {
                ObjectDragTest dragableObj = gridColumns[i].GetChild(j).GetComponent<ObjectDragTest>();
                dragableObj.OnSwap += handleTileSwap;
                dragableObj.TileType = (TileType) ran.Next(0, tileTypeMax);

                Vector3 pos = gridColumns[i].GetChild(j).position;

                gridImagePositions[i, j] = new Vector2(pos.x, pos.y);
            }
        }
    }

    private void handleTileSwap(ObjectDragTest a, ObjectDragTest b)
    {
        doingSwap = true;
        translationImageStartPositions = getCurrentImagePositions();
        translationStartTime = Time.time;
        doImageTranslations = true;
        OnTranslationEnd += () => checkAfterSwap(a, b);
    }


    private void checkAfterSwap(ObjectDragTest a, ObjectDragTest b)
    {
        checkObject(a);
        checkObject(b);

        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest>();

        var connectedThings = allObjects.Where(obj => obj.IsConnected).ToArray();

        Debug.Log(connectedThings.Length);


        OnTranslationEnd = null;
    }

    private void checkObject(ObjectDragTest a)
    {
        bool isAConected = false;

        if (a.North != null && !a.North.IsConnected && a.North.TileType == a.TileType)
        {
            isAConected = true;
            a.North.IsConnected = true;
            checkObject(a.North);
        }

        if (a.East != null && !a.East.IsConnected && a.East.TileType == a.TileType)
        {
            isAConected = true;
            a.East.IsConnected = true;
            checkObject(a.East);
        }

        if (a.South != null && !a.South.IsConnected && a.South.TileType == a.TileType)
        {
            isAConected = true;
            a.South.IsConnected = true;
            checkObject(a.South);
        }

        if (a.West != null && !a.West.IsConnected && a.West.TileType == a.TileType)
        {
            isAConected = true;
            a.West.IsConnected = true;
            checkObject(a.West);
        }

        a.IsConnected = isAConected;
    }
}