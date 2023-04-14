using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
//using com.generiton.io;
using Newtonsoft.Json;
using TsecTowerBuzzer;

namespace SingleTowerClient
{
    public partial class Form1 : Form
    { 
        private static IMqttClient client;
        private static MqttClientOptionsBuilder optionsBuilder;
        public TowerBuzzer oTowerBuzzer = new TowerBuzzer();
        bool towerState = false;

        public Form1()
        {
            InitializeComponent();
        }

        private delegate void LogAppendText(string value, RichTextBox ctl);
        private void LogAppend(string value, RichTextBox ctl)
        {
            if (this.InvokeRequired)
            {
                LogAppendText uu = new LogAppendText(LogAppend);
                this.Invoke(uu, value, ctl);
            }
            else
            {
                ctl.AppendText(value + "\n");
            }
        }
        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            oTowerBuzzer.TowerOffBuzzerOff();
            oTowerBuzzer.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var settings = ConfigurationManager.AppSettings;

                oTowerBuzzer.Open(settings["COM_PORT"]);
                Closing += new CancelEventHandler(Form1_Closing);

                var factory = new MqttFactory();
                
                client = factory.CreateMqttClient();

                optionsBuilder = new MqttClientOptionsBuilder();

                optionsBuilder.WithTcpServer(settings["MQTTServer"], 1883);
                optionsBuilder.WithClientId(settings["MQTTID"]);
                MD5 md5 = MD5.Create();
                string resultMd5 = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes("TSEC" + settings["MQTTID"])));
                optionsBuilder.WithCredentials("", resultMd5);
                optionsBuilder.WithCleanSession(true);

                client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(x => MqttClinet_ApplicationMessageReceived(client, x));
                client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(x => MqttClinet_Connected(client, x));
                client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(x => MqttClinet_Disconnected(client, x));

                try
                {
                    client.ConnectAsync(optionsBuilder.Build());
                }
                catch (Exception exception)
                {
                    LogAppend("### 連接失敗 ###" + Environment.NewLine + exception, richTextBox1);
                }

                //LogAppend("### WAITING FOR APPLICATION MESSAGES ###", richTextBox1);
            }

            catch (Exception exception)
            {
                LogAppend(exception.ToString(), richTextBox1);
            }
        }
        private void MqttClinet_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            LogAppend("### 接收到訊息 ###", richTextBox1);

            string MqttMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload); 

            AlarmResponse response = JsonConvert.DeserializeObject<AlarmResponse>(MqttMessage);

            if(response.OnOff)
                if (!towerState)
                {
                    towerState = true;
                    oTowerBuzzer.TowerOnBuzzerOn();
                }

            LogAppend($"訊息 = " + response.Message, richTextBox1);
            LogAppend("", richTextBox1);
        }

        private async void MqttClinet_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            LogAppend("### 連接伺服器 ###", richTextBox1);

            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic(ConfigurationManager.AppSettings["Subscribe"]).Build());

            LogAppend("### 訂閱成功 ###", richTextBox1);
        }

        private async void MqttClinet_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            LogAppend("### 與伺服器斷開連接 ###", richTextBox1);
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await client.ConnectAsync(optionsBuilder.Build());
            }
            catch
            {
                LogAppend("### 重新連接失敗 ###", richTextBox1);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            RichTextBox rtb = sender as RichTextBox;
            rtb.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            oTowerBuzzer.TowerOnBuzzerOn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            towerState = false;
            oTowerBuzzer.TowerOffBuzzerOff();
        }
    }
}
