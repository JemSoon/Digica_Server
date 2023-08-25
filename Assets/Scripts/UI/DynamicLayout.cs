using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class DynamicLayout : MonoBehaviour
{
    private HorizontalLayoutGroup horizontalLayoutGroup;

    private void Start()
    {
        horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        ApplySorting();
    }

    private void OnTransformChildrenChanged()
    {
        ApplySorting();
    }

    private void ApplySorting()
    {
        // 자식 중 FieldCard 컴포넌트를 가진 자식들 중 isUpperMost가 true인 것들만 필터링
        Transform[] filteredChildren = transform.Cast<Transform>()
            .Where(child =>
            {
                FieldCard fieldCard = child.GetComponent<FieldCard>();
                return fieldCard != null && fieldCard.isUnderMostCard;
            })
            .ToArray();

        // 정렬
        var sortedChildren = filteredChildren.OrderBy(child => child.position.x).ToArray();

        // 자식들의 순서 업데이트
        for (int i = sortedChildren.Length-1; i > -1; i--)
        {
            sortedChildren[i].SetSiblingIndex(i);
        }

        // 변경된 레이아웃 즉시 재구성
        LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayoutGroup.GetComponent<RectTransform>());
    }
}