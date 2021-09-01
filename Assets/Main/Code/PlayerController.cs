using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

namespace HashtagChampion
{
    public class PlayerController : NetworkBehaviour
    {
        public class ServerData
        {
            public static readonly float INJURED_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(10.5f);
            public static readonly float NONTAGGER_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(19.8f);
            public static readonly float SPRINT_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(28.8f);
            //public const float ROTATION_SPEED = 1080f;

            public const float PUSH_FORCE = 7.5f;
            public const float SLIP_FORCE = 6f;

            public const sbyte MAX_HEALTH = 100;
            private const float TAGGER_FULL_LIFETIME = 40f;
            public const float TAGGER_HEALTH_LOSS_INTERVAL = TAGGER_FULL_LIFETIME / (float)MAX_HEALTH;
            public const float TAGGER_FREEZE_DURATION = 3.5f;
            public const float TAG_DURATION = 0.32f;
            //TODO: Is this the proper place for things like power up properties??
            public const float SPRINT_DURATION = 2f;
            public const sbyte HEALTH_PICK_UP_BONUS = 16;
            public const float FOOTBALL_INJURY_DURATION = 4f;
            public const float SLIP_FREEZE_DURATION = 2.25f;

            public float taggerSpeed;

            public RepeatingTimer healthReductionTimer;
            public RepeatingTimer nearbyPlayersDetectionTimer;
            public SingleCycleTimer movementStateTimer;
            public MovementStates nextMovementState;

            public List<PlayerController> playersInTagBounds = new List<PlayerController>();
            public List<PlayerController> playersInExtendedTagBounds = new List<PlayerController>();

            public ServerData()
            {
                healthReductionTimer = new RepeatingTimer(TAGGER_HEALTH_LOSS_INTERVAL);
                movementStateTimer = new SingleCycleTimer();
            }

            public void UpdateTaggerSpeed(float taggerSpeedBoostInKilometresPerHour)
            {
                taggerSpeed = (NONTAGGER_SPEED + 
                    UnitConvertor.KilometresPerHourToMetresPerSecond(taggerSpeedBoostInKilometresPerHour));
            }

        }
        //NOTE: This object should exist only on the server.
        private ServerData serverData;
        public enum MovementStates : byte
        {
            Frozen, Injured, Normal, Sprinting
        }
        private readonly static float MIN_MOVEMENT_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(1f);
        private readonly static float MIN_RUN_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(7.8f);
        private readonly static float MIN_SPRINT_SPEED = UnitConvertor.KilometresPerHourToMetresPerSecond(22f);
        public enum GaitTypes : byte
        {
            Standing = 0, Walking = 1, Running = 2, Sprinting = 3
        }
        private GaitTypes gait;
        [SerializeField] private CharacterController characterController;
        private Transform myTransform;
        private static Joystick joystick;
        [SyncVar] private Vector2 forcedMovementInput = Vector3.zero;//TODO: Remove this once testing is over
        [SyncVar(hook = nameof(OnMovementStateChange))] private MovementStates X_movementState;
        private MovementStates MovementState => X_movementState;
        [SyncVar] private float currentMaxMovementSpeed;
        public const float MAX_ROTATION_SPEED = 1080f;
        private Quaternion desiredRotation;
        private Vector3 controlledVelocity;
        private Vector3 externalForces;
        private float currentGravity = 0;
        //TODO: These three don't belong here
        private float EXTERNAL_FORCES_REDUCTION_SPEED = 6f;
        [SerializeField] private float gravity = -9.8f;
        private static readonly Vector3 ZERO_VECTOR3 = Vector3.zero;
        private CharacterCamera characterCamera;
        private Animator animator;
        private NetworkAnimator networkAnimator;
        [SerializeField] private BoxCollider tagBoundsCollider;
        [SerializeField] private BoxCollider extendedTagBoundsCollider;
        #region Tagging
        [SyncVar(hook = nameof(OnTaggerChange))] private bool tagger;
        public bool Tagger
        {
            get { return tagger; }
        }

        private SingleCycleTimer tagCooldownTimer;
        private const float TAG_COOLDOWN_INTERVAL = 0.6f;
        private PlayerInputButton tagButton;
        #endregion
        #region Health:
        [SyncVar(hook = nameof(OnHealthChanged))] private sbyte X_health = ServerData.MAX_HEALTH;
        public sbyte Health
        {
            get { return X_health; }
        }
        //[SyncVar] private bool isAlive;//TODO: Is this noit overkill??
        #endregion
        [SerializeField] private CharacterCreation.NetworkedCharacterSkin skin;
        //private CharacterCreation.Character skinCharacter;
        [SerializeField] private GameObject placeholderGraphics;
        [SerializeField] private Transform playerUIAnchor;
        private PlayerUI playerUI;
        //NOTE: Does it matter wher we place our sync vars in code??
        [SyncVar(hook =nameof(OnNameChanged))] private string displayName;
        //private bool isLocalPlayerCache;
        #region PowerUps:
        private SingleCycleTimer powerUpCooldownTimer;
        private const float POWER_UP_COOLDOWN_INTERVAL = 0.2f;
        [SyncVar(hook = nameof(OnPowerUpChanged))] private PowerUp powerUp;
        private PowerUpButton powerUpButton;
        [SerializeField] private GameObject football;
        #endregion
        private bool initialised = false;
        public static PlayerController localPlayerController;
        //public static List<PlayerController> allPlayers;
        private MatchGameManager matchGameManager;
        private Player player;

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        private void Start()
        {
            StartCoroutine(InitialisationRoutine());
        }

        private IEnumerator InitialisationRoutine()
        {
            //Wait for GameScene
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.15f);
            while(!GameSceneManager.initialised)
            {
                yield return waitForSeconds;
                Debug.Log("Waiting for game scene...");
            }

            myTransform = transform;

            #region UI Initialisation:
            //NOTE: We (used to do)  doing this in Awake in order to avoid hook shinanigans
            playerUI = PlayerUIManager.CreatePlayerUI(playerUIAnchor);
            char character = ' ';
            playerUI.SetProceedingCharacter(character);
            #endregion

            if (isServer)
            {
                Server_Initialise();
            }
            if (hasAuthority)
            {
                localPlayerController = this;

                //TODO: Remove path finding stuff if it's not gonna be used after all
                tagButton = GameObject.Find("TagButton").GetComponent<PlayerInputButton>();
                tagButton.SetIsEnabled(false);

                powerUpButton = GameObject.Find("PowerUpButton").GetComponent<PowerUpButton>();
                UpdatePowerUpButton();

                characterCamera = FindObjectOfType<CharacterCamera>();
                characterCamera.Initialise(myTransform/*, cameraAnchor*/);

                Cmd_SetName(StaticData.playerName.name);

                Transform spawnPoint = GameSceneManager.GetReferences().playerSpawnPoint;
                Teleport(spawnPoint.position, spawnPoint.rotation);
            }

            football.SetActive(false);
            if (skin != null)
            {
                Destroy(placeholderGraphics);
                skin.Initialise();
                //skinCharacter = skin.character;
            }
            initialised = true;
            PerformPostInitialisation();
            //Note: the reason we set it here is in order to perform sync var hook functions. Does this leave us open for bugs??

            /* if(characterController.attachedRigidbody != null)
             {
                 characterController.attachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
             }*/
        }

        private void PerformPostInitialisation()
        {
            if (!isServer)
            {
                PerformSyncVarHookFunctions();
            }
            else
            {
                matchGameManager.UpdatePlayersState();
            }
        }

        private void PerformSyncVarHookFunctions()
        {
            /* TODO: We are calling these manually to ensure things had been initialised. 
            Note that these functions also have initialistaion checks because they are called when the object is created.
            Find a more fitting solution  */
            OnHealthChanged(0, X_health);
            OnNameChanged(null, displayName);
            OnPowerUpChanged(powerUp, powerUp);
            OnTaggerChange(false, tagger);
        }

        public static void SetActiveJoystick(Joystick activeJoystick)
        {
           joystick = activeJoystick;
        }

        #region PlayerName
        //NOTE: Did this work improperly cause it was placed here???
        /*[SyncVar] private string displayName;
        [SyncVar] private int displayNameNumber;*/

        //TODO: Remove this region, it is supposed to be temporary

        [Command]
        private void Cmd_SetName(string newName)
        {
            displayName = newName;
        }

        [Client]
        private void OnNameChanged(string oldValue, string newValue)
        {
            if (!initialised)
            {
                return;
            }
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
            serverData = new ServerData();
            matchGameManager = player.CurrentMatchData.manager;
            serverData.UpdateTaggerSpeed(matchGameManager.Match.settings.taggerSpeedBoostInKilometresPerHour);
            X_health = ServerData.MAX_HEALTH;
            PowerUp initialPowerUp = new PowerUp { count = 0, type = PowerUp.Type.None };
            this.powerUp = initialPowerUp;
            SetMovementState(MovementStates.Normal);
        }

        [Server]
        private void SetMovementState(MovementStates newState)
        {
            X_movementState = newState;
            switch (newState)
            {
                case MovementStates.Frozen:
                    {
                        currentMaxMovementSpeed = 0;
                        break;
                    }
                case MovementStates.Injured:
                    {
                        currentMaxMovementSpeed = ServerData.INJURED_SPEED;
                        break;
                    }
                case MovementStates.Normal:
                    {
                        currentMaxMovementSpeed = tagger ? serverData.taggerSpeed : ServerData.NONTAGGER_SPEED;
                        break;
                    }
                case MovementStates.Sprinting:
                    {
                        currentMaxMovementSpeed = ServerData.SPRINT_SPEED;
                        break;
                    }
            }
        }

        [Client]
        private void OnMovementStateChange(MovementStates oldValue, MovementStates newValue)
        {
            if (!initialised)
            {
                return;
            }
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
                            tagButton.SetIsEnabled(true);
                        }
                    }
                    else
                    {
                        if (tagger)
                        {
                            tagButton.SetIsEnabled(false);
                        }
                    }
                    UpdatePowerUpButton();
                    characterCamera.distanceMultiplier = newValue == MovementStates.Frozen ? 0.7f : 1;//HARDCODEDDD
                }
            }

            if (oldValue == MovementStates.Sprinting && powerUp.type != PowerUp.Type.Sprint)
            {
                skin.character.ShowWings(false);
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
            TargetRpc_OnPush();
            pushed.Freeze(freezeDuration);
            pushed.TargetRpc_OnPushed(pushForce);
            pushed.Rpc_OnPushed();
        }

        [TargetRpc]
        private void TargetRpc_OnPush()
        {
            Audience.React(Audience.ReactionTypes.MinorApplause);
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

            Audience.React(Audience.ReactionTypes.MinorDisappointment);
        }

        [ClientRpc]
        private void Rpc_OnPushed()
        {
            //TODO: summon an explosion particle effect that will handle the sound as well
            SoundManager.PlayOneShotSound(SoundNames.PushHit, myTransform.position);
        }


        #region Tagging:

        [Server]
        public void SetTagger(bool value, float healthReductionFirstIteration = ServerData.TAGGER_HEALTH_LOSS_INTERVAL)
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

        [ClientRpc]
        private void Rpc_OnTagged()
        {
            SoundManager.PlayOneShotSound(SoundNames.Tagged);
        }

        [Client]
        private void OnTaggerChange(bool oldValue, bool newValue)
        {
            if (!initialised)
            {
                return;
            }
            char character = newValue ? '#' : ' ';
            playerUI.SetProceedingCharacter(character);
            if (localPlayerController == this)
            {
                tagButton.SetIsEnabled(newValue);
            }
        }

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
            if (CanTryTag())
            {
                StartCoroutine(TagCoroutine());
                tagCooldownTimer.Start(TAG_COOLDOWN_INTERVAL);
            }
            else
            {
                Debug.LogWarning("Cannot Try Tag! A player Tried to tag illegally.");
            }
        }

        private IEnumerator TagCoroutine()
        {
            float endTime = Time.time + ServerData.TAG_DURATION;

            serverData.playersInTagBounds.Clear();
            serverData.playersInExtendedTagBounds.Clear();

            bool taggedSomeone = false;
           //playerUI.SetPlayerName("Tagging");
            while (Time.time < endTime)
            {
                DetectPlayersInTagBounds();
                int playersInTagBoundsCount = serverData.playersInTagBounds.Count;

                if (playersInTagBoundsCount > 0)
                {
                    PlayerController nextTagger = null; // = serverData.playersInRange[0];

                    if (playersInTagBoundsCount == 1)
                    {
                        nextTagger = serverData.playersInTagBounds[0];
                    }
                    else
                    {
                        Vector3 myPosition = this.myTransform.position;
                        float smallestSquaredDistance = float.MaxValue;
                        for (int i = 0; i < playersInTagBoundsCount; i++)
                        {
                            PlayerController player = serverData.playersInTagBounds[i];
                            float squaredDistance = Vector3.SqrMagnitude(myPosition - player.myTransform.position);
                            if (squaredDistance < smallestSquaredDistance)
                            {
                                nextTagger = player;
                            }
                        }
                    }

                    if (serverData.playersInExtendedTagBounds.Contains(nextTagger))
                    {
                        serverData.playersInExtendedTagBounds.Remove(nextTagger);
                    }

                    //TODO: Croud shocked for untagged players
                    Rpc_OnTagged();
                    nextTagger.SetTagger(true, ServerData.TAGGER_FREEZE_DURATION);
                    Vector3 pushForce = myTransform.forward * ServerData.PUSH_FORCE;
                    Push(nextTagger, pushForce, ServerData.TAGGER_FREEZE_DURATION);
                    SetTagger(false);
                    taggedSomeone = true;
                    break;
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
           // playerUI.SetPlayerName("");

            int playersInExtendedTagBoundsCount = serverData.playersInExtendedTagBounds.Count;
            if (playersInExtendedTagBoundsCount > 0)
            {
                if (!taggedSomeone)
                {
                    TargetRpc_OnOtherAlmostTagged();
                }

                for (int i = 0; i < playersInExtendedTagBoundsCount; i++)
                {
                    serverData.playersInExtendedTagBounds[i].TargetRpc_OnSelfAlmostTagged();
                }
            }
        }

        [TargetRpc]
        private void TargetRpc_OnSelfAlmostTagged()
        {
            int randomiser = Random.Range(0, 2);
            Audience.ReactionTypes audienceReaction = randomiser == 0 ? Audience.ReactionTypes.NeutralSurprise : Audience.ReactionTypes.PositiveSurprise;
            Audience.React(audienceReaction);
        }

        [TargetRpc]
        private void TargetRpc_OnOtherAlmostTagged()
        {
            int randomiser = Random.Range(0, 2);
            Audience.ReactionTypes audienceReaction = randomiser == 0 ? Audience.ReactionTypes.NeutralSurprise : Audience.ReactionTypes.NegativeSurprise;
            Audience.React(audienceReaction);
        }

        [Server]
        private void DetectPlayersInTagBounds()
        {
            //TODO: Do these things in a unified method, cache all players locations
            /*Bounds tagBounds = tagBoundsCollider.bounds;
             Bounds extendedTagBounds = extendedTagBoundsCollider.bounds;//; tagBounds;

             tagBoundsCollider.po*/
            //  extendedTagBounds.Expand(new Vector3(0, 0, 1));//HARDCODED
            //bool playerFound = false;
            PlayerController[] players = matchGameManager.relevantPlayerControllers;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController otherPlayer = players[i];
                if (otherPlayer != null && otherPlayer != this && otherPlayer.IsAlive())
                {
                    Vector3 otherPlayerPosition = otherPlayer.myTransform.position;
                    bool otherPlayerIsInExtendedBounds =
                       extendedTagBoundsCollider.ClosestPoint(otherPlayerPosition) == otherPlayerPosition;
                    if (otherPlayerIsInExtendedBounds)
                    {
                        bool otherPlayerIsInBounds =
                            tagBoundsCollider.ClosestPoint(otherPlayerPosition) == otherPlayerPosition;
                        if (otherPlayerIsInBounds)
                        {
                            serverData.playersInTagBounds.Add(otherPlayer);
                        }
                        else if (!serverData.playersInExtendedTagBounds.Contains(otherPlayer))
                        {
                            serverData.playersInExtendedTagBounds.Add(otherPlayer);
                        }
                    }
                }
            }
        }

        #region Health
        [Server]
        private void ModifyHealth(sbyte by)
        {
            sbyte health = X_health;
            health += by;
            if (health < 0)
            {
                health = 0;
            }
            else if (health > ServerData.MAX_HEALTH)
            {
                health = ServerData.MAX_HEALTH;
            }

            X_health = health;

            if (!IsAlive())
            {
                Lose();
                matchGameManager.UpdatePlayersState();
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
            if (!initialised)
            {
                return;
            }
            //TODO: maybe MAX_HEALTH should not reside in PlayerServerData,.,.
            playerUI.SetHealthBarFill(newHealth);
        }

        public bool IsAlive()
        {
            return (Health > 0);
        }

        #endregion

        #endregion

        private void Update()
        {
            if (!initialised)
            {
                return;
            }
            if (localPlayerController == this)
            {
                #region Testing:
                // TODO: Remove these cheats
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    powerUpButton.SimulatePressFor(0.3f);
                    TryUsePowerUp();
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    tagButton.SimulatePressFor(0.4f);
                    TryTag();
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    Cmd_HealMe();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Cmd_LeaveMatch();
                }
                #endregion
            }
            else if (isServer)
            {//TODO: Remove
                if (false)
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

                    currentForcedMovementInput = Vector2.ClampMagnitude(currentForcedMovementInput, 1);

                    if (currentForcedMovementInput != forcedMovementInput)
                    {
                        forcedMovementInput = currentForcedMovementInput;
                        Debug.Log("forcedMovementInput Changed");
                    }
                }
                
            }

        }


        private void FixedUpdate()
        {
            if (!initialised)
            {
                return;
            }
            float deltaTime = Time.fixedDeltaTime;
            //TODO: Remove the check once it's made sure that there is always a matchGameManager
            GameStates gameState = matchGameManager ? matchGameManager.State : GameStates.Waiting;
            if (localPlayerController == this)
            {
                HandleMovement(ref deltaTime, ref gameState);
            }
            if (/*IsAlive() && */(gameState == GameStates.Waiting || gameState == GameStates.TagGame))
            {

                if (localPlayerController == this || isServer)
                {
                    if (IsAlive())
                    {
                        if (powerUpCooldownTimer.IsActive())
                        {
                            powerUpCooldownTimer.Update(deltaTime);
                        }
                        if (tagCooldownTimer.IsActive())
                        {
                            tagCooldownTimer.Update(deltaTime);
                        }
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

        }

        private void HandleMovement(ref float deltaTime, ref GameStates gameState)
        {
            if (externalForces != ZERO_VECTOR3)
            {
                externalForces = Vector3.MoveTowards
                    (externalForces, ZERO_VECTOR3, EXTERNAL_FORCES_REDUCTION_SPEED * deltaTime);
            }
            controlledVelocity = ZERO_VECTOR3;
            //TODO: This area of th code must be revisited, it is probably outdated 
            bool isControllable = ((gameState != GameStates.ChoosingTagger) && (gameState != GameStates.PostGame) 
                && IsAlive() && (MovementState != MovementStates.Frozen));
            if (isControllable)
            {
                float verticalInput = joystick.Vertical + forcedMovementInput.y;
                float horizontalInput = joystick.Horizontal + forcedMovementInput.x;

                if ((verticalInput != 0 || horizontalInput != 0))
                {
                    Vector3 inputVector3 = new Vector3(horizontalInput, 0, verticalInput);
                    desiredRotation = Quaternion.LookRotation(inputVector3);
                    controlledVelocity = inputVector3 * currentMaxMovementSpeed;// * deltaTime;
                                                                                //Debug.Log($"movement magnitude: {movement.magnitude}"); Magnitude seems good!
                }
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

            {
                //TODO: Move to squaredMagnitude and skip multiplications before shipping.
                float metresPerSecond = controlledVelocity.magnitude;
                GaitTypes newGait;
                if (metresPerSecond > MIN_SPRINT_SPEED)
                    newGait = GaitTypes.Sprinting;
                else if (metresPerSecond > MIN_RUN_SPEED)
                    newGait = GaitTypes.Running;
                else if (metresPerSecond > MIN_MOVEMENT_SPEED)
                    newGait = GaitTypes.Walking;
                else
                    newGait = GaitTypes.Standing;

                if (newGait != gait)
                {
                    gait = newGait;
                    SetGait(newGait);
                    if (animator != null)
                    {
                        int animationSpeedState = (int)gait;
                        animator.SetInteger(AnimatorParameters.Gait, animationSpeedState);

                        //NOTE: Why the FUCK does mirror not sync the the top three???
                        //I think I understand: Our NetworkAnimator only cares about parameters that used to be on our original animator. 
                        //This means that further bugs may occur due to the animator swap we do 
                        /*animator.SetInteger(AnimatorParameters.SpeedState, animationSpeedState);
                        animator.SetInteger("WhatTheHellInt", animationSpeedState);
                        animator.SetFloat("WhatTheHellFloat", animationSpeedState);
                        animator.SetFloat(AnimatorParameters.Speed, animationSpeedState);*/
                    }
                }
            }

            if (gait == GaitTypes.Standing)
            {
                //Stay in place if input is negligible
                controlledVelocity = Vector3.zero;
            }

            Vector3 totalVelocity = (controlledVelocity + externalForces);
            totalVelocity.y += currentGravity;
            characterController.Move(totalVelocity * deltaTime);

            /*characterController.enabled = false;
            myTransform.position += velocity * deltaTime;*/
            //NOTE: Might interfere with RotateRoutine
            myTransform.rotation = Quaternion.RotateTowards
                (myTransform.rotation, desiredRotation, MAX_ROTATION_SPEED * deltaTime);
        }

        [Command]
        private void SetGait(GaitTypes newGait)
        {
            //NOTE: The reason we don't use a sync var is cause syncvars can't be set on clients
            this.gait = newGait;
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
            if (pickUp is HealthPickUp)
            {
                ModifyHealth(ServerData.HEALTH_PICK_UP_BONUS);
            }
            else if (pickUp is PowerUpPickUp)
            {
                PowerUpPickUp powerUpPickUp = (PowerUpPickUp)pickUp;
                powerUp = powerUpPickUp.GetPowerUp();
            }

            TargetRpc_OnCollect(pickUp.netId);
        }

        [TargetRpc]
        private void TargetRpc_OnCollect(uint pickUpNetID)
        {
            NetworkIdentity pickUpNetworkIdentity = NetworkIdentity.spawned[pickUpNetID];
            PickUp pickUp = pickUpNetworkIdentity.GetComponent< PickUp >();
            SoundNames additionalSound = SoundNames.None;
            if (pickUp is HealthPickUp)
            {
                additionalSound = SoundNames.HealthCollect;
                SoundManager.PlayOneShotSound(SoundNames.HealthCollect, null);
            }
            else if (pickUp is PowerUpPickUp)
            {
                PowerUp.Type powerUpType = ((PowerUpPickUp)pickUp).GetPowerUp().type;
                additionalSound = PowerUpsProperties.GetCollectionSound(powerUpType);
            }
            SoundManager.PlayOneShotSound(SoundNames.Collect, null);
            SoundManager.PlayOneShotSound(additionalSound, null);

        }


        [Server]
        private void GetInjured(float duration)
        {
            SetMovementState(MovementStates.Injured);
            serverData.movementStateTimer.Start(duration);
            serverData.nextMovementState = MovementStates.Normal;
            Rpc_GetInjured();
        }

        [ClientRpc]
        private void Rpc_GetInjured()
        {
            SoundManager.PlayOneShotSound(SoundNames.SlowedDown,myTransform.position);
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
                (PhysicalProjectile)TagNetworkManager.Spawner.Spawn(Spawnables.Bullet, bulletSpawnPosition, bulletSpawnRotation, netId);
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
            TagNetworkManager.Spawner.Spawn(Spawnables.ThrownFootball, ballSpawnPosition, ballSpawnRotation, netId);

        }

        [Server]
        public void OnFootballHit()
        {
            GetInjured(ServerData.FOOTBALL_INJURY_DURATION);
        }

        #endregion

        #region BananaThrowing:

        [Server]
        private void ThrowBanana()
        {
            Vector3 bananaPosition =
                myTransform.position + (myTransform.forward * -0.92f) + (Vector3.up * 1.8f);//HARDCODED as F%$#
            Quaternion bananaRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            TagNetworkManager.Spawner.Spawn(Spawnables.ThrownBanana, bananaPosition, bananaRotation, netId);
        }

        [Server]
        public void Slip()
        {
            //TODO: Perhaps make force dependent on the player's current velocity?
            Vector3 force = myTransform.forward * ServerData.SLIP_FORCE;
            Freeze(ServerData.SLIP_FREEZE_DURATION);//HARDCODED
            TargetRpc_OnSlip(force);
            Rpc_OnSlip();
            matchGameManager.kevin.TryLaughAt(myTransform);
        }

        [Server]
        public bool CanSlip()
        {
            //TODO: Meke it so that players have to move fast in order to slip
            //Debug.Log("Slip? : MovementState: " + MovementState.ToString() + "gait: " + gait.ToString());
            return (MovementState != MovementStates.Frozen && gait >= GaitTypes.Running);
        }

        [TargetRpc]
        private void TargetRpc_OnSlip(Vector3 force)
        {
            //TODO: merge with freeze perhaps?
            networkAnimator.SetTrigger(AnimatorParameters.FlipForward);
            externalForces += force;
            Audience.React(Audience.ReactionTypes.Laughter);
        }

        [ClientRpc]
        private void Rpc_OnSlip()
        {
            //TODO: Should be connected to an animation...
            SoundManager.PlayOneShotSound(SoundNames.BananaSlip, myTransform.position);
        }

        #endregion

        #region Sprint:

        [Server]
        private void Sprint()
        {
            SetMovementState(MovementStates.Sprinting);
            serverData.movementStateTimer.Start(ServerData.SPRINT_DURATION);
            serverData.nextMovementState = MovementStates.Normal;
        }

        #endregion
        [Client]
        private void OnPowerUpChanged(PowerUp oldValue, PowerUp newValue)
        {
            if (!initialised)
            {
                return;
            }
            if (oldValue.type != newValue.type)
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
                                skin.character.ShowWings(false);
                            }
                            break;
                        }
                    case PowerUp.Type.Gun:
                        {//TODO: Show the gun for a bit if it just shot??
                            skin.character.ShowGun(false);
                            break;
                        }
                    case PowerUp.Type.Banana:
                        {
                            skin.character.ShowBanana(false);
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
                            skin.character.ShowWings(true);
                            break;
                        }
                    case PowerUp.Type.Gun:
                        {
                            skin.character.ShowGun(true);
                            break;
                        }
                    case PowerUp.Type.Banana:
                        {
                            skin.character.ShowBanana(true);
                            break;
                        }
                }
            }

            if (localPlayerController == this)
            {
                UpdatePowerUpButton();
            }

            playerUI.SetPowerUp(newValue);

        }

        private void UpdatePowerUpButton()
        {
            powerUpButton.SetGraphics(powerUp);
            bool enableButton = (powerUp.type != PowerUp.Type.None && MovementState != MovementStates.Frozen);
            powerUpButton.SetIsEnabled(enableButton);
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
            Teleport(position, rotation);
        }

        [Client]
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            characterController.enabled = false;
            myTransform.position = position;
            myTransform.rotation = rotation;
            characterController.enabled = true;
        }

        [Server]
        public void Win()
        {
            Rpc_Win();
        }

        [ClientRpc]
        private void Rpc_Win()
        {
            if (hasAuthority)
            {
                desiredRotation = Quaternion.Euler(0, 180, 0);
                networkAnimator.SetTrigger(AnimatorParameters.Dance);
                characterCamera.distanceMultiplier = 0.8f;
            }
        }

        [Server]
        public void Lose()
        {
            if (tagger)
            {
                SetTagger(false);
            }
            matchGameManager.kevin.TryLaughAt(myTransform);
            Rpc_OnLose();
        }

        [ClientRpc]
        private void Rpc_OnLose()
        {
            if (hasAuthority)
            {
                //HARDCODED
                desiredRotation = Quaternion.Euler(0, 180, 0);
                networkAnimator.SetTrigger(AnimatorParameters.Lose);
                characterCamera.distanceMultiplier = 0.8f;
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                Server_OnDestroy();
            }
            if (playerUI)
            {
                Destroy(playerUI.gameObject);
            }
        }

        [Server]
        private void Server_OnDestroy()
        {
            matchGameManager.UpdatePlayersState();
        }

        [Command]
        private void Cmd_LeaveMatch()
        {
            player.Server_LeaveMatch();
        }

        [Command]
        private void Cmd_HealMe()
        {
            ModifyHealth(ServerData.HEALTH_PICK_UP_BONUS);
        }
    }
}