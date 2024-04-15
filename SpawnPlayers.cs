using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    private Transform playerTransform;
    public Vector3[] playerPositionsUcanada = { new Vector3(-15f, 2f, 6f), new Vector3(18.18f, 2f, -6f) };
    public Vector3[] playerPositionsSakura = { new Vector3(23f, 8.5f, -10f), new Vector3(-14.5f, 6.85f, 0.5f) };

    [SerializeField] GameObject pleaseWait;
    [HideInInspector] Text pleaseWaitText;

    [HideInInspector] float delay = 1f;
    [HideInInspector] string dotText = "....";
    [HideInInspector] string currentText;

    [HideInInspector] bool isSecondPlayerJoin = false;
    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
    public bool isRoomOpen = true;

    private void Start()
    {
        pleaseWaitText = pleaseWait.GetComponent<Text>();

        if (PhotonNetwork.PlayerList.Length == 1)
        {
            pleaseWait.SetActive(true);
        }

        StartCoroutine(WriterTypeAnimation());

        // Check if there is at least one player already in the room
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (!isRoomOpen)
        {
            //Debug.LogError("Room is closed. Cannot spawn player.");
            return;
        }

        Vector3 spawnPosition = new Vector3(0f, 0f, 0f); ///playerPositionsUcanada[playerIndex];

        if (SceneManager.GetActiveScene().name == "Ucanada")
        {
            spawnPosition = playerPositionsUcanada[playerIndex];
        }
        else if (SceneManager.GetActiveScene().name == "Sakura")
        {
            spawnPosition = playerPositionsSakura[playerIndex];
        }        

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        playerTransform = player.transform;

        int otherPlayerIndex = (playerIndex == 0) ? 1 : 0;
        Vector3 otherPlayerPosition = new Vector3(0f, 0f, 0f);  ///playerPositionsUcanada[otherPlayerIndex];

        if (SceneManager.GetActiveScene().name == "Ucanada")
        {
            otherPlayerPosition = playerPositionsUcanada[otherPlayerIndex];
        }
        else if (SceneManager.GetActiveScene().name == "Sakura")
        {
            otherPlayerPosition = playerPositionsSakura[otherPlayerIndex];
        }

        Vector3 direction = otherPlayerPosition - spawnPosition;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion adjustedRotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y - 31f, rotation.eulerAngles.z);
            playerTransform.rotation = adjustedRotation;
        }

        if (playerIndex == 1)
        {
            Camera mainCamera = Camera.main;
            //Vector3 cameraPosition = new Vector3(25f, 8.05f, 2.2f);
            //Vector3 cameraRotation = new Vector3(27.293869f, -90f, 0f);

            Vector3 cameraPosition = new Vector3(0f, 0f, 0f);
            Vector3 cameraRotation = new Vector3(0f, 0f, 0f);
            
            if (SceneManager.GetActiveScene().name == "Ucanada")
            {
                cameraPosition = new Vector3(25f, 8.05f, 2.2f);
                cameraRotation = new Vector3(27.3f, -90f, 0f);
            }
            else if (SceneManager.GetActiveScene().name == "Sakura") 
            {
                cameraPosition = new Vector3(-21f, 12.5f, -5f);
                cameraRotation = new Vector3(13f, 90f, 0f);
            }         

            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.eulerAngles = cameraRotation;
        }

        string playerName = string.IsNullOrWhiteSpace(PhotonNetwork.NickName) ? string.Format("Player {0}", Random.Range(1, 50)) : PhotonNetwork.NickName;
        string hasKey = string.Format("Player{0}", playerIndex);
        string hasKeyScore = string.Format("Player{0}Score", playerIndex);
        string hasKeyWin = string.Format("IsWinPlayer{0}", playerIndex);
        string hasKeyRightToHit = string.Format("Player{0}RightToHit", playerIndex);
        string hasKeyServeCount = string.Format("Player{0}ServeCount", playerIndex);       

        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(hasKey))
        {
            hash.Add(hasKey, playerName);
            hash.Add(hasKeyScore, 0);
            hash.Add(hasKeyWin, false);

            hash.Add(hasKeyRightToHit, PhotonNetwork.IsMasterClient ? true : false); /// First hit that makes up the room
            hash.Add(hasKeyServeCount, PhotonNetwork.IsMasterClient ? 2 : 0);

            if (!hash.ContainsKey("IsPause"))
                hash.Add("IsPause", false);

            if (!hash.ContainsKey("IsEndGame"))
                hash.Add("IsEndGame", false);

            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            SpawnPlayer();

            Timer.startTime = Time.time;
            isSecondPlayerJoin = true;
            pleaseWait.SetActive(false);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isRoomOpen = false;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
                PhotonNetwork.CloseConnection(player);
        }

        //Debug.Log("A player has left the room. The room is now closed.");

        playerIndex = 1;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 1;
        PhotonNetwork.CurrentRoom.RemovedFromList = true;
        isSecondPlayerJoin = false;
        pleaseWait.SetActive(true);
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(WriterTypeAnimation());

        SceneManager.LoadScene("Loading");
        PhotonNetwork.ConnectUsingSettings();
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
    IEnumerator WriterTypeAnimation()
    {
        while (!isSecondPlayerJoin)
        {
            for (int i = 0; i < dotText.Length; i++)
            {
                currentText = string.Format("Please Wait{0}", dotText.Substring(0, i));
                pleaseWaitText.text = currentText;
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
