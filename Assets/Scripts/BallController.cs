using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour {
    private static BallController instance;
    public static BallController Instance { get { return instance; } }

    public Text scoreText;
    public Text recordScoreText;

    public int score;
    public bool gameOver;

    void Awake () {
        // Inizializzo le variabili
        score = 0;
        gameOver = false;
        instance = this;
        recordScoreText.text = "Record: " + GameDirector.Instance.scoreRecord.ToString();
    }
    void OnTriggerEnter(Collider other)
    {
        // Se il trigger etra in contatto con l'ostacolo setto gameOver = true
        if (other.tag == "Obstacle")
        {
            gameOver = true;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        // se collido con il piano aumento di 1 lo score e aggiorno il testo
        if (collision.transform.tag == "Plane")
        {
            score += 1;
            scoreText.text = "Score: " + score;

        }
    }

}
