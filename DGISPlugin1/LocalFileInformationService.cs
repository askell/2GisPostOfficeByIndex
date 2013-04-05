using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DGisPostOfficeByIndex
{
    class LocalFileInformationService : IPostalInformationService
    {
        private static readonly LocalFileInformationService instance = new LocalFileInformationService();
        private readonly IDictionary<string, string> _postOffices;

        public static LocalFileInformationService Instance
        {
            get { return instance; }
        }
 
        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected LocalFileInformationService() 
        {
            string[] offices = Regex.Split(PostOffices.OPSs, "\r\n|\r|\n");
            _postOffices = new Dictionary<string, string>(offices.Length);
            foreach (var office in offices)
            {
                _postOffices.Add(office.Substring(0, 6), office.Substring(7));
            }
        }  

        public long LastResponseTime { get; private set; }

        public String ServiceName { get { return "Локальный файл"; } }
        
        public PostOffice GetPostOffice(string postIndex)
        {
            var timer = Stopwatch.StartNew(); 
            string foundName;
            PostOffice reuslt = null;
            if (_postOffices.TryGetValue(postIndex, out foundName))
            {
                reuslt= new PostOffice{Name=foundName,FoundBy=ServiceName };
            }
            timer.Stop();
            LastResponseTime = timer.GetElapsedMillisecondsWithCheck(reuslt);
            return reuslt;
        }
    }
}
