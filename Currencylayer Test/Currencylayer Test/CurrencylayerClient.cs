using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Configuration;

namespace Currencylayer_Test
{
   public  class CurrencylayerClient
    {
        public string curr;
        public decimal value ;
    }

    public class JsonDe
    {
        public string success;
        public string terms;
        public string privacy;
        public Int32 timestamp;
        public Dictionary<string,decimal> quotes; 
      
    }
    public enum MessageFormat
    {
        Undefined,
        JSON,
        XML,
        JavaScript,
        JSON_F //formatted Json for Currencylayer



    }

    public class ET
    {
        private string _date ;
        private string _etag;

        public string Date
        {
            get
            {
                return _date ?? "";
            }
        set { _date = value; }

        }
        public string ETag
        {
            get
            {
                return _etag ?? "";
            }
            set { _etag = value; }

        }


    }
    public interface CurrencylayerCommand
    {
        string AccessKey { get; }

        HttpMethod HttpMethod { get; }

        MessageFormat MessageFormat { get; set; }
        Dictionary<string, string> Parameters { get; }

        string Url { get; }
        string callback_function { get;  }
        HttpWebResponse Response { get; set; }
    }

    public class BaseGetCommand : CurrencylayerCommand
    {
        string _accessKey = "";
        public string AccessKey
        {
            get { return _accessKey ?? ""; }
            set { _accessKey = value; }
        }
        public MessageFormat MessageFormat { get; set; }
        private string _callback;
        public string callback_function
        {
            get {return _callback ?? "";}
            set { _callback = value; }
        }
        public HttpMethod HttpMethod
        {
            get { return HttpMethod.Get; }
        }
        private Dictionary<string, string> _parameters;
        public Dictionary<string, string> Parameters
        {
            get
            {
                if (_parameters == null) _parameters = new Dictionary<string, string>();
                return _parameters;
            }
        }

        protected void AddParameter(string key, string value)
        {
            if (_parameters == null) _parameters = new Dictionary<string, string>();
            if (!_parameters.ContainsKey(key))
                _parameters.Add(key, value);
            else
                _parameters[key] = value;
        }
        private string _url;
        public virtual string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        private HttpWebResponse _Response = null;
        public virtual HttpWebResponse Response
        {
            get { return _Response; }
            set { _Response = value; }
        }
    }

    public partial class Form1 : Form
    {
        public HttpWebRequest requestbuilder(CurrencylayerCommand cmd)
        {
            var request = new HttpRequestMessage();
            request.Method = cmd.HttpMethod;
           // cmd.Parameters.Add("access_key",cmd.AccessKey);
            StringBuilder sbParams = new StringBuilder();
            sbParams.Append("access_key="+cmd.AccessKey);
            if (cmd.callback_function != "")
            {
                cmd.Parameters.Add("callback", cmd.callback_function);
            }
            if (cmd.MessageFormat ==MessageFormat.JSON_F)
            {
                cmd.Parameters.Add("format", "1");
            }
            foreach (string key in cmd.Parameters.Keys)
            {
                if (sbParams.Length > 0)
                    sbParams.Append("&");

                sbParams.Append(HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(cmd.Parameters[key]));
            }
            request.RequestUri =
                new Uri(String.Format("{0}{1}?{2}", ConfigurationManager.AppSettings["AccessUrl"], cmd.Url,
                    sbParams.ToString()));
            //   request.Headers.Add("Authorization", String.Format("Bearer {0}", cmd.AccessToken));
          //  request.Headers.IfModifiedSince
           
            HttpWebRequest hwr = (HttpWebRequest) WebRequest.Create(request.RequestUri);

            if (cmd.MessageFormat == MessageFormat.JSON|| cmd.MessageFormat == MessageFormat.JSON_F)
            {
                hwr.Accept = "application/json";
            }
            /*
            else if (cmd.MessageFormat == MessageFormat.XML)

            {
                hwr.Accept = "application/XML";
            }
            */
            else
            {
                FormatException exe = new FormatException("bad message format argument");
                throw exe;
            }
            // hwr.Host = "sandbox.tradier.com";
            // hwr.Headers.Add("Authorization", "Bearer t5ZlzGOhBfxysFFANDo6XCw6D94A");
         //   hwr.Headers.Add("Authorization", String.Format("Bearer {0}", cmd.AccessToken));



            return hwr;
        }
    }
}
