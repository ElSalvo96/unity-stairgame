using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private const int NUMBER_OF_INITIALIZING_STAIRS = 11;
    private const float TIME_AFTER_SPAWN_NEW_PLANE = 0.5f;
    private const float TIME_OF_RESTART_GAME_AFTER_LOSE = 2f;
    private const float INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_X = 0f;
    private const float INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Y = 1f;
    private const float INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Z = 2f;
    private const float MAX_HIGHT_JUMP_BALL = 4.0f;
    
    public GameObject parentColliderPrefab;
    public GameObject planePrefab;
    public GameObject obstaclePrefab;
    public GameObject explosionPrefab;
    public Text gameOverText;


    public float minLeft;
    public float maxRight;

    private Rigidbody rbPlayer;
    private float[] xPosObstacles = { 3f, 1.5f, 0f, -1.5f, -3f};
    private bool gameInRun;

    private float speedY;
    private float speedZ;
    private float timeToLose;
    Vector3 lastSpawnPosition;
    Vector3 lastPositionPlayer;

    void Start()
    {
        // Recupero il componente RigidBody dellla palla
        rbPlayer = BallController.Instance.gameObject.GetComponent<Rigidbody>();
        // Setto che il gioco non è attivo 
        gameInRun = false;
        // Setto il testo gameOver vuoto e lo disattivo
        gameOverText.text = "";
        gameOverText.gameObject.SetActive(false);
        // Calcolo la gravità necessaria per far saltare la palli nel tempo prefissato fino ad un altezza prefissata
        float gravitaY = -8 * MAX_HIGHT_JUMP_BALL / Mathf.Pow(TIME_AFTER_SPAWN_NEW_PLANE,2);
        Physics.gravity = new Vector3(Physics.gravity.x, gravitaY, Physics.gravity.z);

        // Mi setto le posizione da cui partire per calcolare lo spawn del piano e il movimento della palla 
        lastSpawnPosition = new Vector3(INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_X,
                                        -INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Y,
                                        -INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Z);
        lastPositionPlayer = new Vector3(lastSpawnPosition.x,
                                         lastSpawnPosition.y + planePrefab.transform.localScale.y / 2 + rbPlayer.transform.localScale.y / 2,
                                         lastSpawnPosition.z);
        // Mi calcolo la velocità da applicare sulle Y come 4 volte l'altezza del salto diviso il tempo del salto
        speedY = 4 * MAX_HIGHT_JUMP_BALL / TIME_AFTER_SPAWN_NEW_PLANE;
        // Mi calcolo la velocità da applicare sulle Z come spazio / tempo
        speedZ = INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Z / TIME_AFTER_SPAWN_NEW_PLANE;

        // Creo i piani iniziali
        for (int i = 0; i < NUMBER_OF_INITIALIZING_STAIRS; i++)
        {
            lastSpawnPosition = spawnPlane(lastSpawnPosition, false);
        }

    }
    void Update()
    {
        // Al primo giro faccio partire il gioco 
        if (!gameInRun && !BallController.Instance.gameOver)
        {
            rbPlayer.useGravity = true;
            gameInRun = true;
            StartCoroutine(spawnPlanes());
        }

        // Se sono ancora in run e ho perso
        if (BallController.Instance.gameOver && gameInRun)
        {
            // Levo la gravità alla pallina
            rbPlayer.useGravity = false;
            // Setto che non siamo più in run
            gameInRun = false;
            // Instanzio l'esplosione della pallina e disattivo il giocatore
            Instantiate(explosionPrefab.GetComponent<ParticleSystem>(), rbPlayer.transform.position + Vector3.up / 2, Quaternion.identity);
            rbPlayer.gameObject.SetActive(false);
            // Emetto il suono relativo all'esplosione
            transform.GetComponent<AudioSource>().Play();
            // Setto il testo del gameOver e lo attivo
            gameOverText.text = "Game Over!";
            gameOverText.gameObject.SetActive(true);
            timeToLose = Time.time;
        }

        // Quando ho perso qualsiasi tasto cliccato dopo "TIME_OF_RESTART_GAME_AFTER_LOSE" mi rimanda al menu principale
        if (BallController.Instance.gameOver && Input.anyKey && (Time.time - timeToLose >= TIME_OF_RESTART_GAME_AFTER_LOSE))
        {
            // Se ho superato il record lo salvo
            if (GameDirector.Instance.scoreRecord <= BallController.Instance.score)
                GameDirector.Instance.scoreRecord = BallController.Instance.score;
            // Aggiungo le monete in maniera random da un minimo di 2 volte il punteggio a un massimo di 5 volte il punteggio
            GameDirector.Instance.currency = GameDirector.Instance.currency + Random.Range(BallController.Instance.score * 2, BallController.Instance.score * 5);
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }
    void FixedUpdate()
    {
        // Recupero lo spostamento sull'asse delle x e lo applico verificando che non superi i limiti impostati
        float horizontalMovement = Input.GetAxis("Horizontal");
        rbPlayer.transform.position = new Vector3(Mathf.Clamp(rbPlayer.transform.position.x + horizontalMovement * 0.5f, minLeft, maxRight), rbPlayer.transform.position.y, rbPlayer.transform.position.z);
        //Se il gioco è in run incomincio a spostare il GameController e tutti i suoi figli
        if (gameInRun)
            // L'asse delle x rimane sempre quello di partenza, 
            transform.position = new Vector3(transform.position.x,
                                             //l'asse delle y corrisponde al valore massimo che può assumere in quell'istante (Dato come posizione attuale + 2 volte il deltaTime)
                                             Mathf.Clamp(rbPlayer.transform.position.y, transform.position.y, transform.position.y + Time.deltaTime * 2),
                                             //l'asse delle z ad ogni FixedUpdate corrisponde a quello della pallina
                                             rbPlayer.transform.position.z);
    }
    void OnTriggerEnter(Collider other)
    {   // Quando collido con il piano distruggo il babbo cosi da eliminare sia il piano che gli ostacoli
        if (other.tag == "Plane")
        {
            Destroy(other.transform.parent.gameObject);
        }
    }

    // CooRoutine per spawn del piano e per il salto della pallina
    IEnumerator spawnPlanes()
    {
        // Finchè la pallina non setta gameOver a true continuo ad andare avanti
        while (!BallController.Instance.gameOver)
        {
            // Creo piano e ostacoli
            lastSpawnPosition = spawnPlane(lastSpawnPosition,true);
            // Sistemo l'errore dovuto al salto precendente (L'asse delle X è mosso dal giocatore quindi non lo sistemo mai)
            lastPositionPlayer = new Vector3(rbPlayer.transform.position.x,
                                             lastPositionPlayer.y + INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Y ,
                                             lastPositionPlayer.z + INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Z);
            rbPlayer.transform.position = lastPositionPlayer;
            // Applico la velocità al RigidBody
            rbPlayer.velocity = new Vector3(0, speedY, speedZ);

            // Attendo il tempo prefissato per spawnare un nuovo piano ("TIME_AFTER_SPAWN_NEW_PLANE")
            yield return new WaitForSecondsRealtime(TIME_AFTER_SPAWN_NEW_PLANE);
        }
    }

    // Funzione per spawnare un nuovo piano a partire dalla posizione del precedente
    private Vector3 spawnPlane(Vector3 lastSpawnPosition, bool spawnObstacles)
    {
        // Mi calcolo la nuova posizione
        Vector3 spawnPosition = new Vector3(lastSpawnPosition.x + INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_X,
                                            lastSpawnPosition.y + INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Y,
                                            lastSpawnPosition.z + INCREMENT_OF_NEXT_PLANE_RESPECT_BEFORE_IN_Z);
        // Istanzio il padre che andra a contenente il piano e gli ostacoli
        GameObject parentCollider = Instantiate(parentColliderPrefab, spawnPosition, Quaternion.identity);
        // Istanzio il piano e gli setto il padre
        GameObject plane = Instantiate(planePrefab);
        plane.transform.SetParent(parentCollider.transform, false);
        // Se devo spawnare gli ostacoli chiamo l'apposita funzione
        if (spawnObstacles)
            SpawnObstacle(parentCollider.transform);

        // Esco restituendo la posizione del nuovo piano
        return spawnPosition;
    }
    // Funzione per spawnare gli ostacoli
    private void SpawnObstacle(Transform parentCollider)
    {
        // Mi salvo dove inserire gli ostacoli chiamando una funzione che me li calcoli in maniera random
        ArrayList listOfObstacles = RandomNumbers(0, 4, Random.Range(1, 5));
        // Per ogli ostacolo da inserire
        for (int index = 0; index < listOfObstacles.Count; index++)
        {
            // Mi calcolo la posizione
            Vector3 spawnPosition = new Vector3(parentCollider.position.x + xPosObstacles[(int)listOfObstacles[index]],
                                                planePrefab.transform.position.y / 2 + obstaclePrefab.transform.localScale.y,
                                                0f);
            // Lo istanzio nella scena
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
            // Gli setto lo stesso padre del piano sul quale poggiano
            obstacle.transform.SetParent(parentCollider, false);
        }

    }
    // Mi calcolo le posizioni random
    private ArrayList RandomNumbers(int FromNum, int ToNum, int NumOfItem)
    {
        ArrayList lstNumbers = new ArrayList();
        int number;
        int count = 0;
        // Ciclo fino al numero massimo di ostacoli da creare
        do
        {
            // Mi calcolo un numero in maniera random
            number = Random.Range(FromNum, ToNum + 1);
            // Se quel numero è già nella schiera non lo aggiungo
            if (!lstNumbers.Contains(number))
            {   // Se è nuovo incremento anche l'indice
                lstNumbers.Add(number);
                count++;
            }
            
        } while (count < NumOfItem);
        // Restituisco la lista di numeri random
        return lstNumbers;
    }
    
}
