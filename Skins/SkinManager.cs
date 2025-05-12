using UnityEngine;
using UnityEngine.EventSystems;

public class SkinManager : MonoBehaviour
{
    [SerializeField] private SkinMenuItem[] skinsItems;
    private EventSystem eventSystem;
    [SerializeField] private GameObject checkImage;
    [SerializeField] private GameObject skinPanel;
    [SerializeField] private GameObject[] skinsGameobject;
    [SerializeField] private GameObject skinsMenuButton;

    private void Start()
    {
        eventSystem = EventSystem.current;
        SetActualSkin(GameManager.saveData.actualSkin);
    }
    // Start is called before the first frame update
    public void OpenSkinMenu()
    {
        skinPanel.SetActive(true);
        for (int i = 0; i < skinsItems.Length; i++)
        {
            if (GameManager.saveData.unlockedSkins.Contains(i))
            {
                skinsItems[i].ActivateSkin();
            }
        }
        eventSystem.SetSelectedGameObject(skinsItems[GameManager.saveData.actualSkin].gameObject);
        checkImage.transform.position = skinsItems[GameManager.saveData.actualSkin].transform.position;
    }
    public void SetActualSkin(int skinNumber)
    {
        if (GameManager.saveData.unlockedSkins.Contains(skinNumber))
        {
            checkImage.transform.position = skinsItems[skinNumber].transform.position;
            GameManager.saveData.actualSkin = skinNumber;
            for (int i = 0; i < skinsGameobject.Length; i++)
            {
                if (i == skinNumber)
                {
                    skinsGameobject[i].SetActive(true);
                }
                else
                {
                    skinsGameobject[i].SetActive(false);
                }
            }
        }
        else
        {
        }
    }
    public void CloseSkinsMenu()
    {
        skinPanel?.SetActive(false);
        eventSystem.SetSelectedGameObject(skinsMenuButton);
    }
}
