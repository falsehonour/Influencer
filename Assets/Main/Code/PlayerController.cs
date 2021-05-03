using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private class PlayerServerData
    {
        public const float MAX_HEALTHY_MOVEMENT_SPEED = 2.7f;
        public const float MAX_TRAPPED_MOVEMENT_SPEED = 1f;

        public const float MIN_TAG_SQUARED_DISTANCE = 10f;
        public const byte MAX_HEALTH = 32;
        public const float TRAP_DURATION = 3f;
        private const float HEALTH_LOSS_INTERVAL = 1f;
        private const float NEARBY_PLAYERS_DETECTION_INTERVAL = 0.066f;

        public List<PlayerController> playersInRange;
        public RepeatingTimer healthReductionTimer;
        public RepeatingTimer nearbyPlayersDetectionTimer;
        public SingleCycleTimer trapTimer;
        public SingleCycleTimer freezeTimer;

        public PlayerServerData()
        {
            playersInRange = new List<PlayerController>();
            healthReductionTimer = new RepeatingTimer(HEALTH_LOSS_INTERVAL);
            nearbyPlayersDetectionTimer = new RepeatingTimer(NEARBY_PLAYERS_DETECTION_INTERVAL);
            trapTimer = new SingleCycleTimer();
            freezeTimer = new SingleCycleTimer();

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

    [SyncVar] private float maxMovementSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Quaternion desiredRotation;
    private Vector3 velocity;
    [SerializeField] private float gravity = -9.8f;
    private const float MIN_SQUARED_WAYPOINT_DISTANCE = 0.05f;
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
    [SerializeField] private Transform cameraAnchor;
     private Camera camera;
    [SerializeField] private Animator animator;
    private Liftable liftable;// TODO: make it more generic;
    [SerializeField] private Transform grabbingHand;
    //private bool isWalking;
    #region Tagging
    [SyncVar(hook = nameof(OnTaggerChange))] private bool tagger;
    public bool Tagger
    {
        get { return tagger; }
    }
    [SerializeField] private GameObject hashTag;
    [SyncVar(hook = nameof(OnCanTagChange))] private bool canTag;
    [SerializeField] private GameObject tagButton;
    [SyncVar(hook = nameof(OnIsFrozenChanged))] private bool isFrozen;
    #endregion
    #region Health:
    [SyncVar(hook = nameof(OnHealthChanged))] private byte health;
    public byte Health
    {
        get { return health; }
    }
    [SerializeField] private TMPro.TextMeshPro healthGUI;
    //[SyncVar] private bool isAlive;//TODO: Is this noit overkill??
    #endregion
    [SerializeField] private CharacterCreation.NetworkedCharacterSkin skin;
    [SerializeField] private GameObject placeholderGraphics;
    public static PlayerController localPlayerController;
    public static List<PlayerController> allPlayers = new List<PlayerController>();


    private void Start()
    {
        myTransform = transform;

        if (isServer)
        {
            Server_Initialise();
        }

        if (isLocalPlayer)
        {
            localPlayerController = this;

            pathCache = new NavMeshPath();
            pathWayPoints = new Vector3[16];
            joystick  = FindObjectOfType<Joystick>();
            tagButton = GameObject.Find("TagButton");
            tagButton.SetActive(false);
            CharacterCamera characterCamera = FindObjectOfType<CharacterCamera>();
            characterCamera.Initialise(myTransform, cameraAnchor.localPosition, cameraAnchor.rotation);
            camera = characterCamera.GetComponent<Camera>();

            //Cmd_SetTagger(GameManager.Tagger);
        }
        /* else
        {
              hashTag.SetActive(tagger);
        }*/
        hashTag.SetActive(false);
        healthGUI.color = Color.green;

        if (skin != null)
        {
           Destroy( placeholderGraphics);
            skin.Initialise();
        }
    }

    [Server]
    private void Server_Initialise()
    {

        serverData = new PlayerServerData();
        AddPlayer(this);
        //SetTagger(allPlayers.Count == 1);
        health = PlayerServerData.MAX_HEALTH;
        maxMovementSpeed = PlayerServerData.MAX_HEALTHY_MOVEMENT_SPEED;

    }

    #region Tagging:

    /*[Command]
    private void Cmd_SetTagger(bool value)
    {
        SetTagger(value);
    }*/

    [Server]
    public void SetTagger(bool value)
    {
        tagger = value;

        if (value)
        {
            serverData.healthReductionTimer.Reset();
        }
        else
        {
            canTag = false;
        }
    }

    [Server]
    private void Freeze()
    {
        isFrozen = true;
        serverData.freezeTimer.Start(3);//HARDCODED
    }

    [Client]
    private void OnTaggerChange(bool oldValue, bool newValue)
    { 
        hashTag.SetActive(newValue);
    }

    [Client]
    private void OnCanTagChange(bool oldValue, bool newValue)
    {
        if (isLocalPlayer)
        {
            tagButton.SetActive(newValue);
        }
    }
    
    [ClientRpc]
    public void Rpc_Win()
    {
        healthGUI.color = Color.yellow;
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

            canTag = playersInRange.Count > 0;
        }     
    }

    #region Health
    [Server]
    private void HandleTaggerHealthReduction(float deltaTime)
    {
        if (serverData.healthReductionTimer.Update(deltaTime))
        {
            health -= 1;
            if(!IsAlive)
            {
                OnDeath();
            }
        }
    }

    [Client]
    private void OnHealthChanged (byte oldHealth, byte newHealth)
    {
        healthGUI.text = newHealth.ToString();
    }

    public bool IsAlive
    {
        get
        {
            return health > 0;
        }
    }

    [Server]
    private void OnDeath()
    {
        SetTagger(false);
        GameManager.UpdatePlayersState();
        Rpc_OnDeath();
    }

    [ClientRpc]
    private void Rpc_OnDeath()
    {
        healthGUI.color = Color.red;
    }
    #endregion
    [Client]
    private void OnIsFrozenChanged(bool oldValue, bool newValue)
    {
        healthGUI.color = (newValue ? Color.blue : Color.green);
    }

    [Client]
    public void TryTag()
    {
        Debug.Log("tryTag()");
        CmdTryTag();
    }

    [Command]
    private void CmdTryTag()
    {
        //TODO: In case there's more than one players, pick a player based on distance or something.
        //
        if (!tagger)
        {
            Debug.LogWarning("A non-tagger is trying to tag!");
        }
        if (serverData.playersInRange.Count == 0)
        {
            Debug.LogWarning("Can't tag, no players in range");
            return;
        }

        PlayerController nextTagger = serverData.playersInRange[0];
        nextTagger.Freeze();
        nextTagger.SetTagger(true);
        SetTagger(false);
    }
    #endregion

    private void Update()
    {
        if (isLocalPlayer)
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
        }
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        GameStates gameState =  GameManager.State;
        if (gameState == GameStates.Waiting || gameState == GameStates.TagGame)
        {
            if (isLocalPlayer)
            {
                HandleMovement(ref deltaTime);
                //TODO: That's alot of method calls bruh
                /*bool showTagButton = (tagger && canTag);
                tagButton.SetActive(showTagButton);*/
            }
            if (isServer) //Lots of network activity
            {
                if (tagger)
                {
                    if (isFrozen)
                    {
                        bool unFreeze = serverData.freezeTimer.Update(deltaTime);
                        if (unFreeze)
                        {
                            isFrozen = false;
                        }
                    }
                    else if (IsAlive) //TODO: Move up maybe?
                    {
                        HandleNearbyPlayersDetection(deltaTime);
                        HandleTaggerHealthReduction(deltaTime);

                    }
                }
                if (serverData.trapTimer.IsActive)
                {
                    bool unTrap = serverData.trapTimer.Update(deltaTime);
                    if (unTrap)
                    {
                        maxMovementSpeed = PlayerServerData.MAX_HEALTHY_MOVEMENT_SPEED;
                    }
                }
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
        //TODO: This area of th code must be revisited, it is probably outdated 
        float verticalInput = joystick.Vertical;
        float horizontalInput = joystick.Horizontal;
        Vector3 movement = new Vector3();
        //Debug.Log($"Vertical: {verticalInput.ToString("f2")}  + Horizontal: {horizontalInput.ToString("f2")}");

        if ( (!isFrozen) && (verticalInput != 0 || horizontalInput != 0))
        {
            Vector3 inputVector3 = new Vector3(horizontalInput, 0, verticalInput);
            desiredRotation = Quaternion.LookRotation(inputVector3);

            movement = inputVector3 * maxMovementSpeed;// * deltaTime;
            DeactivateNavMesh();
           /* if (!isWalking)
            {
                animator.SetBool("IsWalking",true);
                isWalking = true;
            }*/
            //Debug.Log($"movement magnitude: {movement.magnitude}"); Magnitude seems good!
        }

        #region Gravity
        /* if (navMeshAgent..isGrounded)
         {
             velocity.y = 0;
         }
         else
         {
             velocity.y += gravity * deltaTime;
         }*/
        #endregion

        if (PathFindingIsActive)
        {
            Vector3 difference = (currentWayPoint - myTransform.position);
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

                movement = difference.normalized * maxMovementSpeed;// * deltaTime;
                velocity.x = movement.x;
                velocity.z = movement.z;

                difference.y = 0;
                desiredRotation = Quaternion.LookRotation(difference);
            }
        }

        velocity.x = movement.x;
        velocity.z = movement.z;

        float velocitySquaredMagnitude = velocity.sqrMagnitude;
        float maxVelocitySquaredMagnitude = maxMovementSpeed * maxMovementSpeed;//TODO: Pre-Calculate
        float velocityPercentage = (velocitySquaredMagnitude / maxVelocitySquaredMagnitude);

        if(velocityPercentage < 0.15f)//HARDCODED
        {
            velocity.x = 0;
            velocity.z = 0;
            velocityPercentage = 0;
        }
        //Debug.Log($"velocitySquaredMagnitude: {velocitySquaredMagnitude.ToString("f2")}");
        if(animator != null)
        {
            animator.SetFloat("Speed", velocityPercentage);
        }

        characterController.Move(velocity * deltaTime);

        myTransform.rotation = Quaternion.RotateTowards
            (myTransform.rotation, desiredRotation, maxRotationSpeed * deltaTime);
        // Debug.Log($"navMeshVelocity: {navMeshAgent.velocity}");
    }

    #region Interactables:
    private Interactable FindInteractable()
    {
        Interactable interactable = null;
        Vector2 mousePosition = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(mousePosition);
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

    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

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
            byte healthAddition = 8;//TODO: Hardcoded
            health += healthAddition;
            if (health > PlayerServerData.MAX_HEALTH)
            {
                health = PlayerServerData.MAX_HEALTH;
            }
        }
        else if(pickUp is Trap)
        {
            serverData.trapTimer.Start(PlayerServerData.TRAP_DURATION);
            maxMovementSpeed = PlayerServerData.MAX_TRAPPED_MOVEMENT_SPEED;
        }         
    }

    [Client]
    public void TryPlaceTrap()
    {
        Debug.Log("TryPlaceTrap");
        Cmd_PlaceTrap();
    }

    [Command]
    private void Cmd_PlaceTrap()
    {
        Vector3 trapSpawnPosition = myTransform.position + (myTransform.forward * -1.1f);//HARDCODED
        Spawner.Spawn(Spawnables.Trap, trapSpawnPosition);
    }

    [TargetRpc]
    public void TargetRpc_Teleport(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        myTransform.position = position;
        myTransform.rotation = rotation;
        characterController.enabled = true;

    }

    [Server]
    private void OnDestroy()
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

}