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

            ITable citiesTable = pBaseView.Database.Table["grym_map_city"];
            _cities = new Dictionary<string, string>(citiesTable.RecordCount);
            // заполняем таблицу городов для локализации почтовых отделений
            for (int i = 1; i <= citiesTable.RecordCount; i++)
            {
                IDataRow dr = (IDataRow)citiesTable.GetRecord(i).Value["city"];
                // название населенного пункта в 2Гис
                string cityName = dr.Value["name"].ToString();
                // название населенного пункта в справочнике почтовых индексов
                string unifiedName = NormalizeCityName(cityName);
                // иногда названия населенных пунктов повторяются, бывает, будем исопльзовать встроенные возможности 2Гис для поиска по названию населенного пункта а не по ID
                if (!_cities.ContainsKey(unifiedName))
                {
                    _cities.Add(unifiedName, cityName);
                }
            }
        }

        public static string NormalizeCityName(String str)
        {
            // Приводим название населенного пункта к тома, как оно записано в базе индексов
            return str.ToUpper().Replace('Ё', 'Е');
        }

        private static Regex CITY_POST_OFFICE_NAME = new Regex("^(.*)\\s+(\\d+)$");
        private IBaseViewThread _pBaseView;
        private Dictionary<String, String> _cities;

        public string MakeActions(IFeature f) 
        {
            IDataRow dr = f as IDataRow;
            if ((dr != null) && ("grym_map_building".Equals(dr.Type)) && (dr.Value["post_index"].ToString().Length==6))
            {
                // Для зданий у которых указан почтовый индекс, выводим ссылку "Найти почтовое отделение"
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
                    // Получаем название почтового отделения
                    string postOfficeName = LocalFileInformationService.Instance.GetPostOffice(postIndex);
                    if (postOfficeName != null)
                    {
                        // Название отделения в виде "<Населенный пункт> <номер>"
                        Match m = CITY_POST_OFFICE_NAME.Match(postOfficeName);
                        String city;
                        String number;
                        if (m.Success)
                        {
                            // Почтовое отделение с номером
                            city = m.Groups[1].Value;
                            number = m.Groups[2].Value;
                        }
                        else
                        {
                            // Почтамт, либо единствненое отделение в маленьком населенном пункте
                            city = postOfficeName;
                            number = null;
                        }
                        try
                        {
                            ICriteriaSet criteries = _pBaseView.Factory.CreateCriteriaSet();
                            // ищем организации в рубрике "Почтовые отделения"
                            criteries.set_Criterion("grym_rub:name", "Почтовые отделения");
                            string gisCityName;
                            if (_cities.TryGetValue(city, out gisCityName))
                            {
                                // если в базе есть населенный пункт с названием, совпадающим с названием почтовго отделения, локализуем поиск в данном населенном пункте
                                criteries.set_Criterion("grym_city:name", gisCityName);
                            }
                            if (number != null)
                            {
                                // если у отделения есть номер, скорее всего он будет в его названии
                                criteries.set_Criterion("grym_name", number);
                            }
                            else
                            {
                                // узнаем число отделений в населенном пункте отделения
                                int officesCount = LocalFileInformationService.Instance.GetCityPostOffices(city, postIndex.Substring(0, 3));
                                if (officesCount > 2)
                                {
                                    // если в городе больше двух (для верности) почтовых отделений, то отделение без номера скорее всего называется "Почтамт"
                                    criteries.set_Criterion("grym_name", "Почтамт");
                                }
                                else if (String.IsNullOrEmpty(gisCityName))
                                {
                                    // иначе если не удалось локализовать поиск по населенному пункту (например пос. Светлый в г. Томск не входит в базу населенных пунктов), ищем назвнаие населенного пункта в названии почтового отделения
                                    // остается вопрос как быть с почтовыми отделениями, названия которых не соответствуют названиям населенных пунктов, например отделенеие Томь в Черной речке и Тимирязевский в Тимирязево.
                                    if (((int)dr.Value["addr_count"]) > 0)
                                    {
                                        // определяем город в котором находится данный дом
                                        string featureCity = dr.Value["city"].ToString();
                                        // узнаем число отделений в населенном пункте к которому относится здание
                                        int officesCount2 = LocalFileInformationService.Instance.GetCityPostOffices(NormalizeCityName(featureCity), postIndex.Substring(0, 3));
                                        if (officesCount2 > 0)
                                        {
                                            // если мы находимся в населенном пункте с несколькими отделениями, значит скорее всего мы в поселке, входящем в состав города (не вынесен как отдельный населенный пункт) (пос. Светлый, Томск)
                                            // значит нужно искать по названию отделения
                                            criteries.set_Criterion("grym_name", city);
                                        }
                                        else
                                        {
                                            // в населенном пункте нет почтовых отделений называющихся так же как и сам населенный пункт. Странно, придется просто вывести все почтовые отделения в населенном пункте
                                            // например почтовое отделение в пос. Черная речка, Томск называется Томь
                                            criteries.set_Criterion("grym_city:name", featureCity);
                                        }
                                        // else Как быть с поселком Тимирязево, который входит состав города Томска, а почтовое отделение называется Тимирязевский?
                                    }
                                    else
                                    {
                                        // Дом без адреса? Странно, откуда тогда у него индекс
                                        criteries.set_Criterion("grym_name", city);
                                    }
                                }
                            }
                            _pBaseView.Frame.DirectoryCollection.Search(criteries, "Почтовое отделение " + postIndex, "<criterion>Почтовое отделение</criterion><description>" + postOfficeName + "</description>");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + e.StackTrace + e.GetType().ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Упс, похоже такого индекса не существует.");
                    }
                }
                return true;
            }
            return false;
        }

        public IRasterSet Images { get; set; }
    }
}