using System;
using System.Text;
using System.Runtime.InteropServices;
using GrymCore;
using System.Windows.Forms;

namespace DGisPostOfficeByIndex
{
    [Guid("18842281-90fc-4a58-87af-fae18072ca7f")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class PluginImpl
        : GrymCore.IGrymPlugin
        , GrymCore.IGrymPluginInfo
    {
        public PluginImpl()
        {
            this._xmlInfo =
                @"<grym_plugin_info>
					<name>Поиск отделения почтовой связи по индексу</name>
					<tag>DGIS.DGisPostOfficeByIndex</tag>
					<requirements>
						<requirement_api>API-1.4</requirement_api>
					</requirements>
                    <supported_languages>
                        <language>ru</language>
                    </supported_languages>
				</grym_plugin_info>";
        }

        private GrymCore.IGrym _grymApp;
        public GrymCore.IGrym GrymApp
        {
            get { return this._grymApp; }
        }

        private GrymCore.IBaseViewThread _baseView;
        public GrymCore.IBaseViewThread BaseView
        {
            get { return this._baseView; }
        }

        #region IGrymPlugin Members

        public void Initialize(Grym pRoot, IBaseViewThread pBaseView)
        {
            try
            {
                //сохраняем указатели на приложение Grym и оболочку просмотра данных
                this._grymApp = pRoot;
                this._baseView = pBaseView;

                // TODO: создать команды либо инициализировать класс, в котором эти команды создаются
                CustomMainController c = new CustomMainController(pBaseView);
            }
            catch
            {
                Terminate();
                throw;
            }
        }

        public void Terminate()
        {
            if (this._grymApp != null)
                Marshal.FinalReleaseComObject(this._grymApp);
            if (this._baseView != null)
                Marshal.FinalReleaseComObject(this._baseView);

            this._grymApp = null;
            this._baseView = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion

        #region IGrymPluginInfo Members

        private string _xmlInfo;
        public string XMLInfo
        {
            get { return this._xmlInfo; }
        }

        #endregion
    }
}
