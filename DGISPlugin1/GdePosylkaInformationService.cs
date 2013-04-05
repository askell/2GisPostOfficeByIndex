using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;

namespace DGisPostOfficeByIndex
{
    class GdePosylkaInformationService : IPostalInformationService
    {
        private static readonly Regex OFFICE_NAME = new Regex("ОТДЕЛЕНИЕ ПОЧТОВОЙ СВЯЗИ <b>(.*)</b>,");

        private static readonly GdePosylkaInformationService instance = new GdePosylkaInformationService();

        public static GdePosylkaInformationService Instance
        {
            get { return instance; }
        }
 
        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected GdePosylkaInformationService() { }

        public long LastResponseTime { get; private set; }

        public String ServiceName { get { return "ГдеПосылка.ру"; } }

        public PostOffice GetPostOffice(string postIndex)
        {
            PostOffice first = null;
            string requestUri = String.Format(Constants.GDE_POS_SERVICE_URL, postIndex);

            var timer = Stopwatch.StartNew(); 
            for (int i = 0; i < 10 && first == null; i++)
            {
                try
                {
                    HttpWebRequest wrGETURL = (HttpWebRequest)HttpWebRequest.Create(requestUri);
                    wrGETURL.Timeout = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
                    using (HttpWebResponse resp = (HttpWebResponse)wrGETURL.GetResponse())
                    {
                        using (Stream objStream = resp.GetResponseStream())
                        {
                            using (StreamReader objReader = new StreamReader(objStream))
                            {
                                char[] bytes = new char[5024];
                                objReader.ReadBlock(bytes, 0, bytes.Length);
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
            if (first != null && !String.IsNullOrWhiteSpace(first.Name))
            {
                LastResponseTime = timer.ElapsedMilliseconds;
            }
            else
            {
                LastResponseTime = long.MaxValue;
            }
            return first;
        }
    }
}
