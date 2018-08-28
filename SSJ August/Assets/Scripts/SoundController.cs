using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	[Header ("AudioSources")]
	public AudioSource OneTimeAudioPlayer;
	public AudioSource Music;
	public AudioSource BackGround_Long;
	public AudioSource BackGround_Short;
	public AudioSource Game_BackGround_Short;

	[Header ("ThiccTrooper Clips")]
	public AudioClip ThiccTrooperDeath;
	public AudioClip ThiccTrooperHit;

	[Header ("Rhodean Clips")]
	public AudioClip RhodeanDeath;
	public AudioClip RhodeanHit;

	[Header ("Clear Clips")]
	public AudioClip Combo;
	public AudioClip clear3_2;
	public AudioClip clear3_3;
	public AudioClip clear;

	[Header ("Tile Swap Clips")]
	public AudioClip SwapGood;
	public AudioClip Swapbad;

	[Header ("Music Clips")]
	public AudioClip MenuMusic;
	public AudioClip GameMusic;

	[Header ("Misc Clips")]
	public AudioClip LaserFire;
	public AudioClip TurnCounterDone;

	public void OnThiccTrooperHit () {
		BackGround_Short.clip = ThiccTrooperHit;
		BackGround_Short.Play ();
	}

	public void OnThiccTrooperDeath () {
		BackGround_Short.clip = ThiccTrooperDeath;
		BackGround_Short.Play ();
	}

	public void OnRhodeanHit () {
		BackGround_Short.clip = RhodeanHit;
		BackGround_Short.Play ();
	}

	public void OnRhodeanDeath () {
		BackGround_Short.clip = RhodeanDeath;
		BackGround_Short.Play ();
	}

	public void OnTileSwitchSuccess () {
		Game_BackGround_Short.clip = SwapGood;
		Game_BackGround_Short.Play ();
	}

	public void OnTileSwitchFail () {
		Game_BackGround_Short.clip = Swapbad;
		Game_BackGround_Short.Play ();
	}

	public void OnGoToMenu () {
		Music.clip = MenuMusic;
		Music.Play ();
	}

	public void OnGameStart () {
		Music.clip = GameMusic;
		Music.Play ();
	}

	public void TurnCounterFullyCharged () {
		BackGround_Short.clip = TurnCounterDone;
		BackGround_Short.Play ();
	}

	public void LaserFireStart () {
		BackGround_Long.clip = LaserFire;
		BackGround_Long.Play ();
	}

	public void LaserFireStop () {
		if (BackGround_Long.isPlaying && BackGround_Long.clip.Equals (LaserFire)) {
			BackGround_Long.Stop ();
		}
	}

	public void ComboClear () {
		Game_BackGround_Short.clip = Combo;
		Game_BackGround_Short.Play ();
	}

	public void Clear3_2 () {
		Game_BackGround_Short.clip = clear3_2;
		Game_BackGround_Short.Play ();
	}

	public void Clear3_3 () {
		Game_BackGround_Short.clip = clear3_3;
		Game_BackGround_Short.Play ();
	}

	public void ClearBlocks () {
		Game_BackGround_Short.clip = clear;
		Game_BackGround_Short.Play ();
	}

	public void TurnCounterCharged () {
		Game_BackGround_Short.clip = TurnCounterDone;
		Game_BackGround_Short.Play ();
	}
}