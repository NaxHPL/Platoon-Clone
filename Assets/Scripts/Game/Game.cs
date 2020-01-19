using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
    
    public static Game Instance { get; private set; }
    
    public GameState State { get; private set; }
    public PlatoonPlayer Player { get; private set; }
    public PlatoonPlayer Opponent { get; private set; }

    public event Action<GameState> OnGameStateChanged;
    public event Action<BattleOutcome> OnBattleResolved;
    
    [SerializeField]
    private Deck cardDeck = default;
    [SerializeField, Range(7, 10)]
    private int cardsToDeal = default;
    [SerializeField]
    private PlayArea playerPlayArea = default;
    [SerializeField]
    private PlayArea opponentPlayArea = default;
    [SerializeField]
    private TextMeshProUGUI messageText = default;
    [SerializeField]
    private UiUnitScores uiUnitScores = default;
    
    private enum GameEndStatus { PlayerWins, OpponentWins, Draw, NotOverYet }

    private Coroutine messageCoroutine;
    private bool playerHasNextTurn;
    private int numRoundsBattled;

    [ContextMenu("Start New Game")]
    public void StartNewGame() {
        ResetGame();
        StartCoroutine(DealCards());
    }

    public void ShowMessage(string message, float duration) {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
            
        messageCoroutine = StartCoroutine(ShowMessageFor(message, duration));
    }

    public void PlayerFinishedCreatingGroups() {
        ChangeGameState(GameState.OpponentCreatingGroups);
        StartCoroutine(PutOpponentCardsInGroups());
    }

    public void GroupsForBattleConfirmed() {
        ChangeGameState(GameState.BattleResolving);
        StartCoroutine(MoveBattleUnitsToMiddle());
    }

    private IEnumerator PutOpponentCardsInGroups() {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        Dictionary<CardGroup, List<Card>> groupsOfCards = Opponent.RandomlyAssignCardsToGroups();

        foreach (CardGroup group in groupsOfCards.Keys) {
            foreach (Card card in groupsOfCards[group]) {
                group.AddCard(card, false);
                
                yield return wait;
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        ChangeGameState(GameState.DecidingFirstTurn);
        StartCoroutine(DecideFirstTurn());
    }

    private IEnumerator MoveBattleUnitsToMiddle() {
        List<Card> opponentUnit = Opponent.cardGroupSelectedForBattle.cards;
        Vector2[] positionsForOpponentUnits = CardLayoutHelper.GetCardLayout(
            new Vector2(0f, 1f),
            opponentUnit.Count,
            opponentUnit.First().CardWidth,
            0.1f
        );
        
        List<Card> playerUnit = Player.cardGroupSelectedForBattle.cards;
        Vector2[] positionsForPlayerUnits = CardLayoutHelper.GetCardLayout(
            new Vector2(0f, -1f),
            playerUnit.Count,
            playerUnit.First().CardWidth,
            0.1f
        );

        Opponent.cardGroupSelectedForBattle.Select(false);
        Player.cardGroupSelectedForBattle.Select(false);
        
        for (int i = 0; i < opponentUnit.Count; i++) {
            Vector2 startPos = opponentUnit[i].transform.localPosition;
            Vector2 endPos = positionsForOpponentUnits[i];
            StartCoroutine(opponentUnit[i].MoveFromTo(startPos, endPos, 0.3f));
        }
        for (int i = 0; i < playerUnit.Count; i++) {
            Vector2 startPos = playerUnit[i].transform.localPosition;
            Vector2 endPos = positionsForPlayerUnits[i];
            StartCoroutine(playerUnit[i].MoveFromTo(startPos, endPos, 0.3f));
        }

        yield return new WaitForSeconds(0.5f);
        
        foreach (Card card in opponentUnit)
            StartCoroutine(card.FlipCard(0.3f));
        foreach (Card card in playerUnit)
            StartCoroutine(card.FlipCard(0.3f));
        
        StartCoroutine(ResolveBattle(opponentUnit, playerUnit, false));
    }

    private IEnumerator ResolveBattle(List<Card> opUnit, List<Card> plUnit, bool ignoreWizards) {
        uiUnitScores.UpdateScores(opUnit, plUnit);
        
        yield return new WaitForSeconds(1f);

        BattleOutcome outcome = BattleResolver.GetBattleOutcome(opUnit, plUnit, ignoreWizards);
        
        switch (outcome) {
            case BattleOutcome.PlayerWins:
                ShowMessage("Player wins!", 2.0f);
                Player.numberOfWins++;
                numRoundsBattled++;
                OnBattleResolved?.Invoke(outcome);
                yield return new WaitForSeconds(2.0f);

                StartCoroutine(RemoveCardsFromField(opUnit, plUnit));
                ContinueToNextTurn();
                break;
            
            case BattleOutcome.OpponentWins:
                ShowMessage("Opponent wins!", 2.0f);
                Opponent.numberOfWins++;
                numRoundsBattled++;
                OnBattleResolved?.Invoke(outcome);
                yield return new WaitForSeconds(2.0f);
                
                StartCoroutine(RemoveCardsFromField(opUnit, plUnit));
                ContinueToNextTurn();
                break;
            
            case BattleOutcome.SwapUnitsOnce:
                SwapUnits(opUnit, plUnit, 0.5f);
                yield return new WaitForSeconds(0.5f);
                
                StartCoroutine(ResolveBattle(plUnit, opUnit, true));
                break;
            
            case BattleOutcome.SwapUnitsTwice:
                SwapUnits(opUnit, plUnit, 0.5f);
                yield return new WaitForSeconds(0.5f);
                
                SwapUnits(plUnit, opUnit, 0.5f);
                yield return new WaitForSeconds(0.5f);
                
                StartCoroutine(ResolveBattle(plUnit, opUnit, true));
                break;
            
            case BattleOutcome.Draw:
                ShowMessage("Draw!", 2.0f);
                numRoundsBattled++;
                OnBattleResolved?.Invoke(outcome);
                yield return new WaitForSeconds(2.0f);
                
                StartCoroutine(RemoveCardsFromField(opUnit, plUnit));
                ContinueToNextTurn();
                break;
        }
    }

    private IEnumerator RemoveCardsFromField(List<Card> opUnit, List<Card> plUnit) {
        foreach (Card card in opUnit) {
            Vector2 startPos = card.transform.localPosition;
            Vector2 endPos = new Vector2(startPos.x + 20f, startPos.y);
            StartCoroutine(card.MoveFromTo(startPos, endPos, 0.5f));
        }
        
        foreach (Card card in plUnit) {
            Vector2 startPos = card.transform.localPosition;
            Vector2 endPos = new Vector2(-20f, startPos.y);
            StartCoroutine(card.MoveFromTo(startPos, endPos, 0.5f));
        }
        
        opUnit.First().group.ClearCardsAndDisableCanvas();
        plUnit.First().group.ClearCardsAndDisableCanvas();
        
        yield return new WaitForSeconds(1f);
        
        foreach (Card card in opUnit)
            CardPool.Instance.ReturnToPool(card);
        foreach (Card card in plUnit)
            CardPool.Instance.ReturnToPool(card);
    }
    
    private void ContinueToNextTurn() {
        GameEndStatus endStatus = CheckWinnerStatus();
        
        if (endStatus != GameEndStatus.NotOverYet) {
            EndGame(endStatus);
            return;
        }
        
        if (playerHasNextTurn) {
            playerHasNextTurn = false;
            ChangeGameState(GameState.PlayersTurn);
        }
        else {
            playerHasNextTurn = true;
            StartCoroutine(OpponentChooseGroupsForBattle());
        }
    }

    private void EndGame(GameEndStatus endStatus) {
        ChangeGameState(GameState.GameIsOver);

        switch (endStatus) {
            case GameEndStatus.PlayerWins:
                ShowMessage("You win!", 120f);
                break;
            case GameEndStatus.OpponentWins:
                ShowMessage("You lose!", 120f);
                break;
            case GameEndStatus.Draw:
                ShowMessage("It's a draw!", 120f);
                break;
        }
        
        CardPool.Instance.ReturnAllToPool();
    }
    
    private GameEndStatus CheckWinnerStatus() {
        if (Player.numberOfWins == 3)
            return GameEndStatus.PlayerWins;
        if (Opponent.numberOfWins == 3)
            return GameEndStatus.OpponentWins;
        if (numRoundsBattled == 5 && Player.numberOfWins < 3 && Opponent.numberOfWins < 3)
            return GameEndStatus.Draw;

        return GameEndStatus.NotOverYet;
    }

    private void SwapUnits(List<Card> unit1, List<Card> unit2, float duration) {
        float yPosOfUnit1 = unit1.First().transform.localPosition.y;
        float yPosOfUnit2 = unit2.First().transform.localPosition.y;
        
        foreach (Card card in unit1) {
            Vector2 startPos = card.transform.localPosition;
            Vector2 endPos = new Vector2(card.transform.localPosition.x, yPosOfUnit2);
            StartCoroutine(card.MoveFromTo(startPos, endPos, duration));
        }
        
        foreach (Card card in unit2) {
            Vector2 startPos = card.transform.localPosition;
            Vector2 endPos = new Vector2(card.transform.localPosition.x, yPosOfUnit1);
            StartCoroutine(card.MoveFromTo(startPos, endPos, duration));
        }
    }
    
    private IEnumerator DecideFirstTurn() {
        cardDeck.RemoveAllWizards();
        
        Card opponentsCard = cardDeck.GetRandomCard();
        opponentsCard.transform.localPosition = new Vector3(10f, 0f);
        opponentsCard.gameObject.SetActive(true);
        opponentsCard.HideImmediately(true);
        
        Card playersCard = cardDeck.GetRandomCard();
        playersCard.transform.localPosition = opponentsCard.transform.localPosition;
        playersCard.gameObject.SetActive(true);
        playersCard.HideImmediately(true);
        
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        
        yield return wait;
        
        StartCoroutine(opponentsCard.MoveFromTo(opponentsCard.transform.localPosition, new Vector2(0f, 1f), 0.5f));
        StartCoroutine(playersCard.MoveFromTo(playersCard.transform.localPosition, new Vector2(0f, -1f), 0.5f));

        yield return wait;
        
        StartCoroutine(opponentsCard.FlipCard(0.2f));
        StartCoroutine(playersCard.FlipCard(0.2f));

        yield return wait;

        if (opponentsCard.CompareTo(playersCard) < 0) {
            ShowMessage("Player gets first turn!", 1.5f);
            playerHasNextTurn = true;
            yield return new WaitForSeconds(1.5f);
            
            ContinueToNextTurn();
        }
        else if (opponentsCard.CompareTo(playersCard) > 0) {
            ShowMessage("Opponent gets first turn!", 1.5f);
            playerHasNextTurn = false;
            yield return new WaitForSeconds(1.5f);
            
            ContinueToNextTurn();
        }
        else {
            ShowMessage("Draw!", 1.5f);
            yield return new WaitForSeconds(1.5f);
            
            StartCoroutine(DecideFirstTurn());
        }
        
        StartCoroutine(opponentsCard.MoveFromTo(opponentsCard.transform.localPosition, new Vector2(10f, 0f), 0.5f));
        StartCoroutine(playersCard.MoveFromTo(playersCard.transform.localPosition, new Vector2(10f, 0f), 0.5f));

        StartCoroutine(opponentsCard.FlipCard(0.2f));
        StartCoroutine(playersCard.FlipCard(0.2f));
        
        yield return new WaitForSeconds(1f);
        
        CardPool.Instance.ReturnToPool(opponentsCard);
        CardPool.Instance.ReturnToPool(playersCard);
    }

    private IEnumerator OpponentChooseGroupsForBattle() {
        ChangeGameState(GameState.OpponentsTurn);
        WaitForSeconds wait = new WaitForSeconds(1f);

        yield return wait;
        
        CardGroup[] opGroups = CardGroup.NonEmptyOpponentGroups();
        opGroups[Random.Range(0, opGroups.Length)].Select(true);

        yield return wait;

        CardGroup[] plGroups = CardGroup.NonEmptyPlayerGroups();
        plGroups[Random.Range(0, plGroups.Length)].Select(true);

        yield return wait;
        
        GroupsForBattleConfirmed();
    }
    
    private IEnumerator ShowMessageFor(string message, float duration) {
        messageText.text = message;
        yield return new WaitForSeconds(duration);
        messageText.text = string.Empty;
        
        messageCoroutine = null;
    }

    private IEnumerator DealCards() {
        ChangeGameState(GameState.DealingCards);
        
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        
        for (int i = 0; i < cardsToDeal; i++) {
            DealCardTo(Player);
            yield return wait;
            
            DealCardTo(Opponent);
            yield return wait;
        }
        
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(GatherCardsThenLayoutCards());
    }

    private void DealCardTo(PlatoonPlayer platoonPlayer) {
        Card card = cardDeck.GetRandomCard();            
        platoonPlayer.AddCardToHand(card);
            
        card.HideImmediately(true);
        card.gameObject.SetActive(true);
            
        Vector2 startPos = new Vector2(10f, 0f);
        Vector2 endPos = platoonPlayer.playArea.Center + Random.insideUnitCircle * 1.5f;

        StartCoroutine(card.MoveFromTo(startPos, endPos, 0.8f));
    }

    private IEnumerator GatherCardsThenLayoutCards() {
        Vector2[] playerCardPositions = Player.playArea.GetCardLayout(
            cardsToDeal,
            Player.cardsInHand[0].CardWidth,
            0.1f
        );
        Vector2[] opponentCardPositions = Opponent.playArea.GetCardLayout(
            cardsToDeal,
            Opponent.cardsInHand[0].CardWidth,
            0.1f
        );
        
        for (int i = 0; i < cardsToDeal; i++) {
            Card playerCard = Player.cardsInHand[i];
            StartCoroutine(playerCard.MoveFromTo(playerCard.transform.localPosition, playerCardPositions[0], 0.5f));
            
            Card opponentCard = Opponent.cardsInHand[i];
            StartCoroutine(opponentCard.MoveFromTo(opponentCard.transform.localPosition, opponentCardPositions[0], 0.5f));
        }

        yield return new WaitForSeconds(0.5f);

        Player.cardsInHand.Sort();
        
        for (int i = 1; i < cardsToDeal; i++) {
            Card playerCard = Player.cardsInHand[i];
            StartCoroutine(playerCard.MoveFromTo(playerCard.transform.localPosition, playerCardPositions[i], 0.5f));
            
            Card opponentCard = Opponent.cardsInHand[i];
            StartCoroutine(opponentCard.MoveFromTo(opponentCard.transform.localPosition, opponentCardPositions[i], 0.5f));
        }
        
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FlipAllPlayersCards());
    }

    private IEnumerator FlipAllPlayersCards() {
        foreach (Card card in Player.cardsInHand)
            StartCoroutine(card.FlipCard(0.3f));
        
        yield return new WaitForSeconds(0.3f);
        
        ChangeGameState(GameState.PlayerCreatingGroups);
    }
    
    private void ResetGame() {
        if (Player == null)
            Player = new PlatoonPlayer(playerPlayArea, false);
        else
            Player.Initialize();
        
        if (Opponent == null)
            Opponent = new PlatoonPlayer(opponentPlayArea, true);
        else
            Opponent.Initialize();
            
        CardPool.Instance.ReturnAllToPool();
        cardDeck.ResetDeck();
        numRoundsBattled = 0;
        ShowMessage("", 1f);
    }

    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start() {
        StartNewGame();
    }

    private void ChangeGameState(GameState newState) {
        State = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}