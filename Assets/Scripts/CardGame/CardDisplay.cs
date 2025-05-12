using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using static UnityEngine.UI.Image;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;                    //카드 데이터
    public int cardIndex;                        //손패에서의 인덱스(나중에 사용)

    //3D 카드요소
    public MeshRenderer cardRederer;             //카드 렌더러(icon or 일러스트)
    public TextMeshPro nameText;                 //이름 텍스트
    public TextMeshPro costText;                 //비용 텍스트
    public TextMeshPro attackText;               //공격력/효과 텍스트
    public TextMeshPro descriptionText;          //설명 텍스트

    //카드 상태
    public bool isDragging = false;
    private Vector3 originalPosition;            //드래그 전 위치

    //레이어 마스크
    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    private CardManager cardManager;            //카드 매니저 참조 추가
    void Start()
    {
        //레이어 마스크 설정
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        cardManager = FindObjectOfType<CardManager>();

        SetupCard(cardData);
    }

    //카드 데이터 설정
    public void SetupCard(CardData data)
    {
        cardData = data;

        //3D 텍스트 업데이트
        if(nameText != null ) nameText.text = data.cardName;
        if(costText != null ) costText.text = data.manaCost.ToString();
        if(attackText != null ) attackText.text = data.effectAmount.ToString();
        if(descriptionText != null ) descriptionText.text = data.description;

        //카드 텍스쳐 설정
        if (cardRederer != null && data.artwork != null)
        {
            Material cardMaterial = cardRederer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

        //SetUpCard 메서드에서 카드 설명 텍스트에 추가 효과 설명 추가
        if (descriptionText != null)
        {
            descriptionText.text = data.description + data.GetAdditionalEffectsDescription();
        }
    }

    private void OnMouseDown()
    {
        //드래스 시작 시 원래 위치 저장
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            //마우스 위치로 카드 이동
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        //버린 카드 더미 근처 드롭 했는지 검사 (마나 체크전)
        if (cardManager != null)
        {
            float disToDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);

            if (disToDiscard < 2.0f)
            {
                cardManager.DiscardCard(cardIndex);  //마나 소모 없이 카드 버리기
                return;
            }
        }

        //여기서 부터 카드 사용 로직(마나 체크)
        CharacterStates playerStates = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerStates = playerObj.GetComponent<CharacterStates>();
        }
        if (playerStates == null || playerStates.currentMana < cardData.manaCost)
        {
            Debug.Log($"마나가 부족합니다! (필요 : {cardData.manaCost}, 현재 : {playerStates?.currentMana ?? 0})");
            transform.position = originalPosition;
            return;
        }

        //레이캐스터로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정 지역 변수
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStates enemyStates = hit.collider.GetComponent<CharacterStates>();

            if (enemyStates != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStates.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if (playerStates != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    playerStates.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}카드로 플레이어의 체력을 {cardData.effectAmount}회복 했습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
            }
        }

        if (!cardUsed)              //카드를 사용하지 않았다면 원래 위치로 되돌리기
        {
            transform.position = originalPosition;
            if (cardManager != null)
                cardManager.ArrangeHand();
            return;
        }

        //카드 사용 시 마나 소모
        playerStates.UseMana(cardData.manaCost);
        Debug.Log($"마나를 {cardData.manaCost} 사용했습니다. (남은 마나 : {playerStates.currentMana})");

        //추가 효과가 있는 경우 처리
        if (cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
        {
            ProcessAdditionalEffectsAndDiscard();      //추가 효과 적용
        }
        else
        {
            if(cardManager != null)
                cardManager.DiscardCard(cardIndex);
        }
    }

    private void ProcessAdditionalEffectsAndDiscard()
    {
        //카드 데이터 및 인덱스 보존
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        //추가 효과 적용
        foreach (var effect in cardDataCopy.additionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:
                    
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (cardManager != null)
                        {
                            cardManager.DrawCard();
                        }
                    }
                    Debug.Log($"{effect.effectAmount}장의 카드를 드로우 했습니다.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:          //카드 버리기 구현 (랜덤 버리기)
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (cardManager != null && cardManager.handCards.Count > 0)
                        {
                            int randomIndex = Random.Range(0, cardManager.handCards.Count);  //손패 크기 기준으로 랜덤 인덱스 생성

                            Debug.Log($"랜덤 카드 버리기 : 선택된 인덱스 {randomIndex}, 현재 손패 크기 : {cardManager.handCards.Count}");

                            if (cardIndexCopy < cardManager.handCards.Count)
                            {
                                if (randomIndex != cardIndexCopy)                 //카드 인덱스 예외처리(주의)
                                {
                                    cardManager.DiscardCard(randomIndex);

                                    //만약 버린 카드의 인덱스가 현재 카드의 인덱스보다 작다면 현재 카드의 인덱스를 1 감소 시켜야 한다.
                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                                else if(cardManager.handCards.Count > 1)
                                {
                                    //다른 카드 선택
                                    int newIndex = (randomIndex + 1)% cardManager.handCards.Count;
                                    cardManager.DiscardCard(newIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                            }
                            else
                            {
                                //cardIndexCopy가 유효하지 않을 경우, 아무 카드 버림
                                cardManager.DiscardCard(randomIndex);
                            }
                        }
                    }
                    Debug.Log($"랜덤으로 {effect.effectAmount}장의 카드를 버렸습니다.");
                    break;

                case CardData.AdditionalEffectType.GainMana:   //플레이어 마나 획득
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");  //태그로 플레이어 캐릭터 찾기
                    if (playerObj != null)
                    {
                        CharacterStates playerStates = playerObj.GetComponent<CharacterStates>();
                        if (playerStates != null)
                        {
                            playerStates.GainMana(effect.effectAmount);
                            Debug.Log($"마나를 {effect.effectAmount} 획득 했습니다.(현재 마나 : {playerStates.currentMana})");
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:   //적 마나 감소
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  //태그로 적찾기
                    foreach (GameObject enemy in enemies)
                    {
                        if (enemy != null)
                        {
                            CharacterStates enemyStates = enemy.GetComponent<CharacterStates>();
                            if (enemyStates != null)
                            {
                                enemyStates.UseMana(effect.effectAmount);
                                Debug.Log($"마나를 {enemyStates.characterName}의 마나를 {effect.effectAmount} 감소 시켰습니다.");
                            }

                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceCardCost:      //다음 카드 비용 감소 효과 (시각적으로만 보여줌 실제 감소는 없음)
                    for (int i = 0; i < cardManager.cardObjects.Count; i++)
                    {
                        CardDisplay display = cardManager.cardObjects[i].GetComponent<CardDisplay>();
                        if (display != null && display != this)             //현재 카드 제외
                        {
                            TextMeshPro costText = display.costText;
                            if (costText != null)
                            {
                                int originalCost = display.cardData.manaCost;
                                int newCost = Mathf.Max(0, originalCost - effect.effectAmount);
                                costText.text = newCost.ToString();
                                costText.color = Color.green;               //감소된 효과는 녹색으로 표시
                            }
                        }
                    }
                    break;
            }
        }

        //효과 적용 후 현재 카드 버리기
        if(cardManager != null)
            cardManager.DiscardCard(cardIndexCopy);
    }
}
