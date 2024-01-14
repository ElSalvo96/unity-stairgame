using UnityEngine;

public class GameDirector : MonoBehaviour {
    private static GameDirector instance;
    public static GameDirector Instance { get { return instance; } }

    public int currency
    {
        get
        {
            return _currency;
        }
        set
        {
            if (_currency != value)
            {
                _currency = value;
                Save("Currency", _currency);
            }
        }
    }
    public int currentSkinIndex
    {
        get
        {
            return _currentSkinIndex;
        }
        set
        {
            if (_currentSkinIndex != value)
            {
                _currentSkinIndex = value;
                Save("CurrentSkinIndex", _currentSkinIndex);
            }
        }
    }
    public int scoreRecord
    {
        get
        {
            return _scoreRecord;
        }
        set
        {
            if (_scoreRecord != value)
            {
                _scoreRecord = value;
                Save("ScoreRecord", _scoreRecord);
            }
        }
    }
    public string skinAvailability
    {
        get
        {
            return _skinAvailability;
        }
        set
        {
            if (_skinAvailability != value)
            {
                _skinAvailability = value;
                Save("SkinAvailability", _skinAvailability);
            }
        }
    }
    public string musicVolSettings
    {
        get
        {
            return _musicVolSettings;
        }
        set
        {
            if (_musicVolSettings != value)
            {
                _musicVolSettings = value;
                Save("MusicVolSettings", _musicVolSettings);
            }
        }
    }
    public string musicEffectsSettings
    {
        get
        {
            return _musicEffectsSettings;
        }
        set
        {
            if (_musicEffectsSettings != value)
            {
                _musicEffectsSettings = value;
                Save("MusicEffectsSettings", _musicEffectsSettings);
            }
        }
    }

    private int _currency = 0;
    private int _currentSkinIndex = 0;
    private int _scoreRecord = 0;

    private string _skinAvailability = "1";
    private string _musicVolSettings = "-30";
    private string _musicEffectsSettings = "0";

    void Awake () {

        //Se instance è null allora è la priva volta che entro 
        if (instance == null)
        {
            // Setto l'oggetto che non si distrugge mai quando si passa da una scena all'altra
            DontDestroyOnLoad(gameObject);
            instance = this;
            transform.GetComponent<AudioSource>().Play();
            // Verifico che tutte le chiavi siano scritte. In caso contrario le setto con i valori di default
            CheckStartKey();
        }
    }
    private void FixedUpdate()
    {
        // L'oggetto Game Director deve seguire sempre la telecamera principale della scena
        transform.position = Camera.main.gameObject.transform.position;        
    }

    // Verifico che tutte le chiavi siano scritte. In caso contrario le setto con i valori di default
    private void CheckStartKey() {
        // Se la chiave "CurrentSkinIndex" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("CurrentSkinIndex"))
            PlayerPrefs.SetInt("CurrentSkinIndex", currentSkinIndex);
        else
            currentSkinIndex = PlayerPrefs.GetInt("CurrentSkinIndex");

        // Se la chiave "Currency" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("Currency"))
            PlayerPrefs.SetInt("Currency", currency);
        else
            currency = PlayerPrefs.GetInt("Currency");

        // Se la chiave "ScoreRecord" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("ScoreRecord"))
            PlayerPrefs.SetInt("ScoreRecord", scoreRecord);
        else
            scoreRecord = PlayerPrefs.GetInt("ScoreRecord");

        // Se la chiave "SkinAvailability" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("SkinAvailability"))
            PlayerPrefs.SetString("SkinAvailability", skinAvailability);
        else
            skinAvailability = PlayerPrefs.GetString("SkinAvailability");

        // Se la chiave "MusicVolSettings" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("MusicVolSettings"))
            PlayerPrefs.SetString("MusicVolSettings", musicVolSettings);
        else
            musicVolSettings = PlayerPrefs.GetString("MusicVolSettings");

        // Se la chiave "MusicEffectsSettings" non è mai stata scritta la scrivo con il valore di default altrimenti recupero quella già scritta
        if (!PlayerPrefs.HasKey("MusicEffectsSettings"))
            PlayerPrefs.SetString("MusicEffectsSettings", musicEffectsSettings);
        else
            musicEffectsSettings = PlayerPrefs.GetString("MusicEffectsSettings");
    }
    // Funzioni per salvataggio chiavi di registro
    private void Save(string name, string value)
    {
        PlayerPrefs.SetString(name, value);
        PlayerPrefs.Save();
    }
    private void Save(string name, float value)
    {
        PlayerPrefs.SetFloat(name, value);
        PlayerPrefs.Save();
    }
    private void Save(string name, int value)
    {
        PlayerPrefs.SetInt(name, value);
        PlayerPrefs.Save();
    }

}
