using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider gaugeSlider; // 나중에 Image로 바꿀 수 있음
    // public Image gaugeImage; // 나중에 이걸로 바꾸려면 주석 해제

    [Header("Gauge Settings")]
    public float maxGauge = 100f;
    private float currentGauge;

    // 게이지가 0보다 큰지 확인하는 프로퍼티 (읽기 전용)
    public bool HasGauge
    {
        get { return currentGauge > 0; }
    }


        public void ResetGauge()
    {
        currentGauge = maxGauge;
        UpdateUIVisual();
    }

    void Start()
    {
        // 게이지 초기화
        currentGauge = maxGauge;
        UpdateUIVisual();
    }

    // SnowPathDrawer가 이 함수를 호출해서 게이지를 소모시킴
    public void ConsumeGauge(float amount)
    {
        if (currentGauge > 0)
        {
            
            currentGauge -= amount;
            if (currentGauge < 0)
            {
                currentGauge = 0;
            }
            UpdateUIVisual();
        }
    }

    // 게이지 값을 UI에 반영
    private void UpdateUIVisual()
    {
        // 슬라이더가 있다면 슬라이더 값 업데이트
        if (gaugeSlider != null)
        {
            gaugeSlider.maxValue = maxGauge; // (Start에서 한 번만 해도 됨)
            gaugeSlider.value = currentGauge;
        }

        // (나중에 이미지로 바꿀 경우)
        // if (gaugeImage != null)
        // {
        //     gaugeImage.fillAmount = currentGauge / maxGauge;
        // }
    }

    // (참고) 게이지를 다시 채우는 함수가 필요하면 여기에 추가
    // public void RegenerateGauge(float amount)
    // { ... }
}