using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TestMatchThreeGrid : MonoBehaviour
{
    public Transform[] gridColumns;
    private int ColumnLength => gridColumns.Length;

    public Vector2[,] gridImagePositions;
    public Vector2[,] translationImageStartPositions;

    public float currentTranslationTime => doingSwap ? swapTime : translationTime;
    
    public float translationTime;
    public float swapTime;
    public bool doingSwap;
    
    #region Debug

    public bool doImageTranslations;
    public float translationStartTime;
    
    #endregion
    
    // Use this for initialization
    void Start()
    {
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
            float percentage = (Time.time - translationStartTime) / currentTranslationTime;
            float percentageClamp = Mathf.Clamp(percentage, 0, 1);
            translateImagesToGridPositions(percentageClamp);

            if (percentageClamp >= 1)
            {
                doImageTranslations = false;
                translationImageStartPositions = null;
                doingSwap = false;
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

        for (int i = 0; i < ColumnLength; i++)
        {
            for (int j = 0; j < gridColumns[0].childCount; j++)
            {
                ObjectDragTest dragableObj = gridColumns[i].GetChild(j).GetComponent<ObjectDragTest>();
                dragableObj.OnSwap += handleTileSwap;
                
                Vector3 pos = gridColumns[i].GetChild(j).position;

                gridImagePositions[i, j] = new Vector2(pos.x, pos.y);
            }
        }
    }

    private void handleTileSwap()
    {
        doingSwap = true;
        translationImageStartPositions = getCurrentImagePositions();
        translationStartTime = Time.time;
        doImageTranslations = true;
    }
}