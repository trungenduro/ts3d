// Decompiled with JetBrains decompiler
// Type: TS_E3D.DataTransferAddin
// Assembly: TS_E3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 659D0519-D3E2-4277-9257-4F7DDB844680
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\TS_E3D.dll

using Aveva.ApplicationFramework;
using Aveva.ApplicationFramework.Presentation;
using System.Drawing;
using System.IO;
using System.Reflection;
using TS_E3D.Properties;

namespace TS_E3D
{
  public class DataTransferAddin : IAddin
  {
    public static ServiceManager ServiceManager;

    string IAddin.Name
    {
      get
      {
        return "TS Import-Export";
      }
    }

    string IAddin.Description
    {
      get
      {
        return "TS Import-Export";
      }
    }

    void IAddin.Start(ServiceManager serviceManager)
    {
      DataTransferAddin.ServiceManager = serviceManager;
      WindowManager service1 = (WindowManager) serviceManager.GetService(typeof (WindowManager));
      CommandManager service2 = (CommandManager) serviceManager.GetService(typeof (CommandManager));
      CommandBarManager service3 = (CommandBarManager) serviceManager.GetService(typeof (CommandBarManager));
      service2.Commands.Add((Command) new TS_E3DCommand(service1));
      service3.RootTools.AddButtonTool("TS_E3DCommand", "Tekla Interoperability", (Image) Resources.ResourceManager.GetObject("tekla"), "TS.E3D.TS_E3D");
      if (new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CompanySettingsForE3DRibbon.txt")).Exists)
        return;
      service3.CommandBars.AddCommandBar("Tekla Interoperability Bar").Tools.AddTool("TS_E3DCommand");

    }

    void IAddin.Stop()
    {
    }
  }
}
