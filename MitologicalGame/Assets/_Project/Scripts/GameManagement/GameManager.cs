using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.AI;
using _Project.Scripts.Card;
using _Project.Scripts.Character;

namespace _Project.Scripts.GameManagement
{
    using _Project.Scripts.Card;
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Player player;
        public AIPlayer ai;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private List<Card> _aiDeck;

        private int _turnCount;
        private bool _isPlayerBlocked;
        private bool _isAiBlocked;
        public Card playerCard;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // player = GameObject.Find("Player").GetComponent<Player>();
            // ai = GameObject.Find("AIPlayer").GetComponent<AIPlayer>();
            InitializeDeck();
            ai = new AIPlayer(new List<Card>(_aiDeck));
            _turnCount = 0;
            StartCoroutine(PlayTurn());
        }

        void InitializeDeck()
        {
            _aiDeck = new List<Card>();
            for (int i = 1; i <= 5; i++)
            {
                AddCardToDeck(i, CardType.Normal);
                AddCardToDeck(i, CardType.BlockOpponent);
                AddCardToDeck(i, CardType.DoubleScore);
            }
        }

        void AddCardToDeck(int value, CardType type)
        {
            GameObject cardObject = Instantiate(cardPrefab);
            Card card = cardObject.GetComponent<Card>();
            card.Initialize(value, type);
            _aiDeck.Add(card);
        }

        IEnumerator PlayTurn()
        {
            Card aiCard = null;
            
            while (_turnCount < 5)
            {
                Debug.Log("Tur: " + _turnCount);
                Debug.Log("Oyuncu Puanı: " + player.Score + " AI Puanı: " + ai.Score);
 
                if (_turnCount % 2 == 0 && !_isPlayerBlocked)
                {
                    playerCard = null;
                    Debug.Log("Oyuncu kart seçiyor...");
                    if (playerCard == null) break;
                    Debug.Log("Oyuncu kart seçti: " + playerCard.Value + " Tür: " + playerCard.Type);
                }
                else if(_turnCount %2 == 1 && !_isAiBlocked)
                {
                    playerCard = null;
                    Debug.Log("AI kart seçiyor...");
                    aiCard = ai.DrawCard();
                    Debug.Log("AI kart seçti: " + aiCard.Value + " Tür: " + aiCard.Type);
                }
                ApplyCardEffects(playerCard, aiCard);
                _turnCount++;
                yield return new WaitForSeconds(1f);
            }
            DetermineWinner();
        }

        void ApplyCardEffects(Card playerCard, Card aiCard)
        {
            if (playerCard != null)
            {
                switch (playerCard.Type)
                {
                    case CardType.BlockOpponent:
                        _isAiBlocked = true;
                        break;
                    case CardType.DoubleScore:
                        if (playerCard.Value > (aiCard?.Value ?? 0)) player.Score += 2;
                        else player.Score++;
                        break;
                    default:
                        if (playerCard.Value > (aiCard?.Value ?? 0)) player.Score++;
                        break;
                }
            }

            if (aiCard != null)
            {
                switch (aiCard.Type)
                {
                    case CardType.BlockOpponent:
                        _isPlayerBlocked = true;
                        break;
                    case CardType.DoubleScore:
                        if (aiCard.Value > (playerCard?.Value ?? 0)) ai.Score += 2;
                        else ai.Score++;
                        break;
                    default:
                        if (aiCard.Value > (playerCard?.Value ?? 0)) ai.Score++;
                        break;
                }
            }
        }

        void DetermineWinner()
        {
            if (player.Score > ai.Score)
            {
                Debug.Log("Oyuncu oyunu kazandı!");
            }
            else if (player.Score < ai.Score)
            {
                Debug.Log("AI oyunu kazandı!");
            }
            else
            {
                Debug.Log("Oyun berabere bitti!");
            }
        }
    }
}
