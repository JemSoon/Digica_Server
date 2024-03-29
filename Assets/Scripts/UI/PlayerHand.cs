using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public GameObject panel;
    public HandCard cardPrefab;
    public Transform handContent;
    public PlayerType playerType;
    private Player player;
    private PlayerInfo enemyInfo;
    private int cardCount = 0; // Amount of cards in hand
    private bool isFirstTime = true;

    void LateUpdate()
    {
        player = Player.localPlayer;
        if (player && player.hasEnemy) enemyInfo = player.enemyInfo;

        if (playerType == PlayerType.PLAYER && isFirstTime && player && player.hasEnemy)
        {
            player.deck.DrawCard(5);
            isFirstTime = false;
        }
        if (IsEnemyHand())
        {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(cardPrefab.gameObject, enemyInfo.handCount, handContent);
            // refresh all members
            for (int i = 0; i < enemyInfo.handCount; ++i)
            {
                HandCard slot = handContent.GetChild(i).GetComponent<HandCard>();

                slot.AddCardBack();

                cardCount = enemyInfo.handCount;
            }
        }
    }

    public void AddCard(int index)
    {
        GameObject cardObj = Instantiate(cardPrefab.gameObject);
        cardObj.transform.SetParent(handContent, false);

        CardInfo card = player.deck.hand[index];
        HandCard slot = cardObj.GetComponent<HandCard>();

        slot.AddCard(card, index, playerType);
    }

    public void RemoveCard(int index)
    {
        for (int i = index; i < handContent.childCount; ++i)
        {
            HandCard slot = handContent.GetChild(i).GetComponent<HandCard>();
            int count = i;
            if (count == index)
            { 
                slot.RemoveCard();
            }
            else if (slot.handIndex > index) 
            { slot.handIndex--; }
        }
    }

    //IsEnemyHand()의 마지막 조건문이 핸드카드 하나 더 삭제되게 하는 문제있는듯..?
    bool IsEnemyHand() => player && player.hasEnemy /*&& player.deck.hand.Count == 7*/ && playerType == PlayerType.ENEMY; /*&& enemyInfo.handCount != cardCount;*/
    bool IsPlayerHand() => player && player.deck.spawnInitialCards && playerType == PlayerType.PLAYER;
}