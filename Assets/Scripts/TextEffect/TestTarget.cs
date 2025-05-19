using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTarget : MonoBehaviour
{
    [SerializeField] private int minDamage = 5;
    [SerializeField] private int maxDamage = 50;
    [SerializeField] private int minHeal = 10;
    [SerializeField] private int maxHeal = 60;
    [SerializeField] private float criticalChance = 0.2f;
    [SerializeField] private float missChance = 0.1f;
    [SerializeField] private float statusEffectChance = 0.15f;

    //상태 이상 종류 
    private string[] statusEffects = { "Position", "Burn", "Freeze", "Stun", "Blind", "Silence" };

    private void ShowDamage(int amount, bool isCritical)
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowDamage(position, amount, isCritical);
        }
    }

    private void ShowHeal(int amount, bool isCritical)
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowHeal(position, amount, isCritical);
        }
    }

    private void ShowMiss()
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowMiss(position);
        }
    }

    private void ShowStatusEffect(string effectName)
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowStatusEffect(position, effectName);
        }
    }

    private void OnMouseDown()
    {
        float randomValue = Random.value;               //랜덤 값으로 결정

        if (randomValue < missChance)
        {
            ShowMiss();                                  //미스 처리
        }
        else if (randomValue < 0.5f)      //50% 확률로 데미지
        {
            bool isCritical = Random.value < criticalChance;
            int damage = Random.Range(minDamage, maxDamage + 1); //데미지 처리

            if(isCritical) damage *= 2;                     //크리티컬이면 데미지 2배

            ShowDamage(damage, isCritical);

            if (Random.value < statusEffectChance)          //상태 이상 추가 확률
            {
                string statusEffect = statusEffects[Random.Range(0, statusEffects.Length)];
                ShowStatusEffect(statusEffect);
            }
        }
        else
        {
            bool isCritical = Random.value < criticalChance;
            int heal = Random.Range(minHeal, maxHeal + 1);

            if(isCritical) heal = Mathf.RoundToInt(heal * 1.5f);
            ShowHeal(heal, isCritical);
        }
    }
}
