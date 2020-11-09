// Decompiled with JetBrains decompiler
// Type: TS_E3D.TS_E3DControl
// Assembly: TS_E3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 659D0519-D3E2-4277-9257-4F7DDB844680
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\TS_E3D.dll

using Aveva.ApplicationFramework.Presentation;
using Aveva.Core.Database;
using IfcModelCollaboration;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TS_ModelConverter;
using ProgressBar = System.Windows.Forms.ProgressBar;

namespace TS_E3D
{
  public class TS_E3DControl : UserControl
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private IContainer components = (IContainer) null;
    private Mapping _mappingImport;
    private Mapping _mappingExport;
    private List<ImportToPdmsModel> _listOfModels;
    private List<ExportToTeklaModel> _listOfExportModels;
    private ArrayList _struList;
    private TabPage Import;
    private Label label1;
    private Label label4;
    private GroupBox groupBox1;
    private GroupBox groupBoxExport1;
    private Label label2;
    private Label label3;
    private DataGridView dataGridView2;
    private DataGridView dataGridView1;
    private DataGridView dataGridViewExport2;
    private DataGridView dataGridViewExport1;
    private ProgressBar progressBar1;
    private ProgressBar progressBar2;
    private Button buttonMapping;
    private Button buttonMapping1;
    private Button createModel;
    private Button exportModel;
    private TabControl tabControl1;
    private TabPage Export;
    private Button buttonRefresh;
    private Button buttonChangeViewer;

    public TS_E3DControl()
    {
      this.InitializeComponent();
      // ISSUE: method pointer
      DbEvents.AddDBFileChangedEventHandler(new DbEvents.ChangeEventHandler((object) this, __methodptr(SaveWork)));
    }

    private void SaveWork(DbRawChanges ch, DbEvents.operation op)
    {
      try
      {
        if (!op.Equals((object) (DbEvents.operation) 0) || !TS_ModelConverter.Tools.CheckMappingObject(this._mappingImport))
          return;
        Errors errors;
        if (!new PdmsInterface().RemoveBackUpFiles(this._mappingImport.ProjectPath.FullName, out errors))
        {
          int num = (int) MessageBox.Show(((int) errors.ErrorCode).ToString() + ": " + errors.ErrorInfo);
        }
        Mapping.ModelOpeningTimeUtc = DateTime.UtcNow;
      }
      catch (Exception ex)
      {
      }
    }

    public List<ImportToPdmsModel> ListOfModels
    {
      get
      {
        return this._listOfModels;
      }
    }

    public void UpdateControl()
    {
      try
      {
        this._listOfModels = new List<ImportToPdmsModel>();
        if (this._mappingImport.ProjectPath != null)
        {
          this._mappingImport.LoadUdaMapping();
          List<ImportToPdmsModel> list;
          ImportData importData;
          new PdmsInterface().GetImportModelsInformation(this._mappingImport.ProjectPath.FullName, out list, Mapping.ModelOpeningTimeUtc, out importData);
          foreach (ImportToPdmsModel importToPdmsModel in list)
            this._listOfModels.Add(importToPdmsModel);
        }
        this.label1.Text = string.Empty;
        this.UpdateDataGridView1Rows(false);
        this.dataGridView1.FirstDisplayedCell = (DataGridViewCell) null;
        this.dataGridView1.ClearSelection();
        this.label3.Text = string.Empty;
        this.UpdateDataGridViewExport1Rows();
        this.dataGridViewExport1.FirstDisplayedCell = (DataGridViewCell) null;
        this.dataGridViewExport1.ClearSelection();
      }
      catch (Exception ex)
      {
      }
    }

    public void ImportBatch()
    {
      bool enabledSuccessfully = RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully;
      PdmsInterface pd = new PdmsInterface();
      this._mappingImport = new Mapping(TS_ModelConverter.Tools.GetProjectPath(), TS_ModelConverter.Constants.System.IFC);
      this._mappingImport.LibraryProfiles.Clear();
      this._mappingImport.ParametricProfiles.Clear();
      this._mappingImport.ProfileDS = (DataSet) null;
      this._mappingImport.MaterialDS = (DataSet) null;
      this._listOfModels = new List<ImportToPdmsModel>();
      this._mappingImport.LibraryProfiles.Clear();
      this._mappingImport.ParametricProfiles.Clear();
      List<ImportToPdmsModel> list;
      ImportData importData;
      pd.GetImportModelsInformation(this._mappingImport.ProjectPath.FullName, out list, Mapping.ModelOpeningTimeUtc, out importData);
      foreach (ImportToPdmsModel importToPdmsModel in list)
      {
        this._listOfModels.Add(importToPdmsModel);
        pd.GetImportModelInformation(this._mappingImport.ProjectPath.FullName, importToPdmsModel, out importData);
        pd.StoreImportModelToPdmsData(this._mappingImport.ProjectPath.FullName, importToPdmsModel, ref importData);
        this._mappingImport.LoadMapping(importData, pd);
      }
      foreach (ImportToPdmsModel listOfModel in this._listOfModels)
      {
        DbElement element = DbElement.GetElement(listOfModel.Stru.StartsWith("/") ? listOfModel.Stru : "/" + listOfModel.Stru);
        if (!element.get_IsNull())
          CurrentElement.set_Element(element);
        this.ImportRow(listOfModel, pd, true);
      }
    }

    public void ExportBatch()
    {
      PdmsInterface pdmsInterface = new PdmsInterface();
      this._mappingExport = new Mapping(TS_ModelConverter.Tools.GetProjectPath(), TS_ModelConverter.Constants.System.PDMS);
      ExportData data;
      pdmsInterface.GetExportSettings(this._mappingExport.ProjectPath.FullName, out this._listOfExportModels, out data);
      foreach (ExportToTeklaModel listOfExportModel in this._listOfExportModels)
        this.ExportRow(listOfExportModel, true);
    }

    private void TsPdmsDialogLoad(object sender, EventArgs e)
    {
      try
      {
        if (!new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CompanySettingsForE3DRibbon.txt")).Exists)
        {
          CommandBarManager service = (CommandBarManager) DataTransferAddin.ServiceManager.GetService(typeof (CommandBarManager));
          ITool itool = service.get_Ribbon().get_RibbonTabs().AddRibbonTab("TEKLA", string.Empty).get_Groups().AddRibbonTabGroup("Tekla Interoperability").get_Tools().AddTool("TS_E3DCommand");
          itool.ImageOnly = (true);
          itool.Image = (itool.Image);
          itool.set_DisplayLargeImage(true);
          itool.set_IsFirstInGroup(true);
          if (service.get_CommandBars().Contains("Tekla Interoperability Bar"))
            service.get_CommandBars().RemoveCommandBar("Tekla Interoperability Bar");
        }
        this._listOfModels = new List<ImportToPdmsModel>();
        this._mappingImport = new Mapping(TS_ModelConverter.Tools.GetProjectPath(), TS_ModelConverter.Constants.System.IFC);
        this._mappingExport = new Mapping(TS_ModelConverter.Tools.GetProjectPath(), TS_ModelConverter.Constants.System.PDMS);
        this.label1.Text = string.Empty;
        this._struList = TS_ModelConverter.Tools.GetStruList();
        Mapping.ModelOpeningTimeUtc = DateTime.UtcNow;
        this.CreateDataGridView1Columns();
        if (this._mappingImport.ProjectPath != null)
        {
          List<ImportToPdmsModel> list;
          ImportData importData;
          new PdmsInterface().GetImportModelsInformation(this._mappingImport.ProjectPath.FullName, out list, Mapping.ModelOpeningTimeUtc, out importData);
          foreach (ImportToPdmsModel importToPdmsModel in list)
            this._listOfModels.Add(importToPdmsModel);
        }
        this.UpdateDataGridView1Rows(false);
        this.InitializeDataGridView2();
        this.dataGridView1.FirstDisplayedCell = (DataGridViewCell) null;
        this.dataGridView1.ClearSelection();
        this.label3.Text = string.Empty;
        this.CreateDataGridViewExport1Columns();
        this.UpdateDataGridViewExport1Rows();
        this.InitializeDataGridViewExport2();
        this.dataGridViewExport1.FirstDisplayedCell = (DataGridViewCell) null;
        this.dataGridViewExport1.ClearSelection();
      }
      catch (Exception ex)
      {
      }
    }

    private void Add()
    {
      Cursor.Current = Cursors.WaitCursor;
      PdmsInterface pd = new PdmsInterface();
      Errors errors;
      ProjectSettings projectSettings = pd.GetProjectSettings(this._mappingImport.ProjectPath.FullName, out errors);
      bool avevaCreatedObjects = projectSettings.UpdateAvevaCreatedObjects;
      this._mappingImport.UpdateAvevaHierarchy = projectSettings.UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy;
      if (string.IsNullOrEmpty(this.dataGridView2.Rows[0].Cells["Folder"].Value.ToString()))
      {
        int num1 = (int) MessageBox.Show("Nothing selected.");
      }
      else
      {
        FileInfo fileInfo = TS_ModelConverter.Tools.GetFileInfo(this.dataGridView2.Rows[0].Cells["Folder"].Value.ToString());
        if (!fileInfo.Name.Contains("#"))
        {
          int num2 = (int) MessageBox.Show("File does not have date tag, aborting.");
        }
        else
        {
          ImportToPdmsModel importToPdmsModel = (ImportToPdmsModel) null;
          ImportData importData = (ImportData) null;
          if (fileInfo.Exists)
          {
            string name = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf("#"));
            if (this.dataGridView1.Rows.Cast<DataGridViewRow>().Count<DataGridViewRow>((Func<DataGridViewRow, bool>) (r => r.Cells["Content"].Value.ToString().Equals(name))) > 0)
            {
              int num3 = (int) MessageBox.Show("File already exists, will not be added.");
              return;
            }
            pd.FirstImportToPdmsData(fileInfo.FullName, this._mappingImport.ProjectPath.FullName, out importData, out importToPdmsModel);
            if (importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.PacketIsNotValid))
            {
              int num3 = (int) MessageBox.Show("Packet is not valid, aborting.");
              return;
            }
            if (importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.InvalidFileName))
            {
              int num3 = (int) MessageBox.Show("Invalid file name (" + importData.Errors.ErrorInfo + "), aborting.");
              return;
            }
            if (importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.DataInformationReadingFailed))
            {
              int num3 = (int) MessageBox.Show("Error reading data information, aborting.");
              return;
            }
            if (!importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.None))
            {
              int num3 = (int) MessageBox.Show("Error reading package. " + importData.Errors.ErrorInfo + "\nAborting.");
              return;
            }
            string message;
            if (!this.CheckDatainformation(importData.DataInformation, 1, out message))
            {
              if (1 > importData.DataInformation.LinkVersion)
              {
                if (MessageBox.Show("Model link version is older than current application version, please ask Tekla user to update export application. Continue Yes/No", "Linkversion", MessageBoxButtons.YesNo) == DialogResult.No)
                  return;
              }
              else if (1 < importData.DataInformation.LinkVersion && MessageBox.Show("Model link version is newer than current application version, please update import application. Continue Yes/No", "Linkversion", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            }
          }
          string name1 = this.dataGridView2.Rows[0].Cells["Content"].Value.ToString().Replace(" ", "_");
          string str = string.Empty;
          if (CurrentElement.get_Element().get_IsDeleted() || !CurrentElement.get_Element().get_IsValid())
          {
            int num4 = (int) MessageBox.Show("Current element not selected, aborting.");
          }
          else
          {
            if (!((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.STRUCTURE) && !((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.FRMWORK))
            {
              if (!importData.DataInformation.Hierarchy)
              {
                int num3 = (int) MessageBox.Show("Selected hierarchy item is not a STRU/FRMW, aborting.");
                return;
              }
            }
            else
              str = CurrentElement.get_Element().GetString((DbAttribute) DbAttributeInstance.NAME).Substring(1);
            if (!importData.DataInformation.Hierarchy)
            {
              DbElement element;
              bool flag = TS_ModelConverter.Tools.ExistsInModel(name1, out element);
              if (!avevaCreatedObjects & flag)
              {
                int num3 = (int) MessageBox.Show(name1 + " already exists in model, aborting.");
                return;
              }
              if (avevaCreatedObjects & flag && (!((object) element.GetElementType()).Equals((object) DbElementTypeInstance.FRMWORK) && !((object) element.GetElementType()).Equals((object) DbElementTypeInstance.SBFRAMEWORK)))
              {
                int num3 = (int) MessageBox.Show(name1 + " exists in model but is not a FRMW/SBFR. Aborting.");
                return;
              }
            }
            if (importToPdmsModel != null)
            {
              importToPdmsModel.Stru = str;
              importToPdmsModel.Frmw = name1;
              importToPdmsModel.Negatives = (bool) this.dataGridView2.Rows[0].Cells["Negatives"].Value;
              if (string.IsNullOrEmpty(importToPdmsModel.DateTimeUtc))
                importToPdmsModel.DateTimeUtc = string.Empty;
              pd.StoreImportModelToPdmsData(this._mappingImport.ProjectPath.FullName, importToPdmsModel, ref importData);
              this.AddGridRow(importToPdmsModel);
              this._listOfModels.Add(importToPdmsModel);
              this._mappingImport.LoadMapping(importData, pd);
              this.dataGridView1.FirstDisplayedCell = (DataGridViewCell) null;
              this.dataGridView1.ClearSelection();
            }
            Cursor.Current = Cursors.Default;
          }
        }
      }
    }

    private void CreateModelClick(object sender, EventArgs e)
    {
      Cursor.Current = Cursors.WaitCursor;
      this.label1.Text = string.Empty;
      bool enabledSuccessfully = RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully;
      PdmsInterface pd = new PdmsInterface();
      List<int> list = this.dataGridView1.SelectedRows.Cast<DataGridViewRow>().Select<DataGridViewRow, int>((Func<DataGridViewRow, int>) (row1 => row1.Index)).ToList<int>();
      if (list.Count.Equals(0))
      {
        int num = (int) MessageBox.Show("No rows selected in datagrid.");
      }
      foreach (int index in list)
      {
        DataGridViewRow row = this.dataGridView1.Rows[index];
        this.ImportRow((ImportToPdmsModel) row.Tag, pd, false);
        row.DefaultCellStyle.BackColor = Color.Green;
      }
      Cursor.Current = Cursors.Default;
    }

    private void ButtonMappingImportClick(object sender, EventArgs e)
    {
      if (this._mappingImport == null)
        return;
      Form form;
      if (TS_ModelConverter.Tools.CheckOpened("Tekla Interoperability Import Mapping", out form))
      {
        form.WindowState = FormWindowState.Normal;
        form.Dock = DockStyle.Fill;
        int num = (int) form.ShowDialog();
        form.Focus();
      }
      else
      {
        MappingDialog mappingDialog1 = new MappingDialog(this, this._mappingImport);
        mappingDialog1.Name = "MappingDialog";
        mappingDialog1.Text = "Tekla Interoperability Import Mapping";
        MappingDialog mappingDialog2 = mappingDialog1;
        int num = (int) mappingDialog2.ShowDialog();
        mappingDialog2.Focus();
      }
    }

    private void CreateDataGridView1Columns()
    {
      if ((uint) this.dataGridView1.Columns.Count > 0U)
        return;
      DataGridViewTextBoxColumn viewTextBoxColumn1 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn1.Name = "Version";
      viewTextBoxColumn1.Resizable = DataGridViewTriState.False;
      viewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewTextBoxColumn1);
      DataGridViewTextBoxColumn viewTextBoxColumn2 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn2.Name = "Name";
      viewTextBoxColumn2.Resizable = DataGridViewTriState.True;
      viewTextBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewTextBoxColumn2);
      DataGridViewTextBoxColumn viewTextBoxColumn3 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn3.Name = "Content";
      viewTextBoxColumn3.Resizable = DataGridViewTriState.True;
      viewTextBoxColumn3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewTextBoxColumn3);
      DataGridViewCheckBoxColumn viewCheckBoxColumn = new DataGridViewCheckBoxColumn();
      viewCheckBoxColumn.Name = "Negatives";
      viewCheckBoxColumn.Resizable = DataGridViewTriState.False;
      viewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewCheckBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn4 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn4.Name = "Folder";
      viewTextBoxColumn4.ReadOnly = true;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewTextBoxColumn4);
      DataGridViewTextBoxColumn viewTextBoxColumn5 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn5.Name = "Browse";
      viewTextBoxColumn5.DataPropertyName = "Browse";
      viewTextBoxColumn5.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) viewTextBoxColumn5);
      DataGridViewImageColumn gridViewImageColumn1 = new DataGridViewImageColumn();
      gridViewImageColumn1.Name = "Remove";
      gridViewImageColumn1.DataPropertyName = "Remove";
      gridViewImageColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) gridViewImageColumn1);
      DataGridViewImageColumn gridViewImageColumn2 = new DataGridViewImageColumn();
      gridViewImageColumn2.Name = "Add";
      gridViewImageColumn2.DataPropertyName = "Add";
      gridViewImageColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView1.Columns.Add((DataGridViewColumn) gridViewImageColumn2);
      this.DataGridView1ColumnSizes();
    }

    private void UpdateDataGridView1Rows(bool updateSettings = true)
    {
      while (this.dataGridView1.Rows.Count > 0)
        this.dataGridView1.Rows.RemoveAt(0);
      PdmsInterface pd = new PdmsInterface();
      this._mappingImport.LibraryProfiles.Clear();
      this._mappingImport.ParametricProfiles.Clear();
      this._mappingImport.ProfileDS = (DataSet) null;
      this._mappingImport.MaterialDS = (DataSet) null;
      foreach (ImportToPdmsModel listOfModel in this._listOfModels)
      {
        ImportData importData;
        pd.GetImportModelInformation(this._mappingImport.ProjectPath.FullName, listOfModel, out importData);
        if (updateSettings)
          pd.StoreImportModelToPdmsData(this._mappingImport.ProjectPath.FullName, listOfModel, ref importData);
        this.AddGridRow(listOfModel);
        this._mappingImport.LoadMapping(importData, pd);
      }
    }

    private void DataGridView1ColumnSizes()
    {
      foreach (DataGridViewColumn column in (BaseCollection) this.dataGridView1.Columns)
      {
        switch (column.Name)
        {
          case "Add":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Browse":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Content":
            column.HeaderText = "Current item";
            column.ReadOnly = true;
            column.Width = 140;
            column.Visible = true;
            break;
          case "Name":
            column.HeaderText = "Hierarchy item";
            column.ReadOnly = true;
            column.Width = 190;
            column.Visible = true;
            break;
          case "Negatives":
            column.HeaderText = "Negatives";
            column.ReadOnly = true;
            column.Width = 60;
            column.Visible = true;
            break;
          case "Remove":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Version":
            column.HeaderText = "Version";
            column.ReadOnly = true;
            column.Width = 105;
            column.Visible = true;
            break;
        }
      }
    }

    private void DataGridView1CellClick(object sender, DataGridViewCellEventArgs e)
    {
      try
      {
        DataGridViewColumn column1 = this.dataGridView1.Columns["Browse"];
        if (column1 != null && e.ColumnIndex == column1.Index)
        {
          string str1 = this.dataGridView1.Rows[e.RowIndex].Cells["Content"].Value.ToString();
          string str2 = this.BrowseFolder("Select folder");
          if (string.IsNullOrEmpty(str2))
            return;
          foreach (ImportToPdmsModel listOfModel in this._listOfModels)
          {
            if (!(listOfModel.Name != str1))
            {
              listOfModel.Location = str2;
              this.UpdateDataGridView1Rows(true);
              this.dataGridView1.Refresh();
              break;
            }
          }
        }
        else
        {
          DataGridViewColumn column2 = this.dataGridView1.Columns["Remove"];
          if (column2 != null && e.ColumnIndex == column2.Index)
          {
            if (MessageBox.Show("Are you sure you want to remove selected?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
              return;
            string str = this.dataGridView1.Rows[e.RowIndex].Cells["Content"].Value.ToString();
            PdmsInterface pdmsInterface = new PdmsInterface();
            for (int index = 0; index < this._listOfModels.Count; ++index)
            {
              if (!(this._listOfModels[index].Frmw != str))
              {
                ImportToPdmsModel listOfModel = this._listOfModels[index];
                ImportToPdmsModel importToPdmsModel = pdmsInterface.CheckImportVersion(listOfModel.Name, listOfModel);
                pdmsInterface.RemoveImportInstance(this._mappingImport.ProjectPath.FullName, importToPdmsModel.Name);
                this._listOfModels.Remove(listOfModel);
                if (TS_ModelConverter.Tools.ExistsInModel(importToPdmsModel.Frmw) && MessageBox.Show("Delete FRMW/SBFR in model?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                  DbElement owner = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + importToPdmsModel.Frmw).get_Owner();
                  ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + importToPdmsModel.Frmw).Delete();
                  CurrentElement.set_Element(owner);
                  break;
                }
                break;
              }
            }
            this.UpdateDataGridView1Rows(true);
          }
          else
          {
            DataGridViewColumn column3 = this.dataGridView1.Columns["Add"];
            if (column3 != null && e.ColumnIndex == column3.Index)
              this.OpenRootFolderImport(this.dataGridView1.Rows[e.RowIndex].Cells["Folder"].Value.ToString());
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void InitializeDataGridView2()
    {
      DataGridViewComboBoxColumn viewComboBoxColumn = new DataGridViewComboBoxColumn();
      viewComboBoxColumn.DataSource = (object) this._struList;
      viewComboBoxColumn.DisplayIndex = 0;
      viewComboBoxColumn.Name = "Name";
      viewComboBoxColumn.SortMode = DataGridViewColumnSortMode.Automatic;
      viewComboBoxColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
      viewComboBoxColumn.Resizable = DataGridViewTriState.False;
      viewComboBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewComboBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn1 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn1.Name = "Content";
      viewTextBoxColumn1.Resizable = DataGridViewTriState.False;
      viewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewTextBoxColumn1);
      DataGridViewCheckBoxColumn viewCheckBoxColumn = new DataGridViewCheckBoxColumn();
      viewCheckBoxColumn.Name = "Negatives";
      viewCheckBoxColumn.Resizable = DataGridViewTriState.False;
      viewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewCheckBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn2 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn2.Name = "Folder";
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewTextBoxColumn2);
      DataGridViewButtonColumn viewButtonColumn1 = new DataGridViewButtonColumn();
      viewButtonColumn1.Name = "Browse";
      viewButtonColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      viewButtonColumn1.Resizable = DataGridViewTriState.False;
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewButtonColumn1);
      DataGridViewButtonColumn viewButtonColumn2 = new DataGridViewButtonColumn();
      viewButtonColumn2.Name = "Add";
      viewButtonColumn2.Resizable = DataGridViewTriState.False;
      viewButtonColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridView2.Columns.Add((DataGridViewColumn) viewButtonColumn2);
      this.DataGridView2ColumnSizes();
      int index = this.dataGridView2.Rows.Add();
      this.dataGridView2.RowCount = 1;
      this.dataGridView2.Rows[index].Cells["Name"].Value = (object) this._struList[0].ToString();
      this.dataGridView2.Rows[index].Cells["Content"].Value = (object) string.Empty;
      this.dataGridView2.Rows[index].Cells["Folder"].Value = (object) string.Empty;
      this.dataGridView2.Rows[index].Cells["Negatives"].Value = (object) true;
      this.dataGridView2.Rows[index].Cells["Browse"].Value = (object) "...";
      this.dataGridView2.Rows[index].Cells["Browse"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridView2.Rows[index].Cells["Add"].Value = (object) "Add";
      this.dataGridView2.Refresh();
      this.dataGridView2.FirstDisplayedCell = (DataGridViewCell) null;
      this.dataGridView2.ClearSelection();
    }

    private void DataGridView2ColumnSizes()
    {
      foreach (DataGridViewColumn column in (BaseCollection) this.dataGridView2.Columns)
      {
        string name = column.Name;
        if (!(name == "Name"))
        {
          if (!(name == "Content"))
          {
            if (!(name == "Negatives"))
            {
              if (!(name == "Folder"))
              {
                if (!(name == "Browse"))
                {
                  if (name == "Add")
                  {
                    column.ReadOnly = true;
                    column.Width = 60;
                  }
                }
                else
                {
                  column.ReadOnly = true;
                  column.Width = 20;
                }
              }
              else
                column.Width = 80;
            }
            else
              column.Width = 60;
          }
          else
            column.Width = 140;
        }
        else
          column.Width = 190;
      }
    }

    private void DataGridView2CellClick(object sender, DataGridViewCellEventArgs e)
    {
      DataGridViewColumn column1 = this.dataGridView2.Columns["Browse"];
      if (column1 != null && e.ColumnIndex == column1.Index)
      {
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        openFileDialog1.DefaultExt = "tczip";
        openFileDialog1.Filter = "TCZIP file (*.tczip)|*.tczip|IFCZIP file (*.ifczip)|*.ifczip|All files (*.*)|*.*";
        openFileDialog1.AddExtension = true;
        openFileDialog1.RestoreDirectory = true;
        openFileDialog1.Title = "Open model import file";
        openFileDialog1.InitialDirectory = this._mappingImport.ProjectPath.FullName;
        OpenFileDialog openFileDialog2 = openFileDialog1;
        if (openFileDialog2.ShowDialog() == DialogResult.OK)
        {
          FileInfo fileInfo = new FileInfo(openFileDialog2.FileName);
          string str = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension, StringComparison.Ordinal));
          this.dataGridView2.Rows[0].Cells["Content"].Value = str.Contains("#") ? (object) str.Substring(0, str.IndexOf("#", StringComparison.Ordinal)) : (object) string.Empty;
          this.dataGridView2.Rows[0].Cells["Folder"].Value = (object) openFileDialog2.FileName;
        }
        openFileDialog2.Dispose();
      }
      else
      {
        DataGridViewColumn column2 = this.dataGridView2.Columns["Add"];
        if (column2 != null && e.ColumnIndex == column2.Index)
          this.Add();
      }
    }

    private void DataGridView2DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
    }

    private void AddGridRow(ImportToPdmsModel importToPdmsModel)
    {
      int index = this.dataGridView1.Rows.Add();
      this.dataGridView1.Rows[index].Tag = (object) importToPdmsModel;
      this.dataGridView1.Rows[index].Cells["Version"].Value = (object) importToPdmsModel.DateTimeUtc;
      this.dataGridView1.Rows[index].Cells["Name"].Value = (object) importToPdmsModel.Stru;
      this.dataGridView1.Rows[index].Cells["Content"].Value = (object) importToPdmsModel.Frmw;
      this.dataGridView1.Rows[index].Cells["Negatives"].Value = (object) importToPdmsModel.Negatives;
      this.dataGridView1.Rows[index].Cells["Negatives"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridView1.Rows[index].Cells["Folder"].Value = (object) importToPdmsModel.Location;
      this.dataGridView1.Rows[index].Cells["Browse"].Value = (object) "...";
      this.dataGridView1.Rows[index].Cells["Browse"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridView1.Rows[index].Cells["Remove"].Value = TS_E3D.Properties.Resources.ResourceManager.GetObject("Remove_16");
      this.dataGridView1.Rows[index].Cells["Remove"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridView1.Rows[index].Cells["Add"].Value = TS_E3D.Properties.Resources.ResourceManager.GetObject("Open_16");
      this.dataGridView1.Rows[index].Cells["Add"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      if (string.IsNullOrEmpty(importToPdmsModel.NewDateTimeUtc))
        return;
      this.dataGridView1.Rows[index].Cells["Version"].Value = (object) importToPdmsModel.NewDateTimeUtc;
      this.dataGridView1.Rows[index].DefaultCellStyle.BackColor = Color.Yellow;
    }

    private void OpenRootFolderImport(string folderLocation)
    {
      if (folderLocation.StartsWith("."))
        folderLocation = Path.Combine(this._mappingImport.ProjectPath.FullName, folderLocation);
      if (!Directory.Exists(folderLocation))
        return;
      Process.Start(folderLocation);
    }

    private void AddExport()
    {
      Cursor.Current = Cursors.WaitCursor;
      DbElement element1 = CurrentElement.get_Element();
      TS_ModelConverter.Tools.GetFileInfo(this.dataGridViewExport2.Rows[0].Cells["Folder"].Value.ToString());
      string struName = this.dataGridViewExport2.Rows[0].Cells["Name"].Value.ToString();
      if (struName.Equals("--- Selected PDMS hierarchy item ---"))
      {
        struName = CurrentElement.get_Element().GetString((DbAttribute) DbAttributeInstance.NAME).Substring(1);
        struName = struName.Equals("*") ? "WORLD" : struName;
      }
      else
        CurrentElement.set_Element(((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + struName));
      if (this.dataGridViewExport1.Rows.Cast<DataGridViewRow>().Count<DataGridViewRow>((Func<DataGridViewRow, bool>) (r => r.Cells["Name"].Value.ToString().Equals(struName))) > 0)
      {
        int num1 = (int) MessageBox.Show("File already exists, will not be added.");
      }
      else
      {
        try
        {
          DbElement element2 = CurrentElement.get_Element();
          ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + struName);
          ExportToTeklaModel exportVersionData = new ExportToTeklaModel()
          {
            Name = TS_ModelConverter.Tools.ReplaceSpecialChar(struName),
            Folder = this.dataGridViewExport2.Rows[0].Cells["Folder"].Value.ToString()
          };
          exportVersionData.FolderAbsolute = exportVersionData.Folder;
          exportVersionData.Negatives = (bool) this.dataGridViewExport2.Rows[0].Cells["Negatives"].Value;
          exportVersionData.HierarchyName = exportVersionData.Name;
          exportVersionData.SoftwareVersion = "E3D2.1";
          exportVersionData.ApplicationVersion = 1;
          exportVersionData.HierarchyId = element2.RefNo()[0].ToString() + "/" + (object) element2.RefNo()[1];
          this.AddGridRow(exportVersionData);
          this._listOfExportModels.Add(exportVersionData);
          this.dataGridViewExport1.FirstDisplayedCell = (DataGridViewCell) null;
          this.dataGridViewExport1.ClearSelection();
          PdmsInterface pdmsInterface = new PdmsInterface();
          IfcModelCollaboration.ErrorCode errorCode;
          List<string> errorList;
          string tobezipped;
          ExportData exportData;
          if (!pdmsInterface.ExportModelToTekla(this._mappingExport.ProjectPath.FullName, ref exportVersionData, out errorCode, out errorList, out tobezipped, out exportData, false))
            TS_E3DControl.Log.Error((object) ("Error ExportModelToTekla: " + errorCode.ToString()));
          else
            pdmsInterface.StoreExportToTeklaModelInformation(exportVersionData, ref exportData);
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show("Error exporting stru: " + ex.Message);
          CurrentElement.set_Element(element1);
        }
        CurrentElement.set_Element(element1);
      }
    }

    private void ExportModelClick(object sender, EventArgs e)
    {
      this.label3.Text = string.Empty;
      Cursor.Current = Cursors.WaitCursor;
      bool enabledSuccessfully = RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully;
      List<int> list = this.dataGridViewExport1.SelectedRows.Cast<DataGridViewRow>().Select<DataGridViewRow, int>((Func<DataGridViewRow, int>) (row1 => row1.Index)).ToList<int>();
      if (list.Count.Equals(0))
      {
        int num = (int) MessageBox.Show("No rows selected in datagrid.");
      }
      foreach (int index in list)
      {
        DataGridViewRow row = this.dataGridViewExport1.Rows[index];
        ExportToTeklaModel tag = (ExportToTeklaModel) row.Tag;
        this.ExportRow(tag, false);
        if (this.dataGridViewExport1.Rows.Count > index)
        {
          row.Cells["Version"].Value = (object) tag.DateTimeUtc;
          row.DefaultCellStyle.BackColor = Color.Green;
        }
        else if (this.dataGridViewExport1.Rows.Count - 1 > index)
          row.DefaultCellStyle.BackColor = Color.Red;
      }
      Cursor.Current = Cursors.Default;
    }

    private void CreateDataGridViewExport1Columns()
    {
      if ((uint) this.dataGridViewExport1.Columns.Count > 0U)
        return;
      DataGridViewTextBoxColumn viewTextBoxColumn1 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn1.Name = "Version";
      viewTextBoxColumn1.Resizable = DataGridViewTriState.False;
      viewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      viewTextBoxColumn1.Width = 60;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewTextBoxColumn1);
      DataGridViewTextBoxColumn viewTextBoxColumn2 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn2.Name = "Name";
      viewTextBoxColumn2.Resizable = DataGridViewTriState.True;
      viewTextBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewTextBoxColumn2);
      DataGridViewTextBoxColumn viewTextBoxColumn3 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn3.Name = "Content";
      viewTextBoxColumn3.Resizable = DataGridViewTriState.True;
      viewTextBoxColumn3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewTextBoxColumn3);
      DataGridViewCheckBoxColumn viewCheckBoxColumn = new DataGridViewCheckBoxColumn();
      viewCheckBoxColumn.Name = "Negatives";
      viewCheckBoxColumn.Resizable = DataGridViewTriState.False;
      viewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewCheckBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn4 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn4.Name = "Folder";
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewTextBoxColumn4);
      DataGridViewTextBoxColumn viewTextBoxColumn5 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn5.Name = "Browse";
      viewTextBoxColumn5.DataPropertyName = "Browse";
      viewTextBoxColumn5.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      DataGridViewTextBoxColumn viewTextBoxColumn6 = viewTextBoxColumn5;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) viewTextBoxColumn6);
      DataGridViewImageColumn gridViewImageColumn1 = new DataGridViewImageColumn();
      gridViewImageColumn1.Name = "Remove";
      gridViewImageColumn1.DataPropertyName = "Remove";
      DataGridViewImageColumn gridViewImageColumn2 = gridViewImageColumn1;
      viewTextBoxColumn6.Resizable = DataGridViewTriState.False;
      gridViewImageColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) gridViewImageColumn2);
      DataGridViewImageColumn gridViewImageColumn3 = new DataGridViewImageColumn();
      gridViewImageColumn3.Name = "Add";
      gridViewImageColumn3.DataPropertyName = "Add";
      DataGridViewImageColumn gridViewImageColumn4 = gridViewImageColumn3;
      viewTextBoxColumn6.Resizable = DataGridViewTriState.False;
      gridViewImageColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport1.Columns.Add((DataGridViewColumn) gridViewImageColumn4);
      this.DataGridViewExport1ColumnSizes();
    }

    private void DataGridViewExport1ColumnSizes()
    {
      foreach (DataGridViewColumn column in (BaseCollection) this.dataGridViewExport1.Columns)
      {
        switch (column.Name)
        {
          case "Add":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Browse":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Content":
            column.HeaderText = "IFC package name";
            column.ReadOnly = true;
            column.Width = 140;
            column.Visible = true;
            break;
          case "Name":
            column.HeaderText = "Hierarchy item";
            column.ReadOnly = true;
            column.Width = 190;
            column.Visible = true;
            break;
          case "Negatives":
            column.HeaderText = "Negatives";
            column.ReadOnly = true;
            column.Width = 60;
            column.Visible = true;
            break;
          case "Remove":
            column.HeaderText = string.Empty;
            column.ReadOnly = true;
            column.Width = 20;
            column.Visible = true;
            break;
          case "Version":
            column.HeaderText = "Version";
            column.ReadOnly = true;
            column.Width = 105;
            column.Visible = true;
            break;
        }
      }
    }

    private void UpdateDataGridViewExport1Rows()
    {
      while (this.dataGridViewExport1.Rows.Count > 0)
        this.dataGridViewExport1.Rows.RemoveAt(0);
      ExportData data;
      new PdmsInterface().GetExportSettings(this._mappingExport.ProjectPath.FullName, out this._listOfExportModels, out data);
      foreach (ExportToTeklaModel listOfExportModel in this._listOfExportModels)
        this.AddGridRow(listOfExportModel);
    }

    private void DataGridViewExport1CellClick(object sender, DataGridViewCellEventArgs e)
    {
      DataGridViewColumn column1 = this.dataGridViewExport1.Columns["Browse"];
      if (column1 != null && e.ColumnIndex == column1.Index)
      {
        if (string.IsNullOrEmpty(this.BrowseFolder("Select folder")))
          ;
      }
      else
      {
        DataGridViewColumn column2 = this.dataGridViewExport1.Columns["Remove"];
        if (column2 != null && e.ColumnIndex == column2.Index)
        {
          if (MessageBox.Show("Are you sure you want to remove selected?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
            return;
          string selectedModel = this.dataGridViewExport1.Rows[e.RowIndex].Cells["Content"].Value.ToString();
          for (int index = 0; index < this._listOfExportModels.Count; ++index)
          {
            if (!(this._listOfExportModels[index].Name != selectedModel))
            {
              PdmsInterface pdmsInterface = new PdmsInterface();
              ExportToTeklaModel listOfExportModel = this._listOfExportModels[index];
              pdmsInterface.RemoveExportInstance(this._mappingExport.ProjectPath.FullName, selectedModel);
              this._listOfExportModels.Remove(this._listOfExportModels[index]);
              this.UpdateDataGridViewExport1Rows();
              this.dataGridViewExport1.Refresh();
              break;
            }
          }
        }
        else
        {
          DataGridViewColumn column3 = this.dataGridViewExport1.Columns["Add"];
          if (column3 != null && e.ColumnIndex == column3.Index)
            this.OpenRootFolderExport(this.dataGridViewExport1.Rows[e.RowIndex].Cells["Folder"].Value.ToString());
        }
      }
    }

    private void InitializeDataGridViewExport2()
    {
      DataGridViewComboBoxColumn viewComboBoxColumn = new DataGridViewComboBoxColumn();
      viewComboBoxColumn.DataSource = (object) this._struList;
      viewComboBoxColumn.Name = "Name";
      viewComboBoxColumn.SortMode = DataGridViewColumnSortMode.Automatic;
      viewComboBoxColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
      viewComboBoxColumn.Resizable = DataGridViewTriState.False;
      viewComboBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewComboBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn1 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn1.Name = "Content";
      viewTextBoxColumn1.Resizable = DataGridViewTriState.False;
      viewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewTextBoxColumn1);
      DataGridViewCheckBoxColumn viewCheckBoxColumn = new DataGridViewCheckBoxColumn();
      viewCheckBoxColumn.Name = "Negatives";
      viewCheckBoxColumn.Resizable = DataGridViewTriState.False;
      viewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewCheckBoxColumn);
      DataGridViewTextBoxColumn viewTextBoxColumn2 = new DataGridViewTextBoxColumn();
      viewTextBoxColumn2.Name = "Folder";
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewTextBoxColumn2);
      DataGridViewButtonColumn viewButtonColumn1 = new DataGridViewButtonColumn();
      viewButtonColumn1.Name = "Browse";
      viewButtonColumn1.Resizable = DataGridViewTriState.False;
      viewButtonColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewButtonColumn1);
      DataGridViewButtonColumn viewButtonColumn2 = new DataGridViewButtonColumn();
      viewButtonColumn2.Name = "Add";
      viewButtonColumn2.Resizable = DataGridViewTriState.False;
      viewButtonColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.dataGridViewExport2.Columns.Add((DataGridViewColumn) viewButtonColumn2);
      this.DataGridViewExport2ColumnSizes();
      int index = this.dataGridViewExport2.Rows.Add();
      this.dataGridViewExport2.RowCount = 1;
      this.dataGridViewExport2.Rows[index].Cells["Name"].Value = (object) this._struList[0].ToString();
      this.dataGridViewExport2.Rows[index].Cells["Content"].Value = (object) string.Empty;
      this.dataGridViewExport2.Rows[index].Cells["Folder"].Value = (object) this._mappingExport.ProjectPath;
      this.dataGridViewExport2.Rows[index].Cells["Negatives"].Value = (object) true;
      this.dataGridViewExport2.Rows[index].Cells["Browse"].Value = (object) "...";
      this.dataGridViewExport2.Rows[index].Cells["Browse"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridViewExport2.Rows[index].Cells["Add"].Value = (object) "Add";
      this.dataGridViewExport2.Refresh();
      this.dataGridViewExport2.FirstDisplayedCell = (DataGridViewCell) null;
      this.dataGridViewExport2.ClearSelection();
    }

    private void DataGridViewExport2ColumnSizes()
    {
      foreach (DataGridViewColumn column in (BaseCollection) this.dataGridViewExport2.Columns)
      {
        string name = column.Name;
        if (!(name == "Name"))
        {
          if (!(name == "Content"))
          {
            if (!(name == "Negatives"))
            {
              if (!(name == "Folder"))
              {
                if (!(name == "Browse"))
                {
                  if (name == "Add")
                  {
                    column.ReadOnly = true;
                    column.Width = 60;
                  }
                }
                else
                {
                  column.ReadOnly = true;
                  column.Width = 20;
                }
              }
              else
                column.Width = 80;
            }
            else
              column.Width = 60;
          }
          else
            column.Width = 140;
        }
        else
          column.Width = 190;
      }
    }

    private void DataGridViewExport2CellClick(object sender, DataGridViewCellEventArgs e)
    {
      DataGridViewColumn column1 = this.dataGridViewExport2.Columns["Browse"];
      if (column1 != null && e.ColumnIndex == column1.Index)
      {
        string str = this.BrowseFolder("Select folder");
        if (string.IsNullOrEmpty(str))
          return;
        this.dataGridViewExport2.Rows[0].Cells["Folder"].Value = (object) str;
      }
      else
      {
        DataGridViewColumn column2 = this.dataGridViewExport2.Columns["Add"];
        if (column2 != null && e.ColumnIndex == column2.Index)
          this.AddExport();
      }
    }

    private void AddGridRow(ExportToTeklaModel exportToTeklaModel)
    {
      int index = this.dataGridViewExport1.Rows.Add();
      this.dataGridViewExport1.Rows[index].Tag = (object) exportToTeklaModel;
      this.dataGridViewExport1.Rows[index].Cells["Version"].Value = (object) exportToTeklaModel.DateTimeUtc;
      this.dataGridViewExport1.Rows[index].Cells["Name"].Value = (object) exportToTeklaModel.Name;
      this.dataGridViewExport1.Rows[index].Cells["Content"].Value = (object) exportToTeklaModel.HierarchyName;
      this.dataGridViewExport1.Rows[index].Cells["Negatives"].Value = (object) exportToTeklaModel.Negatives;
      this.dataGridViewExport1.Rows[index].Cells["Negatives"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridViewExport1.Rows[index].Cells["Folder"].Value = (object) exportToTeklaModel.FolderAbsolute;
      this.dataGridViewExport1.Rows[index].Cells["Browse"].Value = (object) "...";
      this.dataGridViewExport1.Rows[index].Cells["Browse"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridViewExport1.Rows[index].Cells["Remove"].Value = TS_E3D.Properties.Resources.ResourceManager.GetObject("Remove_16");
      this.dataGridViewExport1.Rows[index].Cells["Remove"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      this.dataGridViewExport1.Rows[index].Cells["Add"].Value = TS_E3D.Properties.Resources.ResourceManager.GetObject("Open_16");
      this.dataGridViewExport1.Rows[index].Cells["Add"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      if (string.IsNullOrEmpty(exportToTeklaModel.DateTimeUtc))
        return;
      this.dataGridViewExport1.Rows[index].Cells["Version"].Value = (object) exportToTeklaModel.DateTimeUtc;
    }

    private void ButtonMappingExportClick(object sender, EventArgs e)
    {
      Form form;
      if (TS_ModelConverter.Tools.CheckOpened("Tekla Interoperability Export Mapping", out form))
      {
        form.WindowState = FormWindowState.Normal;
        form.Dock = DockStyle.Fill;
        int num = (int) form.ShowDialog();
        form.Focus();
      }
      else
      {
        MappingDialog mappingDialog = new MappingDialog(this, this._mappingExport);
        mappingDialog.Name = "MappingDialog";
        mappingDialog.Text = "Tekla Interoperability Export Mapping";
        mappingDialog.groupBox1.Enabled = false;
        mappingDialog.groupBox2.Enabled = false;
        int num = (int) mappingDialog.ShowDialog();
      }
    }

    private void buttonRefresh_Click(object sender, EventArgs e)
    {
      this.UpdateControl();
    }

    private void OpenRootFolderExport(string folderLocation)
    {
      if (folderLocation.StartsWith("."))
        folderLocation = Path.Combine(this._mappingExport.ProjectPath.FullName, folderLocation);
      if (!Directory.Exists(folderLocation))
        return;
      Process.Start(folderLocation);
    }

    private string BrowseFolder(string description)
    {
      string str = string.Empty;
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
      {
        Description = description,
        SelectedPath = TS_ModelConverter.Tools.GetProjectPath().FullName
      };
      int num = (int) folderBrowserDialog.ShowDialog();
      if (folderBrowserDialog.SelectedPath != string.Empty)
        str = folderBrowserDialog.SelectedPath;
      return str;
    }

    private bool CheckDatainformation(
      DataInformation dataInformation,
      int version,
      out string message)
    {
      message = string.Empty;
      if (!dataInformation.ExportSoftware.Equals((object) SoftwareOptions.TeklaStructures))
      {
        message = "Package not exported from Tekla Structures.";
        return false;
      }
      if (!dataInformation.ImportSoftware.Equals((object) SoftwareOptions.PDMS))
      {
        message = "Package must be imported to PDMS.";
        return false;
      }
      if (dataInformation.LinkVersion.Equals(version))
        return true;
      message = "Incorrect version, update and do a new export.";
      return false;
    }

    private void LoadLoggerConfig(string logFile)
    {
      log4net.Repository.Hierarchy.Hierarchy repository = (log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository();
      repository.Root.RemoveAllAppenders();
      PatternLayout patternLayout = new PatternLayout()
      {
        ConversionPattern = "%level [%method] - %message%newline"
      };
      patternLayout.ActivateOptions();
      FileAppender fileAppender1 = new FileAppender();
      fileAppender1.File = logFile;
      fileAppender1.Layout = (ILayout) patternLayout;
      fileAppender1.ImmediateFlush = true;
      fileAppender1.LockingModel = (FileAppender.LockingModelBase) new FileAppender.MinimalLock();
      fileAppender1.AppendToFile = false;
      FileAppender fileAppender2 = fileAppender1;
      repository.Root.AddAppender((IAppender) fileAppender2);
      fileAppender2.ActivateOptions();
      BasicConfigurator.Configure((IAppender) fileAppender2);
      repository.Root.Level = Level.All;
    }

    private void ImportRow(ImportToPdmsModel importToPdmsModel, PdmsInterface pd, bool isBatch)
    {
      Errors errors;
      ProjectSettings projectSettings = pd.GetProjectSettings(this._mappingImport.ProjectPath.FullName, out errors);
      this._mappingImport.UpdateAvevaObjects = projectSettings.UpdateAvevaCreatedObjects;
      this._mappingImport.UpdateAvevaHierarchy = projectSettings.UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy;
      this._mappingImport.RecreateDeletedObjects = projectSettings.CheckAndRecreateAvevaModelDeletedObjects;
      this._mappingImport.UpdateUnchangedObjects = projectSettings.UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures;
      this._mappingImport.DeleteEmptyContainerElementsAfterImport = projectSettings.DeleteEmptyContainerElementsAfterImport;
      this._mappingImport.KeepDeletedObjects = projectSettings.KeepDeletedObjectsInDELETED_FRMW;
      this._mappingImport.AvevaSiteCanContainOneTeklaModelOnly = projectSettings.AvevaSiteCanContainOneTeklaModelOnly;
      this._mappingImport.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly = projectSettings.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly;
      ImportData importData;
      pd.GetImportModelInformation(this._mappingImport.ProjectPath.FullName, importToPdmsModel, out importData);
      this._mappingImport.UpdateAvevaHierarchyFile = importData.DataInformation.Hierarchy;
      if (this._mappingImport.UpdateAvevaHierarchy)
        this._mappingImport.UpdateAvevaHierarchy = importData.DataInformation.Hierarchy;
      this._mappingImport.CreateNegatives = importToPdmsModel.Negatives;
      this.LoadLoggerConfig(Path.Combine(importToPdmsModel.Location, importData.DataInformation.NameFull + ".log"));
      TS_E3DControl.Log.Info((object) "Version: Tekla Interoperability [2.1.13.1]");
      TS_E3DControl.Log.Info((object) ("Import log file: " + Path.Combine(importToPdmsModel.Location, importData.DataInformation.NameFull + ".log")));
      TS_E3DControl.Log.Info((object) ("Import started: " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString()));
      TS_E3DControl.Log.Info((object) "Settings from file:");
      TS_E3DControl.Log.Info((object) ("UpdateAvevaObjects: " + this._mappingImport.UpdateAvevaObjects.ToString()));
      TS_E3DControl.Log.Info((object) ("UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy: " + this._mappingImport.UpdateAvevaHierarchy.ToString()));
      TS_E3DControl.Log.Info((object) ("CheckAndRecreateAvevaModelDeletedObjects: " + this._mappingImport.RecreateDeletedObjects.ToString()));
      TS_E3DControl.Log.Info((object) ("UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures: " + this._mappingImport.UpdateUnchangedObjects.ToString()));
      TS_E3DControl.Log.Info((object) ("DeleteEmptyContainerElementsAfterImport: " + this._mappingImport.DeleteEmptyContainerElementsAfterImport.ToString()));
      TS_E3DControl.Log.Info((object) ("KeepDeletedObjectsInDELETED_FRMW: " + this._mappingImport.KeepDeletedObjects.ToString()));
      TS_E3DControl.Log.Info((object) ("AvevaSiteCanContainOneTeklaModelOnly: " + this._mappingImport.AvevaSiteCanContainOneTeklaModelOnly.ToString()));
      TS_E3DControl.Log.Info((object) ("TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly: " + this._mappingImport.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly.ToString()));
      if (string.IsNullOrEmpty(importToPdmsModel.NewDateTimeUtc))
      {
        if (!isBatch)
        {
          int num = (int) MessageBox.Show("Model: " + importToPdmsModel.Name + ". Nothing new to import, aborting...");
        }
        TS_E3DControl.Log.Info((object) ("Model: " + importToPdmsModel.Name + ". Nothing new to import, aborting..."));
      }
      else if (CurrentElement.get_Element().get_Db().get_IsReadOnly())
      {
        if (!isBatch)
        {
          int num = (int) MessageBox.Show("Database is ReadOnly, aborting.");
        }
        TS_E3DControl.Log.Info((object) "Database is ReadOnly, aborting.");
      }
      else
      {
        List<DbElement> dbElementList = new List<DbElement>();
        dbElementList.Add(DbElement.GetElement("WORLD"));
        List<DbElement> curList = dbElementList;
        if (!importData.DataInformation.Hierarchy && this._mappingImport.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly)
        {
          curList.Clear();
          DbElement dbElement = DbElement.GetElement(importToPdmsModel.Stru.StartsWith("/") ? importToPdmsModel.Stru : "/" + importToPdmsModel.Stru);
          if (dbElement.get_IsNull())
            dbElement = CurrentElement.get_Element();
          while (!((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SITE))
            dbElement = dbElement.get_Owner();
          curList.Add(dbElement);
        }
        if (importData.DataInformation.Hierarchy)
        {
          List<string> claimList;
          List<string> hierarchyNames;
          this._mappingImport.SiteList = TS_ModelConverter.Tools.GetSites(importData.NewImportVersionFile, this._mappingImport, out claimList, out hierarchyNames);
          if (this._mappingImport.SiteList == null)
          {
            if (!isBatch)
            {
              int num = (int) MessageBox.Show("Error collecting SITE's, aborting.");
            }
            TS_E3DControl.Log.Info((object) "Error collecting SITE's, aborting.");
            return;
          }
          if (this._mappingImport.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly)
          {
            curList.Clear();
            curList.AddRange(this._mappingImport.SiteList.Select<string, DbElement>((Func<string, DbElement>) (site => DbElement.GetElement(site.StartsWith("/") ? site : "/" + site))));
          }
          if (this._mappingImport.AvevaSiteCanContainOneTeklaModelOnly)
          {
            if (this._mappingImport.SiteList.Count > 1)
            {
              TS_E3DControl.Log.Warn((object) "Aborted due to several SITE's in file but AvevaSiteCanContainOneTeklaModelOnly set to true.");
              if (isBatch)
                return;
              int num = (int) MessageBox.Show("Aborted due to several SITE's in file but AvevaSiteCanContainOneTeklaModelOnly set to true.");
              return;
            }
            this._mappingImport.SiteElementList = TS_ModelConverter.Tools.CollectExistingItems(DbElement.GetElement("/" + this._mappingImport.SiteList[0]));
          }
          List<string> wrongNames;
          if (!TS_ModelConverter.Tools.CheckHierarchyNames((IEnumerable<string>) hierarchyNames, out wrongNames))
          {
            TS_E3DControl.Log.Warn((object) "Aborted due to problematic hierarchy names.");
            foreach (string str in wrongNames)
              TS_E3DControl.Log.Error((object) ("    Error in name: " + str));
            if (isBatch)
              return;
            int num = (int) MessageBox.Show("Aborted due to problematic hierarchy names, see logfile");
            return;
          }
          if (claimList != null && claimList.Count > 0)
          {
            TS_E3DControl.Log.Warn((object) "Aborted due to claimed/locked elements.");
            foreach (string str in claimList)
              TS_E3DControl.Log.Warn((object) ("    Claimed/locked: " + str));
            if (isBatch)
              return;
            int num = (int) MessageBox.Show("Aborted due to claimed/locked elements, see logfile");
            return;
          }
        }
        TS_E3DControl.Log.Info((object) ("Collect comparable elements from model, starting: " + DateTime.Now.ToLongTimeString()));
        List<string> duplicates;
        this._mappingImport.CollectExistingItems(curList, out duplicates);
        TS_E3DControl.Log.Info((object) ("Collect comparable elements from model, ending: " + DateTime.Now.ToLongTimeString()));
        if (duplicates.Count > 0)
        {
          TS_E3DControl.Log.Warn((object) "Aborted due to duplicate IFC GUIDS in model:");
          foreach (string str in duplicates)
            TS_E3DControl.Log.Warn((object) str);
          if (isBatch)
            return;
          int num = (int) MessageBox.Show("Aborted due to duplicate IFC GUIDS in model, see log file.");
        }
        else if (!importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.None))
        {
          TS_E3DControl.Log.Error((object) ("Error reading package. " + importData.Errors.ErrorInfo + "\nAborting."));
          if (isBatch)
            return;
          int num = (int) MessageBox.Show("Error reading package. " + importData.Errors.ErrorInfo + "\nAborting.");
        }
        else
        {
          DataSet dataSet = new DataSet();
          if (!importToPdmsModel.DateTimeUtc.Equals(string.Empty) && !this._mappingImport.AvevaSiteCanContainOneTeklaModelOnly)
          {
            pd.UpdateImportToPdmsData(importToPdmsModel, this._mappingImport.ProjectPath.FullName, this._mappingImport.MappingUda.FullName, out importData, out dataSet, importData.DataInformation.Hierarchy);
            if (importData.Errors.ErrorCode.Equals((object) IfcModelCollaboration.ErrorCode.DataMissing))
            {
              TS_E3DControl.Log.Error((object) ("Error reading package. " + importData.Errors.ErrorInfo + "\nAborting."));
              if (isBatch)
                return;
              int num = (int) MessageBox.Show("Error : " + importData.Errors.ErrorInfo + "\nAborting.");
              return;
            }
          }
          this._mappingImport.ModificationSet = dataSet;
          if (importData.DataInformation.Hierarchy)
          {
            string str1 = this._mappingImport.SiteList.Count.Equals(1) ? this._mappingImport.SiteList[0] : "TEKLA-SITE";
            string str2 = this._mappingImport.SiteList.Count.Equals(1) ? this._mappingImport.SiteList[0] + "/TEKLA-ZONE" : "TEKLA-ZONE";
            try
            {
              TS_ModelConverter.Tools.GetHierarchyObject("FRMW", "/" + str1, "/" + str2, "/" + importToPdmsModel.Frmw.ToUpper() + "-FILE", "/" + importToPdmsModel.Frmw.ToUpper() + "-DELETED", string.Empty);
              TS_ModelConverter.Tools.GetHierarchyObject("FRMW", "/" + str1, "/" + str2, "/" + importToPdmsModel.Frmw.ToUpper() + "-FILE", "/" + importToPdmsModel.Frmw.ToUpper() + "-UNPLACED", string.Empty);
            }
            catch (Exception ex)
            {
              int num = (int) MessageBox.Show("Error : " + ex.Message + "\nAborting.");
              return;
            }
            this._mappingImport.DeletedElements = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + importToPdmsModel.Frmw.ToUpper() + "-DELETED");
            this._mappingImport.UnplacedElements = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + importToPdmsModel.Frmw.ToUpper() + "-UNPLACED");
            try
            {
              CurrentElement.set_Element(DbElement.GetElement("/" + str1));
            }
            catch (Exception ex)
            {
            }
          }
          if (!string.IsNullOrEmpty(importToPdmsModel.Stru))
          {
            DbElement element = CurrentElement.get_Element();
            CurrentElement.set_Element(DbElement.GetElement(importToPdmsModel.Stru.StartsWith("/") ? importToPdmsModel.Stru : "/" + importToPdmsModel.Stru));
            if (CurrentElement.get_Element().get_IsNull())
              CurrentElement.set_Element(element);
          }
          DbElement cur = CurrentElement.get_Element();
          if (!importData.DataInformation.Hierarchy)
          {
            if (!((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.STRUCTURE) && !((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.FRMWORK))
            {
              TS_E3DControl.Log.Warn((object) "Selected hierarchy item is not a STRU/FRMW, aborting (Hierarchy update is false).");
              if (isBatch)
                return;
              int num = (int) MessageBox.Show("Selected hierarchy item is not a STRU/FRMW, aborting.");
              return;
            }
            DbElement element;
            if (TS_ModelConverter.Tools.ExistsInModel(importToPdmsModel.Frmw, out element))
              cur = element;
            else if (((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.STRUCTURE))
            {
              element = cur.CreateLast((DbElementType) DbElementTypeInstance.FRMWORK);
              element.SetAttribute((DbAttribute) DbAttributeInstance.NAME, "/" + importToPdmsModel.Frmw);
              cur = element;
            }
            else if (((object) CurrentElement.get_Element().GetElementType()).Equals((object) DbElementTypeInstance.FRMWORK))
            {
              element = cur.CreateLast((DbElementType) DbElementTypeInstance.SBFRAMEWORK);
              element.SetAttribute((DbAttribute) DbAttributeInstance.NAME, "/" + importToPdmsModel.Frmw);
              cur = element;
            }
          }
          if (!new HierarchyItem(this._mappingImport, importData.NewImportVersionFile, importToPdmsModel.NewDateTimeUtc, this.progressBar1).CreatePdmsStructure(importToPdmsModel, cur))
            return;
          importToPdmsModel.DateTimeUtc = importToPdmsModel.NewDateTimeUtc;
          importToPdmsModel.NewDateTimeUtc = string.Empty;
          pd.StoreImportModelToPdmsData(this._mappingImport.ProjectPath.FullName, importToPdmsModel, ref importData);
          TS_E3DControl.Log.Info((object) ("Import complete: " + DateTime.Now.ToLongTimeString()));
          if (isBatch)
            return;
          this.label1.Text = "Import complete, check log for details";
          this.dataGridView1.FirstDisplayedCell = (DataGridViewCell) null;
          this.dataGridView1.ClearSelection();
        }
      }
    }

    private void ExportRow(ExportToTeklaModel exportVersionData, bool isBatch)
    {
      try
      {
        PdmsInterface pdmsInterface = new PdmsInterface();
        IfcModelCollaboration.ErrorCode errorCode;
        List<string> errorList;
        string tobezipped;
        ExportData exportData;
        if (!pdmsInterface.ExportModelToTekla(this._mappingExport.ProjectPath.FullName, ref exportVersionData, out errorCode, out errorList, out tobezipped, out exportData, false))
        {
          TS_E3DControl.Log.Error((object) ("Error ExportModelToTekla: " + errorCode.ToString()));
          if (isBatch)
            return;
          int num = (int) MessageBox.Show("Error ExportModelToTekla: " + errorCode.ToString());
        }
        else
        {
          this._mappingExport.CreateNegatives = exportVersionData.Negatives;
          this.LoadLoggerConfig(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".log"));
          TS_E3DControl.Log.Info((object) "Version: Tekla Interoperability [2.1.13.1]");
          TS_E3DControl.Log.Info((object) ("Export started, " + (object) DateTime.Now.ToUniversalTime()));
          TS_E3DControl.Log.Info((object) ("Export TCZIP file: " + Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".TCZIP")));
          TS_E3DControl.Log.Info((object) ("Export log file    : " + Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".log")));
          FileInfo ifcFile = new FileInfo(Path.Combine(tobezipped, exportVersionData.NameFull + ".ifc"));
          char[] chArray = new char[1]{ '/' };
          CurrentElement.set_Element(DbElement.GetElement(new int[2]
          {
            int.Parse(exportVersionData.HierarchyId.Split(chArray)[0]),
            int.Parse(exportVersionData.HierarchyId.Split(chArray)[1])
          }));
          HierarchyItem hierarchyItem = new HierarchyItem(this._mappingExport, ifcFile, string.Empty, this.progressBar2);
          if (exportData != null)
          {
            if (!pdmsInterface.ExportModelToTeklaNext(ref exportVersionData, tobezipped, ref exportData))
            {
              int num = (int) MessageBox.Show("Error ExportModelToTeklaNext: " + exportData.Errors.ErrorInfo + " (" + (object) exportData.Errors.ErrorCode + ").");
              TS_E3DControl.Log.Error((object) ("Error ExportModelToTeklaNext: " + exportData.Errors.ErrorInfo + " (" + (object) exportData.Errors.ErrorCode + ")."));
              return;
            }
            FileInfo fileInfo1 = new FileInfo(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".tcZip"));
            if (fileInfo1.Exists)
            {
              FileInfo fileInfo2 = new FileInfo(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.Name + ".tcZip"));
              if (fileInfo2.Exists)
                fileInfo2.Delete();
              fileInfo1.CopyTo(fileInfo2.FullName);
            }
          }
          this.dataGridViewExport1.FirstDisplayedCell = (DataGridViewCell) null;
          this.dataGridViewExport1.ClearSelection();
          TS_E3DControl.Log.Info((object) ("Export complete, " + (object) DateTime.Now.ToUniversalTime()));
          this.label3.Text = "Export complete, check log for details";
        }
      }
      catch (Exception ex)
      {
        TS_E3DControl.Log.Error((object) ex.Message);
        if (isBatch)
          return;
        int num = (int) MessageBox.Show("Warning: " + ex.Message);
      }
    }

    public void ExportRow(DirectoryInfo path, ExportToTeklaModel exportVersionData)
    {
      try
      {
        PdmsInterface pdmsInterface = new PdmsInterface();
        this._mappingExport = new Mapping(path, TS_ModelConverter.Constants.System.PDMS);
        IfcModelCollaboration.ErrorCode errorCode;
        List<string> errorList;
        string tobezipped;
        ExportData exportData;
        if (!pdmsInterface.ExportModelToTekla(this._mappingExport.ProjectPath.FullName, ref exportVersionData, out errorCode, out errorList, out tobezipped, out exportData, false))
        {
          TS_E3DControl.Log.Error((object) ("Error ExportModelToTekla: " + errorCode.ToString()));
        }
        else
        {
          this._mappingExport.CreateNegatives = exportVersionData.Negatives;
          this.LoadLoggerConfig(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".log"));
          TS_E3DControl.Log.Info((object) "Version: Tekla Interoperability [2.1.13.1]");
          ILog log1 = TS_E3DControl.Log;
          DateTime now = DateTime.Now;
          string str1 = "Export started, " + (object) now.ToUniversalTime();
          log1.Info((object) str1);
          TS_E3DControl.Log.Info((object) ("Export TCZIP file: " + Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".TCZIP")));
          TS_E3DControl.Log.Info((object) ("Export log file    : " + Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".log")));
          FileInfo ifcFile = new FileInfo(Path.Combine(tobezipped, exportVersionData.NameFull + ".ifc"));
          char[] chArray = new char[1]{ '/' };
          CurrentElement.set_Element(DbElement.GetElement(new int[2]
          {
            int.Parse(exportVersionData.HierarchyId.Split(chArray)[0]),
            int.Parse(exportVersionData.HierarchyId.Split(chArray)[1])
          }));
          HierarchyItem hierarchyItem = new HierarchyItem(this._mappingExport, ifcFile, string.Empty, this.progressBar2);
          if (exportData != null)
          {
            if (!pdmsInterface.ExportModelToTeklaNext(ref exportVersionData, tobezipped, ref exportData))
            {
              int num = (int) MessageBox.Show("Error ExportModelToTeklaNext: " + exportData.Errors.ErrorInfo + " (" + (object) exportData.Errors.ErrorCode + ").");
              TS_E3DControl.Log.Error((object) ("Error ExportModelToTeklaNext: " + exportData.Errors.ErrorInfo + " (" + (object) exportData.Errors.ErrorCode + ")."));
              return;
            }
            FileInfo fileInfo1 = new FileInfo(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.NameFull + ".tcZip"));
            if (fileInfo1.Exists)
            {
              FileInfo fileInfo2 = new FileInfo(Path.Combine(exportVersionData.FolderAbsolute, exportVersionData.Name + ".tcZip"));
              if (fileInfo2.Exists)
                fileInfo2.Delete();
              fileInfo1.CopyTo(fileInfo2.FullName);
            }
          }
          ILog log2 = TS_E3DControl.Log;
          now = DateTime.Now;
          string str2 = "Export complete, " + (object) now.ToUniversalTime();
          log2.Info((object) str2);
        }
      }
      catch (Exception ex)
      {
        TS_E3DControl.Log.Error((object) ex.Message);
      }
    }

    private void buttonChangeViewer_Click(object sender, EventArgs e)
    {
      PdmsInterface pdmsInterface = new PdmsInterface();
      List<int> list = this.dataGridView1.SelectedRows.Cast<DataGridViewRow>().Select<DataGridViewRow, int>((Func<DataGridViewRow, int>) (row1 => row1.Index)).ToList<int>();
      if (!list.Count.Equals(1))
      {
        int num = (int) MessageBox.Show("Select only one row for comparison.");
      }
      foreach (int index in list)
      {
        DataGridViewRow row = this.dataGridView1.Rows[index];
        string name = this.dataGridView1.Rows[index].Cells["Content"].Value.ToString();
        string modelsFolder = this.dataGridView1.Rows[index].Cells["Folder"].Value.ToString();
        string fullName = this._mappingImport.MappingUda.FullName;
        Errors errors = new Errors();
        this.LaunchChangeViewer(modelsFolder, name, fullName, out errors, "?", "?");
      }
    }

    public bool LaunchChangeViewer(
      string modelsFolder,
      string name,
      string attributesFilterFileFullPath,
      out Errors errors,
      string newer = "?",
      string older = "?")
    {
      errors = new Errors();
      FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (fileInfo.Directory == null)
        return false;
      string str1 = fileInfo.Directory.FullName + "\\AvevaCompare\\CompareIfcModels\\TeklaStructuresInteroperability.exe";
      Process process = new Process();
      process.StartInfo.FileName = str1;
      string str2 = "\"" + modelsFolder + "\" \"" + name + "\" \"" + attributesFilterFileFullPath + "\"";
      ProcessStartInfo startInfo = process.StartInfo;
      string str3;
      if (!(newer == "?"))
        str3 = str2 + " \"" + newer + "\" \"" + older + "\"";
      else
        str3 = str2;
      startInfo.Arguments = str3;
      try
      {
        process.Start();
        return true;
      }
      catch (Exception ex)
      {
        TS_E3DControl.Log.Error((object) "Launching of Change viewer", ex);
        return false;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.Import = new System.Windows.Forms.TabPage();
            this.buttonChangeViewer = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonMapping = new System.Windows.Forms.Button();
            this.createModel = new System.Windows.Forms.Button();
            this.dataGridViewExport2 = new System.Windows.Forms.DataGridView();
            this.dataGridViewExport1 = new System.Windows.Forms.DataGridView();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.buttonMapping1 = new System.Windows.Forms.Button();
            this.exportModel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBoxExport1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Export = new System.Windows.Forms.TabPage();
            this.Import.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExport2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExport1)).BeginInit();
            this.groupBoxExport1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.Export.SuspendLayout();
            this.SuspendLayout();
            // 
            // Import
            // 
            this.Import.BackColor = System.Drawing.SystemColors.Control;
            this.Import.Controls.Add(this.buttonChangeViewer);
            this.Import.Controls.Add(this.buttonRefresh);
            this.Import.Controls.Add(this.label1);
            this.Import.Controls.Add(this.groupBox1);
            this.Import.Controls.Add(this.progressBar1);
            this.Import.Controls.Add(this.buttonMapping);
            this.Import.Controls.Add(this.createModel);
            this.Import.Location = new System.Drawing.Point(4, 22);
            this.Import.Name = "Import";
            this.Import.Padding = new System.Windows.Forms.Padding(3);
            this.Import.Size = new System.Drawing.Size(652, 288);
            this.Import.TabIndex = 0;
            this.Import.Text = "Import";
            // 
            // buttonChangeViewer
            // 
            this.buttonChangeViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonChangeViewer.Location = new System.Drawing.Point(262, 263);
            this.buttonChangeViewer.Name = "buttonChangeViewer";
            this.buttonChangeViewer.Size = new System.Drawing.Size(106, 21);
            this.buttonChangeViewer.TabIndex = 19;
            this.buttonChangeViewer.Text = "Change viewer...";
            this.buttonChangeViewer.UseVisualStyleBackColor = true;
            this.buttonChangeViewer.Click += new System.EventHandler(this.buttonChangeViewer_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefresh.Location = new System.Drawing.Point(170, 263);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(86, 21);
            this.buttonRefresh.TabIndex = 18;
            this.buttonRefresh.Text = "Refresh dialog";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(416, 266);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(229, 16);
            this.label1.TabIndex = 17;
            this.label1.Text = "Import complete, check log for details";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.dataGridView2);
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(639, 225);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(20, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "New:";
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.AllowUserToDeleteRows = false;
            this.dataGridView2.AllowUserToResizeRows = false;
            this.dataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView2.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.ColumnHeadersVisible = false;
            this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView2.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView2.Location = new System.Drawing.Point(110, 11);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView2.RowHeadersVisible = false;
            this.dataGridView2.Size = new System.Drawing.Size(523, 25);
            this.dataGridView2.TabIndex = 0;
            this.dataGridView2.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView2CellClick);
            this.dataGridView2.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView2DataError);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView1.Location = new System.Drawing.Point(7, 42);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(628, 175);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BackColor = System.Drawing.SystemColors.Control;
            this.progressBar1.Location = new System.Drawing.Point(6, 236);
            this.progressBar1.Maximum = 1000000;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(639, 21);
            this.progressBar1.TabIndex = 15;
            // 
            // buttonMapping
            // 
            this.buttonMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonMapping.AutoSize = true;
            this.buttonMapping.Location = new System.Drawing.Point(99, 262);
            this.buttonMapping.Name = "buttonMapping";
            this.buttonMapping.Size = new System.Drawing.Size(65, 22);
            this.buttonMapping.TabIndex = 13;
            this.buttonMapping.Text = "Mapping";
            this.buttonMapping.UseVisualStyleBackColor = true;
            this.buttonMapping.Click += new System.EventHandler(this.ButtonMappingImportClick);
            // 
            // createModel
            // 
            this.createModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.createModel.AutoSize = true;
            this.createModel.Location = new System.Drawing.Point(6, 262);
            this.createModel.Name = "createModel";
            this.createModel.Size = new System.Drawing.Size(89, 22);
            this.createModel.TabIndex = 12;
            this.createModel.Text = "Import-Update";
            this.createModel.UseVisualStyleBackColor = true;
            this.createModel.Click += new System.EventHandler(this.CreateModelClick);
            // 
            // dataGridViewExport2
            // 
            this.dataGridViewExport2.AllowUserToAddRows = false;
            this.dataGridViewExport2.AllowUserToDeleteRows = false;
            this.dataGridViewExport2.AllowUserToResizeRows = false;
            this.dataGridViewExport2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewExport2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewExport2.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewExport2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewExport2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewExport2.ColumnHeadersVisible = false;
            this.dataGridViewExport2.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport2.Location = new System.Drawing.Point(110, 11);
            this.dataGridViewExport2.Name = "dataGridViewExport2";
            this.dataGridViewExport2.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport2.RowHeadersVisible = false;
            this.dataGridViewExport2.Size = new System.Drawing.Size(524, 25);
            this.dataGridViewExport2.TabIndex = 0;
            this.dataGridViewExport2.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewExport2CellClick);
            this.dataGridViewExport2.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView2DataError);
            // 
            // dataGridViewExport1
            // 
            this.dataGridViewExport1.AllowUserToAddRows = false;
            this.dataGridViewExport1.AllowUserToDeleteRows = false;
            this.dataGridViewExport1.AllowUserToResizeRows = false;
            this.dataGridViewExport1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewExport1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewExport1.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewExport1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewExport1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewExport1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport1.Location = new System.Drawing.Point(6, 42);
            this.dataGridViewExport1.Name = "dataGridViewExport1";
            this.dataGridViewExport1.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewExport1.RowHeadersVisible = false;
            this.dataGridViewExport1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewExport1.Size = new System.Drawing.Size(628, 175);
            this.dataGridViewExport1.TabIndex = 0;
            this.dataGridViewExport1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewExport1CellClick);
            // 
            // progressBar2
            // 
            this.progressBar2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar2.Location = new System.Drawing.Point(6, 236);
            this.progressBar2.Maximum = 1000000;
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(640, 21);
            this.progressBar2.TabIndex = 15;
            // 
            // buttonMapping1
            // 
            this.buttonMapping1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonMapping1.AutoSize = true;
            this.buttonMapping1.Location = new System.Drawing.Point(133, 262);
            this.buttonMapping1.Name = "buttonMapping1";
            this.buttonMapping1.Size = new System.Drawing.Size(117, 22);
            this.buttonMapping1.TabIndex = 13;
            this.buttonMapping1.Text = "Mapping";
            this.buttonMapping1.UseVisualStyleBackColor = true;
            this.buttonMapping1.Click += new System.EventHandler(this.ButtonMappingExportClick);
            // 
            // exportModel
            // 
            this.exportModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exportModel.AutoSize = true;
            this.exportModel.Location = new System.Drawing.Point(6, 262);
            this.exportModel.Name = "exportModel";
            this.exportModel.Size = new System.Drawing.Size(121, 22);
            this.exportModel.TabIndex = 12;
            this.exportModel.Text = "Export";
            this.exportModel.UseVisualStyleBackColor = true;
            this.exportModel.Click += new System.EventHandler(this.ExportModelClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(20, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "New:";
            // 
            // groupBoxExport1
            // 
            this.groupBoxExport1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExport1.Controls.Add(this.label4);
            this.groupBoxExport1.Controls.Add(this.dataGridViewExport2);
            this.groupBoxExport1.Controls.Add(this.dataGridViewExport1);
            this.groupBoxExport1.Location = new System.Drawing.Point(6, 6);
            this.groupBoxExport1.Name = "groupBoxExport1";
            this.groupBoxExport1.Size = new System.Drawing.Size(640, 225);
            this.groupBoxExport1.TabIndex = 16;
            this.groupBoxExport1.TabStop = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(416, 266);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(230, 16);
            this.label3.TabIndex = 17;
            this.label3.Text = "Export complete, check log for details";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.Import);
            this.tabControl1.Controls.Add(this.Export);
            this.tabControl1.Location = new System.Drawing.Point(5, 5);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(660, 314);
            this.tabControl1.TabIndex = 45;
            // 
            // Export
            // 
            this.Export.BackColor = System.Drawing.SystemColors.Control;
            this.Export.Controls.Add(this.label3);
            this.Export.Controls.Add(this.groupBoxExport1);
            this.Export.Controls.Add(this.progressBar2);
            this.Export.Controls.Add(this.buttonMapping1);
            this.Export.Controls.Add(this.exportModel);
            this.Export.Location = new System.Drawing.Point(4, 22);
            this.Export.Name = "Export";
            this.Export.Padding = new System.Windows.Forms.Padding(3);
            this.Export.Size = new System.Drawing.Size(652, 288);
            this.Export.TabIndex = 0;
            this.Export.Text = "Export";
            // 
            // TS_E3DControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.MaximumSize = new System.Drawing.Size(1749, 1097);
            this.MinimumSize = new System.Drawing.Size(670, 323);
            this.Name = "TS_E3DControl";
            this.Size = new System.Drawing.Size(670, 323);
            this.Load += new System.EventHandler(this.TsPdmsDialogLoad);
            this.Import.ResumeLayout(false);
            this.Import.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExport2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExport1)).EndInit();
            this.groupBoxExport1.ResumeLayout(false);
            this.groupBoxExport1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.Export.ResumeLayout(false);
            this.Export.PerformLayout();
            this.ResumeLayout(false);

    }
  }
}
