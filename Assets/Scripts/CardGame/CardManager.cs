using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> deckCards = new List<CardData>();     //덱에 있는 카드
    public List<CardData> handCards = new List<CardData>();     //손에 있는 카드
    public List<CardData> discardCards = new List<CardData>();  //버린 카드 더미

    public GameObject cardPrefab;       //카드 프리팹
    public Transform deckPosition;      //덱 위치
    public Transform handPosition;      //손 중앙 위치
    public Transform discardPosition;   //버린 카드더미 위치

    public List<GameObject> cardObjects = new List<GameObject>();     //실제 카드 게임 오브젝트들
    void Start()
    {
        ShuffleDeck();       //시작 시 카드 섞기
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"현재덱카드개수 : {deckCards.Count}");

        if (Input.GetKeyDown(KeyCode.D))     //D 키를 누르면 카드 드로우
        {
            DrawCard();
        }

        if (Input.GetKeyDown(KeyCode.F))     //F 키를 누르면 버린 카드를 덱으로 되돌리고 섞기
        {
            ReturnDiscardsToDeck();
        }

        ArrangeHand();
    }

    //덱 섞기
    public void ShuffleDeck()
    {
        //임시 리스트에 카드 복사
        List<CardData> tempDeck = new List<CardData>(deckCards);
        deckCards.Clear();

        //랜덤하게 섞기
        while (tempDeck.Count > 0)
        {
            int randIndex = Random.Range(0, tempDeck.Count);
            deckCards.Add(tempDeck[randIndex]);
            tempDeck.RemoveAt(randIndex);
        }

        Debug.Log("덱을 섞었습니다. : " + deckCards.Count + "장");
    }

    //카드 드로우
    public void DrawCard()
    {
        if (handCards.Count >= 6)
        {
            Debug.Log("손패가 가득 찼습니다. ! (최대 6장)");
            return;
        }

        if (deckCards.Count == 0)
        {
            Debug.Log("덱에 카드가 없습니다.");
            return;
        }
        //덱에서 맨 위 카드 가져오기
        CardData cardData = deckCards[0];
        deckCards.RemoveAt(0);

        //손패에 추가
        handCards.Add(cardData);

        //카드 게임 오브젝트 생성
        GameObject cardObj = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);

        //카드 정보 설정
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();

        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            cardDisplay.cardIndex = handCards.Count - 1;
            cardObjects.Add(cardObj);
        }

        //손패 위치 업데이트
        ArrangeHand();

        Debug.Log("카드를 드로우 했습니다. : " + cardData.cardName + "(손패 : " + handCards.Count + "/6");
    }

    public void ArrangeHand()  //손에 있는 카드 재정렬
    {
        if (handCards.Count == 0) return;

        //손패 배치를 위한 변수
        float cardWidth = 1.2f;
        float spacing = cardWidth + 1.8f;
        float totalWidth = (handCards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        //각 카드위치 설정
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i] != null)
            {
                //드래그 주인 카드는 건너뛰기
                CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
                if (display != null && display.isDragging)
                    continue;

                //목표 위치 게산
                Vector3 targetPosition = handPosition.position + new Vector3(startX + (i * spacing), 0, 0);

                //부드러운 이동
                cardObjects[i].transform.position = Vector3.Lerp(cardObjects[i].transform.position, targetPosition, Time.deltaTime * 10f);
            }
        }
    }

    public void DiscardCard(int handIndex)   //카드 버리기
    {
        if (handIndex < 0 || handIndex >= handCards.Count)
        {
            Debug.Log("유효하지 않은 카드 인덱스 입니다.");
            return;
        }

        //손패에서 카드 가져오기
        CardData cardData = handCards[handIndex];
        handCards.RemoveAt(handIndex);
        
        //버린 카드 더미에서 추가
        discardCards.Add(cardData);

        //해당 카드 게임 오브젝트 제거
        if (handIndex < cardObjects.Count)
        {
            Destroy(cardObjects[handIndex]);
            cardObjects.RemoveAt(handIndex);
        }

        //카드 인덱스 재설정
        for (int i = 0; i < cardObjects.Count; i++)  //카드 인덱스 재설정
        {
            CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
            if(display != null) display.cardIndex = i;
        }

        ArrangeHand();                  //손패 위치 업데이트
        Debug.Log("카드를 버렸습니다. " + cardData.cardName);
    }

    //버린 카드를 덱에 되돌리고 섞기
    public void ReturnDiscardsToDeck()
    {
        if (discardCards.Count == 0)
        {
            Debug.Log("버린 카드 더미가 비여 있습니다");
            return;
        }

        deckCards.AddRange(discardCards);      //버린 카드를 모두 덱에 추가
        discardCards.Clear();                  //버린 카드 더미 비우기
        ShuffleDeck();                         //덱 섞기

        Debug.Log("버린 카드" +  deckCards.Count + "장을 덱으로 되돌리고 섞었습니다.");
    }
}
