//Learn more : https://mirror-networking.com/docs/Guides/DataTypes.html#scriptable-objects
using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public partial struct CardInfo
{
    // A uniqueID (unique identifier) used to help identify which ScriptableCard is which when we acess ScriptableCard data.
    // If any ScriptableCards share the same uniqueID, Unity will return a bunch of errors.
    public string cardID;
    public int amount; // Used for deck building only. Serves no purpose once the card is in the game / on the board.

    public CardInfo(ScriptableCard data, int amount = 1)
    {
        cardID = data.CardID;
        this.amount = amount;
    }

    public ScriptableCard data
    {
        get
        {
            // Return ScriptableCard from our cached list, based on the card's uniqueID.
            return ScriptableCard.Cache[cardID];
        }
    }

    public Sprite image => data.image;
    public string name => data.name; // Scriptable Card name (name of the file)
    public int cost => data.cost;
    public string description => data.description;

    public List<Target> acceptableTargets => ((CreatureCard)data).acceptableTargets;

    //public List<Target> acceptableTargets
    //{
    //    get
    //    {
    //        if (data is CreatureCard creatureCard)
    //        {
    //            // data가 CreatureCard인 경우
    //            return creatureCard.acceptableTargets;
    //        }
    //        else
    //        {
    //            // data가 CreatureCard가 아닌 경우 (예: SpellCard)
    //            return null;
    //        }
    //    }
    //}

    //#region Equals
    ////=========== "=="연산자 용 추가 함수 ===========//
    //public bool Equals(CardInfo other)
    //{
    //    return name == other.name && image == other.image;
    //}

    //public override bool Equals(object obj)
    //{
    //    return obj is CardInfo other && Equals(other);
    //}

    //public override int GetHashCode()
    //{
    //    unchecked
    //    {
    //        int hashCode = name != null ? name.GetHashCode() : 0;
    //        hashCode = (hashCode * 397) ^ (image != null ? image.GetHashCode() : 0);
    //        return hashCode;
    //    }
    //}
    //#endregion
}

// Card List
public class SyncListCard : SyncList<CardInfo> { }