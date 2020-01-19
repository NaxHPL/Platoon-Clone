using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Deck : ScriptableObject {

    [HideInInspector]
    public List<CardData> cardsInDeck;
    
    [SerializeField]
    private CardData[] initialCards = default;

    public void ResetDeck() {
        if (cardsInDeck == null)
            cardsInDeck = new List<CardData>();
        
        cardsInDeck.Clear();
        cardsInDeck.AddRange(initialCards);
    }

    public Card GetRandomCard() {
        int index = Random.Range(0, cardsInDeck.Count);
        Card card = CardPool.Instance.Get(cardsInDeck[index]);
        cardsInDeck.Remove(card.cardData);
        
        return card;
    }

    public void RemoveAllWizards() {
        cardsInDeck.RemoveAll(card => card.IsWizard);
    }

    private void Awake() {
        ResetDeck();
    }
}