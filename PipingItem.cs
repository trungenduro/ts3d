// Decompiled with JetBrains decompiler
// Type: TS_ModelConverter.PipingItem
// Assembly: TS_ModelConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F81CDA0D-1216-40ED-828A-BAF333294439
// Assembly location: C:\TS_PDMS\12.0SP6\TS-PDMS_Library\TS_ModelConverter.dll

using Aveva.Pdms.Database;
using Aveva.PDMS.Database.Filters;
using Aveva.Pdms.Geometry;
using Aveva.Pdms.Utilities.CommandLine;
using IfcModelCollaboration;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tekla.Structures.Geometry3d;
using Tekla.Technology.IfcLib;

namespace TS_ModelConverter
{
  public class PipingItem
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly Mapping _mapping;
    private readonly List<DbElement> _branches;
    private bool _insulation;

    public PipingItem(DbElement cur, string version, Mapping mapping)
    {
      this._mapping = mapping;
      Command command = Command.CreateCommand("VAR !!INSU REPRE INSU");
      command.Run();
      this._insulation = !command.GetPMLVariableString("INSU").ToUpper().Equals("OFF");
      this._branches = new List<DbElement>();
      if (((object) cur.GetActualType()).Equals((object) DbElementTypeInstance.BRANCH) || ((object) cur.GetActualType()).Equals((object) DbElementTypeInstance.CWBRAN))
        this._branches.Add(cur);
      foreach (DbElement member in cur.Members())
      {
        if (((object) member.GetActualType()).Equals((object) DbElementTypeInstance.BRANCH) || ((object) member.GetActualType()).Equals((object) DbElementTypeInstance.CWBRAN))
          this._branches.Add(member);
      }
    }

    public void ExportComponents(ref List<IIfcProduct> elements)
    {
      using (List<DbElement>.Enumerator enumerator1 = this._branches.GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          DbElement current = enumerator1.Current;
          try
          {
            DbElement[] dbElementArray = current.Members();
            bool flag = true;
            foreach (DbElement dbElement in dbElementArray)
            {
              if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.FTUBE))
              {
                flag = false;
                break;
              }
              if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.DUCTING))
              {
                flag = false;
                break;
              }
              if (((object) current.get_Owner().GetActualType()).Equals((object) DbElementTypeInstance.HVAC))
              {
                flag = false;
                break;
              }
              if (((object) current.get_Owner().GetActualType()).Equals((object) DbElementTypeInstance.CWAY))
              {
                flag = false;
                break;
              }
            }
            if (flag)
            {
              try
              {
                int index = 0;
                foreach (DbElement component in dbElementArray)
                {
                  if (index.Equals(0))
                  {
                    try
                    {
                      if (!this.CheckPosition(current, "PH", dbElementArray[index], "PA"))
                      {
                        Position position1 = this.GetPosition(current, "PH");
                        Position position2 = this.GetPosition(dbElementArray[index], "PA");
                        double diameter = dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.ABOR);
                        this.GetTubeDiameter(dbElementArray[index], ref diameter);
                        this.CreateStraight(position1, position2, diameter, dbElementArray[index], ref elements);
                      }
                    }
                    catch (Exception ex)
                    {
                    }
                    if (!dbElementArray.Length.Equals(1) && !this.CheckPosition(dbElementArray[index], "PL", dbElementArray[index + 1], "PA"))
                    {
                      try
                      {
                        Position position1 = this.GetPosition(dbElementArray[index], "PL");
                        Position position2 = this.GetPosition(dbElementArray[index + 1], "PA");
                        double diameter = dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.LBOR);
                        this.GetTubeDiameter(dbElementArray[index], ref diameter);
                        this.CreateStraight(position1, position2, diameter, dbElementArray[index], ref elements);
                      }
                      catch (Exception ex)
                      {
                      }
                    }
                  }
                  else if (index.Equals(dbElementArray.Length - 1))
                  {
                    try
                    {
                      if (!this.CheckPosition(dbElementArray[dbElementArray.Length - 1], "PL", current, "PT"))
                      {
                        Position position1 = this.GetPosition(dbElementArray[dbElementArray.Length - 1], "PL");
                        Position position2 = this.GetPosition(current, "PT");
                        double diameter = dbElementArray[dbElementArray.Length - 1].GetDouble((DbAttribute) DbAttributeInstance.LBOR);
                        this.GetTubeDiameter(dbElementArray[dbElementArray.Length - 1], ref diameter);
                        this.CreateStraight(position1, position2, diameter, dbElementArray[dbElementArray.Length - 1], ref elements);
                      }
                    }
                    catch (Exception ex)
                    {
                    }
                  }
                  else
                  {
                    try
                    {
                      if (!this.CheckPosition(dbElementArray[index], "PL", dbElementArray[index + 1], "PA"))
                      {
                        Position position1 = this.GetPosition(dbElementArray[index], "PL");
                        Position position2 = this.GetPosition(dbElementArray[index + 1], "PA");
                        double diameter = dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.LBOR);
                        this.GetTubeDiameter(dbElementArray[index], ref diameter);
                        this.CreateStraight(position1, position2, diameter, dbElementArray[index], ref elements);
                      }
                    }
                    catch (Exception ex)
                    {
                    }
                  }
                  if (dbElementArray.Length.Equals(1) && index.Equals(dbElementArray.Length - 1))
                  {
                    try
                    {
                      if (!this.CheckPosition(dbElementArray[dbElementArray.Length - 1], "PL", current, "PT"))
                      {
                        Position position1 = this.GetPosition(dbElementArray[dbElementArray.Length - 1], "PL");
                        Position position2 = this.GetPosition(current, "PT");
                        double diameter = dbElementArray[dbElementArray.Length - 1].GetDouble((DbAttribute) DbAttributeInstance.LBOR);
                        this.GetTubeDiameter(dbElementArray[dbElementArray.Length - 1], ref diameter);
                        this.CreateStraight(position1, position2, diameter, dbElementArray[dbElementArray.Length - 1], ref elements);
                      }
                    }
                    catch (Exception ex)
                    {
                    }
                  }
                  ++index;
                  this.CreatePipingComponent(component, ref elements);
                }
              }
              catch (Exception ex)
              {
                PipingItem.Log.Info((object) ("Error exporting piping component: " + ex.Message));
              }
            }
            else
            {
              int index = 0;
              foreach (DbElement component in dbElementArray)
              {
                if (index.Equals(0))
                {
                  if (!this.CheckPosition(current, "PH", dbElementArray[index], "PA"))
                    this.CreateStraight(this.GetPosition(current, "PH"), this.GetPosition(dbElementArray[index], "PA"), dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.ABOR), dbElementArray[index], ref elements);
                  if (!dbElementArray.Length.Equals(1) && !this.CheckPosition(dbElementArray[index], "PL", dbElementArray[index + 1], "PA"))
                    this.CreateStraight(this.GetPosition(dbElementArray[index], "PL"), this.GetPosition(dbElementArray[index + 1], "PA"), dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.LBOR), dbElementArray[index + 1], ref elements);
                }
                else if (index.Equals(dbElementArray.Length - 1))
                {
                  if (!this.CheckPosition(dbElementArray[dbElementArray.Length - 1], "PL", current, "PT"))
                    this.CreateStraight(this.GetPosition(dbElementArray[dbElementArray.Length - 1], "PL"), this.GetPosition(current, "PT"), dbElementArray[dbElementArray.Length - 1].GetDouble((DbAttribute) DbAttributeInstance.LBOR), dbElementArray[dbElementArray.Length - 1], ref elements);
                }
                else if (!this.CheckPosition(dbElementArray[index], "PL", dbElementArray[index + 1], "PA"))
                  this.CreateStraight(this.GetPosition(dbElementArray[index], "PL"), this.GetPosition(dbElementArray[index + 1], "PA"), dbElementArray[index].GetDouble((DbAttribute) DbAttributeInstance.LBOR), dbElementArray[index], ref elements);
                if (dbElementArray.Length.Equals(1) && index.Equals(dbElementArray.Length - 1) && !this.CheckPosition(dbElementArray[dbElementArray.Length - 1], "PL", current, "PT"))
                  this.CreateStraight(this.GetPosition(dbElementArray[dbElementArray.Length - 1], "PL"), this.GetPosition(current, "PT"), dbElementArray[dbElementArray.Length - 1].GetDouble((DbAttribute) DbAttributeInstance.LBOR), dbElementArray[dbElementArray.Length - 1], ref elements);
                ++index;
                if (((object) component.GetActualType()).Equals((object) DbElementTypeInstance.TUBING))
                  this.CreateStraight(component, ref elements);
                else if (((object) component.GetActualType()).Equals((object) DbElementTypeInstance.CTMTRL))
                {
                  DbElementType[] dbElementTypeArray = new DbElementType[1]
                  {
                    (DbElementType) DbElementTypeInstance.CTRAY
                  };
                  DBElementCollection elementCollection = new DBElementCollection(component);
                  elementCollection.set_IncludeRoot(true);
                  elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
                  DBElementEnumerator enumerator2 = (DBElementEnumerator) elementCollection.GetEnumerator();
                  while (enumerator2.MoveNext())
                    this.CreateComponent((DbElement) enumerator2.get_Current(), ref elements);
                }
                else
                  this.CreateComponent(component, ref elements);
              }
            }
          }
          catch (Exception ex)
          {
            PipingItem.Log.Error((object) ("Error exporting components: " + ex.Message));
          }
        }
      }
    }

    private void CreatePipingComponent(DbElement component, ref List<IIfcProduct> elements)
    {
      try
      {
        if (((object) component.GetActualType()).Equals((object) DbElementTypeInstance.ATTACHMENT))
        {
          if (component.GetElement((DbAttribute) DbAttributeInstance.CATR).GetAsString((DbAttribute) DbAttributeInstance.GTYP).ToUpper().Equals("PENI"))
            this.CreatePenetration(component, ref elements);
          else
            this.CreateComponent(component, ref elements);
        }
        else if (((object) component.GetActualType()).Equals((object) DbElementTypeInstance.TUBING))
          this.CreateStraight(component, ref elements);
        else
          this.CreateComponent(component, ref elements);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Error creating pipe component: " + ex.Message));
      }
    }

    private void CreateComponent(DbElement component, ref List<IIfcProduct> elements)
    {
      if (component.GetElement((DbAttribute) DbAttributeInstance.GMRE).get_IsNull())
        return;
      DbElement[] dbElementArray = component.GetElement((DbAttribute) DbAttributeInstance.GMRE).Members();
      int i = 0;
      foreach (DbElement dbElement in dbElementArray)
      {
        if (Tools.CheckLevel(dbElement, 6) && Tools.CheckObstruction(dbElement, 1))
        {
          if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SBOX))
            this.CreateBox(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SCYLINDER) || ((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.LCYLINDER))
            this.CreateCylinder(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SSLCYLINDER))
            this.CreateSlopedCylinder(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.LSNOUT))
            this.CreateSnout(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SCONE))
            this.CreateCone(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.LPYRAMID))
            this.CreatePyramid(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SCTORUS))
            this.CreateCircularTorus(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SRTORUS))
            this.CreateRectangularTorus(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SEXTRUSION))
            this.CreateExtrusion(component, dbElement, ref elements, i);
          else if (((object) dbElement.GetActualType()).Equals((object) DbElementTypeInstance.SREVOLUTION))
            this.CreateRevolution(component, dbElement, ref elements, i);
          ++i;
        }
      }
    }

    private void CreateStraight(DbElement component, ref List<IIfcProduct> elements)
    {
      try
      {
        double num1 = component.GetDouble((DbAttribute) DbAttributeInstance.LBOR);
        try
        {
          double[] doubleArray = component.GetDoubleArray(DbAttribute.GetDbAttribute("PARA"));
          double num2 = 0.0;
          if (doubleArray.Length > 1)
            num2 = doubleArray[1];
          num1 = num2 > num1 ? num2 : num1;
        }
        catch (Exception ex)
        {
        }
        try
        {
          if (!component.GetElement((DbAttribute) DbAttributeInstance.ISPE).get_IsNull())
          {
            double num2 = component.GetDouble((DbAttribute) DbAttributeInstance.INTHK);
            num1 += 2.0 * num2;
          }
          else if (!component.GetElement((DbAttribute) DbAttributeInstance.GMRE).get_IsNull())
          {
            foreach (DbElement member in component.GetElement((DbAttribute) DbAttributeInstance.GMRE).Members())
            {
              if (((object) member.GetActualType()).Equals((object) DbElementTypeInstance.TUBE))
                num1 = this.GetCatalogueValue(component, member.GetString(DbAttribute.GetDbAttribute("PDIA")));
            }
          }
        }
        catch (Exception ex)
        {
        }
        Position position1 = this.GetPosition(component, "PA");
        Position position2 = this.GetPosition(component, "PL");
        Direction direction1 = Direction.Create(position1, position2);
        double height = position1.Distance(position2);
        Position position3 = position1.MidPoint(position2);
        Orientation orientation = Orientation.Create("Z  IS " + ((object) direction1).ToString() + " WRT WORLD");
        Direction direction2 = orientation.XDir();
        Direction direction3 = orientation.YDir();
        Direction direction4 = orientation.ZDir();
        Primitive primitive = new Primitive()
        {
          Position = position3,
          DirX = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up()),
          DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up()),
          DirZ = IfcTools.CreateDirection(direction4.get_East(), direction4.get_North(), direction4.get_Up())
        };
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1]);
        IIfcElement cylinder = primitive.CreateCylinder(ref elements, height, num1 / 2.0, guid);
        Tools.CreateUda(component, (IIfcObject) cylinder, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Straight pipe: " + ex.Message));
      }
    }

    private void CreateStraight(
      Position startPos,
      Position endPos,
      double diameter,
      DbElement component,
      ref List<IIfcProduct> elements)
    {
      try
      {
        Direction direction1 = Direction.Create(startPos, endPos);
        double height = startPos.Distance(endPos);
        Position position = startPos.MidPoint(endPos);
        Orientation orientation = Orientation.Create("Z  IS " + ((object) direction1).ToString() + " WRT WORLD");
        Direction direction2 = orientation.XDir();
        Direction direction3 = orientation.YDir();
        Direction direction4 = orientation.ZDir();
        Primitive primitive = new Primitive()
        {
          Position = position,
          DirX = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up()),
          DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up()),
          DirZ = IfcTools.CreateDirection(direction4.get_East(), direction4.get_North(), direction4.get_Up())
        };
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.FullName + (object) startPos + "/" + (object) endPos);
        primitive.CreateCylinder(ref elements, height, diameter / 2.0, guid);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Straight pipe: " + ex.Message));
      }
    }

    private void CreateBox(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      try
      {
        Primitive primitive = new Primitive(component);
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PXLEN")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PYLEN")));
        double catalogueValue3 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PZLEN")));
        double catalogueValue4 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PX")));
        double catalogueValue5 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PY")));
        double catalogueValue6 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PZ")));
        Point point = this.CreateGlobalMatrix(primitive).Transform(new Point(catalogueValue4, catalogueValue5, catalogueValue6));
        primitive.Position = Position.Create(point.X, point.Y, point.Z);
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement box = primitive.CreateBox(ref elements, catalogueValue1, catalogueValue2, catalogueValue3, guid);
        Tools.CreateUda(component, (IIfcObject) box, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Box: " + ex.Message));
      }
    }

    private void CreateCylinder(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString = geometry.GetAsString(DbAttribute.GetDbAttribute("PAXIS"));
        Position position;
        Direction direction1;
        this.GetPosDir(component, asString, out position, out direction1);
        Orientation orientation = Orientation.Create("Z  IS " + ((object) direction1).ToString());
        double num1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIA"))) / 2.0;
        double num2;
        if (((object) geometry.GetActualType()).Equals((object) DbElementTypeInstance.SCYLINDER))
        {
          double catalogueValue = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIST")));
          num2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PHEIG")));
          double num3 = catalogueValue + num2 / 2.0;
          position.MoveBy(direction1, num3);
        }
        else
        {
          double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBDIST")));
          double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PTDIST")));
          num2 = Math.Abs(catalogueValue2 - catalogueValue1);
          double num3 = catalogueValue1 + (catalogueValue2 - catalogueValue1) / 2.0;
          position.MoveBy(direction1, num3);
        }
        if (num1.Equals(0.0) || num2.Equals(0.0))
          return;
        Direction direction2 = orientation.XDir();
        Direction direction3 = orientation.YDir();
        Direction direction4 = orientation.ZDir();
        primitive.Position = position;
        primitive.DirX = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        primitive.DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        primitive.DirZ = IfcTools.CreateDirection(direction4.get_East(), direction4.get_North(), direction4.get_Up());
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement cylinder = primitive.CreateCylinder(ref elements, Math.Abs(num2), Math.Abs(num1), guid);
        Tools.CreateUda(component, (IIfcObject) cylinder, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Cylinder: " + ex.Message));
      }
    }

    private void CreateSlopedCylinder(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString = geometry.GetAsString(DbAttribute.GetDbAttribute("PAXIS"));
        Position position;
        Direction direction1;
        this.GetPosDir(component, asString, out position, out direction1);
        Orientation.Create("Z  IS " + (object) direction1);
        double num1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIA"))) / 2.0;
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIST")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PHEIG")));
        double catalogueValue3 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PXTS")));
        double catalogueValue4 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PYTS")));
        double catalogueValue5 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PXBS")));
        double catalogueValue6 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PYBS")));
        double num2 = catalogueValue1 + catalogueValue2 / 2.0;
        position.MoveBy(direction1, num2);
        if (num1.Equals(0.0) || catalogueValue2.Equals(0.0))
          return;
        Direction direction2 = Tools.GetDirection("PA DIR", component).Opposite();
        Direction direction3 = Tools.GetDirection("PL DIR", component);
        Vector vector1 = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        Vector Vector1 = new Vector(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        Vector vector2 = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        Vector Vector2 = vector1.Cross(Vector1);
        Vector vector3 = vector2.Cross(Vector2);
        primitive.Position = position;
        primitive.DirX = IfcTools.CreateDirection(Vector2.X, Vector2.Y, Vector2.Z);
        primitive.DirY = IfcTools.CreateDirection(vector3.X, vector3.Y, vector3.Z);
        primitive.DirZ = IfcTools.CreateDirection(vector2.X, vector2.Y, vector2.Z);
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement slopedCylinder = primitive.CreateSlopedCylinder(ref elements, Math.Abs(catalogueValue2), Math.Abs(num1), catalogueValue5, catalogueValue6, catalogueValue3, catalogueValue4, guid);
        Tools.CreateUda(component, (IIfcObject) slopedCylinder, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Sloped Cylinder: " + ex.Message));
      }
    }

    private void CreateSnout(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBDIS")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PTDIS")));
        double catalogueValue3 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBDM")));
        double catalogueValue4 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PTDM")));
        double catalogueValue5 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("POFF")));
        double height = Math.Abs(catalogueValue2 - catalogueValue1);
        double num = catalogueValue1 + (catalogueValue2 - catalogueValue1) / 2.0;
        position1.MoveBy(direction1, num);
        position1.MoveBy(direction2, catalogueValue5 / 2.0);
        double diaBot = catalogueValue3.Equals(0.0) ? 1.0 : catalogueValue3;
        double diaTop = catalogueValue4.Equals(0.0) ? 1.0 : catalogueValue4;
        primitive.Position = position1;
        primitive.DirZ = IfcTools.CreateDirection(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        primitive.DirX = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement snout = primitive.CreateSnout(ref elements, height, diaBot, diaTop, catalogueValue5, 0.0, guid);
        Tools.CreateUda(component, (IIfcObject) snout, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Snout: " + ex.Message));
      }
    }

    private void CreateCone(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString = geometry.GetAsString(DbAttribute.GetDbAttribute("PAXIS"));
        Position position;
        Direction direction1;
        this.GetPosDir(component, asString, out position, out direction1);
        Orientation orientation = Orientation.Create("Z  IS " + ((object) direction1).ToString());
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIST")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIAM")));
        if (catalogueValue2.Equals(0.0) || catalogueValue1.Equals(0.0))
          return;
        Direction direction2 = orientation.XDir();
        Direction direction3 = orientation.YDir();
        Direction direction4 = orientation.ZDir();
        position.MoveBy(direction1, catalogueValue1 / 2.0);
        primitive.Position = position;
        primitive.DirX = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        primitive.DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        primitive.DirZ = IfcTools.CreateDirection(direction4.get_East(), direction4.get_North(), direction4.get_Up());
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement snout = primitive.CreateSnout(ref elements, catalogueValue1, catalogueValue2, 1.0, 0.0, 0.0, guid);
        Tools.CreateUda(component, (IIfcObject) snout, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Cone: " + ex.Message));
      }
    }

    private void CreateCircularTorus(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        Direction direction3 = direction1.Opposite();
        Point point1 = new Point(position1.get_X(), position1.get_Y(), position1.get_Z());
        Point Origin = new Point(position2.get_X(), position2.get_Y(), position2.get_Z());
        Vector vector1 = new Vector(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        Vector vector2 = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        Point Point2;
        double angle;
        if (direction3.IsParallel(direction2) || direction3.IsAntiParallel(direction2))
        {
          Position position3 = position1.MidPoint(position2);
          Point2 = new Point(position3.get_X(), position3.get_Y(), position3.get_Z());
          angle = Math.PI;
        }
        else
        {
          Point2 = Intersection.LineToPlane(Intersection.PlaneToPlane(new GeometricPlane(point1, vector1), new GeometricPlane(Origin, vector2)), new GeometricPlane(Origin, vector1, vector2));
          angle = vector1.GetAngleBetween(vector2);
        }
        Vector vector3 = new Vector(point1 - Point2);
        primitive.Position = Position.Create(Point2.X, Point2.Y, Point2.Z);
        primitive.DirX = IfcTools.CreateDirection(vector3.X, vector3.Y, vector3.Z);
        primitive.DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        double radius = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIA"))) / 2.0;
        double point2 = Distance.PointToPoint(point1, Point2);
        if (point2 <= 0.0)
          return;
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement circularTorus = primitive.CreateCircularTorus(ref elements, radius, point2, angle, guid);
        Tools.CreateUda(component, (IIfcObject) circularTorus, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Circular Torus: " + ex.Message));
      }
    }

    private void CreateRectangularTorus(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        Direction direction3 = direction1.Opposite();
        Point point1 = new Point(position1.get_X(), position1.get_Y(), position1.get_Z());
        Point Origin = new Point(position2.get_X(), position2.get_Y(), position2.get_Z());
        Point Point2;
        double angle;
        if (direction3.IsParallel(direction2) || direction3.IsAntiParallel(direction2))
        {
          Position position3 = position1.MidPoint(position2);
          Point2 = new Point(position3.get_X(), position3.get_Y(), position3.get_Z());
          angle = Math.PI;
        }
        else
        {
          Vector vector1 = new Vector(direction3.get_East(), direction3.get_North(), direction3.get_Up());
          Vector vector2 = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up());
          Point2 = Intersection.LineToPlane(Intersection.PlaneToPlane(new GeometricPlane(point1, vector1), new GeometricPlane(Origin, vector2)), new GeometricPlane(Origin, vector1, vector2));
          angle = vector1.GetAngleBetween(vector2);
        }
        Vector vector = new Vector(point1 - Point2);
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PDIA")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PHEI")));
        double point2 = Distance.PointToPoint(point1, Point2);
        if (point2 <= 0.0)
          return;
        primitive.Position = Position.Create(Point2.X, Point2.Y, Point2.Z);
        primitive.DirX = IfcTools.CreateDirection(vector.X, vector.Y, vector.Z);
        primitive.DirY = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement rectangularTorus = primitive.CreateRectangularTorus(ref elements, Math.Abs(catalogueValue1), Math.Abs(catalogueValue2), point2, angle, guid);
        Tools.CreateUda(component, (IIfcObject) rectangularTorus, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Rectangular Torus: " + ex.Message));
      }
    }

    private void CreatePyramid(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        string asString3 = geometry.GetAsString(DbAttribute.GetDbAttribute("PCAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        Direction direction3;
        this.GetPosDir(component, asString3, out position2, out direction3);
        double catalogueValue1 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBDIS")));
        double catalogueValue2 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PTDIS")));
        double catalogueValue3 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBBT")));
        double catalogueValue4 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PCBT")));
        double catalogueValue5 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBTP")));
        double catalogueValue6 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PCTP")));
        double catalogueValue7 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PBOFF")));
        double catalogueValue8 = this.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PCOFF")));
        Vector vector1 = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        Vector Vector = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        Vector vector2 = vector1.Cross(Vector);
        Point position1_1 = new Point(0.0, 0.0, catalogueValue1);
        Point position2_1 = new Point(catalogueValue7, catalogueValue8, catalogueValue2);
        double botX = catalogueValue3.Equals(0.0) ? 1.0 : catalogueValue3;
        double botY = catalogueValue4.Equals(0.0) ? 1.0 : catalogueValue4;
        double topX = catalogueValue5.Equals(0.0) ? 1.0 : catalogueValue5;
        double topY = catalogueValue6.Equals(0.0) ? 1.0 : catalogueValue6;
        primitive.Position = position1;
        primitive.DirX = IfcTools.CreateDirection(Vector.X, Vector.Y, Vector.Z);
        primitive.DirY = IfcTools.CreateDirection(vector2.X, vector2.Y, vector2.Z);
        primitive.DirZ = IfcTools.CreateDirection(vector1.X, vector1.Y, vector1.Z);
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement pyramid = primitive.CreatePyramid(ref elements, position1_1, position2_1, botX, botY, topX, topY, guid);
        Tools.CreateUda(component, (IIfcObject) pyramid, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Pyramid: " + ex.Message));
      }
    }

    private void CreateExtrusion(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        double catalogueValue1 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PHEIG")));
        double catalogueValue2 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PX")));
        double catalogueValue3 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PY")));
        double catalogueValue4 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PZ")));
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        primitive.Position = position1;
        Vector vector = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up()).Cross(new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up()));
        vector.Normalize();
        Direction direction3 = Direction.Create(Position.Create(vector.X, vector.Y, vector.Z));
        primitive.Position.MoveBy(direction1, catalogueValue2);
        primitive.Position.MoveBy(direction2, catalogueValue3);
        primitive.Position.MoveBy(direction3, catalogueValue4);
        primitive.DirZ = IfcTools.CreateDirection(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        primitive.DirY = IfcTools.CreateDirection(direction2.get_East(), direction2.get_North(), direction2.get_Up());
        primitive.DirX = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
        DBElementCollection elementCollection = new DBElementCollection(((BaseFilter) new TypeFilter((DbElementType) DbElementTypeInstance.SLOOP)).FirstMember(geometry));
        elementCollection.set_IncludeRoot(false);
        elementCollection.set_Filter((BaseFilter) new TypeFilter((DbElementType) DbElementTypeInstance.SVERTEX));
        DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
        List<Point> points = new List<Point>();
        List<double> chamfers = new List<double>();
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.get_Current();
          double catalogueValue5 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PX")));
          double catalogueValue6 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PY")));
          double catalogueValue7 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PRADIUS")));
          points.Add(new Point(catalogueValue5, catalogueValue6));
          chamfers.Add(catalogueValue7);
        }
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement extrusion = primitive.CreateExtrusion(ref elements, points, chamfers, catalogueValue1, guid);
        Tools.CreateUda(component, (IIfcObject) extrusion, this._mapping, Discipline.Mechanical);
        this.CreateNegatives(extrusion, component, geometry);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Extrusion: " + ex.Message));
      }
    }

    private void CreateRevolution(
      DbElement component,
      DbElement geometry,
      ref List<IIfcProduct> elements,
      int i)
    {
      Primitive primitive = new Primitive(component);
      try
      {
        double catalogueValue1 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PX")));
        double catalogueValue2 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PY")));
        double catalogueValue3 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PZ")));
        double catalogueValue4 = Tools.GetCatalogueValue(component, geometry.GetAsString(DbAttribute.GetDbAttribute("PANGLE")));
        string asString1 = geometry.GetAsString(DbAttribute.GetDbAttribute("PAAXIS"));
        string asString2 = geometry.GetAsString(DbAttribute.GetDbAttribute("PBAXIS"));
        Position position1;
        Direction direction1;
        this.GetPosDir(component, asString1, out position1, out direction1);
        Position position2;
        Direction direction2;
        this.GetPosDir(component, asString2, out position2, out direction2);
        primitive.Position = position1;
        Vector vector = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up()).Cross(new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up()));
        vector.Normalize();
        Direction direction3 = Direction.Create(Position.Create(vector.X, vector.Y, vector.Z));
        if (!catalogueValue1.Equals(0.0))
          primitive.Position.MoveBy(direction1, catalogueValue1);
        if (!catalogueValue2.Equals(0.0))
          primitive.Position.MoveBy(direction2, catalogueValue2);
        if (!catalogueValue3.Equals(0.0))
          primitive.Position.MoveBy(direction3, catalogueValue3);
        primitive.DirX = IfcTools.CreateDirection(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        primitive.DirY = IfcTools.CreateDirection(-direction2.get_East(), -direction2.get_North(), -direction2.get_Up());
        primitive.DirZ = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction1.get_Up());
        DBElementCollection elementCollection = new DBElementCollection(((BaseFilter) new TypeFilter((DbElementType) DbElementTypeInstance.SLOOP)).FirstMember(geometry));
        elementCollection.set_IncludeRoot(false);
        elementCollection.set_Filter((BaseFilter) new TypeFilter((DbElementType) DbElementTypeInstance.SVERTEX));
        DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
        List<Point> points = new List<Point>();
        List<double> chamfers = new List<double>();
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.get_Current();
          double catalogueValue5 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PX")));
          double catalogueValue6 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PY")));
          double catalogueValue7 = Tools.GetCatalogueValue(component, current.GetAsString(DbAttribute.GetDbAttribute("PRADIUS")));
          Point point = new Point(catalogueValue5, catalogueValue6);
          points.Add(point);
          chamfers.Add(catalogueValue7);
        }
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1] + "_" + (object) i);
        IIfcElement revolution = primitive.CreateRevolution(ref elements, points, chamfers, catalogueValue4 * Math.PI / 180.0, guid);
        Tools.CreateUda(component, (IIfcObject) revolution, this._mapping, Discipline.Mechanical);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Revolution: " + ex.Message));
      }
    }

    private void CreatePenetration(DbElement component, ref List<IIfcProduct> elements)
    {
      try
      {
        Direction direction1 = Tools.GetDirection("PL DIR WRT WORLD", component);
        Position position = Tools.GetPosition("PA POS WRT WORLD", component).MidPoint(Tools.GetPosition("PL POS WRT WORLD", component));
        string str = component.GetElement((DbAttribute) DbAttributeInstance.SPRE).GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1);
        double num1 = component.GetDouble((DbAttribute) DbAttributeInstance.ABOR);
        double num2 = 100.0;
        double num3 = num1 / 2.0;
        IIfcDirection direction2 = IfcTools.CreateDirection(1.0, 0.0, 0.0);
        IIfcDirection direction3 = IfcTools.CreateDirection(direction1.get_East(), direction1.get_North(), direction1.get_Up());
        IfcAxis2Placement location = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(position.get_X(), position.get_Y(), position.get_Z()), direction3, direction2));
        IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
        IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, -num2 / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(0.0, -1.0, 0.0));
        IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
        {
          IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateExtrudedAreaSolid((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) num3), axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) num2), "Body", "SweptSolid")
        });
        Guid guid = Tools.CreateGuid(this._mapping.ProjectPath.ToString() + (object) component.RefNo()[0] + "/" + (object) component.RefNo()[1]);
        IIfcBuildingElementProxy penetration = IfcTools.CreatePenetration(location, (IIfcProductRepresentation) productDefinitionShape, guid);
        Tools.CreateUda(component, (IIfcObject) penetration, this._mapping, Discipline.Mechanical);
        IfcTools.AddQuantities((IIfcObject) penetration);
        IfcTools.AddColorToElement((IIfcElement) penetration, "red");
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PENI_SPEC",
            (object) (IfcLabel) str
          },
          {
            "System",
            (object) (IfcLabel) "PDMS"
          },
          {
            "Shape",
            (object) (IfcLabel) "Circular"
          },
          {
            "Depth",
            (object) (IfcPositiveLengthMeasure) num2
          },
          {
            "Diameter",
            (object) (IfcPositiveLengthMeasure) num1
          }
        };
        if (attributes.Count > 0)
          IfcTools.AddProperties((IIfcObject) penetration, "Pset_ProvisionForVoid", attributes);
        elements.Add((IIfcProduct) penetration);
      }
      catch (Exception ex)
      {
        PipingItem.Log.Error((object) ("Penetration: " + ex.Message));
      }
    }

    private Position GetPosition(DbElement cur, string type)
    {
      if (type.Equals("PH"))
        return Tools.GetPosition("HPOS WRT WORLD", cur);
      if (type.Equals("PT"))
        return Tools.GetPosition("TPOS WRT WORLD", cur);
      if (type.Equals("PA"))
        return Tools.GetPosition("APOS WRT WORLD", cur);
      if (type.Equals("PL"))
        return Tools.GetPosition("LPOS WRT WORLD", cur);
      return (Position) null;
    }

    private void GetPosDir(
      DbElement element,
      string key,
      out Position position,
      out Direction direction)
    {
      position = Position.Create();
      direction = Direction.Create();
      if (key.Contains("P"))
      {
        int startIndex = key.StartsWith("-") ? 1 : 0;
        DbExpression dbExpression1 = DbExpression.Parse(key.Substring(startIndex) + " POS WRT WORLD");
        if (dbExpression1 != null)
          position = element.EvaluatePosition(dbExpression1);
        else
          PipingItem.Log.Error((object) ("Problem executing expression: " + key));
        DbExpression dbExpression2 = DbExpression.Parse(key.Substring(startIndex) + " DIR WRT WORLD");
        if (dbExpression2 != null)
        {
          direction = element.EvaluateDirection(dbExpression2);
          if (!startIndex.Equals(1))
            return;
          direction = direction.Opposite();
        }
        else
          PipingItem.Log.Error((object) ("Problem executing expression: " + key));
      }
      else
      {
        int startIndex = key.StartsWith("-") ? 1 : 0;
        key = Tools.ModifyExpression(key.Substring(startIndex));
        DbExpression dbExpression1 = DbExpression.Parse(key);
        if (dbExpression1 != null)
          direction = element.EvaluateDirection(dbExpression1);
        if (startIndex.Equals(1))
          direction = direction.Opposite();
        DbExpression dbExpression2 = DbExpression.Parse("POS WRT WORLD");
        if (dbExpression2 != null)
          position = element.EvaluatePosition(dbExpression2);
        else
          PipingItem.Log.Error((object) ("Problem executing expression: " + key));
        Orientation orientation = Orientation.Create();
        DbExpression dbExpression3 = DbExpression.Parse("ORI WRT WORLD");
        if (dbExpression3 != null)
          orientation = element.EvaluateOrientation(dbExpression3);
        else
          PipingItem.Log.Error((object) ("Problem executing expression: " + key));
        orientation.XDir();
        orientation.YDir();
        orientation.ZDir();
        CoordinateSystem CoordSys = new CoordinateSystem()
        {
          Origin = new Point(position.get_X(), position.get_Y(), position.get_Z()),
          AxisX = new Vector(orientation.XDir().get_East(), orientation.XDir().get_North(), orientation.XDir().get_Up()),
          AxisY = new Vector(orientation.YDir().get_East(), orientation.YDir().get_North(), orientation.YDir().get_Up())
        };
        Matrix matrix = MatrixFactory.FromCoordinateSystem(CoordSys);
        Vector vector1 = new Vector(direction.get_East(), direction.get_North(), direction.get_Up());
        vector1.Normalize(1000.0);
        Vector vector2 = new Vector(matrix.Transform(new Point(vector1.X, vector1.Y, vector1.Z)) - CoordSys.Origin);
        direction = Direction.Create(Position.Create(vector2.X, vector2.Y, vector2.Z));
      }
    }

    private Matrix CreateGlobalMatrix(Primitive primitive)
    {
      return MatrixFactory.FromCoordinateSystem(new CoordinateSystem(new Point(primitive.Position.get_X(), primitive.Position.get_Y(), primitive.Position.get_Z()), new Vector(primitive.Orientation.XDir().get_East(), primitive.Orientation.XDir().get_North(), primitive.Orientation.XDir().get_Up()), new Vector(primitive.Orientation.YDir().get_East(), primitive.Orientation.YDir().get_North(), primitive.Orientation.YDir().get_Up())));
    }

    private double GetCatalogueValue(DbElement component, string expressionString)
    {
      string str = expressionString;
      double num = 0.0;
      try
      {
        if (DbExpression.Parse(expressionString) != null)
          expressionString = Tools.ModifyExpression(expressionString, component);
        DbExpression dbExpression = DbExpression.Parse(expressionString);
        if (dbExpression != null)
          num = Math.Round(component.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
        else
          PipingItem.Log.Error((object) ("Could not parse expression: " + str + " : " + ((object) component.GetActualType()).ToString()));
      }
      catch
      {
        PipingItem.Log.Error((object) ("Unable to evaluate expression: " + str + " : " + ((object) component.GetActualType()).ToString()));
      }
      return num;
    }

    private void CreateNegatives(IIfcElement ifcParent, DbElement sub, DbElement geometry)
    {
      foreach (DbElement member in geometry.Members())
      {
        if (Tools.CheckLevel(member, 6))
        {
          if (((object) member.GetActualType()).Equals((object) DbElementTypeInstance.NSBOX))
          {
            try
            {
              double catalogueValue1 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PXLEN")));
              double catalogueValue2 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PYLEN")));
              double catalogueValue3 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PZLEN")));
              double catalogueValue4 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PX")));
              double catalogueValue5 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PY")));
              double catalogueValue6 = Tools.GetCatalogueValue(sub, member.GetAsString(DbAttribute.GetDbAttribute("PZ")));
              Position position = Position.Create();
              Orientation orientation = Orientation.Create();
              DbExpression dbExpression1 = DbExpression.Parse("POS WRT WORLD");
              if (dbExpression1 != null)
                position = sub.EvaluatePosition(dbExpression1);
              DbExpression dbExpression2 = DbExpression.Parse("ORI WRT WORLD");
              if (dbExpression2 != null)
                orientation = sub.EvaluateOrientation(dbExpression2);
              if (!catalogueValue4.Equals(0.0))
                position.MoveBy(orientation.XDir(), catalogueValue4);
              if (!catalogueValue5.Equals(0.0))
                position.MoveBy(orientation.YDir(), catalogueValue5);
              if (!catalogueValue6.Equals(0.0))
                position.MoveBy(orientation.ZDir(), catalogueValue6);
              IIfcOpeningElement box = Negative.CreateBox(position, orientation, catalogueValue1, catalogueValue2, catalogueValue3);
              IfcTools.CreateRelVoidsElement(ifcParent, box);
            }
            catch (Exception ex)
            {
              PipingItem.Log.Error((object) ("Negative box (Catalogue NBOX): " + ex.Message));
            }
          }
        }
      }
    }

    private bool CheckPosition(
      DbElement startObj,
      string startTag,
      DbElement endObj,
      string endTag)
    {
      return this.GetPosition(startObj, startTag).Equals(this.GetPosition(endObj, endTag));
    }

    private bool GetTubeDiameter(DbElement component, ref double diameter)
    {
      if (!component.GetElement((DbAttribute) DbAttributeInstance.ISPE).get_IsNull())
      {
        double num = component.GetDouble((DbAttribute) DbAttributeInstance.INTHK);
        diameter += 2.0 * num;
        return true;
      }
      if (!component.GetElement((DbAttribute) DbAttributeInstance.GMRE).get_IsNull())
      {
        foreach (DbElement member in component.GetElement((DbAttribute) DbAttributeInstance.GMRE).Members())
        {
          if (((object) member.GetActualType()).Equals((object) DbElementTypeInstance.TUBE))
          {
            diameter = this.GetCatalogueValue(component, member.GetString(DbAttribute.GetDbAttribute("PDIA")));
            return true;
          }
        }
      }
      return false;
    }
  }
}
