using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System;

public class MenuControlls : MonoBehaviour {
    private const float CAMERA_TRANSITION_SPEED = 3.0f;

    public GameObject shopButtonPrefab;
    public GameObject shopButtonContainer;
    public Material playerMaterial;
    public Text currencyText;
    public AudioMixer MusicMixer;
    public GameObject PanelSettings;
    [Serializable]
    public struct transformMenu
    {
        public string name;
        public Transform value;
    }
    public transformMenu[] listOfTransformMenu;

    private Transform cameraDesiredLookAt;
    private string cameraLastDesiredLookAt;
    private int[] costSkins = {0,150,150,150,
                             300,300,300,300,
                             450,600,750, 1000,
                             1250,1500,1750,2000};
    
    void Start () {
        // Cambio la skin del player
        ChangePlayerSkin(GameDirector.Instance.currentSkinIndex, false);
        // Setto il testo moneta con la corretta valuta
        SetCurrecyText();
        // Setto l'ultima vista della camera
        cameraLastDesiredLookAt = "Center";
        // Recupero e setto il volume salvato in precedenza
        MusicMixer.SetFloat("MusicBackgroundVolume", float.Parse(GameDirector.Instance.musicVolSettings));
        MusicMixer.SetFloat("MusicEffectsVolume", float.Parse(GameDirector.Instance.musicEffectsSettings));
        PanelSettings.transform.GetChild(0).GetChild(1).GetComponent<Slider>().value = float.Parse(GameDirector.Instance.musicVolSettings);
        PanelSettings.transform.GetChild(1).GetChild(1).GetComponent<Slider>().value = float.Parse(GameDirector.Instance.musicEffectsSettings);

        // Mi valorizzo le variabili che andrò ad utilizzare nel ciclo 
        string binarySkinAvailability = Convert.ToString(Convert.ToInt32(GameDirector.Instance.skinAvailability), 2);
        int textureIndex = 0;
        Sprite[] playerTextures = Resources.LoadAll<Sprite>("Player");
        foreach(Sprite texture in playerTextures)
        {
            // Istanzio il prefab del bottone
            GameObject container = Instantiate(shopButtonPrefab);
            // Aggiungo la texture al bottone
            container.GetComponent<Image>().sprite = texture;
            // Setto il bottone come figlio del contenitore "shopButtonContainer"
            container.transform.SetParent(shopButtonContainer.transform , false);
            // Aggiungo l'evento al click
            int index = textureIndex;
            container.GetComponent<Button>().onClick.AddListener(() => ChangePlayerSkin(index, true));
            // Verifico e nel caso inserisco se devo metterci il prezzo
            if (binarySkinAvailability.Length > index && (binarySkinAvailability.Substring(binarySkinAvailability.Length - (index + 1), 1)).Equals("1"))
            {
                container.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                container.transform.GetComponentInChildren<Text>().text = costSkins[index].ToString();
            }
            // Incremento l'indice e rinizio il ciclo 
            textureIndex++;
        }
	}
    void Update () {
        // Se necessario sposta la telecamera da un menu all'altro in maniera lenta
		if(cameraDesiredLookAt != null)
        {
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, cameraDesiredLookAt.rotation, CAMERA_TRANSITION_SPEED * Time.deltaTime);
        }
        // Se clicco spazio assumo il tasto Play Game
        if (Input.GetKeyDown(KeyCode.Space))
            LoadGame("Game");
        // Se clicco esc assumo il tasto Play Game
        else if (Input.GetKeyDown(KeyCode.Escape))
            ExitGame();
        // Se clicco la freccia a destra mi muovo verso destra
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Se sono al centro mi muovo a destra
            if (cameraLastDesiredLookAt == "Center")
                LookAtMenu("Right");
            // Se sono a sinistra mi muovo al centro 
            else if (cameraLastDesiredLookAt == "Left")
                LookAtMenu("Center");
        }
        // Se clicco la freccia a sinistra mi muovo verso sinistra
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Se sono al centro mi muovo a sinistra
            if (cameraLastDesiredLookAt == "Center")
                LookAtMenu("Left");
            // Se sono a destra mi muovo al centro 
            else if (cameraLastDesiredLookAt == "Right")
                LookAtMenu("Center");
        }

    }

    // Caricamento della scena di gioco
    public void LoadGame(string Scenes)
    {
        SceneManager.LoadScene(Scenes, LoadSceneMode.Single);
    }
    // Esco dal game
    public void ExitGame()
    {
        //GameDirector.Instance.messageBox(Camera.main, "Are you sure you want to exit " + Application.productName, "Yes");
        Application.Quit();
    }
    // Chiamo le funzioni per cercare e settare la nuova posizione della telecamera
    public void LookAtMenu(string menuTransform)
    {
        cameraLastDesiredLookAt = menuTransform;
        LookAtMenu(FindValueOfTransformMenu(menuTransform));
    }
    // Setto la musica in background
    public void SetMusicBackgroundVolume(float musicVolume)
    {
        MusicMixer.SetFloat("MusicBackgroundVolume", musicVolume);
        GameDirector.Instance.musicVolSettings = musicVolume.ToString();
    }
    // Setto gli effetti sonori
    public void SetMusicEffectsVolume(float musicVolume)
    {
        MusicMixer.SetFloat("MusicEffectsVolume", musicVolume);
        GameDirector.Instance.musicEffectsSettings = musicVolume.ToString();
    }

    // Setto il testo della moneta
    private void SetCurrecyText()
    {
        currencyText.text = "Currency: " + GameDirector.Instance.currency.ToString();
    }
    // Dato l'index setto la skin 
    private void ChangePlayerSkin(int index, bool saveLastIndexValue)
    {
        // Verifico se la skin è già disponibile o meno
        string binarySkinAvailability = Convert.ToString(Convert.ToInt32(GameDirector.Instance.skinAvailability), 2);
        if (binarySkinAvailability.Length > index && (binarySkinAvailability.Substring(binarySkinAvailability.Length - (index + 1), 1)).Equals("1"))
        {
            // Eseguo tutti i calcoli ecessare per scoprire quale skin è stata selezionata  
            float xVector = (index % 4) * 0.25f;
            float yVector = ((int)index / 4) * 0.25f;
            yVector = Mathf.Round(yVector * 100f) / 100f;
            if (yVector == 0.0f)
                yVector = 0.75f;
            else if (yVector == 0.25f)
                yVector = 0.50f;
            else if (yVector == 0.50f)
                yVector = 0.25f;
            else if (yVector == 0.75f)
                yVector = 0.00f;
            // Setto la skin
            playerMaterial.SetTextureOffset("_MainTex", new Vector2(xVector, yVector));
            // Se necessito il salvataggio della skin lo eseguo
            if (saveLastIndexValue)
                GameDirector.Instance.currentSkinIndex = index;
        }
        // Se non disponibile verifico se posso comprarla
        else
        {
            if (GameDirector.Instance.currency >= costSkins[index])
            {   // Se la posso comprare sottraggo il costo della skin
                GameDirector.Instance.currency -= costSkins[index];
                // Setto il testo delle monete
                SetCurrecyText();
                // Mi salvo che la skin è stata sbloccata 
                GameDirector.Instance.skinAvailability = (Convert.ToInt32(GameDirector.Instance.skinAvailability) + Mathf.Pow(2, index)).ToString(); ;
                // Setto a false l'oggetto che identificava il costo della skin
                shopButtonContainer.transform.GetChild(index).GetChild(0).gameObject.SetActive(false);
                // Cambio la skin al player
                ChangePlayerSkin(index, true);
            }

        }

    }
    // Setto che voglio spostare la telecamera
    private void LookAtMenu(Transform menuTransform)
    {
        cameraDesiredLookAt = menuTransform;
    }
    // Cerco la posizione(Transform) giusta nella lista composta da nome,valore
    private Transform FindValueOfTransformMenu(string valueToFind)
    {
        // Ciclo su tutte le strutture della lista
        foreach (transformMenu menuTransform in listOfTransformMenu)
        {
            //Se trovo il nome nella lista restituisco il corrispettivo valore 
            if (menuTransform.name == valueToFind)
                return menuTransform.value;
        }
        // Altrimenti restituisco null
        return null;
    }

}
