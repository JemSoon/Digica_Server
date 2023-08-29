using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//�Ʊ� �ʵ�ī�� ����
public class PlayerFieldCardSort : MonoBehaviour
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
        float totalWidth = sortedChildren.Count * 138;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < sortedChildren.Count; i++)
        {
            Transform child = sortedChildren[i];
            Vector3 newPosition = new Vector3(startX + i * 172f, -60f, child.localPosition.z);
            child.localPosition = newPosition;
        }
    }
}
