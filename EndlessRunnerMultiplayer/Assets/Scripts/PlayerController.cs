using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    private Animator animator;
    private Camera mainCamera;
    private TextMesh nameLabel;
    private CustomNetworkManager networkManager;

    const float RUNNING_SPEED = 10.0f;
    const float DISTANCE_TO_SPAWN_SECTION = 50f;

    Vector3 lastEndPosition;

    GameObject Spawn;
    GameObject Spawn2;

    private bool run = false;
    private bool is_jumping = false;
    private bool db_jump = false;
    private bool easy = true;
    private bool medium = false;
    private bool hard = false;

    private enum PlayerStates { left, center, right};

    PlayerStates states;

    float COUNTDOWN_DIFFICULTY_CHANGE = 20f;

    //int TELEPORT_AVAILABLE = 3;

    // Name sync /////////////////////////////////////
    [SyncVar(hook = "SyncNameChanged")]
    public string playerName = "Player";

    [Command]
    void CmdChangeName(string name) { playerName = name; }

    void SyncNameChanged(string name) { nameLabel.text = name; }

    //Prefab Sync ////////////////////////////
    [Command]
    void CmdChangePlayerPrefab(int id)
    {
        networkManager.ChangePlayerPrefab(this, id);
    }

    [Command]
    void CmdAddLevelSection(int id, Vector3 pos)
    {
        networkManager.SpawnLevelSection(id, pos);
    }
    // OnGUI /////////////////////////////////////////

    private void OnGUI()
    {
        if(isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, Screen.height - 20));

            string prevPlayerName = playerName;
            playerName = GUILayout.TextField(playerName);
            if (playerName != prevPlayerName)
            {
                CmdChangeName(playerName);
            }

            GUILayout.EndArea();
            
        }else
        {
            short newIndex = (short)GUILayout.SelectionGrid(
                networkManager.playerPrefabIndex, networkManager.playerNames, 3);
            if (newIndex != networkManager.playerPrefabIndex)
            {
                networkManager.playerPrefabIndex = newIndex;
                CmdChangePlayerPrefab(newIndex);
            }
        }
    }


    // Animation sync ////////////////////////////////
    [SyncVar(hook = "OnSetAnimation")]
    string animationName;

    void setAnimation(string animName)
    {
        OnSetAnimation(animName);
        CmdSetAnimation(animName);
    }

    [Command]
    void CmdSetAnimation(string animName)
    {
        animationName = animName;
    }
    void OnSetAnimation(string animName)
    {
        if (animationName == animName) return;
        animationName = animName;

        animator.SetBool("Idling", false);
        animator.SetBool("Running", false);
        animator.ResetTrigger("Jumping");

        if (animationName == "Idling") animator.SetBool("Idling", true);
        else if (animationName == "Running") animator.SetBool("Running", true);
        else if (animationName == "Jumping") animator.SetTrigger("Jumping");

    }
    // Lifecycle methods ////////////////////////////

    // Use this for initialization
    void Start ()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        nameLabel = transform.Find("Label").gameObject.GetComponent<TextMesh>();

        NetworkManager manager = NetworkManager.singleton;
        networkManager = manager.GetComponent<CustomNetworkManager>();

        Spawn = GameObject.Find("SpawnPos");
        Spawn2 = GameObject.Find("SpawnPos2");

        //Player 1 side map
        if (Vector3.Distance(transform.position,Spawn.transform.position) < 1.0f)
        {
            lastEndPosition = new Vector3(-9.2f,0,23.7f);
            gameObject.tag = "Player1";
            states = PlayerStates.center;
            
        }
        else if(Vector3.Distance(transform.position, Spawn2.transform.position) < 1.0f)
        {
            lastEndPosition = new Vector3(8.5f, 0, 24);
            gameObject.tag = "Player2";
            states = PlayerStates.center;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        {
            Vector3 translation = new Vector3();
            Rigidbody rb;
            rb = GetComponent<Rigidbody>();
            float verticalAxis = 0.5f;

            //start running (temp)
            if (Input.GetKeyDown("r"))
            {
                run = true;
            }
            if(Input.GetKeyDown("a") && states != PlayerStates.left)
            {
                if(states == PlayerStates.center)
                {
                    transform.Translate(new Vector3(-1.3f, 0, 0));
                    states = PlayerStates.left;
                }
                else if(states == PlayerStates.right)
                {
                    transform.Translate(new Vector3(-1.3f, 0, 0));
                    states = PlayerStates.center;
                }
            }
            if (Input.GetKeyDown("d") && states != PlayerStates.right)
            {
                if (states == PlayerStates.center)
                {
                    transform.Translate(new Vector3(1.3f, 0, 0));
                    states = PlayerStates.right;
                }
                else if (states == PlayerStates.left)
                {
                    transform.Translate(new Vector3(1.3f, 0, 0));
                    states = PlayerStates.center;
                }
            }
            //increase difficulty
            if (run && !hard)
            {
                COUNTDOWN_DIFFICULTY_CHANGE -= Time.deltaTime;

                if(COUNTDOWN_DIFFICULTY_CHANGE <= 0.0f)
                {
                    if(easy)
                    {
                        easy = false;
                        medium = true;
                        COUNTDOWN_DIFFICULTY_CHANGE += 20f;
                    }
                    else if(medium)
                    {
                        medium = false;
                        hard = true;
                    }
                }
            }
            if(hard)
            {
                verticalAxis = 0.8f;
            }
            if (verticalAxis > 0.0 && run)
            {
                setAnimation("Running");
                translation += new Vector3(0.0f, 0.0f, verticalAxis * RUNNING_SPEED * Time.deltaTime);
                transform.Translate(translation);
            }
            else
            {
                setAnimation("Idling");
            }
            //jump and double jump
            if (Input.GetButtonDown("Jump"))
            {
                if(!is_jumping && !db_jump)
                {
                    rb.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
                    setAnimation("Jumping");

                    is_jumping = true;
                }
                else if(is_jumping && !db_jump)
                {
                    rb.AddForce(new Vector3(0, 7, 0), ForceMode.Impulse);
                    setAnimation("Jumping");

                    db_jump = true;
                }
            }
            if(Input.GetKeyDown("s"))
            {
                if(db_jump)
                {
                    rb.velocity = Vector3.down * 4f;
                }
            }
            //if (Input.GetButtonDown("Fire1"))
            //{
            //    if(TELEPORT_AVAILABLE > 0)
            //    {
            //        transform.Translate(new Vector3(0,0,-10));
            //        TELEPORT_AVAILABLE--;
            //    }
            //}
            if (Vector3.Distance(GetComponent<Transform>().position, lastEndPosition) < DISTANCE_TO_SPAWN_SECTION)
            {
                //easy sections
                if(easy)
                {
                    CmdAddLevelSection(Random.Range(4, 7), lastEndPosition);
                }
                else if(medium || hard)
                {
                    CmdAddLevelSection(Random.Range(7, networkManager.spawnPrefabs.Count-1), lastEndPosition);
                }
                
                lastEndPosition += new Vector3(0,0,30);
            }

        }

        if (mainCamera)
        {
            if(isLocalPlayer)
            {
                mainCamera.transform.SetPositionAndRotation(transform.position + new Vector3(0.0f, 6.0f, -5.0f), Quaternion.identity);
                mainCamera.transform.LookAt(transform.position + new Vector3(0.0f, 2.0f, 0.0f), Vector3.up);
            }
          
        }

        if (nameLabel)
        {
            nameLabel.transform.rotation = Quaternion.identity;
        }
    }

    [Command]
    void CmdAddObstacle(int id, GameObject enemy)
    {
        networkManager.AddObstacleForEnemy(id, enemy);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Obstacle")
        {
            is_jumping = false;
            db_jump = false;
        }

        if(collision.gameObject.tag == "PickUp")
        {
            Destroy(collision.gameObject);

            //Add obstacle for rival
            if(gameObject.tag == "Player1")
            {
                GameObject enemy = GameObject.FindGameObjectWithTag("Player2");
                CmdAddObstacle(10, enemy);
            }
            else if(gameObject.tag == "Player2")
            {
                GameObject enemy = GameObject.FindGameObjectWithTag("Player1");
                CmdAddObstacle(10, enemy);
            }

        }
    }
    

    private void OnDestroy()
    {
    }
}
