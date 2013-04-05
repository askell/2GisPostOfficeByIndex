using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using GrymCore;

namespace DGisPostOfficeByIndex
{
    class RosptInformationService : IPostalInformationService, IGrymConnectionOptionsCallback
    {
        private static readonly Regex OFFICE_NAME = new Regex("<title>(.*)\\.");
        private static WebProxy _proxy;

        private static readonly RosptInformationService instance = new RosptInformationService();

        public static RosptInformationService Instance
        {
            get { return instance; }
        }
 
        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected RosptInformationService() 
        {
            Uri serviceUri = new Uri(Constants.ROS_PT_SERVICE_URL);

            //((IGrymConnectionOptions)PostalInformationServiceManager.Instance.BaseViewThread).RequestProxyOptions(serviceUri.Host, serviceUri.Scheme, this);
        }

        public void OnProxyOptionsReady(IGrymProxyOptions opts)
        {
            if (opts.UseProxy)
            {
                WebProxy proxy = new WebProxy(opts.ProxyAddr, opts.ProxyPort);
                switch (opts.AuthType)
	            {
                    case ProxyAuthType.ProxyAuthTypeCustom:
                        IGrymAuthData data = opts.RequestProxyAuthData(false, null);
                        proxy.Credentials = new NetworkCredential(data.Login, data.Password);
                        break;
                    case ProxyAuthType.ProxyAuthTypeNTLM: 
                        proxy.Credentials = new NetworkCredential().GetCredential(proxy.Address,"NTLM");
                        break;
            	}
                
                _proxy = proxy;
            }
        }

        public long LastResponseTime { get; private set; }

        public String ServiceName { get { return "Независимый рейтинг почтовых отделений России"; } }

        public PostOffice GetPostOffice(string postIndex)
        {
            PostOffice first = null;
            string requestUri = String.Format(Constants.ROS_PT_SERVICE_URL, postIndex);

            var timer = Stopwatch.StartNew(); 
            for (int i = 0; i < 10 && first == null; i++)
            {
                try
                {
                    HttpWebRequest wrGETURL = (HttpWebRequest)HttpWebRequest.Create(requestUri);
                    wrGETURL.Timeout = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
                    if (_proxy!=null)
                    {
                        wrGETURL.Proxy = _proxy;
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)wrGETURL.GetResponse())
                    {
                        using (Stream objStream = resp.GetResponseStream())
                        {
                            using (StreamReader objReader = new StreamReader(objStream, Encoding.GetEncoding(1251)))
                            {
                                char[] bytes = new char[1024];
                                objReader.ReadBlock(bytes,0,bytes.Length);
                                String response = new String(bytes);
                                Match m = OFFICE_NAME.Match(response);
                                if (m.Success)
                                {
                                    first = new PostOffice { Name = m.Groups[1].Value, FoundBy = ServiceName };
                                }
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    //MessageBox.Show("WebException:" + e.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.GetType().ToString() + e.Message + e.StackTrace);
                }
            }
            timer.Stop();
            LastResponseTime = timer.GetElapsedMillisecondsWithCheck(first);
            return first;
        }
    }
}
