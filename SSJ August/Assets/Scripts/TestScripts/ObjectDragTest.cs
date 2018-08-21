using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ObjectDragTest : MonoBehaviour
{
    private TileType tileType;

    public TileType TileType
    {
        get { return tileType; }
        set
        {
            tileType = value;
            GetComponent<Image>().sprite = TestMatchThreeGrid.TileTypeDictionary[value];

            #region testing only
            GetComponent<Image>().color = Color.white;
            return;

            switch (value)
            {
                case TileType.Red:
                    GetComponent<Image>().color = isConnected ? Color.magenta : Color.red;
                    break;
                case TileType.Blue:
                    GetComponent<Image>().color = isConnected ? Color.cyan : Color.blue;
                    break;
                case TileType.Green:
                    GetComponent<Image>().color = isConnected ? Color.yellow : Color.green;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            #endregion
        }
    }

    public List<ObjectDragTest> Connections = new List<ObjectDragTest>();

    private bool isConnected;

    public bool IsConnected
    {
        get { return isConnected; }
        set
        {
            isConnected = value;
            TileType = tileType; //just to set color properly
        }
    }

    public int RowInColmn => transform.GetSiblingIndex();
    public int InColmn => transform.parent.GetSiblingIndex();

    private int swappingWithIndex;

    private bool isBeingDragged;

    private ObjectDragTest swappableObject;

    private Vector3 positionBeforeDrag;

    private float Width;
    private float Height;

    public Action<ObjectDragTest, ObjectDragTest> OnSwap;

    private void Start()
    {
        Image image = GetComponent<Image>();

        Assert.IsNotNull(image);

        Width = image.rectTransform.sizeDelta.x;
        Height = image.rectTransform.sizeDelta.y;

        EventTrigger triggers = GetComponent<EventTrigger>();

        EventTrigger.Entry startDragEntry = new EventTrigger.Entry();
        startDragEntry.callback.AddListener(arg0 => startDrag());
        startDragEntry.eventID = EventTriggerType.BeginDrag;

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.callback.AddListener(arg0 => pointerDrag());
        dragEntry.eventID = EventTriggerType.Drag;

        EventTrigger.Entry dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.callback.AddListener(arg0 => endDrag());
        dragEndEntry.eventID = EventTriggerType.EndDrag;

        triggers.triggers.Add(startDragEntry);
        triggers.triggers.Add(dragEntry);
        triggers.triggers.Add(dragEndEntry);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBeingDragged) return;

        ObjectDragTest dragObj = other.gameObject.GetComponent<ObjectDragTest>();

        if (dragObj)
        {
            swappableObject = dragObj;
            swappingWithIndex = dragObj.RowInColmn;
//			Debug.Log(swappingWithIndex);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        swappableObject = null;
    }

    private void startDrag()
    {
        positionBeforeDrag = transform.position;
    }

    private void pointerDrag()
    {
        isBeingDragged = true;

        float x = Input.mousePosition.x;

        int totalColumnCount = transform.parent.parent.childCount;

        float xMin = InColmn == 0 ? positionBeforeDrag.x : positionBeforeDrag.x - Width;
        float xMax = InColmn == totalColumnCount - 1 ? positionBeforeDrag.x : positionBeforeDrag.x + Width;

        float xClamped = Mathf.Clamp(x, xMin, xMax);


        float y = Input.mousePosition.y;

        float yMin = RowInColmn == transform.parent.childCount - 1
            ? positionBeforeDrag.y
            : positionBeforeDrag.y - Height;
        float yMax = RowInColmn <= TestMatchThreeGrid.RowCutOff ? positionBeforeDrag.y : positionBeforeDrag.y + Height;

        float yClamped = Mathf.Clamp(y, yMin, yMax);


        Vector3 pos = transform.position;

        Vector3 mouseDirection = positionBeforeDrag - Input.mousePosition;

        float angle = 45;

        if (Vector3.Angle(transform.up, mouseDirection) <= angle ||
            Vector3.Angle(transform.up * -1, mouseDirection) <= angle)
        {
            pos.y = yClamped;
            pos.x = positionBeforeDrag.x;
        }
        else if (Vector3.Angle(transform.right, mouseDirection) <= angle ||
                 Vector3.Angle(transform.right * -1, mouseDirection) <= angle)
        {
            pos.x = xClamped;
            pos.y = positionBeforeDrag.y;
        }

        transform.position = pos;

//		Debug.Log("Is being dragged");
    }

    private void endDrag()
    {
        swappingWithIndex = -1;
        isBeingDragged = false;

        transform.position = positionBeforeDrag;


        if (swappableObject)
        {
            OnSwap?.Invoke(this, swappableObject);
        }

//		Debug.Log("Drag End");
    }

    public void swapPositionsWith(ObjectDragTest draggedObject)
    {
        Transform swappedParent = draggedObject.transform.parent;
        int swappedSibiliingIndex = draggedObject.transform.GetSiblingIndex();

        Transform currentParent = transform.parent;
        int currentSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(swappedParent);
        transform.SetSiblingIndex(swappedSibiliingIndex);

        draggedObject.transform.SetParent(currentParent);
        draggedObject.transform.SetSiblingIndex(currentSiblingIndex);
    }

    public ObjectDragTest getConnectingObj(int dir)
    {
        int totalColumnCount = transform.parent.parent.childCount;

        int currentRowindex = transform.GetSiblingIndex();

        Transform currentColumnTransform = transform.parent;
        Transform gridTransform = currentColumnTransform.parent;

        switch (dir)
        {
            case 0:
                return currentRowindex < TestMatchThreeGrid.RowCutOff
                    ? null
                    : currentColumnTransform.GetChild(currentRowindex - 1).GetComponent<ObjectDragTest>();
            case 1:
                return InColmn == totalColumnCount - 1
                    ? null
                    : gridTransform.GetChild(InColmn + 1).GetChild(currentRowindex).GetComponent<ObjectDragTest>();
            case 2:
                return currentRowindex == currentColumnTransform.childCount - 1
                    ? null
                    : currentColumnTransform.GetChild(currentRowindex + 1).GetComponent<ObjectDragTest>();
            case 3:
                return InColmn == 0
                    ? null
                    : gridTransform.GetChild(InColmn - 1).GetChild(currentRowindex).GetComponent<ObjectDragTest>();
            default:
                return null;
        }
    }

    public void AddConnection(ObjectDragTest obj)
    {
        if (Connections == null) Connections = new List<ObjectDragTest>();

        obj.IsConnected = true;
        IsConnected = true;
        Connections.Add(obj);
    }

    public void ResetConnections()
    {
        Connections.Clear();
        IsConnected = false;
    }
}