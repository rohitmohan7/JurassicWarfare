using UnityEngine;

public class Galli : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Spine5,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8,Tail9,Tail10,Tail11,
  Arm1,Arm2,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot0,Right_Foot0;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Smallstep,Smallsplash,Chew,Bite,Idlecarn,Galli1,Galli2,Galli3,Galli4;
  Vector3 dir=Vector3.zero;
  
	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Galli1; break; case 1: painSnd=Galli2; break; case 2: painSnd=Galli3; break; case 3: painSnd=Galli4; break; }
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
			case "Bite": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(Bite, 0.75f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(0.8f, 1.0f); source[1].PlayOneShot(IsOnWater|IsInWater?Smallsplash:Smallstep, 1.0f);
				lastframe=currframe; IsDead=true; break;
			case "Food": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Chew, 0.5f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "GrowlA": int rnd1 = Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd1==0)source[0].PlayOneShot(Galli1, 1.0f);
				else source[0].PlayOneShot(Galli2, 1.0f);
				lastframe=currframe; break;
				case "GrowlB": int rnd2 = Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Galli3, 1.0f);
				else source[0].PlayOneShot(Galli4, 1.0f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!IsActive | AnimSpeed==0.0f) { body.Sleep(); return; }
		OnReset=false; OnAttack=false; IsConstrained=false;

		if(UseAI && Health!=0) { AICore(1, 2, 3, 0, 4, 5, 6); }// CPU
		else if(Health!=0) { GetUserInputs(1, 2, 3, 0, 4, 5, 6); }// Human
		else { anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(IsOnGround | IsInWater | IsOnWater)
    {
      if(!IsOnGround) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos(); anm.SetBool("OnGround", true);
      dir=new Vector3(transform.forward.x, 0, transform.forward.z);
    }
    else { ApplyGravity(); anm.SetBool("OnGround", false); }

		//Stopped
		if(OnAnm.IsName("Galli|IdleA") | OnAnm.IsName("Galli|Die"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Galli|Die")) { OnReset=true; if(!IsDead) { PlaySound("AtkB", 2); PlaySound("Die", 12); } }
		}

		//Jump
		else if(OnAnm.IsName("Galli|IdleJumpStart") | OnAnm.IsName("Galli|RunJumpStart") | OnAnm.IsName("Galli|JumpIdle") |
			OnAnm.IsName("Galli|IdleJumpEnd") | OnAnm.IsName("Galli|RunJumpEnd"))
		{
			if(OnAnm.IsName("Galli|IdleJumpStart") | OnAnm.IsName("Galli|RunJumpStart"))
			{
				if(OnAnm.normalizedTime > 0.4) Move(Vector3.up, 3, true); else OnJump=true;
        if(anm.GetInteger("Move").Equals(2)) Move(dir, 160);
        else if(anm.GetInteger("Move").Equals(1)) Move(dir,32);
        PlaySound("Step", 1); PlaySound("Step", 2);
			}
			else if(OnAnm.IsName("Galli|IdleJumpEnd") | OnAnm.IsName("Galli|RunJumpEnd"))
			{ 
        if(OnAnm.IsName("Galli|RunJumpEnd")) Move(dir, 160);
        body.velocity = new Vector3(body.velocity.x, 0.0f, body.velocity.z); OnJump=false;
				PlaySound("Step", 3); PlaySound("Step", 4); 
			}
      else if(!anm.GetInteger("Move").Equals(0)) Move(Vector3.Lerp(dir, Vector3.zero, 0.5f), 160);
		}

		//Forward
		else if(OnAnm.IsName("Galli|Walk") | OnAnm.IsName("Galli|WalkGrowl"))
		{
			Move(transform.forward, 32);
			if(OnAnm.IsName("Galli|Walk")){ PlaySound("Step", 6); PlaySound("Step", 14);}
			else if(OnAnm.IsName("Galli|WalkGrowl")) { PlaySound("GrowlB", 2); PlaySound("Step", 6); PlaySound("Step", 14); }
		}

		//Running
		else if(OnAnm.IsName("Galli|Run") | OnAnm.IsName("Galli|RunGrowl"))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*15.0f, 0.1f), -30f, 30f);
			Move(transform.forward, 160);
			if(OnAnm.IsName("Galli|Run")){ PlaySound("Step", 4); PlaySound("Step", 12); }
			else { PlaySound("GrowlB", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
		}

		//Backward
		else if(OnAnm.IsName("Galli|Walk-"))
		{
			if(OnAnm.normalizedTime > 0.25 && OnAnm.normalizedTime < 0.45 | 
			 OnAnm.normalizedTime > 0.75 && OnAnm.normalizedTime < 0.9) Move(-transform.forward, 32);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}
		
		//Strafe/Turn right
		else if(OnAnm.IsName("Galli|Strafe-"))
		{
			Move(transform.right, 16);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Galli|Strafe+"))
		{
			Move(-transform.right, 16);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}

		//Various
		else if(OnAnm.IsName("Galli|Sleep")) { OnReset=true; IsConstrained=true; PlaySound("Repose", 1); }
		else if(OnAnm.IsName("Galli|SitGrowl")) { IsConstrained=true; PlaySound("GrowlB", 3); }
		else if(OnAnm.IsName("Galli|SitIdle")) IsConstrained=true;
		else if(OnAnm.IsName("Galli|ToSit")) IsConstrained=true;
		else if(OnAnm.IsName("Galli|EatA")) { OnReset=true; PlaySound("Food", 5); PlaySound("Food", 7); }
		else if(OnAnm.IsName("Galli|EatB")) { OnReset=true; PlaySound("Step", 3); PlaySound("Step", 6); PlaySound("Step", 11); }
		else if(OnAnm.IsName("Galli|EatC")) OnReset=true;
		else if(OnAnm.IsName("Galli|IdleC")) { PlaySound("GrowlB", 4); PlaySound("Bite", 3); PlaySound("Bite", 8); }
		else if(OnAnm.IsName("Galli|IdleD")) { PlaySound("GrowlA", 1); PlaySound("GrowlA", 4); PlaySound("GrowlA", 8);}
		else if(OnAnm.IsName("Galli|Die-")) { OnReset=true; PlaySound("GrowlB", 1);  IsDead=false; }

    RotateBone(IkType.SmBiped, 60f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!IsActive) return; HeadPos=Head.GetChild(0).GetChild(0).position;
		float headZ =-headY*headX/Yaw_Max;
		Spine0.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine1.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine2.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine3.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine4.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine5.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Arm1.rotation*=Quaternion.Euler(headY*8, 0, 0);
		Arm2.rotation*=Quaternion.Euler(0, headY*8, 0);
		Neck0.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck1.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck2.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck3.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Head.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Tail0.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail1.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail2.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail3.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail4.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail5.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail6.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail7.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail8.rotation*=Quaternion.Euler(0, 0, -spineX);
    Tail9.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail10.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail11.rotation*=Quaternion.Euler(0, 0, -spineX);
		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(0, roll, 0);
    if(!IsDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.SmBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0);
	}
}

