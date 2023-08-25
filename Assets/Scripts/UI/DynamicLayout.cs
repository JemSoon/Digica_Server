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
        // �ڽ� �� FieldCard ������Ʈ�� ���� �ڽĵ� �� isUpperMost�� true�� �͵鸸 ���͸�
        Transform[] filteredChildren = transform.Cast<Transform>()
            .Where(child =>
            {
                FieldCard fieldCard = child.GetComponent<FieldCard>();
                return fieldCard != null && fieldCard.isUnderMostCard;
            })
            .ToArray();

        // ����
        var sortedChildren = filteredChildren.OrderBy(child => child.position.x).ToArray();

        // �ڽĵ��� ���� ������Ʈ
        for (int i = sortedChildren.Length-1; i > -1; i--)
        {
            sortedChildren[i].SetSiblingIndex(i);
        }

        // ����� ���̾ƿ� ��� �籸��
        LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayoutGroup.GetComponent<RectTransform>());
    }
}