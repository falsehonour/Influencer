using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Runtime.CompilerServices;

public class PlayerController : NetworkBehaviour
{
    private class PlayerServerData
    {
        public static readonly float INJURED_SPEED = UnitConvertor.MetresPerSecondToKilometresPerHour(10.5f);
        public static readonly float MAX_NONTAGGER_SPEED = UnitConvertor.MetresPerSecondToKilometresPerHour(19.8f);
        public static readonly float MAX_TAGGER_SPEED = MAX_NONTAGGER_SPEED;
        public static readonly float SPRINT_SPEED = UnitConvertor.MetresPerSecondToKilometresPerHour(28.8f);

        //public static readonly float MIN_TAG_SQUARED_DISTANCE = (2.33f * 2.33f);
        public const sbyte MAX_HEALTH = 32;
        //public const float TRAP_DURATION = 2f;
        public const float TAGGER_HEALTH_LOSS_INTERVAL = 100f;//  1f;
        //private const float NEARBY_PLAYERS_DETECTION_INTERVAL = 0.05f;

        public const float TAGGER_FREEZE_DURATION = 3.5f;

        //public List<PlayerController> playersInRange;
        public RepeatingTimer healthReductionTimer;
        public RepeatingTimer nearbyPlayersDetectionTimer;
        public SingleCycleTimer movementStateTimer;
        public MovementStates nextMovementState;

        public PlayerServerData()
        {
            //playersInRange = new List<PlayerController>();
            healthReductionTimer = new RepeatingTimer(TAGGER_HEALTH_LOSS_INTERVAL);
            //nearbyPlayersDetectionTimer = new RepeatingTimer(NEARBY_PLAYERS_DETECTION_INTERVAL);
            movementStateTimer = new SingleCycleTimer();
        }
    }
    //This object should exist only on the server.
    private PlayerServerData serverData;
    private enum MovementStates : byte
    {
        Frozen, Injured, Normal, Sprinting
    }
    [SerializeField] private CharacterController characterController;
    private Transform myTransform;
    private Joystick joystick;
    [SyncVar] private Vector2 forcedMovementInput = Vector3.zero;//TODO: Remove this once testing is over
    [SyncVar(hook = nameof(OnMovementStateChange))] private MovementStates DT_movementState;
    private MovementStates MovementState => DT_movementState;
    [SyncVar] private float currentMaxMovementSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Quaternion desiredRotation;
    private Vector3 controlledVelocity;
    private Vector3 externalForces;
    private float currentGravity = 0;
    //TODO: These three don't belong here
    private float EXTERNAL_FORCES_REDUCTION_SPEED = 6f;
    private const float PUSH_FORCE = 7.5f;
    private const float SLIP_FORCE = 6f;
    [SerializeField] private float gravity = -9.8f;
    private static readonly Vector3 ZERO_VECTOR3 = Vector3.zero;
    [SerializeField] private Transform cameraAnchor;//TODO: I believe this is nop longer needed
    private CharacterCamera characterCamera;
    private Animator animator;
    private NetworkAnimator networkAnimator;
    [SerializeField] private BoxCollider tagBoundsCollider;
    //private bool isWalking;
    #region Tagging
    [SyncVar(hook = nameof(OnTaggerChange))] private bool tagger;
    public bool Tagger
    {
        get { return tagger; }
    }
    private List<PlayerController> playersInRange = new List<PlayerController>();
    private SingleCycleTimer tagCooldownTimer;
    private const float TAG_COOLDOWN_INTERVAL = 0.5f;
    //[SyncVar(hook = nameof(OnCanTagChange))] private bool canTag;
    private FakeButton fakeTagButton;
    #endregion
    #region Health:
    [SyncVar(hook = nameof(OnHealthChanged))] private sbyte DT_health;
    public sbyte Health
    {
        get { return DT_health; }
    }
    //[SyncVar] private bool isAlive;//TODO: Is this noit overkill??
    #endregion
    [SerializeField] private CharacterCreation.NetworkedCharacterSkin skin;
    private CharacterCreation.Character skinCharacter;
    [SerializeField] private GameObject placeholderGraphics;
    [SerializeField] private Transform playerUIAnchor;
    private PlayerUI playerUI;
    //private bool isLocalPlayerCache;
    #region PowerUps:
    private SingleCycleTimer powerUpCooldownTimer;
    private const float POWER_UP_COOLDOWN_INTERVAL = 0.2f;
    [SyncVar(hook = nameof(OnPowerUpChanged))] private PowerUp powerUp;
    private PowerUpButton fakePowerUpButton;
    [SerializeField] private GameObject football;
    #endregion
    private bool initialised = false;
    public static PlayerController localPlayerController;
    public static List<PlayerController> allPlayers = new List<PlayerController>();

    private void Awake()
    {
        myTransform = transform;
    }

    private void Start()
    {

        playerUI = PlayerUIManager.CreatePlayerUI(playerUIAnchor);
        char character = ' ';
        playerUI.SetProceedingCharacter(character);

        if (isServer)
        {
            Server_Initialise();
        }

        if (isLocalPlayer)
        {
            localPlayerController = this;

            //TODO: Remove path finding stuff if it's not gonna be used after all
            /*pathCache = new NavMeshPath();
            pathWayPoints = new Vector3[16];*/

            //TODO: Very inappropriate, pls change
            Joystick[] joysticks = FindObjectsOfType<Joystick>();
            for (int i = 0; i < joysticks.Length; i++)
            {
                if (joysticks[i].isActiveAndEnabled)
                {
                    joystick = joysticks[i];
                    break;
                }
            }
            if(joystick == null)
            {
                Debug.LogError("NO JOYSTICK FOUND!");
            }
            /*tagButton = GameObject.Find("TagButton");
            tagButton.SetActive(false);*/

            fakeTagButton = GameObject.Find("FakeTagButton").GetComponent<FakeButton>();
            fakeTagButton.SetIsEnabled(false);

            /* fakeFootballButton = GameObject.Find("FakeFootballButton").GetComponent<FakeButton>();
             UpdateFakeFootballButtonText();*/
            fakePowerUpButton = GameObject.Find("FakePowerUpButton").GetComponent<PowerUpButton>();
            UpdatePowerUpButton();

            characterCamera = FindObjectOfType<CharacterCamera>();
            characterCamera.Initialise(myTransform/*, cameraAnchor*/);
            //camera = characterCamera.GetComponent<Camera>();

            //Cmd_SetTagger(GameManager.Tagger);
            Cmd_SetName(PlayerName.name);
        }

        if (skin != null)
        {
            Destroy(placeholderGraphics);
            skin.Initialise();
            skinCharacter = skin.character;
        }

        football.SetActive(false);
        initialised = true;
    }

    #region PlayerName
    [SyncVar(hook = nameof(SetName))] private string displayName;
    //TODO: Remove this region, it is supposed to be temporary
    [Command]
    private void Cmd_SetName(string newName)
    {
        displayName = newName;
    }

    [Client]
    private void SetName(string oldValue ,string newValue)
    {
        playerUI.SetPlayerName(newValue);
    }

    #endregion

    public void SetAnimator(Animator animator, NetworkAnimator networkAnimator)
    {
        this.animator = animator;
        this.networkAnimator = networkAnimator;
        //TODO: This is a placeholder. Dance animation should be predetermined by the player
        animator.SetInteger(AnimatorParameters.DanceIndex, Random.Range(0, 4));
    }

    [Server]
    private void Server_Initialise()
    {

        serverData = new PlayerServerData();
        AddPlayer(this);
        DT_health = PlayerServerData.MAX_HEALTH;
        SetMovementState(MovementStates.Normal);
    }

    [Server]
    private void SetMovementState(MovementStates newState)
    {
        DT_movementState = newState;
        switch (newState)
        {
            case MovementStates.Frozen:
                {
                    currentMaxMovementSpeed = 0;
                    break;
                }
            case MovementStates.Injured:
                {
                    currentMaxMovementSpeed = PlayerServerData.INJURED_SPEED;
                    break;
                }
            case MovementStates.Normal:
                {
                    currentMaxMovementSpeed = tagger ? PlayerServerData.MAX_TAGGER_SPEED : PlayerServerData.MAX_NONTAGGER_SPEED;
                    break;
                }
            case MovementStates.Sprinting:
                {
                    currentMaxMovementSpeed = PlayerServerData.SPRINT_SPEED;
                    break;
                }
        }
    }

    [Client]
    private void OnMovementStateChange(MovementStates oldValue, MovementStates newValue)
    {
        if (initialised)
        {
            if (hasAuthority)
            {
                animator.SetBool(AnimatorParameters.Hurting, (newValue == MovementStates.Injured));

                if (oldValue == MovementStates.Frozen || newValue == MovementStates.Frozen)
                {
                    if (newValue != MovementStates.Frozen)
                    {
                        networkAnimator.SetTrigger(AnimatorParameters.Recover);
                        if (tagger)
                        {
                            fakeTagButton.SetIsEnabled(true);
                        }
                    }
                    else
                    {
                        if (tagger)
                        {
                            fakeTagButton.SetIsEnabled(false);
                        }
                    }
                    UpdatePowerUpButton();
                    characterCamera.distanceMultiplier = newValue == MovementStates.Frozen ? 0.7f : 1;//HARDCODEDDD
                    //TODO: Add indicators
                    // healthGUI.color = (newValue ? Color.blue : Color.green);
                }
            }
            
            if(oldValue == MovementStates.Sprinting && powerUp.type != PowerUp.Type.Sprint)
            {
                skinCharacter.ShowWings(false);
            }
        }
    }

    [Server]
    private void Freeze(float freezeDuration)
    {
        SetMovementState(MovementStates.Frozen);
        serverData.movementStateTimer.Start(freezeDuration);
        serverData.nextMovementState = MovementStates.Normal;
    }

    [Server]
    private void Push(PlayerController pushed, Vector3 pushForce, float freezeDuration)
    {
        pushed.Freeze(freezeDuration);    
        pushed.TargetRpc_OnPushed(pushForce);
    }

    [TargetRpc]
    private void TargetRpc_OnPushed(Vector3 pushForce)
    {
        externalForces += pushForce;
        Vector3 pushDirection = pushForce.normalized;
        bool pushedFromBehind;
        {
            Vector3 directionsVector = (pushDirection - myTransform.forward);
            float comparison = directionsVector.sqrMagnitude * 0.25f;
            pushedFromBehind = comparison < 0.5f;
        }    
        //If both directions are the same, comparison should be 0, if we face opposite directions, should get a 1
        int animation = pushedFromBehind ? AnimatorParameters.FlipForward : AnimatorParameters.FallBackward;

        networkAnimator.SetTrigger(animation);
        Quaternion rotation = Quaternion.LookRotation(pushedFromBehind ? pushDirection : -pushDirection);
        //StartCoroutine(RotateRoutine(rotation, 0.5f));//HARDCODED
        desiredRotation = rotation;
    }

    #region Tagging:

    [Server]
    public void SetTagger(bool value, float healthReductionFirstIteration = PlayerServerData.TAGGER_HEALTH_LOSS_INTERVAL)
    {
        tagger = value;
        SetMovementState(MovementStates.Normal);//TODO: Not an ideal way to conform to tagger's speed..
        //currentMaxMovementSpeed = value ? PlayerServerData.MAX_TAGGER_SPEED : PlayerServerData.MAX_NONTAGGER_SPEED;
        
        //This is done in order to prevent a new tagger from tagging previously stored players somehow
        //TODO: should we do this upon freeze??
        /*canTag = false;
        serverData.playersInRange.Clear();*/

        if (value)
        {
            //serverData.healthReductionTimer.Reset();
            serverData.healthReductionTimer.SetCurrentIteration(healthReductionFirstIteration);
        }
    }

    [Client]
    private void OnTaggerChange(bool oldValue, bool newValue)
    {
        char character = newValue ? '#' : ' ';
        playerUI.SetProceedingCharacter(character);
        if (localPlayerController == this)
        {
            fakeTagButton.SetIsEnabled(newValue);
        }
    }

    /* [Client]
     private void OnCanTagChange(bool oldValue, bool newValue)
     {s
         if (localPlayerController == this)
         {
             //tagButton.SetActive(newValue);

             if (newValue)
             {
                 fakeTagButton.SetIsEnabled(true);//.Enable();
             }
             else 
             {
                 if (tagger)
                 {
                     fakeTagButton.SetIsEnabled(false);//.Enable();
                 }
                 else
                 {
                     //TODO: Very gay. 
                     fakeTagButton.Invoke("Disable", 0.75f);//TODO: LAAAAZY....
                 }
             }

         }
     }*/

    /*[Server]
    private void HandleNearbyPlayersDetection(float timePassed)
    {

        //The content of this method is executed when a timer finishes a cycle
        if (serverData.nearbyPlayersDetectionTimer.Update(timePassed))
        {
            if (MovementState == MovementStates.Frozen)
            {
                canTag = false;
            }
            else
            {
                //Debug.Log("NearbyPlayersDetection");
                //TODO: Do these things in a unified method, cache all players locations
                List<PlayerController> playersInRange = serverData.playersInRange;
                //TODO: No need for this clear, we could use an array instead
                playersInRange.Clear();
                //Vector3 myPosition = myTransform.position;
                Bounds tagBounds = tagBoundsCollider.bounds;
                //bool playerFound = false;
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    PlayerController otherPlayer = allPlayers[i];
                    if (otherPlayer != null && otherPlayer != this && otherPlayer.IsAlive)
                    {
                        bool otherPlayerIsInBounds = tagBounds.Contains(otherPlayer.myTransform.position);
                        //(squaredDistance < PlayerServerData.MIN_TAG_SQUARED_DISTANCE)
                        /*float squaredDistance = Vector3.SqrMagnitude(myPosition - otherPlayer.myTransform.position);*
                        if (otherPlayerIsInBounds)
                        {
                            playersInRange.Add(otherPlayer);
                        }
                    }
                }
                //TODO: perhaps using a syncvar is too slow for this
                canTag = playersInRange.Count > 0;
            }
        }     
    }*/

    private void DetectPlayersInTagBounds()
    {    
        //TODO: Do these things in a unified method, cache all players locations
        //TODO: No need for this clear, we could use an array instead
        playersInRange.Clear();
        Bounds tagBounds = tagBoundsCollider.bounds;
        //bool playerFound = false;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerController otherPlayer = allPlayers[i];
            if (otherPlayer != null && otherPlayer != this && otherPlayer.IsAlive())
            {
                bool otherPlayerIsInBounds = tagBounds.Contains(otherPlayer.myTransform.position);
                if (otherPlayerIsInBounds)
                {
                    playersInRange.Add(otherPlayer);
                }
            }
        }
    }

#if UNITY_EDITOR
    //TODO: Make sure these #if segments really do not get into our builds. (How come we can refference PLAYER_DETECTION_RADIUS outside the segment?? )
    /*  private static readonly float MIN_TAG_SQUARED_DISTANCE = (2.33f * 2.33f);
      private static float PLAYER_DETECTION_RADIUS = Mathf.Sqrt(MIN_TAG_SQUARED_DISTANCE);*/
    /* private void OnDrawGizmos()
     {
        // Gizmos.DrawWireSphere(transform.position, PLAYER_DETECTION_RADIUS);
         Gizmos.color = Color.blue;
         Gizmos.DrawWireCube(tagBoundsCollider.transform.position, tagBoundsCollider.transform.localScale);
     }*/
#endif

    private bool CanTryTag()
    {
        return (tagger && !tagCooldownTimer.IsActive() && MovementState != MovementStates.Frozen);
    }

    [Client]
    public void TryTag()
    {
        //TODO: Limit client abilioty to spam us with TryTag calls.
        if (CanTryTag())
        {
            networkAnimator.SetTrigger(AnimatorParameters.Push);
            Cmd_TryTag();
            tagCooldownTimer.Start(TAG_COOLDOWN_INTERVAL);
        }
        /*if (canTag)
        {
            networkAnimator.SetTrigger(AnimatorParameters.Push);
            Cmd_TryTag();
        }*/
    }

    [Command]
    private void Cmd_TryTag()
    {
        //TODO: In case there's more than one players, pick a player based on distance or something.
        if (CanTryTag())
        {
            DetectPlayersInTagBounds();
            int playersInRangeCount = playersInRange.Count;

            if (playersInRangeCount > 0)
            {
                PlayerController nextTagger = null; // = serverData.playersInRange[0];

                if (playersInRangeCount == 1)
                {
                    nextTagger = playersInRange[0];
                }
                else
                {
                    Vector3 myPosition = this.myTransform.position;
                    float smallestSquaredDistance = float.MaxValue;
                    for (int i = 0; i < playersInRangeCount; i++)
                    {
                        PlayerController player = playersInRange[i];
                        float squaredDistance = Vector3.SqrMagnitude(myPosition - player.myTransform.position);
                        if (squaredDistance < smallestSquaredDistance)
                        {
                            nextTagger = player;
                        }
                    }
                }
                nextTagger.SetTagger(true, PlayerServerData.TAGGER_FREEZE_DURATION);
                Vector3 pushForce = myTransform.forward * PUSH_FORCE;
                Push(nextTagger, pushForce, PlayerServerData.TAGGER_FREEZE_DURATION);
                SetTagger(false);
            }

            tagCooldownTimer.Start(TAG_COOLDOWN_INTERVAL);
        }
        else
        {
            Debug.LogWarning("Cannot Try Tag! A player Tried to tag illegally.");
        }
    }

    #region Health
    [Server]
    private void ModifyHealth(sbyte by)
    {
        sbyte health = DT_health;
        health += by;
        if (health < 0)
        {
            health = 0;
        }
        else if (health > PlayerServerData.MAX_HEALTH)
        {
            health = PlayerServerData.MAX_HEALTH;
        }

        DT_health = health;

        if (!IsAlive())
        {
            Server_OnDeath();
        }
    }

    [Server]
    private void HandleTaggerHealthReduction(float deltaTime)
    {
        if (serverData.healthReductionTimer.Update(deltaTime))
        {
            ModifyHealth(-1);
        }
    }

    [Client]
    private void OnHealthChanged(sbyte oldHealth, sbyte newHealth)
    {
        //healthGUI.text = newHealth.ToString();
        //TODO: maybe MAX_HEALTH should not reside in PlayerServerData,.,.
        SetHealthBarFill(newHealth);
    }

    private void SetHealthBarFill(sbyte health)
    {
        float fill = ((float)health / (float)PlayerServerData.MAX_HEALTH);
        playerUI.SetHealthBarFill(fill);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive()
    {
        return (Health > 0);
    }

    #endregion

    #endregion

    private IEnumerator RotateRoutine(Quaternion targetRotation, float timeToComplete)
    {
        Quaternion startRotation = myTransform.rotation;
        Quaternion currentRotation = startRotation;

        /*float currentY = myTransform.rotation.eulerAngles.y
        myTransform.rotation = Quaternion.Euler(0, 180, 0);*/
        float timePassed = 0;
        Debug.Log("target:" + targetRotation.eulerAngles.ToString());
        while (timePassed < timeToComplete)
        {
            float t = (timePassed / timeToComplete);
            //Debug.Log("t:" + t.ToString("f3"));
            currentRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            myTransform.rotation = currentRotation;

            //TODO: maybe use fixed update instead
            timePassed += Time.deltaTime;
            yield return null;
        }

        myTransform.rotation = targetRotation;

        Debug.Log("RotateRoutine finished");
        //float asas =  PLAYER_DETECTION_RADIUS;
    }


    private void Update()
    {
        //float deltaTime = Time.deltaTime;
        if (localPlayerController == this)
        {
            #region Testing:
           // Debug.Log("canTag: " + canTag);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                fakePowerUpButton.SimulatePressFor(0.3f);
                TryUsePowerUp();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                fakeTagButton.SimulatePressFor(0.4f);
                TryTag();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                PushSelf();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Vector3 rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                StartCoroutine(RotateRoutine(Quaternion.Euler(rotation), 2f));
            }
            #endregion
        }
        else if (isServer)
        {
            Vector2 currentForcedMovementInput = forcedMovementInput;

            if (Input.GetKeyDown(KeyCode.A))
                currentForcedMovementInput.x = -1;
            else if (Input.GetKeyUp(KeyCode.A))
                currentForcedMovementInput.x = 0;

            if (Input.GetKeyDown(KeyCode.D))
                currentForcedMovementInput.x = 1;
            else if (Input.GetKeyUp(KeyCode.D))
                currentForcedMovementInput.x = 0;

            if (Input.GetKeyDown(KeyCode.S))
                currentForcedMovementInput.y = -1;
            else if (Input.GetKeyUp(KeyCode.S))
                currentForcedMovementInput.y = 0;

            if (Input.GetKeyDown(KeyCode.W))
                currentForcedMovementInput.y = 1;
            else if (Input.GetKeyUp(KeyCode.W))
                currentForcedMovementInput.y = 0;

            currentForcedMovementInput = Vector2.ClampMagnitude(currentForcedMovementInput,1);

            if (currentForcedMovementInput != forcedMovementInput)
            {
                forcedMovementInput = currentForcedMovementInput;
                Debug.Log("forcedMovementInput Changed");
            }
        }

    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        GameStates gameState =  GameManager.State;
        if (IsAlive() && (gameState == GameStates.Waiting || gameState == GameStates.TagGame)) 
        {
            if (powerUpCooldownTimer.IsActive())
            {
                powerUpCooldownTimer.Update(deltaTime);
            }
            if (tagCooldownTimer.IsActive())
            {
                tagCooldownTimer.Update(deltaTime);
            }

            if (localPlayerController == this)
            {
                HandleMovement(ref deltaTime);
            }
            if (isServer) //Lots of network activity
            {
                if (tagger)//TODO: these conditions don't seem to belong here
                {
                    /*if(MovementState != MovementStates.Frozen)//TODO: Are we sure we don't wanna let anyone tag whilst on the floor..?
                    {
                        HandleNearbyPlayersDetection(deltaTime);
                    }*/
                    HandleTaggerHealthReduction(deltaTime);
                }
                //TODO: Find a more elegant way to control all those timers...
                if (serverData.movementStateTimer.IsActive())
                {
                    //Debug.Log(" sprint time left: " + serverData.sprintTimer.TimeLeft.ToString("f2"));
                    if (serverData.movementStateTimer.Update(deltaTime))
                    {
                        SetMovementState(serverData.nextMovementState);
                    }
                }
            }
        }
       
    }

    private void HandleMovement(ref float deltaTime)
    {
        if(externalForces != ZERO_VECTOR3)
        {
            externalForces = Vector3.MoveTowards
                (externalForces, ZERO_VECTOR3, EXTERNAL_FORCES_REDUCTION_SPEED * deltaTime);
        }
        controlledVelocity = ZERO_VECTOR3;
        //TODO: This area of th code must be revisited, it is probably outdated 
        float verticalInput   = joystick.Vertical + forcedMovementInput.y;
        float horizontalInput = joystick.Horizontal + forcedMovementInput.x;

        //Vector3 controlledMovement = new Vector3();
        //Debug.Log($"Vertical: {verticalInput.ToString("f2")}  + Horizontal: {horizontalInput.ToString("f2")}");

        if ((MovementState != MovementStates.Frozen) && (verticalInput != 0 || horizontalInput != 0))
        {
            Vector3 inputVector3 = new Vector3(horizontalInput, 0, verticalInput);
            //Debug.Log("inputVector3 magnitude:" + inputVector3.magnitude);
            desiredRotation = Quaternion.LookRotation(inputVector3);

            controlledVelocity = inputVector3 * currentMaxMovementSpeed;// * deltaTime;
            //Debug.Log($"movement magnitude: {movement.magnitude}"); Magnitude seems good!
        }

        #region Gravity
         bool isGrounded = characterController.isGrounded; //navMeshAgent.isGrounded;
         if (isGrounded)
         {
            currentGravity = 0;
         }
         else
         {
            currentGravity += gravity * deltaTime;
         }
        #endregion

        //TODO: Move to squaredMagnitude and skip multiplications before shipping.
        float kilometresPerHour = controlledVelocity.magnitude * 3.6f;
       // Debug.Log("player kilometresPerHour:" + kilometresPerHour.ToString("f2"));
        if(kilometresPerHour < 1f)//HARDCODED
        {
            //Stay in place if input is negligible
            controlledVelocity = Vector3.zero;
            kilometresPerHour = 0;
        } 
        if(animator != null)
        {
            //TODO: do this in bigger intervals for performance
           // animator.SetFloat(AnimatorParameters.Speed, kilometresPerHour);

            //HARDCODED AHEAD
            int animationSpeedState = 0;
            if(kilometresPerHour > 22f)
                animationSpeedState = 3;
            else if (kilometresPerHour > 7.65f)
                animationSpeedState = 2;
            else if (kilometresPerHour > 0)
                animationSpeedState = 1;
            animator.SetInteger(AnimatorParameters.SpeedState, animationSpeedState);

            //NOTE: Why the FUCK does mirror not sync the the top three???
            //I think I understand: Our NetworkAnimator only cares about parameters that used to be on our original animator. 
            //This means that further bugs may occur due to the animator swap we do 
            /*animator.SetInteger(AnimatorParameters.SpeedState, animationSpeedState);
            animator.SetInteger("WhatTheHellInt", animationSpeedState);
            animator.SetFloat("WhatTheHellFloat", animationSpeedState);
            animator.SetFloat(AnimatorParameters.Speed, animationSpeedState);*/
        }

        Vector3 velocity = (controlledVelocity + externalForces);
        velocity.y += currentGravity;
        characterController.Move(velocity * deltaTime);
        /*characterController.enabled = false;
        myTransform.position += velocity * deltaTime;*/
        //NOTE: Might interfere with RotateRoutine
        myTransform.rotation = Quaternion.RotateTowards
            (myTransform.rotation, desiredRotation, maxRotationSpeed * deltaTime);
    }

    #region Interactables:
    private Interactable FindInteractable()
    {
        Interactable interactable = null;
        Vector2 mousePosition = Input.mousePosition;
        //NOTE: this is destroyed
        Ray ray = new Ray();// camera.ScreenPointToRay(mousePosition);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit))
        {
            interactable = raycastHit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                Debug.Log("Interactable found!");
            }
        }

        return interactable;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            Server_OnTriggerEnter(other);
        }
    }

    [Server]
    private void Server_OnTriggerEnter(Collider other)
    {
        PickUp pickUp = other.GetComponentInParent<PickUp>();
        if (pickUp != null)
        {
            Collect(pickUp);
        }
    }

    [Server]
    private void Collect(PickUp pickUp)
    {
        pickUp.Collect();
        if(pickUp is HealthPickUp)
        {
            sbyte healthAddition = 8;//Hardcoded
            ModifyHealth(healthAddition);
        }
        else if(pickUp is Trap)
        {
            Debug.LogError("TRAP has to be re-implemented");
            /*
            serverData.trapTimer.Start(PlayerServerData.TRAP_DURATION);
            maxMovementSpeed = PlayerServerData.MAX_TRAPPED_MOVEMENT_SPEED;*/
        }      
        else if(pickUp is PowerUpPickUp)
        {
            PowerUpPickUp powerUpPickUp = (PowerUpPickUp)pickUp;
            powerUp = powerUpPickUp.GetPowerUp();
        }
    }

    [Server]
    private void GetInjured(float duration)
    {
        SetMovementState(MovementStates.Injured);
        serverData.movementStateTimer.Start(duration);
        serverData.nextMovementState = MovementStates.Normal;
    }

    #region PowerUps:

    #region Shooting:

    [Server]
    private void Shoot()
    {
        //NOTE: rotation and position are based on what's on the server, this might cause noticeable impercision
        Vector3 bulletSpawnPosition =
            (myTransform.position + (myTransform.forward * 0.75f) + (Vector3.up * 0.82f));//HARDCODED
        Quaternion bulletSpawnRotation = myTransform.rotation;
        PhysicalProjectile projectile =
            (PhysicalProjectile)Spawner.Spawn(Spawnables.Bullet, bulletSpawnPosition, bulletSpawnRotation, netId);
        // projectile.SetIgnoredCollider(characterController/* GetComponent<Collider>()*/);
        // projectile.SetIgnoredCollider(this/* GetComponent<Collider>()*/);
    }

    [Server]
    public void OnBulletHit()
    {
        GetInjured(1.5f);
    }

    #endregion

    #region FootballThrowing:


    [Server]
    private void ThrowFootball()
    {
        //NOTE: rotation and position are based on what's on the server, this might cause noticeable impercision
        Vector3 ballSpawnPosition =
            (myTransform.position + (myTransform.forward * 0.7f) + (Vector3.up * 0.5f));//HARDCODED
        Quaternion ballSpawnRotation = myTransform.rotation;
        Spawner.Spawn(Spawnables.ThrownFootball, ballSpawnPosition, ballSpawnRotation, netId);

    }

    [Server]
    public void OnFootballHit()
    {
        GetInjured(4f);
    }

    #endregion

    #region BananaThrowing:

    [Server]
    private void ThrowBanana()
    {
        Vector3 bananaPosition =
            myTransform.position + (myTransform.forward * -0.92f) + (Vector3.up * 1.8f);//HARDCODED as F%$#
        Quaternion bananaRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        Spawner.Spawn(Spawnables.ThrownBanana, bananaPosition, bananaRotation, netId);
    }

    [Server]
    public void Slip()
    {
        //TODO: Perhaps make force dependent on the player's current velocity?
        Vector3 force = myTransform.forward * SLIP_FORCE;
        Freeze(2.5f);//HARDCODED
        TargetRpc_OnSlip(force);
        Kevin.TryLaughAt(myTransform);
    }

    [Server]
    public bool CanSlip()
    {
        return MovementState != MovementStates.Frozen;
    }

    [TargetRpc]
    private void TargetRpc_OnSlip(Vector3 force)
    {
        //TODO: merge with freeze perhaps?
        networkAnimator.SetTrigger(AnimatorParameters.FlipForward);
        externalForces += force;
    }

    #endregion

    #region Sprint:

    [Server]
    private void Sprint()
    {
        SetMovementState(MovementStates.Sprinting);
        serverData.movementStateTimer.Start(2f);//HARDCODED
        serverData.nextMovementState = MovementStates.Normal;
    }

    #endregion

    private void OnPowerUpChanged(PowerUp oldValue, PowerUp newValue)
    {
        if (localPlayerController == this)
        {
            UpdatePowerUpButton();
        }
        if(oldValue.type != newValue.type)
        {
            switch (oldValue.type)
            {
                case PowerUp.Type.Football:
                    {
                        football.SetActive(false);
                        break;
                    }
                case PowerUp.Type.Sprint:
                    {//TODO: Turning the wings off has become convoluted...
                        if (MovementState != MovementStates.Sprinting)
                        {
                            skinCharacter.ShowWings(false);
                        }
                        break;
                    }
                case PowerUp.Type.Gun:
                    {//TODO: Show the gun for a bit if it just shot??
                        skinCharacter.ShowGun(false);
                        break;
                    }
                case PowerUp.Type.Banana:
                    {
                        skinCharacter.ShowBanana(false);
                        break;
                    }
            }

            switch (newValue.type)
            {
                case PowerUp.Type.Football:
                    {
                        football.SetActive(true);
                        break;
                    }
                case PowerUp.Type.Sprint:
                    {
                        skinCharacter.ShowWings(true);
                        break;
                    }
                case PowerUp.Type.Gun:
                    {
                        skinCharacter.ShowGun(true);
                        break;
                    }
                case PowerUp.Type.Banana:
                    {
                        skinCharacter.ShowBanana(true);
                        break;
                    }
            }
        }
    }



    private void UpdatePowerUpButton()
    {
        /* fakeFootballButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = footballCount.ToString();
         Debug.LogWarning("UpdatePowerUpButton is not implemented");*/
        fakePowerUpButton.SetGraphics(powerUp);
        bool enableButton = (powerUp.type != PowerUp.Type.None && MovementState != MovementStates.Frozen );
        fakePowerUpButton.SetIsEnabled(enableButton);
    }

    private bool CanUsePowerUp()
    {
        return (IsAlive() && !powerUpCooldownTimer.IsActive() && MovementState != MovementStates.Frozen 
            && powerUp.type != PowerUp.Type.None/* && powerUp.count > 0*/);
    }

    [Client]
    public void TryUsePowerUp()
    {
        //Debug.Log("TryShoot");
        if (CanUsePowerUp())
        {
            //Play animation, we can rely on the player for this
            int triggerID = 0;
            switch (this.powerUp.type)
            {
                case PowerUp.Type.Football:
                {
                    triggerID = AnimatorParameters.Kick;
                    break;
                }
                case PowerUp.Type.Banana:
                {
                    triggerID = AnimatorParameters.ThrowBackward;
                    break;
                }
                case PowerUp.Type.Gun:
                {
                    triggerID = AnimatorParameters.Shoot;
                    break;
                }
            }
            networkAnimator.SetTrigger(triggerID);

            //This should happen after we play the animation cause the powerup can become none due to this function
            Cmd_TryUsePowerUp();
            powerUpCooldownTimer.Start(POWER_UP_COOLDOWN_INTERVAL);

        }
    }

    [Command]
    private void Cmd_TryUsePowerUp()
    {
        if (CanUsePowerUp())
        {
            //NOTE: changing part of a struct does not seem to trigger syncVar hooks,,,

            switch (this.powerUp.type)
            {
                case PowerUp.Type.Football:
                {
                    ThrowFootball();
                    break;
                }
                case PowerUp.Type.Banana:
                {
                    ThrowBanana();
                    break;
                }
                case PowerUp.Type.Sprint:
                {
                    Sprint();
                    break;
                }
                case PowerUp.Type.Gun:
                {
                    Shoot();
                    break;
                }
            }

            PowerUp powerUp = this.powerUp;
            sbyte newCount = (sbyte)(powerUp.count - 1);
            if (newCount <= 0)
            {
                powerUp.type = PowerUp.Type.None;
            }
            powerUp.count = newCount;
            this.powerUp = powerUp;

            powerUpCooldownTimer.Start(POWER_UP_COOLDOWN_INTERVAL);

        }
        else
        {
            Debug.LogWarning("Can't use power up! " +
                "Make sure client and server are synchronised since both depend on CanUsePowerUp.");
        }
    }

    #endregion
    
    [TargetRpc]
    public void TargetRpc_Teleport(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        myTransform.position = position;
        myTransform.rotation = rotation;
        characterController.enabled = true;
    }

    [ClientRpc]
    public void Rpc_Win()
    {
        /*char character = 'V';
        playerUI.SetCharacter(character);*/
        if (hasAuthority)
        {
            networkAnimator.SetTrigger(AnimatorParameters.Dance);
            characterCamera.distanceMultiplier = 0.8f;
            StartCoroutine(RotateRoutine(Quaternion.Euler(new Vector3(0, 180, 0)), 0.5f));

        }
        // healthGUI.color = Color.yellow;
    }

    [Server]
    private void Server_OnDeath()
    {
        SetTagger(false);
        GameManager.UpdatePlayersState();
        Kevin.TryLaughAt(myTransform);
        Rpc_OnDeath();
    }

    [ClientRpc]
    private void Rpc_OnDeath()
    {
        //healthGUI.color = Color.red;
        /*char character = 'X';
        playerUI.SetCharacter(character);*/
        playerUI.gameObject.SetActive(false);

        if (hasAuthority)
        {
            //HARDCODED
            networkAnimator.SetTrigger(AnimatorParameters.Lose);
            characterCamera.distanceMultiplier = 0.8f;
            StartCoroutine(RotateRoutine(Quaternion.Euler(new Vector3(0, 180, 0)), 0.5f));
        }
    }

    private void OnDestroy()
    {
        //if (isServer)
        {
            Server_OnDestroy();
        }
        Destroy(playerUI.gameObject);
    }

    [Server]
    private void Server_OnDestroy()
    {
        RemovePlayer(this);
        GameManager.UpdatePlayersState();
    }

    [Server]
    public static void AddPlayer(PlayerController player)
    {
        allPlayers.Add(player);
    }

    [Server]
    public static void RemovePlayer(PlayerController player)
    {
        allPlayers.Remove(player);
    }

    #region Tests:
    private void PushSelf()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        //randomDirection. = Vector3.ClampMagnitude(randomDirection, 1);
        Vector3 pushForce = randomDirection.normalized * PUSH_FORCE;
        Push(this, pushForce, 3f);
    }
    #endregion
}