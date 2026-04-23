using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 카드 슬롯 오브젝트에 붙여서 해당 슬롯의 이미지를 자동으로 업데이트하는 클래스
/// </summary>
[RequireComponent(typeof(Image))]
public class CardSlotUI : MonoBehaviour
{
    [Header("슬롯 설정")]
    public int slotIndex = 0;      // 0 ~ 4번 슬롯 중 어디인지 지정
    public bool isStorage = false; // 보관함 슬롯인지 여부 (기본값은 핸드 슬롯)

    private Image myImage;

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }

    private void LateUpdate()
    {
        if (CardManager.instance == null) return;

        // 1. 데이터 가져오기 (핸드 또는 보관함)
        var cardList = isStorage ? CardManager.instance.storage : CardManager.instance.hand;

        // 2. 해당 인덱스에 카드가 있는지 확인
        bool hasCard = cardList != null && slotIndex < cardList.Count;

        // 3. 이미지 및 오브젝트 상태 업데이트
        // (부모나 본인이 꺼져있지 않을 때만 동작하게 하려면 활성화 체크가 필요할 수 있음)
        if (myImage != null)
        {
            myImage.enabled = hasCard;
            if (hasCard)
            {
                var card = cardList[slotIndex];
                var sprite = CardManager.instance.GetCardSprite(card.suit, card.rank);
                if (sprite != null)
                {
                    myImage.sprite = sprite;
                    myImage.color = Color.white;
                }
            }
        }
    }
}
