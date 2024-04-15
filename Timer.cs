using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] Text timeText;
    [HideInInspector] public static float startTime;

    // Start is called before the first frame update
    void Start()
    {
        timeText.text = "";
        startTime = Time.time;
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    private void FixedUpdate()
    {
        if (PhotonNetwork.CurrentRoom != null && 
            PhotonNetwork.CurrentRoom.PlayerCount == 2 && 
            !(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsPause"] &&
            !(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsEndGame"])
        {
            float t = Time.time - startTime;

            string minutes = ((int)t / 60).ToString("00");
            string seconds = (t % 60).ToString("00");

            timeText.text = string.Format("{0}:{1}", minutes, seconds);
        }
    }
}
