using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Declaracion Singleton
    public static AudioManager Instance;

    [Header("Audio Source References")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Audio Clip Arrays")]
    public AudioClip[] musicList;
    public AudioClip[] sfxList;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //No se destruye entre escenas
        }
        else
        {
            Destroy(gameObject); //Si ya hay un audioManager destruye el segundo
        }

    }

    public void PlayMusic(int musicIndex)
    {
        musicSource.clip = musicList[musicIndex];
        musicSource.Play();
    }
    public void Playsfx(int sfxIndex)
    {
        sfxSource.PlayOneShot(sfxList[sfxIndex]);
    }
}
