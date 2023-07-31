// Put all our cards in the Resources folder
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct CardAndAmount
{
    public string cardID;
    public int amount;
}

//public static class CustomReadWriteFunctions //CardAndAmount 스트럭트안의 ScriptableCard안의 Sprite가 직렬화 안되기땜에 만든 클래스(연산자 재정의 같은 클래스)
//{
//    public static void WriteCardAndAmount(this NetworkWriter writer, CardAndAmount value)
//    {
//        writer.WriteString(value.cardID);
//        writer.WriteInt(value.amount);
//    }

//    public static CardAndAmount ReadCardAndAmount(this NetworkReader reader)
//    {
//        CardAndAmount result = new CardAndAmount();
//        result.cardID = reader.ReadString();
//        result.amount = reader.ReadInt();
//        return result;
//    }

//    public static void WriteScriptableCard(this NetworkWriter writer, ScriptableCard value)
//    {
//        // ScriptableCard의 고유 ID를 쓰도록 변경합니다.
//        writer.WriteString(value.CardID);
//    }

//    public static ScriptableCard ReadScriptableCard(this NetworkReader reader)
//    {
//        // ScriptableCard의 고유 ID를 읽어와서 해당 ID에 해당하는 ScriptableCard를 찾아 반환합니다.
//        string cardID = reader.ReadString();
//        if (ScriptableCard.Cache.TryGetValue(cardID, out ScriptableCard card))
//        {
//            return card;
//        }
//        else
//        {
//            Debug.LogError("ScriptableCard not found in cache for ID: " + cardID);
//            return null;
//        }
//    }
//}

[System.Serializable]
public class CardAndAmountListWrapper
{
    public List<CardAndAmount> deckList;
}

public enum CardColor1
{ Red, Green, Blue, Yellow, Purple, Black, }
public enum CardColor2
{ None, Red, Green, Blue, Yellow, Purple, Black, }

// Struct for cards in your deck. Card + amount (ex : Sinister Strike x3). Used for Deck Building. Probably won't use it, just add amount to Card struct instead.
public partial class ScriptableCard : ScriptableObject
{
    [SerializeField] string id = "";
    public string CardID { get { return id; } }

    [Header("Image")]
    public Sprite image; // Card image

    [Header("Card Color")]
    public CardColor1 color1;
    public CardColor2 color2;

    [Header("Properties")]
    public string cardName;
    public int cost;
    public string category;

    [Header("Initiative Abilities")]
    public List<CardAbility> intiatives = new List<CardAbility>();

    [HideInInspector] public bool hasInitiative = false; // If our card has an INITIATIVE ability

    [Header("Description")]
    [SerializeField, TextArea(1, 30)] public string description;

    // We can't pass ScriptableCards over the Network, but we can pass uniqueIDs.
    // Throughout this project, you'll find that I've passed uniqueIDs through the Server,
    static Dictionary<string, ScriptableCard> _cache;
    public static Dictionary<string, ScriptableCard> Cache
    {
        get
        {
            if (_cache == null)
            {
                // Load all ScriptableCards from our Resources folder
                ScriptableCard[] cards = Resources.LoadAll<ScriptableCard>("");

                _cache = cards.ToDictionary(card => card.CardID, card => card);
            }
            return _cache;
        }
    }

    // Called when casting abilities or spells
    public virtual void Cast(Entity caster, Entity target)
    {

    }

    private void OnValidate()
    {
        // Get a unique identifier from the asset's unique 'Asset Path' (ex : Resources/Weapons/Sword.asset)
        // You're free to set your own uniqueIDs instead of using this current system, but unless
        // you know what you're doing, I wouldn't recommend changing this in the inspector.
        // If you do change it and want to change back, just erase the uniqueID in the inspector and it will refill itself.
        if (CardID == "")
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            id = AssetDatabase.AssetPathToGUID(path);
#endif
        }

        if (intiatives.Count > 0) hasInitiative = true;
    }
}