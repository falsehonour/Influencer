using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private class PlayerServerData
    {
        public const float MAX_HEALTHY_MOVEMENT_SPEED = 5.5f;
        public const float MAX_TRAPPED_MOVEMENT_SPEED = 2.2f;

        public static readonly float MIN_TAG_SQUARED_DISTANCE = (2f * 2f);
        public const sbyte MAX_HEALTH = 32;
        public const float TRAP_DURATION = 3f;
        private const float HEALTH_LOSS_INTERVAL = 1f;
        private const float NEARBY_PLAYERS_DETECTION_INTERVAL = 0.05f;

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
    private Vector3 controlledVelocity;
    private Vector3 externalForces;
    private float currentGravity = 0;
    private float EXTERNAL_FORCES_REDUCTION_SPEED = 6f;
    private const float PUSH_FORCE = 6f;
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
    [SerializeField] private Transform cameraAnchor;
     private Camera camera;
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
    [SerializeField] private GameObject tagButton;
    [SyncVar(hook = nameof(OnIsFrozenChanged))] private bool isFrozen;
    #endregion
    #region Health:
    [SyncVar(hook = nameof(OnHealthChanged))] private sbyte DT_health;
    public sbyte Health
    {
        get { return DT_health; }
    }
    //[SerializeField] private TMPro.TextMeshPro healthGUI;
    //[SyncVar] private bool isAlive;//TODO: Is this noit overkill??
    #endregion
    [SerializeField] private CharacterCreation.NetworkedCharacterSkin skin;
    [SerializeField] private GameObject placeholderGraphics;
    [SerializeField] private Transform playerUIAnchor;
    private PlayerUI playerUI;
    public static PlayerController localPlayerController;
    public static List<PlayerController> allPlayers = new List<PlayerController>();



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

            pathCache = new NavMeshPath();
            pathWayPoints = new Vector3[16];
            joystick  = FindObjectOfType<Joystick>();
            tagButton = GameObject.Find("TagButton");
            tagButton.SetActive(false);
            CharacterCamera characterCamera = FindObjectOfType<CharacterCamera>();
            characterCamera.Initialise(myTransform, cameraAnchor);
            camera = characterCamera.GetComponent<Camera>();

            //Cmd_SetTagger(GameManager.Tagger);
        }

        if (skin != null)
        {
            Destroy(placeholderGraphics);
            skin.Initialise();
        }
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
        //TODO: combine with push
        isFrozen = true;
        serverData.freezeTimer.Start(3);//HARDCODED
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
        if (isLocalPlayer)
        {
            tagButton.SetActive(newValue);
        }
    }
    
    [ClientRpc]
    public void Rpc_Win()
    {
        char character = 'V';
        playerUI.SetCharacter(character);
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
            //TODO: No need for this clear, we use an array instead
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
        Rpc_OnDeath();
    }

    [ClientRpc]
    private void Rpc_OnDeath()
    {
        //healthGUI.color = Color.red;
        char character = 'X';
        playerUI.SetCharacter(character);
        playerUI.gameObject.SetActive(false);

    }

    #endregion
    [Client]
    private void OnIsFrozenChanged(bool oldValue, bool newValue)
    {
        if(hasAuthority && !newValue)
        {
            networkAnimator.SetTrigger(AnimatorParameters.Recover);
        }
        //TODO: Add indicators
       // healthGUI.color = (newValue ? Color.blue : Color.green);
    }

    [Client]
    public void TryTag()
    {
        networkAnimator.SetTrigger(AnimatorParameters.Push);
        Cmd_TryTag();
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
            nextTagger.Freeze();
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
            #region Testing:
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryShoot();
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
            #endregion
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
        if(externalForces != ZERO_VECTOR3)
        {
            externalForces = Vector3.MoveTowards(externalForces, ZERO_VECTOR3, EXTERNAL_FORCES_REDUCTION_SPEED * deltaTime);
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

            controlledVelocity = inputVector3 * maxMovementSpeed;// * deltaTime;
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

        float velocitySquaredMagnitude = controlledVelocity.sqrMagnitude;
        float maxVelocitySquaredMagnitude = maxMovementSpeed * maxMovementSpeed;//TODO: Pre-Calculate
        float velocityPercentage = (velocitySquaredMagnitude / maxVelocitySquaredMagnitude);

        if(velocityPercentage < 0.1f)//HARDCODED
        {
            //Stay in place if input is negligible
            controlledVelocity = Vector3.zero;
            velocityPercentage = 0;
        } 
        //Debug.Log($"velocitySquaredMagnitude: {velocitySquaredMagnitude.ToString("f2")}");
        if(animator != null)
        {
            //TODO: do this in bigger intervals for performance
            animator.SetFloat(AnimatorParameters.Speed, velocityPercentage);
        }

        Vector3 velocity = (controlledVelocity + externalForces);
        velocity.y += currentGravity;
        characterController.Move(velocity * deltaTime);
        /*characterController.enabled = false;
        myTransform.position += velocity * deltaTime;*/
        myTransform.rotation = Quaternion.RotateTowards
            (myTransform.rotation, desiredRotation, maxRotationSpeed * deltaTime);
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
            sbyte healthAddition = 8;//TODO: Hardcoded
            ModifyHealth(healthAddition);
        }
        else if(pickUp is Trap)
        {
            serverData.trapTimer.Start(PlayerServerData.TRAP_DURATION);
            maxMovementSpeed = PlayerServerData.MAX_TRAPPED_MOVEMENT_SPEED;
        }         
    }

    #region Shooting:
    [Client]
    public void TryShoot()
    {
        //Debug.Log("TryShoot");
        Cmd_TryShoot(myTransform.position, myTransform.rotation);
    }

    [Command]
    private void Cmd_TryShoot(Vector3 clientPlayerPosition, Quaternion clientRotation)
    {
        networkAnimator.SetTrigger(AnimatorParameters.Slap);
        Vector3 bulletSpawnPosition = 
            (clientPlayerPosition + (myTransform.forward * 1f) +  (Vector3.up * 0.4f)) ;//HARDCODED
        Quaternion bulletSpawnRotation = clientRotation;
        Spawner.Spawn(Spawnables.Bullet, bulletSpawnPosition, bulletSpawnRotation);
    }

    [Server]
    public void OnBulletHit()
    {
        ModifyHealth(-1); 
    }

    #endregion
    [Client]
    public void TryPlaceTrap()
    {
        Debug.Log("TryPlaceTrap");
        Cmd_TryPlaceTrap();
    }

    [Command]
    private void Cmd_TryPlaceTrap()
    {
        Vector3 trapSpawnPosition = myTransform.position + (myTransform.forward * -1.1f);//HARDCODED
        Spawner.Spawn(Spawnables.Trap, trapSpawnPosition, Quaternion.identity);
    }

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
    #endregion
}