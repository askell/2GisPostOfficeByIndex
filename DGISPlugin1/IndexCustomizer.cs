using System;
using System.Collections.Generic;
using System.Text;
using GrymCore;

namespace DGisPostOfficeByIndex
{
    public class IndexCustomizer : IMapInfoActionsCustomizer 
    {
        public string MakeActions(IFeature f) 
        {
            return "";
        }

        public bool OnAction(IFeature f, string s)
        {
            return true;
        }

        public IRasterSet Images { get; set; }
    }
}
