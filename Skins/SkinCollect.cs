using UnityEngine;

public class SkinCollect : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private AudioSource skinSound;
    [SerializeField] private int skinNumber;
    [SerializeField] private SkinManager skinManager;

    private void Start()
    {
        if (GameManager.saveData.unlockedSkins.Contains(skinNumber))
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        UnlockSkin();
    }



    private void UnlockSkin()
    {
        skinSound.Play();
        particles.transform.position = transform.position;
        particles.Play();
        if (!GameManager.saveData.unlockedSkins.Contains(skinNumber))
        {
            GameManager.saveData.unlockedSkins.Add(skinNumber);
            if (GameManager.saveData.unlockedSkins.Count == 6)
            {
                SteamAchievements.TryToUnlockAchievements(4);
            }
        }
        skinManager.SetActualSkin(skinNumber);
        gameObject.SetActive(false);
    }
}
