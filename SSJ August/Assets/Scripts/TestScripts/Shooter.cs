using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shooter : MonoBehaviour {

	public GameObject ShotPrefab;
	public const int destroyDistance = 10;

	public List<Transform> shots = new List<Transform> ();

	public void StartShooting (int numberOfShots, float successRate, int fireRate = 2) {
		StopAllCoroutines ();

		shots.ForEach (a => Destroy (a.gameObject));
		shots = new List<Transform> ();

		StartCoroutine (shootIEnumerator (numberOfShots, successRate, fireRate));
	}

	private IEnumerator shootIEnumerator (int numberOfShots, float successRate, int shootEvery) {

		shoot ();
		int shotsLeft = numberOfShots;
		float timeFromLastShot = Time.time;

		while (shotsLeft > 0) {
			shots.ForEach ((a) => {
				if (isPastDestroyDistance (a)) {
					Destroy (a);
				}
			});
			shots.TrimExcess ();

			if (Time.time - timeFromLastShot > shootEvery) {
				shoot ();
				shotsLeft--;
				timeFromLastShot = Time.time;
			}

			yield return new WaitForEndOfFrame ();
		}

	}

	private void shoot () {
		GameObject shot = Instantiate (ShotPrefab, transform.transform);
		shots.Add (shot.transform);
	}

	private bool isPastDestroyDistance (Transform objtransform) {
		return Vector3.Distance (transform.position, objtransform.position) > destroyDistance;
	}
}