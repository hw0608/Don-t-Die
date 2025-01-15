using System;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] VoronoiMapGenerator mapGenerator;
    [SerializeField] NatureResourceData natureResourceData;

    protected virtual void Init()
    {
        // �ӽ���. ���߿� GameManager���� �޾ƿ��� ������ ����.
        mapGenerator = transform.parent.parent.GetComponent<VoronoiMapGenerator>();
    }

    private void Start()
    {
        Init();
    }

    protected void Harvest()
    {
        mapGenerator.objectMap.ClearTiles(new Vector2Int((int)transform.position.x, (int)transform.position.y), natureResourceData.Width, natureResourceData.Height);
        gameObject.SetActive(false);
    }
}
