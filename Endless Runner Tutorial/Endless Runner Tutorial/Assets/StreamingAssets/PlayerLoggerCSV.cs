using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerLoggerCSV : MonoBehaviour
{
    string nomeArquivo = "dados_jogador.csv";
    string caminho;

    void Awake()
    {
        // Define o caminho completo
        caminho = Path.Combine(Application.streamingAssetsPath, nomeArquivo);

        // Garante que o arquivo exista com cabeçalho
        CriarCSVSeNaoExistir();
    }

    void CriarCSVSeNaoExistir()
    {
        if (!File.Exists(caminho))
        {
            // Cria o arquivo com cabeçalho
            using (StreamWriter sw = new StreamWriter(caminho))
            {
                sw.WriteLine("Nome,Pontuacao,Resposta");
            }
            Debug.Log("Arquivo CSV criado com cabeçalho em: " + caminho);
        }
    }

    public void SalvarDadosCSV(string nome, int pontuacao, string resposta)
    {
        using (StreamWriter sw = new StreamWriter(caminho, true))
        {
            string linha = $"{nome},{pontuacao},{resposta}";
            sw.WriteLine(linha);
        }

        Debug.Log("Dados adicionados ao CSV: " + caminho);
    }
}
