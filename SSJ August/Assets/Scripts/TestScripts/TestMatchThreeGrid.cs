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

    public int minNumberOfConnections = 3;



    public static Dictionary<TileType, Sprite> TileTypeDictionary;
    
    #region Debug

    public bool doImageTranslations;
    public float translationStartTime;

    #endregion


    public Sprite tileTypeOne;
    public Sprite tileTypeTwo;
    public Sprite tileTypeThree;
    public Sprite tileTypeFour;

    // Use this for initialization
    void Start()
    {
        RowCutOff = 10;
        TileTypeDictionary = new Dictionary<TileType, Sprite>()
        {
            { TileType.Red, tileTypeOne},
            { TileType.Blue, tileTypeTwo},
            { TileType.Green, tileTypeThree},
            { TileType.Purple, tileTypeFour},
        };
        SetInitialGridPositions();
    }

    // Update is called once per frame
    void Update()
    {
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

        //swap positions (only grid positions)
        a.swapPositionsWith(b);
        //go through connections
        setConnectionRoot(a, a);
        setConnectionRoot(b, b);

        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest>();
        //if number of connections is zero swap back
        if (allObjects.Count(obj => obj.Connections.Count + 1 >= minNumberOfConnections) == 0 ||
            a.TileType == b.TileType)
        {
            //swap back
            a.swapPositionsWith(b);
            allObjects.ToList().ForEach(obj => obj.ResetConnections());
        }

        OnTranslationEnd += handleSwapTranslationEnd;
    }

    private void handleSwapTranslationEnd()
    {
        doingSwap = false;
        
        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest>();
        //we add one because root is not part of the connection nodes
        foreach (var connectionRoot in allObjects.Where(a => a.Connections.Count + 1 >= minNumberOfConnections))
        {
            //push connections up super high out of sight and make first child of parent
            for (int i = 0; i < connectionRoot.Connections.Count; i++)
            {
                connectionRoot.Connections[i].transform.SetSiblingIndex(0);
                Vector3 posInner = connectionRoot.Connections[i].transform.position;

                posInner.y = 1000;

                connectionRoot.Connections[i].transform.position = posInner;
            }

            connectionRoot.transform.SetSiblingIndex(0);
            Vector3 pos = connectionRoot.transform.position;

            pos.y = 1000;

            connectionRoot.transform.position = pos;
        }

        //reset connections
        allObjects.ToList().ForEach(obj => obj.ResetConnections());

        OnTranslationEnd = null;

        //since connections have been moved we need to translate again to push tiles down
        translationImageStartPositions = getCurrentImagePositions();
        translationStartTime = Time.time;
        doImageTranslations = true;
    }

    private void setConnectionRoot(ObjectDragTest a, ObjectDragTest root)
    {
        for (int i = 0; i < 4; i++)
        {
            ObjectDragTest connection = a.getConnectingObj(i);

            if (connection != null && !connection.IsConnected && connection.TileType == a.TileType)
            {
                root.AddConnection(connection);
                setConnectionRoot(connection, root);
            }
        }
    }
}