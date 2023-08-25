using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LayOutOrder : MonoBehaviour
{
    private List<Transform> sortedChildren;

    private void FixedUpdate()
    {
        // FieldCard ������Ʈ�� �ִ� �ڽĵ� �� isUnderMostCard�� true�� �͵鸸 ���͸��Ͽ� ����
        sortedChildren = transform.Cast<Transform>()
            .Where(child => child.GetComponent<FieldCard>()?.isUnderMostCard == true)
            .ToList();

        // FieldCard ������Ʈ�� ���� ���� ������Ʈ�� ���� ��� ����
        if (sortedChildren.Count == 0)
        {
            return;
        }

        // �̵鼾�� ������ ���� x �������� ���� * 50��ŭ�� ��ġ�� ����
        float totalWidth = sortedChildren.Count * 172f;
        //float startX = -totalWidth / 2f;
        for (int i = 0; i < sortedChildren.Count; i++)
        {
            Transform child = sortedChildren[i];
            Vector3 newPosition = new Vector3(0f + i * 172f, child.localPosition.y, child.localPosition.z);
            child.localPosition = newPosition;
        }
    }
}