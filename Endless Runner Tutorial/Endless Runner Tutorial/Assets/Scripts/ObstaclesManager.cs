using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstaclesManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float questionDuration = 60f;
    public float timeBetweenPhases = 2f;
    public int totalQuestions = 3;
    public int totalObstacles = 3;
    public float obstacleSpeed = 5f;
    public float obstacleSpacing = 15f;
    public float obstacleDestroyDistance = 15f;
    public float minSpawnDistance = 30f;
    public float maxSpawnDistance = 45f;
    public int obstacleRowsBetweenQuestions = 5;

    [Header("References")]
    public Transform player;
    public Player playerController;
    public UIManager uiManager;
    public GameObject[] obstaclePrefabs;
    public float[] lanes = { -2f, 0f, 2f };
    public Pergunta[] questions;

    [System.Serializable]
    public class Pergunta
    {
        public string texto;
        public string[] respostas = new string[3];
        public int respostaCorreta;
    }

    private List<GameObject> activeObstacles = new List<GameObject>();
    private int currentQuestionIndex = 0;
    private bool isQuestionActive = false;
    private float questionTimer;
    private int correctLaneIndex = 0;
    private bool gameActive = true;
    private int obstaclesSpawned = 0;
    private int questionsAsked = 0;
    private List<string> phaseOrder = new List<string>();

    void Start()
    {
        InitializeReferences();
        ValidateQuestions();
        ShuffleQuestionOrder();
        CreateShuffledPhases();

        // Inicializa o GameDataLogger para gravar dados do jogador
        GameDataLogger.Initialize("NomeJogador", 1990, "Masculino");

        StartCoroutine(GameLoop());
    }

    void InitializeReferences()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<Player>();
            if (playerController == null)
            {
                Debug.LogError("Player not found in scene!");
                return;
            }
        }

        if (player == null && playerController != null)
        {
            player = playerController.transform;
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    void ValidateQuestions()
    {
        if (questions == null || questions.Length == 0)
        {
            CreateDefaultQuestions();
            Debug.LogWarning("No questions provided - using defaults");
        }
    }

    void ShuffleQuestionOrder()
    {
        for (int i = 0; i < questions.Length; i++)
        {
            int rand = Random.Range(i, questions.Length);
            var temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void CreateShuffledPhases()
    {
        phaseOrder.Clear();
        phaseOrder.Add("obstacle"); // First phase always obstacle

        List<string> remainingPhases = new List<string>();
        for (int i = 0; i < totalQuestions; i++) remainingPhases.Add("question");
        for (int i = 1; i < totalObstacles; i++) remainingPhases.Add("obstacle");

        for (int i = 0; i < remainingPhases.Count; i++)
        {
            int rand = Random.Range(i, remainingPhases.Count);
            string temp = remainingPhases[i];
            remainingPhases[i] = remainingPhases[rand];
            remainingPhases[rand] = temp;
        }

        phaseOrder.AddRange(remainingPhases);
    }

    IEnumerator GameLoop()
    {
        int currentPhaseIndex = 0;

        while (gameActive && !playerController.IsDead && currentPhaseIndex < phaseOrder.Count)
        {
            string currentPhase = phaseOrder[currentPhaseIndex];

            if (currentPhase == "question" && questionsAsked < totalQuestions)
            {
                yield return StartCoroutine(ShowQuestionPhase());
                questionsAsked++;
            }
            else if (currentPhase == "obstacle" && obstaclesSpawned < totalObstacles)
            {
                float spawnZ = player.position.z + Random.Range(minSpawnDistance, maxSpawnDistance);
                CreateObstacleRow(spawnZ);
                obstaclesSpawned++;
                yield return new WaitUntil(() => AllObstaclesPassed() || playerController.IsDead);
            }

            currentPhaseIndex++;
            yield return new WaitForSeconds(timeBetweenPhases);
        }

        if (!playerController.IsDead && playerController.GetCurrentLife() > 0)
        {
            EndGameWithVictory();
        }
        else
        {
            // Finalizar jogo com derrota
            GameDataLogger.SetGameResult(false);
            GameDataLogger.SaveToCSV();
        }
    }

    void EndGameWithVictory()
    {
        gameActive = false;

        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        CleanUpObstacles();
        GameDataLogger.SetGameResult(true);
        GameDataLogger.SaveToCSV();

        if (uiManager != null)
        {
            uiManager.ShowGameWin();
        }

        Invoke("ReturnToMenu", 3f);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu_01");
    }

    IEnumerator ShowQuestionPhase()
    {
        Pergunta currentQuestion = PrepareQuestion();
        isQuestionActive = true;
        questionTimer = questionDuration;

        // Inicia a pergunta no GameDataLogger
        GameDataLogger.StartQuestion(currentQuestion.texto, currentQuestion.respostas[currentQuestion.respostaCorreta], "FaixaExemplo");

        if (uiManager != null)
        {
            uiManager.ShowQuestionUI(
                currentQuestion.texto,
                "Use arrows to move and SPACE to confirm",
                currentQuestion.respostas[0],
                currentQuestion.respostas[1],
                currentQuestion.respostas[2]
            );
        }

        bool answered = false;
        while (!answered && questionTimer > 0f && gameActive && !playerController.IsDead)
        {
            questionTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckAnswer(playerController.GetCurrentLane());
                answered = true;
            }
            yield return null;
        }

        if (!answered && !playerController.IsDead)
        {
            CheckAnswer(-1); // Timeout
        }

        isQuestionActive = false;
        if (uiManager != null) uiManager.HideQuestionUI();
    }

    Pergunta PrepareQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            currentQuestionIndex = 0;
        }

        Pergunta currentQuestion = questions[currentQuestionIndex];

        // Embaralha respostas
        List<int> answerIndices = new List<int> { 0, 1, 2 };
        for (int i = 0; i < answerIndices.Count; i++)
        {
            int rand = Random.Range(i, answerIndices.Count);
            int temp = answerIndices[i];
            answerIndices[i] = answerIndices[rand];
            answerIndices[rand] = temp;
        }

        string[] shuffledAnswers = new string[3];
        int newCorrectIndex = 0;

        for (int i = 0; i < 3; i++)
        {
            shuffledAnswers[i] = currentQuestion.respostas[answerIndices[i]];
            if (answerIndices[i] == currentQuestion.respostaCorreta)
            {
                newCorrectIndex = i;
            }
        }

        currentQuestion.respostas = shuffledAnswers;
        currentQuestion.respostaCorreta = newCorrectIndex;
        correctLaneIndex = newCorrectIndex;

        currentQuestionIndex++;
        return currentQuestion;
    }

    void CheckAnswer(int selectedLane)
    {
        bool isCorrect = selectedLane == correctLaneIndex;

        // Corrigindo a chamada para o GameDataLogger
        GameDataLogger.RecordQuestionAnswer(
            selectedLane,
            questions[currentQuestionIndex - 1].respostas,
            selectedLane >= 0 ? selectedLane.ToString() : "No Answer",
            isCorrect ? "Correct" : "Incorrect"
        );

        if (isCorrect)
        {
            Debug.Log("Correct answer!");
        }
        else if (selectedLane == -1)
        {
            Debug.Log("Time out!");
            playerController.TakeDamage();
        }
        else
        {
            Debug.Log("Wrong answer!");
            playerController.TakeDamage();
        }
    }

    void CreateObstacleRow(float zPos)
    {
        int obstaclesToCreate = Random.Range(1, 3);
        List<int> availableLanes = new List<int> { 0, 1, 2 };

        // Embaralhar lanes
        for (int i = 0; i < availableLanes.Count; i++)
        {
            int rand = Random.Range(i, availableLanes.Count);
            int temp = availableLanes[i];
            availableLanes[i] = availableLanes[rand];
            availableLanes[rand] = temp;
        }

        for (int i = 0; i < obstaclesToCreate; i++)
        {
            int chosenLane = availableLanes[i];
            Vector3 pos = new Vector3(lanes[chosenLane], 0, zPos);

            GameObject newObstacle = Instantiate(
                obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)],
                pos,
                Quaternion.identity,
                transform
            );

            activeObstacles.Add(newObstacle);
        }
    }

    bool AllObstaclesPassed()
    {
        CleanUpObstacles();
        return activeObstacles.Count == 0;
    }

    void CleanUpObstacles()
    {
        if (player == null) return;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObstacles[i];
            if (obj == null || obj.transform.position.z < player.position.z - obstacleDestroyDistance)
            {
                if (obj != null) Destroy(obj);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void Update()
    {
        if (!gameActive || playerController.IsDead) return;

        if (!isQuestionActive && player != null)
        {
            foreach (GameObject obj in activeObstacles.ToArray())
            {
                if (obj != null)
                {
                    obj.transform.Translate(Vector3.back * obstacleSpeed * Time.deltaTime);
                }
            }
        }
    }

    void CreateDefaultQuestions()
    {
        questions = new Pergunta[]
        {
           new Pergunta() { texto = "Qual desses animais faz 'miau'?", respostas = new string[] { "Cachorro", "Gato", "Vaca" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Qual cor se forma misturando azul e amarelo?", respostas = new string[] { "Verde", "Laranja", "Roxo" }, respostaCorreta = 0 },
            new Pergunta() { texto = "Qual é o nome do personagem que vive num abacaxi no fundo do mar?", respostas = new string[] { "Sonic", "Bob Esponja", "Mickey" }, respostaCorreta = 1 },
            new Pergunta() { texto = "O que usamos para escovar os dentes?", respostas = new string[] { "Colher", "Pente", "Escova de dentes" }, respostaCorreta = 2 },
            new Pergunta() { texto = "Quantos dias tem uma semana?", respostas = new string[] { "5", "7", "10" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Quem é conhecido por voar com capa vermelha e ter superforça?", respostas = new string[] { "Superman", "Batman", "Homem-Aranha" }, respostaCorreta = 0 },
            new Pergunta() { texto = "Qual desses alimentos é uma fruta?", respostas = new string[] { "Maçã", "Arroz", "Pão" }, respostaCorreta = 0 },
            new Pergunta() { texto = "O que a água vira quando congela?", respostas = new string[] { "Vapor", "Gelo", "Areia" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Quem cuida dos dentes das pessoas?", respostas = new string[] { "Professor", "Dentista", "Padeiro" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Qual é o planeta em que vivemos?", respostas = new string[] { "Marte", "Terra", "Júpiter" }, respostaCorreta = 1 }
        };

         questions = new Pergunta_nivel2[]
        {
           new Pergunta() { texto = "Qual desses animais faz 'miau'?", respostas = new string[] { "Cachorro", "Gato", "Vaca" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Qual cor se forma misturando azul e amarelo?", respostas = new string[] { "Verde", "Laranja", "Roxo" }, respostaCorreta = 0 },
            new Pergunta() { texto = "Qual é o nome do personagem que vive num abacaxi no fundo do mar?", respostas = new string[] { "Sonic", "Bob Esponja", "Mickey" }, respostaCorreta = 1 },
            new Pergunta() { texto = "O que usamos para escovar os dentes?", respostas = new string[] { "Colher", "Pente", "Escova de dentes" }, respostaCorreta = 2 },
            new Pergunta() { texto = "Quantos dias tem uma semana?", respostas = new string[] { "5", "7", "10" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Quem é conhecido por voar com capa vermelha e ter superforça?", respostas = new string[] { "Superman", "Batman", "Homem-Aranha" }, respostaCorreta = 0 },
            new Pergunta() { texto = "Qual desses alimentos é uma fruta?", respostas = new string[] { "Maçã", "Arroz", "Pão" }, respostaCorreta = 0 },
            new Pergunta() { texto = "O que a água vira quando congela?", respostas = new string[] { "Vapor", "Gelo", "Areia" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Quem cuida dos dentes das pessoas?", respostas = new string[] { "Professor", "Dentista", "Padeiro" }, respostaCorreta = 1 },
            new Pergunta() { texto = "Qual é o planeta em que vivemos?", respostas = new string[] { "Marte", "Terra", "Júpiter" }, respostaCorreta = 1 }
        };
    }
}
