using System.Collections.Generic;
using _Project.Scripts.Base___Interfaces;
using UnityEngine;

namespace _Project.Scripts.AI
{
    using _Project.Scripts.Card;
    public class AIPlayer : CharacterBase
    {
        public List<Card> deck;
        
        public AIPlayer(List<Card> deck)
        {
            this.deck = deck;
        }

        public Card DrawCard()
        {
            int index = Random.Range(0, deck.Count);
            Card card = deck[index];
            deck.RemoveAt(index);
            return card;
        }
        
    }
}
