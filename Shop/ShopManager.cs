using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private Animator shopPanel;
    [SerializeField] private AudioSource doorBellIn;
    [SerializeField] private AudioSource doorBellOut;
    [SerializeField] private GameObject selector;
    [SerializeField] private CrabManager crabManager;
    [SerializeField] private ConfirmationScript confirmationPanel;
    [SerializeField] private CoinCount coinManager;
    [SerializeField] private GameObject licencePanel;
    [SerializeField] private GameObject gigaLicenceHUB;
    [SerializeField] private GameObject continueLicenceButton;

    [HideInInspector] public ShopVoice voiceShop;
    [HideInInspector] public ShopDialogs actualShopDialogs;
    [HideInInspector] public bool canPressXButton = true;

    public ShopItem[] items;
    public AudioSource moveAudio;
    private EventSystem eventSystem;


    private void OnEnable()
    {
        InputController.OnStartPressedOnShop += CloseShop;
        InputController.OnXPressed += TryToOpenShop;
    }

    private void OnDisable()
    {
        InputController.OnStartPressedOnShop -= CloseShop;
        InputController.OnXPressed -= TryToOpenShop;


    }
    private void Awake()
    {
        voiceShop = GetComponent<ShopVoice>();
    }
    private void Start()
    {
        UpdateShop();
        eventSystem = EventSystem.current;
    }

    private void TryToOpenShop()
    {
        if (canPressXButton)
        {
            if (ShopCollider.inFrontOfShop && GameManager.state == GameState.Play)
            {
                if (ShopCollider.gigacrabLicenceReady)
                {
                    OpenGigacrabLicence();
                }
                else
                {
                    OpenShop();
                    CheckShopAchievements();
                }
                AddColdownXbutton(0.5f);
            }
        }
    }

   

    public void TryToBuy(int itemNumber) //CALLED FROM BUTTONS
    {
        if (GameManager.state == GameState.Shop && canPressXButton)
        {

            if (!items[itemNumber].unlocked)
            {
                items[itemNumber].PlayLockAnimation();
            }
            else if (items[itemNumber].sold)
            {
                //already bought
            }
            else
            {
                HandleItemPurchase(itemNumber);
            }
        }
    }

    private void HandleItemPurchase(int itemNumber)
    {
        if (coinManager.actualCoins >= items[itemNumber].cost)
        {
            if (itemNumber == 0)
            {
                confirmationPanel.gameObject.SetActive(true);
                moveAudio.Play();
                if (GameManager.saveData.gigacrabLicence)
                {
                    SteamAchievements.TryToUnlockAchievements(1);
                    voiceShop.PlayRandomBuy();
                    BuyItem(0);
                    items[3].GetItem(false);
                }
                else
                {
                    voiceShop.PlayRandomDuda();

                }
                AddColdownXbutton(1f);

            }
            else
            {
                BuyItem(itemNumber);
                AddColdownXbutton(0.2f);

            }
        }
        else
        {
            items[itemNumber].PlayNoMoneyAnimation();
        }
    }

    public void BuyItem(int itemNumber)
    {
        coinManager.RemoveCoin(items[itemNumber].cost);
        items[itemNumber].GetItem(true);
        switch (itemNumber)
        {
            case 0:
                crabManager.magnetScript.ActivateMagnet();
                GameManager.saveData.unlockedAbilities = 1;
                break;

            case 1:
                crabManager.slowMotionScript.ActivateSlowMotion();
                GameManager.saveData.unlockedAbilities = 2;

                break;

            case 2:
                crabManager.wingsScript.ActivateWings();
                GameManager.saveData.unlockedAbilities = 3;

                break;
        }
        UnlockNextItem(itemNumber);
    }

    private void UnlockNextItem(int itemNumber)
    {
        if (itemNumber < items.Length - 2)
        {
            items[itemNumber + 1].UnlockItem();
        }
    }

    public void ChangeDescription(int itemNumber)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i == itemNumber)
            {
                items[i].description.SetActive(true);
            }
            else
            {
                items[i].description.SetActive(false);

            }
        }
    }
    public void AnimateSelected(int itemNumber)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i == itemNumber)
            {
                items[i].SelectItem();

            }
            else
            {
                items[i].DeselectItem();

            }
        }
    }


    private void CheckShopAchievements()
    {
        if (GameManager.saveData.highestShop % 2 != 0) //KENKRO'S SHOP
        {
            SteamAchievements.TryToUnlockAchievements(8);
        }
        else if (GameManager.saveData.highestShop % 2 == 0) //PUZO'S SHOP
        {
            SteamAchievements.TryToUnlockAchievements(9);
        }
    }

    public void AddColdownXbutton(float seconds)
    {
        StopAllCoroutines();
        StartCoroutine(ResetButtonCooldown(seconds));
    }
    public IEnumerator ResetButtonCooldown(float time)
    {
        canPressXButton = false;

        // Espera el tiempo del cooldown
        yield return new WaitForSeconds(time);

        // Permite pulsar el botón de nuevo
        canPressXButton = true;
    }

   private void OpenGigacrabLicence()
    {
        licencePanel.SetActive(true);
        GameManager.state = GameState.Cinematic;
        gigaLicenceHUB.SetActive(true);
        GameManager.saveData.gigacrabLicence = true;
        eventSystem.SetSelectedGameObject(continueLicenceButton);
        SteamAchievements.TryToUnlockAchievements(7);
    }
    public void CloseGigacrabLicence()
    {
        if (canPressXButton)
        {
            licencePanel?.SetActive(false);
            voiceShop.PlayRandomBye();
            DeviceManager.HideMouse();
            actualShopDialogs.OpenGoodbye();
            ShopCollider.gigacrabLicenceReady = false;
            eventSystem.SetSelectedGameObject(null);
            GameManager.state = GameState.Play; 
        }
    }

    private void OpenShop()
    {
        shopPanel.SetBool("Open", true);
        GameManager.state = GameState.Shop;
        doorBellIn.Play();
        SelectFirstItem();
        ChangeDescription(0);
        if (GameManager.usingMouse) 
            { 
                DeviceManager.ShowMouse(); 
            }
    }
    public void SelectFirstItem()
    {
        eventSystem.SetSelectedGameObject(items[0].gameObject);
        Vector3 newPosition = selector.transform.localPosition;
        newPosition.x = items[0].transform.localPosition.x;
        selector.transform.localPosition = newPosition;
        AnimateSelected(0);
    }
    public void CloseShop()
    {
        if (GameManager.state == GameState.Shop)
        {
            shopPanel.SetBool("Open", false);
            doorBellOut.Play();
            eventSystem.SetSelectedGameObject(null);
            DeviceManager.HideMouse();
            actualShopDialogs.OpenGoodbye();
            GameManager.state = GameState.Play;

        }
    }
    public void UpdateShop()
    {
        if (GameManager.saveData.gigacrabLicence)
        {
            gigaLicenceHUB.SetActive(true);
        }
        for (int i = 0; i < items.Length; i++)
        {
            if (i < GameManager.saveData.unlockedAbilities)
            {
                items[i].GetItem(false);
                items[3].GetItem(false);
                if (i == 0)
                {
                    crabManager.magnetScript.ActivateMagnet();

                }
                else if (i == 1) 
                {
                    crabManager.slowMotionScript.ActivateSlowMotion(); 
                }
                else if (i == 2)
                {
                    crabManager.wingsScript.ActivateWings();
                }
               
            }
            else if (i == GameManager.saveData.unlockedAbilities && i != 3)
            {
                items[i].UnlockItem();
            }
        }
    }
}
