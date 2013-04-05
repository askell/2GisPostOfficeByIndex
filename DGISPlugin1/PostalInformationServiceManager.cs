using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using GrymCore;

namespace DGisPostOfficeByIndex
{
    class PostalInformationServiceManager
    {
        private ICollection<IPostalInformationService> _services = new HashSet<IPostalInformationService>();

        private static readonly PostalInformationServiceManager instance = new PostalInformationServiceManager();

        public static PostalInformationServiceManager Instance
        {
            get { return instance; }
        }

        public IBaseViewThread BaseViewThread { get; set; }
 
        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected PostalInformationServiceManager() { }

        private void CheckAndAddToList(IPostalInformationService service)
        {
            PostOffice moscow = service.GetPostOffice(Constants.MOSCOW_INDEX);
            if (moscow != null && Constants.MOSCOW_NAME.Equals(moscow.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                _services.Add(service);
            }
        }

        public PostOffice GetPostOffice(string postIndex)
        {
            IPostalInformationService bestService = _services.OrderBy(s => s.LastResponseTime).FirstOrDefault();
            if (bestService!=null) 
            {
                return bestService.GetPostOffice(postIndex);
            }
            return null;
        }

        public void RefreshServices()
        {
            do
            {
                CheckAndAddToList(LocalFileInformationService.Instance);
                //CheckAndAddToList(RosptInformationService.Instance);
                //CheckAndAddToList(GdePosylkaInformationService.Instance);
                //CheckAndAddToList(RussianPostInformationService.Instance);
                if (_services.Count==0) 
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            } while (_services.Count==0);
            //MessageBox.Show(string.Join<string>(Environment.NewLine,_services.Select(s => s.ServiceName+"="+s.LastResponseTime.ToString())));
        }
    }
    public delegate void AsyncRefreshServices();
}
