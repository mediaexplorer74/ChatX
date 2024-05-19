using System;

using Xamarin.Forms;
using System.Collections.ObjectModel;
//using ChatGPT.Net;
using System.Diagnostics;
//using ChatGPT.Net.DTO.ChatGPT;

namespace XamarinSignalRExample
{
	public class App : Application
	{
		public SignalRClient SignalRClient = 
			new SignalRClient("http://localhost:63172"); //:9995

		public App()
		{
            /*
			//show an error if the connection doesn't succeed for some reason
			SignalRClient.Start ().ContinueWith(task => {
				if (task.IsFaulted) 
					MainPage.DisplayAlert("Error",
						"An error occurred when trying to connect to SignalR: " 
						+ task.Exception.InnerExceptions[0].Message, "OK");
			});


			//try to reconnect every 10 seconds, just in case
			Device.StartTimer (TimeSpan.FromSeconds (10), () => {
				if (!SignalRClient.IsConnectedOrConnecting)
					SignalRClient.Start();

				return true;
			});
			*/

            //******************

            ChatGpt chatbot = new ChatGpt 
            (//"pk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            new ChatGptOptions
            {
               BaseUrl = "https://api.pawan.krd"
            });

            //******************

            UsernameStackLayout usernameStack = new UsernameStackLayout();

            SendMessageStackLayout sendMessage = new SendMessageStackLayout();

            ChatListViewStackLayout listView = new ChatListViewStackLayout();

            Label connectionLabel = new Label 
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				BindingContext = SignalRClient
			};
			
			connectionLabel.SetBinding (Label.TextProperty, "ConnectionState");

			// The root page of your application
			MainPage = new ContentPage
			{
				Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5),
				Content = new StackLayout
				{
					Children = 
					{
						new Label {
							FontSize = 12.0,
							HorizontalOptions = LayoutOptions.CenterAndExpand,
							Text = "ChatGPT + Xamarin!"
						},
						usernameStack,
						sendMessage,
						listView,
						connectionLabel
					}
				}
			};

			sendMessage.MessageSent += async (message) => 
			{
				// 1. send
				listView.AddText("Me: " + message);

                //SignalRClient.SendMessage(usernameStack.UsernameTextbox.Text, message);


                // *************************



                string? prompt = string.Empty;

                listView.AddText("Welcome to ChatGPT.");
                Debug.Write("Welcome to ChatGPT.");

				//while (true)
				//{
				//Console.Write("You: ");
				prompt = message;//Console.ReadLine();

                if (prompt is null) return;
                
				if (string.IsNullOrWhiteSpace(prompt)) return;
                    
				    
                    
				Debug.Write("ChatGPT: prompt=" + prompt);

				//System.Action<string> message1 = default;

                //await 
                string str = await chatbot.AskStream(/*message1, */prompt);
                    
			    Debug.WriteLine(" Your message = "  + prompt);
                //}

                // *************************


                //2. get
                string ainame = "AI"; 
                listView.AddText(ainame + ": " + str.ToString());
                Debug.WriteLine("AI message1 = " + str);
            };

			SignalRClient.OnMessageReceived += (username, message) =>
			{
				listView.AddText(username + ": " + message);
			};
		}
	}
}

