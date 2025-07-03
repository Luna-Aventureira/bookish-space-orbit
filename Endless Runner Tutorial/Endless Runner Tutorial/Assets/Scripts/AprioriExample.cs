using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AprioriExample : MonoBehaviour
{
    public string csvFileName = "dados_partidas.csv";
    public int minSupport = 2; // mínimo de ocorrências para considerar um item frequente

    void Start()
    {
        List<List<string>> transactions = LoadTransactions();
        if (transactions.Count == 0)
        {
            Debug.LogWarning("Nenhuma transação foi carregada.");
            return;
        }

        Dictionary<HashSet<string>, int> frequentItemsets = Apriori(transactions, minSupport);
        PrintFrequentItemsets(frequentItemsets);
    }

    List<List<string>> LoadTransactions()
    {
        List<List<string>> transactions = new List<List<string>>();
        TextAsset csvFile = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(csvFileName));

        if (csvFile == null)
        {
            Debug.LogError("Arquivo CSV não encontrado em Resources.");
            return transactions;
        }

        string[] lines = csvFile.text.Split('\n');
        foreach (string line in lines.Skip(1)) // pula cabeçalho
        {
            string[] values = line.Split(',');

            // Garante que a linha tem 32 colunas esperadas (ajuste se for outro formato)
            if (values.Length != 32 || string.IsNullOrWhiteSpace(values[0])) continue;

            // Exemplo: Usar apenas respostas selecionadas
            var items = new List<string>();
            if (!string.IsNullOrWhiteSpace(values[5])) items.Add($"R1:{values[5]}");
            if (!string.IsNullOrWhiteSpace(values[13])) items.Add($"R2:{values[13]}");
            if (!string.IsNullOrWhiteSpace(values[21])) items.Add($"R3:{values[21]}");

            transactions.Add(items);
        }

        return transactions;
    }

    Dictionary<HashSet<string>, int> Apriori(List<List<string>> transactions, int minSupport)
    {
        Dictionary<HashSet<string>, int> result = new Dictionary<HashSet<string>, int>();
        var singleItems = transactions.SelectMany(t => t).GroupBy(i => i)
            .Where(g => g.Count() >= minSupport)
            .ToDictionary(g => new HashSet<string> { g.Key }, g => g.Count());

        var currentItemsets = singleItems;

        while (currentItemsets.Count > 0)
        {
            foreach (var itemset in currentItemsets)
                result[itemset.Key] = itemset.Value;

            var nextItemsets = new Dictionary<HashSet<string>, int>(HashSetComparer<string>.Instance);
            var currentKeys = currentItemsets.Keys.ToList();

            for (int i = 0; i < currentKeys.Count; i++)
            {
                for (int j = i + 1; j < currentKeys.Count; j++)
                {
                    var union = new HashSet<string>(currentKeys[i]);
                    union.UnionWith(currentKeys[j]);

                    if (union.Count != currentKeys[i].Count + 1) continue;

                    int count = transactions.Count(t => union.IsSubsetOf(t));
                    if (count >= minSupport)
                        nextItemsets[union] = count;
                }
            }

            currentItemsets = nextItemsets;
        }

        return result;
    }

    void PrintFrequentItemsets(Dictionary<HashSet<string>, int> frequentItemsets)
    {
        Debug.Log("Itens frequentes:");
        foreach (var itemset in frequentItemsets.OrderByDescending(i => i.Value))
        {
            string items = string.Join(", ", itemset.Key);
            Debug.Log($"{items} => Suporte: {itemset.Value}");
        }
    }
}

// Comparador para HashSet usado como chave de dicionário
public class HashSetComparer<T> : IEqualityComparer<HashSet<T>>
{
    public static readonly HashSetComparer<T> Instance = new HashSetComparer<T>();

    public bool Equals(HashSet<T> x, HashSet<T> y) => x.SetEquals(y);
    public int GetHashCode(HashSet<T> obj) => obj.Aggregate(0, (hash, item) => hash ^ item.GetHashCode());
}
