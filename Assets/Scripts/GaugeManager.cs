using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("게이지를 표시할 Image 컴포넌트. Image Type을 'Filled'로 설정해야 합니다.")]
    public Image gaugeImage;
    [Tooltip("기존 슬라이더 방식 (사용하지 않는 경우 비워두세요)")]
    public Slider gaugeSlider;

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
        // Image 방식 (권장)
        if (gaugeImage != null)
        {
            gaugeImage.fillAmount = currentGauge / maxGauge;
        }

        // 기존 Slider 방식
        if (gaugeSlider != null)
        {
            gaugeSlider.maxValue = maxGauge;
            gaugeSlider.value = currentGauge;
        }
    }

    // (참고) 게이지를 다시 채우는 함수가 필요하면 여기에 추가
    // public void RegenerateGauge(float amount)
    // { ... }
}