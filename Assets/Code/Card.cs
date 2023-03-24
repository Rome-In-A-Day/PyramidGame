using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public Sprite cardBackSprite;
    public SpriteRenderer cardRenderer;
    public Dictionary<string, Texture2D> cardTextures;    
    public bool active { get; set; } = true;
    public GameObject child1 { get; set; }
    public GameObject child2 { get; set; }
    public bool showingFace { get; set; } = false;

    private PyramidGame game;
    private int cardNumber;
    private string cardName;    
    private Sprite cardFaceSprite;    
    // Move Animation Variables
    private Vector3 velocity;
    private float smoothTime = 0.5f;
   
        
    // Start is called before the first frame update
    void Start()
    {
        GameObject gameO = GameObject.Find("GameBoard");
        game = gameO.GetComponent<PyramidGame>();

    }

    // Update is called once per frame
    void Update()
    {
        if (child1 != null && child2 != null)
        {
            if (!child1.GetComponent<Card>().active && !child2.GetComponent<Card>().active)
            {
                if (!showingFace)
                {
                    showFaceSprite(true);
                    game.AddToShowingPyramid(this);
                }
            }
        }
        
    }

    public void setCardNumber(int cardNumber)
    {
        this.cardNumber = (int)cardNumber;
    }

    public int getCardNumber()
    {
        return this.cardNumber;
    }

    public void setCardName(string cardName)
    {
        this.cardName = cardName;
        AsyncOperationHandle<Sprite> spriteHandle = Addressables.LoadAssetAsync<Sprite>(this.cardName);        
        spriteHandle.Completed += OnLoadDone;
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<Sprite> obj)
    {
        // In a production environment, you should add exception handling to catch scenarios such as a null result.
        if (obj.Status == AsyncOperationStatus.Succeeded && obj.Result != null) {
            cardFaceSprite = obj.Result;            
            if (showingFace)
            {
                cardRenderer.sprite = cardFaceSprite;
            }
        }        
    }


    public void showFaceSprite(bool showFace)
    {
        if (showingFace != showFace)
        {
            if (showFace)
            {
                cardRenderer.sprite = cardFaceSprite;
                showingFace = true;
            }
            else
            {
                cardRenderer.sprite = cardBackSprite;
                showingFace = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (active)
        {
            game.selectedCard(this);        
        }        
    }

    public void MoveCard(Vector3 newPosition)
    {
        //transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        // Ensure only one move at a time    
        StopAllCoroutines();        
        StartCoroutine(SmoothMove(newPosition));
    }    

    // Handles translating the cards to locations
    private IEnumerator SmoothMove(Vector3 newPosition)
    {                
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        float temp = Mathf.Abs((transform.position.x - newPosition.x));
        float temp2 = Mathf.Abs((transform.position.y - newPosition.y));
        while (Mathf.Abs((transform.position.x - newPosition.x)) > 0.01 |
            Mathf.Abs((transform.position.y - newPosition.y)) > 0.01)
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
            yield return new WaitForEndOfFrame();
        }        
        yield return null;
    }
}
