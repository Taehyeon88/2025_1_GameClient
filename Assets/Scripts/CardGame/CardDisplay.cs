using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    private bool isDragging = false;
    private Vector3 originalPosition;            //드래그 전 위치

    //레이어 마스크
    public LayerMask enemyLayer;
    public LayerMask playerLayer;
    void Start()
    {
        //레이어 마스크 설정
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");


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

        //레이캐스트로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 편정 지역 변수
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            //적에게 공격효과 적용
            CharacterStates enemyStates = hit.collider.GetComponent<CharacterStates>();

            if (enemyStates != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    //공격카드면 데미지 추가
                    enemyStates.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}카드로 적에게 {cardData.effectAmount}데미지를 입혔습니다");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
                }
            }
        }
        else if(Physics.Raycast(ray,out hit, Mathf.Infinity, playerLayer))
        {
            //플레이어에게 힐 효과 적용
            CharacterStates playerStates = hit.collider.GetComponent<CharacterStates>();
            if (playerStates != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    //힐카드면 회복
                    playerStates.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다!");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
                }
            }
        }

        //카드를 사용하지 않았다면 원래 위치로 되돌리기
        if (!cardUsed)
        {
            transform.position = originalPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
