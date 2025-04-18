using UnityEngine;
using UnityEngine.UI;

public class StatusUIManager : MonoBehaviour
{
    #region SINGLETON
    private static StatusUIManager instance;
    public  static StatusUIManager Instance
    {
        get
        {
            return instance;
        }
    }

    void SingletonInitialize()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    public PlayerStatus playerStatus; // 인스펙터에서 할당

    private Image _healthGauge;
    private Image _hungryGauge;
    private Image _thirstyGauge;

    [Range(0f, 1f)]
    public float Temperature = 1f;

    public Gradient Gradient;

    void Awake()
    {
        SingletonInitialize();

        _healthGauge      = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        _hungryGauge      = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        _thirstyGauge     = transform.GetChild(2).GetChild(0).GetComponent<Image>();

        //playerStatus = GameManager.Instance.PlayerTransform.GetComponent<PlayerStatus>();
    }

    public void UpdateHealthUI()
    {
        _healthGauge.fillAmount = playerStatus._currentHealthPoint / playerStatus._maxHealthPoint;
    }


    public void UpdateHungryUI()
    {
        _hungryGauge.fillAmount = playerStatus._currentHungryPoint / playerStatus._maxHungryPoint;
    }

    public void UpdateThirstyUI()
    {
        _thirstyGauge.fillAmount = playerStatus._currentThirstyPoint / playerStatus._maxThirstyPoint;
    }
}
