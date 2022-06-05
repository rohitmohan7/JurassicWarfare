using UnityEngine;

public class Dimo : Creature
{
	public Transform Root,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Right_Wing0,Left_Wing0,Right_Wing1,Left_Wing1,Right_Hand,Left_Hand, 
	Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Wind,Hit_jaw,Hit_head,Hit_tail,Smallstep,Smallsplash,Idlecarn,Bite,Sniff2,Swallow,Medstep,Medsplash,Dimo1,Dimo2,Dimo3;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 3); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Dimo1; break; case 1: painSnd=Dimo2; break; case 2: painSnd=Dimo3; break; }
		ManageCollision(col, Pitch_Max, Crouch_Max, source, painSnd, Hit_jaw, Hit_head, Hit_tail);
	}
	void PlaySound(string name, int time)
	{
		if(time==currframe && lastframe!=currframe)
		{
			switch (name)
			{
			case "Step": source[1].pitch=Random.Range(0.75f, 1.25f); 
				if(IsInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else if(IsOnWater) source[1].PlayOneShot(Smallsplash, Random.Range(0.25f, 0.5f));
				else if(IsOnGround) source[1].PlayOneShot(Smallstep, Random.Range(0.25f, 0.5f));
				lastframe=currframe; break;
			case "Bite": source[1].pitch=Random.Range(1.5f, 1.75f); source[1].PlayOneShot(Bite, 0.5f);
				lastframe=currframe; break;
			case "Sniff": source[1].pitch=Random.Range(1.5f, 1.75f);
				if(IsInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else source[1].PlayOneShot(Sniff2, Random.Range(0.1f, 0.2f));
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(0.8f, 1.0f); source[1].PlayOneShot(IsOnWater|IsInWater?Medsplash:Medstep, 0.5f);
				lastframe=currframe; IsDead=true; break;
			case "Food": source[0].pitch=Random.Range(3.0f, 3.25f); source[0].PlayOneShot(Swallow, 0.1f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(3.0f, 3.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "Atk":int rnd = Random.Range(0, 4); source[0].pitch=Random.Range(1.0f, 1.75f);
				if(rnd==0) source[0].PlayOneShot(Dimo1, 1.0f);
				else if(rnd==1) source[0].PlayOneShot(Dimo2, 1.0f);
				lastframe=currframe; break;
			case "Growl": rnd = Random.Range(0, 2); source[0].pitch=Random.Range(1.5f, 1.75f);
				if(rnd==0) source[0].PlayOneShot(Dimo1, 0.5f);
				else source[0].PlayOneShot(Dimo3, 0.5f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!IsActive | AnimSpeed==0.0f) { body.Sleep(); return; }
    Vector3 dir=-Root.right; anm.SetBool("OnGround", IsOnGround);
		OnReset=false; OnAttack=false; IsOnLevitation=false; IsConstrained=false; OnJump=false;

		if(UseAI && Health!=0) { AICore(1, 2, 3, 0, 4, 5, 6); }// CPU
		else if(Health!=0) { GetUserInputs(1, 2, 3, 0, 4, 5, 6); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(IsInWater) { body.drag=4; body.angularDrag=4; ApplyYPos(); anm.SetInteger ("Move", 1); }
    else if(IsOnGround)
    {
      roll = Mathf.Lerp(roll, 0.0f, 0.1f); pitch = Mathf.Lerp(pitch, 0.0f, 0.1f);
      body.drag=4; body.angularDrag=4; ApplyYPos();
    }
    else if(Health==0) { body.drag=4; body.angularDrag=4; ApplyGravity(0.5f); }
    else { body.drag=1; body.angularDrag=1; } //in air drag

		//Stopped
		if(OnAnm.IsName("Dimo|IdleA") | OnAnm.IsName("Dimo|Die1") | OnAnm.IsName("Dimo|Die2") | OnAnm.IsName("Dimo|Fall"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Dimo|Die1")) { OnReset=true; if(!IsDead) { PlaySound("Growl", 1); PlaySound("Die", 11); } }
			else if(OnAnm.IsName("Dimo|Die2"))
			{ OnReset=true; if(!IsDead) PlaySound("Die", 0); }
			else if(OnAnm.IsName("Dimo|Fall"))
			{
				OnReset=true; IsOnLevitation=true;
				if(IsInWater) anm.SetBool("OnGround", true);
				if(OnAnm.normalizedTime<0.1f) source[0].PlayOneShot(Dimo2, 1.0f);
			} 
		}
		
		//Forward
		else if(OnAnm.IsName("Dimo|Walk"))
		{
			Move(transform.forward, 10);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Running
		else if(OnAnm.IsName("Dimo|Run") | OnAnm.IsName("Dimo|FlightToRun") | OnAnm.IsName("Dimo|RunToFlight"))
		{
			IsOnLevitation=true; Move(transform.forward, 40);
			PlaySound("Step", 5); PlaySound("Step", 6); PlaySound("Sniff", 7); PlaySound("Sniff", 8);
		}
		
		//Backward
		else if(OnAnm.IsName("Dimo|Walk-"))
		{
			Move(-transform.forward, 5);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}
		
		//Strafe/Turn right
		else if(OnAnm.IsName("Dimo|Strafe+"))
		{
			Move(transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Dimo|Strafe-"))
		{
			Move(-transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Takeoff
		else if(OnAnm.IsName("Dimo|Takeoff"))
		{
			if(OnAnm.normalizedTime > 0.5) { IsOnLevitation=true; OnJump=true; Move(Vector3.up, 50); }
			PlaySound("Sniff", 7); PlaySound("Sniff", 8);
		}

		//Fly
		else if(OnAnm.IsName("Dimo|Flight") | OnAnm.IsName("Dimo|FlightGrowl") |
		        OnAnm.IsName("Dimo|Glide") | OnAnm.IsName("Dimo|GlideGrowl"))
		{
			IsOnLevitation=true;
			roll = Mathf.Lerp(roll, -spineX*10.0f, Ang_T); pitch = Mathf.Lerp(pitch, Mathf.Clamp(anm.GetFloat("Pitch"),-0.75f, 1.0f)*90f, Ang_T);
      Move(-Root.right, (100+Mathf.Abs(anm.GetFloat("Pitch")*50f)));
			if(OnAnm.IsName("Dimo|Flight")) { PlaySound("Sniff", 5); PlaySound("Sniff", 6); }
			else if(OnAnm.IsName("Dimo|FlightGrowl")) { PlaySound("Atk", 2); PlaySound("Sniff", 5); }
			else if(OnAnm.IsName("Dimo|GlideGrowl")) PlaySound("Growl", 2);
		}
		
		//Fly - Stationary
		else if(OnAnm.IsName("Dimo|Statio") | OnAnm.IsName("Dimo|StatioGrowl") | OnAnm.IsName("Dimo|IdleD") | OnAnm.IsName("Dimo|FlyAtk"))
		{
			IsOnLevitation=true;
			roll = Mathf.Lerp(roll, 0.0f, Ang_T); pitch = Mathf.Lerp(pitch, 0.0f, Ang_T);
			Move(Vector3.up, 100*-anm.GetFloat("Pitch")); //fly up/down
			if(IsOnGround&&OnAnm.IsName("Dimo|FlyAtk")) Move(Vector3.up, 50);//takeoff
			if(anm.GetInteger("Move")>0 && anm.GetInteger("Move")<4) Move(transform.forward, 100); //fly forward
			else if(anm.GetInteger("Move")== -1) Move(-transform.forward, 100); //fly backward
			else if(anm.GetInteger("Move")== -10) Move(transform.right, 100); //fly right
			else if(anm.GetInteger("Move") == 10) Move(-transform.right, 100); //fly left

			if(OnAnm.IsName("Dimo|StatioGrowl")) PlaySound("Atk", 2);
			else if(OnAnm.IsName("Dimo|IdleD")) { PlaySound("Atk", 2); PlaySound("Step", 10); }
			else if(OnAnm.IsName("Dimo|FlyAtk")) { OnAttack=true; PlaySound("Atk", 3); PlaySound("Bite", 8); }
			else { PlaySound("Sniff", 5); PlaySound("Sniff", 6); }
		}

		//Various
		else if(OnAnm.IsName("Dimo|Landing")) { IsOnLevitation=true; PlaySound("Step", 2); PlaySound("Step", 3); }
		else if(OnAnm.IsName("Dimo|IdleB")) PlaySound("Atk", 2);
		else if(OnAnm.IsName("Dimo|IdleC")) { OnReset=true; IsConstrained=true; }
		else if(OnAnm.IsName("Dimo|EatA")) { OnReset=true; IsConstrained=true; PlaySound("Food", 1); }
		else if(OnAnm.IsName("Dimo|EatB")) { OnReset=true; IsConstrained=true; PlaySound("Bite", 0); }
		else if(OnAnm.IsName("Dimo|EatC")) OnReset=true;
		else if(OnAnm.IsName("Dimo|ToSleep")){ OnReset=true; IsConstrained=true; }
		else if(OnAnm.IsName("Dimo|Sleep")) { OnReset=true; IsConstrained=true; PlaySound("Repose", 1); }
		else if(OnAnm.IsName("Dimo|Die-")) { IsConstrained=true; PlaySound("Atk", 2);  IsDead=false; }

    RotateBone(IkType.Flying, 64f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!IsActive) return; HeadPos=Head.GetChild(0).GetChild(0).position;
		Root.rotation*= Quaternion.Euler(roll, pitch, 0);
		Right_Wing0.rotation*= Quaternion.Euler(roll/2, Mathf.Clamp(roll, -35, 0), Mathf.Clamp(-pitch, -35, 0));
		Left_Wing0.rotation*= Quaternion.Euler(roll/2, Mathf.Clamp(-roll, -35, 0), Mathf.Clamp(pitch, 0, 35));
		Right_Wing0.GetChild(0).rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(pitch, 0, 90)+Mathf.Abs(roll)/3);
		Left_Wing0.GetChild(0).rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(-pitch, -90, 0)-Mathf.Abs(roll)/3);
		Right_Hand.rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(-pitch, -90, 0)-Mathf.Abs(roll)/2);
		Left_Hand.rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(pitch, 0, 90)+Mathf.Abs(roll)/2);
		float headZ =headY*headX/Yaw_Max;
		Neck0.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck1.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck2.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck3.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Head.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Tail0.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail1.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail2.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail3.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail4.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail5.rotation*= Quaternion.Euler(0, 0, -spineX);
    if(!IsDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(0, lastHit, 0);
		//Check for ground layer
		GetGroundPos(IkType.Flying, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Wing0, Right_Wing1, Right_Hand, Left_Wing0, Left_Wing1, Left_Hand);
    anm.SetBool("OnGround", IsOnGround);
	}
}










