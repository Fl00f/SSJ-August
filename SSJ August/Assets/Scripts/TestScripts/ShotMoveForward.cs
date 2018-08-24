using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMoveForward : MonoBehaviour {

	private float moveRate = 200;
	public bool CanHit;
	// Update is called once per frame
	void Update () {
		transform.Translate (moveRate * Time.deltaTime, 0, 0);
	}

	void OnTriggerEnter2D (Collider2D col) {
		Target tar = col.gameObject.GetComponent<Target> ();
		if (tar != null && CanHit) {
			tar.OnHit?.Invoke ();
			Destroy(gameObject);
		}
	}

}