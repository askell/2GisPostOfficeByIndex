using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace DGisPostOfficeByIndex
{
    class RussianPostInformationService : IPostalInformationService
    {
        private static readonly RussianPostInformationService instance = new RussianPostInformationService();

        public static RussianPostInformationService Instance
        {
            get { return instance; }
        }
 
        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected RussianPostInformationService() 
        {
            try
            {
                HttpWebRequest wrGETURL = (HttpWebRequest)HttpWebRequest.Create(Constants.RUSSIAN_POST_PAGE_URL);
                wrGETURL.Timeout = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
                using (HttpWebResponse resp = (HttpWebResponse)wrGETURL.GetResponse())
                {
                    _cookies = resp.Cookies;
                }
            }
            catch (WebException e)
            {
                //MessageBox.Show(e.Message + e.StackTrace);
            }
        }  

        private CookieCollection _cookies;

        public long LastResponseTime { get; private set; }

        public String ServiceName { get { return "Почта России"; } }
        
        public PostOffice GetPostOffice(string postIndex)
        {
            string requestUri = string.Format(Constants.RUSSIAN_POST_SERVICE_URL, postIndex);

            JavaScriptSerializer jsS = new JavaScriptSerializer();
            PostOffice first = null;

            var timer = Stopwatch.StartNew(); 
            for (int i = 0; i < 10 && first == null; i++)
            {
                try
                {
                    HttpWebRequest wrGETURL = (HttpWebRequest)HttpWebRequest.Create(requestUri);
                    wrGETURL.Timeout = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
                    if (_cookies != null)
                    {
                        wrGETURL.CookieContainer = new CookieContainer();
                        wrGETURL.CookieContainer.Add(_cookies);
                    }

                    wrGETURL.Referer = Constants.RUSSIAN_POST_PAGE_URL;
                    using (WebResponse resp = wrGETURL.GetResponse())
                    {
                        using (Stream objStream = resp.GetResponseStream())
                        {
                            using (StreamReader objReader = new StreamReader(objStream))
                            {
                                IList<PostOffice> opss = jsS.Deserialize<IList<PostOffice>>(objReader.ReadToEnd());
                                first = opss.FirstOrDefault(o => !String.IsNullOrWhiteSpace(o.Name));
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    //MessageBox.Show("WebException:" + e.Message);
                }
                catch (ArgumentException e)
                {
                    //MessageBox.Show("ArgumentException:" + e.Message+e.StackTrace);
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
