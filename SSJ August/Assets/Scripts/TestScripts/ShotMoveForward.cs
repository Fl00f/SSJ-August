using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMoveForward : MonoBehaviour {

	public float moveRate = 1;
	public bool CanHit;
	// Update is called once per frame
	void Update () {
		transform.Translate (0, 0, moveRate * Time.deltaTime);
	}
}