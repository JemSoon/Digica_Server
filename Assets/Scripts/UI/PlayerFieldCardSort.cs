using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//아군 필드카드 정렬
public class PlayerFieldCardSort : MonoBehaviour
{
    private List<Transform> sortedChildren;

    private void FixedUpdate()
    {
        // FieldCard 컴포넌트가 있는 자식들 중 isUnderMostCard가 true인 것들만 필터링하여 정렬
        sortedChildren = transform.Cast<Transform>()
            .Where(child => child.GetComponent<FieldCard>()?.isUnderMostCard == true)
            .ToList();

        // FieldCard 컴포넌트를 가진 게임 오브젝트가 없을 경우 종료
        if (sortedChildren.Count == 0)
        {
            return;
        }

        // 미들센터 정렬을 위해 x 포지션을 개수 * 50만큼의 위치로 배정
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
