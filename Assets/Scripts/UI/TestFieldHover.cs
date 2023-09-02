
using UnityEngine;

public class TestFieldHover : MonoBehaviour
{
    public GameObject image;
    private float maxX = 1920f - 171.5f;
    private float minX = -171.5f;
    private float maxY = 1080f - 250f;
    private float minY = -250f;

    // Start is called before the first frame update
    void Start()
    {
        SetDontOverPos(); // 나중에 필드에 카드추가되서 필드 업데이트되어 위치가 바뀔 때 이걸 한번 더 호출하게 하면 될듯
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetDontOverPos()
    {
        //Mathf.Clamp함수는 범위 제한 

        Vector3 newPosition = image.transform.position;

        // X 좌표 제한
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Y 좌표 제한
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // 이미지 위치 업데이트
        image.transform.position = newPosition;
    }
}
