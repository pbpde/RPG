using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    TextMeshProUGUI levelText;
    Image healthSlider;
    Image expSlider;
    private void Awake()
    {
        levelText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        levelText.text = "Level" + GameManager.Instance.playerStates.characterData.currentLevel.ToString("00");
        UpdateExp();
        UpdateHealth();
    }

    void UpdateHealth()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.currentHealth / GameManager.Instance.playerStates.maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }
    void UpdateExp()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.characterData.currentExp / GameManager.Instance.playerStates.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }
}
