// Decompiled with JetBrains decompiler
// Type: TS_E3D.TS_E3DCommand
// Assembly: TS_E3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 659D0519-D3E2-4277-9257-4F7DDB844680
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\TS_E3D.dll

using Aveva.ApplicationFramework.Presentation;
using System;
using System.Windows.Forms;

namespace TS_E3D
{
  public class TS_E3DCommand : Command
  {
    private DockedWindow _myWindow;

    public TS_E3DCommand()
    {
      //base..ctor;
      this.Key = ("TS.E3D.TS_E3D");
    }

    public TS_E3DCommand1(WindowManager wndManager)
    {
      //base.\u002Ector();
      this.Key = ("TS.E3D.TS_E3D");
      this._myWindow = wndManager.CreateDockedWindow("Tekla Interoperability [2.1.13.1]", "Tekla Interoperability [2.1.13.1]", (Control) new TS_E3DControl(), (DockedPosition) 4);
      this._myWindow.Width = (700);
      this._myWindow.Height = (400);
      this._myWindow.SaveLayout = (true);
      wndManager.WindowLayoutLoaded += (new EventHandler(this.WndManagerWindowLayoutLoaded));
      this._myWindow.Closed += (new EventHandler(this.MyWindowClosed));
      this.ExecuteOnCheckedChange = (false);
    }

    private void MyWindowClosed(object sender, EventArgs e)
    {
      this.Checked = (false);
    }

    private void WndManagerWindowLayoutLoaded(object sender, EventArgs e)
    {
      this.Checked = (this._myWindow.Visible);
    }

    public virtual void Execute()
    {
      try
      {
        if (this._myWindow.Visible)
        {
          this._myWindow.Hide();
        }
        else
        {
          TS_E3DControl control = (TS_E3DControl) this._myWindow.Control;
          try
          {
            control.UpdateControl();
          }
          catch (Exception ex)
          {
          }
          this._myWindow.Show();
        }
        base.Execute();
      }
      catch (Exception ex)
      {
      }
    }
  }
}
