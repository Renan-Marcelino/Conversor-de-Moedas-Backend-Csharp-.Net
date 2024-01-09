using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

class ConversorMoeda
{
    private const string apiKey = "bf85cd4cc4157b1b81332b1b";
    private const string apiUrl = "https://api.exchangerate-api.com/v4/latest/";

    private static readonly List<string> MoedasDesejadas = new List<string> { "USD", "EUR", "BRL", "ARS", "JPY", "GBP" };

    static async Task Main()
    {
        Console.WriteLine("Conversor de Moeda");

        Console.Write("Digite o valor em sua moeda: ");
        double valorOriginal = Convert.ToDouble(Console.ReadLine());

        Console.Write("Digite o código da moeda de origem (ex: USD, EUR, BRL, ARS, JPY, GBP): ");
        string moedaOrigem = Console.ReadLine().ToUpper();

        Console.Write("Deseja converter para todas as moedas disponíveis? (S/N): ");
        bool converterParaTodas = Console.ReadLine().ToUpper() == "S";

        if (converterParaTodas)
        {
            Dictionary<string, double> taxasDeCambio = await ObterTodasAsTaxasDeCambio(moedaOrigem);

            if (taxasDeCambio.Count == 0)
            {
                Console.WriteLine($"Não foi possível obter taxas de câmbio para {moedaOrigem}.");
            }
            else
            {
                Console.WriteLine($"Conversões para {moedaOrigem}:");
                foreach (var taxa in taxasDeCambio)
                {
                    double valorConvertido = ConverterMoeda(valorOriginal, taxa.Value);
                    Console.WriteLine($"{valorOriginal:F2} {moedaOrigem} para {valorConvertido:F2} {taxa.Key}");
                }
            }
        }
        else
        {
            Console.Write("Digite o código da moeda de destino (ex: USD, EUR, BRL, ARS, JPY, GBP): ");
            string moedaDestino = Console.ReadLine().ToUpper();

            double taxaDeCambioOrigem = await ObterTaxaDeCambio(moedaOrigem, moedaDestino);

            if (taxaDeCambioOrigem > 0)
            {
                double valorConvertido = ConverterMoeda(valorOriginal, taxaDeCambioOrigem);
                Console.WriteLine($"Valor convertido: {valorConvertido:F2} {moedaDestino}");
            }
            else
            {
                Console.WriteLine($"Não foi possível obter a taxa de câmbio para a conversão de {moedaOrigem} para {moedaDestino}.");
            }
        }
    }

    static async Task<Dictionary<string, double>> ObterTodasAsTaxasDeCambio(string moedaOrigem)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                string url = $"{apiUrl}{moedaOrigem}?apikey={apiKey}";

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    ExchangeRateData exchangeRateData = JsonConvert.DeserializeObject<ExchangeRateData>(content);

                    Dictionary<string, double> taxasDesejadas = exchangeRateData.Rates
                        .Where(pair => MoedasDesejadas.Contains(pair.Key.ToUpper()))
                        .ToDictionary(pair => pair.Key, pair => pair.Value);

                    return taxasDesejadas;
                }
                else
                {
                    Console.WriteLine($"Erro ao obter taxas de câmbio para {moedaOrigem}. Status Code: {response.StatusCode}");
                    return new Dictionary<string, double>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter taxas de câmbio: {ex.Message}");
                return new Dictionary<string, double>();
            }
        }
    }

    static async Task<double> ObterTaxaDeCambio(string moedaOrigem, string moedaDestino)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                string url = $"{apiUrl}{moedaOrigem}?apikey={apiKey}";
                Console.WriteLine($"Solicitando taxas de câmbio para {moedaOrigem} na URL: {url}");

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    ExchangeRateData exchangeRateData = JsonConvert.DeserializeObject<ExchangeRateData>(content);

                    if (exchangeRateData.Rates.TryGetValue(moedaDestino, out double taxaDeCambio))
                    {
                        return taxaDeCambio;
                    }
                    else
                    {
                        Console.WriteLine($"Taxa de câmbio para {moedaDestino} não encontrada.");
                        return -1.0;
                    }
                }
                else
                {
                    Console.WriteLine($"Erro ao obter taxas de câmbio para {moedaOrigem}. Status Code: {response.StatusCode}");
                    return -1.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter taxas de câmbio: {ex.Message}");
                return -1.0;
            }
        }
    }

    static double ConverterMoeda(double valorOriginal, double taxaDeCambioDestino)
    {
        return valorOriginal * taxaDeCambioDestino;
    }
}

public class ExchangeRateData
{
    public string Base { get; set; }
    public DateTime Date { get; set; }
    public Dictionary<string, double> Rates { get; set; }
}

/* FIM */