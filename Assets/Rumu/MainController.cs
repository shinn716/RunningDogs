using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public enum DogSelect {
	Corgi,
	Beagle,
	Frenchie,
	Pug,
	Random
}

public class MainController : MonoBehaviour {

	public DogSelect dogSelect;
	public GameObject[] dog;
	public GameObject cube;
	public float RunSpeed;
	public bool DebugMode;
	public int StandbyTimer;
	public GameObject blank;
	public GameObject showTitle;

	bool standby;
	Vector2 tracker;
	float dogPosy;
	float dogPosz;
	Vector2 oldMousePos;
	float standbyCount=0;
	float dist;
	Animator anim1;
	int index=0;

	bool sitOnce=false;
	bool runOnce=false;
	bool idleOnce=false;
	string dogName;

	bool showTime = false;
	bool fadeOutSt = false;
	bool showOnce = false;
	float fadeOut_value=0;

	//Thread standbyThread;

	void Start () {

		init ();

		/*
		standbyThread = new Thread (() => {
			Vector2 input = new Vector2(Input.mousePosition.x, Input.mousePosition.y); 
			StandbyFuct (input);
		}
		);
		standbyThread.IsBackground = true;
		standbyThread.Start ();
		*/


	}

	void init(){
		for (int i = 0; i < dog.Length; i++)
			dog [i].SetActive (false);

		switch(dogSelect){
		default:
			index = Random.Range (0, 4);
			if(index==0)
				dogName = "Corgi";
			else if(index==1)
				dogName = "Beagle";
			else if(index==2)
				dogName = "Frenchie";
			else if(index==3)
				dogName = "Pug";
			break;

		case DogSelect.Corgi:
			index = 0;
			dogName = "Corgi";
			break;

		case DogSelect.Beagle:
			index = 1;
			dogName = "Beagle";
			break;

		case DogSelect.Frenchie:
			index = 2;
			dogName = "Frenchie";
			break;

		case DogSelect.Pug:
			index = 3;
			dogName = "Pug";
			break;
		}

		dog [index].SetActive (true);
		showTitle.SetActive (false);
		dogPosy = dog[index].transform.position.y;		
		dogPosz = dog[index].transform.position.z;
		anim1 = dog[index].GetComponent<Animator> ();
		anim1.CrossFade (dogName + "Idle", 0.25f);

		sitOnce=false;
		runOnce=false;
		idleOnce=false;
	}

	void Update () {


		if(Input.GetKeyDown(KeyCode.Space)){
			fadeOutSt = !fadeOutSt;
			//blank.SetActive (true);

			if(!fadeOutSt){
				init();
			}
		}
			
		if (fadeOutSt) {
			blankFadein ();
		} else {
			blankFadeout ();

		}

		//----ShowTime
		if (showTime) {
			print ("show");
			showTitle.SetActive (true);
		} else {
			showTitle.SetActive (false);
		}

		if (DebugMode) {

			//----收滑鼠座標
			Camera c = Camera.main;
			tracker = c.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, c.nearClipPlane));

			//----移動
			Vector2 nowDogPos = new Vector2 (dog [index].transform.position.x, dog [index].transform.position.y);
			LerpFuct (dog [index].transform, nowDogPos, tracker);

			//----轉頭
			cube.transform.position = new Vector3 (tracker.x, -3f, -3f);
			Transform target = cube.transform;
			dog [index].transform.LookAt (target);


			//----standby
			Vector2 input = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
			StandbyFuct (input);


		} else {
				//-----串 webcam
		}


		//---- standby
		if (standby) {
			if (!sitOnce) {
				sitOnce = true;
				anim1.CrossFade (dogName + "SitIdle", 0.25f);
			}

		} else {
			sitOnce = false;	

			if (DebugMode) {
				Camera c = Camera.main;
				Vector3 t = tracker = c.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, c.nearClipPlane));
				dist = Mathf.Abs (dog [index].transform.position.x - t.x);
			} else {
				//-----串 webcam
			}

			//print (dist);

			if (dist <= 0.15f) {
				if (!idleOnce) {
					runOnce = false;
					idleOnce = true;
					anim1.CrossFade (dogName + "Idle", 0.25f);
				}
			} else {

				if (!runOnce) {
					runOnce = true;
					idleOnce = false;
					anim1.CrossFade (dogName + "Run", 0.25f);

				}
			}
		}

	
	


	}

	void blankFadein(){
		if (fadeOut_value >= 1) {
			//blank.SetActive (false);
			//dog [index].SetActive (false);
			fadeOut_value = 1;
			showTime = true;
		} else {
			fadeOut_value += 0.5f * Time.deltaTime;	
			blank.GetComponent<Image> ().color = new Color (0, 0, 0, fadeOut_value);
		}
	}

	void blankFadeout(){
		if (fadeOut_value <= 0) {
			//blank.SetActive (false);
			fadeOut_value = 0;
		} else {
			fadeOut_value -= 0.5f * Time.deltaTime;	
			blank.GetComponent<Image> ().color = new Color (0, 0, 0, fadeOut_value);
			showTime = false;
			//dog [index].SetActive (true);
		}
	}


	void LerpFuct(Transform target, Vector2 startPos, Vector2 endPos){
		
		float dist = Vector2.Distance (startPos, endPos);
		float time = dist / Time.deltaTime * Mathf.PerlinNoise (Time.time*2, 0) * RunSpeed * 0.001f; 
		float lerpx = Mathf.Lerp (startPos.x, endPos.x, time);
	
		target.transform.position = new Vector3 (lerpx, dogPosy, dogPosz);

	}

	void StandbyFuct(Vector2 input){
		
		Vector2 nowMousePos = input;
		if (oldMousePos.x - nowMousePos.x == 0 && oldMousePos.y - nowMousePos.y == 0) {
			standbyCount+=Time.deltaTime;
			int seconds = (int) standbyCount%60;
			if(seconds > StandbyTimer){
				standby = true;
			}

		} else {
			standbyCount=0;
			standby = false;
		}
		oldMousePos = input;
	}

	/*
	void OnDisable(){
		standbyThread.Abort ();
	}
	void OnApplicationQuit(){
		standbyThread.Abort ();
	}
	*/
		

}
