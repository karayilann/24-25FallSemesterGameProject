// CardBehaviours.cs

using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using TMPro;
using UnityEngine;

public class CardBehaviours : MonoBehaviour, IInteractable
{
    public CardContainer cardContainer;
    public RectTransform cardTransform;
    private List<Transform> _list;
    private int _index;
    private CardType _cardType;
    public CardType CardType => _cardType;
    
    public TextMeshProUGUI cardText;
    public DragAndDrop dragAndDrop;
    
    private bool _isInitialized = false;
    private CardBehaviours _selectedCard;
    
    private void Start()
    {
        cardContainer = FindObjectOfType<CardContainer>();
        if (cardContainer == null)
        {
            Debug.LogError("CardContainer is null");
            return;
        }
        _list = cardContainer.cardPositions;
        dragAndDrop.canDrag = true;
    }

    public void SetCardType(CardType cardType)
    {
        _cardType = cardType;
    }
    
    public void Initialize()
    {
        if (_isInitialized) return;
        cardText.text = _cardType.ToString();
        _isInitialized = true; 
    }
    
    public void CheckCard(CardBehaviours card)
    {
        if (!card.dragAndDrop.canDrag) return; // Zaten işlenmiş kartı kontrol etme
        if (card.CardType == CardType && card != this) // Kendisiyle eşleşmeyi önle
        {
            _selectedCard = card;
            CorrectCard(card);
        }
    }

    private void CorrectCard(CardBehaviours card)
    {
        if (!card.dragAndDrop.canDrag) return; // Çift işlemi önle
        
        // Kartın orijinal pozisyonunu kaydet
        Vector3 targetPos = transform.position;
        Transform targetParent = transform.parent;
        
        // Kartı yeni parent'a ata ve pozisyonla
        card.transform.SetParent(targetParent);
        card.transform.position = targetPos;
        
        // Offset uygula
        Vector3 stackOffset = new Vector3(0, 0, -0.1f * targetParent.childCount);
        card.cardTransform.localPosition = stackOffset;

        // Kart durumunu güncelle
        card.dragAndDrop.isProccessed = true;
        card.dragAndDrop.canDrag = false;
        card.cardContainer._cardStatus[card._index] = true;
        
        Debug.Log($"Eşleşme yapıldı - Kart: {card.CardType}, Parent: {targetParent.name}");
        
        // 3'lü eşleşme kontrolü
        if (targetParent.childCount == 3)
        {
            Debug.Log("3'lü eşleşme yapıldı");
            MoveToCorrectMatchZone();
        }
    }

    private void MoveToCorrectMatchZone()
    {
        Transform currentParent = transform.parent;
        Vector3 targetPosition = new Vector3(166.710007f, -195.842422f, 90);
        
        // Önce tüm child kartları taşı
        for (int i = currentParent.childCount - 1; i >= 0; i--)
        {
            Transform child = currentParent.GetChild(i);
            child.position = targetPosition + new Vector3(0, 0, -0.1f * i);
            child.SetParent(null);
            
            // Kart bileşenlerini devre dışı bırak
            var cardBehaviour = child.GetComponent<CardBehaviours>();
            if (cardBehaviour != null)
            {
                cardBehaviour.dragAndDrop.canDrag = false;
                cardBehaviour.dragAndDrop.isProccessed = true;
            }
        }
        
        Debug.Log("Kartlar doğru eşleşme alanına taşındı");
    }

    public void DestroyCards()
    {
        //cardContainer.ClearCard(_index);
    }
    
    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}