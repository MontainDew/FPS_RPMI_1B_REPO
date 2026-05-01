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

    [Header("Music References")]
    [SerializeField] GameObject Clock;
    [SerializeField] EnemyAiBase Enemy;
    [SerializeField] AudioClip MusicaActiva;
    [SerializeField] AudioClip nuevaMusica;

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

    private void Update()
    {
        if (Clock.activeInHierarchy)
        {
            nuevaMusica = musicList[3];
        } 
        else if (Enemy.inChase)
        {
            nuevaMusica = musicList[2];
        }
        else
        {
            nuevaMusica = musicList[1];
        }

        if (MusicaActiva != nuevaMusica)
        {
            CambiarMusica(nuevaMusica);
        }
    }

    void CambiarMusica(AudioClip cambio)
    {
        MusicaActiva = cambio;
        musicSource.Stop();
        musicSource.clip = cambio;
        musicSource.Play();
    }
}
