using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = System.Random;

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

                imageTransform.position = Vector3.Lerp(translationImageStartPositions[i, j], gridImagePositions[i, j],
                    translationPercentage);
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
        checkObject(a,0);
        checkObject(b,0);

        OnTranslationEnd = null;
    }

    private void checkObject(ObjectDragTest a , int step)
    {
        if (step >= 10) return;
        
        if (a.North != null && !a.North.IsConnected)
        {
            checkObject(a.North,step + 1);
        }

        if (a.East != null && !a.East.IsConnected)
        {
            checkObject(a.East, step + 1);
        }

        if (a.South != null && !a.South.IsConnected)
        {
            checkObject(a.South, step + 1);
        }

        if (a.West != null && !a.West.IsConnected)
        {
            checkObject(a.West, step + 1);
        }
    }
}