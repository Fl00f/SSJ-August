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
	public int RowInColmn => transform.GetSiblingIndex();
	public int InColmn => transform.parent.GetSiblingIndex();
	
	private int swappingWithIndex;
	
	public int SwappingWithIndex => swappingWithIndex;

	private bool isBeingDragged;

	private ObjectDragTest swappableObject;
	
	private Vector3 positionBeforeDrag;

	private float Width;

	public Action OnSwap;
	
	private void Start()
	{
		Image image = GetComponent<Image>();

		Assert.IsNotNull(image);
		
		Width = image.rectTransform.sizeDelta.x;
		
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
		float xClamped = Mathf.Clamp(x, positionBeforeDrag.x - Width, positionBeforeDrag.x + Width);

		Vector3 pos = transform.position;
		pos.x = xClamped;

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
			swapPositionsWith(swappableObject);
			OnSwap?.Invoke();
		}
		
//		Debug.Log("Drag End");

	}

	private void swapPositionsWith(ObjectDragTest draggedObject)
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
}
