// ChatGPT 'clone' (PrivateAI)

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace XamarinSignalRExample
{
    public class ChatGpt
    {
        private readonly string ApiKey = ""; // paste your key here
        private string v;
        private ChatGptOptions chatGptOptions;

        public ChatGpt(string v, ChatGptOptions chatGptOptions)
        {
            this.v = v;
            this.chatGptOptions = chatGptOptions;
        }

        public async Task<string> AskStream(Action<string> message1, string prompt, string v)
        {
            string response = "";
            using (var client = new HttpClient())
            {
                JObject json = JObject.Parse(
                @"{
                text: [
                      'Hello everyone, kidfive- my mom is on Tucatinib, similar to you she had brain mets in 2015, september craniotomy and srs and gamma knife October and November of that year. Yes her oncologist is very excited about Tucatinib, this is the reason why I’m actually here today, we had her follow up brain mri end of June, everything was good, recently she was hospitalized for high iron, during her hospitalization they did another brain mri (this is a different hospital than where we do the follow ups) and they said where that where they did the gamma there is an 14mm lesion now and a lesion where they did the craniotomy there is a lesion there too, so now im confused i don’t know what to do , I’m hoping there’s no lesion and this Tucatinib will take care of it. Has anyone experienced two different brain mri results'
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
            }");

            var postData = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            postData.Headers.Add("X-API-KEY", ApiKey);
            var request = await client.PostAsync("https://api.private-ai.com/deid/v3/process/text", postData);
                
            response = await request.Content.ReadAsStringAsync();

            Debug.WriteLine(response);
        }

        return response;
    }
  }
}