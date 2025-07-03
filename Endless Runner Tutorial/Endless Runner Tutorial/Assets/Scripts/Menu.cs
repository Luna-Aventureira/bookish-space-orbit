using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class Menu : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private InputField inputNome;
    [SerializeField] private InputField inputAno;
    [SerializeField] private Toggle toggleFeminino;
    [SerializeField] private Toggle toggleMasculino;
    [SerializeField] private Toggle togglePrefiroNaoDizer;
    [SerializeField] private Toggle toggleOutro;
    [SerializeField] private InputField inputGeneroCustom;
    [SerializeField] private ToggleGroup generoToggleGroup;
    [SerializeField] private Button playButton;
    [SerializeField] private Text errorText;

    [Header("Configurações")]
    [SerializeField] private string gameSceneName = "01_CorreCutia";
    [SerializeField] private float errorDisplayTime = 3f;

    private void Start()
    {
        // Impede múltiplas inscrições do evento
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayButtonClick);

        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }

        if (generoToggleGroup != null)
        {
            toggleFeminino.group = generoToggleGroup;
            toggleMasculino.group = generoToggleGroup;
            togglePrefiroNaoDizer.group = generoToggleGroup;
            toggleOutro.group = generoToggleGroup;

            StartCoroutine(DesligarTogglesDepoisDeUmFrame());
        }

        if (toggleOutro != null)
        {
            toggleOutro.onValueChanged.AddListener(OnToggleOutroChanged);
            OnToggleOutroChanged(toggleOutro.isOn);
        }

        if (inputAno != null)
        {
            inputAno.contentType = InputField.ContentType.IntegerNumber;
            inputAno.onValidateInput += ValidateIntegerInput;
        }
    }

    private System.Collections.IEnumerator DesligarTogglesDepoisDeUmFrame()
    {
        yield return null;
        generoToggleGroup.SetAllTogglesOff();
    }

    private char ValidateIntegerInput(string text, int charIndex, char addedChar)
    {
        return char.IsDigit(addedChar) ? addedChar : '\0';
    }

    private void OnToggleOutroChanged(bool isOn)
    {
        if (inputGeneroCustom != null)
        {
            inputGeneroCustom.gameObject.SetActive(isOn);
            if (!isOn)
                inputGeneroCustom.text = "";
        }
    }

    public void OnPlayButtonClick()
    {
        Debug.Log("Botão foi clicado1!");

        if (!ValidateInputs())
            return;

        try
        {
            string downloadsPath = GetDownloadsPath();
            InitializeGameData(downloadsPath);
            LoadGameScene();
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao iniciar jogo: {e.Message}");
            ShowError("Erro ao iniciar. Tente novamente.");
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(inputNome?.text))
        {
            ShowError("Informe seu nome!");
            return false;
        }

        if (string.IsNullOrEmpty(inputAno?.text) || !int.TryParse(inputAno.text, out int year) || year < 1900 || year > DateTime.Now.Year)
        {
            ShowError($"Informe um ano válido (1900-{DateTime.Now.Year})!");
            return false;
        }

        return true;
    }

    private string GetDownloadsPath()
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            return Directory.Exists(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }

    private void InitializeGameData(string customPath)
    {
        string generoFinal = "";

        if (toggleFeminino.isOn)
            generoFinal = "Feminino";
        else if (toggleMasculino.isOn)
            generoFinal = "Masculino";
        else if (togglePrefiroNaoDizer.isOn)
            generoFinal = "Prefiro não dizer";
        else if (toggleOutro.isOn && !string.IsNullOrWhiteSpace(inputGeneroCustom.text))
            generoFinal = inputGeneroCustom.text.Trim();

        GameDataLogger.Initialize(
            inputNome.text.Trim(),
            int.Parse(inputAno.text),
            generoFinal,
            customPath
        );
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Nome da cena do jogo não configurado!");
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            Invoke(nameof(HideError), errorDisplayTime);
        }
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }
}
