using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TestMatchThreeGrid : MonoBehaviour {

	
	
	
	
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}


public class MatchThreeObject
{
	public Sprite SpriteVisual {
		set
		{
			Assert.IsNotNull(imageHolder);
			imageHolder.sprite = value;
		}
	}

	public int rowInColmn;
	
	private int column;

	public int Column => column;

	private Image imageHolder;
	
	public MatchThreeObject(Image image, int inColumn, int rowInColmn)
	{
		this.imageHolder = image;
		this.column = inColumn;
		this.rowInColmn = rowInColmn;
	}
}