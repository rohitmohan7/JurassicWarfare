using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
  #region VARIABLES
  [Space (10)] [Header("ARTIFICIAL INTELLIGENCE")]
	public bool UseAI=false;
	const string PathHelp=
	"Use gameobjects as waypoints to define a path for this creature by \n"+
	"taking into account the priority between autonomous AI and its path.";
	const string WaypointHelp=
	"Place your waypoint gameobject in a reacheable position.\n"+
	"Don't put a waypoint in air if the creature are not able to fly";
	const string PriorityPathHelp=
	"Using a priority of 100% will disable all autonomous AI for this waypoint\n"+
	"Obstacle avoid AI and custom targets search still enabled";
	const string TargetHelp=
	"Use gameobjects to assign a custom enemy/friend for this creature\n"+
	"Can be any kind of gameobject e.g : player, other creature.\n"+
	"The creature will include friend/enemy goals in its search. \n"+
	"Enemy: triggered if the target is in range. \n"+
	"Friend: triggered when the target moves away.";
	const string MaxRangeHelp=
	"If MaxRange is zero, range is infinite. \n"+
	"Creature will start his attack/tracking once in range.";
	//Path editor
	[Space (10)]
  [Tooltip(PathHelp)] public List<_PathEditor> PathEditor;
	[HideInInspector] public int nextPath=0;
	[HideInInspector] public enum PathType { Walk, Run };
  [HideInInspector] public enum TargetAction { None, Sleep, Eat, Drink };
	[System.Serializable] public struct _PathEditor
	{
		[Tooltip(WaypointHelp)] public GameObject _Waypoint;
		public PathType _PathType;
    public TargetAction _TargetAction;
		[Tooltip(PriorityPathHelp)] [Range(1, 100)] public int _Priority;
   
    public _PathEditor(GameObject Waypoint, PathType PathType, TargetAction TargetAction, int Priority)
    {_Waypoint=Waypoint; _PathType=PathType; _TargetAction=TargetAction; _Priority=Priority; }
	}

	//Target editor
	[Space (10)]
  [Tooltip(TargetHelp)]  public List< _TargetEditor> TargetEditor;
	[HideInInspector] public enum TargetType { Enemy, Friend };
	[System.Serializable] public struct _TargetEditor
	{
		public GameObject _GameObject;
		public TargetType _TargetType;
		[Tooltip(MaxRangeHelp)]
		public int MaxRange;
	}

  [Space (10)] [Header("CREATURE SETTINGS")]
	public Skin bodyTexture;
	public Eyes eyesTexture;
  [Space (5)]
	[Range(0.0f, 100.0f)] public float Health=100f;
	[Range(0.0f, 100.0f)] public float Water=100f;
	[Range(0.0f, 100.0f)] public float Food=100f;
	[Range(0.0f, 100.0f)] public float Stamina=100f;
  [Space (5)]
	[Range(1.0f, 10.0f)] public float DamageMultiplier=1.0f;
	[Range(1.0f, 10.0f)] public float ArmorMultiplier=1.0f;
	[Range(0.0f, 2.0f)] public float AnimSpeed=1.0f;
  public bool Herbivorous, CanAttack, CanHeadAttack, CanTailAttack, CanWalk, CanJump, CanFly, CanSwim, LowAltitude, CanInvertBody;
  public float BaseMass=1, Ang_T=0.025f, Crouch_Max=0, Yaw_Max=0, Pitch_Max=0;
  
  [Space (20)] [Header("COMPONENTS AND TEXTURES")]
  public Rigidbody body;
  public LODGroup lod;
  public Animator anm;
  public AudioSource[] source;
  public SkinnedMeshRenderer[] rend;
  public Texture[] skin, eyes;
  public enum Skin {SkinA, SkinB, SkinC};
  public enum Eyes {Type0, Type1, Type2, Type3, Type4, Type5, Type6, Type7, Type8, Type9, Type10, Type11, Type12, Type13, Type14, Type15};
  [Space (20)] [Header("TRANSFORMS AND SOUNDS")]
  public Transform Head;

  [HideInInspector] public Manager main=null;
  [HideInInspector] public AnimatorStateInfo OnAnm;
  [HideInInspector] public bool IsActive, IsVisible, IsDead, IsOnGround, IsOnWater, IsInWater, IsConstrained, IsOnLevitation;
  [HideInInspector] public bool OnAttack, OnJump, OnCrouch, OnReset, OnInvert, OnHeadMove, OnAutoLook, OnTailAttack;
  [HideInInspector] public int rndX, rndY, rndMove, rndIdle, loop;
  [HideInInspector] public string behavior, specie;
  [HideInInspector] public GameObject objTGT=null, objCOL=null;
  [HideInInspector] public Vector3 HeadPos, posCOL=Vector3.zero, posTGT=Vector3.zero, lookTGT=Vector3.zero, boxscale=Vector3.zero, normal=Vector3.zero;
  [HideInInspector] public Quaternion angTGT=Quaternion.identity, normAng=Quaternion.identity;
  [HideInInspector] public float currframe, lastframe, lastHit;
  [HideInInspector] public float crouch, spineX, spineY, headX, headY, pitch, roll, reverse;
  [HideInInspector] public float posY, waterY, withersSize, size, speed;
  [HideInInspector] public float behaviorCount, distTGT, delta, actionDist, angleAdd, avoidDelta, avoidAdd;
  const int enemyMaxRange=50, waterMaxRange=200, foodMaxRange=200, friendMaxRange=200, preyMaxRange=200;
  public bool networkObject = true;
  PlayerCreature playerCreature;

	//Input actions
	private const int 
  MoveX=0, MoveY=1, Attack=2, Interact=3, Sleep=4, MoveZ=5, Run=6,
  CamX=7, CamY=8, CamZ=9, Focus=10, Target=11, Map=12, YesNo=13, Menu=14;
  //IK TYPES
  public enum IkType { None, Convex, Quad, Flying, SmBiped, LgBiped }
	// IK goal position
	Vector3 FR_HIT, FL_HIT, BR_HIT, BL_HIT;
  // Terrain normals
  Vector3 FR_Norm=Vector3.up, FL_Norm=Vector3.up, BR_Norm=Vector3.up, BL_Norm=Vector3.up;
	//Back Legs
	float BR1, BR2, BR3, BR_Add; //Right
	float BL1, BL2, BL3, BL_Add; //Left
  float alt1, alt2, a1, a2, b1, b2, c1, c2;
	//Front Legs
	float FR1, FR2, FR3, FR_Add; //Right
	float FL1, FL2, FL3, FL_Add; //Left
  float alt3, alt4, a3, a4, b3, b4, c3, c4;
  #endregion
  #region CREATURE INITIALIZATION
	void Start()
	{
		playerCreature = GetComponent<PlayerCreature>();
		main =Camera.main.transform.GetComponent<Manager>(); //Get manager compononent
		SetScale(transform.localScale.x);//Start scale 
		SetMaterials(bodyTexture.GetHashCode(), eyesTexture.GetHashCode());//Start materials
		loop=Random.Range(0, 100);//Randomise start action
		specie=transform.GetChild(0).name;//Creature specie name
	}
#endregion
  #region CREATURE SETUP FUNCTIONS
  //AI on/off
  public void SetAI(bool UseAI) { this.UseAI=UseAI; if(!this.UseAI) { posTGT=Vector3.zero; objTGT=null; objCOL=null; behaviorCount=0; } }
  //Change materials
  #if UNITY_EDITOR
  void OnDrawGizmosSelected()
	{
		foreach (SkinnedMeshRenderer o in rend)
		{
			if(o.sharedMaterials[0].mainTexture!=skin[bodyTexture.GetHashCode()]) o.sharedMaterials[0].mainTexture=skin[bodyTexture.GetHashCode()];
      if(o.sharedMaterials[1].mainTexture!=eyes[eyesTexture.GetHashCode()]) o.sharedMaterials[1].mainTexture=eyes[eyesTexture.GetHashCode()];
		}
	}
	#endif
  
	public void SetMaterials(int bodyindex, int eyesindex)
	{
		bodyTexture= (Skin) bodyindex; eyesTexture= (Eyes) eyesindex;
		foreach (SkinnedMeshRenderer o in rend)
		{
			o.materials[0].mainTexture = skin[bodyindex];
			o.materials[1].mainTexture = eyes[eyesindex];
		}
	}

  //Creature size
	public void SetScale(float resize)
	{
    size=resize;
		transform.localScale=new Vector3(resize, resize, resize); //creature size
    body.mass=BaseMass*size; //creature mass based on size
		withersSize = (transform.GetChild(0).GetChild(0).position-transform.position).magnitude; //At the withers altitude
		boxscale = rend[0].bounds.extents; //bounding box scale
    source[0].maxDistance=Mathf.Lerp(50f, 300f, size);
    source[1].maxDistance=Mathf.Lerp(50f, 150f, size);
	}
  #endregion FUNCTION
  #region CREATURE STATUS UPDATE
  public void StatusUpdate()
	{
		// Check if this creature are visible or near the camera, if not, and game are not in realtime mode, turn off all activity
    IsVisible=false;
    foreach (SkinnedMeshRenderer o in rend) { if(o.isVisible) IsVisible=true; }
    if(!main.RealtimeGame)
    {
      float dist=(main.transform.position-transform.position).magnitude;
      if(!IsVisible && dist>100f) { IsActive=false; anm.cullingMode=AnimatorCullingMode.CullCompletely; return; }
      else { IsActive=true; anm.cullingMode=AnimatorCullingMode.AlwaysAnimate; }
    } else { IsActive=true; anm.cullingMode=AnimatorCullingMode.AlwaysAnimate; }


    anm.speed=AnimSpeed;
    if(anm.GetNextAnimatorClipInfo(0).Length!=0) OnAnm=anm.GetNextAnimatorStateInfo(0);
    else if(anm.GetCurrentAnimatorClipInfo(0).Length!=0) OnAnm=anm.GetCurrentAnimatorStateInfo(0);

		if(currframe==15f | anm.GetAnimatorTransitionInfo(0).normalizedTime>0.5) { currframe=0.0f; lastframe=-1; }
		else currframe = Mathf.Round((OnAnm.normalizedTime % 1.0f) * 15f);

		//Manage health bar
		if(Health>0.0f)
		{
			if(loop>100)	
			{
        if(CanSwim)
        { 
          if(anm.GetInteger("Move")!=0) Food=Mathf.Clamp(Food-0.01f, 0.0f, 100f);
          if(IsInWater | IsOnWater) { Stamina=Mathf.Clamp(Stamina+1.0f, 0.0f, 100f); Water=Mathf.Clamp(Water+1.0f, 0.0f, 100f); }  
          else if(CanWalk) { Stamina=Mathf.Clamp(Stamina-0.01f, 0.0f, 100f);  Water=Mathf.Clamp(Water-0.01f, 0.0f, 100f); }
          else { Stamina=Mathf.Clamp(Stamina-1.0f, 0.0f, 100f); Water=Mathf.Clamp(Water-1.0f, 0.0f, 100f); Health=Mathf.Clamp(Health-1.0f, 0.0f, 100f); }
        }
        else
        { 
          if(anm.GetInteger("Move")!=0) { Stamina=Mathf.Clamp(Stamina-0.01f, 0.0f, 100f); Water=Mathf.Clamp(Water-0.01f, 0.0f, 100f); Food=Mathf.Clamp(Herbivorous?Food-0.1f:Food-0.01f, 0.0f, 100f); }
          if(IsInWater) { Stamina=Mathf.Clamp(Stamina-1.0f, 0.0f, 100f); Health=Mathf.Clamp(Health-1.0f, 0.0f, 100f); } 
        }

        if(Food==0.0f | Stamina==0.0f | Water==0.0f) Health=Mathf.Clamp(Health-0.1f, 0.0f, 100f); else Health=Mathf.Clamp(Health+0.1f, 0.0f, 100f);
				loop=0;
			}
      else loop++;
		}
		else
		{
			Water=0.0f; Food=0.0f; Stamina=0.0f; behavior="Dead";
      if(main.TimeAfterDead==0) return;
			if(behaviorCount>0) behaviorCount=0;
			else if(behaviorCount==-main.TimeAfterDead)
			{
				//Delete from list and destroy gameobject
				if(main.selected>=main.creaturesList.IndexOf(transform.gameObject)) { if(main.selected>0) main.selected--; }
				main.creaturesList.Remove(transform.gameObject); Destroy(transform.gameObject);
			}
			else behaviorCount--;
		}
	}
#endregion
  #region COLLISIONS AND DAMAGES
  //Spawn blood particle
  void SpawnBlood(Vector3 position)
	{
		ParticleSystem particle=Instantiate(main.blood, position, Quaternion.Euler(-90, 0, 0))as ParticleSystem; //spawn particle
		particle.transform.localScale=new Vector3(boxscale.z/10, boxscale.z/10, boxscale.z/10); //particle size
		Destroy(particle.gameObject, 1.0f); //destroy particle
	}

  //Collisions
  void OnCollisionExit() { objCOL=null; }
  public void ManageCollision(Collision col, float pitch_max, float crouch_max, AudioSource[] source, AudioClip pain, AudioClip Hit_jaw, AudioClip Hit_head, AudioClip Hit_tail)
	{
		//Collided with a Creature
	  if(col.transform.root.tag.Equals("Creature"))
		{
			Creature other=col.gameObject.GetComponent<Creature>(); objCOL=other.gameObject;

      //Is Player?
		  if(!UseAI && OnAttack)
		  {
			  objTGT=other.gameObject; other.objTGT=transform.gameObject;
			  behaviorCount=500; other.behaviorCount=500;
			  if(other.specie==specie) { behavior="Contest"; other.behavior="Contest"; }
			  else if(other.CanAttack) { behavior="Battle"; other.behavior="Battle"; }
			  else { behavior="Battle"; other.behavior="ToFlee"; }
		  }
			//Eat ?
      if(IsDead && lastHit==0 && other.IsConstrained)
      { 
        SpawnBlood(col.GetContact(0).point);
        body.AddForce(-other.transform.forward, ForceMode.Acceleration);
        lastHit=25; return;
      }
      //Attack ?
			else if(lastHit==0 && other.OnAttack)
			{
				float baseDamages=Mathf.Clamp((other.BaseMass*other.DamageMultiplier) / (BaseMass*ArmorMultiplier), 10,100);

				if(col.collider.gameObject.name.StartsWith("jaw")) //bite damage
				{
          SpawnBlood(col.GetContact(0).point);
          if(!IsInWater) body.AddForce(-col.GetContact(0).normal*other.body.mass/4, ForceMode.Acceleration);
          lastHit=50; if(IsDead) return;
          source[0].pitch=Random.Range(1.0f, 1.5f); source[0].PlayOneShot(pain, 1.0f);
					source[1].PlayOneShot(Hit_jaw, Random.Range(0.1f, 0.4f));
					Health=Mathf.Clamp(Health-baseDamages, 0.0f, 100f);
				}
				else if(col.collider.gameObject.name.Equals("head")) //head damage
				{
          SpawnBlood(col.GetContact(0).point); 
					if(!IsInWater) body.AddForce(col.GetContact(0).normal*other.body.mass/4, ForceMode.Acceleration);
          lastHit=50; if(IsDead) return;
          source[0].pitch=Random.Range(1.0f, 1.5f); source[0].PlayOneShot(pain, 1.0f);
					source[1].PlayOneShot(Hit_head, Random.Range(0.1f, 0.4f));
					if(!Herbivorous) Health=Mathf.Clamp(Health-baseDamages, 0.0f, 100f);
					else Health=Mathf.Clamp(Health-baseDamages/10, 0.0f, 100f);
				}
				else  if(!col.collider.gameObject.name.Equals("root")) //tail damage
				{
					SpawnBlood(col.GetContact(0).point);
          if(!IsInWater) body.AddForce(col.GetContact(0).normal*other.body.mass/4, ForceMode.Acceleration);
          lastHit=50; if(IsDead) return;
          source[0].pitch=Random.Range(1.0f, 1.5f); source[0].PlayOneShot(pain, 1.0f);
          source[1].PlayOneShot(Hit_tail, Random.Range(0.1f, 0.4f));
					if(!Herbivorous) Health=Mathf.Clamp(Health-baseDamages, 0.0f, 100f);
					else Health-= Health=Mathf.Clamp(Health-baseDamages/10, 0.0f, 100f);
				 }
			}

      //Not the current target creature, avoid and look at
      if(objTGT!=objCOL) { lookTGT=other.Head.position; posCOL=col.GetContact(0).point; }
		}
		//Collided with world, avoid
		else if(col.gameObject!=objTGT)
    { 
       objCOL=col.gameObject;
       posCOL=col.GetContact(0).point;
    }
	}
  #endregion
  #region ENVIRONEMENTAL CHECKING
  public void GetGroundPos(IkType ikType, Transform RLeg1=null, Transform RLeg2=null, Transform RLeg3=null, Transform LLeg1=null, Transform LLeg2=null, Transform LLeg3=null,
                     Transform RArm1=null, Transform RArm2=null, Transform RArm3=null, Transform LArm1=null, Transform LArm2=null, Transform LArm3=null, float FeetOffset=0.0f)
  {
	  posY=-transform.position.y;
    #region Use Raycast
    if(main.UseRaycast)
    {
      if(ikType==IkType.None | IsDead | IsInWater | !IsOnGround)
      {
          if(Physics.Raycast(transform.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit hit, withersSize*1.5f,1<<0))
          { posY=hit.point.y; normal=hit.normal; IsOnGround=true; } else IsOnGround=false;
      }
      else if(ikType>=IkType.SmBiped) // Biped
      {
        if(Physics.Raycast((transform.position+transform.forward*2)+Vector3.up, -Vector3.up, out RaycastHit hit, withersSize*2.0f,1<<0))
        { posY=hit.point.y; normal=hit.normal; }
			  if(Physics.Raycast(RLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BR, withersSize*2.0f,1<<0))
        { IsOnGround=true; BR_HIT=BR.point; BR_Norm=BR.normal; } else BR_HIT.y=-transform.position.y;
          if(Physics.Raycast(LLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BL, withersSize*2.0f,1<<0))
        { IsOnGround=true;  BL_HIT=BL.point; BL_Norm=BL.normal; } else BL_HIT.y=-transform.position.y;

        if(posY>BL_HIT.y && posY>BR_HIT.y) posY=Mathf.Max(BL_HIT.y,BR_HIT.y); else posY=Mathf.Min(BL_HIT.y,BR_HIT.y);
        normal=(BL_Norm+BR_Norm+normal)/3;
      }
      else if(ikType==IkType.Flying) // Flying
      {
        IsOnGround=false;
        if(Physics.Raycast(transform.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit hit, withersSize*4.0f,1<<0))
        {
          normal=hit.normal; IsOnGround=true;
        if(Physics.Raycast(RArm3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit FR, withersSize*4.0f,1<<0))
          { FR_HIT=FR.point; FR_Norm=FR.normal; } else { FR_Norm=hit.normal; FR_HIT.y=-transform.position.y; }
        if(Physics.Raycast(LArm3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit FL, withersSize*4.0f,1<<0))
          { FL_HIT=FL.point; FL_Norm=FL.normal; } else { FL_Norm=hit.normal; FL_HIT.y=-transform.position.y;}
        if(Physics.Raycast(RLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BR, withersSize*4.0f,1<<0))
          { BR_HIT=BR.point; BR_Norm=BR.normal; } else { BR_Norm=hit.normal; BR_HIT.y=-transform.position.y; }
        if(Physics.Raycast(LLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BL, withersSize*4.0f,1<<0))
          { BL_HIT=BL.point; BL_Norm=BL.normal;  }else { BL_Norm=hit.normal; BL_HIT.y=-transform.position.y; }
          posY=hit.point.y;
        }
          
      }
      else //Quadruped
      {
        IsOnGround=false;
        if(Physics.Raycast(RArm3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit FR, withersSize*2.0f,1<<0))
          { FR_HIT=FR.point; FR_Norm=FR.normal; IsOnGround=true; } else FR_HIT.y=-transform.position.y;
        if(Physics.Raycast(LArm3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit FL, withersSize*2.0f,1<<0))
          { FL_HIT=FL.point; FL_Norm=FL.normal; IsOnGround=true; } else FL_HIT.y=-transform.position.y;
        if(Physics.Raycast(RLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BR, withersSize*2.0f,1<<0))
          { BR_HIT=BR.point; BR_Norm=BR.normal; IsOnGround=true; } else BR_HIT.y=-transform.position.y;
        if(Physics.Raycast(LLeg3.position+Vector3.up*withersSize, -Vector3.up, out RaycastHit BL, withersSize*2.0f,1<<0))
          { BL_HIT=BL.point; BL_Norm=BL.normal; IsOnGround=true; } else BL_HIT.y=-transform.position.y;

        if(ikType==IkType.Convex)
        {
          if(IsConstrained) posY=Mathf.Min(BR_HIT.y,BL_HIT.y,FR_HIT.y,FL_HIT.y);
          else posY=(BR_HIT.y+BL_HIT.y+FR_HIT.y+FL_HIT.y)/4;
        }
        else
        {
          if(IsConstrained | !main.UseIK) posY=Mathf.Min(BR_HIT.y,BL_HIT.y,FR_HIT.y,FL_HIT.y);
          else posY=(BR_HIT.y+BL_HIT.y+FR_HIT.y+FL_HIT.y-size)/4;
        }

        normal=Vector3.Cross(FR_HIT-BL_HIT,BR_HIT-FL_HIT).normalized;
      }
		}
    #endregion
    #region Terrain Only
    else
    {
      if(ikType==IkType.None | IsDead | IsInWater | !IsOnGround)
      {
        float x = ((transform.position.x -main.T.transform.position.x) / main.T.terrainData.size.x ) * main.tres;
		    float y = ((transform.position.z -main.T.transform.position.z) / main.T.terrainData.size.z ) * main.tres;
		    normal=main.T.terrainData.GetInterpolatedNormal(x/main.tres, y/main.tres);
        posY=main.T.SampleHeight(transform.position)+main.T.GetPosition().y;
      }
      else if(ikType>=IkType.SmBiped) // Biped
      {
		    BR_HIT=new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position)+main.tpos.y, RLeg3.position.z);
        float x = ((RLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres, y = ((RLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BR_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        BL_HIT=new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position)+main.tpos.y, LLeg3.position.z);
		    x = ((LLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres; y = ((LLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BL_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);

        if(posY>BL_HIT.y && posY>BR_HIT.y) posY=Mathf.Max(BL_HIT.y,BR_HIT.y);  else posY=Mathf.Min(BL_HIT.y,BR_HIT.y);
        normal=(BL_Norm+BR_Norm+normal)/3;
      }
      else if(ikType==IkType.Flying) // Flying
      {
        float x = ((transform.position.x -main.T.transform.position.x) / main.T.terrainData.size.x ) * main.tres;
		    float y = ((transform.position.z -main.T.transform.position.z) / main.T.terrainData.size.z ) * main.tres;
        normal=main.T.terrainData.GetInterpolatedNormal(x/main.tres, y/main.tres);
        posY=main.T.SampleHeight(transform.position)+main.T.GetPosition().y;

		    BR_HIT=new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position)+main.tpos.y, RLeg3.position.z);
        x = ((RLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres;  y = ((RLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BR_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        BL_HIT=new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position)+main.tpos.y, LLeg3.position.z);
		    x = ((LLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres; y = ((LLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BL_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        FR_HIT=new Vector3(RArm3.position.x, main.T.SampleHeight(RArm3.position)+main.tpos.y, RArm3.position.z);
		    x = ((RArm3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres;  y = ((RArm3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    FR_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        FL_HIT=new Vector3(LArm3.position.x, main.T.SampleHeight(LArm3.position)+main.tpos.y, LArm3.position.z);
		    x = ((LArm3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres;  y = ((LArm3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    FL_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
      }
      else //Quadruped
      {
		    BR_HIT=new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position)+main.tpos.y, RLeg3.position.z);
        float x = ((RLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres, y = ((RLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BR_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        BL_HIT=new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position)+main.tpos.y, LLeg3.position.z);
		    x = ((LLeg3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres; y = ((LLeg3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    BL_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        FR_HIT=new Vector3(RArm3.position.x, main.T.SampleHeight(RArm3.position)+main.tpos.y, RArm3.position.z);
		    x = ((RArm3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres; y = ((RArm3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    FR_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);
        FL_HIT=new Vector3(LArm3.position.x, main.T.SampleHeight(LArm3.position)+main.tpos.y, LArm3.position.z);
		    x = ((LArm3.position.x - main.tpos.x) / main.tdata.size.x ) * main.tres; y = ((LArm3.position.z - main.tpos.z) / main.tdata.size.z ) * main.tres;
		    FL_Norm=main.tdata.GetInterpolatedNormal(x/main.tres, y/main.tres);

        if(ikType==IkType.Convex)
        {
          if(IsConstrained) posY=Mathf.Min(BR_HIT.y,BL_HIT.y,FR_HIT.y,FL_HIT.y);
          else posY=(BR_HIT.y+BL_HIT.y+FR_HIT.y+FL_HIT.y)/4;
        }
        else
        {
          if(IsConstrained | !main.UseIK) posY=Mathf.Min(BR_HIT.y,BL_HIT.y,FR_HIT.y,FL_HIT.y);
          else posY=(BR_HIT.y+BL_HIT.y+FR_HIT.y+FL_HIT.y-size)/4;
        }
        normal=Vector3.Cross(FR_HIT-BL_HIT,BR_HIT-FL_HIT).normalized;
      }
	}
    #endregion
    #region Set status
    //Set status
    if((transform.position.y-size)<=posY) IsOnGround=true; else IsOnGround=false; //On ground?
    waterY=main.WaterAlt-crouch; //Check for water altitude
    if((transform.position.y)<waterY && body.worldCenterOfMass.y>waterY) IsOnWater=true; else IsOnWater=false; //On water ?
    if(body.worldCenterOfMass.y<waterY) IsInWater=true; else IsInWater=false; // In water ?

    //Setup Rigidbody
    if(IsDead)
    {
      body.maxDepenetrationVelocity=0.25f;
      body.constraints=RigidbodyConstraints.None;
    }
    else if(IsConstrained)
    {
      body.maxDepenetrationVelocity=0.0f; crouch=0.0f;
      body.constraints=RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    else
    {
      body.maxDepenetrationVelocity=5.0f;
      if(lastHit==0) body.constraints=RigidbodyConstraints.FreezeRotationZ;
      else body.constraints=RigidbodyConstraints.None;
    }
    
    //Setup Y position and rotation
    if(IsOnGround && !IsInWater) //On Ground outside water
    {
      Quaternion n=Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
      if(!CanFly)
      {
        float rx = Mathf.DeltaAngle(n.eulerAngles.x, 0.0f), rz = Mathf.DeltaAngle(n.eulerAngles.z, 0.0f);
        float pitch=Mathf.Clamp(rx, -45f, 45f), roll=Mathf.Clamp(rz, -10f, 10f);
        normAng=Quaternion.Euler(-pitch, anm.GetFloat("Turn"), -roll);
      }
      else normAng=Quaternion.Euler(n.eulerAngles.x, anm.GetFloat("Turn"), n.eulerAngles.z); posY-=crouch;
    }
    else if(IsInWater | IsOnWater) //On Water or In water
    { normAng=Quaternion.Euler(0, anm.GetFloat("Turn"), 0); posY=waterY-body.centerOfMass.y; }
    else //In Air
    { normAng=Quaternion.Euler(0, anm.GetFloat("Turn"), 0); posY=-transform.position.y; }

    if(!IsVisible | !main.UseIK) return;
    switch (ikType)
    {
      case IkType.None: break;
      case IkType.Convex: Convex(RLeg1, RLeg2, RLeg3, LLeg1, LLeg2, LLeg3, RArm1, RArm2, RArm3, LArm1, LArm2, LArm3); break;
      case IkType.Quad: Quad(RLeg1, RLeg2, RLeg3, LLeg1, LLeg2, LLeg3, RArm1, RArm2, RArm3, LArm1, LArm2, LArm3, FeetOffset); break;
      case IkType.Flying: Flying(RLeg1, RLeg2, RLeg3, LLeg1, LLeg2, LLeg3, RArm1, RArm2, RArm3, LArm1, LArm2, LArm3); break;
      case IkType.SmBiped: SmBiped(RLeg1, RLeg2, RLeg3, LLeg1, LLeg2, LLeg3); break;
      case IkType.LgBiped: LgBiped(RLeg1, RLeg2, RLeg3, LLeg1, LLeg2, LLeg3); break;
    }
    #endregion
  }
  #endregion
  #region PHYSICAL FORCES
  public void ApplyGravity(float multiplier=1.0f)
  {
    body.AddForce((Vector3.up*size)*(body.velocity.y>0?-20*body.drag:-50*body.drag)*multiplier, ForceMode.Acceleration);
  }
  public void ApplyYPos()
  {
    if(IsOnGround && (Mathf.Abs(normal.x)>main.MaxSlope | Mathf.Abs(normal.z)>main.MaxSlope))
    { body.AddForce(new Vector3(normal.x, -normal.y, normal.z)*64, ForceMode.Acceleration); behaviorCount=0; }
    body.AddForce(Vector3.up*Mathf.Clamp(posY-transform.position.y,-size,size), ForceMode.VelocityChange);
  }
  public void Move(Vector3 dir, float force=0, bool jump=false)
  {
    if(CanAttack && anm.GetBool("Attack").Equals(true))
    { force*=1.5f; transform.rotation=Quaternion.Lerp(transform.rotation, normAng, Ang_T*2);
    } else transform.rotation=Quaternion.Lerp(transform.rotation, normAng, Ang_T);

    if(dir!=Vector3.zero)
    {
      if(!CanSwim && !IsOnGround)
      {
        if(IsInWater | IsOnWater) force/=8;
        else if(!CanFly && !OnJump) force/=8;
        else force/=(4/body.drag);
      }
      else force/=(4/body.drag);

      body.AddForce(dir*force*speed, jump?ForceMode.VelocityChange:ForceMode.Acceleration);
    }
  }
  #endregion
  #region LERP SKELETON ROTATION
  public void RotateBone(IkType ikType, float maxX, float maxY=0, bool CanMoveHead=true, float t=0.5f)
  {
    //Freeze all
    if(AnimSpeed==0.0f) return;

    //Slowdown on turning
    if(!OnAttack && !OnJump)
    { speed=size*anm.speed*(1.0f-Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y,anm.GetFloat("Turn")))/135f); }
  
    //Lerp feet position
    if(main.UseIK && ikType!=IkType.None)
    {
      float s;
      switch (ikType)
      {
        case IkType.Convex:
          s=0.1f;
          if(!IsConstrained && !IsDead && IsOnGround && !IsInWater)
		      {
			      FR1=Mathf.Lerp(FR1, Mathf.Clamp(-alt1, -55, 0), s); FR2=Mathf.Lerp(FR2, b1, s); FR3=Mathf.Lerp(FR3, c1, s);
			      FL1=Mathf.Lerp(FL1, Mathf.Clamp(-alt2, -55, 0), s); FL2=Mathf.Lerp(FL2, b2, s); FL3=Mathf.Lerp(FL3, c2, s);
			      BR1=Mathf.Lerp(BR1, Mathf.Clamp(-alt3, -55, 0), s); BR2=Mathf.Lerp(BR2, b3, s); BR3=Mathf.Lerp(BR3, c3, s);
			      BL1=Mathf.Lerp(BL1, Mathf.Clamp(-alt4, -55, 0), s); BL2=Mathf.Lerp(BL2, b4, s); BL3=Mathf.Lerp(BL3, c4, s);
		      }
		      else
          {
            FR_Add=Mathf.Lerp(FR_Add, 0, s); FR1=Mathf.Lerp(FR1, 0, s); FR2=Mathf.Lerp(FR2, 0, s); FR3=Mathf.Lerp(FR3, 0, s);
            FL_Add=Mathf.Lerp(FL_Add, 0, s); FL1=Mathf.Lerp(FL1, 0, s); FL2=Mathf.Lerp(FL2, 0, s); FL3=Mathf.Lerp(FL3, 0, s);
            BR_Add=Mathf.Lerp(BR_Add, 0, s); BR1=Mathf.Lerp(BR1, 0, s); BR2=Mathf.Lerp(BR2, 0, s); BR3=Mathf.Lerp(BR3, 0, s);
            BL_Add=Mathf.Lerp(BL_Add, 0, s); BL1=Mathf.Lerp(BL1, 0, s); BL2=Mathf.Lerp(BL2, 0, s); BL3=Mathf.Lerp(BL3, 0, s);
          }
        break;
        case IkType.Quad:
          s=0.1f;
          if(!IsConstrained && !IsDead && IsOnGround)
			    {
       
				    FR1=Mathf.Lerp(FR1, Mathf.Clamp(-alt1, -50, 0), s); FR2=Mathf.Lerp(FR2, b1, s); FR3=Mathf.Lerp(FR3, c1, s);
				    FL1=Mathf.Lerp(FL1, Mathf.Clamp(-alt2, -50, 0), s); FL2=Mathf.Lerp(FL2, b2, s); FL3=Mathf.Lerp(FL3, c2, s);
				    BR1=Mathf.Lerp(BR1, Mathf.Clamp(-alt3, -50, 0), s); BR2=Mathf.Lerp(BR2, b3, s); BR3=Mathf.Lerp(BR3, c3, s);
				    BL1=Mathf.Lerp(BL1, Mathf.Clamp(-alt4, -50, 0), s); BL2=Mathf.Lerp(BL2, b4, s); BL3=Mathf.Lerp(BL3, c4, s);
			    }
			    else
          { 
            FR_Add=Mathf.Lerp(FR_Add, 0, s); FR1=Mathf.Lerp(FR1, 0, s); FR2=Mathf.Lerp(FR2, 0, s); FR3=Mathf.Lerp(FR3, 0, s);
            FL_Add=Mathf.Lerp(FL_Add, 0, s); FL1=Mathf.Lerp(FL1, 0, s); FL2=Mathf.Lerp(FL2, 0, s); FL3=Mathf.Lerp(FL3, 0, s);
            BR_Add=Mathf.Lerp(BR_Add, 0, s); BR1=Mathf.Lerp(BR1, 0, s); BR2=Mathf.Lerp(BR2, 0, s); BR3=Mathf.Lerp(BR3, 0, s);
            BL_Add=Mathf.Lerp(BL_Add, 0, s); BL1=Mathf.Lerp(BL1, 0, s); BL2=Mathf.Lerp(BL2, 0, s); BL3=Mathf.Lerp(BL3, 0, s);
          }
        break;
        case IkType.Flying:
          s=0.25f;
          if(!IsConstrained && !IsDead && IsOnGround && !IsOnLevitation)
		      {
			      FR1=Mathf.Lerp(FR1, Mathf.Clamp(-alt1, -100, 0), s); FR2=Mathf.Lerp(FR2, b1, s); FR3=Mathf.Lerp(FR3, c1, s);
			      FL1=Mathf.Lerp(FL1, Mathf.Clamp(-alt2, -100, 0), s); FL2=Mathf.Lerp(FL2, b2, s); FL3=Mathf.Lerp(FL3, c2, s);
			      BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt3, -60, 0), s); BR2=Mathf.Lerp(BR2, b3, s); BR3=Mathf.Lerp(BR3, c3, s);
			      BL1=Mathf.Lerp(BL1, Mathf.Clamp(-alt4, -60, 0), s); BL2=Mathf.Lerp(BL2, b4, s); BL3=Mathf.Lerp(BL3, c4, s);
		      }
		      else
          {
            FR_Add=Mathf.Lerp(FR_Add, 0, s); FR1=Mathf.Lerp(FR1, 0, s); FR2=Mathf.Lerp(FR2, 0, s); FR3=Mathf.Lerp(FR3, 0, s);
            FL_Add=Mathf.Lerp(FL_Add, 0, s); FL1=Mathf.Lerp(FL1, 0, s); FL2=Mathf.Lerp(FL2, 0, s); FL3=Mathf.Lerp(FL3, 0, s);
            BR_Add=Mathf.Lerp(BR_Add, 0, s); BR1=Mathf.Lerp(BR1, 0, s); BR2=Mathf.Lerp(BR2, 0, s); BR3=Mathf.Lerp(BR3, 0, s);
            BL_Add=Mathf.Lerp(BL_Add, 0, s); BL1=Mathf.Lerp(BL1, 0, s); BL2=Mathf.Lerp(BL2, 0, s); BL3=Mathf.Lerp(BL3, 0, s);
          }
        break;
        case IkType.SmBiped:
          s=0.25f;
          if(!IsConstrained && !IsDead && IsOnGround)
		      {
			      BR1=Mathf.Lerp(BR1, Mathf.Clamp(-alt1, -60, 0), s); BR2=Mathf.Lerp(BR2, b1, s); BR3=Mathf.Lerp(BR3, c1, s);
			      BL1=Mathf.Lerp(BL1, Mathf.Clamp(-alt2, -60, 0), s); BL2=Mathf.Lerp(BL2, b2, s); BL3=Mathf.Lerp(BL3, c2, s);
		      }
		      else
          { 
            BR_Add=Mathf.Lerp(BR_Add, 0, s); BR1=Mathf.Lerp(BR1, 0, s); BR2=Mathf.Lerp(BR2, 0, s); BR3=Mathf.Lerp(BR3, 0, s);
            BL_Add=Mathf.Lerp(BL_Add, 0, s); BL1=Mathf.Lerp(BL1, 0, s); BL2=Mathf.Lerp(BL2, 0, s); BL3=Mathf.Lerp(BL3, 0, s);
          }
        break;
        case IkType.LgBiped:
          s=0.25f;
          if(!IsDead && IsOnGround)
		      {
			      BR1=Mathf.Lerp(BR1, Mathf.Clamp(-alt1, -55, 0), s); BR2=Mathf.Lerp(BR2, b1, s); BR3=Mathf.Lerp(BR3, c1, s);
			      BL1=Mathf.Lerp(BL1, Mathf.Clamp(-alt2, -55, 0), s); BL2=Mathf.Lerp(BL2, b2, s); BL3=Mathf.Lerp(BL3, c2, s);
		      }
		      else
          { 
            BR_Add=Mathf.Lerp(BR_Add, 0, s); BR1=Mathf.Lerp(BR1, 0, s); BR2=Mathf.Lerp(BR2, 0, s); BR3=Mathf.Lerp(BR3, 0, s);
            BL_Add=Mathf.Lerp(BL_Add, 0, s); BL1=Mathf.Lerp(BL1, 0, s); BL2=Mathf.Lerp(BL2, 0, s); BL3=Mathf.Lerp(BL3, 0, s);
          }
        break;
      }
    }

    //Take damages animation
    if(lastHit!=0) { if(!IsDead&&CanWalk) crouch=Mathf.Lerp(crouch, (Crouch_Max*size)/2, 1.0f); lastHit--; }

    //Reset skeleton rotations
    if(OnReset)
    {
      pitch = Mathf.Lerp(pitch, 0.0f, t/10f);
      roll = Mathf.Lerp(roll, 0.0f, t/10f);
      headX= Mathf.LerpAngle(headX, 0.0f, t/10f);
      headY= Mathf.LerpAngle(headY, 0.0f, t/10f);
      crouch=Mathf.Lerp(crouch, 0.0f, t/10f);
      spineX= Mathf.LerpAngle(spineX, 0.0f, t/10f);
      spineY= Mathf.LerpAngle(spineY, 0.0f, t/10f);
      return;
    }

    //Smooth avoiding angle
    if(avoidDelta!=0)
    { 
      if(Mathf.Abs(avoidAdd)>90) avoidDelta=0;
      avoidAdd=Mathf.MoveTowardsAngle(avoidAdd,avoidDelta>0.0f?135f:-135f, t);
    }
    else avoidAdd=Mathf.MoveTowardsAngle(avoidAdd, 0.0f, t);
    
    //Setup Look target position
    if(objTGT)
		{
			if(behavior.EndsWith("Hunt") | behavior.Equals("Battle") |  behavior.EndsWith("Contest") ) lookTGT=objTGT.transform.position;
      else if(Herbivorous && behavior.Equals("Food")) lookTGT=posTGT;
			else if(loop==0) lookTGT=Vector3.zero;
		} else if(loop==0) lookTGT=Vector3.zero;

    //Lerp all skeleton parts
    if(CanMoveHead)
    {
      if(!OnTailAttack && !anm.GetInteger("Move").Equals(0))
      {
        spineX= Mathf.MoveTowardsAngle(spineX, (Mathf.DeltaAngle(anm.GetFloat("Turn"), transform.eulerAngles.y)/360f)*maxX, t);
        spineY= Mathf.LerpAngle(spineY, 0.0f, t/10f);
      }
      else
      {
        spineX= Mathf.MoveTowardsAngle(spineX, 0.0f, t/10f);
        spineY= Mathf.LerpAngle(spineY, 0.0f, t/10f);
      }

      if((!CanFly && !CanSwim && anm.GetInteger("Move")!=2) | !IsOnGround) roll=Mathf.Lerp(roll, 0.0f, Ang_T);
		  crouch=Mathf.Lerp(crouch, 0.0f, t/10f);

      if(OnHeadMove) return;

      if(lookTGT!=Vector3.zero && (lookTGT-transform.position).magnitude>boxscale.z)
      {
        Quaternion dir;
			  if(objTGT && objTGT.tag.Equals("Creature")) dir= Quaternion.LookRotation(objTGT.GetComponent<Rigidbody>().worldCenterOfMass-HeadPos);
			  else dir= Quaternion.LookRotation(lookTGT-HeadPos);

        headX = Mathf.MoveTowardsAngle(headX, (Mathf.DeltaAngle(dir.eulerAngles.y, transform.eulerAngles.y)/(180f-Yaw_Max))*Yaw_Max, t);
        headY = Mathf.MoveTowardsAngle(headY, (Mathf.DeltaAngle(dir.eulerAngles.x, transform.eulerAngles.x)/(90f-Pitch_Max))*Pitch_Max, t);
      }
      else
      {
        if(Mathf.RoundToInt(anm.GetFloat("Turn"))==Mathf.RoundToInt(transform.eulerAngles.y))
        {
          if(loop==0 && Mathf.RoundToInt(headX*100)==Mathf.RoundToInt(rndX*100) && Mathf.RoundToInt(headY*100)==Mathf.RoundToInt(rndY*100))
          {
            rndX=Random.Range((int)-Yaw_Max/2, (int)Yaw_Max/2);
            rndY=Random.Range((int)-Pitch_Max/2,(int)Pitch_Max/2);
          }
          headX= Mathf.LerpAngle(headX, rndX, t/10f);
          headY= Mathf.LerpAngle(headY, rndY, t/10f);
        } 
        else
        {
          headX= Mathf.LerpAngle(headX, spineX, t/10f);
          headY= Mathf.LerpAngle(headY, 0.0f, t/10f);
        }
      }
    }
    else
    {
      spineX = Mathf.LerpAngle(spineX, (Mathf.DeltaAngle(anm.GetFloat("Turn"), transform.eulerAngles.y)/360f)*maxX, Ang_T);
      if(IsOnGround && !IsInWater) { spineY= Mathf.LerpAngle(spineY, 0.0f, t/10f); roll=Mathf.LerpAngle(roll, 0.0f, t/10f); pitch=Mathf.Lerp(pitch, 0.0f, t/10f); }
      else if(CanFly) 
      { 
        if(anm.GetInteger("Move")>=2 && anm.GetInteger("Move")<3)
        spineY = Mathf.LerpAngle(spineY, (Mathf.DeltaAngle(anm.GetFloat("Pitch")*90f, pitch)/180f)*maxY, Ang_T);
        roll=Mathf.LerpAngle(roll, -spineX, t/10f);
      }
      else { spineY = Mathf.LerpAngle(spineY, (Mathf.DeltaAngle(anm.GetFloat("Pitch")*90f, pitch)/180f)*maxY, Ang_T); roll=Mathf.LerpAngle(roll, -spineX, t/10f); }
      headX= Mathf.LerpAngle(headX, spineX, t);
      headY= Mathf.LerpAngle(headY, spineY, t);
    }

  }
  #endregion
  #region FEET INVERSE KINEMATICS
//QUADRUPED
	void Quad(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3,
            Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3, float FeetOffset)
	{
		//Right arm
			float offset = (RArm3.position-RArm3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
			Vector3 va1= RArm3.position-transform.up*offset;

			RArm1.rotation*=Quaternion.Euler(0, -FR1+(FR1+FR_Add), 0);
			a1 = Vector3.Angle(RArm1.position-RArm2.position, RArm1.position-RArm3.position);
			RArm2.rotation*=Quaternion.Euler(0, (FR1*2f)-FR_Add, 0);
			b1 = Vector3.Angle(FR_Norm, RArm3.right)-100f;
			c1 = Vector3.Angle(-FR_Norm, RArm3.up)-90;
			RArm3.rotation*=Quaternion.Euler(FR3, FR2, 0);

			Vector3 va3 =FR_HIT+(FR_HIT-RArm3.position)+transform.up*offset;
			Vector3 va2=new Vector3(va1.x, va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y), va1.z);
			alt1 = ((va1-va2).magnitude-(va3-va2).magnitude) * (100/(va1-va2).magnitude); 
		//Left arm
			offset = (LArm3.position-LArm3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
			Vector3 vb1= LArm3.position-transform.up*offset;

			LArm1.rotation*=Quaternion.Euler(-FL1+(FL1+FL_Add), 0, 0);
			a2 = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.position);
			LArm2.rotation*=Quaternion.Euler((FL1*2f)-FL_Add, 0, 0);
			b2 = Vector3.Angle(FL_Norm, LArm3.right)-90f;
			c2 = Vector3.Angle(-FL_Norm, LArm3.up)-100f;
			LArm3.rotation*=Quaternion.Euler(FL3, FL2, 0);

			Vector3 vb3 =FL_HIT+(FL_HIT-LArm3.position)+transform.up*offset;
			Vector3 vb2=new Vector3(vb1.x, vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y), vb1.z);
			alt2 = ((vb1-vb2).magnitude-(vb3-vb2).magnitude) * (100/(vb1-vb2).magnitude); 
		//Right leg
			offset = (RLeg3.position-RLeg3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
			Vector3 vc1= RLeg3.position-transform.up*offset;
			
			RLeg1.rotation*=Quaternion.Euler(0, BR1-(BR1+BR_Add), 0);
			a3 = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position);
			RLeg2.rotation*=Quaternion.Euler(0, (-BR1*2f)+BR_Add, 0);
			b3 = Vector3.Angle(BR_Norm, RLeg3.right)-90f;
			c3 = Vector3.Angle(-BR_Norm, RLeg3.up)-90f;
			RLeg3.rotation*=Quaternion.Euler(BR3, BR2, 0);
			
			Vector3 vc3 =BR_HIT+(BR_HIT-RLeg3.position)+transform.up*offset;
			Vector3 vc2=new Vector3(vc1.x, vc1.y-(vc1.y-RLeg1.position.y)-(vc1.y-BR_HIT.y), vc1.z);
			alt3 = ((vc1-vc2).magnitude-(vc3-vc2).magnitude)* (100/(vc1-vc2).magnitude); 
		//Left leg
			offset = (LLeg3.position-LLeg3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
			Vector3 vd1= LLeg3.position-transform.up*offset;
			
			LLeg1.rotation*=Quaternion.Euler(0, BL1-(BL1+BL_Add), 0);
			a4 = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position);
			LLeg2.rotation*=Quaternion.Euler(0, (-BL1*2f)+BL_Add, 0);
			b4 = Vector3.Angle(BL_Norm, LLeg3.right)-90f;
			c4 = Vector3.Angle(-BL_Norm, LLeg3.up)-90f;
			LLeg3.rotation*=Quaternion.Euler(BL3, BL2, 0);
			
			Vector3 vd3 =BL_HIT+(BL_HIT-LLeg3.position)+transform.up*offset;
			Vector3 vd2=new Vector3(vd1.x, vd1.y-(vd1.y-LLeg1.position.y)-(vd1.y-BL_HIT.y), vd1.z);
			alt4 = ((vd1-vd2).magnitude-(vd3-vd2).magnitude) * (100/(vd1-vd2).magnitude); 

      //Add rotations
      if(!IsConstrained && !IsDead && IsOnGround)
			{
				FR_Add = Vector3.Angle(RArm1.position-RArm2.position, RArm1.position-RArm3.position)-a1;
        FL_Add = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.position)-a2;
        BR_Add = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position)-a3;
        BL_Add = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position)-a4;
			}
	}

//SMALL BIPED
	void SmBiped(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3)
	{
    Transform RLeg4 = RLeg3.GetChild(0);
		//Right leg
		float offset1 = (RLeg4.position-RLeg4.GetChild(0).position).magnitude;
		Vector3 va1= RLeg4.position-transform.up*offset1;
		float inv1 = Mathf.Clamp(Vector3.Cross(RLeg4.position-transform.position, RLeg1.position-transform.position).y, -1.0f, 1.0f);
			
		RLeg1.rotation*=Quaternion.Euler(0, BR1-(BR1+BR_Add), 0);
		a1 = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0, -BR1*2f, 0);
		RLeg3.rotation*=Quaternion.Euler(0, BR1-BR_Add*inv1, 0);
		b1 = Vector3.Angle(-BR_Norm, RLeg4.GetChild(0).right)-90f;
		c1 = Vector3.Angle(-BR_Norm, RLeg4.up)-90f;
		RLeg4.rotation*=Quaternion.Euler(BR3, 0, 0);
		RLeg4.GetChild(0).rotation*=Quaternion.Euler(0, -BR2, 0);
			
		Vector3 va3 =BR_HIT+(BR_HIT-RLeg4.GetChild(0).position)+transform.up*offset1;
		Vector3 va2=(va1+transform.up*(va1-RLeg1.position).magnitude);
		alt1 = ((va1-va2).magnitude-(va3-va2).magnitude) * (100/(va1-va2).magnitude);

    Transform LLeg4 =LLeg3.GetChild(0);
		//Left Leg
		float offset2 = (LLeg4.position-LLeg4.GetChild(0).position).magnitude;
		Vector3 vb1= LLeg4.position-transform.up*offset2;
		float inv2 = Mathf.Clamp(Vector3.Cross(LLeg4.position-transform.position, LLeg1.position-transform.position).y, -1.0f, 1.0f);
			
		LLeg1.rotation*=Quaternion.Euler(BL1-(BL1+BL_Add), 0, 0);
		a2 = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(-BL1*2f, 0, 0);
		LLeg3.rotation*=Quaternion.Euler(BL1+BL_Add*inv2, 0, 0);
			
		b2 = Vector3.Angle(-BL_Norm, -LLeg4.GetChild(0).up)-90f;
		c2 = Vector3.Angle(-BL_Norm, LLeg4.up)-90f;
		LLeg4.rotation*=Quaternion.Euler(BL3, 0, 0);
		LLeg4.GetChild(0).rotation*=Quaternion.Euler(0, 0, BL2);
		
			
		Vector3 vb3 =BL_HIT+(BL_HIT-LLeg4.GetChild(0).position)+transform.up*offset2;
		Vector3 vb2=(vb1+transform.up*(vb1-LLeg1.position).magnitude);
		alt2 = ((vb1-vb2).magnitude-(vb3-vb2).magnitude) * (100/(vb1-vb2).magnitude); 

    //Add rotations
    if(!IsConstrained && !IsDead && IsOnGround)
		{
			BR_Add = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position)-a1;
      BL_Add = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position)-a2;
		}


	}

//LARGE BIPED
	public void LgBiped(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3)
	{
		//Right leg
    Transform RLeg4=RLeg3.GetChild(0);
		float offset1 = (RLeg4.position-RLeg4.GetChild(1).position).magnitude;
		Vector3 va1= RLeg4.position-transform.up*offset1;
		float inv1 = Mathf.Clamp(Vector3.Cross(RLeg4.position-transform.position, RLeg1.position-transform.position).y, -1.0f, 1.0f);
				
		RLeg1.rotation*=Quaternion.Euler(0, BR1-(BR1+BR_Add), 0);
		a1 = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0, -BR1*2f, 0);
		RLeg3.rotation*=Quaternion.Euler(0, BR1-BR_Add*inv1, 0);
		b1 = Vector3.Angle(-BR_Norm, RLeg4.GetChild(1).right)-90f;
		c1 = Vector3.Angle(-BR_Norm, RLeg4.up)-90f;
		RLeg4.rotation*=Quaternion.Euler(BR3, 0, 0);
		RLeg4.GetChild(0).rotation*=Quaternion.Euler(0, -BR2, 0);
		RLeg4.GetChild(1).rotation*=Quaternion.Euler(0, -BR2, 0);
		RLeg4.GetChild(2).rotation*=Quaternion.Euler(0, -BR2, 0);

		Vector3 va3 =BR_HIT+(BR_HIT-RLeg4.position)+transform.up*offset1;
		Vector3 va2=(va1+transform.up*(va1-RLeg1.position).magnitude);
		alt1 = ((va1-va2).magnitude-(va3-va2).magnitude) * (100/(va1-va2).magnitude); 

		//Left Leg
    Transform LLeg4=LLeg3.GetChild(0);
		float offset2 = (LLeg4.position-LLeg4.GetChild(1).position).magnitude;
		Vector3 vb1= LLeg4.position-transform.up*offset2;
		float inv2 = Mathf.Clamp(Vector3.Cross(LLeg4.position-transform.position, LLeg1.position-transform.position).y, -1.0f, 1.0f);
				
		LLeg1.rotation*=Quaternion.Euler(0, BL1-(BL1+BL_Add), 0);
		a2 = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(0, -BL1*2f, 0);
		LLeg3.rotation*=Quaternion.Euler(0, BL1+BL_Add*inv2, 0);

		b2 = Vector3.Angle(-BL_Norm, LLeg4.GetChild(1).up)-90f;
		c2 = Vector3.Angle(-BL_Norm, LLeg4.up)-90f;
		LLeg4.rotation*=Quaternion.Euler(BL3, 0, 0);
		LLeg4.GetChild(0).rotation*=Quaternion.Euler(0, BL2, 0);
		LLeg4.GetChild(1).rotation*=Quaternion.Euler(BL2, 0, 0);
		LLeg4.GetChild(2).rotation*=Quaternion.Euler(0, BL2, 0);
				
		Vector3 vb3 =BL_HIT+(BL_HIT-LLeg4.position)+transform.up*offset2;
		Vector3 vb2=(vb1+transform.up*(vb1-LLeg1.position).magnitude);
		alt2 = ((vb1-vb2).magnitude-(vb3-vb2).magnitude) * (100/(vb1-vb2).magnitude); 

    //Add rotations
    if(!IsDead && IsOnGround)
		{
			BR_Add = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position)-a1;
      BL_Add = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position)-a2;
		}
	}

//CONVEX QUADRUPED
	void Convex(Transform RLeg1, Transform RLeg2, Transform RLeg3,Transform LLeg1, Transform LLeg2, Transform LLeg3, 
                    Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3)
	{
		//Right arm
		float offset1 = (RArm3.position-RArm3.GetChild(0).position).magnitude;
		Vector3 va1= RArm3.position-transform.up*offset1;
			
		RArm1.rotation*=Quaternion.Euler(FR1-(FR1+FR_Add),0 , 0);
		a1 = Vector3.Angle(RArm1.position-RArm2.position, RArm1.position-RArm3.GetChild(0).GetChild(0).position);
		RArm2.rotation*=Quaternion.Euler(0,FR1-FR_Add, 0);
		b1 = Vector3.Angle(FR_Norm, RArm3.GetChild(0).right)-90f;
		c1 = Vector3.Angle(FR_Norm, -RArm3.GetChild(0).up)-90f;
		RArm3.rotation*=Quaternion.Euler(-FR3/2, -FR2/2, 0);
		RArm3.GetChild(0).rotation*=Quaternion.Euler(-FR3/2, -FR2/2, 0);

		Vector3 va3 =FR_HIT+(FR_HIT-RArm3.GetChild(0).GetChild(0).position)+transform.up*offset1;
		Vector3 va2=new Vector3(va1.x, va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y), va1.z);
		alt1 = ((va1-va2).magnitude-(va3-va2).magnitude) * (100/(va1-va2).magnitude);

		//Left arm
		float offset2 = (LArm3.position-LArm3.GetChild(0).position).magnitude;
		Vector3 vb1= LArm3.position-transform.up*offset2;
			
		LArm1.rotation*=Quaternion.Euler(FL1-(FL1+FL_Add), 0, 0);
		a2 = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.GetChild(0).GetChild(0).position);
		LArm2.rotation*=Quaternion.Euler(-FL1+FL_Add, 0, 0);
		b2 = Vector3.Angle(FL_Norm, -LArm3.GetChild(0).up)-90f;
		c2 = Vector3.Angle(FL_Norm, LArm3.GetChild(0).right)-90f;
		LArm3.rotation*=Quaternion.Euler(-FL2/2, -FL3/2, 0);
		LArm3.GetChild(0).rotation*=Quaternion.Euler(-FL2/2, -FL3/2, 0);
			
		Vector3 vb3 =FL_HIT+(FL_HIT-LArm3.GetChild(0).GetChild(0).position)+transform.up*offset2;
		Vector3 vb2=new Vector3(vb1.x, vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y), vb1.z);
		alt2 = ((vb1-vb2).magnitude-(vb3-vb2).magnitude) * (100/(vb1-vb2).magnitude);

	  //Right leg
		float offset3 = (RLeg3.position-RLeg3.GetChild(0).GetChild(0).position).magnitude;
		Vector3 vc1= RLeg3.position-transform.up*offset3;
			
		RLeg1.rotation*=Quaternion.Euler(0,-(BR1+(BR1+BR_Add)), 0);
		a3 = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0, (BR1*2f)-BR_Add, 0);
		b3 = Vector3.Angle(BR_Norm, RLeg3.GetChild(0).right)-90f;
		c3 = Vector3.Angle(-BR_Norm, RLeg3.GetChild(0).up)-90f;
		RLeg3.rotation*=Quaternion.Euler(-BR3/2, -BR2/2, 0);
		RLeg3.GetChild(0).rotation*=Quaternion.Euler(-BR3/2, -BR2/2, 0);

		Vector3 vc3 =BR_HIT+(BR_HIT-RLeg3.position)+transform.up*offset3;
		Vector3 vc2=new Vector3(vc1.x, vc1.y-(vc1.y-RLeg1.position.y)-(vc1.y-BR_HIT.y), vc1.z);
		alt3 = ((vc1-vc2).magnitude-(vc3-vc2).magnitude)* (100/(vc1-vc2).magnitude); 

		//Left leg
		float offset4 = (LLeg3.position-LLeg3.GetChild(0).GetChild(0).position).magnitude;
		Vector3 vd1= LLeg3.position-transform.up*offset4;

		LLeg1.rotation*=Quaternion.Euler(BL1+(BL1+BL_Add), 0, 0);
		a4 = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(-(BL1*2f)+BL_Add, 0, 0);
		b4 = Vector3.Angle(BL_Norm, LLeg3.GetChild(0).right)-90f;
		c4 = Vector3.Angle(-BL_Norm, LLeg3.GetChild(0).up)-90f;
		LLeg3.rotation*=Quaternion.Euler(-BL3/2, -BL2/2, 0);
		LLeg3.GetChild(0).rotation*=Quaternion.Euler(-BL3/2, -BL2/2, 0);

		Vector3 vd3 =BL_HIT+(BL_HIT-LLeg3.position)+transform.up*offset4;
		Vector3 vd2=new Vector3(vd1.x, vd1.y-(vd1.y-LLeg1.position.y)-(vd1.y-BL_HIT.y), vd1.z);
		alt4 = ((vd1-vd2).magnitude-(vd3-vd2).magnitude) * (100/(vd1-vd2).magnitude); 

    if(!IsConstrained && !IsDead && IsOnGround && !IsInWater)
		{
			FR_Add = Vector3.Angle(RArm1.position-RArm2.position, RArm1.position-RArm3.GetChild(0).GetChild(0).position)-a1;
      FL_Add = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.GetChild(0).GetChild(0).position)-a2;
      BR_Add = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.position)-a3;
      BL_Add = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.position)-a4;
		}
	}

//FLYING
	void Flying(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3,
                Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3)
	{
		//Right wing
		Vector3 va1= RArm3.GetChild(1).position;
			
		RArm1.rotation*=Quaternion.Euler(FR1, FR1-(FR1-FR_Add), FR1);
		a1 = Vector3.Angle(RArm1.position-RArm2.position, RArm1.position-RArm3.GetChild(1).position);
		RArm2.rotation*=Quaternion.Euler(0, 0, (-FR1*2.4f)-FR_Add);
		b1 = Vector3.Angle(FR_Norm, RArm3.right)-90f;
		c1 = Vector3.Angle(-FR_Norm, RArm3.up)-90f;
		RArm3.rotation*=Quaternion.Euler(FR3, FR2, 0);
			
		Vector3 va3 =FR_HIT+(FR_HIT-RArm3.GetChild(1).position);
		Vector3 va2=new Vector3(va1.x, va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y), va1.z);
		alt1 = ((va1-va2).magnitude-(va3-va2).magnitude) * (100/(va1-va2).magnitude);

		//Left Wing
		Vector3 vb1= LArm3.GetChild(1).position;
			
		LArm1.rotation*=Quaternion.Euler(-FL1, FL1-(FL1-FL_Add), -FL1);
		a2 = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.GetChild(1).position);
		LArm2.rotation*=Quaternion.Euler(0, 0, (FL1*2.4f)+FL_Add);
		b2 = Vector3.Angle(FL_Norm, LArm3.right)-90f;
		c2 = Vector3.Angle(-FL_Norm, LArm3.up)-90f;
		LArm3.rotation*=Quaternion.Euler(FL3, FL2, 0);
			
		Vector3 vb3 =FL_HIT+(FL_HIT-LArm3.GetChild(1).position);
		Vector3 vb2=new Vector3(vb1.x, vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y), vb1.z);
		alt2 = ((vb1-vb2).magnitude-(vb3-vb2).magnitude) * (100/(vb1-vb2).magnitude);

		//Right leg
		float offset1 = (RLeg3.position-RLeg3.GetChild(2).position).magnitude/1.5f;
		Vector3 vc1= RLeg3.position-transform.up*offset1;
		float inv1 = Mathf.Clamp(Vector3.Cross(RLeg3.GetChild(2).position-transform.position, RLeg1.position-transform.position).y, -1.0f, 1.0f);
			
		RLeg1.rotation*=Quaternion.Euler(0, -BR1+(BR1-BR_Add), 0);
		a3 = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.GetChild(2).position);
		RLeg2.rotation*=Quaternion.Euler(0, -BR1*2, 0);
		c3 = Vector3.Angle(BR_Norm, RLeg3.GetChild(2).up)-90f;
		RLeg3.rotation*=Quaternion.Euler(0, BR1-BR_Add*inv1, BR3);
		b3 = Vector3.Angle(BR_Norm, RLeg3.GetChild(2).right)-90f;
		RLeg3.GetChild(0).rotation*=Quaternion.Euler(0, -BR2, 0);
		RLeg3.GetChild(1).rotation*=Quaternion.Euler(0, -BR2, 0);
		RLeg3.GetChild(2).rotation*=Quaternion.Euler(0, -BR2, 0);
		RLeg3.GetChild(3).rotation*=Quaternion.Euler(0, -BR2, 0);

		Vector3 vc3 =BR_HIT+(BR_HIT-RLeg3.GetChild(2).position)+transform.up*offset1;
		Vector3 vc2=(vc1+transform.up*(vc1-RLeg1.position).magnitude);
		alt3 = ((vc1-vc2).magnitude-(vc3-vc2).magnitude) * (100/(vc1-vc2).magnitude);

		//Left leg
		float offset2 = (LLeg3.position-LLeg3.GetChild(2).position).magnitude/1.5f;
		Vector3 vd1= LLeg3.position-transform.up*offset2;
		float inv2 = Mathf.Clamp(Vector3.Cross(LLeg3.GetChild(2).position-transform.position, LLeg1.position-transform.position).y, -1.0f, 1.0f);
			
		LLeg1.rotation*=Quaternion.Euler(0, -BL1+(BL1-BL_Add), 0);
		a4 = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.GetChild(2).position);
		LLeg2.rotation*=Quaternion.Euler(0, -BL1*2, 0);
		c4 = Vector3.Angle(BL_Norm, LLeg3.GetChild(2).up)-90f;
		LLeg3.rotation*=Quaternion.Euler(0, BL1-BL_Add*inv2, BL3);
		b4 = Vector3.Angle(BL_Norm, LLeg3.GetChild(2).right)-90f;
		LLeg3.GetChild(0).rotation*=Quaternion.Euler(0, -BL2, 0);
		LLeg3.GetChild(1).rotation*=Quaternion.Euler(0, -BL2, 0);
		LLeg3.GetChild(2).rotation*=Quaternion.Euler(0, -BL2, 0);
		LLeg3.GetChild(3).rotation*=Quaternion.Euler(0, -BL2, 0);
			
		Vector3 vd3 =BL_HIT+(BL_HIT-LLeg3.GetChild(2).position)+transform.up*offset2;
		Vector3 vd2=(vd1+transform.up*(vd1-LLeg1.position).magnitude);
		alt4 = ((vd1-vd2).magnitude-(vd3-vd2).magnitude) * (100/(vd1-vd2).magnitude); 

    //Add rotations
    if(!IsConstrained && !IsDead && IsOnGround && !IsOnLevitation)
		{
			FR_Add = Vector3.Angle(RArm1.position-RArm2.position, LArm1.position-RArm3.GetChild(1).position)-a1;
      FL_Add = Vector3.Angle(LArm1.position-LArm2.position, LArm1.position-LArm3.GetChild(1).position)-a2;
      BR_Add = Vector3.Angle(RLeg1.position-RLeg2.position, RLeg1.position-RLeg3.GetChild(2).position)-a3;
      BL_Add = Vector3.Angle(LLeg1.position-LLeg2.position, LLeg1.position-LLeg3.GetChild(2).position)-a4;
		}
	}
  #endregion
  #region PLAYER INPUTS
  public void GetUserInputs(int idle1=0, int idle2=0, int idle3=0, int idle4=0, int eat=0, int drink=0, int sleep=0, int rise=0)
	{
		if (!networkObject)
        {
			return;
        }

		if(behavior=="Repose" && anm.GetInteger("Move")!=0) behavior="Player";
		else if(behaviorCount<=0) { objTGT=null; behavior="Player"; behaviorCount=0; } else behaviorCount--;

		// Current camera manager target ?
		if(transform.gameObject==main.creaturesList[main.selected].gameObject && main.CameraMode!=0)
		{
			//Run key
			bool run=Input.GetKey(KeyCode.LeftShift)?true:false;

			//Attack key
			if(CanAttack)
      { 
        if(Input.GetKey(KeyCode.Mouse0)) { behaviorCount=500; behavior="Hunt"; anm.SetBool ("Attack", true); }
        else anm.SetBool ("Attack", false);
      }

			//Crouch key
			if(main.UseIK && Input.GetKey(KeyCode.LeftControl)) { crouch=Crouch_Max*size; OnCrouch=true; }
			else OnCrouch=false;

			//Fly/swim up/down key
			if(CanFly | CanSwim)
			{
				if(Input.GetKey(KeyCode.Mouse1))
				{
					anm.SetFloat("Turn", transform.eulerAngles.y+Input.GetAxis("Mouse X")*22.5f);//Mouse turn
					if(Input.GetAxis("Mouse Y")!=0 && anm.GetInteger("Move")==3) //Pitch with mouse if is moving
					anm.SetFloat("Pitch",Input.GetAxis("Mouse Y"));
					else if(Input.GetKey(KeyCode.LeftControl)) anm.SetFloat("Pitch", 1.0f);
					else if(Input.GetKey(KeyCode.Space)) anm.SetFloat("Pitch",-1.0f);
				}
				else
				{
					if(Input.GetKey(KeyCode.LeftControl)) anm.SetFloat("Pitch", 1.0f);
					else if(Input.GetKey(KeyCode.Space)) anm.SetFloat("Pitch", -1.0f);
					else anm.SetFloat("Pitch", 0);
				}
			}

			//Jump
			if (CanJump && Input.GetKey(KeyCode.Space) && !OnJump)
			{
				anm.SetInteger("Move", 3);
				playerCreature.networkObject.jump = true;
			}
			//Move
			else if (Input.GetAxis("Horizontal") != 0 | Input.GetAxis("Vertical") != 0)
			{
				//Flying/swim
				if (CanSwim | (CanFly && !IsOnGround))
				{
					if (Input.GetKey(KeyCode.Mouse1))
					{
						if (Input.GetAxis("Vertical") < 0) anm.SetInteger("Move", -1); //Backward
						else if (Input.GetAxis("Vertical") > 0) anm.SetInteger("Move", 3); //Forward
						else if (Input.GetAxis("Horizontal") > 0) anm.SetInteger("Move", -10); //Strafe-
						else if (Input.GetAxis("Horizontal") < 0) anm.SetInteger("Move", 10); //Strafe+
						else anm.SetInteger("Move", 0);
					}
					else
					{
						if (run) anm.SetInteger("Move", CanSwim ? 2 : 1); else anm.SetInteger("Move", CanSwim ? 1 : 2);
						float ang = main.transform.eulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
						anm.SetFloat("Turn", ang); //Turn
					}
				}
				//Terrestrial
				else
				{
					if (Input.GetKey(KeyCode.Mouse1))
					{
						if (Input.GetAxis("Vertical") > 0 && !run) anm.SetInteger("Move", 1); //Forward
						else if (Input.GetAxis("Vertical") > 0) anm.SetInteger("Move", 2); //Run
						else if (Input.GetAxis("Vertical") < 0) anm.SetInteger("Move", -1); //Backward
						else if (Input.GetAxis("Horizontal") > 0) anm.SetInteger("Move", -10); //Strafe-
						else if (Input.GetAxis("Horizontal") < 0) anm.SetInteger("Move", 10); //Strafe+
						anm.SetFloat("Turn", transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f);//Mouse turn
					}
					else
					{
						float ang = main.transform.eulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
						anm.SetInteger("Move", run ? 2 : 1); anm.SetFloat("Turn", ang); //Turn
					}
				}
			}
			//Stop
			else
			{
				//Flying/Swim
				if ((CanSwim | CanFly) && !IsOnGround)
				{
					if (CanSwim && anm.GetFloat("Pitch") != 0 && !Input.GetKey(KeyCode.Mouse1)) anm.SetInteger("Move", run ? 2 : 1);
					else anm.SetInteger("Move", 0);
				}
				//Terrestrial
				else
				{
					if (Input.GetKey(KeyCode.Mouse1))
					{
						if (Input.GetAxis("Mouse X") > 0) anm.SetInteger("Move", 10); //Strafe- 
						else if (Input.GetAxis("Mouse X") < 0) anm.SetInteger("Move", -10); //Strafe+
						else anm.SetInteger("Move", 0);
						anm.SetFloat("Turn", transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f);//Mouse turn
					}
					else anm.SetInteger("Move", 0); //Stop
				}
			}

      //Invert body (Ammonite & Cameroceras)
      if(CanInvertBody && Input.GetKeyDown(KeyCode.R)) { if(OnInvert) OnInvert=false; else OnInvert=true; }

			//Idles key
			if(Input.GetKey(KeyCode.E))
			{
				int idles=0; if(idle1>0) idles++; if(idle2>0) idles++; if(idle3>0) idles++; if(idle4>0) idles++; //idles to play
				rndIdle = Random.Range(1, idles+1);
		
        switch(rndIdle)
        {
				  case 1: anm.SetInteger ("Idle", idle1); break;
          case 2: anm.SetInteger ("Idle", idle2); break;
          case 3: anm.SetInteger ("Idle", idle3); break;
          case 4: anm.SetInteger ("Idle", idle4); break;
        }
			}
			else if(Input.GetKey(KeyCode.F)) //Eat / Drink
			{
				if(posTGT==Vector3.zero) FindPlayerFood(); //looking for food
				//Drink
				if(IsOnWater)
				{
					anm.SetInteger ("Idle", drink);
					if(Water<100) { behavior="Water"; Water=Mathf.Clamp(Water+0.05f, 0.0f, 100f); }
					if(Input.GetKeyUp(KeyCode.F)) posTGT=Vector3.zero;
					else posTGT=transform.position;
				}
				//Eat
				else if(posTGT!=Vector3.zero)
				{
					anm.SetInteger ("Idle", eat); behavior="Food";
					if(Food<100) Food=Mathf.Clamp(Food+0.05f, 0.0f, 100f);
					if(Water<25) Water+=0.05f;
					if(Input.GetKeyUp(KeyCode.F)) posTGT=Vector3.zero;
				}
				//nothing found
				else main.message=1;
      }
			//Sleep/Sit
			else if(Input.GetKey(KeyCode.Q))
			{ 
				anm.SetInteger("Idle", sleep);
				if(anm.GetInteger("Move")!=0) anm.SetInteger ("Idle", 0);
			}
			//Rise
			else if(rise!=0 && Input.GetKey(KeyCode.Space)) anm.SetInteger ("Idle", rise);
			else { anm.SetInteger ("Idle", 0); posTGT=Vector3.zero; }

      
      //Head move
      if(Input.GetKey(KeyCode.Mouse2))
      {
        OnHeadMove=true;
        headX=Mathf.Lerp(headX, Mathf.Clamp(headX-Input.GetAxis("Mouse X"), -Yaw_Max, Yaw_Max), 0.5f);
        headY=Mathf.Lerp(headY, Mathf.Clamp(headY+Input.GetAxis("Mouse Y"), -Pitch_Max, Pitch_Max),  0.5f);
      } else OnHeadMove=false;
      

      //Angle gap
      delta=Mathf.DeltaAngle(main.transform.eulerAngles.y, anm.GetFloat("Turn"));

			if(OnAnm.IsName(specie+"|Sleep"))
			{ behavior="Repose"; Stamina=Mathf.Clamp(Stamina+0.05f, 0.0f, 100f); }

		}
		// Not current camera target, reset parameters
		else
		{
			//anm.SetFloat("Turn", transform.eulerAngles.y);
      anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", 0); //Stop
			if(CanAttack) anm.SetBool ("Attack", false);
			if(CanFly | CanSwim) anm.SetFloat ("Pitch", 0.0f);
		}
	}
  
  bool FindPlayerFood()
  {
		//Find carnivorous food (looking for a dead creature in range)
		if(!Herbivorous)
		{
			foreach (GameObject o in main.creaturesList.ToArray())
			{
				if((o.transform.position-Head.position).magnitude>boxscale.z) continue; //not in range
				Creature other= o.GetComponent<Creature>(); //Get other creature script
				if(other.IsDead) { objTGT=other.gameObject; posTGT = other.body.worldCenterOfMass; return true; } // meat found
			}
		}
		else
		{
			//Find herbivorous food (looking for trees/details on terrain in range )
			if(main.T)
			{
				//Large creature, look for trees
				if(withersSize>8) 
				{
          if(Physics.CheckSphere(Head.position, withersSize, main.treeLayer)) { posTGT = Head.position; return true; }
          else return false;
				}
				//Look for grass detail
				else
				{
					int layer=0;
					float x = ((transform.position.x - main.T.transform.position.x) / main.tdata.size.z * main.tres);
          float y = ((transform.position.z - main.T.transform.position.z) / main.tdata.size.x * main.tres);

					for(layer=0; layer<main.tdata.detailPrototypes.Length; layer++)
					{
						if(main.tdata.GetDetailLayer( (int) x,  (int) y, 1, 1, layer) [ 0, 0]>0)
						{
							posTGT.x=(main.tdata.size.x/main.tres)*x+main.T.transform.position.x;
							posTGT.z=(main.tdata.size.z/main.tres)*y+main.T.transform.position.z;
							posTGT.y = main.T.SampleHeight( new Vector3(posTGT.x, 0, posTGT.z)); 
							objTGT=null; return true; 
						}
					}
				}
			}
		}

		objTGT=null; posTGT=Vector3.zero; return false; //nothing found...
}
  #endregion
  #region ARTIFICIAL INTELLIGENCE
  #region CORE
  public void AICore(int idle1=0, int idle2=0, int idle3=0, int idle4=0, int eat=0, int drink=0, int sleep=0)
	{
		//Look for a target
		if(posTGT==Vector3.zero)
		{
			if(nextPath>=PathEditor.Count) nextPath=0; //reset path list
      // edited path 
			if(PathEditor.Count>0 && Random.Range(0, 100) < PathEditor[nextPath]._Priority)
			{ objTGT = PathEditor[nextPath]._Waypoint; posTGT=PathEditor[nextPath]._Waypoint.transform.position; behavior="ToWaypoint"; behaviorCount=4000; }
       // look for water
			else if(CanWalk && Random.Range(0, 75)>Water) { FindWater(); behaviorCount=4000; }
			// look for food or prey
			else if(Random.Range(0, 75)>Food)
			{
				if(!Herbivorous) {  if(!FindFood()) FindPrey(); } 
				else FindFood(); behaviorCount=4000;
			}
			// to sleep
			else if(!CanSwim && Random.Range(0, 50)>Stamina) { behavior="ToRepose"; FindPath(); behaviorCount=4000; }
      // look for friend
      else if(Random.Range(0, 5)==0) { FindFriend(); behaviorCount=4000; }
			// if nothing found, find a random path
			if(posTGT==Vector3.zero) { behavior="ToPath"; FindPath(); behaviorCount=4000; }
		}
		//Target found
		else
		{
			//search for enemy
			if(TargetEditor.Count>0) { if(!FindCustomTarget()) FindEnemy(); } else FindEnemy();
			//Execute current behavior
			ExecuteBehavior(idle1, idle2, idle3, idle4, eat, drink, sleep);
		}
	}
  #endregion
  #region SEARCH FUNCTIONS
  //***********************************************************************************************************************************************************************************************************
  //FIND WATER (Find nearest water point, need water layer)
  bool FindWater()
	{
		objTGT=null;
		Vector3 V1=Vector3.zero; float i=0, range=withersSize; RaycastHit hit;
		while(range<waterMaxRange)
		{
			while(i<360)
			{
				V1=transform.position+(Quaternion.Euler(0, i, 0)*Vector3.forward*range);
				if(Physics.Raycast(V1+Vector3.up*withersSize, -Vector3.up*waterMaxRange, out hit) &&
					!Physics.Linecast(transform.position+Vector3.up*withersSize, V1+Vector3.up*withersSize))
				{
					if(hit.transform.gameObject.layer.Equals(main.waterLayer) && Physics.Linecast(hit.point, hit.point-Vector3.up, -1, QueryTriggerInteraction.Ignore))
					{
						behavior="ToWater"; posTGT = hit.point; return true; //Found
					} else { i+=15; V1=Vector3.zero; } //not match
				} else { i+=15; V1=Vector3.zero; } //not match
			}
			range+=withersSize; i=0;
		}

		posTGT=Vector3.zero; return false;
	}

  //***********************************************************************************************************************************************************************************************************
	//FIND FOOD
	bool FindFood()
	{
		//Find carnivorous food (looking for a dead creature)
		if(!Herbivorous)
		{
			foreach (GameObject o in main.creaturesList.ToArray())
			{
				if((o.transform.position-transform.position).magnitude>foodMaxRange) continue; //not in range
				if(o.gameObject==transform.gameObject) continue; //cannot eat himself
				Creature other= o.GetComponent<Creature>(); //Get other creature script
        if(!CanSwim && other.IsInWater) continue;
				if(CanSwim && !CanWalk && !other.IsInWater) continue;
				if(other.IsDead) { behavior="ToFood"; objTGT=other.gameObject; posTGT = other.body.worldCenterOfMass; return true; } // meat found
			}
		}
		else
		{
			//Find herbivorous food (looking for trees/details on terrain )
			if(main.T)
			{
				//Large creature, look for trees
				if(withersSize>8) 
				{

					Vector3 V1=Vector3.zero; float i=0; RaycastHit hit;
					while(i<360)
					{
						V1=transform.position+(Quaternion.Euler(0, i, 0)*Vector3.forward*(foodMaxRange/4));
						if(Physics.Linecast(V1+Vector3.up*withersSize, transform.position+Vector3.up*withersSize, out hit, main.treeLayer))
						{ behavior="ToFood"; posTGT = hit.point;  return true; } //tree found
						else { i++; V1=Vector3.zero; } // not found, continue
					}
					objTGT=null; posTGT = Vector3.zero; return false; //not found
        
				}
				//Look for grass detail
				else
				{
					int layer=0;
					float sx = ( (transform.position.x - main.T.transform.position.x) / main.tdata.size.x * main.tres)-2, x=sx;
					float sy = ( (transform.position.z - main.T.transform.position.z) / main.tdata.size.z * main.tres)-2, y=sy;

					for (y = sy; y < (sy+2); y++)
					{
						for (x = sx; x < (sx+2); x++)
						{
							for(layer=0; layer<main.tdata.detailPrototypes.Length; layer++)
							{
								if(main.tdata.GetDetailLayer( (int) x,  (int) y, 1, 1, layer) [0, 0]>0)
								{
									posTGT.x=(main.tdata.size.x/main.tres)*x+main.T.transform.position.x;
									posTGT.z=(main.tdata.size.z/main.tres)*y+main.T.transform.position.z;
									posTGT.y = main.T.SampleHeight( new Vector3(posTGT.x, 0, posTGT.z))+main.T.GetPosition().y; 
									if(!Physics.Linecast(transform.position+Vector3.up*withersSize, posTGT+Vector3.up*withersSize)) { objTGT=null; behavior="ToFood"; return true; }
								}
							}
						}
					}

				}
			}
		}
		objTGT=null; posTGT=Vector3.zero; return false; //not found
	}

	//***********************************************************************************************************************************************************************************************************
	//FIND PREY (Find a size suited prey for carnivorous by priority)
	bool FindPrey()
	{
		Vector3 V1= Vector3.zero;
		foreach (GameObject o in main.creaturesList.ToArray())
		{
			if((o.transform.position-transform.position).magnitude>preyMaxRange) continue; 	//not in range
			if(o.gameObject==transform.gameObject) continue; 	//own gameobject, skip
			Creature other= o.GetComponent<Creature>(); //Get other creature script

      if(!CanSwim && other.IsInWater) continue;
			if(CanSwim && !CanWalk && !other.IsInWater) continue;

			if((other.Herbivorous && other.withersSize<withersSize*3))
			{ behavior="ToHunt"; objTGT=other.gameObject; posTGT=other.body.worldCenterOfMass; return true; } // suitable herbivorous prey found
			else if((!other.Herbivorous && other.withersSize<withersSize*1.5f) && !other.specie.Equals(specie))
			{ behavior="ToHunt"; objTGT=other.gameObject; posTGT=other.body.worldCenterOfMass; } // suitable carmivorous prey found
			else if(Food==0 && other.withersSize<withersSize*3) { objTGT=other.gameObject; V1=other.body.worldCenterOfMass; } // any prey, cannibalism allowed
		}
		if(V1==Vector3.zero) return false; else { behavior="ToHunt"; posTGT=V1; return true; }
	}

	//***********************************************************************************************************************************************************************************************************
	//FIND FRIEND (Find in current creature list a same specie creature and share his activity)
	bool FindFriend()
	{
		foreach (GameObject o in main.creaturesList.ToArray())
		{
			Creature other= o.GetComponent<Creature>(); //Get other creature script
			float range=(other.transform.position-transform.position).magnitude;
			if(range>friendMaxRange | range<boxscale.x*25 ) continue; //not in range
			else if(!other.specie.Equals(specie) | other.IsDead) continue;  //skip, not same specie or dead
			else if(other.gameObject==transform.gameObject) continue; //skip, own gameobject
			else if(other.IsInWater && !CanSwim) continue; //skip, in water and can't swim
			else if(!other.IsInWater && CanSwim) continue; //skip, not in water and can't walk

			// share friend prey
			if(other.behavior.EndsWith("Hunt") && other.objTGT!=transform.gameObject)
			{ behavior="ToHunt"; objTGT=other.objTGT; posTGT=other.posTGT; return true; }
			else if(other.behavior.Equals("Battle")  && other.objTGT!=transform.gameObject)
			{ behavior="Battle"; objTGT=other.objTGT; posTGT=other.posTGT; return true; }
			// share friend food
			else if(other.behavior.EndsWith("Food") && Food<75)
			{ behavior="ToFood"; objTGT=other.objTGT; posTGT=other.posTGT; return true; }
			// share friend water
			else if(other.behavior.EndsWith("Water") && Water<75)
			{ behavior="ToWater"; posTGT=other.posTGT; return true; }
			// goto friend position
			else
			{
				lookTGT=other.transform.position;
				behavior="ToHerd"; objTGT=other.gameObject; posTGT= other.body.worldCenterOfMass; return true;
			}
		}

		objTGT=null; posTGT = Vector3.zero; return false; //nothing found
	}

	//***********************************************************************************************************************************************************************************************************
	//FIND ENEMY (find any hostile target in current creature list and adapts its behavior according to the target)
	bool FindEnemy()
	{
		if (loop==0 && !behavior.Equals("ToFlee") && !behavior.Equals("Battle") && !behavior.EndsWith("Hunt") && !behavior.Equals("ToTarget"))
		{
			//Look for all creatures
			foreach (GameObject o in main.creaturesList.ToArray())
			{
				if(o.gameObject==transform.gameObject) continue; //own gameobject
				float range=(o.transform.position-transform.position).magnitude; //range
				if(range>enemyMaxRange) continue; //not in range

				Creature other= o.GetComponent<Creature>(); //Get other creature script
				if(other.IsDead) continue; //skip, dead

				//Carnivorous behavior
				if(!Herbivorous) 
				{
					if(!other.Herbivorous && (other.behavior.EndsWith("Hunt") | other.behavior.Equals("Battle")))
					{
						if(other.specie==specie && other.objTGT != transform.gameObject ) continue;
						if(boxscale.z>other.boxscale.z/1.5f) 
						{
							behavior="Battle"; objTGT=other.gameObject;
							posTGT=other.transform.position; return true;
						}
						else
						{ behavior="ToFlee"; objTGT=other.gameObject; FindPath(true); return true; }
					}
					else if((other.Herbivorous && other.behavior.Equals("Battle"))
						&& boxscale.z>other.boxscale.z/3 && other.objTGT==transform.gameObject)
					{
						behavior="ToFlee"; behaviorCount=1000;
						objTGT=other.gameObject;
						FindPath(true); return true;
					}
				}
				//Herbivorous behavior
				else
				{
					if(!other.Herbivorous)
					{
						if(other.behavior.EndsWith("Hunt") | (other.objTGT == transform.gameObject && other.behavior.Equals("Battle")))
						{
							if(CanAttack && boxscale.z>other.boxscale.z/3 && Health >25 && other.objTGT == transform.gameObject)
							{
								behavior="Battle"; behaviorCount=1000;
								objTGT=other.gameObject;
								posTGT=other.body.worldCenterOfMass;  
								return true;
							}
							else if(!other.behavior.Equals("ToFlee"))
							{
								behavior="ToFlee"; behaviorCount=1000;
								objTGT=other.gameObject;
								FindPath(true); return true;
							}
						}
					}
				}
			}
		} return false;
	}

	//***********************************************************************************************************************************************************************************************************
	//FIND CUSTOM TARGET (find any enemy or friend target added into target editor list)
	bool FindCustomTarget()
	{
		if(loop==0)
		{
			//Looking for custom target
			foreach ( _TargetEditor o in TargetEditor)
			{
				if(!o._GameObject) { TargetEditor.Remove(o); continue; } //gameobject no more exist
				if(o.MaxRange!=0 && (o._GameObject.transform.position-transform.position).magnitude>o.MaxRange) continue; //not in range

				if(o._TargetType==TargetType.Enemy)
				{
					if(CanAttack)
					{
						if((o._GameObject.transform.position-transform.position).magnitude>enemyMaxRange)
						{ objTGT=o._GameObject; posTGT=o._GameObject.transform.position; behavior="ToTarget";  return true; }
						else
						{ objTGT=o._GameObject; posTGT=o._GameObject.transform.position; behavior="Battle";  return true; }

					}
					else if((o._GameObject.transform.position-transform.position).magnitude<enemyMaxRange)
					{ objTGT=o._GameObject; behavior="ToFlee";  FindPath(true); return true; }
				}
				else
				{
					if((o._GameObject.transform.position-transform.position).magnitude<boxscale.z*10) continue; //target are near
					Creature other= o._GameObject.GetComponent<Creature>(); //Get other creature script
					if(other)
					{
						// share friend prey/enemy
						if(other.behavior.EndsWith("Hunt") && other.objTGT!=transform.gameObject)
						{
							if(!Herbivorous) behavior="ToHunt"; else behavior="Battle";
							objTGT=other.objTGT; posTGT=other.posTGT; return true;
						}
						else if(other.behavior.Equals("Battle") && other.objTGT!=transform.gameObject)
						{  behavior="Battle"; objTGT=other.objTGT; posTGT=other.posTGT; return true; }
						// share friend food
						else if(other.behavior.EndsWith("Food") && Food<75)
						{ behavior="ToFood"; objTGT=other.objTGT; posTGT=other.posTGT; return true; }
						// share friend water
						else if(other.behavior.EndsWith("Water") && Water<75)
						{ behavior="ToWater"; posTGT=other.posTGT; return true; }
						// goto friend position
						else
						{
							lookTGT=other.transform.position;
							behavior="ToFriend"; objTGT=other.gameObject; posTGT= other.body.worldCenterOfMass; return true;
						}
					}
					else
					{
						objTGT=o._GameObject; lookTGT=objTGT.transform.position;
						posTGT=o._GameObject.transform.position; behavior="ToFriend";  return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	//***********************************************************************************************************************************************************************************************************
	//FIND PATH (find reachable path)
	bool FindPath(bool invert=false)
	{
		RaycastHit hit; Vector3 TGT=Vector3.zero; float dist, angle, alt;
    // FLY TYPE
		if(CanFly)
		{
      //altitude
      if(IsOnGround) alt = Random.Range(-90f, 0); 
			else if(posY<(LowAltitude?-75:-150)) { alt =Random.Range(0, 45f); }

			else alt = Random.Range(-15f, 0);
      //distance
      dist=Random.Range(boxscale.z*10, boxscale.z*20);
      //direction 
      if(invert && objTGT)
      {
        angle=Random.Range(-15f-angleAdd, 15f+angleAdd);
			  TGT=transform.position+(Quaternion.Euler(alt, objTGT.transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }
      else
      {
        angle=Random.Range(-45f-angleAdd, 45f+angleAdd);
			  TGT=transform.position+(Quaternion.Euler(alt, transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }
			//check if position is reachable
			if(behavior.Equals("ToRepose"))  //check for ground...
			{
				if(Physics.Linecast(TGT+Vector3.up*withersSize, TGT-Vector3.up*200, out hit))
				{ if(hit.collider.gameObject.layer.Equals(main.waterLayer)) TGT = Vector3.zero; else TGT=hit.point; } 
			}
			else if(Physics.Linecast(transform.position+Vector3.up*withersSize, TGT+Vector3.up*withersSize)) TGT = Vector3.zero; //check for obstacle
		}
    // GROUND TYPE
		else if((CanWalk && !CanSwim) | (CanSwim && CanWalk && !IsOnWater && !IsInWater))
		{
      //direction and distance
      if(invert && objTGT)
      {
			  dist=Random.Range(boxscale.z*4, boxscale.z*10);
        angle=Random.Range(-15-angleAdd, 15+angleAdd);
			  TGT=transform.position+(Quaternion.Euler(0, objTGT.transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }
      else
      {
			  dist=Random.Range(boxscale.z*4, boxscale.z*10);
        angle=Random.Range(-45-angleAdd, 45+angleAdd);
			  TGT=transform.position+(Quaternion.Euler(0, transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }

			//check if position is reachable
			if(!Physics.Linecast(transform.position+Vector3.up*withersSize, TGT+Vector3.up*withersSize) && Physics.Linecast(TGT+Vector3.up*withersSize, TGT-Vector3.up*withersSize*2, out hit))
			{ if(hit.collider.gameObject.layer.Equals(main.waterLayer) && !CanSwim) TGT = Vector3.zero; else TGT= hit.point;} else TGT = Vector3.zero;
		}
    // SWIM TYPE
		else if(CanSwim)
		{
      //altitude
      if(IsInWater)
			{
				if(LowAltitude) { if(IsOnGround) alt=0; else alt=Random.Range(0, 45f); }
				else if(IsOnWater) alt=Random.Range(0, 45f);
				else alt=Random.Range(-60f, 60f);
			} else alt=Random.Range(0, 45f);
      //direction and distance
      if(invert && objTGT)
      {
			  dist=Random.Range(boxscale.z*10, boxscale.z*20);
        angle=Random.Range(-15f-angleAdd, 15f+angleAdd);
			  TGT=transform.position+(Quaternion.Euler(alt, objTGT.transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }
      else
      {
			  dist=Random.Range(boxscale.z*4, boxscale.z*15);
        angle=Random.Range(-45f-angleAdd, 45f+angleAdd);
			  if(IsInWater)
			  {
				  if(LowAltitude) { if(IsOnGround) alt=0; else alt=Random.Range(0, 45f); }
				  else if(IsOnWater) alt=Random.Range(0, 45f);
				  else alt=Random.Range(-60f, 60f);
			  } else alt=Random.Range(0, 45f);
			  TGT=transform.position+(Quaternion.Euler(alt, transform.eulerAngles.y+angle, 0)*Vector3.forward*dist);
      }
			
			//check if position is reachable
			if(IsInWater && TGT.y>waterY) TGT = Vector3.zero;
			if(Physics.Linecast(transform.position+Vector3.up*withersSize, TGT+Vector3.up*withersSize)) TGT = Vector3.zero; //check for obstacle
		}

    // RESULT
		if(angleAdd> 360) { angleAdd = 0; posTGT = Vector3.zero; return false; }
		if(TGT==Vector3.zero) // not found
		{
			angleAdd+=5; anm.SetInteger("Move", 0);  anm.SetInteger("Idle", 0);
      if(!invert) posTGT = Vector3.zero; 
      return false;
		} 
		else { angleAdd=0; posTGT = TGT; return true; } //reachable position found
	}
  #endregion
  #region EXECUTE BEHAVIOR
  //***********************************************************************************************************************************************************************************************************
  //EXECUTE BEHAVIOR
  void ExecuteBehavior(int idle1=0, int idle2=0, int idle3=0, int idle4=0, int eat=0, int drink=0, int sleep=0)
	{
		if(posTGT==Vector3.zero) return;
		bool EndBehavior=false; Creature other=null;
		// Idles to play for current instance
		int idles_lenght=0; if(idle1>0) idles_lenght++; if(idle2>0) idles_lenght++; if(idle3>0) idles_lenght++; if(idle4>0) idles_lenght++;
		// Generate random action
		if(loop==0) { rndMove=Random.Range(0, 100); if(idles_lenght>0) rndIdle=Random.Range(0, idles_lenght+1); }


    if(objCOL) // obstacle object
    {
      Quaternion r=Quaternion.LookRotation(posCOL-transform.position); //obstacle direction
      avoidDelta=Mathf.DeltaAngle(r.eulerAngles.y, transform.eulerAngles.y); //obstacle angle gap
      if(Mathf.Abs(avoidDelta)>90) avoidDelta=0; //max avoid delta
    }

		if(objTGT) // object target
    {
      other=objTGT.GetComponent<Creature>();
			if(other && !behavior.Equals("ToFlee")) posTGT=other.body.worldCenterOfMass;
    }

    distTGT=(transform.position-posTGT).magnitude; //target distance
    angTGT=Quaternion.LookRotation(posTGT-transform.position); //target direction
    delta=Mathf.DeltaAngle(angTGT.eulerAngles.y, transform.eulerAngles.y); //target angle gap
    actionDist=(transform.position-HeadPos).magnitude; //distance from head

    Debug.DrawLine(Head.transform.position, posTGT);

		// Set Animator parameters for each behavior
		switch(behavior)
		{
		case "ToPath" :
       if(CanFly)
       {
			   if(distTGT>boxscale.z*4.0f) AnmRun(rndMove);
			   else { if(IsOnGround) anmStop(); EndBehavior= true; }
       }
       else
       {
         if(distTGT>actionDist*2.0f) anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
			   else { anmStop(); EndBehavior= true; }
       }
		break;

		case "ToWaypoint" :
			if(distTGT>actionDist*3.0f)
			{
				if(PathEditor[nextPath]._PathType==PathType.Run) AnmRun(rndMove); else
				anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
			}
			else
      { 
         if(PathEditor[nextPath]._TargetAction==TargetAction.Sleep && Stamina!=100) AnmSleep(sleep);
         else if(PathEditor[nextPath]._TargetAction==TargetAction.Eat &&  Food!=100) AnmEat(rndMove, eat,rndIdle, idle1, idle2, idle3, idle4);
         else if(PathEditor[nextPath]._TargetAction==TargetAction.Drink && Water!=100) AnmDrink(drink);
         else { anmStop(); EndBehavior= true; nextPath++; }
      }
		break;


		case "ToTarget":
		case "ToFriend" :
			if(objTGT)
			{
				if(distTGT>actionDist*4.0f) AnmRun(rndMove); 
				else if(distTGT>actionDist*2.0f)
				{
					if(CanFly) AnmRun(rndMove); else
					anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
				}
				else { anmStop(); EndBehavior= true; }
			} else { anmStop(); EndBehavior= true; }
		break;

		case "ToFlee" :
			if(objTGT && (objTGT.transform.position-transform.position).magnitude<enemyMaxRange*2)
			{
        AnmRun(rndMove); if(distTGT<actionDist*4.0f) FindPath(true);
			}
			else { anmStop(); EndBehavior= true; }
		break;

		case "ToHerd":
			if(objTGT)
			{
				if(other && other.Health==0) { anmStop(); EndBehavior= true; }
				else if(other && other.IsInWater && !CanSwim && !CanWalk) { anmStop(); EndBehavior= true; }
				else if(other && !other.IsInWater && CanSwim && !CanWalk) { anmStop(); EndBehavior= true; }
				else if(distTGT>actionDist*10.0f && CanFly) AnmRun(rndMove);
				else if(distTGT>actionDist*3.0f) anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
				// same species contest
				else if(other)
				{
          if(other.behavior.Equals("Contest")) { AnmRun(rndMove); behavior="ToFlee"; FindPath(true); }
					else if(CanWalk && CanAttack && !CanTailAttack && rndMove<=10 && other.behavior.Equals("ToHerd"))
					{
						  other.objTGT=transform.gameObject;
						  other.behavior="Contest";
						  other.behaviorCount=500;
						  behavior="Contest"; behaviorCount=500; anmStop();
					}
					else { anmStop(); EndBehavior= true; }
				}
        else { anmStop(); EndBehavior= true; }
			}
			else { anmStop(); EndBehavior= true; }
		break;

		case "ToFood":
			if(Food==100 | (!Herbivorous && !objTGT)) { anmStop(); EndBehavior= true; }
			else
			{
				if(other && other.Health!=0) { anmStop(); EndBehavior= true; }
        else if(!CanSwim && IsInWater) { anmStop(); EndBehavior= true; }
				else if(!CanWalk && !IsInWater) { anmStop(); EndBehavior= true; }
				else if(CanFly && distTGT>actionDist*4.0f) AnmRun(rndMove);
				else if(!Herbivorous && distTGT>actionDist*1.25f) anmWalk(rndMove, rndIdle, idle1, idle2, idle3, idle4);
        else if(Herbivorous && distTGT>actionDist) anmWalk(rndMove, rndIdle, idle1, idle2, idle3, idle4);
				else { anmStop(); behavior="Food"; behaviorCount =5000; }
			}
		break;

		case "Food":
			if(Food==100 | (!Herbivorous && !objTGT)) { anmStop(); EndBehavior= true; }
			else
			{
				if(other && other.Health!=0) { anmStop(); EndBehavior= true; }
        else if(!CanSwim && IsInWater) { anmStop(); EndBehavior= true; }
				else if(!CanWalk && !IsInWater) { anmStop(); EndBehavior= true; }
				else if(!Herbivorous && distTGT<actionDist*1.25f | (objTGT==objCOL)) AnmEat(rndMove, eat,rndIdle, idle1, idle2, idle3, idle4);
        else if(Herbivorous && distTGT<actionDist) AnmEat(rndMove, eat,rndIdle, idle1, idle2, idle3, idle4);
				else behavior="ToFood";
			}
		break;

		case "ToWater":
			if(Water==100 | IsInWater) { anmStop(); EndBehavior= true; }
			else if(CanFly && distTGT>actionDist) anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
      else if(IsOnWater && IsOnGround)
      { 
        if(CanSwim) { anmStop(); behavior="ToPath";  FindPath(); }
        else { anmStop(); behavior="Water"; behaviorCount =5000; }
      }
			else anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
		break;

		case "Water":
			if(Water==100 | IsInWater) { anmStop(); EndBehavior= true; }
			else if(IsOnWater && IsOnGround && !CanSwim) AnmDrink(drink);
      else behavior="ToWater";
		break;

		case "ToRepose":
			if(Stamina==100) { anmStop(); EndBehavior= true; }
			else if(CanFly && distTGT>boxscale.z*5.0f) AnmRun(rndMove);
			else if(distTGT>boxscale.z) anmWalk(rndMove,rndIdle,idle1, idle2, idle3, idle4);
			else { anmStop(); behavior="Repose"; behaviorCount =5000; }
		break;

		case "Repose":
			if(Stamina==100) { anmStop(); EndBehavior= true; }
			else AnmSleep(sleep);
		break;

    case "Contest":
    if(objTGT)
		{
			if(distTGT>enemyMaxRange) { anmStop(); EndBehavior= true; }
			else if(IsInWater) { anmStop(); EndBehavior= true; }
			else AnmBattle(rndMove, idle1, idle2, idle3, idle4, other);
		}
		else { anmStop(); EndBehavior= true; }
		break;
    case "Battle":
		  if(objTGT)
		  {
				if(distTGT>enemyMaxRange) { anmStop(); EndBehavior= true; }
				else if(!CanSwim && IsInWater) { anmStop(); EndBehavior= true; }
        else if(!CanWalk && IsOnGround && !IsOnWater) { anmStop(); EndBehavior= true; }
				else if(other && other.behavior.Equals("ToFlee") && !Herbivorous) { anmStop(); behavior= "ToHunt"; }
        else if(other && other.behavior.Equals("ToFlee") && Herbivorous) { anmStop(); EndBehavior= true; }
				else if(other && other.Health==0 && !Herbivorous) { anmStop(); behavior="ToFood"; } 
				else if(other && other.Health==0 && Herbivorous) { anmStop(); EndBehavior= true; }
				else AnmBattle(rndMove, idle1, idle2, idle3, idle4, other);
		  }
		  else { anmStop(); EndBehavior= true; }
		break;
  
    case "ToHunt":
			if(objTGT)
			{
				if(other && other.Health==0) { anmStop(); behavior="ToFood";  }
        else if(other && distTGT<enemyMaxRange && (other.behavior.Equals("ToHunt") | other.behavior.Equals("Battle"))) { anmStop(); behavior="Battle"; }
        else if(!CanSwim && IsInWater) { anmStop(); EndBehavior= true; }
        else if(!CanWalk && IsOnGround && !IsOnWater) { anmStop(); EndBehavior= true; }
        else if(distTGT>actionDist*1.5f) AnmRun(rndMove);
				else behavior="Hunt";
			}
			else { anmStop(); EndBehavior= true; }
		break;
		case "Hunt":
			if(objTGT)
			{
				if(other && other.Health==0) { anmStop(); behavior="ToFood"; }
        else if(other && distTGT<enemyMaxRange && (other.behavior.Equals("Hunt") | other.behavior.Equals("Battle"))) { anmStop(); behavior="Battle"; }
        else if(!CanSwim && IsInWater) { anmStop(); EndBehavior= true; }
        else if(!CanWalk && IsOnGround && !IsOnWater) { anmStop(); EndBehavior= true; }
        else if(distTGT<actionDist*1.5f) AnmHunt(rndMove, other);
				else behavior="ToHunt";
			}
			else { anmStop(); EndBehavior= true; }
		break;

		default : anmStop(); EndBehavior= true; break;
		}

		if(behaviorCount<=0) EndBehavior=true; else behaviorCount--; // Behavior counter, end if reach 0
		if(EndBehavior) { objTGT=null; posTGT=Vector3.zero; }; // End of this behavior, go to the AI entry point...
	}
  #endregion
  #region ANIMATOR SETUP
  #region TURN
  enum Vect{forward, strafe, backward, zero };
	void AnmTurn (Vect type)
	{
    switch (type)
		{
      case Vect.backward: anm.SetFloat("Turn", angTGT.eulerAngles.y+(delta>0.0f?180f:-180f)); break;
		  case Vect.strafe:   anm.SetFloat("Turn", angTGT.eulerAngles.y+(delta>0.0f?90:-90)); break;
		  case Vect.forward:  anm.SetFloat("Turn", angTGT.eulerAngles.y+avoidAdd); break;
      case Vect.zero:
      if(delta>135 | delta<-135 | (CanSwim&&distTGT<actionDist*0.9f) | (CanWalk&&distTGT<actionDist*0.4f)) anm.SetInteger("Move", -1);
		  else if(delta>45) anm.SetInteger("Move", 10);
      else if(delta<-45) anm.SetInteger("Move", -10);
      anm.SetFloat("Turn", angTGT.eulerAngles.y);
      break;
    }
	}
  #endregion
  #region MOVES
  void anmStop()
	{
		if(CanAttack) anm.SetBool("Attack", false); anm.SetInteger("Move", 0); anm.SetInteger("Idle", 0);
	}

	void anmWalk(int rndMove, int rndIdle, int idle1, int idle2, int idle3, int idle4)
	{
		if(CanAttack) anm.SetBool("Attack", false);

		if(CanFly)
		{
      if(!IsOnGround) anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
      else anm.SetFloat("Pitch", 0.25f);
			if(rndMove>95) { AnmIdles(rndIdle, idle1, idle2, idle3, idle4); }
      else { anm.SetInteger("Idle", 0); anm.SetInteger("Move", 1); }
      AnmTurn(Vect.forward);
		}
		else
		{
      if(CanSwim)  anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
			if(rndMove>98) AnmIdles(rndIdle, idle1, idle2, idle3, idle4);
			else if(rndMove>96) { anm.SetInteger("Move", 1);  anm.SetInteger("Idle", 1); }
			else { anm.SetInteger("Move", 1);  anm.SetInteger("Idle", 0); }
      AnmTurn(Vect.forward);
		}
	}

	void AnmRun(int rndMove)
	{
		if(CanAttack) anm.SetBool("Attack", false);
		if(CanSwim) anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
    if(CanFly) anm.SetFloat("Pitch", IsOnGround?-0.75f:(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
		if(rndMove>98) anm.SetInteger("Idle", 1); else anm.SetInteger("Idle", 0);
    anm.SetInteger("Move", 2);
		AnmTurn(Vect.forward);
	}
  #endregion
  #region ACTIONS
  void AnmDrink(int drink)
	{
		if(CanAttack) anm.SetBool("Attack", false); anm.SetInteger("Move", 0);
		if(CanFly && !IsOnGround) { anm.SetFloat("Pitch", 0.25f); }
		else
    { 
      anm.SetInteger("Idle", drink);
      Water=Mathf.Clamp(Water+0.025f, 0.0f, 100f);
    }
	}

	void AnmEat(int rndMove, int eat, int rndIdle, int idle1, int idle2, int idle3, int idle4)
	{
		if(CanAttack) anm.SetBool("Attack", false); anm.SetInteger("Move", 0);
		if(CanSwim)
    { 
      body.MovePosition(Vector3.Lerp(transform.position, posTGT+(transform.position-Head.GetChild(0).GetChild(0).position), 0.01f));
      anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
    }
		if(CanFly && !IsOnGround) anm.SetFloat("Pitch", 0.25f);
		else if((delta<45 && delta>-45)) 
		{
      if(anm.GetInteger("Idle")==eat) { Food=Mathf.Clamp(Food+0.05f, 0.0f, 100f); if(Water<25) Water+=0.1f; }

			if(rndMove>50) anm.SetInteger("Idle", eat);
			else if(rndMove>25) anm.SetInteger("Idle", 0);
			else AnmIdles(rndIdle, idle1, idle2, idle3, idle4);
		} else anm.SetInteger("Idle", 0);
    AnmTurn(Vect.zero);
	}

	void AnmSleep(int sleep)
	{
		if(CanAttack) anm.SetBool("Attack", false); anm.SetInteger("Move", 0);
		if(CanFly && !IsOnGround) anm.SetFloat("Pitch", 0.25f);
		else
		{
			  Stamina=Mathf.Clamp(Stamina+0.01f, 0.0f, 100f);
			  if(!OnAnm.IsName(specie+"|SitIdle")) anm.SetInteger("Idle", sleep);
			  else if(Herbivorous && rndMove>95) anm.SetInteger("Idle", sleep);
			  else if(Herbivorous && rndMove>90) anm.SetInteger("Idle", 1);
        else if(Herbivorous) anm.SetInteger("Idle", 0);
		}
	}

	void AnmIdles(int rndIdle, int idle1, int idle2, int idle3, int idle4)
	{
		if(CanAttack) anm.SetBool("Attack", false); anm.SetInteger("Move", 0);
		switch(rndIdle)
		{
			case 0: anm.SetInteger("Idle", 0); break;
			case 1: anm.SetInteger("Idle", idle1); break;
			case 2: anm.SetInteger("Idle", idle2); break;
			case 3: anm.SetInteger("Idle", idle3); break;
			case 4: anm.SetInteger("Idle", idle4); break;
		}
	}
  #endregion
  #region BATTLES
  void AnmHunt(int rndMove, Creature other)
	{
    bool aim=false; if(delta<-25| delta>25 | (other&&!other.anm.GetInteger("Move").Equals(2))) aim=true;
     //Air hunt
    if(CanFly)
    {
      AnmTurn(Vect.forward); anm.SetBool("OnGround", false); IsOnGround=false;
      anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
      if(other)
      { 
        body.velocity=other.body.velocity;
        body.MovePosition(Vector3.Lerp(transform.position, other.body.worldCenterOfMass+(transform.position-Head.GetChild(0).position)+transform.up, 0.1f));
      } else body.MovePosition(Vector3.Lerp(transform.position, objTGT.transform.position+(transform.position-Head.GetChild(0).position)+transform.up, 0.025f));

      if(rndMove<25) { anm.SetInteger("Move", 1); anm.SetBool("Attack", true); anm.SetInteger("Idle", 0); }
      else if(rndMove<50) { anm.SetInteger("Move", -10); anm.SetBool("Attack", true); anm.SetInteger("Idle", 0); }
      else if(rndMove<75) { anm.SetInteger("Move", -10); anm.SetBool("Attack", true); anm.SetInteger("Idle", 0); }
      else { anm.SetInteger("Move", -1); anm.SetBool("Attack", false); anm.SetInteger("Idle", 1); }
    }
    //Terrestrial hunt
    else if(!CanSwim | (CanSwim && CanWalk && !IsInWater))
    {
      if(objCOL==objTGT) { anm.SetBool("Attack", true);  AnmTurn(Vect.zero); }
      else if(distTGT<actionDist) { anm.SetInteger("Move", 0);  anm.SetBool("Attack", false);  AnmTurn(Vect.zero); }
      else if(distTGT<actionDist*1.25f) { anm.SetInteger("Move",  rndMove<50?1:2);  anm.SetBool("Attack", true); AnmTurn(Vect.forward); }
      else { anm.SetInteger("Move", aim?0:2); anm.SetBool("Attack", false); AnmTurn(Vect.forward); }
    }
    //Water hunt
    else
    {
      if(other) body.MovePosition(Vector3.Lerp(transform.position, other.body.worldCenterOfMass+(transform.position-Head.GetChild(0).position), 0.01f));
      else body.MovePosition(Vector3.Lerp(transform.position, objTGT.transform.position+(transform.position-Head.GetChild(0).position), 0.01f));
      anm.SetInteger("Idle", 0);
      anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
 
      if(distTGT<actionDist) { anm.SetInteger("Move", 0);  anm.SetBool("Attack", false);  AnmTurn(Vect.zero); }
      else if(distTGT<actionDist*1.25f | objCOL==objTGT) { anm.SetInteger("Move",  rndMove<50?1:2);  anm.SetBool("Attack", true); AnmTurn(Vect.forward); }
      else { anm.SetInteger("Move", aim?0:2); anm.SetBool("Attack", false); AnmTurn(Vect.forward); }
    }
	}
  void AnmBattle(int rndMove, int idle1, int idle2, int idle3, int idle4, Creature other)
	{
    //Air battles
    if(CanFly)
    {
      bool aim=false; if(delta<-25| delta>25) aim=true;
      AnmTurn(Vect.forward);
      anm.SetBool("OnGround", false); IsOnGround=false;
      anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);

      if(rndMove<75)
      {
        if(other) body.MovePosition(Vector3.Lerp(transform.position, other.body.worldCenterOfMass+(transform.position-Head.GetChild(0).position)+transform.up, 0.025f));
        else body.MovePosition(Vector3.Lerp(transform.position, objTGT.transform.position+(transform.position-Head.GetChild(0).position)+transform.up, 0.025f));
        if(objCOL==objTGT | distTGT<actionDist*1.25f) anm.SetBool("Attack", true); else anm.SetBool("Attack", false);
        if(rndMove>40) anm.SetInteger("Move", aim?0:1);
        else if(rndMove>30) anm.SetInteger("Move", aim?0:-1);
        else if(rndMove>20) anm.SetInteger("Move", aim?0:10);
        else if(rndMove>10) anm.SetInteger("Move", aim?0:-10);
        else anm.SetInteger("Move", 0);
      }
      else if(distTGT<actionDist*5.0f)
      {
         anm.SetBool("Attack", false); anm.SetInteger("Idle", Random.Range(0, 100)==0?1:0);
        if(rndMove>95) anm.SetInteger("Move", aim?0:1);
        else if(rndMove>90) anm.SetInteger("Move", aim?0:-1);
        else if(rndMove>85) anm.SetInteger("Move", aim?0:10);
        else if(rndMove>80) anm.SetInteger("Move", aim?0:-10);
        else anm.SetInteger("Move", 0);
      } else { anm.SetInteger("Move", 2); anm.SetInteger("Idle", Random.Range(0, 100)==0?1:0); }
    }
    //Terrestrial battles
    else if(!CanSwim | (CanSwim && CanWalk && !IsInWater))
    {
      if((other&&((rndMove<50 | other.rndMove<50) && distTGT<actionDist*2.0f)) | (!other&&(rndMove<50 && distTGT<actionDist*2.0f)) )
      {
        anm.SetInteger("Idle",0);
        if(distTGT<actionDist) {  anm.SetInteger("Move",-1); AnmTurn(Vect.forward); anm.SetBool("Attack", false); }
        else if(distTGT<actionDist*1.25f) { anm.SetInteger("Move", rndMove<25?0:1); AnmTurn(Vect.forward); anm.SetBool("Attack", true); }
        else { anm.SetInteger("Move", rndMove<25?1:2);  AnmTurn(Vect.forward); anm.SetBool("Attack", true); }
      }
      else if(distTGT<actionDist*5.0f)
      {
        anm.SetBool("Attack", false);
        if(other && distTGT<actionDist*2.0f && (rndMove>50 && other.rndMove<50)) { anm.SetInteger("Move", -1); AnmTurn(Vect.forward); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
        else if(distTGT<actionDist*2.0f && rndMove>50) { anm.SetInteger("Move", -1); AnmTurn(Vect.forward); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
        else if(other && rndMove<50 && other.rndMove<50) { anm.SetInteger("Move", 2);  AnmTurn(Vect.forward); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
        else if(!other && rndMove<50) { anm.SetInteger("Move", 2);  AnmTurn(Vect.forward); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
        else if(!CanSwim && rndMove<75) { anm.SetInteger("Move", 1);  AnmTurn(Vect.strafe); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
        else { anm.SetInteger("Move", 0); AnmTurn(Vect.forward); AnmIdles(rndIdle, idle1, idle2, idle3, idle4); }
      }
      else { anm.SetInteger("Move", 2); anm.SetBool("Attack", false); AnmTurn(Vect.forward); anm.SetInteger("Idle", Random.Range(0, 10)==0?1:0); }
    }
    //Water battles 
    else if(CanSwim)
    {
      anm.SetFloat("Pitch",(Vector3.Angle(Vector3.up, (body.worldCenterOfMass-posTGT).normalized)-90f)/-90f);
      if(Mathf.Abs(delta)<25)
      {
        AnmTurn(Vect.forward);  
        if(distTGT<actionDist*2.0f) { anm.SetInteger("Move", rndMove<50?0:1);  anm.SetBool("Attack", true); }
        else if(distTGT<actionDist*3.0f) { anm.SetInteger("Move", 2);  anm.SetBool("Attack", true); }
        else  { anm.SetInteger("Move", 2);  anm.SetBool("Attack", false); }
      }
      else
      {
        if(rndMove<33) { AnmTurn(Vect.strafe); anm.SetInteger("Move", 1); }
        else if(rndMove<66) { AnmTurn(Vect.forward); anm.SetInteger("Move", 2); }
        else { AnmTurn(Vect.zero); }
      }
    }
	}
  #endregion
  #endregion
  #endregion
}


