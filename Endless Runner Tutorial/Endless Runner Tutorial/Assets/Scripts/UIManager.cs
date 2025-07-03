using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public Image[] lifeHearts;
    public Text coinText;
    
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    public Text scoreText;

    [Header("UI de Perguntas")]
    public GameObject questionPanel;
    public Text perguntaText;
    public Text dicaText;
    public Text faixaEsquerdaText;
    public Text faixaCentralText;
    public Text faixaDireitaText;
    public GameObject planeObject;

    void Start()
    {
        // Chama HideQuestionUI apenas no início para garantir que os textos estejam invisíveis no começo
        HideQuestionUI();
        HideGameOver();
        HideGameWin();
    }

    public void ShowQuestionUI(string pergunta, string dica, string esquerda, string centro, string direita)
    {
        // Verifica se a pergunta não é vazia ou nula
        if (!string.IsNullOrEmpty(pergunta))
        {
            // Ativar os objetos da UI
            if (questionPanel != null)
            {
                questionPanel.SetActive(true);
                //Debug.Log("questionPanel ativado");
            }

            if (planeObject != null)
            {
                planeObject.SetActive(true);
                //Debug.Log("planeObject ativado");
            }

            // Atualizando os textos e garantindo que eles fiquem visíveis
            if (perguntaText != null)
            {
                perguntaText.text = pergunta;
                perguntaText.color = new Color(perguntaText.color.r, perguntaText.color.g, perguntaText.color.b, 1f); // visível
                //Debug.Log("perguntaText atualizado: " + perguntaText.text);
            }

            if (dicaText != null)
            {
                dicaText.text = dica;
                dicaText.color = new Color(dicaText.color.r, dicaText.color.g, dicaText.color.b, 1f); // visível
                //Debug.Log("dicaText atualizado: " + dicaText.text);
            }

            if (faixaEsquerdaText != null)
            {
                faixaEsquerdaText.text = "A. " + esquerda;
                faixaEsquerdaText.color = new Color(faixaEsquerdaText.color.r, faixaEsquerdaText.color.g, faixaEsquerdaText.color.b, 1f); // visível
                //Debug.Log("faixaEsquerdaText atualizado: " + faixaEsquerdaText.text);
            }

            if (faixaCentralText != null)
            {
                faixaCentralText.text = "B. " + centro;
                faixaCentralText.color = new Color(faixaCentralText.color.r, faixaCentralText.color.g, faixaCentralText.color.b, 1f); // visível
                //Debug.Log("faixaCentralText atualizado: " + faixaCentralText.text);
            }

            if (faixaDireitaText != null)
            {
                faixaDireitaText.text = "C. " + direita;
                faixaDireitaText.color = new Color(faixaDireitaText.color.r, faixaDireitaText.color.g, faixaDireitaText.color.b, 1f); // visível
                //Debug.Log("faixaDireitaText atualizado: " + faixaDireitaText.text);
            }
        }
        else
        {
            // Caso a pergunta seja nula ou vazia, oculta a UI
           // Debug.Log("Pergunta vazia ou nula, UI não será exibida.");
            HideQuestionUI();
        }
    }

    public void HideQuestionUI()
    {
        // Desativa os objetos da UI
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
            //Debug.Log("questionPanel desativado");
        }

        if (planeObject != null)
        {
            planeObject.SetActive(false);
            //Debug.Log("planeObject desativado");
        }

        // Tornando os textos invisíveis (com transparência 0)
        SetTextTransparency(perguntaText, 0f);
        SetTextTransparency(dicaText, 0f);
        SetTextTransparency(faixaEsquerdaText, 0f);
        SetTextTransparency(faixaCentralText, 0f);
        SetTextTransparency(faixaDireitaText, 0f);
    }

    // Função auxiliar para ajustar a transparência de qualquer texto
    private void SetTextTransparency(Text text, float alpha)
    {
        if (text != null)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            if (alpha == 0f)
            {
               // Debug.Log(text.name + " invisível");
            }
            else
            {
                //Debug.Log(text.name + " visível");
            }
        }
    }

    // Função para atualizar a quantidade de vidas
    public void UpdateLives(int lives)
    {
        for (int i = 0; i < lifeHearts.Length; i++)
        {
            lifeHearts[i].color = (lives > i) ? Color.white : Color.black;
        }
    }

    // Função para atualizar a quantidade de moedas
    public void UpdateCoins(int coin)
    {
        if (coinText != null) coinText.text = coin.ToString();
    }

    // Função para atualizar a pontuação
    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Score: " + score + "m";
    }

   /* // Função para exibir o painel de Game Over
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        gameWinPanel.SetActive(false);
    }

    public void ShowGameWin()
    {
        gameWinPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }*/


public void ShowGameOver()
{
    gameOverPanel.SetActive(true);
    gameWinPanel.SetActive(false);
    GameDataLogger.SaveToCSV(); // Salva os dados quando o jogo termina
    //Debug.Log("CSV salvo em: " + csvFilePath);
}

public void ShowGameWin()
{
    gameWinPanel.SetActive(true);
    gameOverPanel.SetActive(false);
    GameDataLogger.SaveToCSV(); // Salva os dados quando o jogo termina
   // Debug.Log("CSV salvo em: " + csvFilePath);
}


    // Função para esconder o painel de Game Over
    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
     public void HideGameWin()
    {
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
    }
}
