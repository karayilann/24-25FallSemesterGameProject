using System.Collections.Generic;
using _Project.Scripts.Base___Interfaces;
using UnityEngine;

namespace _Project.Scripts.AI
{
    using _Project.Scripts.Card;
    public class AIPlayer : CharacterBase
    {
        public List<CancelledCard> deck;
        
        public AIPlayer(List<CancelledCard> deck)
        {
            this.deck = deck;
        }

        public CancelledCard DrawCard()
        {
            int index = Random.Range(0, deck.Count);
            CancelledCard cancelledCard = deck[index];
            deck.RemoveAt(index);
            return cancelledCard;
        }
        
    }
}
