using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using _Project.Scripts.CoreScripts;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.GameManagement
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private List<CardType> requiredCardTypesList;
        public GameObject popUpNotification;
        public TextMeshProUGUI popUpNotificationText;
        private HashSet<CardType> _requiredCardTypes;
        public readonly HashSet<CardType> SelectedCardTypes = new HashSet<CardType>();

        public Material dissolveMaterial;
        public float dissolveSpeed = 0.01f;
        private float _dissolveValue = 0f;
        
        public List<MeshRenderer> cardMeshRenderers;
        
        public CardBehaviours selected1;
        public CardBehaviours selected2;
        
        private bool _isProcessingWin = false;
        private bool _isProcessingReset = false;

        private readonly Vector3 _cardClosedRotation = new Vector3(-180f, 90f, 0f);

        public int suspicionCount;

        private void Awake()
        {
            _requiredCardTypes = new HashSet<CardType>(requiredCardTypesList);
        }
 
        public void OnCardSelected(CardBehaviours cardType)
        {
            if (_isProcessingWin || _isProcessingReset) return;
            
            if (selected1 == null)
            {
                selected1 = cardType;
                SelectedCardTypes.Add(cardType.CardType);
            }
            else if (selected2 == null && cardType != selected1)
            {
                selected2 = cardType;
                SelectedCardTypes.Add(cardType.CardType);
                
                if (SelectedCardTypes.Count == requiredCardTypesList.Count)
                {
                    CheckWinCondition();
                }
                else
                {
                    StartCoroutine(ResetCardsWithDelay());
                }
            }
        }

        private IEnumerator ResetCardsWithDelay()
        {
            _isProcessingReset = true;
            
            yield return new WaitForSeconds(1f);
            
            if (selected1 != null)
            {
                selected1.transform.DORotate(_cardClosedRotation, 1f)
                    .OnComplete(() => 
                    {
                        if(selected1 != null)
                            selected1.transform.rotation = Quaternion.Euler(_cardClosedRotation);
                    });
            }
            
            if (selected2 != null)
            {
                selected2.transform.DORotate(_cardClosedRotation, 1f)
                    .OnComplete(() => 
                    {
                        if(selected2 != null)
                            selected2.transform.rotation = Quaternion.Euler(_cardClosedRotation);
                    });
            }

            yield return new WaitForSeconds(1f);
            
            ResetSelections();
            _isProcessingReset = false; 
        }

        public void OnCardDeselected(CardBehaviours cardType)
        {
            if (_isProcessingWin || _isProcessingReset) return;
            
            SelectedCardTypes.Remove(cardType.CardType);
            
            if (cardType == selected1)
                selected1 = null;
            else if (cardType == selected2)
                selected2 = null;
        }

        private void CheckWinCondition()
        {
            if (_requiredCardTypes.IsSubsetOf(SelectedCardTypes))
            {
                Debug.Log("Kazandın!");
                _isProcessingWin = true;

                popUpNotification.transform.DOLocalMoveY(186.1807f, .5f)
                    .OnStart(() => popUpNotificationText.text = "Kazandın!")
                    .OnComplete(() =>
                    {
                        StartCoroutine(PlayDissolveEffect(selected1.cardMeshRenderers));
                        StartCoroutine(PlayDissolveEffect(selected2.cardMeshRenderers));
                    
                        popUpNotification.transform.DOLocalMoveY(252.2807f, .5f)
                            .SetDelay(1f)
                            .OnComplete(ResetSelections);
                    });
            }
            else
            {
                Debug.Log("Henüz kazanmıyorsun.");
                StartCoroutine(ResetCardsWithDelay());
            }
        }

        private void ResetSelections()
        {
            selected1 = null;
            selected2 = null;
            SelectedCardTypes.Clear();
            _isProcessingWin = false;
        }

        public IEnumerator PlayDissolveEffect(List<MeshRenderer> meshes)
        {
            Debug.Log("Dissolve efekti başlatılıyor");

            if (meshes == null || meshes.Count == 0)
            {
                Debug.LogError("Kartta MeshRenderer bileşeni bulunamadı!");
                yield break;
            }

            Material[] dissolveInstance = { new Material(dissolveMaterial) };

            foreach (var mesh in meshes)
            {
                if (mesh != null)
                {
                    mesh.materials = dissolveInstance;
                }
            }

            _dissolveValue = 0f;

            while (_dissolveValue < 1f)
            {
                _dissolveValue += Time.deltaTime * dissolveSpeed;
                dissolveInstance[0].SetFloat("_Cutoff", _dissolveValue);
                yield return null;
            }

            var card1 = selected1;
            var card2 = selected2;

            Destroy(dissolveInstance[0]);
            
            if (card1 != null) card1.DestroyCards();
            if (card2 != null) card2.DestroyCards();
            
            dissolveInstance = null;
            Debug.Log("Dissolve efekti tamamlandı");
        }
    }
}