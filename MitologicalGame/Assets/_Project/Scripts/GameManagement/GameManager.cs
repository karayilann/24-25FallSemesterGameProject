using System.Collections;
using System.Collections.Generic;
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
        private HashSet<CardType> requiredCardTypes;
        public HashSet<CardType> selectedCardTypes = new HashSet<CardType>();

        public List<MeshRenderer> cardMeshRenderers;
        
        public MatchCardBehaviours selected1;
        public MatchCardBehaviours selected2;

        public int suspicionCount;

        private void Awake()
        {
            requiredCardTypes = new HashSet<CardType>(requiredCardTypesList);
        }
 
        public void OnCardSelected(MatchCardBehaviours cardType)
        {
            if (selected1 == null)
            {
                selected1 = cardType;
            }
            else
            {
                selected2 = cardType;
            }
            selectedCardTypes.Add(cardType.CardType);
            if (selectedCardTypes.Count == requiredCardTypesList.Count) CheckWinCondition();
        }

        // Kartı geri kapatma olursa çalışacak fonksiyon
        public void OnCardDeselected(MatchCardBehaviours cardType)
        {
            selectedCardTypes.Remove(cardType.CardType);
        }

        public Material dissolveMaterial; // Shader'ın bağlı olduğu materyal
        public float dissolveSpeed = 0.01f; // Efektin oynatma hızı
        private float dissolveValue = 0f;

        private void CheckWinCondition()
        {
            if (requiredCardTypes.IsSubsetOf(selectedCardTypes))
            {
                Debug.Log("Kazandın!");

                popUpNotification.transform.DOLocalMoveY(186.1807f, .5f).OnStart(() => popUpNotificationText.text = "Kazandın!").OnComplete(() =>
                {
                    StartCoroutine(PlayDissolveEffect(selected1.cardMeshRenderers));
                    StartCoroutine(PlayDissolveEffect(selected2.cardMeshRenderers));
                    
                    popUpNotification.transform.DOLocalMoveY(252.2807f, .5f).SetDelay(1f).OnComplete( () =>
                    {
                        
                    });
                });
                
                // Dissolve efekti oynat
                
            }
            else
            {
                Debug.Log("Henüz kazanmıyorsun.");
            }
        }

        public IEnumerator PlayDissolveEffect(List<MeshRenderer> meshes)
        {
            Debug.Log("Dissolve efekti başlatılıyor");

            if (meshes == null || meshes.Count == 0)
            {
                Debug.LogError("Kartta MeshRenderer bileşeni bulunamadı!");
                yield break;
            }

            foreach (var mesh in meshes)
            {
                Material[] originalMaterials = mesh.materials;
                Material[] materialInstances = new Material[originalMaterials.Length];

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    materialInstances[i] = new Material(dissolveMaterial);
                }
        
                mesh.materials = materialInstances;

                dissolveValue = 0f;

                while (dissolveValue < 1f)
                {
                    dissolveValue += Time.deltaTime * dissolveSpeed;

                    foreach (var materialInstance in materialInstances)
                    {
                        materialInstance.SetFloat("_Cutoff", dissolveValue);
                    }
            
                    yield return null;
                }
            }

            Debug.Log("Dissolve efekti tamamlandı");
        }


    }
}