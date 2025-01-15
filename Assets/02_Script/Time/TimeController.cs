using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeController : MonoBehaviour
{
    const float secondsInDay = 86400f;

    [SerializeField] AnimationCurve[] timeCurves;

    [SerializeField] float timeScale = 180f; // �Ϸ簡 8���� �Ƿ��� realtime���� 180�� ����� ��
    [SerializeField] float startAtTime = 28800f; // 8am
    [SerializeField] Light2D globalLight;

    List<TimeAgent> timeAgents;
    int season;
    int days;
    float time;

    float agentTimer;
    float agentInterval = 3600f; // ���� ���� 1�ð����� �� ���� ������Ʈ���� ��ȣ ����

    //Debug
    [SerializeField] TMP_Text timeDisplayer;

    private void Awake()
    {
        timeAgents = new List<TimeAgent>();
    }

    void Start()
    {
        time = startAtTime;
        agentTimer = 0f;
    }

    float Hours
    {
        get { return time / 3600f; }
    }

    float Minutes
    {
        get { return time % 3600f / 60f; }
    }
    
    void Update()
    {
        time += Time.deltaTime * timeScale;
        agentTimer += Time.deltaTime * timeScale;
        DisplayTime();
        ControlLight();

        if (time > secondsInDay)
        {
            NextDay();
        }

        if (agentTimer >= agentInterval)
        {
            SignalToTimeAgents();
            agentTimer = 0f;
        }
    }

    void SignalToTimeAgents()
    {
        foreach(TimeAgent agent in timeAgents)
        {
            agent.InvokeOnTimeTick();
        }
    }

    public void Subscribe(TimeAgent agent)
    {
        timeAgents.Add(agent);
    }

    public void Unsubscribe(TimeAgent agent)
    {
        timeAgents.Remove(agent);
    }

    void DisplayTime()
    {
        int h = (int)Hours;
        int m = (int)Minutes;
        timeDisplayer.text = $"{h.ToString("00")} : {m.ToString("00")}";
    } 

    void ControlLight()
    {
        float v = timeCurves[season].Evaluate(Hours);
        globalLight.intensity = v;
    }

    void NextDay()
    {
        time = 0;
        days++;
    }
}
