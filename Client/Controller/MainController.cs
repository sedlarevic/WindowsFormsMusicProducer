using Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Client.Controller
{
    public class MainController
    {

        private static MainController instance;
        public FrmMain frmMain;

        public static MainController Instance
        {
            get
            {
                if(instance == null)
                    instance = new MainController();
                return instance;
            }
        }

        public void ShowFrmMain(MusicProducer producer)
        {
            frmMain = new FrmMain(producer);
            frmMain.AutoSize = true;
            frmMain.ShowDialog();
        }
        public void Dispose()
        {
            frmMain.Dispose();
        }
        internal void Logout()
        {
            LogoutController.Instance.Logout();
        }

        internal void ShowMenu(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;

            string toolstripName =toolStripMenuItem.Name;
            string menuName = "UC" + toolstripName;
            Type className = Type.GetType("Client.UserControls." + menuName);
            ConstructorInfo constructor= className.GetConstructor(Type.EmptyTypes);
            UserControl userControl = (UserControl)constructor.Invoke(null);

            frmMain.MainPnl.Controls.Clear();
            frmMain.MainPnl.Controls.Add(userControl);
        }

    }
}
