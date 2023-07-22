using UnityEngine;
using Mirror;
using System;

public class MemoryChecker : NetworkBehaviour
{
    public static MemoryChecker Inst { get; private set; }
    void Awake() => Inst = this;

    [SyncVar]
    public int memory;

    public void memoryCheckerPos()
    {
        //if (Player.localPlayer != null && Player.localPlayer.isLocalPlayer)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (Player.localPlayer.firstPlayer)
            {
                switch (memory)
                {
                    case 0:
                        rectTransform.anchoredPosition = new Vector2(0, 32);
                        break;
                    case 1:
                        rectTransform.anchoredPosition = new Vector2(-91, 32);
                        break;
                    case 2:
                        rectTransform.anchoredPosition = new Vector2(-91 * 2, 32);
                        break;
                    case 3:
                        rectTransform.anchoredPosition = new Vector2(-91 * 3, 32);
                        break;
                    case 4:
                        rectTransform.anchoredPosition = new Vector2(-91 * 4, 32);
                        break;
                    case 5:
                        rectTransform.anchoredPosition = new Vector2(-91 * 5, 32);
                        break;
                    case 6:
                        rectTransform.anchoredPosition = new Vector2(-91 * 6, 32);
                        break;
                    case 7:
                        rectTransform.anchoredPosition = new Vector2(-91 * 7, 32);
                        break;
                    case 8:
                        rectTransform.anchoredPosition = new Vector2(-91 * 8, 32);
                        break;
                    case 9:
                        rectTransform.anchoredPosition = new Vector2(-91 * 9, 32);
                        break;
                    case 10:
                        rectTransform.anchoredPosition = new Vector2(-91 * 10, 32);
                        break;
                    case -1:
                        rectTransform.anchoredPosition = new Vector2(91, 32);
                        break;
                    case -2:
                        rectTransform.anchoredPosition = new Vector2(91 * 2, 32);
                        break;
                    case -3:
                        rectTransform.anchoredPosition = new Vector2(91 * 3, 32);
                        break;
                    case -4:
                        rectTransform.anchoredPosition = new Vector2(91 * 4, 32);
                        break;
                    case -5:
                        rectTransform.anchoredPosition = new Vector2(91 * 5, 32);
                        break;
                    case -6:
                        rectTransform.anchoredPosition = new Vector2(91 * 6, 32);
                        break;
                    case -7:
                        rectTransform.anchoredPosition = new Vector2(91 * 7, 32);
                        break;
                    case -8:
                        rectTransform.anchoredPosition = new Vector2(91 * 8, 32);
                        break;
                    case -9:
                        rectTransform.anchoredPosition = new Vector2(91 * 9, 32);
                        break;
                    case -10:
                        rectTransform.anchoredPosition = new Vector2(91 * 10, 32);
                        break;
                    default:
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                }
            }

            else
            {
                switch (-memory)
                {
                    case 0:
                        rectTransform.anchoredPosition = new Vector2(0, 32);
                        break;
                    case 1:
                        rectTransform.anchoredPosition = new Vector2(-91, 32);
                        break;
                    case 2:
                        rectTransform.anchoredPosition = new Vector2(-91 * 2, 32);
                        break;
                    case 3:
                        rectTransform.anchoredPosition = new Vector2(-91 * 3, 32);
                        break;
                    case 4:
                        rectTransform.anchoredPosition = new Vector2(-91 * 4, 32);
                        break;
                    case 5:
                        rectTransform.anchoredPosition = new Vector2(-91 * 5, 32);
                        break;
                    case 6:
                        rectTransform.anchoredPosition = new Vector2(-91 * 6, 32);
                        break;
                    case 7:
                        rectTransform.anchoredPosition = new Vector2(-91 * 7, 32);
                        break;
                    case 8:
                        rectTransform.anchoredPosition = new Vector2(-91 * 8, 32);
                        break;
                    case 9:
                        rectTransform.anchoredPosition = new Vector2(-91 * 9, 32);
                        break;
                    case 10:
                        rectTransform.anchoredPosition = new Vector2(-91 * 10, 32);
                        break;
                    case -1:
                        rectTransform.anchoredPosition = new Vector2(91, 32);
                        break;
                    case -2:
                        rectTransform.anchoredPosition = new Vector2(91 * 2, 32);
                        break;
                    case -3:
                        rectTransform.anchoredPosition = new Vector2(91 * 3, 32);
                        break;
                    case -4:
                        rectTransform.anchoredPosition = new Vector2(91 * 4, 32);
                        break;
                    case -5:
                        rectTransform.anchoredPosition = new Vector2(91 * 5, 32);
                        break;
                    case -6:
                        rectTransform.anchoredPosition = new Vector2(91 * 6, 32);
                        break;
                    case -7:
                        rectTransform.anchoredPosition = new Vector2(91 * 7, 32);
                        break;
                    case -8:
                        rectTransform.anchoredPosition = new Vector2(91 * 8, 32);
                        break;
                    case -9:
                        rectTransform.anchoredPosition = new Vector2(91 * 9, 32);
                        break;
                    case -10:
                        rectTransform.anchoredPosition = new Vector2(91 * 10, 32);
                        break;
                    default:
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                }
            }
            //Debug.Log(Player.localPlayer.mana);
        }
    }
}
