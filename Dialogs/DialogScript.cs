using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum DialogType
{
    weapon,
    falling,
    imBack,
    vietnam,
    afterFall
}

public class DialogScript : MonoBehaviour
{
    // TEXTS
    [SerializeField] private GameObject[] weaponTexts;
    [SerializeField] private GameObject[] imBackTexts;
    [SerializeField] private GameObject[] afterFallTexts;

    // SOUNDS
    [SerializeField] private AudioClip[] weaponSounds;
    [SerializeField] private AudioClip[] fallingSounds;
    [SerializeField] private AudioClip[] imBackSounds;
    [SerializeField] private AudioClip[] afterFallSounds;

    // REFERENCES
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private ClawController[] controller;
    [SerializeField] private CrabManager crabManager;
    [SerializeField] private PostProcessing processingScript;
    [SerializeField] private GameObject tutorial2;
    [SerializeField] private GameObject dialogBackground;
    private AudioSource audioSource;

    // BOOLEANS AND INTS
    private float originalEffectsVolume = 0f;
    private float originalMusicVolume = 0f;
    private const float REDUCTION_DB = 10f;
    private bool canShowDialog = true;
    private bool canShowImBackDialog = true;
    [HideInInspector] public bool canShowFallingDialog = true;

    // VIETNAM
    [SerializeField] private GameObject vietnamCam;
    [SerializeField] private GameObject vietnamImage;
    [SerializeField] private AudioClip vietnamSound;
    

    // SOUNDS INDEXES
    private int lastWeaponSound = -1;
    private int lastFallingSound = -1;
    private int lastImBackSound = -1;
    private int actualAfterFallIndex = 0;
    private int currentSoundPlayingIndex = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetOriginalVolumen();
    }

    
    public void OpenDialog(DialogType type)
    {
        switch (type)
        {
            case DialogType.weapon:
                if (canShowDialog && Random.value > 0.8f && !crabManager.fallingScript.isFalling)
                {
                    StartCoroutine(OpenWeaponDialog());
                }
                break;

            case DialogType.falling:
                if (canShowFallingDialog)
                {
                    InterruptDialog();
                    StartCoroutine(OpenFallingDialog());
                }
                break;

            case DialogType.imBack:
                if (canShowDialog && canShowImBackDialog)
                {
                    StartCoroutine(OpenImBackDialog());
                }
                break;
            case DialogType.vietnam:
                if (canShowDialog)
                {
                    if (crabManager.transform.position.y > 25)
                    {
                        GameManager.saveData.firstWeapon = false;
                        OpenDialog(DialogType.weapon);
                    }
                    else
                    {
                        StartCoroutine(OpenVietnamDialog());
                    }
                }
                break;
            case DialogType.afterFall:
                if (canShowDialog)
                {
                    StartCoroutine(OpenAfterFallDialog());
                }
                break;
        }
    }
    private IEnumerator OpenAfterFallDialog()
    {
        canShowDialog = false;

        yield return new WaitForSecondsRealtime(1);

        PlayDialogSound(afterFallSounds, ref actualAfterFallIndex, false);
        ActivateDialogUI(afterFallTexts);

        if (currentSoundPlayingIndex == 6)
        {
            processingScript.ActivateFireEffect();
        }
         
        yield return new WaitForSecondsRealtime(audioSource.clip.length);

        StopCurrentDialog(afterFallTexts);
        if (currentSoundPlayingIndex == 6)
        {
            processingScript.DeactivateFireEffect();
        }

        yield return new WaitForSecondsRealtime(1);

        canShowDialog = true;
    }

    
    private IEnumerator OpenVietnamDialog()
    {
        canShowDialog = false;

        StartVietnamCinematic();

        bool wasTutoActive = CheckTutorialStatus();

        yield return new WaitForSecondsRealtime(audioSource.clip.length - 1);

        lastWeaponSound = 0;
        PlayDialogSound(weaponSounds, ref lastWeaponSound, false);
        ActivateDialogUI(weaponTexts);
        StopVietnamCinematic();
        if (wasTutoActive)
        {
            tutorial2.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(audioSource.clip.length);

        StopCurrentDialog(weaponTexts);

        yield return new WaitForSecondsRealtime(1);

        canShowDialog = true;
    }

    
    private IEnumerator OpenWeaponDialog()
    {
        canShowDialog = false;
       
        PlayDialogSound(weaponSounds, ref lastWeaponSound, true);
        ActivateDialogUI(weaponTexts);

        yield return new WaitForSecondsRealtime(audioSource.clip.length);

        StopCurrentDialog(weaponTexts);


        yield return new WaitForSecondsRealtime(1);

        canShowDialog = true;
    }

    private IEnumerator OpenFallingDialog()
    {
        canShowDialog = false;
        canShowFallingDialog = false;
        PlayDialogSound(fallingSounds, ref lastFallingSound, true);

        yield return new WaitForSecondsRealtime(audioSource.clip.length);

        SetMusicVolume(1);
        SetEffectsVolume(1);

        yield return new WaitForSecondsRealtime(1);

        canShowDialog = true;
    }
    private IEnumerator OpenImBackDialog()
    {
        canShowDialog = false;
        canShowImBackDialog = false;
        
        PlayDialogSound(imBackSounds, ref lastImBackSound, true);
        ActivateDialogUI(imBackTexts);

        yield return new WaitForSecondsRealtime(audioSource.clip.length);

        StopCurrentDialog(imBackTexts);

        yield return new WaitForSecondsRealtime(1);

        canShowDialog = true;

        yield return new WaitForSecondsRealtime(5);

        canShowImBackDialog = true;

    }


    public void SetEffectsVolume(float volume)
    {
        ApplyVolume("EffectsVolume", volume, originalEffectsVolume);
    }

    public void SetMusicVolume(float volume)
    {
        ApplyVolume("MusicVolume", volume, originalMusicVolume);
    }

    private void ApplyVolume(string parameterName, float volume, float originalVolume)
    {
        if (volume == 0f)
        {
            audioMixer.SetFloat(parameterName, -80f);
        }
        else if (volume == 0.5f)
        {
            audioMixer.SetFloat(parameterName, Mathf.Max(originalVolume - REDUCTION_DB, -80f));
        }
        else if (volume == 1f)
        {
            audioMixer.SetFloat(parameterName, originalVolume);
        }
    }
    private int GetUniqueRandomIndex(int arrayLength, ref int lastIndex)
    {
        int newIndex;
        do
        {
            newIndex = Random.Range(0, arrayLength);
        } while (newIndex == lastIndex);

        lastIndex = newIndex;

        return newIndex;
    }
    public void HandleDialogsOnGrab(GameObject grabbedObject) //CALLED FROM CLAWCONTROLLER WHEN GRAB
    {
        if (crabManager.fallingScript.rememberLastFall && grabbedObject == crabManager.fallingScript.lastObjectGrabbed)
        {
            OpenDialog(DialogType.imBack);
            crabManager.fallingScript.rememberLastFall = false;
        }
        if (crabManager.fallingScript.isFalling)
        {
            crabManager.fallingScript.isFalling = false;
            canShowFallingDialog = true;
            InterruptDialog();
            if (crabManager.fallingScript.bigFall)
            {
                if (!grabbedObject.CompareTag("MovingObject"))
                {
                   OpenDialog(DialogType.afterFall);
                }
                crabManager.fallingScript.bigFall = false;
            }
        }
        crabManager.fallingScript.StopFalling();
    }
    public void InterruptDialog()
    {
        StopAllCoroutines();
        audioSource.Stop();
        canShowDialog = true;
        canShowFallingDialog = true;
        canShowImBackDialog = true;
        vietnamCam.SetActive(false);
        DisableAllDialogTexts();
        dialogBackground.SetActive(false);
        SetMusicVolume(1);
        SetEffectsVolume(1);
        processingScript.ResetEffects();
    }

    private void DisableAllDialogTexts()
    {
        foreach (GameObject text in weaponTexts)
        {
            text.SetActive(false);
        }
        
        foreach (GameObject text in imBackTexts)
        {
            text.SetActive(false);
        }
        foreach (GameObject text in afterFallTexts)
        {
            text.SetActive(false);
        }
    }

    public void SetOriginalVolumen()
    {
        originalEffectsVolume = Mathf.Log10(GameManager.saveData.effectsVolume) * 20;
        originalMusicVolume = Mathf.Log10(GameManager.saveData.musicVolume) * 20;
    }
    public void ShowWeaponDialog()
    {
        if (GameManager.saveData.firstWeapon)
        {
            OpenDialog(DialogType.vietnam);
        }
        else
        {
            OpenDialog(DialogType.weapon);
        }
    }
    private bool CheckTutorialStatus()
    {
        bool wasTutoActive = false;
        if (tutorial2.activeSelf)
        {
            wasTutoActive = true;
            tutorial2.SetActive(false);
        }

        return wasTutoActive;
    }

    private void StopVietnamCinematic()
    {
        vietnamCam.SetActive(false);
        vietnamImage.SetActive(false);
        GameManager.state = GameState.Play;
    }

    private void StartVietnamCinematic()
    {
        GameManager.saveData.firstWeapon = false;
        GameManager.state = GameState.Cinematic;
        controller[0].moveDirection = Vector2.zero;
        controller[1].moveDirection = Vector2.zero;
        audioSource.clip = vietnamSound;
        audioSource.Play();
        vietnamCam.SetActive(true);
        vietnamImage.SetActive(true);
        SetMusicVolume(0);
        SetEffectsVolume(0.5f);
    }

    private void PlayDialogSound(AudioClip[] sounds, ref int lastSoundIndex, bool isRandom = false)
    {
        if (!isRandom)
        {
            currentSoundPlayingIndex = lastSoundIndex;
            audioSource.clip = sounds[currentSoundPlayingIndex];
            audioSource.Play();
            lastSoundIndex = lastSoundIndex + 1;
            if (lastSoundIndex == sounds.Length)
            {
                lastSoundIndex = 0;
            }
        }
        else
        {
            currentSoundPlayingIndex = GetUniqueRandomIndex(sounds.Length, ref lastSoundIndex);
            audioSource.clip = sounds[currentSoundPlayingIndex];
            audioSource.Play();
        }
        SetMusicVolume(0.5f);
        SetEffectsVolume(0.5f);
    }
    private void ActivateDialogUI(GameObject[] texts)
    {
        texts[currentSoundPlayingIndex].SetActive(true);
        dialogBackground.SetActive(true);
    }
    private void StopCurrentDialog(GameObject[] textArray)
    {
        textArray[currentSoundPlayingIndex].SetActive(false);
        SetMusicVolume(1);
        SetEffectsVolume(1);
        dialogBackground.SetActive(false);
    }
}
