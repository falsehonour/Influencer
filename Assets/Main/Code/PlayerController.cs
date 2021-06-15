using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private class PlayerServerData
    {
        public const float MAX_NONTAGGER_SPEED = 5.5f;
        public const float MAX_TAGGER_SPEED = MAX_NONTAGGER_SPEED;
        public const float SPRINT_SPEED = 8f;

        public const float MAX_TRAPPED_MOVEMENT_SPEED = 2.2f;

        public static readonly float MIN_TAG_SQUARED_DISTANCE = (2.33f * 2.33f);
        public const sbyte MAX_HEALTH = 32;
        //public const float TRAP_DURATION = 2f;
        private const float HEALTH_LOSS_INTERVAL = 1f;
        private const float NEARBY_PLAYERS_DETECTION_INTERVAL = 0.05f;

        public List<PlayerController> playersInRange;
        public RepeatingTimer healthReductionTimer;
        public RepeatingTimer nearbyPlayersDetectionTimer;
        //public SingleCycleTimer trapTimer;
        public SingleCycleTimer freezeTimer;
        public SingleCycleTimer sprintTimer;

        public PlayerServerData()
        {
            playersInRange = new List<PlayerController>();
            healthReductionTimer = new RepeatingTimer(HEALTH_LOSS_INTERVAL);
            nearbyPlayersDetectionTimer = new RepeatingTimer(NEARBY_PLAYERS_DETECTION_INTERVAL);
           // trapTimer = new SingleCycleTimer();
            freezeTimer = new SingleCycleTimer();
            sprintTimer = new SingleCycleTimer();
        }
    }

    //This object should exist only on the server.
    private PlayerServerData serverData;

    /*private enum PlayerActions
    {
        InputMovement,AutomaticMovement;
    }*/
    [SerializeField] private CharacterController characterController;
    private Transform myTransform;
    private Joystick joystick;

    [SyncVar] private float currentMaxMovementSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Quaternion desiredRotation;
    private Vector3 controlledVelocity;
    private Vector3 externalForces;
    private float currentGravity = 0;
    private float EXTERNAL_FORCES_REDUCTION_SPEED = 6f;
    private const float PUSH_FORCE = 7.5f;
    private const float SLIP_FORCE = 6f;
    [SerializeField] private float gravity = -9.8f;
    private const float MIN_SQUARED_WAYPOINT_DISTANCE = 0.05f;
    private static readonly Vector3 ZERO_VECTOR3 = Vector3.zero;
    #region Path Finding:

    private NavMeshPath pathCache;
    private Vector3[] pathWayPoints;
    private int pathWayPointsCount;
    private Vector3 currentWayPoint;

    private int currentPathIndex;
    private InteractableAccessPoint desiredInteractableAccessPoint;
    private bool PathFindingIsActive
    {
        get
        {
            return (desiredInteractableAccessPoint != null);
        }
    }
    #endregion
    [SerializeField] private Transform cameraAnchor;//TODO: I believe this is nop longer needed
    private CharacterCamera characterCamera;
    //private Camera camera;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    //private int animatorSpeedParameter;
    private Liftable liftable;// TODO: make it more generic;
    [SerializeField] private Transform grabbingHand;
    //private bool isWalking;
    #region Tagging
    [SyncVar(hook = nameof(OnTaggerChange))] private bool tagger;
    public bool Tagger
    {
        get { return tagger; }
    }
    //[SerializeField] private GameObject hashTag;
    [SyncVar(hook = nameof(OnCanTagChange))] private bool canTag;
    //[SerializeField] private GameObject tagButton;
    private FakeButton fakeTagButton;

    [SyncVar(hook = nameof(OnIsFrozenChanged))] private bool isFrozen;
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
    [SerializeField] private GameObject placeholderGraphics;
    [SerializeField] private Transform playerUIAnchor;
    private PlayerUI playerUI;
    public static PlayerController localPlayerController;
    public static List<PlayerController> allPlayers = new List<PlayerController>();
    //private bool isLocalPlayerCache;
    [SyncVar(hook = nameof(OnPowerUpChanged))] private PowerUp powerUp;
    private PowerUpButton fakePowerUpButton;
    [SerializeField] private GameObject football;

    private void Awake()
    {
        myTransform = transform;
        playerUI = PlayerUIManager.CreatePlayerUI(playerUIAnchor);
        char character = ' ';
        playerUI.SetCharacter(character);
    }

    private void Start()
    {

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

            joystick  = FindObjectOfType<Joystick>();
            /*tagButton = GameObject.Find("TagButton");
            tagButton.SetActive(false);*/

            fakeTagButton = GameObject.Find("FakeTagButton").GetComponent<FakeButton>();
            fakeTagButton.Disable();

            /* fakeFootballButton = GameObject.Find("FakeFootballButton").GetComponent<FakeButton>();
             UpdateFakeFootballButtonText();*/
            fakePowerUpButton = GameObject.Find("FakePowerUpButton").GetComponent<PowerUpButton>();
            UpdatePowerUpButton();

            characterCamera = FindObjectOfType<CharacterCamera>();
            characterCamera.Initialise(myTransform/*, cameraAnchor*/);
            //camera = characterCamera.GetComponent<Camera>();

            //Cmd_SetTagger(GameManager.Tagger);
        }

        if (skin != null)
        {
            Destroy(placeholderGraphics);
            skin.Initialise();
        }

        football.SetActive(false);
    }

    public void SetAnimator(Animator animator, NetworkAnimator networkAnimator)
    {
        this.animator = animator;
        this.networkAnimator = networkAnimator;
    }

    [Server]
    private void Server_Initialise()
    {

        serverData = new PlayerServerData();
        AddPlayer(this);
        //SetTagger(allPlayers.Count == 1);
        DT_health = PlayerServerData.MAX_HEALTH;
        currentMaxMovementSpeed = PlayerServerData.MAX_NONTAGGER_SPEED;

    }

    #region Tagging:

    [Server]
    public void SetTagger(bool value)
    {
        tagger = value;
        currentMaxMovementSpeed = value ? PlayerServerData.MAX_TAGGER_SPEED : PlayerServerData.MAX_NONTAGGER_SPEED;
        
        //This is done in order to prevent a new tagger from tagging previously stored players somehow
        canTag = false;
        serverData.playersInRange.Clear();

        if (value)
        {
            serverData.healthReductionTimer.Reset();
        }

    }

    [Server]
    private void Freeze(float freezeDuration)
    {
        //TODO: combine with push
        isFrozen = true;
        serverData.freezeTimer.Start(freezeDuration);
    }

    [Client]
    private void OnTaggerChange(bool oldValue, bool newValue)
    {
        char character = newValue ? '#' : ' ';
        playerUI.SetCharacter(character);
       // hashTag.SetActive(newValue);
    }

    [Client]
    private void OnCanTagChange(bool oldValue, bool newValue)
    {
        if (localPlayerController == this)
        {
            //tagButton.SetActive(newValue);

            if (newValue)
            {
                fakeTagButton.Enable();
            }
            else 
            {
                if (tagger)
                {
                    fakeTagButton.Disable();
                }
                else
                {
                    fakeTagButton.Invoke("Disable", 0.75f);//LAAAAZY....
                }
            }

        }
    }
    
    [ClientRpc]
    public void Rpc_Win()
    {
        /*char character = 'V';
        playerUI.SetCharacter(character);*/
        if (hasAuthority)
        {
            networkAnimator.SetTrigger(AnimatorParameters.Win);
            characterCamera.distanceMultiplier = 0.8f;
            StartCoroutine(RotateRoutine(Quaternion.Euler(new Vector3(0, 180, 0)), 0.5f));

        }
        // healthGUI.color = Color.yellow;
    }

    [Server]
    private void HandleNearbyPlayersDetection(float timePassed)
    {
        //The content of this method is executed when a timer finishes a cycle
        if (serverData.nearbyPlayersDetectionTimer.Update(timePassed))
        {
            //Debug.Log("NearbyPlayersDetection");
            //TODO: Do these things in a unified method, cache all players locations
            List<PlayerController> playersInRange = serverData.playersInRange;
            //TODO: No need for this clear, we could use an array instead
            playersInRange.Clear();
            Vector3 myPosition = myTransform.position;
            
            //bool playerFound = false;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                PlayerController otherPlayer = allPlayers[i];
                if (otherPlayer != null && otherPlayer != this && otherPlayer.IsAlive)
                {
                    float squaredDistance = Vector3.SqrMagnitude
                        (myPosition - otherPlayer.myTransform.position);

                    if (squaredDistance < PlayerServerData.MIN_TAG_SQUARED_DISTANCE )
                    {
                        playersInRange.Add(otherPlayer);
                    }
                }
            }
            //TODO: perhaps using a syncvar is too slow for this
            canTag = playersInRange.Count > 0;
        }     
    }

    #region Health
    [Server]
    private void ModifyHealth(sbyte by)
    {
        sbyte health = DT_health;
        health += by;
        if(health < 0)
        {
            health = 0;
        }
        else if (health > PlayerServerData.MAX_HEALTH)
        {
            health = PlayerServerData.MAX_HEALTH;
        }

        DT_health = health;

        if (!IsAlive)
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
    private void OnHealthChanged (sbyte oldHealth, sbyte newHealth)
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

    public bool IsAlive
    {
        get
        {
            return Health > 0;
        }
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
            StartCoroutine(RotateRoutine(Quaternion.Euler (new Vector3(0,180,0)),0.5f));
        }
    }

    private IEnumerator RotateRoutine(Quaternion targetRotation ,float timeToComplete)
    {
        Quaternion startRotation = myTransform.rotation;
        Quaternion currentRotation = startRotation;

        /*float currentY = myTransform.rotation.eulerAngles.y
        myTransform.rotation = Quaternion.Euler(0, 180, 0);*/
        float timePassed = 0;
        Debug.Log("target:" + targetRotation.eulerAngles.ToString());
        while(timePassed < timeToComplete)
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

    }

    #endregion
    [Client]
    private void OnIsFrozenChanged(bool oldValue, bool newValue)
    {

        if(hasAuthority)
        {
            if (!newValue)
            {
                networkAnimator.SetTrigger(AnimatorParameters.Recover);
            }
            characterCamera.distanceMultiplier = newValue ? 0.7f : 1;
        }
        //TODO: Add indicators
       // healthGUI.color = (newValue ? Color.blue : Color.green);
    }

    [Client]
    public void TryTag()
    {
        fakeTagButton.Press();
        networkAnimator.SetTrigger(AnimatorParameters.Push);
        Cmd_TryTag();

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
        if (!tagger)
        {
            Debug.LogWarning("A non-tagger is trying to tag!");
            return;
        }

        int playersInRangeCount = serverData.playersInRange.Count;

        if (playersInRangeCount > 0)
        {
            PlayerController nextTagger = null; // = serverData.playersInRange[0];

            if (playersInRangeCount == 1)
            {
                nextTagger = serverData.playersInRange[0];
            }
            else
            {
                Vector3 myPosition = this.myTransform.position;
                float smallestSquaredDistance = float.MaxValue;
                for (int i = 0; i < playersInRangeCount; i++)
                {
                    PlayerController player = serverData.playersInRange[i];

                    float squaredDistance = Vector3.SqrMagnitude(myPosition - player.myTransform.position);
                    if(squaredDistance < smallestSquaredDistance)
                    {
                        nextTagger = player;
                    }
                }
            }
            nextTagger.Freeze(3.5f);//HARDCODED
            nextTagger.SetTagger(true);
            Push(nextTagger);
            SetTagger(false);
        }
        else
        {
            Debug.LogWarning("Can't tag, no players in range");
            return;
        }

    }

    [Server]
    private void Push(PlayerController pushed)
    {
        Vector3 pushForce = myTransform.forward * PUSH_FORCE;
        pushed.TargetRpc_OnPushed(pushForce);
    }

    [TargetRpc]
    private void TargetRpc_OnPushed(Vector3 pushForce)
    {
        //TODO: merge with freeze perhaps?
        networkAnimator.SetTrigger(AnimatorParameters.FlipForward);
        externalForces += pushForce;
    }

    #endregion

    private void Update()
    {
        if (localPlayerController == this)
        {
            float deltaTime = Time.deltaTime;

            if (Input.GetMouseButtonDown(0))
            {
                Interactable interactable = FindInteractable();
                if (interactable != null)
                {
                    InteractableAccessPoint accessPoint =
                        interactable.GetClosestAccessPoint(myTransform.position);
                    ActivateNavMesh(accessPoint);
                }
            }
            #region Testing:
           // Debug.Log("canTag: " + canTag);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryUsePowerUp();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                TryTag();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                PushSelf();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                ReportForward();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Vector3 rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                StartCoroutine(RotateRoutine(Quaternion.Euler(rotation), 2f));
            }
            #endregion
        }
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        GameStates gameState =  GameManager.State;
        if (gameState == GameStates.Waiting || gameState == GameStates.TagGame) 
        {
            if (localPlayerController == this && IsAlive)
            {
                HandleMovement(ref deltaTime);
                //TODO: That's alot of method calls bruh
                /*bool showTagButton = (tagger && canTag);
                tagButton.SetActive(showTagButton);*/
            }
            if (isServer) //Lots of network activity
            {
                if (isFrozen)
                {
                    bool unFreeze = serverData.freezeTimer.Update(deltaTime);
                    if (unFreeze)
                    {
                        isFrozen = false;
                    }
                }

                else if (tagger && IsAlive)//TODO: these conditions don't seem to belong here
                {
                    HandleNearbyPlayersDetection(deltaTime);
                    HandleTaggerHealthReduction(deltaTime);
                }
                //TODO: Find a more elegant way to control all those timers...
                if (serverData.sprintTimer.IsActive)
                {
                    //Debug.Log(" sprint time left: " + serverData.sprintTimer.TimeLeft.ToString("f2"));

                    if (serverData.sprintTimer.Update(deltaTime))
                    {
                        currentMaxMovementSpeed = tagger ? PlayerServerData.MAX_TAGGER_SPEED : PlayerServerData.MAX_NONTAGGER_SPEED;
                    }

                }

                /*if (serverData.trapTimer.IsActive)
                {
                    Debug.LogError("NOT IMPLEMENTED anymore..");
                    bool unTrap = serverData.trapTimer.Update(deltaTime);
                    if (unTrap)
                    {
                        maxMovementSpeed = PlayerServerData.MAX_HEALTHY_MOVEMENT_SPEED;
                    }
                }*/
            }
        }
        
    }

    private void ActivateNavMesh(InteractableAccessPoint interactableAccessPoint)
    {
        desiredInteractableAccessPoint = interactableAccessPoint;
        
        NavMesh.CalculatePath(myTransform.position, interactableAccessPoint.myTransform.position, NavMesh.AllAreas, pathCache);
        pathWayPointsCount = pathCache.corners.Length;
        if (pathWayPoints.Length < pathWayPointsCount)
        {
            Debug.LogError("pathWayPoints.Length < pathCache.corners.Length!");
            return;
        }
        else
        {

            for (int i = 0; i < pathWayPointsCount; i++)
            {
                pathWayPoints[i] = pathCache.corners[i];
            }
            currentPathIndex = 0;
            currentWayPoint = pathWayPoints[currentPathIndex];
        }
    }

    private void DeactivateNavMesh()
    {
        desiredInteractableAccessPoint = null;
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
        float verticalInput   = joystick.Vertical;
        float horizontalInput = joystick.Horizontal;

        //Vector3 controlledMovement = new Vector3();
        //Debug.Log($"Vertical: {verticalInput.ToString("f2")}  + Horizontal: {horizontalInput.ToString("f2")}");

        if ((!isFrozen) && (verticalInput != 0 || horizontalInput != 0))
        {
            Vector3 inputVector3 = new Vector3(horizontalInput, 0, verticalInput);
            desiredRotation = Quaternion.LookRotation(inputVector3);

            controlledVelocity = inputVector3 * currentMaxMovementSpeed;// * deltaTime;
            DeactivateNavMesh();
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

        if (false && PathFindingIsActive)
        {
            Debug.LogError("Not implemented!");
            return;
           /* Vector3 difference = (currentWayPoint - myTransform.position);
            float squaredDistance = Vector3.SqrMagnitude(difference);

            if (squaredDistance < MIN_SQUARED_WAYPOINT_DISTANCE)
            {
                currentPathIndex++;

                if (currentPathIndex >= pathWayPointsCount)
                {
                    desiredRotation = desiredInteractableAccessPoint.myTransform.rotation;
                    //desiredInteractableAccessPoint.Interact(this);
                    uint id = desiredInteractableAccessPoint.GetInterractable().netId;
                    CmdTryInteract(id);
                    DeactivateNavMesh();
                }
                else
                {
                    currentWayPoint = pathWayPoints[currentPathIndex];
                }
            }
            else
            {

                controlledMovement = difference.normalized * maxMovementSpeed;// * deltaTime;
                velocity.x = controlledMovement.x;
                velocity.z = controlledMovement.z;

                difference.y = 0;
                desiredRotation = Quaternion.LookRotation(difference);
            }*/
        }

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
            //animator.SetFloat(AnimatorParameters.Speed, metersPerSecond);

            //HARDCODED AHEAD
            int animationSpeedState = 0;
            if(kilometresPerHour > 22f)
                animationSpeedState = 3;
            else if (kilometresPerHour > 7.65f)
                animationSpeedState = 2;
            else if (kilometresPerHour > 0)
                animationSpeedState = 1;
            animator.SetInteger(AnimatorParameters.SpeedState, animationSpeedState);

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

    [Command]
    private void CmdTryInteract(uint interactableID)
    {
        //desiredInteractableAccessPoint.Interact(this);
        Debug.Log("CmdTryInteract");

        var spawned = NetworkIdentity.spawned;
        NetworkIdentity networkIdentity = spawned[interactableID];
        if (networkIdentity == null)
        {
            Debug.LogError("THE NETWORK IDENTITY YOU WERE LOOKING FOR DOES NOT EXIST!");
            return;
        }
        Interactable interactable = networkIdentity.GetComponent<Interactable>();
        if(interactable == null)
        {
            Debug.LogError("THE INTERACTABLE YOU WERE LOOKING FOR DOES NOT EXIST!");
            return;
        }

        float distance =  Vector3.Distance(myTransform.position, interactable.transform.position);

        if (distance < 2)//TODO: Add serious check
        {
            Debug.Log("interactable.Interact(/*this*/);");
            interactable.Interact(/*this*/);
            if(interactable is Liftable)
            {
                Lift((Liftable)interactable);
            }
        }
        else
        {
            Debug.LogWarning($"distance = {distance.ToString("f3")}, illegal interaction");
        }
    }

    public void Lift(Liftable liftable)
    {
        animator.SetTrigger("Lift");
        this.liftable = liftable;      
    }

    public void GrabLiftable()
    {
        liftable.Graphics.SetParent(grabbingHand);
    }

    public void ReleaseLiftable()
    {
        liftable.Graphics.SetParent(grabbingHand);
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

    #region Shooting:

    [Server]
    private void Shoot()
    {
        //NOTE: rotation and position are based on what's on the server, this might cause noticeable impercision
        Vector3 bulletSpawnPosition =
            (myTransform.position + (myTransform.forward * 0.8f) + (Vector3.up * 0.5f));//HARDCODED
        Quaternion bulletSpawnRotation = myTransform.rotation;
        Spawner.Spawn(Spawnables.Bullet, bulletSpawnPosition, bulletSpawnRotation);
    }

    [Server]
    public void OnBulletHit()
    {
        //TODO: modify speed instead of health
        ModifyHealth(-1); 
    }

    #endregion

    #region FootballThrowing:
   
    /*[Client]
    public void TryThrowFootball()
    {
        //Debug.Log("TryShoot");
        fakeFootballButton.Press();

        if (footballCount > 0)
        {
            networkAnimator.SetTrigger(AnimatorParameters.Throw);
            Cmd_TryThrowFootball(myTransform.position, myTransform.rotation);
        }
    }*/

    /*[Command]
    private void Cmd_TryThrowFootball(Vector3 clientPlayerPosition, Quaternion clientRotation)
    {
        if (footballCount > 0)
        {
            footballCount--;
            Vector3 ballSpawnPosition =
                (clientPlayerPosition + (myTransform.forward * 1f) + (Vector3.up * 0.5f));//HARDCODED
            Quaternion ballSpawnRotation = clientRotation;
            Spawner.Spawn(Spawnables.ThrownFootball, ballSpawnPosition, ballSpawnRotation);
        }
    }
    */
    [Server]
    private void ThrowFootball()
    {
        //NOTE: rotation and position are based on what's on the server, this might cause noticeable impercision
        Vector3 ballSpawnPosition =
            (myTransform.position + (myTransform.forward * 1f) + (Vector3.up * 0.5f));//HARDCODED
        Quaternion ballSpawnRotation = myTransform.rotation;
        Spawner.Spawn(Spawnables.ThrownFootball, ballSpawnPosition, ballSpawnRotation);   
    }

    [Server]
    public void OnFootballHit()
    {
        ModifyHealth(-2);
    }

    #endregion
    #region BananaThrowing:

    [Server]
    private void ThrowBanana()
    {
        Vector3 bananaPosition = 
            myTransform.position + (myTransform.forward * -1.1f) + (Vector3.up * 2f);//HARDCODED as F%$#
        Quaternion bananaRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        Spawner.Spawn(Spawnables.ThrownBanana, bananaPosition, bananaRotation);
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
        return !isFrozen;
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
        currentMaxMovementSpeed = PlayerServerData.SPRINT_SPEED;
        serverData.sprintTimer.Start(2f);//HARDCODED
    }

    #endregion

    #region PowerUps:

    private void OnPowerUpChanged(PowerUp oldValue, PowerUp newValue)
    {
        if (localPlayerController == this)
        {
            UpdatePowerUpButton();
        }

        football.SetActive(newValue.type == PowerUp.Type.Football);
        skin.ShowGun(newValue.type == PowerUp.Type.Gun);//TODO: the gun dissappears too soon, give it a second before it deactivates
    }

    private void UpdatePowerUpButton()
    {
        /* fakeFootballButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = footballCount.ToString();
         Debug.LogWarning("UpdatePowerUpButton is not implemented");*/
        fakePowerUpButton.SetGraphics(powerUp);
    }

    [Client]
    public void TryUsePowerUp()
    {
        //Debug.Log("TryShoot");
        fakePowerUpButton.Press();
        if (powerUp.type != PowerUp.Type.None && powerUp.count > 0)
        {
            //Play animation, we can rely on the player for this
            int triggerID = 0;
            switch (this.powerUp.type)
            {
                case PowerUp.Type.Football:
                {
                    triggerID = AnimatorParameters.ThrowForward;
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

        }
    }

    [Command]
    private void Cmd_TryUsePowerUp()
    {
        if (powerUp.type != PowerUp.Type.None && powerUp.count > 0)
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
        }
        else
        {
            Debug.LogWarning("Can't use a nonexistent power up.");
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
        //TODO: Players who quit befor the game starts cause bugs. 
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
        //float max = 7f;
        // externalForces = new Vector3(Random.Range(-max, max), 0, Random.Range(-max, max));
        externalForces += myTransform.forward * PUSH_FORCE;
    }

    private void ReportForward()
    {
        Debug.Log("forward:" + myTransform.forward);
        //Vector3.
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Player: OnCollisionEnter");

        if (IsAlive)
        {
            if (isServer)
            {
                
            }
        }
    }*/
    #endregion
}