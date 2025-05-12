using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int cost;
    public GameObject description;

    [SerializeField] private GameObject checkPanel;
    [SerializeField] private GameObject lockPanel;
    [SerializeField] private GameObject costPanel;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource lockedSound;
    [SerializeField] private AudioSource soldSound;
    [SerializeField] private AudioSource deniedSound;
    [SerializeField] private ShopVoice voiceSounds;

    [HideInInspector] public bool sold = false;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public bool unlocked = false;




    private void Start()
    {
        position = transform.position;
    }

    public void GetItem(bool playSound)
    {
        lockPanel.SetActive(false);
        costPanel.SetActive(false);
        checkPanel.SetActive(true);
        unlocked = true;
        sold = true;
        if (playSound)
        {
            soldSound.Play();
            voiceSounds.PlayRandomBuy();
        }
    }
    public void UnlockItem()
    {
        lockPanel.SetActive(false);
        costPanel.SetActive(true);
        checkPanel.SetActive(false);
        unlocked = true;
        sold = false;
    }
    public void SelectItem()
    {
        anim.SetBool("Selected", true);
    }
    public void DeselectItem()
    {
        anim.SetBool("Selected", false);
    }
    public void PlayNoMoneyAnimation()
    {
        anim.SetTrigger("Money");
        deniedSound.Play();
    }
    public void PlayLockAnimation()
    {
        anim.SetTrigger("Lock");
        lockedSound.Play();
    }
}
