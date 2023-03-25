using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PyramidGame : MonoBehaviour
{

    public GameObject cardObject;    
    public Transform[] cardPlacements = new Transform[28];
    public Transform drawDeckPlacement;
    public Transform discardCardPlacement;
    public Transform pairedCardPlacement;
    public Transform startOfPyramid;
    public UnityEvent<string> GameEndEvent = new UnityEvent<string>();

    private Stack<GameObject> deck = new Stack<GameObject>();
    private Stack<GameObject> discard = new Stack<GameObject>();
    private Stack<GameObject> pairedDiscard = new Stack<GameObject>();
    private List<GameObject> visiblePyramidCards = new List<GameObject>();
    private List<GameObject> cards = new List<GameObject>();
    private List<int> cardValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    private List<string> cardSuits = new List<string> { "Clubs", "Spades", "Diamonds", "Hearts"};
    private List<string> cardLetters = new List<string> {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"  };
    private Card selectedCard1, selectedCard2;
    private int gameState = 0;

    public const int NUM_OF_ROWS = 7;
    public const float SPACE_BETWEEN_CARDS = 1.25f, SPACE_BETWEEN_ROWS = 1.0f;        


    // Start is called before the first frame update
    void Start()
    {
        // Allow cards to be clicked
        AddPhysics2DRaycaster();
        // Setup Game
        // Create all cards
        CreateCards();
        // Shuffle
        ShuffleDeck();
        // 7 Rows of cards
        SetupBoard();
        // Draw one card to create the discards
        DrawCard();
        
    }

    public void Reset()
    {
        selectedCard1 = null;
        selectedCard2 = null;
        CreateCards();
        ShuffleDeck();
        SetupBoard();
        DrawCard();
        gameState = 0;
    }

    private void CreateCards()
    {        
        // Deals with the case if it's a restart by removing old gameobjects
        foreach (GameObject card in cards)
        {
            card.SetActive(false);
            Destroy(card);
        }
        cards.Clear();
        discard.Clear();
        pairedDiscard.Clear(); 
        deck.Clear();
        visiblePyramidCards.Clear();
        // Creates a deck of 52 cards
        for (int i = 0; i < cardSuits.Count; i++)
        {
            for (int j = 0; j < cardLetters.Count; j++)
            {
                GameObject newCard = Instantiate(cardObject);
                newCard.GetComponent<Card>().setCardName("card" + cardSuits[i] + "_" + cardLetters[j]);
                newCard.GetComponent<Card>().setCardNumber(cardValues[j]);
                // Draw on top of table background
                SpriteRenderer tempSprite = newCard.GetComponent<SpriteRenderer>();
                tempSprite.sortingOrder = 1;
                cards.Add(newCard);
            }
        }
    }

    private void SetupBoard()
    {
        deck.Clear();
        discard.Clear();
        pairedDiscard.Clear();
        int placementIndex = 0;
        for (int rowCount = 1; rowCount <= NUM_OF_ROWS; rowCount++)
        {
            for (int cardCount = 1; cardCount <= rowCount; cardCount++)
            {
                // Calculates where each card should be placed
                float xAdjustment = -1*(rowCount-1)/2f*SPACE_BETWEEN_CARDS + (cardCount-1)*SPACE_BETWEEN_CARDS;                    
                Vector3 cardAjustment = new Vector3(xAdjustment, SPACE_BETWEEN_ROWS*(rowCount-1), 0);
                cards[placementIndex].transform.position = startOfPyramid.position - cardAjustment;
                cards[placementIndex].GetComponent<Card>().GetComponent<SpriteRenderer>().sortingOrder = placementIndex + 1;
                // Reveal the bottom row
                if (rowCount == NUM_OF_ROWS)
                {
                    cards[placementIndex].GetComponent<Card>().showFaceSprite(true);
                    visiblePyramidCards.Add(cards[placementIndex]);
                }
                else
                {
                    // Associate child cards
                    GameObject child1, child2;
                    child1 = cards[placementIndex + (rowCount)];
                    child2 = cards[placementIndex + (rowCount + 1)];
                    cards[placementIndex].GetComponent<Card>().child1 = child1;
                    cards[placementIndex].GetComponent<Card>().child2 = child2;
                }
                            

                placementIndex++;
            }
        }
        // Create Deck from rest of cards
        for (int i = placementIndex; i < cards.Count; i++)
        {
            cards[i].transform.position = drawDeckPlacement.position;
            deck.Push(cards[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameState == 0)
        {
            switch (CheckGameState())
            {
                case 0:
                    break;
                case 1:
                    Debug.Log("Ok you won this time");
                    GameEndEvent.Invoke("OK You won this time....");
                    this.gameState = 1;
                    break;
                case -1:
                    Debug.Log("You lost there is nothing more to do");
                    GameEndEvent.Invoke("You have lost.");
                    this.gameState = -1;
                    break;
            }
        }
    }

    private int CheckGameState()
    {    
        // Case where game is already over.
        if (this.gameState != 0)
        {
            return this.gameState;
        }        
        // Can still draw cards so you have a "move"
        if (deck.Count > 0)
        {
            return 0;
        }
        // Look for possible moves left
        IDictionary<int, int> avalibleValues = new Dictionary<int, int>();
        List<GameObject> avalibleCards = new List<GameObject>();
        // Get all accesible cards
        avalibleCards.AddRange(deck);
        if (discard.Count> 0)
            avalibleCards.Add(discard.Peek());
        avalibleCards.AddRange(visiblePyramidCards);
        // If no more cards are there then they won!
        if (avalibleCards.Count == 0) {
            return 1;
        }
        // Check free cards for values up to 13
        foreach (GameObject temp in avalibleCards)
        {
            int cardValue = temp.GetComponent<Card>().getCardNumber();
            // Shortcut it if there is a King still out there
            if (cardValue == 13)
            {
                return 0;
            }
            if (avalibleValues.ContainsKey(cardValue))
                avalibleValues[cardValue]++;
            else
                avalibleValues[cardValue] = 1;            
        }
        foreach (var kvp in avalibleValues)
        {
            int value;
            // Doesn't catch the King, but that's handeled above
            // Checks if there is a card pair that adds to 13
            if(avalibleValues.TryGetValue(13 - kvp.Key, out value))
            {
                return 0;
            }
        }
        // No moves left
        return -1;
    }

    void ShuffleDeck()
    {
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, cards.Count);
            GameObject swapValue = cards[n];
            cards[n] = cards[k];
            cards[k] = swapValue;
        }                
    }
        

    // Draw a card from the deck to the discard pile
    public void DrawCard()
    {
        if(deck.Count > 0)
        {
            GameObject newCard = deck.Pop();
            //newCard.transform.position = discardCardPlacement.position;
            newCard.GetComponent<Card>().showFaceSprite(true);
            newCard.GetComponent<Card>().MoveCard(discardCardPlacement.position);            
            if (discard.Count > 0)
            {
                newCard.GetComponent<SpriteRenderer>().sortingOrder = discard.Peek().GetComponent<SpriteRenderer>().sortingOrder + 1;
            }
            discard.Push(newCard);
        }
    }

    private void AddPhysics2DRaycaster()
    {
        Physics2DRaycaster physicsRaycaster = FindObjectOfType<Physics2DRaycaster>();
        if (physicsRaycaster == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }

    public void selectedCard(Card card)
    {
        // Handle click on the deck
        if (deck.Contains(card.gameObject))
        {
            DrawCard();
            return;
        }
        // Handle click on showing card
        if (card.showingFace)
        {
            if (card.getCardNumber() == 13)
            {
                RemoveCard(card);
                return;
            }
            if (selectedCard1 == null) { selectedCard1 = card; return; }
            else if (selectedCard2 == null) { selectedCard2 = card; }
            else
            {
                selectedCard1 = selectedCard2;
                selectedCard2 = card;
            }
            if (selectedCard1.getCardNumber() + selectedCard2.getCardNumber() == 13)
            {
                RemoveCard(selectedCard1);
                RemoveCard(selectedCard2);
                selectedCard1 = null;
                selectedCard2 = null;
            }
        }
    }

    private void RemoveCard(Card card)
    {        
        card.active = false;
        card.MoveCard(pairedCardPlacement.position);        
        if (pairedDiscard.Count > 0)
        {
            // Means the last card clicked shows up on top
            card.GetComponent<SpriteRenderer>().sortingOrder = pairedDiscard.Peek().GetComponent<SpriteRenderer>().sortingOrder + 1;            
        }
        pairedDiscard.Push(card.gameObject);
        if (visiblePyramidCards.Contains(card.gameObject))
        {
            visiblePyramidCards.Remove(card.gameObject);
        }
        // Case where the discard card is one of the cards
        if (discard.Count > 0 && discard.Peek() == card.gameObject)
        {
            discard.Pop();
        }        
    }

    // Tracks what cards can be used to pair
    public void AddToShowingPyramid(Card card)
    {
        visiblePyramidCards.Add(card.gameObject);
    }

    // Scene / Menu State handling functions
    public void ExitGame()
    {
        Debug.Log("Exiting Game");
        Application.Quit();
    }
}
