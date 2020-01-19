using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatoonPlayer {

    public List<Card> cardsInHand;
    public PlayArea playArea;
    public bool isOpponent;
    public Card cardSelected;
    public CardGroup cardGroupSelectedForBattle;
    public int numberOfWins;
    
    public PlatoonPlayer(PlayArea playArea, bool isOpponent) {
        cardsInHand = new List<Card>();
        this.playArea = playArea;
        this.isOpponent = isOpponent;
        numberOfWins = 0;
    }

    public void Initialize() {
        cardsInHand.Clear();
        cardSelected = null;
        cardGroupSelectedForBattle = null;
        numberOfWins = 0;
    }
    
    public void AddCardToHand(Card card) {
        cardsInHand.Add(card);
        card.owner = this;
    }
    
    public void RemoveCardFromHand(Card card) {
        cardsInHand.Remove(card);
    }

    public Dictionary<CardGroup, List<Card>> RandomlyAssignCardsToGroups() {
        Dictionary<CardGroup, List<Card>> groupsOfCards = new Dictionary<CardGroup, List<Card>>();
        CardGroup[] groups = isOpponent ? CardGroup.OpponentGroups() : CardGroup.PlayerGroups();
        List<Card> copyOfCardsInHand = cardsInHand.ToList();
        
        foreach (CardGroup group in groups) {
            groupsOfCards.Add(group, new List<Card>());
            
            List<Card> cardsNotWizard = copyOfCardsInHand.Where(card => !card.cardData.IsWizard).ToList();
            Card randomCard = cardsNotWizard[Random.Range(0, cardsNotWizard.Count)];
        
            groupsOfCards[group].Add(randomCard);
            copyOfCardsInHand.Remove(randomCard);
        }

        foreach (Card card in copyOfCardsInHand) {
            CardGroup randomGroup;

            do {
                randomGroup = groups[Random.Range(0, groups.Length)];
            } while (!CardGroup.IsValidForGroup(groupsOfCards[randomGroup], card).Item1);
            
            groupsOfCards[randomGroup].Add(card);
        }
        
        return groupsOfCards;
    }
}