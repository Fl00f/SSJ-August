using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shooter : MonoBehaviour {

	public GameObject ShotPrefab;
	public const int destroyDistance = 1000;

	private List<Transform> shots = new List<Transform> ();

	private void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			StartShooting (10, .5f, .5f);
		}
	}

	public void StartShooting (int numberOfShots, float successRate, float fireRate = 2) {
		StopAllCoroutines ();
		shots.RemoveAll (a => a == null);

		shots.ForEach (a => Destroy (a.gameObject));
		shots = new List<Transform> ();

		StartCoroutine (shootIEnumerator (numberOfShots, successRate, fireRate));
	}

	private IEnumerator shootIEnumerator (int numberOfShots, float successRate, float shootEvery) {

		shoot (successRate);
		int shotsLeft = numberOfShots - 1;
		float timeFromLastShot = Time.time;

		while (shotsLeft > 0) {
			shots.RemoveAll (a => a == null);

			shots.ForEach ((a) => {
				if (isPastDestroyDistance (a)) {
					Destroy (a.gameObject);
				}
			});

			if (Time.time - timeFromLastShot > shootEvery) {
				shoot (successRate);
				shotsLeft--;
				timeFromLastShot = Time.time;
			}

			yield return new WaitForEndOfFrame ();
		}

	}

	private void shoot (float successRate) {
		GameObject shot = Instantiate (ShotPrefab, transform.transform);
		float randomAngle = Random.Range (-20, 20);
		shot.transform.localRotation = Quaternion.Euler (0, 0, randomAngle);

		ShotMoveForward shotMF = shot.GetComponent<ShotMoveForward> ();
		shotMF.CanHit = Random.Range (0f, 1f) <= successRate;

		shots.Add (shot.transform);
	}

	private bool isPastDestroyDistance (Transform objtransform) {
		return Vector3.Distance (transform.position, objtransform.position) > destroyDistance;
	}
}