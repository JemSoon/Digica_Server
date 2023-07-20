using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryChecker : MonoBehaviour
{
    public static MemoryChecker Inst { get; private set; }
    void Awake() => Inst = this;

    public void memoryCheckerPos()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        switch (Player.localPlayer.mana)
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
}
