using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace DGisPostOfficeByIndex
{
    static class Constants
    {
        public const String RUSSIAN_POST_PAGE_URL = "http://www.russianpost.ru/rp/servise/ru/home/postuslug/searchops1";
        public const String RUSSIAN_POST_SERVICE_URL = "http://www.russianpost.ru/PostOfficeFindInterfaceTest/AddressComparison.asmx/FindOPSByIndexStr?IndexStr={0}";
        public const String ROS_PT_SERVICE_URL = "http://www.rospt.ru/pochta_{0}.html";
        public const String GDE_POS_SERVICE_URL = "http://gdeposylka.ru/info/pochtamt/{0}";
        public const String MOSCOW_INDEX = "101000";
        public const String MOSCOW_NAME = "МОСКВА";
        
        private static readonly String[] REGION_TYPES = { "Область", "Край", "Республика", "Автономный округ" };

        public static string TrimRegion(this string addr)
        {
            int pos = -1;
            for (int i = 0; i < REGION_TYPES.Length && pos == -1; i++)
            {
                pos = addr.IndexOf(REGION_TYPES[i], StringComparison.CurrentCultureIgnoreCase);
                if (pos >= 0)
                {
                    pos += REGION_TYPES[i].Length;
                }
            }
            if (pos >= 0)
            {
                return addr.Substring(pos);
            }
            return addr;
        }

        public static long GetElapsedMillisecondsWithCheck(this Stopwatch sw, PostOffice foundOffice)
        {
            if (foundOffice != null && !String.IsNullOrWhiteSpace(foundOffice.Name))
            {
                return sw.ElapsedMilliseconds;
            }
            return long.MaxValue;
        }
    }
}