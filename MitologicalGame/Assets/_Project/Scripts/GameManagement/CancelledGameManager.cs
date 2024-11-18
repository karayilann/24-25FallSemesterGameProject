using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.AI;
using _Project.Scripts.Base___Interfaces;
using _Project.Scripts.Card;
using _Project.Scripts.Character;
using UnityEngine.Serialization;

namespace _Project.Scripts.GameManagement
{
    using _Project.Scripts.Card;
    public class CancelledGameManager : MonoBehaviour
    {
        public static CancelledGameManager Instance { get; private set; }

        public CharacterBase player;
        public AIPlayer ai;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private List<CancelledCard> _aiDeck;

        private int _turnCount;
        private bool _isPlayerBlocked;
        private bool _isAiBlocked;
        [FormerlySerializedAs("playerCard")] public CancelledCard playerCancelledCard;

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
            ai = new AIPlayer(new List<CancelledCard>(_aiDeck));
            _turnCount = 0;
            StartCoroutine(PlayTurn());
        }

        void InitializeDeck()
        {
            _aiDeck = new List<CancelledCard>();
            for (int i = 1; i <= 5; i++)
            {
                AddCardToDeck(i, OldCardType.Normal);
            }
        }

        void AddCardToDeck(int value, OldCardType type)
        {
            GameObject cardObject = Instantiate(cardPrefab);
            CancelledCard cancelledCard = cardObject.GetComponent<CancelledCard>();
            cancelledCard.Initialize(value, type);
            _aiDeck.Add(cancelledCard);
        }

        IEnumerator PlayTurn()
        {
            CancelledCard aiCancelledCard = null;
            
            while (_turnCount < 5)
            {
                Debug.Log("Tur: " + _turnCount);
                Debug.Log("Oyuncu Puanı: " + player.Score + " AI Puanı: " + ai.Score);
 
                if (_turnCount % 2 == 0 && !_isPlayerBlocked)
                {
                    playerCancelledCard = null;
                    Debug.Log("Oyuncu kart seçiyor...");
                    if (playerCancelledCard == null) break;
                    Debug.Log("Oyuncu kart seçti: " + playerCancelledCard.Value + " Tür: " + playerCancelledCard.Type);
                }
                else if(_turnCount %2 == 1 && !_isAiBlocked)
                {
                    playerCancelledCard = null;
                    Debug.Log("AI kart seçiyor...");
                    aiCancelledCard = ai.DrawCard();
                    Debug.Log("AI kart seçti: " + aiCancelledCard.Value + " Tür: " + aiCancelledCard.Type);
                }
                ApplyCardEffects(playerCancelledCard, aiCancelledCard);
                _turnCount++;
                yield return new WaitForSeconds(1f);
            }
            DetermineWinner();
        }

        void ApplyCardEffects(CancelledCard playerCancelledCard, CancelledCard aiCancelledCard)
        {
            if (playerCancelledCard != null)
            {
                switch (playerCancelledCard.Type)
                {
                    case OldCardType.BlockOpponent:
                        _isAiBlocked = true;
                        break;
                    case OldCardType.DoubleScore:
                        if (playerCancelledCard.Value > (aiCancelledCard?.Value ?? 0)) player.Score += 2;
                        else player.Score++;
                        break;
                    default:
                        if (playerCancelledCard.Value > (aiCancelledCard?.Value ?? 0)) player.Score++;
                        break;
                }
            }

            if (aiCancelledCard != null)
            {
                switch (aiCancelledCard.Type)
                {
                    case OldCardType.BlockOpponent:
                        _isPlayerBlocked = true;
                        break;
                    case OldCardType.DoubleScore:
                        if (aiCancelledCard.Value > (playerCancelledCard?.Value ?? 0)) ai.Score += 2;
                        else ai.Score++;
                        break;
                    default:
                        if (aiCancelledCard.Value > (playerCancelledCard?.Value ?? 0)) ai.Score++;
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
