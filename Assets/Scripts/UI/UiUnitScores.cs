using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UiUnitScores : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI playerScoreText = default;
    [SerializeField]
    private TextMeshProUGUI opponentScoreText = default;

    public void UpdateScores(List<Card> opUnit, List<Card> plUnit) {

        opponentScoreText.text = BattleResolver.GetUnitScore(opUnit).ToString();
        playerScoreText.text = BattleResolver.GetUnitScore(plUnit).ToString();
    }
    
    private void Start() {
        Game.Instance.OnGameStateChanged += HandleGameStateChange;
    }

    private void HandleGameStateChange(GameState state) {
        switch (state) {
            case GameState.DealingCards:
            case GameState.OpponentsTurn: 
            case GameState.PlayersTurn:
            case GameState.GameIsOver:
                playerScoreText.enabled = false;
                opponentScoreText.enabled = false;
                break;
            case GameState.BattleResolving:
                playerScoreText.text = "0";
                playerScoreText.enabled = true;
                opponentScoreText.text = "0";
                opponentScoreText.enabled = true;
                break;
        }
    }
}