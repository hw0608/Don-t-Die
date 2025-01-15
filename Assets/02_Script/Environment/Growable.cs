using System.Net;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class Growable : TimeAgent
{
    [SerializeField] GrowableResourceData data;
    SpriteRenderer spriteRenderer;
    int growStage;

    bool isAllGrown // �ִ�� �����߳�?
    {
        get {
            if (data == null) { return false; }
            return timer >= data.TimeToAllGrown; 
        }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // �ӽ�!!!!!! �׸��� �� Clear �� ���·� �����ؾ� ���� �� ��
        transform.parent.parent.GetComponent<TimeController>().Subscribe(this);
    }

    private void Update()
    {
        
    }

    public override void UpdateTimer()
    {
        if (isAllGrown) return;

        timer++;

        if (timer > data.GrowthStageTime[growStage])
        {
            growStage++;
            spriteRenderer.sprite = data.Sprites[growStage];
        }
    }
}
