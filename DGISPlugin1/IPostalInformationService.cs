using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DGisPostOfficeByIndex
{
    public interface IPostalInformationService
    {
        PostOffice GetPostOffice(string postIndex);
        long LastResponseTime { get; }
        String ServiceName { get; }
        //public int CheckStatus();
    }
}
