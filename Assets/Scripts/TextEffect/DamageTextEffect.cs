using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Purchasing;

public class DamageTextEffect : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 100f;   //UI 용으로 더 큰 값
    [SerializeField] private float lifeTime = 1.5f;

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Color originalColor;
    private Vector2 moveDirection;
    private float timer = 0f;

    private bool isCritical = false;
    private bool isStatusEffect = false;
    private bool useGravity = false;
    private float verticalVelocity = 100f;
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (textMesh != null)
        {
            originalColor = textMesh.color;
            float randomX = Random.Range(-0.5f, 0.5f);
            float randomY = useGravity ? Random.Range(0.5f, 1.0f) : Random.Range(0.8f, 1.5f);
            moveDirection = new Vector2(randomX, randomY).normalized;  //랜덤 방향 설정

            if (rectTransform != null)   //랜덤 회전
            {
                rectTransform.rotation = Quaternion.Euler(0, 0, Random.Range(-10.0f, 10.0f));
            }
            if (useGravity)         //초기 수직 속도 (중력 사용 시)
            {
                verticalVelocity = Random.Range(100.0f, 200f);
            }
            StartCoroutine(PunchScale(isCritical ? 1.5f : 1.2f));            //펀치 스케일 효과
            if (isCritical)
            {
                StartCoroutine(FlashText());
                StartCoroutine(CreateFlashEffect());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rectTransform == null) return;

        if (useGravity)
        {
            verticalVelocity = 300f * Time.deltaTime;
            rectTransform.anchoredPosition += new Vector2(0, verticalVelocity + Time.deltaTime);
            rectTransform.anchoredPosition += new Vector2(moveDirection.x * moveSpeed * Time.deltaTime, 0);
        }
        else
        {
            rectTransform.anchoredPosition += (Vector2)(moveDirection * moveSpeed * Time.deltaTime);
        }

        timer += Time.deltaTime;
        if (timer >= lifeTime * 0.5f)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (timer - lifeTime * 0.5f) / (lifeTime * 0.5f));
            }
            else if (textMesh != null)
            {
                float alpha = Mathf.Lerp(originalColor.a, 0f, (timer - lifeTime * 0.5f) / (lifeTime * 0.5f));
                textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            moveSpeed = Mathf.Lerp(moveSpeed, 20f, Time.deltaTime * 2.0f);     //속도 감소

            if ((canvasGroup != null && canvasGroup.alpha <= 0.05f) || (textMesh != null && textMesh.color.a <= 0.05f))   //완전히 투명해지면 파괴
            {
                Destroy(gameObject);
            }
        }
    }

    public void Initialized(bool critical, bool statusEffect)
    {
        isCritical = critical;
        isStatusEffect = statusEffect;

        if (isStatusEffect)
        {
            useGravity = false;
        }

        Start();                       //초기화 즉시 시작
    }

    public void SetVerticalMovement()
    {
        useGravity = false;
    }

    private IEnumerator PunchScale(float intentisy)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * intentisy;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator FlashText()                     //번쩍임 효과 
    {
        if(textMesh == null) yield break;

        Color flashColor = Color.white;
        float flashDuration = 0.2f;

        Color startColor = textMesh.color;                //원래 색상 저장

        textMesh.color = flashColor;                      //번쩍임 색상으로 변경

        yield return new WaitForSeconds(flashDuration);   //대기
    }


    private IEnumerator CreateFlashEffect()  //잔상 효과를 UI용 깜빡임 효과로 사용
    {
        if (textMesh == null) yield break;

        float interval = 0.05f;
        int flashCount = 3;

        for (int i = 0; i < flashCount; i++)
        {
            //알파값 조정으로 깜빡임 구현
            textMesh.alpha = 0.5f;
            yield return new WaitForSeconds(interval);
            textMesh.alpha = 1.0f;
            yield return new WaitForSeconds(interval);
        }
    }
}
