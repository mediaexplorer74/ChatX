// ChatGPT (PrivateAI)

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace XamarinSignalRExample
{
    public class ChatGpt
    {
        public string ApiKey = "c72d5e9641764f159d57f5ad3313b7b4"; // paste your key here
        string processtext_url = "https://api.private-ai.com/deid/v3/process/text";


        public ChatGptOptions chatGptOptions;

        public ChatGpt(/*string apikey, */ChatGptOptions chatGptOptions)
        {
            /*this.ApiKey = apikey;*/
            this.chatGptOptions = chatGptOptions;
        }

        public async Task<string> AskStream(/*Action<string> message1,*/ string prompt)
        {
            string result = "";

                  
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    var json = 
                    @"{
                    text: [
                          '*PROMPT*'
                          ],
                    link_batch: false,
                    entity_detection: 
                    {
                      entity_types: [
                        {
                          type: 'ENABLE',
                          value: ['ACCOUNT_NUMBER','AGE','DATE','DATE_INTERVAL','DOB','DRIVER_LICENSE','DURATION','EMAIL_ADDRESS','EVENT','FILENAME','GENDER_SEXUALITY','HEALTHCARE_NUMBER','IP_ADDRESS','LANGUAGE','LOCATION','LOCATION_ADDRESS','LOCATION_ADDRESS_STREET','LOCATION_CITY','LOCATION_COORDINATE','LOCATION_COUNTRY','LOCATION_STATE','LOCATION_ZIP','MARITAL_STATUS','MONEY','NAME','NAME_FAMILY','NAME_GIVEN','NAME_MEDICAL_PROFESSIONAL','NUMERICAL_PII','ORGANIZATION','ORGANIZATION_MEDICAL_FACILITY','OCCUPATION','ORIGIN','PASSPORT_NUMBER','PASSWORD','PHONE_NUMBER','PHYSICAL_ATTRIBUTE','POLITICAL_AFFILIATION','RELIGION','SSN','TIME','URL','USERNAME','VEHICLE_ID','ZODIAC_SIGN','BLOOD_TYPE','CONDITION','DOSE','DRUG','INJURY','MEDICAL_PROCESS','STATISTICS','BANK_ACCOUNT','CREDIT_CARD','CREDIT_CARD_EXPIRATION','CVV','ROUTING_NUMBER']
                        }
                      ],
                      return_entity: true
                    },
                    processed_text: {
                      type: 'MARKER',
                      pattern: '[UNIQUE_NUMBERED_ENTITY_TYPE]'
                    }
                   }";

                    string o_json = json.Replace("*PROMPT*", prompt);

                    JObject jo = JObject.Parse(o_json);

                    StringContent postData = new StringContent(jo.ToString(), Encoding.UTF8, "application/json");

                    postData.Headers.Add("x-api-key", ApiKey);

                    HttpResponseMessage response = await client.PostAsync(processtext_url, postData);
                    
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine("[i] " + responseBody);

                    result = responseBody;
                }
              
            }
            catch (HttpRequestException ex)
            {
               Debug.WriteLine("[ex] Exception Message :{0} ", ex.Message);
            }

            return result;
        }
    }
}