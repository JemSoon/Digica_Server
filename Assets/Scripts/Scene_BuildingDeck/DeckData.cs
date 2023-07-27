using UnityEngine;

[CreateAssetMenu(fileName = "DeckData", menuName = "Custom/Deck Data")]
public class DeckData : ScriptableObject
{
    public CardAndAmount[] deck;
}