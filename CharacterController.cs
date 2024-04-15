using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviourPunCallbacks
{
    private bool isServing = false;
    public enum Direction
    {
        Left,
        Right
    }

    // reference to the GameManager script
    private GameManager gameManager;

    private Vector3 initialPosition;

    private Quaternion initialRotation;

    public Direction hitDirection;

    private Vector3 targetPosition = Vector3.zero;

    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private float ballHitRadius = 5f;
    [SerializeField]
    private GameObject tennisBallPrefab;

    private GameObject powerBar;

    //private Rigidbody rb;
    //private float rotationSpeed = 0.1f;
    private Vector3 currentMousePos;
    private Vector3 previousMousePos;

    [SerializeField]
    private float sidewaysForce = 750f;
    private bool lockArrow = false;

    [SerializeField]
    private GameObject UserInterface;
    private GameObject leftArrow;
    private GameObject rightArrow;

    // set it to -1 so it means serve button not pressed
    private float serveButtonInitialTime = -1f;

    [SerializeField] private PhotonView view;

    [HideInInspector] Animator animator;

    [HideInInspector] TextMeshProUGUI player1NameText;
    [HideInInspector] TextMeshProUGUI player2NameText;

    [HideInInspector] TextMeshProUGUI player1ScoreText;
    [HideInInspector] TextMeshProUGUI player2ScoreText;

    // float for storing x position for start, serve and ball spawn
    private float xPos = 0f;
    private float yPos = 0f;
    private float zPos = 0f;

    public int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

    private bool BallCountCheck() => GameObject.FindGameObjectsWithTag("TennisBall").Length >= 1;

    private string RightToHitKey() => string.Format("Player{0}RightToHit", PhotonNetwork.IsMasterClient ? 0 : 1);

    [HideInInspector] GameObject pleaseWait;

    private void Awake()
    {
        Timer.startTime = Time.time; /// For time synchronization for both players

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        animator = GetComponent<Animator>();
        powerBar = GameObject.Find("UI").transform.GetChild(2).GetChild(0).gameObject;
        powerBar.GetComponent<Slider>().value = 0f;

        // store the initial position
        initialPosition = transform.position;

        // store the initial rotation 
        initialRotation = transform.rotation;

        pleaseWait = GameObject.Find("UI").transform.GetChild(3).gameObject;

        player1NameText = GameObject.Find("Player1").transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        player2NameText = GameObject.Find("Player2").transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        player1ScoreText = GameObject.Find("Player1").transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        player2ScoreText = GameObject.Find("Player2").transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();

        player1ScoreText.text = "0";
        player2ScoreText.text = "0";
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();
        //rb = GetComponent<Rigidbody>();
        previousMousePos = Input.mousePosition;

        //PhotonNetwork.RegisterPhotonView(view);
        
        if (UserInterface == null)
        {
            UserInterface = GameObject.Find("UI");
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Player0")) 
            player1NameText.text = PhotonNetwork.CurrentRoom.CustomProperties["Player0"].ToString();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Player1"))
            player2NameText.text = PhotonNetwork.CurrentRoom.CustomProperties["Player1"].ToString();

        leftArrow = UserInterface.transform.GetChild(0).gameObject;
        rightArrow = UserInterface.transform.GetChild(1).gameObject;

        if (PhotonNetwork.LocalPlayer.ActorNumber -1 == 1)
        {
            xPos = 18.18f;
        }
    }

    private void Update()
    {
        //if(targetPosition == Vector3.zero) 
        //{
        //    Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        //}
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (hash.ContainsKey("Player0Score"))
        {
            // if the score has exceeded 70
            if (Convert.ToInt32(hash["Player0Score"]) > 70)
            {
                player1ScoreText.text = "Adv";
            }
            else
            {
                player1ScoreText.text = hash["Player0Score"].ToString();
            }
        }

        if (hash.ContainsKey("Player1Score"))
        {
            // if the score has exceeded 70
            if (Convert.ToInt32(hash["Player1Score"]) > 70)
            {
                player2ScoreText.text = "Adv";
            }
            else
            {
                player2ScoreText.text = hash["Player1Score"].ToString();
            }
        }

        if (view.IsMine)
        {
            if (isServing)
            {
                Serve();
            }
            if (!isServing)
            {
                if (Input.GetButton("LobLeft"))
                {
                    animator.SetFloat("HitTechnique", 1f);
                }
                if (Input.GetButton("LobRight"))
                {
                    animator.SetFloat("HitTechnique", 2f);
                }
                if (Input.GetButton("Overhead"))
                {
                    animator.SetFloat("HitTechnique", 3f);
                }
                if (!BallCountCheck() && (bool)PhotonNetwork.CurrentRoom.CustomProperties[RightToHitKey()])
                {
                    isServing = true;
                    powerBar.SetActive(true);
                    initialPosition = transform.position;
                    Serve();
                    if (gameManager.ball != null)
                    {
                        if(gameManager.ball.GetComponent<BallController>() != null)
                        {
                            gameManager.ball.GetComponent<BallController>().isServing = true;
                        }
                    }
                }                                
                if (Input.GetButton("Left"))
                {
                    animator.SetFloat("HitTechnique", 5f);
                }
                if (Input.GetButton("Right"))
                {
                    animator.SetFloat("HitTechnique", 6f);
                }
                if (Input.GetButton("Middle"))
                {
                    animator.SetFloat("HitTechnique", 7f);
                }
                targetPosition = Vector3.zero;
            }
            MouseDirectionNotifier();
        }
    }

    private void Serve()
    {
        //Debug.LogError(GameObject.FindGameObjectsWithTag("TennisBall").Length);
        if (view.IsMine && !BallCountCheck() && (bool)PhotonNetwork.CurrentRoom.CustomProperties[RightToHitKey()])
        {
            //if (PhotonNetwork.IsMasterClient)
            //{
            //    targetPosition = GameObject.Find("SpawnPlayers").GetComponent<SpawnPlayers>().playerPositionsUcanada[0];
            //}
            //else
            //{
            //    targetPosition = GameObject.Find("SpawnPlayers").GetComponent<SpawnPlayers>().playerPositionsUcanada[1];
            //}     

            //StartCoroutine(MoveToServe());  
            //while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            //{
            //    transform.position = Vector3.Lerp(initialPosition, targetPosition, 1 * Time.deltaTime);
            //}

            if (SceneManager.GetActiveScene().name == "Ucanada")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    xPos = -15f;
                }
                else
                {
                    xPos = 18.18f;
                }
                yPos = 2.008f;
                zPos = 1.18f;

                transform.position = new Vector3(xPos, yPos, zPos);
            }
            else if (SceneManager.GetActiveScene().name == "Sakura")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    xPos = 23;
                }
                else
                {
                    xPos = -14.5f;
                }
                yPos = 6.85f;
                zPos = -4.5f;
                transform.position = new Vector3(xPos, yPos, zPos);
            }

            animator.SetFloat("xAxis", 0);
            animator.SetFloat("yAxis", 0);
            
            //StartCoroutine("MoveToServe");

            //Quaternion currentRotation = Quaternion.Lerp(transform.rotation, initialRotation, 1);

            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Input.GetMouseButtonDown(0) = true");
                serveButtonInitialTime = Time.time;
            }

            if (serveButtonInitialTime != -1)
            {
                float timeButtonHeld = Time.time - serveButtonInitialTime;
                powerBar.GetComponent<Slider>().value = timeButtonHeld;
            }

            // adjust the slider color
            if (powerBar.GetComponent<Slider>().value < 0.45f)
            {
                powerBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.yellow;
            }
            else if (powerBar.GetComponent<Slider>().value > 0.76f)
            {
                powerBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
            }
            else
            {
                powerBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(166 / 255, 1, 0);
            }

            // On serve button release...
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("Input.GetMouseButtonUp(0) = true");
                //Debug.Log("power: " + powerBar.GetComponent<Slider>().value);
                animator.SetFloat("HitTechnique", 4f);
                ManageBall();
                
                Invoke("DeactivateBar", 1);
            }
            
        }
    }

    private IEnumerator MoveToServe()
    {        
        Vector3 initialPosition = transform.position;
        float elapsedTime = 0f;
        float moveDuration = 1f; // Adjust this value as needed

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure reaching the exact target position
        targetPosition = Vector3.zero;
    }

    private void DeactivateBar()
    {
        powerBar.GetComponent<Slider>().value = 0f;
        powerBar.SetActive(false);
        serveButtonInitialTime = -1;
        isServing = false;
    }

    private void ManageBall()
    {
        float ballFrontPadding = 0.3f;
        float ballSidePadding = - 0.3f;

        if (SceneManager.GetActiveScene().name == "Ucanada")
        {
            // if second player
            if (!PhotonNetwork.IsMasterClient)
            {
                ballFrontPadding *= -1;
                ballSidePadding *= -1;
                //Debug.Log("Changed starting positions for the serve ball ");
            }
        }
        else if (SceneManager.GetActiveScene().name == "Sakura")
        {
            //// if second player
            //if (!PhotonNetwork.IsMasterClient)
            //{
            //    ballFrontPadding *= -1;
            //    ballSidePadding *= -1;
            //    //Debug.Log("Changed starting positions for the serve ball ");
            //}
        }

        
        gameManager.ball = PhotonNetwork.Instantiate(tennisBallPrefab.name, new Vector3(xPos + ballFrontPadding, yPos + 0.4f, zPos + ballSidePadding), Quaternion.identity).transform;
        //GameObject ballToServe = PhotonNetwork.Instantiate(tennisBallPrefab.name, new Vector3(xPos + ballFrontPadding, 2.008f + 0.4f, 1.18f + ballSidePadding), Quaternion.identity) as GameObject;
        gameManager.ball.GetComponent<BallController>().isServing = isServing;
        gameManager.ball.GetComponent<BallController>().isScored = false;
        gameManager.ball.GetComponent<BallController>().yForce = 355f;

        //StartCoroutine(PhotonDestroyBall(4f));        
    }    
    
    private IEnumerator PhotonDestroyBall(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameManager.DestroyBall(0f);
    }

    private void MouseDirectionNotifier()
    {
        if (view.IsMine && !lockArrow)
        {
            currentMousePos = Input.mousePosition;
            Vector3 mouseMovement = currentMousePos - previousMousePos;            

            if (mouseMovement.x > 0)
            {
                rightArrow.SetActive(true);
                leftArrow.SetActive(false);
                hitDirection = Direction.Right;
            }
            else if (mouseMovement.x < 0)
            {
                rightArrow.SetActive(false);
                leftArrow.SetActive(true);
                hitDirection = Direction.Left;
            }
            previousMousePos = Input.mousePosition;
        }
    }

    private void FixedUpdate()
    {
        if (!view.IsMine && (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsPause"])
            pleaseWait.SetActive(true);
        else
            pleaseWait.SetActive(false);

        if (view.IsMine)
        {
            if (!isServing)
            {
                Move();
            }
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 normalizedMovement = new Vector2(horizontal, vertical).normalized;

        animator.SetFloat("xAxis", normalizedMovement.x, 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("yAxis", normalizedMovement.y, 0.1f, Time.fixedDeltaTime);

        transform.Translate(new Vector3(normalizedMovement.x * speed, 0, normalizedMovement.y * speed));
    }

    private void BugCheck()
    {
        if (view.IsMine)
        {
            if (transform.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }
            if (transform.position.y > 0.5)
            {
                transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
            }
        }       
    }

    private void FinishHit()
    {
        if (view.IsMine)
        {
            GetComponent<Animator>().SetFloat("HitTechnique", 0f);

            Invoke("UnlockArrow", 1);
        }            
    }
    
    private void UnlockArrow()
    {
        lockArrow = false;
    }

    private void HitEvent()
    {
        if (view.IsMine)
        {
            SuccessfulHit();
        }
    }

    private void SuccessfulHit()
    {
        //get racket position
        Vector3 racketPos = transform.GetChild(7).GetChild(2).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(5).position;
        GameObject ball = GameObject.FindGameObjectWithTag("TennisBall");
        
        if (ball != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                gameManager.playerResponsible = GameManager.PlayerResponsible.Player0;
            }
            else
            {
                gameManager.playerResponsible = GameManager.PlayerResponsible.Player1;
            }
            gameManager.ballBounceAfterHit = 0;
            gameManager.ballBounceOnTarget = 0;
            //Debug.Log("asas" + racketPos + " " + ball.transform.position);
            if (Vector3.Distance(ball.transform.position, racketPos) < ballHitRadius)
            {
                //Debug.Log("HIT");
                float forceX = 2750f;
                float forceY = 150f;
                float forceZ = 0f;
                forceZ = sidewaysForce;
                if (hitDirection == Direction.Left)
                {                    
                    lockArrow = true;
                    rightArrow.SetActive(false);
                    leftArrow.SetActive(true);
                    if (SceneManager.GetActiveScene().name == "Ucanada")
                    {
                        // if second player
                        if (!PhotonNetwork.IsMasterClient)
                        {
                            forceZ *= -1;
                        }
                    }
                    else if (SceneManager.GetActiveScene().name == "Sakura")
                    {

                        if (PhotonNetwork.IsMasterClient)
                        {
                            forceZ *= -1;
                        }
                    }
                }
                else if (hitDirection == Direction.Right)
                {
                    lockArrow = true;
                    rightArrow.SetActive(true);
                    leftArrow.SetActive(false);
                    if (SceneManager.GetActiveScene().name == "Ucanada")
                    {
                        // if second player
                        if (PhotonNetwork.IsMasterClient)
                        {
                            forceZ *= -1;
                        }
                    }
                    else if (SceneManager.GetActiveScene().name == "Sakura")
                    {

                        if (!PhotonNetwork.IsMasterClient)
                        {
                            forceZ *= -1;
                        }
                    }
                }
                if (animator.GetFloat("HitTechnique") == 1f || animator.GetFloat("HitTechnique") == 2f)
                {
                    forceY += 40f;
                    forceX -= 600f;
                    forceY *= 1.5f;
                }
                if (animator.GetFloat("HitTechnique") == 3f)
                {
                    forceY += -75f;
                    forceX += 250f;
                }
                if (animator.GetFloat("HitTechnique") == 4f)
                {
                    forceY -= powerBar.GetComponent<Slider>().value * 0.2f - 10f;
                    forceX *= powerBar.GetComponent<Slider>().value * 1.65f;
                    forceX += 300f;
                    forceY *= 0.3f;
                }
                else
                {
                    forceY *= 1.2f;
                }

                // ikinci oyuncu ise
                if (!PhotonNetwork.IsMasterClient)
                {
                    // vuruþ yönünü deðiþtir
                    forceX *= -1;
                    forceZ *= -1;
                }
                forceX *= 0.3f;
                forceY *= 1.2f;
                forceZ *= 0.3f;

                if (SceneManager.GetActiveScene().name == "Ucanada")
                {
                    
                }
                else if (SceneManager.GetActiveScene().name == "Sakura")
                {
                    forceX *= -1f;
                    //forceX *= -1f;
                    forceZ *= -1f;
                }

                //ball.GetComponent<Rigidbody>().AddForce(forceX, forceY, forceZ);
                //Debug.Log(forceZ);
                view.RPC("AddForceBall", RpcTarget.All, forceX, forceY, forceZ);                

                //Debug.Log("a " + ball);
                //BallController ballController = ball.GetComponent<BallController>();
                //Debug.Log("b " + ballController);
                //ballController.xForce = forceX; 
                //ballController.yForce = forceY; 
                //ballController.zForce = forceZ;

                //ballController.AddForce();

            }
        }
    }

    [PunRPC]
    void AddForceBall(float forceX, float forceY, float forceZ)
    {
        //GameObject.FindWithTag("TennisBall").GetComponent<BallController>().isServing = false;
        //Debug.Log("ddd");
        GameObject.FindGameObjectWithTag("TennisBall").GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameObject.FindGameObjectWithTag("TennisBall").GetComponent<Rigidbody>().AddForce(forceX, forceY, forceZ);

    }
}
