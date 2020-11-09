// Decompiled with JetBrains decompiler
// Type: TS_ModelConverter.HierarchyItem
// Assembly: TS_ModelConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F81CDA0D-1216-40ED-828A-BAF333294439
// Assembly location: C:\TS_PDMS\12.0SP6\TS-PDMS_Library\TS_ModelConverter.dll

using Aveva.Pdms.Database;
using Aveva.PDMS.Database.Filters;
using Aveva.Pdms.Shared;
using Aveva.Pdms.Utilities.CommandLine;
using IfcModelCollaboration;
using IFCObjectsReader;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Tekla.Technology.IfcLib;

namespace TS_ModelConverter
{
  public class HierarchyItem
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private Dictionary<IFCObjectsReader.Data.IfcObject, DbElement> _connectedIfcList;
    private readonly string _version;
    private readonly IfcDocument _ifcDoc;
    private readonly string _unit;
    private readonly ProgressBar _progressBar;
    private readonly Mapping _mapping;
    private Dictionary<string, int> _sectionMapping;

    public HierarchyItem(
      Mapping mapping,
      string ifcFilePath,
      string version,
      ProgressBar progressBar)
    {
      this._sectionMapping = new Dictionary<string, int>();
      this._mapping = mapping;
      this._version = version;
      this._progressBar = progressBar;
      HierarchyItem.Log.Info((object) ("Windows user    : " + Environment.UserName));
      HierarchyItem.Log.Info((object) ("MAC Adress      : " + Tools.GetMacAddress()));
      HierarchyItem.Log.Info((object) ("Mapping profile : " + this._mapping.MappingProfile.FullName));
      HierarchyItem.Log.Info((object) ("Mapping profile : " + this._mapping.MappingProfile.FullName));
      HierarchyItem.Log.Info((object) ("Mapping material: " + this._mapping.MappingMaterial.FullName));
      HierarchyItem.Log.Info((object) ("Mapping UDA     : " + this._mapping.MappingUda.FullName));
      Command command = Command.CreateCommand("VAR !!UNIT UNIT");
      command.Run();
      this._unit = command.GetPMLVariableString("UNIT");
      string[] strArray = this._unit.Split(' ');
      if (!strArray[0].ToUpper().Equals("MM"))
      {
        Command.CreateCommand("MM BORE").RunInPdms();
        HierarchyItem.Log.Info((object) "Bore temporary set to MM.");
      }
      if (!strArray[2].ToUpper().Equals("MM"))
      {
        Command.CreateCommand("MM DIST").RunInPdms();
        HierarchyItem.Log.Info((object) "Distance temporary set to MM.");
      }
      HierarchyItem.Log.Info((object) "Application started");
      HierarchyItem.Log.Info((object) "Reading IFC file");
      this._ifcDoc = (IfcDocument) null;
      this._ifcDoc = new IfcDocument(ifcFilePath);
      Dictionary<string, string> breps;
      List<string> allGuids;
      List<string> profiles;
      List<string> materials;
      List<string> cutOfCuts;
      List<string> brepCuts;
      mapping.IfcObjectslist = this._ifcDoc.GetIfcObjectList(out breps, out allGuids, out profiles, out materials, out cutOfCuts, out brepCuts, true, false);
      HierarchyItem.Log.Info((object) "Finished reading IFC file");
    }

    public HierarchyItem(
      Mapping mapping,
      FileInfo ifcFile,
      string version,
      ProgressBar progressBar)
    {
      this._mapping = mapping;
      this._version = version;
      this._progressBar = progressBar;
      HierarchyItem.Log.Info((object) ("Windows user    : " + Environment.UserName));
      HierarchyItem.Log.Info((object) ("MAC Adress      : " + Tools.GetMacAddress()));
      HierarchyItem.Log.Info((object) ("Mapping profile : " + this._mapping.MappingProfile.FullName));
      HierarchyItem.Log.Info((object) ("Mapping material: " + this._mapping.MappingMaterial.FullName));
      HierarchyItem.Log.Info((object) ("Mapping UDA     : " + this._mapping.MappingUda.FullName));
      Command command = Command.CreateCommand("VAR !!UNIT UNIT");
      command.Run();
      this._unit = command.GetPMLVariableString("UNIT");
      string[] strArray = this._unit.Split(' ');
      if (!strArray[0].ToUpper().Equals("MM"))
      {
        Command.CreateCommand("MM BORE").RunInPdms();
        HierarchyItem.Log.Info((object) "Bore temporary set to MM.");
      }
      if (!strArray[2].ToUpper().Equals("MM"))
      {
        Command.CreateCommand("MM DIST").RunInPdms();
        HierarchyItem.Log.Info((object) "Distance temporary set to MM.");
      }
      IfcTools.ClearDb();
      IfcDatabaseAPI ifcDatabase = IfcTools.IfcDatabase;
      IfcTools.SetupModel(ifcFile.FullName);
      List<DbElement> dbElementList1 = this.CollectStructuralItems();
      List<DbElement> dbElementList2 = this.CollectPrimitiveItems();
      List<DbElement> dbElementList3 = this.CollectPjoiItems();
      List<DbElement> dbElementList4 = this.CollectPipingItems();
      HierarchyItem.Log.Info((object) ("Amount Structural Items       : " + (object) dbElementList1.Count));
      HierarchyItem.Log.Info((object) ("Amount Primary joints         : " + (object) dbElementList3.Count));
      HierarchyItem.Log.Info((object) ("Amount Primitive items (EQUI) : " + (object) dbElementList2.Count));
      HierarchyItem.Log.Info((object) ("Amount Piping items (PIPE)    : " + (object) dbElementList4.Count));
      this._progressBar.Value = 0;
      int num = (int) Math.Floor(1000000.0 / (double) (dbElementList1.Count + dbElementList2.Count + dbElementList3.Count + dbElementList4.Count));
      List<IIfcProduct> elements = new List<IIfcProduct>();
      using (List<DbElement>.Enumerator enumerator = dbElementList1.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DbElement current = enumerator.Current;
          StructuralItem structuralItem = new StructuralItem(mapping, "");
          this._progressBar.Value += num;
          this._progressBar.Update();
          structuralItem.ExportPdmsItem(current, ref elements);
        }
      }
      dbElementList1.Clear();
      using (List<DbElement>.Enumerator enumerator = dbElementList4.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DbElement current = enumerator.Current;
          this._progressBar.Value += num;
          this._progressBar.Update();
          this.ExportPipingItem(current, ref elements);
        }
      }
      dbElementList4.Clear();
      using (List<DbElement>.Enumerator enumerator = dbElementList2.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DbElement current = enumerator.Current;
          StructuralItem structuralItem = new StructuralItem(mapping, "");
          this._progressBar.Value += num;
          this._progressBar.Update();
          structuralItem.ExportPrimitiveItem(current, ref elements);
        }
      }
      dbElementList2.Clear();
      using (List<DbElement>.Enumerator enumerator = dbElementList3.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DbElement current = enumerator.Current;
          StructuralItem structuralItem = new StructuralItem(mapping, "");
          this._progressBar.Value += num;
          this._progressBar.Update();
          structuralItem.ExportPjoiItem(current, ref elements);
        }
      }
      IfcTools.BindElementsToModel(elements);
      IfcTools.IfcDatabase.WriteModel(ifcFile.FullName);
      Command.CreateCommand(this._unit).RunInPdms();
      this._progressBar.Value = 0;
      this._progressBar.Refresh();
    }

    public bool CreatePdmsStructure(ImportToPdmsModel model, DbElement cur)
    {
      try
      {
        Tools.SetVersion(cur, this._version);
        Tools.CreateCommand(cur, "AUTOCOLOR OFF");
        int num1 = 0;
        int num2 = (int) Math.Floor(1000000.0 / (this._mapping.IfcObjectslist.Count > 0 ? (double) this._mapping.IfcObjectslist.Count : 0.0));
        StructuralItem structuralItem = new StructuralItem(this._mapping, this._version);
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        int num6 = 0;
        int num7 = 0;
        HierarchyItem.Log.Info((object) ("Start processing ifc objects: " + DateTime.Now.ToLongTimeString()));
        foreach (IFCObjectsReader.Data.IfcObject ifcObj in this._mapping.IfcObjectslist)
        {
          try
          {
            structuralItem.BeamCreated = false;
            structuralItem.PaneCreated = false;
            structuralItem.ArbitraryCreated = false;
            structuralItem.GensecCreated = false;
            structuralItem.BrepCreated = false;
            ++num1;
            this._progressBar.Value += num2;
            this._progressBar.Update();
            bool flag = !this._mapping.ModificationSet.Tables.Count.Equals(0);
            this._mapping.IsAvevaObject = false;
            DbElement element;
            bool updateElement = this._mapping.GetUpdateElement(ifcObj.SourceGuid, "IFC", out element);
            if (this._mapping.AvevaSiteCanContainOneTeklaModelOnly & updateElement)
              this._mapping.SiteElementList.Remove(element.GetAsString(DbAttribute.GetDbAttribute(":TEKLA_IFCGUID")));
            if (!updateElement && this._mapping.UpdateAvevaObjects && this._mapping.GetUpdateElement(Tools.GetProperty(ifcObj, "Initial GUID"), "INITIAL", out element))
              this._mapping.IsAvevaObject = true;
            string str = string.Empty;
            if (flag)
            {
              DataRow modificationRow = this.GetModificationRow(ifcObj.SourceGuid);
              if (modificationRow != null)
                str = modificationRow["ChangeType"].ToString();
              if (DbElement.op_Equality(element, (DbElement) null) && !str.Equals("Added"))
              {
                if (this._mapping.RecreateDeletedObjects)
                {
                  HierarchyItem.Log.Info((object) ("Deleted PDMS element recreated: " + ifcObj.SourceGuid));
                  str = "Added";
                }
                else
                {
                  HierarchyItem.Log.Info((object) ("Deleted PDMS element not recreated: " + ifcObj.SourceGuid));
                  continue;
                }
              }
              if (this._mapping.UpdateUnchangedObjects && str.Equals("Unchanged"))
              {
                HierarchyItem.Log.Info((object) ("Updating unchanged PDMS element: " + ifcObj.SourceGuid));
                str = "Modified";
              }
              if (str.Equals("Unchanged") || str.Equals("Deleted"))
                continue;
            }
            string changeType;
            if (DbElement.op_Inequality(element, (DbElement) null))
            {
              structuralItem.NewElement = element;
              changeType = "Modified";
            }
            else
              changeType = "Added";
            structuralItem.CreatePdmsItem(cur, ifcObj, ref this._sectionMapping, changeType);
            Tools.SetColor(structuralItem.NewElement, changeType.Equals("Added") ? "GREEN" : "YELLOW");
            if (structuralItem.BeamCreated)
              ++num3;
            if (structuralItem.PaneCreated)
              ++num4;
            if (structuralItem.ArbitraryCreated)
              ++num5;
            if (structuralItem.GensecCreated)
              ++num6;
            if (structuralItem.BrepCreated)
              ++num7;
          }
          catch (Exception ex)
          {
            HierarchyItem.Log.Error((object) ("Error updating: " + ifcObj.SourceGuid));
          }
        }
        HierarchyItem.Log.Info((object) ("End processing ifc objects: " + DateTime.Now.ToLongTimeString()));
        HierarchyItem.Log.Info((object) ("Total objects processed: " + (object) num1));
        HierarchyItem.Log.Info((object) ("Beams (SCTN)           : " + (object) num3));
        HierarchyItem.Log.Info((object) ("Plates (PANE)          : " + (object) num4));
        HierarchyItem.Log.Info((object) ("Arbitrary (PANE)       : " + (object) num5));
        HierarchyItem.Log.Info((object) ("Polybeams (GENSEC)     : " + (object) num6));
        HierarchyItem.Log.Info((object) ("Surface model (BREP)   : " + (object) num7));
        this._progressBar.Value = 0;
        this._progressBar.Refresh();
        List<string> siteList = this._mapping.SiteList;
        // ISSUE: explicit non-virtual call
        if (siteList != null && __nonvirtual (siteList.Count) > 0)
          Tools.SortMembers(this._mapping, this._mapping.SiteList, this._mapping.DeleteEmptyContainerElementsAfterImport);
        Dictionary<string, DbElement> siteElementList = this._mapping.SiteElementList;
        // ISSUE: explicit non-virtual call
        if ((siteElementList != null ? (__nonvirtual (siteElementList.Count) > 0 ? 1 : 0) : 0) != 0 && this._mapping.AvevaSiteCanContainOneTeklaModelOnly)
        {
          using (Dictionary<string, DbElement>.Enumerator enumerator = this._mapping.SiteElementList.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              KeyValuePair<string, DbElement> current = enumerator.Current;
              if (this._mapping.UpdateAvevaHierarchy && DbElement.op_Inequality(this._mapping.DeletedElements, (DbElement) null))
              {
                if (this._mapping.KeepDeletedObjects)
                  current.Value.InsertBeforeFirst(this._mapping.DeletedElements);
                else
                  current.Value.Delete();
              }
              else
                current.Value.Delete();
            }
          }
          if (DbElement.op_Inequality(this._mapping.DeletedElements, (DbElement) null))
            Tools.SetColor(this._mapping.DeletedElements, "RED");
        }
        this.DeleteElements(cur);
        if (DbElement.op_Inequality(this._mapping.UnplacedElements, (DbElement) null) && this._mapping.UnplacedElements.get_IsValid())
          Tools.SetColor(this._mapping.UnplacedElements, "BLUE");
        if (!this._mapping.UpdateAvevaHierarchy)
        {
          if (((object) CurrentElement.get_Element()).Equals((object) DbElement.GetElement("WORLD")))
          {
            HierarchyItem.Log.Info((object) "Rendering skipped at WORLD level");
          }
          else
          {
            Tools.CreateCommand(cur, "AUTO CE");
            HierarchyItem.Log.Info((object) "View updated.");
          }
        }
        if (this._sectionMapping.Count.Equals(0))
        {
          HierarchyItem.Log.Info((object) "All profiles were mapped.");
        }
        else
        {
          HierarchyItem.Log.Info((object) "The following profiles were not mapped:");
          foreach (KeyValuePair<string, int> keyValuePair in this._sectionMapping)
            HierarchyItem.Log.Info((object) ("   " + keyValuePair.Key + ": " + (object) keyValuePair.Value + " occurrences."));
        }
        Tools.CreateCommand(cur, "AUTOCOLOR ON");
        Command.CreateCommand(this._unit).Run();
        CurrentElement.set_Element(cur);
        return true;
      }
      catch (Exception ex)
      {
        HierarchyItem.Log.Error((object) ("Error during update, aborting (" + ex.Message + ")."));
        int num = (int) MessageBox.Show("Error during update, aborting: " + ex.Message);
        Tools.CreateCommand(cur, "AUTOCOLOR ON");
        Command.CreateCommand(this._unit).Run();
        return false;
      }
    }

    public Dictionary<IFCObjectsReader.Data.IfcObject, DbElement> GetConnectedIfcList(
      DbElement root,
      out List<string> duplicates)
    {
      duplicates = new List<string>();
      this._connectedIfcList = new Dictionary<IFCObjectsReader.Data.IfcObject, DbElement>();
      TypeFilter typeFilter = new TypeFilter(new DbElementType[3]
      {
        (DbElementType) DbElementTypeInstance.SCTN,
        (DbElementType) DbElementTypeInstance.PANEL,
        (DbElementType) DbElementTypeInstance.GENSEC
      });
      foreach (IFCObjectsReader.Data.IfcObject key in this._mapping.IfcObjectslist)
      {
        DbElement dbElement = (DbElement) null;
        AndFilter filter = new AndFilter((BaseFilter) typeFilter, (BaseFilter) new TeklaIfcGuidFilter(key.SourceGuid));
        List<DbElement> elements = this.GetElements(root, filter);
        int count = elements.Count;
        if (!count.Equals(0))
        {
          count = elements.Count;
          if (count.Equals(1))
            dbElement = elements[0];
          else
            duplicates.Add("Duplicates found: " + elements[0].GetAsString((DbAttribute) DbAttributeInstance.REF));
        }
        this._connectedIfcList.Add(key, dbElement);
      }
      return this._connectedIfcList;
    }

    private List<DbElement> GetElements(DbElement root, AndFilter filter)
    {
      List<DbElement> dbElementList = new List<DbElement>();
      IEnumerator enumerator = new DBElementCollection(root, (BaseFilter) filter).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.Current;
          dbElementList.Add(current);
        }
      }
      finally
      {
        (enumerator as IDisposable)?.Dispose();
      }
      return dbElementList;
    }

    private void DeleteElements(DbElement cur)
    {
      if (this._mapping.ModificationSet.Tables.Count.Equals(0))
        return;
      int num = 0;
      foreach (DataRow dataRow in this._mapping.ModificationSet.Tables["AllObjects"].Rows.Cast<DataRow>().Select(row => new
      {
        row = row,
        changeType = row["ChangeType"].ToString()
      }).Where(_param1 => _param1.changeType.Equals("Deleted")).Select(_param1 => _param1.row))
      {
        DbElement element;
        if (this._mapping.GetUpdateElement(dataRow["IfcGuid"].ToString(), "IFC", out element) && !element.get_IsDeleted())
        {
          ++num;
          if (this._mapping.UpdateAvevaHierarchy && DbElement.op_Inequality(this._mapping.DeletedElements, (DbElement) null))
          {
            if (this._mapping.KeepDeletedObjects)
              element.InsertBeforeFirst(this._mapping.DeletedElements);
            else
              element.Delete();
          }
          else
            element.Delete();
        }
      }
      if (DbElement.op_Inequality(this._mapping.DeletedElements, (DbElement) null))
        Tools.SetColor(this._mapping.DeletedElements, "RED");
      if (num <= 0)
        return;
      HierarchyItem.Log.Info((object) ("     Deleted elements       : " + (object) num));
    }

    public void ExportPipingItem(DbElement cur, ref List<IIfcProduct> elements)
    {
      new PipingItem(cur, string.Empty, this._mapping).ExportComponents(ref elements);
      string asString = cur.GetAsString((DbAttribute) DbAttributeInstance.NAME);
      HierarchyItem.Log.Info((object) ("PIPE Exported: " + asString));
    }

    private List<DbElement> CollectStructuralItems()
    {
      List<DbElement> dbElementList = new List<DbElement>();
      DbElement element = CurrentElement.get_Element();
      DbElementType[] dbElementTypeArray = new DbElementType[7]
      {
        (DbElementType) DbElementTypeInstance.SCTN,
        (DbElementType) DbElementTypeInstance.PANEL,
        (DbElementType) DbElementTypeInstance.GENSEC,
        (DbElementType) DbElementTypeInstance.FLOOR,
        (DbElementType) DbElementTypeInstance.GWALL,
        (DbElementType) DbElementTypeInstance.STWALL,
        (DbElementType) DbElementTypeInstance.WALL
      };
      DBElementCollection elementCollection = new DBElementCollection(element);
      elementCollection.set_IncludeRoot(true);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
        dbElementList.Add((DbElement) enumerator.get_Current());
      return dbElementList;
    }

    private List<DbElement> CollectPipingItems()
    {
      List<DbElement> dbElementList = new List<DbElement>();
      DbElementType[] dbElementTypeArray = new DbElementType[3]
      {
        (DbElementType) DbElementTypeInstance.PIPE,
        (DbElementType) DbElementTypeInstance.HVAC,
        (DbElementType) DbElementTypeInstance.CWAY
      };
      DBElementCollection elementCollection = new DBElementCollection(CurrentElement.get_Element());
      elementCollection.set_IncludeRoot(true);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
        dbElementList.Add((DbElement) enumerator.get_Current());
      return dbElementList;
    }

    private List<DbElement> CollectPrimitiveItems()
    {
      List<DbElement> dbElementList = new List<DbElement>();
      try
      {
        DbElement element = CurrentElement.get_Element();
        DbElementType[] dbElementTypeArray = new DbElementType[17]
        {
          (DbElementType) DbElementTypeInstance.BOX,
          (DbElementType) DbElementTypeInstance.CYLINDER,
          (DbElementType) DbElementTypeInstance.SLCYLINDER,
          (DbElementType) DbElementTypeInstance.RTORUS,
          (DbElementType) DbElementTypeInstance.CTORUS,
          (DbElementType) DbElementTypeInstance.POHEDRON,
          (DbElementType) DbElementTypeInstance.SNOUT,
          (DbElementType) DbElementTypeInstance.PYRAMID,
          (DbElementType) DbElementTypeInstance.POLYHEDRON,
          (DbElementType) DbElementTypeInstance.DISH,
          (DbElementType) DbElementTypeInstance.CONE,
          (DbElementType) DbElementTypeInstance.PCLAMP,
          (DbElementType) DbElementTypeInstance.HELEMENT,
          (DbElementType) DbElementTypeInstance.EXTRUSION,
          (DbElementType) DbElementTypeInstance.NOZZLE,
          (DbElementType) DbElementTypeInstance.SCLAMP,
          (DbElementType) DbElementTypeInstance.REVOLUTION
        };
        DBElementCollection elementCollection = new DBElementCollection(element);
        elementCollection.set_IncludeRoot(true);
        elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
        DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.get_Current();
          if (Tools.CheckLevel(current, 6))
            dbElementList.Add(current);
        }
      }
      catch (Exception ex)
      {
        HierarchyItem.Log.Error((object) ("Error collecting primitives :" + ex.Message));
      }
      return dbElementList;
    }

    private DataRow GetModificationRow(string guid)
    {
      return this._mapping.ModificationSet.Tables["AllObjects"].Rows.Cast<DataRow>().Where<DataRow>((Func<DataRow, bool>) (row => row["IfcGuid"].ToString().Equals(guid))).FirstOrDefault<DataRow>();
    }

    private List<DbElement> CollectPjoiItems()
    {
      List<DbElement> dbElementList = new List<DbElement>();
      DbElement element = CurrentElement.get_Element();
      DbElementType[] dbElementTypeArray = new DbElementType[1]
      {
        (DbElementType) DbElementTypeInstance.PJOINT
      };
      DBElementCollection elementCollection = new DBElementCollection(element);
      elementCollection.set_IncludeRoot(true);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
        dbElementList.Add((DbElement) enumerator.get_Current());
      return dbElementList;
    }
  }
}
