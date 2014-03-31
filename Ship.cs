using UnityEngine;
using System.Collections;

// Fluid Racer v0.9.2 
// Unity 3D v4.3
// stuartnz.github.io


public class Ship : MonoBehaviour
{
    public float hoverHeight = 3F;
    public float hoverHeightStrictness = 3F;
    public float forwardThrust = 55000F;
    public float backwardThrust = 2500F;
    public float bankAmount = 66F;
    public float bankSpeed = 0.2F;
    public Vector3 bankAxis = new Vector3(-1F, 0F, 0F);
    public float turnSpeed = 8000F;

    public Vector3 forwardDirection = new Vector3(1F, 0F, 0F);

    public float mass = 5F;

    // positional drag
    public float sqrdSpeedThresholdForDrag = 25F;
    public float superDrag = 2F;
    public float fastDrag = 0.5F;
    public float slowDrag = 0.01F;

    // angular drag
    public float sqrdAngularSpeedThresholdForDrag = 5F;
    public float superADrag = 32F;
    public float fastADrag = 16F;
    public float slowADrag = 0.1F;

    public bool playerControl = true;

    // cameras
    public bool setCameraOn = true;

    public bool setCameraMove = true;
    public bool setCameraFollow = true;
	
    // terrain
    public float terrainDistance = 0.0F;
    public float upwardSpeed = 0.0F;

    Vector3 dive_rotation_z = new Vector3();
    float bank = 0F;

    public Vector3 fHit, bHit;
    RaycastHit hit;

    RaycastHit forwardHit, backwardHit;
    RaycastHit diff;

    public bool terrainAngle = false;
    public bool isAccellerating = false;
    public float distance1, lastdistance;

    // Camera
	public Camera pilotcam, wipeoutcamera, topdowncamera, gatecam;

    //private Camera[] cameras;
    public int currentCamView = 1;

    private Camera currentCamera;

    // Gates
    public bool passgate1 = false;
    public bool passgate2 = false;

	public AudioClip musictrack;

	public int nextGate = 1;

	public AudioClip checkpointsound;

	public float[] gateTimes;
	public float totalLapTime = 0;
	public float currentLapTime = 0;

	public SaveSettings2 saveSettings;

	public float recordLapTime;

    /// Set Player Control
    void SetPlayerControl(bool control)
    {
        playerControl = control;
    }

    void Start()
    {
        rigidbody.mass = mass;
        currentCamera = pilotcam;

        pilotcam.farClipPlane = 4200;
        wipeoutcamera.farClipPlane = 4200;
        topdowncamera.farClipPlane = 1200;
		gatecam.farClipPlane = 6000;

		pilotcam.depth = Camera.main.depth + 1;
		wipeoutcamera.depth = Camera.main.depth + 1;
		topdowncamera.depth = Camera.main.depth + 1;
		gatecam.depth = Camera.main.depth + 1;

		pilotcam.fieldOfView = 90;
		wipeoutcamera.fieldOfView = 90;

		pilotcam.enabled = true;
		wipeoutcamera.enabled = true;

		gateTimes = new float[20];

		saveSettings = new SaveSettings2 ();

		//if (saveSettings.LoadData() != null)
		//{
		//	recordLapTime = saveSettings.LoadData ();
		//} 

		// Set a default record
		recordLapTime = 54.67f; 

        ChangeView(1);
    }

    void FixedUpdate()
    {
        if (Mathf.Abs(thrust) > 0.01F)
        {
            if (rigidbody.velocity.sqrMagnitude > sqrdSpeedThresholdForDrag)
                rigidbody.drag = fastDrag;
            else
                rigidbody.drag = slowDrag;
        }
        else
            rigidbody.drag = superDrag;

        if (Mathf.Abs(turn) > 0.01F)
        {
            if (rigidbody.angularVelocity.sqrMagnitude > sqrdAngularSpeedThresholdForDrag)
                rigidbody.angularDrag = fastADrag;
            else
                rigidbody.angularDrag = slowADrag;
        }
        else
            rigidbody.angularDrag = superADrag;

        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hoverHeight + 260 + Terrain.activeTerrain.SampleHeight(transform.position), transform.position.z), hoverHeightStrictness);

        float amountToBank = rigidbody.angularVelocity.y * bankAmount;

        bank = Mathf.Lerp(bank, amountToBank, bankSpeed);

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation *= Mathf.Deg2Rad;
        rotation.x = 0F;

        // Z-Rotation 
        rotation.z = 0F;

        // terrain
        RaycastHit fHit, bHit, ghit, hit3;

        //Ray downRay = new Ray(transform.position, - Vector3.up);
        if (Physics.Raycast(transform.position + transform.forward, Vector3.down, out forwardHit))
        {
            //rotation.z += 11F;	
            //fHit = ghit.point;

            if (Physics.Raycast(transform.position - transform.forward, Vector3.down, out backwardHit))
            {
                //bHit = hit3.point; // get the back hit point
                diff.point = forwardHit.point - backwardHit.point; // align the object to these points
                // rotation.x += diff.point.rx;
                if (diff.point.y > 0)
                {
                    terrainAngle = true;
                    //rotation.z += diff.point.y / 2;
                }
                else
                {
                    terrainAngle = false;
                    //rotation.z -= diff.point.y / 2;
                }
            }
        }

        // rotation.z = dive_rotation_z.z;	
        rotation += bankAxis * bank;
        //rotation.z += diff.point.y;
        rotation *= Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(rotation);

		if (setCameraOn)
		{
			// Camera Position : Type B - Chase Cam
			wipeoutcamera.transform.position = GameObject.Find("camera-pos").transform.position;
			
			//  v0.1 - First Working Version 1.1 Set Position
			//  Camera.mainCamera.transform.rotation = Quaternion.Euler(new Vector3(22 ,rotation.y + 90, 0));
			
			// v0.2 - Follow object set behind as child for camera position
			wipeoutcamera.transform.rotation = Quaternion.Euler(new Vector3(GameObject.Find("camera-pos").transform.rotation.x, rotation.y + 90, transform.rotation.z * rotation.z));
			
			GameObject.Find("camera-pos").renderer.enabled = false;
			
			// Gate-1 Camera
			gatecam.transform.LookAt(GameObject.Find("camera-pos").transform.position);
			
			// Top Down View
			topdowncamera.transform.position = new Vector3(GameObject.Find("camera-pos").transform.position.x, GameObject.Find("camera-pos").transform.position.y + 400, GameObject.Find("camera-pos").transform.position.z);
		}
    }

    void LateUpdate()
    {

    }

    float thrust = 0F;
    float turn = 0F;

    void Thrust(float t)
    {
        thrust = Mathf.Clamp(t, -1F, 1F);
    }

    void Turn(float t)
    {
        turn = Mathf.Clamp(t, -1F, 1F) * turnSpeed;
    }

    bool thrustGlowOn = false;
    public float duration = 1.0F;

    public AudioClip thrustsound;


    public bool goingUp = false;

    void Update()
    {
        float theThrust = thrust;

        if (playerControl)
        {
            thrust = Input.GetAxis("Vertical");
            turn = Input.GetAxis("Horizontal") * turnSpeed;
        }

        if (thrust > 0F)
        {
            theThrust *= forwardThrust;
            if (!thrustGlowOn)
            {
                thrustGlowOn = !thrustGlowOn;
                BroadcastMessage("SetThrustGlow", thrustGlowOn, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            theThrust *= backwardThrust;
            if (thrustGlowOn)
            {
                thrustGlowOn = !thrustGlowOn;
                BroadcastMessage("SetThrustGlow", thrustGlowOn, SendMessageOptions.DontRequireReceiver);
            }
        }

        rigidbody.AddRelativeTorque(Vector3.up * turn * Time.deltaTime);
        rigidbody.AddRelativeForce(forwardDirection * theThrust * Time.deltaTime);

		SoundController ();

		LightController ();

		GateController ();

		CameraController ();
    }

	// lights
	void LightController()
	{
		//  Lights on Craft
		float phi = Time.time / duration * 2 * Mathf.PI;
		float amplitude = Mathf.Cos(phi) * 0.5F + 0.5F;

		// Lights on starting beacon
		GameObject.Find("beacon1").light.intensity = amplitude;
		GameObject.Find("beacon2").light.intensity = amplitude;
		
		GameObject.Find("backEngine-Light").light.intensity = rigidbody.velocity.magnitude / 8;

	}

	// sound
	void SoundController()
	{
		if (Input.GetKey("up") || Input.GetKey("w"))
		{
			isAccellerating = true;
			audio.volume = 1;
			
			if (!audio.isPlaying)
			{
				audio.loop = true;
				audio.Play();
			}
		}
		else
		{
			isAccellerating = false;
			
			if (audio.volume > 0.01)
			{
				audio.volume -= 0.02f;
			}
		}
	}

	// gates
	void GateController()
	{
		gateTimes[nextGate] += Time.deltaTime;
		currentLapTime += Time.deltaTime;

		// Check Lap Complete
		if (nextGate == 19) 
		{
			nextGate = 1;

			if(currentLapTime < recordLapTime)
			{
				// New Record
				recordLapTime = currentLapTime;
				
				// Save New Time
				saveSettings.SaveData(recordLapTime);
			}

			// reset laptime calculator
			totalLapTime = 0;

			for(int a=1; a<19; a++)
			{
				totalLapTime += gateTimes[a];
				gateTimes[a] = 0;
			}

			// Reset Current LapTime
			currentLapTime = 0;
		}
		
		// Distance to next gate
		var go = GameObject.Find("g" + nextGate.ToString());

		distance1 = Vector3.Distance(go.transform.position, transform.position);
		
		lastdistance = distance1;

		// Start Camera Panning Down..
		if (distance1 < 2302) 
		{
			// Attract-Modo Todo..Pan across as boosing out?
			//gatecam.transform.position += new Vector3(0,0, 2f);
			
			// Lower Camera for Gate-1 Attract Fly-by
			if(gatecam.transform.position.y > 290)
				gatecam.transform.position += new Vector3(0,-0.2F,0);
		}

		// Next-Gate
		// Maybe lower this and have the player have to be central to the gates?
		if (distance1 < 239)
		{
			audio.PlayOneShot(checkpointsound);
			nextGate++;
		}
	}

	// Cameras
	void CameraController()
	{
		// Cameras specific key
		if (Input.GetKeyDown ("1")) {
			ChangeView (1);
		} else  if (Input.GetKeyDown ("2")) {
			ChangeView (2);
		} else  if (Input.GetKeyDown ("3")) {
			ChangeView (3);
		} else  if (Input.GetKeyDown ("4")) {
			ChangeView (4);
		}
		
		// Camera Toggle - Pilot-Cam and WipEout-Cam
		if (Input.GetKeyDown ("e")) {
			if(currentCamView == 1)
			{
				currentCamView = 2;
			}
			else
			{
				currentCamView = 1;
			}
			
			ChangeView(currentCamView);
		}
		
		// Camera Cycle
		if (Input.GetKeyDown ("r")) {
			currentCamView++;
			
			if(currentCamView == 5)
			{
				currentCamView = 1;
			}
			
			ChangeView(currentCamView);
		}
		
		// Camera Cycle
		if (Input.GetKeyDown ("space")) {
			// rigidbody.position.y += new Vector3(0,0.2F,0);
		}
	}
	
    void ChangeView(int cameraId)
    {

        currentCamera.enabled = false;

        if (cameraId == 1)
        {
			currentCamera = pilotcam;
			currentCamView = 1;
        }
        else if (cameraId == 2)
        {
			currentCamera = wipeoutcamera;
			currentCamView = 2;
        }
		else if (cameraId == 3)
		{
			currentCamera = topdowncamera;
			currentCamView = 3;
		}
		else if (cameraId == 4)
		{
			currentCamera = gatecam;
			currentCamView = 4;
		}
		else
		{
			currentCamera = pilotcam;
			currentCamView = 0; //error
		}

		Debug.Log("cameraID : " + cameraId);
		Debug.Log("cam : " + currentCamera.name.ToString());

        currentCamera.enabled = true;
    }

    // Gui
    void OnGUI()
    {
        GUI.Button(new Rect(10, 10, 180, 20), "[v] "
            + rigidbody.velocity.magnitude.ToString("0.00"));

        GUI.Button(new Rect(10, 35, 180, 20), "[cr] "
            + wipeoutcamera.transform.rotation.ToString());

        GUI.Button(new Rect(10, 55, 180, 20), "campos:"
    + GameObject.Find("camera-pos").transform.rotation);

        // hSliderValue = GUI.HorizontalSlider (new Rect (25, 95, 100, 30), hSliderValue, 0.0f, 10.0f);

        forwardThrust = GUI.HorizontalSlider(new Rect(25, 95, 100, 30), forwardThrust, 25000f, 100000.0f);

        GUI.Button(new Rect(10, 75, 180, 20), "[thrust] "
            + forwardThrust.ToString());

        GUI.Button(new Rect(10, 135, 180, 20), "[cp] "
        + wipeoutcamera.transform.position.ToString());

        // turn speed
        turnSpeed = GUI.HorizontalSlider(new Rect(25, 295, 100, 30), turnSpeed, 20000f, 40000.0f);

        GUI.Button(new Rect(24, 305, 180, 20), "[turn] "
        + turnSpeed.ToString());

        // bankAmount
        bankSpeed = GUI.HorizontalSlider(new Rect(25, 345, 100, 30), bankSpeed, 0.0f, 3f);

        GUI.Button(new Rect(24, 355, 180, 20), "[bS] " + bankSpeed.ToString());

        // bankAmount
        bankAmount = GUI.HorizontalSlider(new Rect(25, 395, 100, 30), bankAmount, 0.0f, 3f);

        GUI.Button(new Rect(24, 405, 180, 20), "[bA] " + bankAmount.ToString());

        GUI.Button(new Rect(155, 10, 180, 20), "[fHit] " + fHit.ToString());
        GUI.Button(new Rect(405, 10, 180, 20), "[bHit] " + bHit.ToString());

        GUI.Button(new Rect(655, 10, 180, 20), "[v-ax] " + Input.GetAxis("Vertical").ToString());
	
        GUI.Button(new Rect(855, 10, 180, 20), "[terrd] " + terrainDistance.ToString());
        GUI.Button(new Rect(1155, 10, 180, 20), "[fhp] " + forwardHit.point.ToString());
        GUI.Button(new Rect(1155, 30, 180, 20), "[bhp] " + backwardHit.point.ToString());

        // GUI.Button(new Rect(110, 110, 180, 20), "[z-R] " + rotation.z.ToString("0.00"));
        GUI.Button(new Rect(1355, 10, 180, 20), "[tz] " + terrainAngle.ToString());

        GUI.Button(new Rect(1355, 40, 180, 20), "[->] " + isAccellerating.ToString() + "/" + audio.volume.ToString());

        // Distance
        GUI.Button(new Rect(1155, 60, 180, 20), "[d1] " + distance1.ToString());

        //Cameras
        GUI.Button(new Rect(1355, 80, 180, 20), "[cam] " + currentCamView.ToString());

        GUI.Button(new Rect(1355, 100, 180, 20), "[pg1] " + passgate1.ToString());

        GUI.Button(new Rect(1355, 120, 180, 20), "[pg2] " + passgate2.ToString());
        GUI.Button(new Rect(1355, 140, 180, 20), "[cc]" + currentCamera.name.ToString());

		GUI.Button(new Rect(1355, 160, 180, 20), "[ng] " + nextGate.ToString());

		/*
		for (int a=1; a<19; a++) 
		{
			// GUI.Button(new Rect(1355, 180 + (a*20), 180, 20), "[g-" + a.ToString() + "] " + gateTimes[a].ToString("0.00"));
		}
		*/

		GUI.Button(new Rect(1355, 350, 180, 20), "[g-" + nextGate.ToString() + "] " + gateTimes[nextGate].ToString("0.00"));
		GUI.Button(new Rect(1355, 400, 180, 20), "[total] " + totalLapTime.ToString("0.00"));

		// Large Player Data
		GUIStyle customButton = new GUIStyle("button");
		customButton.fontSize = 44;
		GUIContent content = new GUIContent ("megaCorp");

		// Current and Session Best
		//GUI.Button(new Rect(1355, 440, 180, 60),"" + nextGate.ToString() + "| " + gateTimes[nextGate].ToString("0.00"),customButton);
		GUI.Button(new Rect(1355, 440, 180, 60),"" + nextGate.ToString() + "| " + currentLapTime.ToString("0.00"),customButton);

		GUI.Button(new Rect(1355, 550, 180, 60),totalLapTime.ToString("0.00"),customButton);

		// Record
		GUI.Button(new Rect(1355, 640, 180, 60),recordLapTime.ToString("0.00"),customButton);

		// Draw Rays Todo
        Debug.DrawRay(forwardHit.point, new Vector3(111, 111, 111));
    }
}
