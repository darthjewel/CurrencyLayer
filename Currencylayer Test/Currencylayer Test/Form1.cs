using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Web;

namespace Currencylayer_Test
{
    public partial class Form1 : Form
    {
        public bool replay = true;
        public Form1()
        {
            InitializeComponent();
            replay = false;
            dataGridView1.Columns.Add("Curr", "Curr");
            dataGridView1.Columns.Add("Val", "Val");
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns["Curr"].Width = 80;
            dataGridView1.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.BackColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public void SaveMessage(string message)
        {
            System.IO.StreamWriter file =
          new System.IO.StreamWriter("responses.txt", true);
            file.WriteLine(message);

            file.Close();
        }

        public ET getETag()
        {
            ET alien = new ET();
            try
            {
                System.IO.StreamReader myFile =
               new System.IO.StreamReader("etag.txt");
                string responseString = myFile.ReadToEnd();
                string[] blurry = responseString.Split('~');
                if (blurry.Length == 2)
                {
                    alien.ETag = blurry[1].TrimEnd('\r','\n');
                    alien.Date = blurry[0];
                }
                myFile.Close();
            }
            catch (Exception exe)
            {

                display.Text = Environment.NewLine + exe.Message + Environment.NewLine;
            }
           
            return alien;
        }

        public void ProcessMessage(string message)
        {
            
        }
        public void ProcessHeaders(WebHeaderCollection head)
        {
            display.Text = head.AllKeys.ToString();
            display.Text += head.Keys.Count;
            for (int i = 0; i < head.Keys.Count; ++i)

            { display.Text+=Environment.NewLine+head.Keys[i]+" "+head[i];}

            if (head["ETag"] != null)
            {
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter("etag.txt");
               
                file.WriteLine(head["Date"]+"~"+head["ETag"]);
                file.Close();
            }
            else
            {
                display.Text += Environment.NewLine+ "-----no ETag header----- " +Environment.NewLine;
            }
        }
        public void AnalyseResponse(WebResponse we)
        {
            HttpWebResponse weresponse = (HttpWebResponse) we;
            display.Text += "Status Code : " + weresponse.StatusCode + Environment.NewLine;
          for (int i = 0; i < weresponse.Cookies.Count; ++i)
            { display.Text += weresponse.Cookies[i] + "name:" + weresponse.Cookies[i].Name + "expires :"+weresponse.Cookies[i].Expires+ Environment.NewLine; }
            display.Text += "Content Type : " + weresponse.ContentType + Environment.NewLine;
            display.Text += "Character Set : " + weresponse.CharacterSet + Environment.NewLine;
            display.Text += "Content Encoding : " + weresponse.ContentEncoding+ Environment.NewLine;
            display.Text += "Content Length : " + weresponse.ContentLength + Environment.NewLine;
            display.Text += "Last Modified : " + weresponse.LastModified + Environment.NewLine;
            display.Text += "Method : " + weresponse.Method + Environment.NewLine;
            display.Text += "IsMutuallyAuthenticated : " + weresponse.IsMutuallyAuthenticated + Environment.NewLine;
            display.Text += "Protocol Version : " + weresponse.ProtocolVersion + Environment.NewLine;
            display.Text += "ResponseUri : " + weresponse.ResponseUri + Environment.NewLine;
            display.Text += "Status Description : " + weresponse.StatusDescription + Environment.NewLine;
            display.Text += "Server : " + weresponse.Server + Environment.NewLine;
            display.Text += "--------------------" + Environment.NewLine;
            AnalyseHeaders(weresponse.Headers);
            display.Text += "--------------------" + Environment.NewLine;
            display.Text+="   --- Actual Response ---- "+ Environment.NewLine;
            using (Stream stream = we.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                display.Text += responseString;
              
            }

        }
        public void AnalyseHeaders(WebHeaderCollection head)
        {
          //  display.Text = head.AllKeys.ToString();
            display.Text += "Found "+head.Keys.Count+ " Headers"+Environment.NewLine;
            for (int i = 0; i < head.Keys.Count; ++i)

            { display.Text += head.Keys[i] + ":" + head[i]+Environment.NewLine; }

        }
        private async void button1_Click(object sender, EventArgs e)
        {
            if (replay == true)
            {
                System.IO.StreamReader myFile =
                   new System.IO.StreamReader("usd_current.txt");
                string responseString = myFile.ReadToEnd();
                myFile.Close();
                display.Text = responseString;
               ProcessMessage(responseString);
            }
            else
            {

            
            BaseGetCommand cmd = new BaseGetCommand();
            cmd.AccessKey = ConfigurationManager.AppSettings["AccessKey"];
            cmd.MessageFormat = MessageFormat.JSON_F;
            cmd.Url = "/live";

                // JSONP Callbacks
                //    cmd.callback_function = "nelly";

                //Source Currency Switching - not enabled in Free
                /*
                 cmd.Parameters.Add("source","EUR");
                 */
                HttpWebRequest request = requestbuilder(cmd);
                ET alien = getETag();
                if (alien.ETag != "")
                {
                   
                    DateTime dt =DateTime.Parse(alien.Date);
                    request.IfModifiedSince = dt;
                    request.Headers[HttpRequestHeader.IfNoneMatch] = alien.ETag;

                }
                try
                {

                    var response = (HttpWebResponse)
                        await Task.Factory
                            .FromAsync<WebResponse>(request.BeginGetResponse,
                                request.EndGetResponse,
                                null);


                    //   AnalyseResponse(response);
                    
                    ProcessHeaders(response.Headers);
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();
               //     display.Text += responseString;
                    SaveMessage(responseString);
                    ProcessMessage(responseString);
                }
                
                }
                catch (WebException we)
                {

                    display.Text = we.Message + Environment.NewLine;
                    HttpWebResponse weresponse = (HttpWebResponse)we.Response;
                    AnalyseResponse(weresponse);
                }
                catch (Exception ole)
                {
                    display.Text += ole.Message;
                    
                }
            }
    }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (replay == true)
            {
                /*
                System.IO.StreamReader myFile =
                   new System.IO.StreamReader("usd_current.txt");
                string responseString = myFile.ReadToEnd();
                myFile.Close();
                display.Text = responseString;
                ProcessMessage(responseString);
                */
            }
            else
            {


                BaseGetCommand cmd = new BaseGetCommand();
                cmd.AccessKey = ConfigurationManager.AppSettings["AccessKey"];
                cmd.MessageFormat = MessageFormat.JSON;
                cmd.Url = "/historical";
                cmd.Parameters.Add("date", textBox1.Text);
                // JSONP Callbacks
                //    cmd.callback_function = "nelly";

                //Source Currency Switching - not enabled in Free
                /*
                 cmd.Parameters.Add("source","EUR");
                 */
                HttpWebRequest request = requestbuilder(cmd);
                /*
                ET alien = getETag();
                if (alien.ETag != "")
                {

                    DateTime dt = DateTime.Parse(alien.Date);
                    request.IfModifiedSince = dt;
                    request.Headers[HttpRequestHeader.IfNoneMatch] = alien.ETag;

                }
                */
                try
                {

                    var response = (HttpWebResponse)
                        await Task.Factory
                            .FromAsync<WebResponse>(request.BeginGetResponse,
                                request.EndGetResponse,
                                null);
                    //  ProcessHeaders(response.Headers);

                    AnalyseResponse(response);
                    /*
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();
               //     display.Text += responseString;
                    SaveMessage(responseString);
                    ProcessMessage(responseString);
                }
                */
                }
                catch (WebException we)
                {

                    display.Text = we.Message + Environment.NewLine;
                    HttpWebResponse weresponse = (HttpWebResponse)we.Response;
                    AnalyseResponse(weresponse);
                }
                catch (Exception ole)
                {
                    display.Text += ole.Message;

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*
         ET alien= getETag();
          display.Text += alien.ETag;
          */

            //Currency-Change Queries
            /*
            http://apilayer.net/api/change
    ? access_key = YOUR_ACCESS_KEY
    & start_date = 2005 - 01 - 01
    & end_date = 2010 - 01 - 01
    & currencies = AUD,EUR,MXN
    */
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Currency Conversion Endpoint
            // Currency Conversion using Historical Rates
            /*
            http://apilayer.net/api/convert
    ? access_key = YOUR_ACCESS_KEY
    & from = USD
    & to = GBP
    & amount = 10
    */
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Time-Frame Queries
            /*
             * http://apilayer.net/api/timeframe
    ? access_key = YOUR_ACCESS_KEY
    & start_date = 2010-03-01
    & end_date = 2010-04-01
    & currencies = USD,GBP,EUR
    */

        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            this.textBox1.Text = e.Start.ToString("yyyy-MM-dd");
              //  .ToShortDateString();
            /*
            this.display.Text = "Date Changed: Start =  " +
            e.Start.ToShortDateString() + " : End = " + e.End.ToShortDateString();
            */
        }

        private void button12_Click(object sender, EventArgs e)
        {
            System.IO.StreamReader myFile =
                   new System.IO.StreamReader("usd_current.txt");
            string responseString = myFile.ReadToEnd();
            myFile.Close();
           // var definition = new  {Name = ""};
            JsonDe dim =  JsonConvert.DeserializeObject<JsonDe>(responseString);
            try
            {
                if (dim.success == "true")
                {
                   // display.Text = dim.quotes.Keys.Count().ToString()+Environment.NewLine;
                    foreach (KeyValuePair<string, decimal> quote in dim.quotes)
                    {
                      //  display.Text += quote.Key + " : " + quote.Value.ToString()+Environment.NewLine;
                        dataGridView1.Rows.Add(quote.Key, quote.Value);
                    }
                    //  display.Text += "timestamp : "+dim.timestamp.ToString();
                    display.Text += responseString;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
           // display.Text = definition.Name;
        }
    }
}
