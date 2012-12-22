
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration;
//using Microsoft.SqlServer.Management.Smo.RegSvrEnum;

namespace DatabaseObjectSearcher
{
    public class MSSQLController
    {
        private static MSSQLController currentInstance = new MSSQLController();

        // private DTE2 _application;

        private EnvDTE.Window toolWindow;

        private DatabaseObjectSearcher.ObjectExplorerManager manager = new DatabaseObjectSearcher.ObjectExplorerManager();

        public static MSSQLController Current
        {
            get
            {
                return currentInstance;
            }
        }

        public EnvDTE.Window SearchWindow
        {
            get
            {
                return toolWindow;
            }
        }

        public void CreateAddinWindow(AddIn addinInstance)
        {
            Assembly asm = Assembly.Load("HuntingDog");

            // Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b9"); // this guid doesn't seem to matter?
            //toolWindow = CreateToolWindow("DatabaseObjectSearcherUI.ucMainControl", asm.Location, id,  addinInstance);

            Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b8"); // this guid doesn't seem to matter?
            toolWindow = CreateToolWindow("HuntingDog.ucHost", asm.Location, id, addinInstance);

            //if (_application == null)
            //{
            //    _application = application;
            //}
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        private EnvDTE.Window CreateToolWindow(String typeName, String assemblyLocation, Guid uiTypeGuid, AddIn addinInstance)
        {
            Windows2 win2 = ServiceCache.ExtensibilityModel.Windows as Windows2;
            //Windows2 win2 = applicationObject.Windows as Windows2;

            if (win2 != null)
            {
                Object controlObject = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                EnvDTE.Window toolWindow = win2.CreateToolWindow2(addinInstance, assemblyLocation, typeName, "Hunting Dog", "{" + uiTypeGuid.ToString() + "}", ref controlObject);
                EnvDTE.Window oe = null;

                foreach (EnvDTE.Window w1 in addinInstance.DTE.Windows)
                {
                    if (w1.Caption == "Object Explorer")
                    {
                        oe = w1;

                        //if(oe.LinkedWindows!=null)
                        //    oe.LinkedWindows.Add(toolWindow);  
                    }
                }

                //toolWindow.Width = oe.Width;
                // toolWindow.SetKind((vsWindowType)oe.Kind);
                // toolWindow.IsFloating = oe.IsFloating;

                // oe.LinkedWindows.Add(toolWindow);

                //Window frame = win2.CreateLinkedWindowFrame(toolWindow,oe, vsLinkedWindowType.vsLinkedWindowTypeHorizontal);

                //frame.SetKind(vsWindowType.vsWindowTypeDocumentOutline);

                //addinInstance.DTE.MainWindow.LinkedWindows.Add(frame);

                //frame.Activate();
                //HuntingDog.Properties.Resources.spider1.MakeTransparent(System.Drawing.Color.FromArgb(0, 255, 0));

                //stdole.IPicture tabPic = Support.ImageToIPicture(img) as stdole.IPicture;

                toolWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                toolWindow.Visible = true;

                // toolWindow.Linkable = true;
                //  toolWindow.IsFloating = false;

                //    addinInstance.DTE.MainWindow.LinkedWindows.Add(toolWindow);
                //if (oe != null)
                //{

                //    if (toolWindow.LinkedWindowFrame == null)
                //    {
                //        if (oe.LinkedWindowFrame != null)
                //        {
                //            oe.LinkedWindowFrame.LinkedWindows.Add(toolWindow);
                //        }
                //        else
                //        {
                //            toolWindow.Left = oe.Left;
                //            toolWindow.Top = oe.Top;
                //            toolWindow.Width = oe.Width;
                //            toolWindow.Height = oe.Height;

                //            Window2 winFrame = (Window2)win2.CreateLinkedWindowFrame(oe, toolWindow, vsLinkedWindowType.vsLinkedWindowTypeHorizontal);
                //        }
                //    }

                //    //winFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_MdiChild);  
                //    //winFrame.Linkable = true;
                //    //winFrame.IsFloating = false;
                //}

                return toolWindow;
            }

            return null;
        }
    }
}
