using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public static class GameDataLogger
{
    private static GameSession currentSession;
    private static string csvFilePath;
    private static string csvHeader = "Nome,Ano_Nascimento,Genero,Data_Hora_Inicio," +
                                    "Pergunta_1,Resposta_Correta_1,Resposta_Selecionada_1,Pontuacao_Pergunta_1,Faixa_Pergunta_1,Faixa_Jogador_Pergunta_1,Trocas_Faixa_Pergunta_1,Faixa_Necessaria_Pergunta_1,Tempo_Resposta_Pergunta_1," +
                                    "Pergunta_2,Resposta_Correta_2,Resposta_Selecionada_2,Pontuacao_Pergunta_2,Faixa_Pergunta_2,Faixa_Jogador_Pergunta_2,Trocas_Faixa_Pergunta_2,Faixa_Necessaria_Pergunta_2,Tempo_Resposta_Pergunta_2," +
                                    "Pergunta_3,Resposta_Correta_3,Resposta_Selecionada_3,Pontuacao_Pergunta_3,Faixa_Pergunta_3,Faixa_Jogador_Pergunta_3,Trocas_Faixa_Pergunta_3,Faixa_Necessaria_Pergunta_3,Tempo_Resposta_Pergunta_3," +
                                    "Pontuacao_Total_Perguntas,Tempo_Total_Resposta,Dano_Obstaculo,Status_Final,Total_Perguntas";

    [System.Serializable]
    private class GameSession
    {
        public string nome;
        public int anoNascimento;
        public string genero;
        public DateTime dataHoraInicio;
        public List<QuestionData> perguntas = new List<QuestionData>();
        public int danoObstaculo = 0;
        public bool statusFinal = false;
        public int tempoTotalResposta = 0;
        public float tempoInicioPergunta;
        public int totalPerguntasSorteadas = 0;
    }

    [System.Serializable]
    private class QuestionData
    {
        public string texto;
        public string respostaCorreta;
        public string respostaSelecionada;
        public int pontuacao;
        public string faixaPergunta;
        public string faixaJogador;
        public int trocasFaixa;
        public string faixaNecessaria;
        public int tempoResposta;
    }

    public static void Initialize(string nome, int anoNascimento, string genero, string customPath = null)
    {
        try
        {
            string directoryPath = string.IsNullOrEmpty(customPath) ? Application.persistentDataPath : customPath;
            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            csvFilePath = Path.Combine(directoryPath, "dados_partidas.csv");
            
            Debug.Log("Caminho do arquivo CSV: " + csvFilePath);

            if (!File.Exists(csvFilePath))
            {
                File.WriteAllText(csvFilePath, csvHeader + Environment.NewLine, Encoding.UTF8);
            }

            currentSession = new GameSession
            {
                nome = nome,
                anoNascimento = anoNascimento,
                genero = genero,
                dataHoraInicio = DateTime.Now
            };
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao inicializar GameDataLogger: " + e.Message);
        }
    }

    public static void StartQuestion(string textoPergunta, string respostaCorreta, string faixaPergunta, string faixaNecessaria = null)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("Sessão não inicializada. Chamar Initialize() primeiro.");
            return;
        }

        if (currentSession.perguntas.Count >= 3)
        {
            Debug.LogWarning("Limite de 3 perguntas por sessão alcançado");
            return;
        }

        currentSession.tempoInicioPergunta = Time.time;
        currentSession.totalPerguntasSorteadas++;

        currentSession.perguntas.Add(new QuestionData
        {
            texto = textoPergunta,
            respostaCorreta = respostaCorreta,
            faixaPergunta = faixaPergunta,
            faixaNecessaria = faixaNecessaria ?? faixaPergunta,
            respostaSelecionada = "NÃO RESPONDIDA",
            pontuacao = 0,
            faixaJogador = "N/A",
            trocasFaixa = 0,
            tempoResposta = 0
        });

        Debug.Log($"Pergunta iniciada: {textoPergunta} | Resposta correta: {respostaCorreta} | Faixa: {faixaPergunta} | Faixa necessária: {faixaNecessaria ?? faixaPergunta}");
    }

    public static void RecordQuestionAnswer(int respostaSelecionadaIndex, string[] todasRespostas, string faixaAtual, object trocasFaixa)
    {
        if (currentSession == null || currentSession.perguntas.Count == 0)
        {
            Debug.LogWarning("Nenhuma pergunta ativa para registrar resposta");
            return;
        }

        int trocas = 0;
        if (trocasFaixa is int)
        {
            trocas = (int)trocasFaixa;
        }
        else if (trocasFaixa is string)
        {
            int.TryParse((string)trocasFaixa, out trocas);
        }

        var perguntaAtual = currentSession.perguntas[currentSession.perguntas.Count - 1];
        int tempoResposta = Mathf.RoundToInt(Time.time - currentSession.tempoInicioPergunta);

        perguntaAtual.respostaSelecionada = respostaSelecionadaIndex >= 0 ? 
            todasRespostas[respostaSelecionadaIndex] : "NÃO RESPONDEU";

        perguntaAtual.pontuacao = (respostaSelecionadaIndex >= 0 && 
            todasRespostas[respostaSelecionadaIndex] == perguntaAtual.respostaCorreta) ? 1 : 0;

        perguntaAtual.faixaJogador = faixaAtual;
        perguntaAtual.trocasFaixa = trocas;
        perguntaAtual.tempoResposta = tempoResposta;
        currentSession.tempoTotalResposta += tempoResposta;

        Debug.Log($"Resposta registrada: {perguntaAtual.respostaSelecionada} | Pontuação: {perguntaAtual.pontuacao} | Tempo: {tempoResposta}s | Trocas: {trocas}");
    }

    public static void RecordHit()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("Tentativa de registrar hit sem sessão ativa");
            return;
        }

        currentSession.danoObstaculo++;
        Debug.Log($"Dano registrado. Total: {currentSession.danoObstaculo}");
    }

    public static void SetGameResult(bool venceu)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("Tentativa de definir resultado sem sessão ativa");
            return;
        }

        currentSession.statusFinal = venceu;
        Debug.Log($"Resultado definido: {(venceu ? "Vitória" : "Derrota")}");
    }

    public static void SaveToCSV()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("Nenhuma sessão ativa para salvar");
            return;
        }

        if (string.IsNullOrEmpty(csvFilePath))
        {
            Debug.LogError("Caminho do arquivo CSV não definido");
            return;
        }

        try
        {
            string csvLine = ConvertSessionToCSVLine(currentSession);
            File.AppendAllText(csvFilePath, csvLine + Environment.NewLine, Encoding.UTF8);
            Debug.Log($"Dados salvos com sucesso em: {csvFilePath}");

            currentSession = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao salvar dados: {e.Message}\nStack Trace: {e.StackTrace}");
        }
    }

    private static string ConvertSessionToCSVLine(GameSession session)
    {
        StringBuilder sb = new StringBuilder();

        // Dados do jogador
        sb.Append(EscapeCsv(session.nome)).Append(",");
        sb.Append(session.anoNascimento).Append(",");
        sb.Append(EscapeCsv(session.genero)).Append(",");
        sb.Append(session.dataHoraInicio.ToString("yyyy-MM-dd HH:mm:ss")).Append(",");

        // Dados das perguntas (sempre 3 registros)
        for (int i = 0; i < 3; i++)
        {
            if (i < session.perguntas.Count)
            {
                var p = session.perguntas[i];
                sb.Append(EscapeCsv(p.texto)).Append(",");
                sb.Append(EscapeCsv(p.respostaCorreta)).Append(",");
                sb.Append(EscapeCsv(p.respostaSelecionada)).Append(",");
                sb.Append(p.pontuacao).Append(",");
                sb.Append(EscapeCsv(p.faixaPergunta)).Append(",");
                sb.Append(EscapeCsv(p.faixaJogador)).Append(",");
                sb.Append(p.trocasFaixa).Append(",");
                sb.Append(EscapeCsv(p.faixaNecessaria)).Append(",");
                sb.Append(p.tempoResposta).Append(",");
            }
            else
            {
                sb.Append(",,,,,,,0,,");
            }
        }

        // Dados finais da partida
        sb.Append(session.perguntas.Sum(p => p.pontuacao)).Append(",");
        sb.Append(session.tempoTotalResposta).Append(",");
        sb.Append(session.danoObstaculo).Append(",");
        sb.Append(session.statusFinal ? "1" : "0").Append(",");
        sb.Append(session.totalPerguntasSorteadas);

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(",") ? $"\"{value}\"" : value;
    }
}