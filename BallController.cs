using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallController : MonoBehaviour
{
    private Rigidbody rb;
    private float gravity;
    private Vector3 gravityForce;
    public float yForce, xForce, zForce = 0;

    public GameObject particleSystemPrefab;

    [HideInInspector] public bool isServing = false;
    [HideInInspector] public bool isScored = false;

    // reference for GameManager script
    private GameManager gameManager;    

    void Start()
    {
        gravity = SceneManager.GetActiveScene().name == "Ucanada" ? -7.8f * 60 : -6.5f * 60;

        gravityForce = new Vector3(0, gravity, 0);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(xForce, yForce, zForce = 0));

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //StartCoroutine(DetectDistance());
    }   

    private IEnumerator DetectDistance()
    {
        while (true)
        {
            //Debug.Log(transform.position);
            //Debug.Log(GameObject.FindGameObjectWithTag("RacketCollider").transform.position);
            //Debug.Log(Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("RacketCollider").transform.position));

            yield return new WaitForSeconds(0.5f);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name + " " + other.tag);

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        /// top sahadan çýktý mý kontrol et
        if (other.tag == "OutOfBounds")
        {
            gameManager.DestroyBall(0.1f);

            /// top saha dýþýnda
            if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player0 && PhotonNetwork.IsMasterClient)
            {   /// Player0 sorumlu
                PointToPlayer1();
            }
            if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player1 && PhotonNetwork.IsMasterClient)
            {   /// Player1 sorumlu

                /// Player0'a puan yaz
                PointToPlayer0();
            }

            hash = PhotonNetwork.CurrentRoom.CustomProperties;
        }

        /// top sekiþini say
        if (other.tag != "OutOfBounds" && other.tag != "File")
        {
            gameManager.ballBounceAfterHit++;
        }

        if (other.name == "MasterServeScoreArea" && isServing)
        {
            ///Debug.LogError("MasterServeScoreArea " + isServing);

            if (PhotonNetwork.IsMasterClient)
            {
                if (hash.ContainsKey("Player0Score"))
                {
                    /// Ball bounced on the serve area for "Player0"
                    gameManager.ballBounceOnTarget++;
                    if (gameManager.ballBounceAfterHit == 1 && gameManager.ballBounceOnTarget == 1)
                    {
                        /// Only bounced once on the serve area for "Player0" once, time for "Player1" to catch
                        gameManager.playerResponsible = GameManager.PlayerResponsible.Player1;
                    }

                    hash["Player0RightToHit"] = false;
                    hash["Player1RightToHit"] = true;

                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                }
            }
            else /// the player is nonmaster and the ball has dropped in his own court
            {
                hash["Player0RightToHit"] = true;
                hash["Player1RightToHit"] = false;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                ///Debug.Log("zczxc");
                gameManager.DestroyBall(0.1f);
                PointToPlayer0();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz

                //if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player1)
                //{

                //}
                //Debug.Log("þþþ");
            }
        }
        else if (other.tag == "MasterScoreArea" && isServing) /// yanlýþ servis kullandý
        {
            ///Debug.LogError("MasterScoreArea " + isServing);

            if (!PhotonNetwork.IsMasterClient)
            {   // NON master kendi sahasýna serve attý
                hash["Player0RightToHit"] = true;
                hash["Player1RightToHit"] = false;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player1)
                {
                    gameManager.DestroyBall(0.1f);

                    ///Debug.Log("yyy");
                    PointToPlayer0();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz
                }
            }
            else
            {   /// master rakip sahaya yanlýþ servis kullandý
                hash["Player0RightToHit"] = false;
                hash["Player1RightToHit"] = true;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                gameManager.DestroyBall(0.1f);

                ///Debug.Log("yyy");
                PointToPlayer1();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz

                //if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player0)
                //{

                //}
            }
        }

        if (other.name == "NonMasterServeScoreArea" && isServing)
        {   /// non master serve hedefine gitti 
            ///Debug.LogError("NonMasterServeScoreArea " + isServing);

            if (!PhotonNetwork.IsMasterClient)
            {   /// non master doðru serve kullandý
                if (hash.ContainsKey("Player1Score"))
                {
                    /// Ball bounced on the serve area for "Player1"                                        
                    gameManager.ballBounceOnTarget++;
                    if (gameManager.ballBounceAfterHit == 1 && gameManager.ballBounceOnTarget == 1)
                    {
                        /// Only bounced once on the serve area for "Player1" once, time for "Player0" to catch
                        gameManager.playerResponsible = GameManager.PlayerResponsible.Player0;
                    }

                    hash["Player0RightToHit"] = true;
                    hash["Player1RightToHit"] = false;

                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                }
            }
            else /// if the player is master and the ball has dropped in his own court
            {
                hash["Player0RightToHit"] = false;
                hash["Player1RightToHit"] = true;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                ///Debug.Log("dasdas");

                gameManager.DestroyBall(0.1f);
                PointToPlayer1();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz 

                //if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player0)
                //{

                //}
                //Debug.Log("xxx");
            }
        }
        else if (other.tag == "NonMasterScoreArea" && isServing)
        {   /// yanlýþ servis kullandý
            ///Debug.LogError("NonMasterScoreArea " + isServing);

            if (PhotonNetwork.IsMasterClient)
            {   // master serve de kendi sahasýna düþürdü
                hash["Player0RightToHit"] = false;
                hash["Player1RightToHit"] = true;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                gameManager.DestroyBall(0.1f);

                ///Debug.Log("aa");
                PointToPlayer1();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz

                //if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player0)
                //{

                //}
            }
            else
            {   /// non master serve ü yanlýþ kullandý
                hash["Player0RightToHit"] = true;
                hash["Player1RightToHit"] = false;

                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                gameManager.DestroyBall(0.1f);

                ///Debug.Log("czxc");
                PointToPlayer0();  /// servis yanlýþ kullandýðýnda skoru karþý oyuncuya yaz

                //if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player1)
                //{

                //}
            }
        }

        /// Normal atýþ
        if (other.tag == "MasterScoreArea" && !isServing)
        {
            ///Debug.LogError("MasterScoreArea " + isServing);

            if (PhotonNetwork.IsMasterClient)
            {
                if (hash.ContainsKey("Player0Score"))
                {
                    /// Ball bounced on the score area for "Player0"
                    gameManager.ballBounceOnTarget++;
                    if (gameManager.ballBounceAfterHit == 1 && gameManager.ballBounceOnTarget == 1)
                    {
                        /// Only bounced once on the score area for "Player0" once, time for "Player1" to catch
                        gameManager.playerResponsible = GameManager.PlayerResponsible.Player1;
                    }
                }
            }
            else /// the player is nonmaster and the ball has dropped in his own court
            {
                ///gameManager.playerResponsible == GameManager.PlayerResponsible.Player1 &&
                if (gameManager.ballBounceOnTarget > 1)
                {
                    hash["Player0RightToHit"] = true;
                    hash["Player1RightToHit"] = false;

                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                    gameManager.DestroyBall(0.1f); /// skor verdiðine göre topu destroy yapabiliriz

                    ///Debug.Log("bbb");
                    PointToPlayer0();
                }
            }
        }

        if (other.tag == "NonMasterScoreArea" && !isServing)
        {
            ///Debug.LogError("NonMasterScoreArea " + isServing);

            if (!PhotonNetwork.IsMasterClient)
            {
                if (hash.ContainsKey("Player1Score"))
                {
                    /// Ball bounced on the score area for "Player1"                                        
                    gameManager.ballBounceOnTarget++;
                    if (gameManager.ballBounceAfterHit == 1 && gameManager.ballBounceOnTarget == 1)
                    {
                        /// Only bounced once on the score area for "Player1" once, time for "Player0" to catch
                        gameManager.playerResponsible = GameManager.PlayerResponsible.Player0;
                    }

                    //hash["Player0RightToHit"] = true;
                    //hash["Player1RightToHit"] = false;

                    //PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                }
            }
            else /// if the player is master and the ball has dropped in his own court
            {
                ///gameManager.playerResponsible == GameManager.PlayerResponsible.Player0 &&
                if (gameManager.ballBounceOnTarget > 1)
                {
                    hash["Player0RightToHit"] = false;
                    hash["Player1RightToHit"] = transform;

                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                    gameManager.DestroyBall(0.1f); /// skor verdiðine göre topu destroy yapabiliriz

                    //Debug.Log("1bbb");
                    PointToPlayer1();
                }
            }
        }

        //if (gameManager.ballBounceAfterHit > 1)
        //{
        //    if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player0)
        //    {   // Player0 sorumlu

        //        // Player1'e puan yaz
        //        Debug.Log("1ccc");
        //        PointToPlayer1();
        //    }

        //    if (gameManager.playerResponsible == GameManager.PlayerResponsible.Player1)
        //    {   // Player1 sorumlu

        //        Debug.Log("ccc");
        //        // Player0'a puan yaz
        //        PointToPlayer0();   
        //    }
        //}
        isServing = false;
    }

    private void PointToPlayer0()
    {
        if (isScored) return;

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;

        // Player0'a puan yaz
        if (hash.ContainsKey("Player0Score"))
        {
            if (Convert.ToInt32(hash["Player0Score"]) < 60)
            {
                hash["Player0Score"] = (Convert.ToInt32(hash["Player0Score"]) + 15).ToString();
            }
            else if (Convert.ToInt32(hash["Player0Score"]) == 60)
            {
                hash["Player0Score"] = (Convert.ToInt32(hash["Player0Score"]) + 10).ToString();
            }
            else if (Convert.ToInt32(hash["Player0Score"]) == 70)
            {   // Score is 70, win if opponent is under 70, go adv if both 70, if opponent is adv make opponent score 70
                if (hash.ContainsKey("Player1Score"))
                {
                    // case under 70
                    if (Convert.ToInt32(hash["Player1Score"]) < 70)
                    {  // WON THE GAME
                        //gameManager.gameState = GameManager.GameState.PLAYER1WON;

                        if (hash.ContainsKey("IsWinPlayer0"))
                            hash["IsWinPlayer0"] = true;

                        if (hash.ContainsKey("IsWinPlayer1"))
                            hash["IsWinPlayer1"] = false;

                        if (hash.ContainsKey("IsEndGame"))
                            hash["IsEndGame"] = true;
                    }
                    // case == 70
                    else if (Convert.ToInt32(hash["Player1Score"]) == 70)
                    {
                        hash["Player0Score"] = (Convert.ToInt32(hash["Player0Score"]) + 10).ToString();
                    }
                    // case where opponent had advantage
                    else
                    {
                        hash["Player1Score"] = 70.ToString();
                    }
                }

            }
            else
            {   // Was on adv, thus WON THE GAME
                //gameManager.gameState = GameManager.GameState.PLAYER0WON;

                if (hash.ContainsKey("IsWinPlayer0"))
                    hash["IsWinPlayer0"] = true;

                if (hash.ContainsKey("IsWinPlayer1"))
                    hash["IsWinPlayer1"] = false;

                if (hash.ContainsKey("IsEndGame"))
                    hash["IsEndGame"] = true;
            }
            //hash["Player1Score"] = (Convert.ToInt32(hash["Player1Score"]) + 15).ToString();
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            gameManager.playerResponsible = GameManager.PlayerResponsible.NONE;

            isScored = true;
        }        
    }


    private void PointToPlayer1()
    {
        if (isScored) return;

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        // Player1'e puan yaz
        if (hash.ContainsKey("Player1Score"))
        {
            if (Convert.ToInt32(hash["Player1Score"]) < 60)
            {
                hash["Player1Score"] = (Convert.ToInt32(hash["Player1Score"]) + 15).ToString();
            }
            else if (Convert.ToInt32(hash["Player1Score"]) == 60)
            {
                hash["Player1Score"] = (Convert.ToInt32(hash["Player1Score"]) + 10).ToString();
            }
            else if (Convert.ToInt32(hash["Player1Score"]) == 70)
            {   // Score is 70, win if opponent is under 70, go adv if both 70, if opponent is adv make opponent score 70
                if (hash.ContainsKey("Player0Score"))
                {
                    // case under 70
                    if (Convert.ToInt32(hash["Player0Score"]) < 70)
                    {  // WON THE GAME
                        //gameManager.gameState = GameManager.GameState.PLAYER1WON;

                        if (hash.ContainsKey("IsWinPlayer1"))
                            hash["IsWinPlayer1"] = true;

                        if (hash.ContainsKey("IsWinPlayer0"))
                            hash["IsWinPlayer0"] = false;

                        if (hash.ContainsKey("IsEndGame"))
                            hash["IsEndGame"] = true;
                    }
                    // case == 70
                    else if (Convert.ToInt32(hash["Player0Score"]) == 70)
                    {
                        hash["Player1Score"] = (Convert.ToInt32(hash["Player1Score"]) + 10).ToString();
                    }
                    // case where opponent had advantage
                    else
                    {
                        hash["Player0Score"] = 70.ToString();
                    }
                }                
            }
            else
            {   // Was on adv, thus WON THE GAME
                //gameManager.gameState = GameManager.GameState.PLAYER1WON;

                if (hash.ContainsKey("IsWinPlayer1"))
                    hash["IsWinPlayer1"] = true;

                if (hash.ContainsKey("IsWinPlayer0"))
                    hash["IsWinPlayer0"] = false;

                if (hash.ContainsKey("IsEndGame"))
                    hash["IsEndGame"] = true;
            }
            //hash["Player1Score"] = (Convert.ToInt32(hash["Player1Score"]) + 15).ToString();
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            gameManager.playerResponsible = GameManager.PlayerResponsible.NONE;

            isScored = true;
        }        
    }

    //private static void Player0Won()
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

    //private static void Player1Won()
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.LoadLevel("WinScene");
    //    }
    //    else
    //    {
    //        PhotonNetwork.LoadLevel("LossScene");
    //    }
    //}

    private IEnumerator HitOrMiss(float wait)
    {
        yield return new WaitForSeconds(wait);

        if (PhotonNetwork.IsMasterClient)
        {
        //    if(gameManager.playerResponsible == 2)
        //    {
        //        hitBack = true;
        //    }
        //    else
        //    {
        //        hitBack = false;
        //    }
        //}else
        //{
        //    if (gameManager.playerResponsible == 1)
        //    {
        //        hitBack = true;
        //    }
        //    else
        //    {
        //        hitBack = false;
        //    }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {        
        this.GetComponent<AudioSource>().Play();
    }    

    public void AddForce()
    {
        //Debug.Log("Indide AddForce " + xForce + " "  + yForce + " " + zForce);
        GameObject.FindGameObjectWithTag("TennisBall").GetComponent<Rigidbody>().AddForce(xForce, yForce, zForce);
    }
    private void FixedUpdate()
    {
        // Apply gravity 
        
        //transform.position = new Vector3(transform.position.x, transform.position.y + (gravity * Time.deltaTime), transform.position.z);
    }

    void Update()
    {
        rb.AddForce(gravityForce * Time.deltaTime, ForceMode.Acceleration);
    }
}
