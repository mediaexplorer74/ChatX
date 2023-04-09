﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
//using System.Text.Json;
using ChatGPT.Net.DTO;
using ChatGPT.Net.DTO.ChatGPT;
using ChatGPT.Net.DTO.ChatGPTUnofficial;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatGPT.Net;

public class ChatGptUnofficial
{
    public Guid SessionId { get; set; }
    public ChatGptUnofficialOptions Config { get; set; } = new();
    public List<ChatGptUnofficialConversation> Conversations { get; set; } = new();
    public string SessionToken { get; set; }
    public string AccessToken { get; set; }

    public ChatGptUnofficial(string sessionToken, ChatGptUnofficialOptions? config = null)
    {
        Config = config ?? new ChatGptUnofficialOptions();
        SessionId = Guid.NewGuid();
        SessionToken = sessionToken;
    }

    public async Task RefreshAccessToken()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            UseCookies = false,
        });
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Config.BaseUrl}/api/auth/session"),
            Headers =
            {
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0"},
                {"Accept", "application/json"},
                {"Cookie", $"__Secure-next-auth.session-token={SessionToken}" }
            }
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        //var content = await response.Content.ReadFromJsonAsync<ChatGptUnofficialProfile>();
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<ChatGptUnofficialProfile>(contentString);


        const string name = "__Secure-next-auth.session-token=";
        var cookies = response.Headers.GetValues("Set-Cookie");
        var sToken = cookies.FirstOrDefault(x => x.StartsWith(name));

        SessionToken = sToken == null ? SessionToken : sToken.Replace(name, "");

        if (content is not null)
        {
            if(content.Error is null) AccessToken = content.AccessToken;
        }
    }

    // IAsyncEnumerable
    private /*async*/ IEnumerable<string> StreamCompletion(Stream stream)
    {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var lineTask = /*await*/ reader.ReadLineAsync();
            var line = lineTask.Result;
            if (line != null)
            {
                yield return line;
            }
        }
    }

    public List<ChatGptUnofficialConversation> GetConversations()
    {
        return Conversations;
    }

    public void SetConversations(List<ChatGptUnofficialConversation> conversations)
    {
        Conversations = conversations;
    }

    public ChatGptUnofficialConversation GetConversation(string? conversationId)
    {
        if (conversationId is null)
        {
            return new ChatGptUnofficialConversation();
        }

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null) return conversation;
        conversation = new ChatGptUnofficialConversation
        {
            Id = conversationId
        };
        Conversations.Add(conversation);

        return conversation;
    }
    
    public void SetConversation(string conversationId, ChatGptUnofficialConversation conversation)
    {
        var conv = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conv != null)
        {
            conv = conversation;
        }
        else
        {
            Conversations.Add(conversation);
        }
    }
    
    public void RemoveConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null)
        {
            Conversations.Remove(conversation);
        }
    }

    public void ResetConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation == null) return;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
        conversation.ConversationId = null;
    }

    public void ClearConversations()
    {
        Conversations.Clear();
    }

    public async Task<string> Ask(string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        var reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model);

        if(reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if(reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    public async Task<string> AskStream(Action<string> callback, string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        var reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model,
            response =>
            {
                var content = response.Message.Content.Parts.FirstOrDefault();
                if (content is not null) callback(content);
            });

        if(reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if(reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    private bool ValidateToken(string token) 
    {
        if (string.IsNullOrWhiteSpace(token)) 
        {
            return false;
        }
    
        var tokenParts = token.Split('.');
        if (tokenParts.Length != 3) 
        {
            return false;
        }
    
        string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1]));

        //var parsed = JsonDocument.Parse(decodedPayload).RootElement;
        var jsonObject = JObject.Parse(decodedPayload);
        var parsed = jsonObject.SelectToken("root");

        //TODO
        return true;//DateTimeOffset.Now.ToUnixTimeMilliseconds() <= parsed.GetProperty("exp").GetInt64() * 1000;
    }

    public async Task<ChatGptUnofficialMessageResponse> SendMessage(string message, string messageId, string? parentMessageId = null, string? conversationId = null, string? model = null, Action<ChatGptUnofficialMessageResponse>? callback = null)
    {
        if(!ValidateToken(AccessToken)) await RefreshAccessToken();
        var requestData = new ChatGptUnofficialMessageRequest
        {
            Messages = new List<MessageElement>
            {
                new()
                {
                    Content = new Content
                    {
                        Parts = new List<string>
                        {
                            message
                        }
                    }
                }
            }
        };
 
        if (model is not null)
        {
            requestData.Model = model;
        }
 
        if (conversationId is not null)
        {
            requestData.ConversationId = conversationId;
        }
        
        if (parentMessageId is not null)
        {
            requestData.ParentMessageId = parentMessageId;
        }

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Config.BaseUrl}/backend-api/conversation"),
            Headers =
            {
                {"Authorization", $"Bearer {AccessToken}" }
            },
            Content = new StringContent(JsonConvert.SerializeObject(requestData))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        
        ChatGptUnofficialMessageResponse? reply = null;

        //await 
        foreach (var data in StreamCompletion(stream))
        {
            reply = JsonConvert.DeserializeObject<ChatGptUnofficialMessageResponse>(data.Replace("data: ", ""));
            if(reply is not null)
            {
                callback?.Invoke(reply);
            }
        }

        return reply ?? new ChatGptUnofficialMessageResponse();
    }
}