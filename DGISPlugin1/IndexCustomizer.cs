using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GrymCore;

namespace DGisPostOfficeByIndex
{
    public class IndexCustomizer : IMapInfoActionsCustomizer 
    {
        public IndexCustomizer(IBaseViewThread pBaseView) {
            _pBaseView = pBaseView;
            _currentCity=_pBaseView.BaseReference.Name;
        }

        private static Regex CITY_POST_OFFICE_NAME = new Regex("^(.*)\\s(\\d+)$");
        private IBaseViewThread _pBaseView;
        private string _currentCity;

        public string MakeActions(IFeature f) 
        {
            IDataRow dr = f as IDataRow;
            if ((dr!=null) && ("grym_map_building".Equals(dr.Type))) {
                return "<action_list><action_item placement_code=\"1000\"><text action_code=\"find_post_office\">Найти почтовое отделение</text></action_item></action_list>";
            }
            return "";
        }

        public bool OnAction(IFeature f, string s)
        {
            if ("find_post_office".Equals(s)) {
                IDataRow dr = f as IDataRow;
                if ((dr!=null) && ("grym_map_building".Equals(dr.Type))) {
                    string postIndex= dr.Value["post_index"].ToString();
                    PostOffice first = LocalFileInformationService.Instance.GetPostOffice(postIndex);
                    if (first != null)
                    {
                        Match m = CITY_POST_OFFICE_NAME.Match(first.Name);
                        String city;
                        String number;
                        if (m.Success)
                        {
                            city = m.Groups[1].Value;
                            number = m.Groups[2].Value;
                        }
                        else
                        {
                            city = first.Name;
                            number = null;
                        }
                        try
                        {
                            ICriteriaSet criteries = _pBaseView.Factory.CreateCriteriaSet();
                            criteries.set_Criterion("grym_rub:name", "Почтовые отделения");
                            if (_currentCity.Equals(city, StringComparison.CurrentCultureIgnoreCase))
                            {
                                criteries.set_Criterion("grym_name", number);
                                criteries.set_Criterion("grym_city:idx", 1);
                            }
                            else
                            {
                                criteries.set_Criterion("grym_name", first.Name);
                            }
                            _pBaseView.Frame.DirectoryCollection.Search(criteries, "Почтовое отделение " + postIndex, "<criterion>Почтовое отделение</criterion><description>" + first.Name + "</description>");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + e.StackTrace + e.GetType().ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Упс, не удалось найти почтовое отделение по индексу. Попробуйте еще раз.");
                    }
                }
                return true;
            }
            return false;
        }

        public IRasterSet Images { get; set; }
    }
}
