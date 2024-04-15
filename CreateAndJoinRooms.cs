using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] InputField createInput;
    [SerializeField] InputField joinInput;
    [SerializeField] InputField nickNameInput;
    [SerializeField] Text errorMessageText;
    [SerializeField] Toggle ucandaToggle;
    [SerializeField] Toggle sakuraToggle;

    [SerializeField] GameObject scrollViewContent;
    [HideInInspector] TextMeshProUGUI scrollViewContentText;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        errorMessageText.text = "";

        scrollViewContentText = scrollViewContent.GetComponent<TextMeshProUGUI>();
    }

    public void CreateRoom()
    {
        //PhotonNetwork.CreateRoom(createInput.text);

        if (string.IsNullOrWhiteSpace(createInput.text))
        {
            errorMessageText.text = "Please Enter Room Name.";
            return;
        }

        if (string.IsNullOrWhiteSpace(nickNameInput.text))
        {
            errorMessageText.text = "Please Enter Nick Name.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(createInput.text)) /// null check
            PhotonNetwork.CreateRoom(createInput.text, new RoomOptions { MaxPlayers = 2, IsOpen = true, IsVisible = true }, TypedLobby.Default);

        if (!string.IsNullOrWhiteSpace(nickNameInput.text)) /// null check
            PhotonNetwork.NickName = nickNameInput.text;
    }

    public void JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(joinInput.text))
        {
            errorMessageText.text = "Please Enter Room Name.";
            return;
        }

        if (string.IsNullOrWhiteSpace(nickNameInput.text))
        {
            errorMessageText.text = "Please Enter Nick Name.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(joinInput.text)) /// null check
            PhotonNetwork.JoinRoom(joinInput.text);

        if (!string.IsNullOrWhiteSpace(nickNameInput.text)) /// null check
            PhotonNetwork.NickName = nickNameInput.text;
    }

    public override void OnCreatedRoom()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!hash.ContainsKey("RoomScene"))
        {
            if (ucandaToggle.isOn)
            {
                hash.Add("RoomScene", "Ucanada");
            }
            else if (sakuraToggle.isOn)
            {
                hash.Add("RoomScene", "Sakura");
            }
            else
            {
                hash.Add("RoomScene", "Ucanada");
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    public override void OnJoinedRoom()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;

        if (hash.ContainsKey("RoomScene"))
        {
            if (hash["RoomScene"].ToString() == "Ucanada")
            {
                SceneManager.LoadScene("Ucanada");
            }
            else if (hash["RoomScene"].ToString() == "Sakura")
            {
                SceneManager.LoadScene("Sakura");
            }
            else
            {
                SceneManager.LoadScene("Ucanada");
            }
        } 
        else
        {
            SceneManager.LoadScene("Ucanada");
        }

        //SceneManager.LoadScene("Ucanada");
        //SceneManager.LoadScene("Sakura");
    }



    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorMessageText.text = string.Format("{0}.", message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorMessageText.text = string.Format("{0}.", message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        string roomName = string.Empty;
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
                cachedRoomList.Remove(info.Name);
            else
                cachedRoomList.Add(info.Name, info);
        }

        foreach (KeyValuePair<string, RoomInfo> item in cachedRoomList)
        {
            roomName += string.Format("Room Name: {0} \t\t\t Players: {1}/{2}", item.Value.Name, item.Value.PlayerCount, item.Value.MaxPlayers);
        }

        scrollViewContentText.text = roomName;
    }
}
