using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject {

    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public int Value => value;
    public bool IsPawn => cardType == CardType.Pawn;
    public bool IsJack => cardType == CardType.Jack;
    public bool IsQueen => cardType == CardType.Queen;
    public bool IsBishop => cardType == CardType.Bishop;
    public bool IsKing => cardType == CardType.King;
    public bool IsWizard => cardType == CardType.Wizard;

    public CardType cardType = CardType.Pawn;
    
    [SerializeField]
    private Sprite frontSprite = default;
    [SerializeField]
    private Sprite backSprite = default;
    [SerializeField]
    private int value = default;
}