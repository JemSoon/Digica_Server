
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
        SetDontOverPos(); // ���߿� �ʵ忡 ī���߰��Ǽ� �ʵ� ������Ʈ�Ǿ� ��ġ�� �ٲ� �� �̰� �ѹ� �� ȣ���ϰ� �ϸ� �ɵ�
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetDontOverPos()
    {
        //Mathf.Clamp�Լ��� ���� ���� 

        Vector3 newPosition = image.transform.position;

        // X ��ǥ ����
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Y ��ǥ ����
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // �̹��� ��ġ ������Ʈ
        image.transform.position = newPosition;
    }
}
