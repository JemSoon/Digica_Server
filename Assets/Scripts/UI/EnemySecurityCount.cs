using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySecurityCount : MonoBehaviour
{
    public Sprite normalImage;
    public Sprite breakImage;

    public Image Icon1;
    public Image Icon2;
    public Image Icon3;
    public Image Icon4;
    public Image Icon5;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SetIcon();
    }

    private int SecurityCountCheck()
    {
        if (Player.localPlayer != null && Player.localPlayer.hasEnemy)
        {
            return Player.localPlayer.enemyInfo.data.deck.securityCard.Count;
        }

        else { return 0; }
    }

    private void SetIcon()
    {
        switch (SecurityCountCheck())
        {
            case 0:
                Icon1.sprite = breakImage;
                Icon2.sprite = breakImage;
                Icon3.sprite = breakImage;
                Icon4.sprite = breakImage;
                Icon5.sprite = breakImage;
                break;
            case 1:
                Icon1.sprite = breakImage;
                Icon2.sprite = breakImage;
                Icon3.sprite = breakImage;
                Icon4.sprite = breakImage;
                Icon5.sprite = normalImage;
                break;
            case 2:
                Icon1.sprite = breakImage;
                Icon2.sprite = breakImage;
                Icon3.sprite = breakImage;
                Icon4.sprite = normalImage;
                Icon5.sprite = normalImage;
                break;
            case 3:
                Icon1.sprite = breakImage;
                Icon2.sprite = breakImage;
                Icon3.sprite = normalImage;
                Icon4.sprite = normalImage;
                Icon5.sprite = normalImage;
                break;
            case 4:
                Icon1.sprite = breakImage;
                Icon2.sprite = normalImage;
                Icon3.sprite = normalImage;
                Icon4.sprite = normalImage;
                Icon5.sprite = normalImage;
                break;
            case 5:
                Icon1.sprite = normalImage;
                Icon2.sprite = normalImage;
                Icon3.sprite = normalImage;
                Icon4.sprite = normalImage;
                Icon5.sprite = normalImage;
                break;
        }
    }
}
