using _Project.Scripts.GameManagement;
using DG.Tweening;

namespace _Project.Scripts.Card
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class Card : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private int value = 1;
        [SerializeField] private CardType type;
        private bool _isClicked;
        [SerializeField] private Vector3 _scaleAnimation;
        
        public TextMeshProUGUI cardTypeText;

        public int Value => value;
        public CardType Type => type;

        [SerializeField] private float hoverOffset = 5f;
        private Vector3 _initialPosition;

        public Card(int i, CardType normal)
        {
            value = i;
            type = normal;
        }
        
        public void Initialize(int value, CardType type)
        {
            this.value = value;
            this.type = type;
        }

        private void Start()
        {
            _initialPosition = transform.position;
            cardTypeText.text = Type.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Kart seçildi: " + gameObject.name + " Tür: " + Type);
            _isClicked = true;
            GameManager.Instance.playerCard = this;
            Destroy(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isClicked) return;
            transform.position = _initialPosition + Vector3.up * hoverOffset;
            transform.DOScale(_scaleAnimation,.5f);
            DOTween.Kill(2, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isClicked) return;
            transform.position = _initialPosition;
            transform.DOScale(new Vector3(1,1,1),.3f);
            DOTween.Kill(2, true);
        }
    }

    public enum CardType
    {
        Normal,
        BlockOpponent,
        DoubleScore,
    }
}