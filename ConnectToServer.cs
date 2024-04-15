using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] Text loadingText;
    [SerializeField] Text pleaseWaitText;

    [HideInInspector] float delay = 0.3f;
    [HideInInspector] string dotText = "....";
    [HideInInspector] string currentloadingText;
    [HideInInspector] string currentpleaseWaitText;

    [HideInInspector] bool isJoinLobby = false;

    private void Awake()
    {
        /// The value of AutomaticallySyncScene is set to true. This is used to sync the scene across all the connected players in a room.
        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (loadingText != null || pleaseWaitText != null)
        {
            currentloadingText = loadingText.text;
            currentpleaseWaitText = pleaseWaitText.text;

            StartCoroutine(WriterTypeAnimation());
        }

        //PhotonNetwork.ConnectUsingSettings();
        if (!PhotonNetwork.IsConnected)
        {
            /// The value of AutomaticallySyncScene is set to true. This is used to sync the scene across all the connected players in a room.
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        isJoinLobby = true;
        SceneManager.LoadScene("Lobby");
    }

    IEnumerator WriterTypeAnimation()
    {
        while (!isJoinLobby)
        {
            for (int i = 0; i < dotText.Length; i++)
            {
                loadingText.text = string.Format("{0}{1}", currentloadingText, dotText.Substring(0, i));
                pleaseWaitText.text = string.Format("{0}{1}", currentpleaseWaitText, dotText.Substring(0, i));

                yield return new WaitForSeconds(delay);
            }
        }            
    }
}
