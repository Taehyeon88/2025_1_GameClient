using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas uiCanvas;

    public static DamageEffectManager instance {  get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                Debug.Log("UI 캔버스를 찾을 수 없습니다.");
            }
        }
    }
    public void ShowDamageText(Vector3 position, string text, Color color, bool isCritical = false, bool isStatusEffect = false)
    {
        if (textPrefab == null || uiCanvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        if (screenPos.z < 0) return;

        GameObject damageText = Instantiate(textPrefab, uiCanvas.transform);

        RectTransform rectTransform = damageText.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPos;
        }

        TextMeshProUGUI temp = damageText.GetComponent<TextMeshProUGUI>();          //텍스트 컴포넌트 설정
        if (temp != null)
        {
            temp.text = text;                                                       //텍스트 설정
            temp.color = color;                                                     //색상 설정
            temp.outlineColor = new Color(
                Mathf.Clamp01(color.r - 0.3f),
                Mathf.Clamp01(color.g - 0.3f),
                Mathf.Clamp01(color.b - 0.3f)
                );

            float scale = 1.0f;

            int numbericValue;
            if (int.TryParse(text.Replace("+", "").Replace("CRITI", "").Replace("HEAL CRIT", ""), out numbericValue))
            {
                scale = Mathf.Clamp(numbericValue / 15f, 0.8f, 2.5f);
            }

            if (isCritical) scale = 1.4f;
            if (isStatusEffect) scale *= 0.8f;

            damageText.transform.localScale = new Vector3 (scale, scale, scale);
        }

        DamageTextEffect effect = damageText.GetComponent<DamageTextEffect>();
        if (effect != null)
        {
            effect.Initialized(isCritical, isStatusEffect);
            if (isStatusEffect)
            {
                effect.SetVerticalMovement();
            }
        }
    }

    public void ShowDamage(Vector3 position, int amount, bool isCritical = false)
    {
        string text = amount.ToString();
        Color color = isCritical ? new Color(1.0f, 0.8f, 0.0f) : new Color(1.0f, 0.3f, 0.3f);

        if (isCritical)
        {
            text = "CRITI\n" + text;
        }

        ShowDamageText(position, text, color, isCritical);
    }

    public void ShowHeal(Vector3 position, int amount, bool isCritical = false)
    {
        string text = amount.ToString();
        Color color = isCritical ? new Color(0.4f, 1.0f, 0.4f) : new Color(0.3f, 0.9f, 0.3f);

        if (isCritical)
        {
            text = "HEAL CRITI\n" + text;
        }

        ShowDamageText(position, text, color, isCritical);
    }

    public void ShowMiss(Vector3 position)
    {
        ShowDamageText(position, "Miss", Color.gray, false);
    }

    public void ShowStatusEffect(Vector3 position, string effectName)     //상태 효과 함수
    {
        Color color;

        switch (effectName.ToLower())                                     //상태 효과에 따른 색상 설정
        {
            case "position":
                color = new Color(0.5f, 0.1f, 0.5f);         //보라
                break;
            case "burn":
                color = new Color(1.0f, 0.4f, 0.0f);         //주황
                break;
            case "freeze":
                color = new Color(0.5f, 0.8f, 1.0f);         //하늘
                break;
            case "stun":
                color = new Color(1.0f, 1.0f, 0.0f);         //노란
                break;
            default:
                color = new Color(1.0f, 1.0f, 1.0f);         //기본
                break;
        }

        ShowDamageText(position, effectName.ToLower(), color, true);
    }
}
