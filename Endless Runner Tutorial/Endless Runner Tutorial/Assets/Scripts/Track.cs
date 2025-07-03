using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    [Header("Variantes de pista")]
    public GameObject[] pistasVariacoes;
    public float tamanhoPista = 100f;

    private static Queue<Track> filaPistas = new Queue<Track>();
    private static bool inicializado = false;

    void Start()
    {
        if (!inicializado)
        {
            Track[] pistasIniciais = FindObjectsOfType<Track>();
            List<Track> pistasOrdenadas = new List<Track>(pistasIniciais);
            pistasOrdenadas.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

            foreach (Track pista in pistasOrdenadas)
                filaPistas.Enqueue(pista);

            inicializado = true;

            for (int i = 0; i < 2; i++)
            {
                Track pista = filaPistas.Dequeue();
                float novaZ = filaPistas.ToArray()[filaPistas.Count - 1].transform.position.z + tamanhoPista;
                pista.transform.position = new Vector3(0, 0, novaZ);
                pista.SortearVisual();
                filaPistas.Enqueue(pista);
            }
        }

    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (filaPistas.Peek() != this)
            return;

        Track pistaAtual = filaPistas.Dequeue();
        float novaZ = filaPistas.ToArray()[filaPistas.Count - 1].transform.position.z + tamanhoPista;
        pistaAtual.transform.position = new Vector3(0, 0, novaZ);

        pistaAtual.SortearVisual();

        filaPistas.Enqueue(pistaAtual);
    }

    void SortearVisual()
    {
        foreach (Transform filho in transform)
        {
            if (filho.CompareTag("GeneratedVisual"))
                Destroy(filho.gameObject);
        }

        if (pistasVariacoes.Length == 0) return;

        int index = Random.Range(0, pistasVariacoes.Length);
        GameObject nova = Instantiate(pistasVariacoes[index], transform);
        nova.transform.localPosition = Vector3.zero;
        nova.tag = "GeneratedVisual";
    }
}
