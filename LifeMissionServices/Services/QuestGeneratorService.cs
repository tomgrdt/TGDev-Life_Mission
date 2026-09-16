using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LifeMissionModels.Models;
using LifeMissionModels.Enums;

namespace LifeMissionServices.Services;

/// <summary>
/// Génère un sentier de quêtes via l'API Anthropic à partir d'une mission de vie.
/// IMPORTANT : ce service doit tourner côté serveur (Blazor Server, ou une API
/// derrière un client Blazor WebAssembly). La clé API ne doit jamais être
/// embarquée dans un binaire WASM livré au navigateur.
/// </summary>
public class QuestGeneratorService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _aiType;

    public QuestGeneratorService(HttpClient http, IConfiguration config, string aiType = "Gemini")
    {
        _http = http;
        _aiType = aiType;
        _apiKey = _aiType switch
        {
            "Anthropic" => config["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Clé manquante : ajoute Anthropic:ApiKey dans appsettings.json ou les User Secrets."),
            "Gemini" => config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Clé manquante : ajoute Gemini:ApiKey dans appsettings.json ou les User Secrets."),
            _ => string.Empty
        };
    }

    public async Task<List<QuestModel>> GenerateAsync(string mission)
    {
        var prompt = $$"""
            Tu es un guide d'expédition qui transforme une mission de vie en un sentier concret de 5 à 6 étapes.

            Mission de la personne : "{{mission}}"

            Réponds UNIQUEMENT avec un JSON valide (aucun texte avant ou après, pas de balises markdown), sous cette forme exacte :
            [
              {"title": "Nom court de l'étape (1-2 mots, façon carnet d'expédition)", "xp": 25, "desc": "Une phrase qui explique ce que cette étape apporte concrètement à la mission.", "subtasks": ["Action concrète 1", "Action concrète 2", "Action concrète 3"]}
            ]

            Contraintes :
            - 5 ou 6 étapes, progressives : des bases vers la maîtrise puis la transmission
            - xp entre 15 et 45, croissant avec la difficulté de l'étape
            - Chaque étape doit être spécifique à CETTE mission précise, pas générique
            - Chaque étape a exactement 3 subtasks, courtes, actionnables, réalisables en moins d'une semaine
            - Tout en français
            """;

        string url = string.Empty;
        string apiKeyHeader = string.Empty;
        var tools = new[] { new { type = "google_search" } };

        switch (_aiType)
        {
            case "Anthropic":
                url = "https://api.anthropic.com/v1/messages";
                apiKeyHeader = "x-api-key";
                break;
            case "Gemini":
                url = "https://generativelanguage.googleapis.com/v1beta/interactions";
                apiKeyHeader = "X-goog-api-key";
                break;
            default:
                break;
        }


        //var requestBody = new { Model = "claude-opus-5", MaxTokens = 1024, Messages = new[] { new { Role = "user", Content = prompt } } };
        var requestBody = new { model = "gemini-3.6-flash", input = prompt };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (_aiType == "Anthropic") request.Headers.Add("anthropic-version", "2023-06-01");
        request.Headers.Add(apiKeyHeader, _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LlmResponse>();
        //var payload = await response.Content.ReadAsStringAsync().ResultReadFromJsonAsync<LlmResponse>();
        var raw = string.Concat(payload?.Content.Select(c => c.Text) ?? Array.Empty<string>());
        var clean = raw.Replace("```json", "").Replace("```", "").Trim();

        var dtos = JsonSerializer.Deserialize<List<QuestModel>>(clean, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<QuestModel>();

        return dtos.Select((d, i) => new QuestModel
        {
            QuestId = $"q{i}",
            Title = d.Title,
            Xp = d.Xp,
            Desc = d.Desc,
            Subtasks = d.Subtasks.Select(s => new SubTaskModel{ Text = s.Text }).ToList(),
            Status = i == 0 ? QuestStatus.Active : QuestStatus.Locked
        }).ToList();
    }

    private class LlmResponse
    {
        [JsonPropertyName("content")]
        public List<ContentBlock> Content { get; set; } = new();
    }

    private class ContentBlock
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
}
