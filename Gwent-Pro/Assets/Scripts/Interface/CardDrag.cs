using LogicalSide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CardDrag : MonoBehaviour
{
    public bool Avalancha;//Usado para cuando un efecto desencadena la invocacion de multiples cartas y no se desea que cada activacion provoque un cambio de turno
    public bool IsDragging= false;
    public bool Played= false;
    private Vector2 startPos;
    public GameObject dropzone;
    public List<GameObject> dropzones = new List<GameObject>();
    private Efectos efectos;
    public Card AssociatedCard;
    private GameManager GM;
    void Start()
    // Start is called before the first frame update
    {
        efectos = GameObject.Find("Effects").GetComponent<Efectos>();

        Visualizer = GameObject.Find("Visualizer");
        AssociatedCard = gameObject.GetComponent<CardDisplay>().cardTemplate;
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Start2()
    {
        Start();
    }
    #region Drag
    public void StartDrag()
    {
        if (!IsDragging)
        {
            startPos = gameObject.transform.position;
            if (!Played && GM.WhichPlayer(AssociatedCard.DownBoard).SetedUp)
            {
                if (AssociatedCard.DownBoard == GM.Turn)
                {
                    startPos = gameObject.transform.position;
                    IsDragging = true;
                    BigCardDestroy();
                }
            }
        }
    }
    public void EndDrag()
    {
        if (!Played)
        {
            IsDragging = false;
            dropzone = IsPosible();
            if (dropzone != null)
            {
                if (AssociatedCard.TypeInterno != "D")
                {
                    if (AssociatedCard.Eff != "Light")
                        transform.SetParent(dropzone.transform, false);
                    if (AssociatedCard.TypeInterno.IndexOf("C") == -1 && AssociatedCard.TypeInterno.IndexOf("A") == -1)
                        AssociatedCard.current_Rg = dropzone.tag;
                    else
                    {
                        AssociatedCard.current_Rg = AssociatedCard.Range;
                    }
                }
                else
                {
                    //Es un Decoy, regreso la carta a la mano
                    CardDisplay exchange = dropzone.GetComponent<CardDisplay>();
                    AssociatedCard.current_Rg = exchange.cardTemplate.current_Rg;
                    Transform drop = dropzone.transform.parent;
                    transform.SetParent(drop.transform, false);
                    efectos.Decoy(exchange.cardTemplate);
                    efectos.RestartCard(dropzone, null, true);
                }
                Played = true;
                if (GM.Turn)
                    GM.P1.Surrender = false;
                else
                    GM.P2.Surrender = false;
                if (AssociatedCard.TypeInterno == "U")
                    efectos.PlayCard(AssociatedCard);
                GM.Sounds.PlaySoundButton();
                if(AssociatedCard.TypeInterno!="D" )
                    efectos.ListEffects[AssociatedCard.Eff].Invoke(AssociatedCard);
                if (!(AssociatedCard.Effects == null || AssociatedCard.Effects.Count == 0))
                {
                    try
                    {
                        AssociatedCard.Execute(efectos);
                    }
                    catch (System.Exception ex)
                    {
                        GM.SendPrincipal("Error en la ejecuci�n del efecto:");
                        GM.SendPrincipal(ex.Message);
                    }
                }
                if(!Avalancha)
                GM.Turn = !GM.Turn;
                if (AssociatedCard.Eff == "Light")
                {
                    PlayerDeck deck = efectos.Decking(AssociatedCard.DownBoard);
                    deck.AddToCement(AssociatedCard);
                    Destroy(gameObject);
                }
            }
        }
        if (!Played)
        {
            transform.position = startPos;
            dropzone = null;
            GM.Sounds.PlayError();
        }
    }
    private GameObject IsPosible()
    {
        foreach(GameObject drop in dropzones)
        if (AssociatedCard.TypeInterno.IndexOf("C") == -1)
            if (AssociatedCard.TypeInterno.IndexOf("A") == -1)
            {
                if (AssociatedCard.TypeInterno.IndexOf('D') == -1)
                {
                    if (drop.transform.childCount < 6 && AssociatedCard.Range.IndexOf(drop.tag) != -1 && efectos.RangeMap[(AssociatedCard.DownBoard, drop.tag)] == drop)
                    {
                        return drop;
                    }
                }
                else
                {
                    if (drop.tag == "Card"&& drop.transform.parent.tag!="P"&& drop.transform.parent.tag != "E")
                        {
                            if(drop.GetComponent<CardDisplay>().cardTemplate.DownBoard== AssociatedCard.DownBoard)
                                return drop;

                        }
                }
            }
            else
            {
                if (AssociatedCard.TypeInterno.IndexOf(drop.tag)!= -1 && efectos.RangeMap[(AssociatedCard.DownBoard, drop.tag)] == drop&& drop.transform.childCount<1)
                    return drop;
            }
        else
        {
            if ((drop.transform.childCount < 3 && drop.tag == "C") || (drop.tag != "P" && drop.tag != "E" && AssociatedCard.Eff == "Light"))
                return drop;
        }
        return null;
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        dropzones.Insert(0,collision.gameObject);
    }
    public void OnCollisionExit2D(Collision2D collision)
    {
        dropzones.Remove(collision.gameObject);
    }
    void Update()
    {
        if (IsDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }
    #endregion

    #region BigCard
    public GameObject BigCardPrefab;
    GameObject Big;
    public GameObject Visualizer;
    public Vector3 zoneBig= new Vector3(1800, 300);
    public void BigCardProduce() 
    {
        if(Big!=null)
            BigCardDestroy();
        if (!IsDragging&&( gameObject.tag=="LeaderCard"||(gameObject.tag=="Card" && !gameObject.transform.GetChild(7).gameObject.activeSelf)))
        {
            CardDisplay card = gameObject.GetComponent<CardDisplay>();
            Big = Instantiate(BigCardPrefab, zoneBig, Quaternion.identity);
            Big.transform.SetParent(Visualizer.transform, worldPositionStays: true);
            Big.transform.position = zoneBig;
            CardDisplay disp = Big.GetComponent<CardDisplay>();
            disp.cardTemplate = card.cardTemplate;
            disp.ImBig= true;
            // disp.ArtworkImg = Big.transform.GetChild(0).GetComponent<Image>();
            // if (disp.ArtworkImg != null)
            //     disp.DescriptionText = Big.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            // disp.PwrTxt = Big.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        }
    }
    public void BigCardDestroy()
    {
        Destroy(Big);
    }
    #endregion
    public void CardExchange()
    {
        Player P = GM.WhichPlayer(AssociatedCard.DownBoard);
        if (GM.Turn == AssociatedCard.DownBoard) 
        {
            if (!P.SetedUp)
            {
                BigCardDestroy();
                PlayerDeck Deck = efectos.Decking(AssociatedCard.DownBoard);
                Deck.deck.Insert(0,AssociatedCard);
                Deck.InstanciateLastOnDeck(1, true);
                P.cardsExchanged++;
                Destroy(gameObject);
                if (P.cardsExchanged == 2)
                {
                    GM.Teller.text="";
                }
            }
        }
    }
    bool acted = false;
    public void LeaderAction()
    {
        if(AssociatedCard != null)
        {
            if(AssociatedCard.TypeInterno== "L" && GM.Turn== AssociatedCard.DownBoard && !acted)
            {
                if (!(AssociatedCard.Effects == null || AssociatedCard.Effects.Count == 0))
                {
                    try
                    {
                        acted = true;
                        AssociatedCard.Execute(efectos);
                    }
                    catch (System.Exception ex)
                    {
                        GM.SendPrincipal("Error en la ejecuci�n del efecto:");
                        GM.SendPrincipal(ex.Message);
                    }
                    GM.Turn= !GM.Turn;
                }
            }
        }
    }
    
}
