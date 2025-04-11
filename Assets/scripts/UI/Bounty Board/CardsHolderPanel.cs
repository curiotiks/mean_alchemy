using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsHolderPanel : MonoBehaviour
{
    public enum CardDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public CardDifficulty cardDifficulty;

    [SerializeField,Range(1,5)] public int MaxCards = 5;
    [SerializeField] private List<GameObject> cards;

    private void Awake()
    {
        cards = new List<GameObject>();
    }

    public void AddCardsToThisRow(List<GameObject> cardsToAdd)
    {
        if (cards.Count > MaxCards)
        {
            foreach (GameObject card in cardsToAdd)
            {
                Destroy(card);
            }
            Debug.LogWarning("Too many cards to add to this row");
            return;
        }
        foreach (GameObject card in cardsToAdd)
        {
            if (cards.Count <= MaxCards)
            {
                cards.Add(card);
                card.transform.SetParent(transform);
                //card.transform.localScale = Vector3.one;
            }
            else
            {
                Destroy(card);
                Debug.LogError("Max cards reached");
                break;
            }
        }
    }

    public void ClearTheCards()
    {
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        cards.Clear();
    }

    private void OnDisable()
    {
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        cards.Clear();
    }
}
