using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreboardItemPrefab;
    [SerializeField] CanvasGroup canvasGroup;

    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    void Start(){
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }
    }

    void AddScoreboardItem(Player player){
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Inicialize(player);
        scoreboardItems[player] = item;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        AddScoreboardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        RemoveScoreboardItem(otherPlayer);
    }

    void RemoveScoreboardItem(Player player){
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player);
    }
    
    void Update(){
        if(Input.GetKeyDown(KeyCode.Tab)){
            canvasGroup.alpha = 1;
        }
        else if(Input.GetKeyUp(KeyCode.Tab)){
            canvasGroup.alpha = 0;
        }
    }
}
