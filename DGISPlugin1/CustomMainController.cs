using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using GrymCore;
using System.Linq;

namespace DGisPostOfficeByIndex
{
    class CustomMainController : IMapInfoController, IControlAppearance, IObjectCustomization
    {
        private IMapInfoController _innerController;
        private IBaseViewThread _pBaseView;
        private string _currentCity;
        private static Regex INDEX_LINK = new Regex("<span(.*) style=\"(.*)\"(.*)>(\\d{6}), ");
        private static Regex CITY_POST_OFFICE_NAME = new Regex("^(.*)\\s(\\d+)$");

        public CustomMainController(IBaseViewThread pBaseView)
        {
            _innerController = ((GrymCore.IMapInfoControllers2)pBaseView.Frame.Map.MapInfoControllers).FindMapInfoController("Grym.MapInfo.Default");
            _pBaseView = pBaseView;
            _currentCity=_pBaseView.BaseReference.Name;
            ((GrymCore.IMapInfoControllers2)pBaseView.Frame.Map.MapInfoControllers).RemoveController(_innerController);
            ((GrymCore.IMapInfoControllers2)pBaseView.Frame.Map.MapInfoControllers).AddController(this);

            PostalInformationServiceManager.Instance.BaseViewThread = _pBaseView;

            //AsyncRefreshServices caller = PostalInformationServiceManager.Instance.RefreshServices;
            //caller.BeginInvoke(null, null);
        }

        public bool Check(IFeature f)
        {
            return _innerController.Check(f);
        }

        public string Caption
        {
            get { return ((IControlAppearance)_innerController).Caption; }
        }

        public string Description
        {
            get { return ((IControlAppearance)_innerController).Description; }
        }

        public object Icon
        {
            get { return ((IControlAppearance)_innerController).Icon; }
        }

        public string Tag
        {
            get { return "Grym.MapInfo.Default"; }
        }

        public void Fill(IFeature f, CalloutTab t)
        {
            Callout c = t.Callout;
            /*CalloutTab ct1 = c.AddTab("addtab");
            ct1.Title = "add";
            ct1.Text = "add";*/
            CalloutTab ct2 = c.InsertTab(t, "insert");
            ct2.Title = "insert";
            ct2.Text = "insert";
            _innerController.Fill(f,t);
            //MessageBox.Show(t.Text);
            t.Text = INDEX_LINK.Replace(t.Text, "<span$1 style=\"$2\"$3><a style=\"$2\" href=\"search_post:$4\">$4</a>, ");
        }

        public void OnTabAction(CalloutTab t, string s)
        {
            
            //_pBaseView.Frame.Map
            if (s.StartsWith("search_post"))
            {
                string postIndex = s.Substring(12,6);
                //PostOffice first = PostalInformationServiceManager.Instance.GetPostOffice(postIndex);
                PostOffice first = LocalFileInformationService.Instance.GetPostOffice(postIndex);
                if (first!=null)
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
                        if (_currentCity.Equals(city,StringComparison.CurrentCultureIgnoreCase))
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
                        MessageBox.Show(e.Message+e.StackTrace+e.GetType().ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Упс, не удалось найти почтовое отделение по индексу. Попробуйте еще раз.");
                }
            }
            else
            {
                _innerController.OnTabAction(t, s);
            }
        }

        public string Title
        {
            get { return _innerController.Title; }
        }

        public void RegisterCustomizer(object o)
        {
            ((IObjectCustomization)_innerController).RegisterCustomizer(o);
        }

        public void UnregisterCustomizer(object o)
        {
            ((IObjectCustomization)_innerController).UnregisterCustomizer(o);
        }
    }
}