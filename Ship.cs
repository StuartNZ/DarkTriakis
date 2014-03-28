using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {

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

    // ** C A M E R A 

    public bool setCameraOn = true; 

	public bool setCameraMove = true;
	public bool setCameraFollow = true; 


	//Terrain

	public float terrainDistance = 0.0F;
	public float upwardSpeed = 0.0F;

	Vector3 dive_rotation_z = new Vector3();
 
    float bank = 0F;
	
	// Terrain Rotation

	public Vector3 fHit, bHit; 
	RaycastHit hit;
	
	RaycastHit forwardHit, backwardHit;
 	RaycastHit diff;
	
	public bool terrainAngle = false;

	public bool isAccellerating = false;

	public float distance1;

	// Cameras

	public Camera maincamera, topdowncamera, pilotcam;
	//public Camera gatecam;

  	private Camera[] cameras;

    private int currentCameraIndex = 0;

    private Camera currentCamera;


    // Gates
    public bool passgate1 = false;
	public bool passgate2 = false;


	//////////////////////////////////
	///
	/// Set Player Control

    void SetPlayerControl(bool control)
    {
        playerControl = control;
    }
 
    void Start()
    {
        rigidbody.mass = mass;

        // cameras = new Camera[] {pilotcam,  maincamera, topdowncamera, gatecam};
		cameras = new Camera[] {pilotcam,  maincamera, topdowncamera};	

        currentCamera = maincamera; // Cant set initial camera?

        ChangeView();
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
 
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hoverHeight+260+Terrain.activeTerrain.SampleHeight(transform.position), transform.position.z), hoverHeightStrictness);
 
        float amountToBank = rigidbody.angularVelocity.y * bankAmount;
 
        bank = Mathf.Lerp(bank, amountToBank, bankSpeed);
 
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation *= Mathf.Deg2Rad;
        rotation.x = 0F;
        
		// Z-Rotation 
		rotation.z = 0F;
        
		// *************
		// T E R R A I N 
		// *************

 		RaycastHit fHit, bHit, ghit, hit3;

        //Ray downRay = new Ray(transform.position, - Vector3.up);
        if (Physics.Raycast(transform.position + transform.forward, Vector3.down, out forwardHit)) {
            //rotation.z += 11F;	
            //fHit = ghit.point;

			if (Physics.Raycast(transform.position - transform.forward, Vector3.down, out backwardHit))
			{
	            //bHit = hit3.point; // get the back hit point
	            diff.point = forwardHit.point - backwardHit.point; // align the object to these points
	            // rotation.x += diff.point.rx;
	            if(diff.point.y > 0)
	            {
            		terrainAngle = true;
            		//rotation.z += diff.point.y / 2;

            	}else
            	{
					terrainAngle = false;
					//rotation.z -= diff.point.y / 2;
            	}
    		}
        }

		// rotation.z = dive_rotation_z.z;	S
		rotation += bankAxis * bank;
		//rotation.z += diff.point.y;
		rotation *= Mathf.Rad2Deg;
        
		transform.rotation = Quaternion.Euler(rotation);
	
		if(setCameraOn)
		{
			//Camera Position : Type A
			//  Camera.mainCamera.transform.position = new Vector3(rigidbody.position.x - 20f, rigidbody.position.y + 10 ,rigidbody.position.z);
			//  Camera.mainCamera.transform.rotation = Quaternion.Euler(new Vector3(22,rotation.y + 90, 0));
			
			// Camera Position : Type B - Chase Cam v1.0!
			
			maincamera.transform.position = GameObject.Find("camera-pos").transform.position;
			
			//  v0.1 - First Working Version 1.1 Set Position
			//  Camera.mainCamera.transform.rotation = Quaternion.Euler(new Vector3(22 ,rotation.y + 90, 0));
			
			// v0.2 - Follow object set behind as child for camera position
			maincamera.transform.rotation = Quaternion.Euler(new Vector3(GameObject.Find("camera-pos").transform.rotation.x, rotation.y + 90, transform.rotation.z * rotation.z));
			
			GameObject.Find("camera-pos").renderer.enabled = false;
			//  GameObject.Find("camera-pos-2").renderer.enabled = false;
			//  setCameraOn = false;

			maincamera.transform.position = GameObject.Find("camera-pos").transform.position;

			// Gate-1 Camera
			// gatecam.transform.LookAt(GameObject.Find("camera-pos").transform.position);

			// Top Down View
			topdowncamera.transform.position = new Vector3(GameObject.Find("camera-pos").transform.position.x,GameObject.Find("camera-pos").transform.position.y + 400, GameObject.Find("camera-pos").transform.position.z);
			

		}	
    }
	
	void LateUpdate() {
       
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

    void Update ()
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
 
        //  S O U N D 

  		if (Input.GetKey("up") || Input.GetKey("w"))
  		{
  			isAccellerating = true;
 			audio.volume = 1;

  			if(!audio.isPlaying) {
		      audio.loop = true;
		     
		      audio.Play( );
		    }
  		}
  		else
  		{
			isAccellerating = false;
			//audio.Stop();

			//audio.volume -= 5;

			if(audio.volume > 0.01)
			{

				audio.volume -= 0.02f;
			}
  		}
            
        rigidbody.AddRelativeTorque(Vector3.up * turn * Time.deltaTime);
        rigidbody.AddRelativeForce(forwardDirection * theThrust * Time.deltaTime);
		
		//  L I G H T
		
		float phi = Time.time / duration * 2 * Mathf.PI;
        float amplitude = Mathf.Cos(phi) * 0.5F + 0.5F;
		
        GameObject.Find("beacon1").light.intensity = amplitude;
		GameObject.Find("beacon2").light.intensity = amplitude;

		GameObject.Find("backEngine-Light").light.intensity =  rigidbody.velocity.magnitude / 8;

		// Dist
		var go = GameObject.Find("gate1");
		distance1 = Vector3.Distance(go.transform.position, transform.position);

		if(distance1 < 39)
		{
			passgate1 = true;
		}

  		if(Input.GetKeyDown("e"))
  		{
  			currentCameraIndex++;

            if (currentCameraIndex > cameras.Length - 1)
            {
            	 currentCameraIndex = 0;
            }

            ChangeView();

    	}
    }
	
    // C a m e r a

	void ChangeView()
	{

	    currentCamera.enabled = false;

	    currentCamera = cameras[currentCameraIndex];

	    currentCamera.enabled = true;

	}

	// G U I 

	void OnGUI() {
       
		
		GUI.Button(new Rect(10, 10, 180, 20), "[v] " 
			+ rigidbody.velocity.magnitude.ToString("0.00"));
		//	+ "[t] " +turn.ToString("000.0");// + Camera.mainCamera.transform.rotation.y.ToString());
		
		GUI.Button(new Rect(10, 35, 180, 20), "[cr] " 
			+ maincamera.transform.rotation.ToString());
		
				GUI.Button(new Rect(10,55, 180, 20), "campos:" 
			+ GameObject.Find("camera-pos").transform.rotation);
		
		// hSliderValue = GUI.HorizontalSlider (new Rect (25, 95, 100, 30), hSliderValue, 0.0f, 10.0f);
		
		forwardThrust = GUI.HorizontalSlider (new Rect (25, 95, 100, 30), forwardThrust, 25000f, 100000.0f);
		
		GUI.Button(new Rect(10, 75, 180, 20), "[t] " 
			+ forwardThrust.ToString());
		
		
			GUI.Button(new Rect(10, 135, 180, 20), "[cp] " 
			+ maincamera.transform.position.ToString());
		
		// turn speed
		turnSpeed = GUI.HorizontalSlider (new Rect (25, 295, 100, 30), turnSpeed, 20000f, 40000.0f);
		
		GUI.Button(new Rect(24, 305, 180, 20), "[turn] " 
		+ turnSpeed.ToString());
		
				// turn speed
		//bankSpeed = GUI.HorizontalSlider (new Rect (225, 345, 100, 30), bankSpeed, 0.2f, 55f);
		
		//	GUI.Button(new Rect(224, 355, 150, 20), "[bank] " 
		//	+ bankSpeed.ToString());
		
				// bankAmount
		bankSpeed = GUI.HorizontalSlider (new Rect (25, 345, 100, 30), bankSpeed, 0.0f, 3f);
		
		GUI.Button(new Rect(24, 355, 180, 20), "[bS] " + bankSpeed.ToString());
		
		// bankAmount
		bankAmount = GUI.HorizontalSlider (new Rect (25, 395, 100, 30), bankAmount, 0.0f, 3f);
		
		GUI.Button(new Rect(24, 405, 180, 20), "[bA] " + bankAmount.ToString());
		
		GUI.Button(new Rect(155, 10, 180, 20), "[fHit] " + fHit.ToString());
		GUI.Button(new Rect(405, 10, 180, 20), "[bHit] " + bHit.ToString());

		GUI.Button(new Rect(655, 10, 180, 20), "[v-ax] " + Input.GetAxis("Vertical").ToString());


		GUI.Button(new Rect(855, 10, 180, 20), "[terrd] " + terrainDistance.ToString());

		//GUI.Button(new Rect(1155, 10, 180, 20), "[upw] " + forwardHit.point.ToString() + " - " + backwardHit.point.ToString());
        // Adjust to terrain angle
		//GUI.Button(new Rect(1155, 10, 180, 20), "[upw] " + diff.point.ToString());

		GUI.Button(new Rect(1155, 10, 180, 20), "[fhp] " + forwardHit.point.ToString());
		GUI.Button(new Rect(1155, 30, 180, 20), "[bhp] " + backwardHit.point.ToString());
        


        // GUI.Button(new Rect(110, 110, 180, 20), "[z-R] " + rotation.z.ToString("0.00"));
		GUI.Button(new Rect(1355, 10, 180, 20), "[tz] " +terrainAngle.ToString());

		GUI.Button(new Rect(1355, 40, 180, 20), "[->] " + isAccellerating.ToString() + "/" + audio.volume.ToString());
		
		// Distance
		GUI.Button(new Rect(1155, 60, 180, 20), "[d1] " + distance1.ToString());
		
		//Cameras
		GUI.Button(new Rect(1355, 80, 180, 20), "[cam] " + currentCameraIndex.ToString());

		GUI.Button(new Rect(1355, 100, 180, 20), "[pg1] " + passgate1.ToString());

		GUI.Button(new Rect(1355, 120, 180, 20), "[pg2] " + passgate2.ToString());

		Debug.DrawRay (forwardHit.point,new Vector3(111,111,111));
		
    }
}