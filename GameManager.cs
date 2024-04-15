using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform ball;
    public enum GameState
    {
        ACTIVE,
        PLAYER0WON,
        PLAYER1WON
    }
    
    public enum PlayerResponsible
    {
        NONE,
        Player0,
        Player1,
    }

    //public GameState gameState = GameState.ACTIVE;
    public PlayerResponsible playerResponsible;

    //private void Player0Won()
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.LoadLevel("WinScene");
    //    }
    //    else
    //    {
    //        PhotonNetwork.LoadLevel("LossScene");
    //    }
    //}
    //private void Player1Won()
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.LoadLevel("LossScene");
    //    }
    //    else
    //    {
    //        PhotonNetwork.LoadLevel("LossScene");
    //    }
    //}   

    private List<GameObject> balls = new List<GameObject>();
    public int ballBounceAfterHit = 0;
    public int ballBounceOnTarget = 0;    

    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject lossPanel;

    [SerializeField] ParticleSystem confettiParticle1;
    [SerializeField] ParticleSystem confettiParticle2;

    [SerializeField] GameObject pausePanel;
    [HideInInspector] public bool gameIsPause = false;

    [HideInInspector] GameObject pleaseWait;

    [HideInInspector] Text timeText;

    [HideInInspector] Text winPanelWinPlayerNameText;
    [HideInInspector] Text winPanelWinPlayerScoreText;
    [HideInInspector] Text winPanelLossPlayerNameText;
    [HideInInspector] Text winPanelLossPlayerScoreText;
    [HideInInspector] Text winPanelTimeText;

    [HideInInspector] Text lossPanelWinPlayerNameText;
    [HideInInspector] Text lossPanelWinPlayerScoreText;
    [HideInInspector] Text lossPanelLossPlayerNameText;
    [HideInInspector] Text lossPanelLossPlayerScoreText;
    [HideInInspector] Text lossPanelTimeText;

    public int FirstOrSecondPlayer() => PhotonNetwork.IsMasterClient ? 0 : 1;

    private void Awake()
    {
        winPanel.SetActive(false);
        lossPanel.SetActive(false);

        timeText = GameObject.Find("UI").transform.GetChild(4).gameObject.GetComponent<Text>();

        winPanelWinPlayerNameText = winPanel.transform.GetChild(1).gameObject.GetComponent<Text>();
        winPanelWinPlayerScoreText = winPanel.transform.GetChild(2).gameObject.GetComponent<Text>();
        winPanelLossPlayerNameText = winPanel.transform.GetChild(3).gameObject.GetComponent<Text>();
        winPanelLossPlayerScoreText = winPanel.transform.GetChild(4).gameObject.GetComponent<Text>();
        winPanelTimeText = winPanel.transform.GetChild(5).gameObject.GetComponent<Text>();

        lossPanelWinPlayerNameText = lossPanel.transform.GetChild(1).gameObject.GetComponent<Text>();
        lossPanelWinPlayerScoreText = lossPanel.transform.GetChild(2).gameObject.GetComponent<Text>();
        lossPanelLossPlayerNameText = lossPanel.transform.GetChild(3).gameObject.GetComponent<Text>();
        lossPanelLossPlayerScoreText = lossPanel.transform.GetChild(4).gameObject.GetComponent<Text>();
        lossPanelTimeText = lossPanel.transform.GetChild(5).gameObject.GetComponent<Text>();
    }

    private void Start()
    {
        pleaseWait = GameObject.Find("UI").transform.GetChild(3).gameObject;

        //Debug.Log("In Start Func");
        //StartCoroutine("DestroyBallClones");
    }

    private void Update()
    {
        //if (gameState == GameState.PLAYER0WON)
        //{
        //    Player0Won();
        //}
        //if (gameState == GameState.PLAYER1WON)
        //{
        //    Player1Won();
        //}

        if (PhotonNetwork.CurrentRoom != null)
        {
            var hash = PhotonNetwork.CurrentRoom.CustomProperties;

            if (hash.ContainsKey("IsEndGame") && (bool)hash["IsEndGame"])
            {
                if (hash.ContainsKey("IsWinPlayer" + FirstOrSecondPlayer()))
                {
                    if ((bool)hash["IsWinPlayer" + FirstOrSecondPlayer()])
                    {
                        winPanel.SetActive(true);
                        lossPanel.SetActive(false);

                        winPanelWinPlayerNameText.text = hash["Player0"].ToString();
                        winPanelWinPlayerScoreText.text = hash["Player0Score"].ToString();
                        winPanelLossPlayerNameText.text = hash["Player1"].ToString();
                        winPanelLossPlayerScoreText.text = hash["Player1Score"].ToString();
                        winPanelTimeText.text = timeText.text;

                        confettiParticle1.Play();
                        confettiParticle2.Play();
                    }
                    else
                    {
                        winPanel.SetActive(false);
                        lossPanel.SetActive(true);

                        lossPanelWinPlayerNameText.text = hash["Player0"].ToString();
                        lossPanelWinPlayerScoreText.text = hash["Player0Score"].ToString();
                        lossPanelLossPlayerNameText.text = hash["Player1"].ToString();
                        lossPanelLossPlayerScoreText.text = hash["Player1Score"].ToString();
                        lossPanelTimeText.text = timeText.text;

                        confettiParticle1.Stop();
                        confettiParticle2.Stop();
                    }
                }
                //Time.timeScale = 0f;
            }
        }        

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;

        pausePanel.SetActive(true);
        pleaseWait.SetActive(false);
        //Cursor.lockState = CursorLockMode.None;

        if (hash.ContainsKey("IsPause"))
            hash["IsPause"] = true;

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        Time.timeScale = 0f;
        gameIsPause = true;        
    }

    public void ResumeGame()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;

        pausePanel.SetActive(false);
        pleaseWait.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;

        if (hash.ContainsKey("IsPause"))
            hash["IsPause"] = false;

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        Time.timeScale = 1f;
        gameIsPause = false;       
    }

    public void ExitApp()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;

        //Dictionary<int, Player> playerList = PhotonNetwork.CurrentRoom.Players;

        //foreach (KeyValuePair<int, Player> p in playerList)
        //{
        //    PhotonNetwork.DestroyPlayerObjects(p.Value);
        //}

        //PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        //PhotonNetwork.LeaveRoom();

        //if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        //{
        //    var dict = PhotonNetwork.CurrentRoom.Players;
        //    if (PhotonNetwork.SetMasterClient(dict[dict.Count - 1]))
        //    {
        //        Debug.LogError("SetMasterClient");
        //        //PhotonNetwork.DestroyPlayerObjects(dict[dict.Count - 1]);
        //        PhotonNetwork.LeaveRoom();
        //    }
        //}
        //else
        //{
        //    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        //    PhotonNetwork.LeaveRoom();
        //}

        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        Application.Quit();
    }

    public void MainMenu()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        Time.timeScale = 1f;
    }

    public override void OnLeftRoom()
    {
        //Debug.LogError("OnLeftRoom");
        StartCoroutine(WaitToLeave());
    }

    IEnumerator WaitToLeave()
    {
        while (PhotonNetwork.InRoom)
            yield return null;

        SceneManager.LoadScene("Loading");
    }

    public void WinGame()
    {
        winPanel.SetActive(true);
        //Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    //private IEnumerator DestroyBallClones()
    //{
    //    while (true)
    //    {
    //        Debug.Log("DestroyBallClones Coroutine");
    //        yield return new WaitForSeconds(4);
    //        Debug.Log("Passed the Delay");
    //        int cloneCount = 0;
    //        foreach (GameObject go in Object.FindObjectsOfType(typeof(GameObject)))
    //        {
    //            if (go.name == "TennisBall(Clone)")
    //            {
    //                cloneCount++;
    //                balls.Add(go);
    //            }
    //            if (cloneCount == 2)
    //            {
    //                for (int i = 0; i < balls.Count; i++)
    //                {
    //                    if (balls.Count > 0)
    //                    {
    //                        PhotonNetwork.Destroy(balls[i]);
    //                    }
    //                }
    //            }
    //        }
    //        cloneCount = 0; // Reset the cloneCount after checking all balls
    //        Debug.Log("For loop ended, cloneCount = " + cloneCount);
    //    }
    //}
    
    public void DestroyBall(float seconds)
    {
        GameObject ball = GameObject.FindGameObjectWithTag("TennisBall");
        if (ball != null)
        {
            PlayParticleEffect(ball.transform.position);
            StartCoroutine(DelayedDestroy(seconds));            
        }
    }

    private IEnumerator DelayedDestroy(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        GameObject ball = GameObject.FindGameObjectWithTag("TennisBall");
        if (ball != null)
            PhotonNetwork.Destroy(ball);
    }   

    public void PlayParticleEffect(Vector3 position)
    {
        GameObject ball = GameObject.FindGameObjectWithTag("TennisBall");
        if (ball == null || ball.GetComponent<BallController>() == null) return;

        GameObject particleSystemObject = PhotonNetwork.Instantiate(ball.gameObject.GetComponent<BallController>().particleSystemPrefab.name, position, Quaternion.identity);
        if (particleSystemObject != null)
        {
            ParticleSystem particleSystem = particleSystemObject.GetComponent<ParticleSystem>();
            particleSystem.Play();
            StartCoroutine(StopParticleEffect(particleSystem, 0.5f));
        }
    }

    private IEnumerator StopParticleEffect(ParticleSystem particleSystem, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (particleSystem != null || particleSystem.gameObject != null) 
            PhotonNetwork.Destroy(particleSystem.gameObject);     
    }
}
