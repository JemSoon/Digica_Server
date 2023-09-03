using UnityEngine;
using UnityEngine.UI;

public partial class UIPortrait : MonoBehaviour
{
    public GameObject panel;
    public Image portrait;
    public Text username;
    public Text deckAmount;
    public Text graveyardAmount;
    public Text handAmount;
    public Text tamaAmount;
    public Text health;
    public Text mana;
    public PlayerType playerType;

    private PlayerInfo enemyInfo;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player && player.hasEnemy) enemyInfo = player.enemyInfo;

        if (player && playerType == PlayerType.PLAYER)
        {
            panel.SetActive(true);
            //player.transform.position = portrait.transform.position;
            portrait.sprite = player.portrait;
            username.text = player.username;
            deckAmount.text = player.deck.deckList.Count.ToString();
            graveyardAmount.text = player.deck.graveyard.Count.ToString();
            handAmount.text = player.deck.hand.Count.ToString();
            tamaAmount.text = player.deck.babyCard.Count.ToString();
            //health.text = player.health.ToString();
            //mana.text = player.mana.ToString();
            player.spawnOffset = portrait.transform;
            player.transform.localScale = new Vector3(100, 100, 10);
            player.transform.localPosition = new Vector3(portrait.transform.position.x, portrait.transform.position.y, 0f);
        }
        else if (player && player.hasEnemy && playerType == PlayerType.ENEMY)
        {
            panel.SetActive(true);
            //enemyInfo.player.transform.position = portrait.transform.position;
            portrait.sprite = enemyInfo.portrait;
            username.text = enemyInfo.username;
            deckAmount.text = enemyInfo.deckCount.ToString();
            graveyardAmount.text = enemyInfo.graveCount.ToString();
            handAmount.text = enemyInfo.handCount.ToString();
            tamaAmount.text = enemyInfo.tamaCount.ToString(); //¾È¸¸µê ¾ÆÁ÷
            //health.text = enemyInfo.health.ToString();
            //mana.text = enemyInfo.mana.ToString();
            enemyInfo.data.spawnOffset = portrait.transform;
            enemyInfo.player.transform.localScale = new Vector3(100, 100, 10);
            enemyInfo.player.transform.localPosition = new Vector3(portrait.transform.position.x, portrait.transform.position.y, 0f);
        }
        else
        {
            panel.SetActive(false);
        }
    }
}