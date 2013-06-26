using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DGisPostOfficeByIndex
{
    /// <summary>
    /// Локальный файл на основе открытых данных http://info.russianpost.ru/database/ops.html
    /// </summary>
    class LocalFileInformationService
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

        public string GetPostOffice(string postIndex)
        {
            string result;
            _postOffices.TryGetValue(postIndex, out result);
            return result;
        }

        public int GetCityPostOffices(string city, string indexStart)
        {
            Regex cityOffices = new Regex("^"+city+"\\s+(\\d+)$");
            return _postOffices.Count(o => o.Key.Substring(0,3).Equals(indexStart) && (cityOffices.IsMatch(o.Value) || o.Value.Equals(city)));
        }
    }
}
