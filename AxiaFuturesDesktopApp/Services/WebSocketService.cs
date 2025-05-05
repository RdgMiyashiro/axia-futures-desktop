using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace AxiaFuturesDesktopApp.Services
{
    public class WebSocketService
    {
        private ClientWebSocket _webSocket = new ClientWebSocket();
        public event Action<Models.Message> OnMessageReceived;

        public async Task ConnectAsync(string url)
        {
            try
            {
                await _webSocket.ConnectAsync(new Uri(url), CancellationToken.None);
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke(new Models.Message
                {
                    DisplayText = $"Erro na conexão: {ex.Message}",
                    TtsText = string.Empty
                });
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 32];
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (message.Contains("ping"))
                    {
                        await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("pong")), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else if (message.Contains("message"))
                    {
                        var formattedMessage = FormatMessage(message);
                        OnMessageReceived?.Invoke(formattedMessage);
                    }
                }
                catch
                {
                    var errorMessage = new Models.Message
                    {
                        DisplayText = "Conexão perdida. Tentando reconectar...",
                        TtsText = string.Empty
                    };
                    OnMessageReceived?.Invoke(errorMessage);
                    await ReconnectAsync();
                }
            }
        }

        private Models.Message FormatMessage(string json)
        {
            try
            {
                var jObject = JObject.Parse(json);
                var contentArray = jObject["content"] as JArray;
                if (contentArray == null || contentArray.Count == 0)
                {
                    return new Models.Message
                    {
                        DisplayText = "Mensagem vazia recebida.",
                        TtsText = string.Empty
                    };
                }

                var contentObj = contentArray[0];
                var headline = contentObj["headline"]?.ToString() ?? "";
                var dateTimeString = contentObj["dateTimeString"]?.ToString() ?? "";
                var rawContent = contentObj["content"]?.ToString() ?? "";

                // Limpar o conteúdo: remover &#BREAK#& e URLs
                var cleanContent = rawContent
                    .Replace("&#BREAK#&", " ");

                // Formatar para UI: [data] headline: conteúdo
                var displayText = $"[{dateTimeString}] {headline}: {cleanContent}".Trim();

                // Formatar para TTS: apenas headline e conteúdo
                var ttsText = $"{headline}. {cleanContent}".Trim();

                return new Models.Message
                {
                    DisplayText = displayText,
                    TtsText = ttsText,
                    DateTimeString = dateTimeString
                };
            }
            catch
            {
                return new Models.Message
                {
                    DisplayText = "Erro ao processar mensagem.",
                    TtsText = string.Empty
                };
            }
        }

        private async Task ReconnectAsync()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _webSocket = new ClientWebSocket();
                    await ConnectAsync("wss://edge-api.axiafutures.com/ws/?token=...");
                    return;
                }
                catch
                {
                    await Task.Delay(5000);
                }
            }
            OnMessageReceived?.Invoke(new Models.Message
            {
                DisplayText = "Falha ao reconectar.",
                TtsText = string.Empty
            });
        }
    }
}