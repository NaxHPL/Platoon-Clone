using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class UiBottomRightButton : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI text = default;
    
    private Button button;
    private Image backgroundImage;

    private void Awake() {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void Start() {
        Game.Instance.OnGameStateChanged += HandleGameStateChange;
        CardGroup.OnCardAddedToAGroup += HandleCardAddedToGroup;
        CardGroup.OnGroupSelected += HandleGroupSelected;
    }

    private void HandleGameStateChange(GameState state) {
        switch (state) {
            case GameState.DealingCards:
            case GameState.OpponentCreatingGroups:
            case GameState.OpponentsTurn:
            case GameState.BattleResolving:
                Show(false);
                break;
            case GameState.PlayersTurn:
            case GameState.PlayerCreatingGroups:
                button.interactable = false;
                text.text = "Confirm";
                Show(true);
                break;
            case GameState.GameIsOver:
                button.interactable = true;
                text.text = "Start New Game";
                Show(true);
                break;
        }
    }

    private void Show(bool show) {
        text.enabled = show;
        button.enabled = show;
        backgroundImage.enabled = show;
    }

    private void HandleCardAddedToGroup() {
        if (Game.Instance.Player.cardsInHand.Count > 0)
            return;

        button.interactable = true;
    }

    private void HandleGroupSelected() {
        if (Game.Instance.Player.cardGroupSelectedForBattle != null &&
            Game.Instance.Opponent.cardGroupSelectedForBattle != null) {
            button.interactable = true;
        }
    }

    private void OnButtonClick() {
        switch (Game.Instance.State) {
            case GameState.PlayerCreatingGroups:
                Game.Instance.PlayerFinishedCreatingGroups();
                break;
            case GameState.PlayersTurn:
                Game.Instance.GroupsForBattleConfirmed();
                break;
            case GameState.GameIsOver:
                Game.Instance.StartNewGame();
                break;
        }
    }
}