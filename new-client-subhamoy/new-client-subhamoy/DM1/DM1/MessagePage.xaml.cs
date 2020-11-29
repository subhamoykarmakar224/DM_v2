using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DM1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessagePage : ContentPage
    {
       
        public MessagePage()
        {
            InitializeComponent();
        }


        private void OnComposeButtonClicked(object sender, EventArgs e)
        {
            idEntry.IsEnabled = true;
            idEntry.Placeholder = "Enter recipient's ID here";
            SendButton.IsEnabled = true;
            

        }
        async void OnSendButtonClicked(object sender, EventArgs e)
        {

            using (var client = new HttpClient())
            {
                try
                {
                    // send a GET request  
                    var uri = Globals.URI_NAME + "/semanticobservation/add?";

                    var newPost = new MessageClass
                    {
                        Timestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                        Message_content = MessageText.Text,
                        Semantic_observation_type_id = "2",
                        Semantic_entity_id = idEntry.Text,
                        Virtual_sensor_id = "2" 
                    };
                    //  latlabel.Text = "made it to json newpost in Gyro";
                    var json2 = "[{\"Timestamp\":" + "\"" + newPost.Timestamp + "\"" + ",";
                    json2 = json2 + "\"payload\":{\"Message\":" + "\"" + newPost.Message_content + "\"" + "},";
                    json2 = json2 + "\"semantic_observation_type_id\":" + "\"" + newPost.Semantic_observation_type_id + "\"" + ",";
                    json2 = json2 + "\"semantic_entity_id\":" + "\"" + newPost.Semantic_entity_id + "\"" + ",";
                    json2 = json2 + "\"virtual_sensor_id\":" + "\"" + newPost.Virtual_sensor_id + "\"";
                    json2 = json2 + "}]";
                    //  send a POST request  
                    MessageText.Text = "Sending Message";
                    var content = new StringContent(json2, Encoding.Default, "application/json");
                    var result = await client.PostAsync(uri, content);

                    // on error throw a exception  
                    result.EnsureSuccessStatusCode();

                    // handling the answer  
                    var resultString = await result.Content.ReadAsStringAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        MessageText.Text = "\nGood POST of Message to Tippers";
                    }
                    else
                    {
                        MessageText.Text = "\nBad POST of Message to Tippers - try again";
                    }
                    // Throw an error if a problem.
                }
                catch
                {
                    MessageText.Text = "System.IO error (onSend) connecting to Tippers";
                }
            }


        }
        void OnClearButtonClicked(object sender, EventArgs e)
        {
            //// var note = (Message)BindingContext;

            // if (string.IsNullOrWhiteSpace(note.Filename))

            // Save
            //   string filename = Path.Combine(Environment.SpecialFolder.LocalApplicationData, $"{Path.GetRandomFileName()}.notes.txt");
            //   File.WriteAllText(filename, note.Text);
            //  }
            ////  else
            {
                // Update
                //    File.WriteAllText(note.Filename, note.Text);
            }

            MessageText.Text = "";

        }
        private void Entry_Completed(object sender, EventArgs e)
        {
            idEntry.IsEnabled = false;

        }
        async void OnRetrieveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                HttpClient client;
                client = new HttpClient();
                var uri =  Globals.URI_NAME + "/semanticobservation/get?requestor_id=cbdavison@bsu.edu&type=2&service_id=1&subject_id=cbdavison@bsu.edu";
                string response = await client.GetStringAsync(uri);

                //This gets all the JSON stuff 

                var temp1 = JsonConvert.DeserializeObject<List<Msgjson>>(response);
                var tester5 = temp1[0].Payload1.Message.ToString();
                MessageText.Text = tester5;                 
            }
            catch
            {
                MessageText.Text = "System.IO exception error (onRetrieve) connecting to Tippers";
            }
        }
    }
}