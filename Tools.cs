// Decompiled with JetBrains decompiler
// Type: TS_ModelConverter.Tools
// Assembly: TS_ModelConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F81CDA0D-1216-40ED-828A-BAF333294439
// Assembly location: C:\TS_PDMS\12.0SP6\TS-PDMS_Library\TS_ModelConverter.dll

using Aveva.Pdms.Database;
using Aveva.PDMS.Database.Filters;
using Aveva.Pdms.Geometry;
using Aveva.Pdms.Maths.Geometry;
using Aveva.Pdms.Shared;
using Aveva.Pdms.Utilities.CommandLine;
using IfcModelCollaboration;
using IFCObjectsReader;
using IFCObjectsReader.Data;
using IFCObjectsReader.Tools;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Technology.IfcLib;

namespace TS_ModelConverter
{
  public static class Tools
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static DbQualifier wrtQualifyer;

    public static DbQualifier WrtQualifyer
    {
      get
      {
        if (TS_ModelConverter.Tools.wrtQualifyer == null)
        {
          TS_ModelConverter.Tools.wrtQualifyer = new DbQualifier();
          DbElement element = DbElement.GetElement("/*");
          TS_ModelConverter.Tools.wrtQualifyer.set_wrtQualifier(element);
        }
        return TS_ModelConverter.Tools.wrtQualifyer;
      }
    }

    public static DirectoryInfo GetProjectPath()
    {
      try
      {
        Command command = Command.CreateCommand("var !!PROJ EVAR '" + ((Project) Project.CurrentProject).get_Name() + "000'");
        command.RunInPdms();
        string pmlVariableString = command.GetPMLVariableString("PROJ");
        if (pmlVariableString.Length > 0)
        {
          DirectoryInfo directoryInfo1 = new DirectoryInfo(Path.Combine(new DirectoryInfo(pmlVariableString).Parent.FullName, "TS-PDMS"));
          if (!directoryInfo1.Exists)
            directoryInfo1.Create();
          DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(directoryInfo1.FullName, "Mapping"));
          if (!directoryInfo2.Exists)
            directoryInfo2.Create();
          return directoryInfo1;
        }
      }
      catch
      {
        return (DirectoryInfo) null;
      }
      return (DirectoryInfo) null;
    }

    public static bool CheckMappingObject(Mapping mapping)
    {
      if (mapping?.ProjectPath == null || !mapping.ProjectPath.Exists)
        return false;
      return !string.IsNullOrEmpty(mapping.ProjectPath.FullName);
    }

    public static ArrayList GetStruList()
    {
      return new ArrayList()
      {
        (object) "Selected PDMS hierarchy item",
        (object) "Selected GPSET members"
      };
    }

    public static bool GetPlin(DbElement spre, string name, out double valueX, out double valueY)
    {
      valueX = 0.0;
      valueY = 0.0;
      try
      {
        if (!spre.get_IsNull())
        {
          foreach (DbElement member in spre.GetElement((DbAttribute) DbAttributeInstance.PSTR).Members())
          {
            if (member.GetAsString((DbAttribute) DbAttributeInstance.PKEY).Equals(name))
            {
              if (!TS_ModelConverter.Tools.EvaluateDouble(member.GetAsString((DbAttribute) DbAttributeInstance.PX), spre, out valueX))
                TS_ModelConverter.Tools.EvaluateDouble(TS_ModelConverter.Tools.ModifyExpression(member.GetAsString((DbAttribute) DbAttributeInstance.PX)), spre, out valueX);
              if (!TS_ModelConverter.Tools.EvaluateDouble(member.GetAsString((DbAttribute) DbAttributeInstance.PY), spre, out valueY))
                TS_ModelConverter.Tools.EvaluateDouble(TS_ModelConverter.Tools.ModifyExpression(member.GetAsString((DbAttribute) DbAttributeInstance.PY)), spre, out valueY);
              return true;
            }
          }
        }
      }
      catch
      {
        TS_ModelConverter.Tools.Log.Error((object) ("Problem getting alignment for " + name));
      }
      return false;
    }

    public static string GetPlinDir(DbElement cur, string name)
    {
      DbElement element = CurrentElement.get_Element();
      CurrentElement.set_Element(cur);
      Command command = Command.CreateCommand("var !!VALUE pplin " + name + " z direction wrt world");
      command.RunInPdms();
      string pmlVariableString = command.GetPMLVariableString("VALUE");
      CurrentElement.set_Element(element);
      return pmlVariableString;
    }

    public static bool ExistsPlin(DbElement spre, string justification)
    {
      foreach (DbElement member in spre.GetElement((DbAttribute) DbAttributeInstance.PSTR).Members())
      {
        if (member.GetString((DbAttribute) DbAttributeInstance.PKEY).Equals(justification))
          return true;
      }
      return false;
    }

    public static bool ExistsInModel(string name)
    {
      return !((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + name).get_IsNull();
    }

    public static bool ExistsInModel(string name, out DbElement element)
    {
      name = name.StartsWith("/") ? name : "/" + name;
      if (!((MDB) MDB.CurrentMDB).FindElement((DbType) 1, name).get_IsValid())
      {
        element = (DbElement) null;
        return false;
      }
      element = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, name);
      return true;
    }

    public static bool ExistsProfile(ref string profile)
    {
      string str1 = profile;
      bool flag = !string.IsNullOrEmpty(profile);
      if (flag && ((MDB) MDB.CurrentMDB).FindElement((DbType) 2, profile).get_IsNull())
      {
        string str2 = str1.Replace("\"", "-");
        if (!((MDB) MDB.CurrentMDB).FindElement((DbType) 2, str2).get_IsNull())
          profile = str2;
        else
          flag = false;
      }
      return flag;
    }

    public static void FindCircle(Point a, Point b, Point c, out Point center, out double radius)
    {
      double num1 = (b.X + a.X) / 2.0;
      double num2 = (b.Y + a.Y) / 2.0;
      double num3 = b.X - a.X;
      double num4 = -(b.Y - a.Y);
      double num5 = (c.X + b.X) / 2.0;
      double num6 = (c.Y + b.Y) / 2.0;
      double num7 = c.X - b.X;
      double num8 = -(c.Y - b.Y);
      double X = (num2 * num4 * num8 + num5 * num4 * num7 - num1 * num3 * num8 - num6 * num4 * num8) / (num4 * num7 - num3 * num8);
      double Y = (X - num1) * num3 / num4 + num2;
      center = new Point(X, Y, 0.0);
      double num9 = X - a.X;
      double num10 = Y - a.Y;
      radius = Math.Sqrt(num9 * num9 + num10 * num10);
    }

    public static List<DbElement> CollectElements(string name)
    {
      List<DbElement> dbElementList = new List<DbElement>();
      DbElement element = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + name);
      DbElementType[] dbElementTypeArray = new DbElementType[2]
      {
        (DbElementType) DbElementTypeInstance.SCTN,
        (DbElementType) DbElementTypeInstance.PANEL
      };
      DBElementCollection elementCollection = new DBElementCollection(element);
      elementCollection.set_IncludeRoot(false);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
        dbElementList.Add((DbElement) enumerator.get_Current());
      return dbElementList;
    }

    public static double CreateValue(string sValue)
    {
      double result;
      bool flag = double.TryParse(sValue, NumberStyles.Number, (IFormatProvider) CultureInfo.InvariantCulture, out result);
      if (!flag)
        flag = double.TryParse(sValue.Replace(",", "."), out result);
      if (!flag)
        double.TryParse(sValue.Replace(".", ","), out result);
      return result;
    }

    public static string ModifyExpression(string expr)
    {
      expr = expr.Replace("TIMES", "*");
      expr = expr.Replace("ATTRIB", string.Empty);
      expr = expr.Replace("IPARAM", "IPAR");
      expr = expr.Replace("PARAM", "PARA");
      if (expr.Contains("DDRADIUS"))
      {
        Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDRADIUS");
        command.Run();
        string newValue = command.GetPMLVariableString("VALUE").Replace("mm", string.Empty);
        expr = expr.Replace("DDRADIUS", newValue);
      }
      if (expr.Contains("DDHEIGHT"))
      {
        Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDHEIGHT");
        command.Run();
        string newValue = command.GetPMLVariableString("VALUE").Replace("mm", string.Empty);
        expr = expr.Replace("DDHEIGHT", newValue);
      }
      if (expr.Contains("DDANGLE"))
      {
        Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDANGLE");
        command.Run();
        string pmlVariableString = command.GetPMLVariableString("VALUE");
        expr = expr.Replace("DDANGLE", pmlVariableString);
      }
      expr = expr.Replace("DESIGN PARA", "DESP");
      char[] chArray = new char[1]{ ' ' };
      if (expr.Contains("PARA"))
        TS_ModelConverter.Tools.CleanExpression("PARA", ref expr);
      if (expr.Contains("IPAR"))
        TS_ModelConverter.Tools.CleanExpression("IPAR", ref expr);
      if (expr.Contains("OPAR"))
        TS_ModelConverter.Tools.CleanExpression("OPAR", ref expr);
      if (expr.Contains("SUM"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("SUM");
        source.Insert(index + 2, "+");
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      if (expr.Contains("DIFFERENCE"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("DIFFERENCE");
        source.Insert(index + 2, "-");
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      if (expr.StartsWith("DIFF"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("DIFF");
        source.Insert(index + 2, "-");
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      if (expr.Contains("* -"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("-");
        source.Insert(index, "(");
        source.Insert(index + 3, ")");
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      return expr;
    }

    public static string ModifyExpression(string expr, DbElement cur)
    {
      expr = expr.Replace("DESIGN PARAM", "DESP");
      expr = expr.Replace("DESIGN PARA", "DESP");
      expr = expr.Replace("TIMES", "*");
      expr = expr.Replace("ATTRIB", string.Empty);
      expr = expr.Replace("IPARAM", "IPAR");
      expr = expr.Replace("PARAM", "PARA");
      if (expr.Contains("DDRADIUS"))
      {
        Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDRADIUS");
        command.Run();
        string newValue = command.GetPMLVariableString("VALUE").Replace("mm", string.Empty);
        expr = expr.Replace("DDRADIUS", newValue);
      }
      if (expr.Contains("DDANGLE"))
      {
        Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDANGLE");
        command.Run();
        string pmlVariableString = command.GetPMLVariableString("VALUE");
        expr = expr.Replace("DDANGLE", pmlVariableString);
      }
      if (expr.Contains("DDHEIGHT"))
      {
        try
        {
          double num = cur.GetDouble((DbAttribute) DbAttributeInstance.HEIG);
          expr = expr.Replace("DDHEIGHT", Math.Round(num, 0).ToString());
        }
        catch (Exception ex)
        {
          Command command = Command.CreateCommand("VAR !!VALUE MODEL SET DDHEIGHT");
          command.Run();
          string newValue = command.GetPMLVariableString("VALUE").Replace("mm", string.Empty);
          expr = expr.Replace("DDHEIGHT", newValue);
        }
      }
      char[] chArray = new char[1]{ ' ' };
      if (expr.Contains("PARA"))
        TS_ModelConverter.Tools.CleanExpression("PARA", ref expr);
      if (expr.Contains("IPAR"))
        TS_ModelConverter.Tools.CleanExpression("IPAR", ref expr);
      if (expr.Contains("OPAR"))
        TS_ModelConverter.Tools.CleanExpression("OPAR", ref expr);
      if (expr.Contains("DESP"))
        TS_ModelConverter.Tools.CleanExpression("DESP", ref expr);
      if (expr.Contains("TWICE"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("TWICE");
        source[index] = "2*";
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
      }
      if (expr.Contains("SUM"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("SUM");
        if (source[index + 1].Equals("2*"))
        {
          source.Insert(index + 4, ")");
          source.Insert(index + 3, "+");
          source.Insert(index + 2, "(");
        }
        else
        {
          source.Insert(index + 3, ")");
          source.Insert(index + 2, "+");
          source.Insert(index + 1, "(");
        }
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      if (expr.Contains("DIFFERENCE"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("DIFFERENCE");
        source.Insert(index + 3, ")");
        source.Insert(index + 2, "-");
        source.Insert(index + 1, "(");
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      if (expr.StartsWith("DIFF"))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf("DIFF");
        source.Insert(index + 2, "-");
        source.RemoveAt(index);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
      return expr;
    }

    public static List<Point> GetProfilePoints(DbElement cur)
    {
      try
      {
        List<Point> pointList = new List<Point>();
        foreach (DbElement member1 in cur.GetElement((DbAttribute) DbAttributeInstance.GSTR).Members())
        {
          int num = member1.GetAsString((DbAttribute) DbAttributeInstance.PLAX).StartsWith("-") ? -1 : 1;
          if (TS_ModelConverter.Tools.CheckLevel(member1, 6))
          {
            if (((object) member1.GetActualType()).Equals((object) DbElementTypeInstance.SRECTANGLE))
            {
              double doubleValue1 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PX), cur);
              double doubleValue2 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PY), cur);
              double doubleValue3 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PXLE), cur);
              double doubleValue4 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PYLE), cur);
              pointList.Add(new Point(doubleValue1 - doubleValue3 / 2.0, doubleValue2 - doubleValue4 / 2.0));
              pointList.Add(new Point(doubleValue1 + doubleValue3 / 2.0, doubleValue2 - doubleValue4 / 2.0));
              pointList.Add(new Point(doubleValue1 + doubleValue3 / 2.0, doubleValue2 + doubleValue4 / 2.0));
              pointList.Add(new Point(doubleValue1 - doubleValue3 / 2.0, doubleValue2 + doubleValue4 / 2.0));
            }
            else if (((object) member1.GetActualType()).Equals((object) DbElementTypeInstance.SANNULUS))
            {
              double doubleValue1 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PX), cur);
              double doubleValue2 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PY), cur);
              double doubleValue3 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PRAD), cur);
              double doubleValue4 = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PANG), cur);
              Direction direction = TS_ModelConverter.Tools.GetDirection(member1.GetString((DbAttribute) DbAttributeInstance.PLAX), cur);
              Point point1 = new Point(doubleValue1, doubleValue2, 0.0);
              Vector vector = new Vector(direction.get_East(), direction.get_North(), direction.get_Up());
              vector.Normalize(doubleValue3);
              Point p = point1 + (Point) vector;
              Point point2 = MatrixFactory.Rotate(Math.PI * doubleValue4 / 180.0, new Vector(0.0, 0.0, 1.0)).Transform(p);
              pointList.Add(p);
              pointList.Add(point2);
            }
            else
            {
              foreach (DbElement member2 in member1.Members())
              {
                double X = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PX), cur) * (double) num;
                double Y = TS_ModelConverter.Tools.GetDoubleValue(member1.GetString((DbAttribute) DbAttributeInstance.PY), cur) * (double) num;
                pointList.Add(new Point(X, Y));
              }
            }
          }
        }
        return pointList;
      }
      catch
      {
        TS_ModelConverter.Tools.Log.Error((object) "Error finding section points.");
      }
      return (List<Point>) null;
    }

    public static List<Point> GetProfilePoints(DbElement cur, DbElement modelItem)
    {
      try
      {
        List<Point> pointList1 = new List<Point>();
        string str = cur.GetString((DbAttribute) DbAttributeInstance.GTYP);
        DbElement[] dbElementArray1 = cur.GetElement((DbAttribute) DbAttributeInstance.GSTR).Members();
        bool flag = false;
        foreach (DbElement cur1 in dbElementArray1)
        {
          if (TS_ModelConverter.Tools.CheckLevel(cur1, 6))
          {
            if (((object) cur1.GetActualType()).Equals((object) DbElementTypeInstance.SRECTANGLE))
            {
              flag = true;
              double num1 = 0.0;
              double num2 = 0.0;
              double num3 = 0.0;
              double num4 = 0.0;
              DbExpression dbExpression1 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PX));
              if (dbExpression1 != null)
                num1 = Math.Round(((object) dbExpression1).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression2 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PY));
              if (dbExpression2 != null)
                num2 = Math.Round(((object) dbExpression2).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression3 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PXLE));
              if (dbExpression3 != null)
                num3 = Math.Round(((object) dbExpression3).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression3, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression3, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression4 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PYLE));
              if (dbExpression4 != null)
                num4 = Math.Round(((object) dbExpression4).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression4, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression4, (DbAttributeUnit) 939199), 2);
              pointList1.Add(new Point(num1 - num3 / 2.0, num2 - num4 / 2.0));
              pointList1.Add(new Point(num1 + num3 / 2.0, num2 - num4 / 2.0));
              pointList1.Add(new Point(num1 + num3 / 2.0, num2 + num4 / 2.0));
              pointList1.Add(new Point(num1 - num3 / 2.0, num2 + num4 / 2.0));
            }
            else if (((object) cur1.GetActualType()).Equals((object) DbElementTypeInstance.SANNULUS))
            {
              double X = 0.0;
              double Y = 0.0;
              double NewLength = 0.0;
              double num = 0.0;
              DbExpression dbExpression1 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PX));
              if (dbExpression1 != null)
                X = Math.Round(((object) dbExpression1).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression2 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PY));
              if (dbExpression2 != null)
                Y = Math.Round(((object) dbExpression2).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression3 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PRAD));
              if (dbExpression3 != null)
                NewLength = Math.Round(((object) dbExpression3).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression3, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression3, (DbAttributeUnit) 939199), 2);
              DbExpression dbExpression4 = DbExpression.Parse(cur1.GetString((DbAttribute) DbAttributeInstance.PANG));
              if (dbExpression4 != null)
                num = Math.Round(((object) dbExpression4).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression4, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression4, (DbAttributeUnit) 939199), 2);
              Direction direction = TS_ModelConverter.Tools.GetDirection(cur1.GetAsString((DbAttribute) DbAttributeInstance.PLAX), cur);
              Point point1 = new Point(X, Y, 0.0);
              Vector vector = new Vector(direction.get_East(), direction.get_North(), direction.get_Up());
              vector.Normalize(NewLength);
              Point p = point1 + (Point) vector;
              Point point2 = MatrixFactory.Rotate(Math.PI * num / 180.0, new Vector(0.0, 0.0, 1.0)).Transform(p);
              pointList1.Add(p);
              pointList1.Add(point2);
            }
            else if (((object) cur1.GetActualType()).Equals((object) DbElementTypeInstance.SPROFILE))
            {
              DbElement[] dbElementArray2 = cur1.Members();
              string asString = cur1.GetAsString(DbAttribute.GetDbAttribute("PLAXIS"));
              Position position;
              Direction direction;
              TS_ModelConverter.Tools.GetPosDir(modelItem, asString, out position, out direction);
              double num1 = asString.StartsWith("-") ? -1.0 : 1.0;
              double num2 = asString.StartsWith("-") ? -1.0 : 1.0;
              foreach (DbElement dbElement in dbElementArray2)
              {
                double num3 = 0.0;
                double num4 = 0.0;
                DbExpression dbExpression1 = DbExpression.Parse(dbElement.GetString((DbAttribute) DbAttributeInstance.PX));
                if (dbExpression1 != null)
                  num3 = Math.Round(((object) dbExpression1).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression1, (DbAttributeUnit) 939199), 2);
                DbExpression dbExpression2 = DbExpression.Parse(dbElement.GetString((DbAttribute) DbAttributeInstance.PY));
                if (dbExpression2 != null)
                  num4 = Math.Round(((object) dbExpression2).ToString().Contains("DESP") ? modelItem.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199) : cur.EvaluateDouble(dbExpression2, (DbAttributeUnit) 939199), 2);
                pointList1.Add(new Point(num3 * num1, num4 * num2, 0.0));
              }
            }
          }
        }
        if (flag && str.Equals("WANG"))
        {
          List<Point> pointList2 = new List<Point>();
          pointList2.Add(pointList1[0]);
          pointList2.Add(pointList1[1]);
          pointList2.Add(pointList1[4]);
          pointList2.Add(pointList1[5]);
          pointList2.Add(pointList1[6]);
          pointList2.Add(pointList1[3]);
          pointList2.Add(pointList1[0]);
          pointList1.Clear();
          pointList1.AddRange((IEnumerable<Point>) pointList2);
        }
        return pointList1;
      }
      catch
      {
        TS_ModelConverter.Tools.Log.Error((object) "Error finding section points.");
      }
      return (List<Point>) null;
    }

    public static void GetPosDir(
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
        DbExpression dbExpression2 = DbExpression.Parse(key.Substring(startIndex) + " DIR WRT WORLD");
        if (dbExpression2 == null)
          return;
        direction = element.EvaluateDirection(dbExpression2);
        if (startIndex.Equals(1))
          direction = direction.Opposite();
      }
      else
      {
        direction = Direction.Create(key);
        DbExpression dbExpression1 = DbExpression.Parse("POS WRT WORLD");
        if (dbExpression1 != null)
          position = element.EvaluatePosition(dbExpression1);
        Orientation orientation = Orientation.Create();
        DbExpression dbExpression2 = DbExpression.Parse("ORI WRT WORLD");
        if (dbExpression2 != null)
          orientation = element.EvaluateOrientation(dbExpression2);
        CoordinateSystem CoordSys = new CoordinateSystem();
        CoordSys.Origin = new Point(position.get_X(), position.get_Y(), position.get_Z());
        CoordSys.AxisX = new Vector(orientation.XDir().get_East(), orientation.XDir().get_North(), orientation.XDir().get_Up());
        CoordSys.AxisY = new Vector(orientation.YDir().get_East(), orientation.YDir().get_North(), orientation.YDir().get_Up());
        Matrix matrix = MatrixFactory.FromCoordinateSystem(CoordSys);
        Vector vector1 = new Vector(direction.get_East(), direction.get_North(), direction.get_Up());
        vector1.Normalize(1000.0);
        Vector vector2 = new Vector(matrix.Transform(new Point(vector1.X, vector1.Y, vector1.Z)) - CoordSys.Origin);
        direction = Direction.Create(Position.Create(vector2.X, vector2.Y, vector2.Z));
      }
    }

    public static double GetCatalogueValue(DbElement component, string expressionString)
    {
      string str = expressionString;
      double num = 0.0;
      try
      {
        expressionString = TS_ModelConverter.Tools.ModifyExpression(expressionString, component);
        DbExpression dbExpression = DbExpression.Parse(expressionString);
        if (dbExpression != null)
          num = Math.Round(component.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
        else
          TS_ModelConverter.Tools.Log.Error((object) ("Could not parse expression: " + str));
      }
      catch
      {
        TS_ModelConverter.Tools.Log.Error((object) ("Unable to evaluate expression: " + str));
      }
      return num;
    }

    public static Matrix CreateGlobalMatrix(Primitive primitive)
    {
      return MatrixFactory.FromCoordinateSystem(new CoordinateSystem(new Point(primitive.Position.get_X(), primitive.Position.get_Y(), primitive.Position.get_Z()), new Vector(primitive.Orientation.XDir().get_East(), primitive.Orientation.XDir().get_North(), primitive.Orientation.XDir().get_Up()), new Vector(primitive.Orientation.YDir().get_East(), primitive.Orientation.YDir().get_North(), primitive.Orientation.YDir().get_Up())));
    }

    public static Position GetPosition(string expressionString, DbElement cur)
    {
      Position position = Position.Create();
      DbExpression dbExpression = DbExpression.Parse(expressionString);
      if (dbExpression != null)
        position = cur.EvaluatePosition(dbExpression);
      return position;
    }

    public static Orientation GetOrientation(string expressionString, DbElement cur)
    {
      Orientation orientation = Orientation.Create();
      DbExpression dbExpression = DbExpression.Parse(expressionString);
      if (dbExpression != null)
        orientation = cur.EvaluateOrientation(dbExpression);
      return orientation;
    }

    public static Direction GetDirection(string expressionString, DbElement cur)
    {
      Direction direction = Direction.Create();
      int startIndex = expressionString.StartsWith("-") ? 1 : 0;
      DbExpression dbExpression = DbExpression.Parse(expressionString.Substring(startIndex));
      if (dbExpression != null)
      {
        direction = cur.EvaluateDirection(dbExpression);
        if (startIndex.Equals(1))
          direction = direction.Opposite();
      }
      return direction;
    }

    private static void CleanExpression(string str, ref string expr)
    {
      char[] chArray = new char[1]{ ' ' };
      while (expr.Contains(str + " "))
      {
        string[] strArray = expr.Split(chArray);
        List<string> source = new List<string>();
        source.AddRange((IEnumerable<string>) strArray);
        int index = source.IndexOf(str);
        source[index] = str + "[" + source[index + 1] + "]";
        source.RemoveAt(index + 1);
        expr = source.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, s) => current + " " + s));
        expr = expr.Substring(1);
      }
    }

    public static void CleanPolygonPoints(ref List<Point> points, ref List<double> chamfers)
    {
      List<Point> pointList = new List<Point>();
      List<double> doubleList = new List<double>();
      for (int index = 0; index < points.Count; ++index)
      {
        if (!pointList.Contains(points[index]))
        {
          pointList.Add(points[index]);
          doubleList.Add(chamfers[index]);
        }
      }
      points.Clear();
      chamfers.Clear();
      points.AddRange((IEnumerable<Point>) pointList);
      chamfers.AddRange((IEnumerable<double>) doubleList);
      pointList.Clear();
      doubleList.Clear();
      for (int index1 = 0; index1 < points.Count; ++index1)
      {
        int index2 = index1 > 0 ? index1 - 1 : points.Count - 1;
        int index3 = index1 > points.Count - 1 ? 0 : index1;
        int index4 = index1 < points.Count - 1 ? index1 + 1 : 0;
        Position position1 = Position.Create(points[index2].X, points[index2].Y, points[index2].Z);
        Position position2 = Position.Create(points[index3].X, points[index3].Y, points[index3].Z);
        Position position3 = Position.Create(points[index4].X, points[index4].Y, points[index4].Z);
        Direction direction1 = Line.Create(position1, position2).Direction();
        Direction direction2 = Line.Create(position2, position3).Direction();
        if (direction1.IsParallel(direction2) || direction1.IsAntiParallel(direction2))
        {
          if (!chamfers[1].Equals(0.0))
          {
            pointList.Add(points[index1]);
            doubleList.Add(chamfers[index1]);
          }
        }
        else
        {
          pointList.Add(points[index1]);
          doubleList.Add(chamfers[index1]);
        }
      }
      points.Clear();
      chamfers.Clear();
      points.AddRange((IEnumerable<Point>) pointList);
      chamfers.AddRange((IEnumerable<double>) doubleList);
      pointList.Clear();
      doubleList.Clear();
      if (!TS_ModelConverter.Tools.IsPolygonClockWise(points))
        return;
      for (int index = points.Count - 1; index >= 0; --index)
      {
        pointList.Add(points[index]);
        doubleList.Add(chamfers[index]);
      }
      points.Clear();
      chamfers.Clear();
      points.AddRange((IEnumerable<Point>) pointList);
      chamfers.AddRange((IEnumerable<double>) doubleList);
      pointList.Clear();
      doubleList.Clear();
    }

    public static bool CheckLevel(DbElement cur, int level)
    {
      try
      {
        int[] integerArray = cur.GetIntegerArray((DbAttribute) DbAttributeInstance.LEVE);
        string obstruction = TS_ModelConverter.Tools.GetObstruction();
        string insulation = TS_ModelConverter.Tools.GetInsulation();
        if (integerArray[0] > level || integerArray[1] < level)
        {
          if (level != 6)
            return false;
          if (obstruction == "On" && integerArray[0] >= 8 && integerArray[1] <= 10)
          {
            TS_ModelConverter.Tools.Log.Info((object) ("Obstruct On, Object " + cur.GetAsString((DbAttribute) DbAttributeInstance.REF) + ", levels " + (object) integerArray[0] + ", " + (object) integerArray[1] + " passed level 8-10"));
            return true;
          }
          if (!(insulation == "On") || integerArray[0] < 7 || integerArray[1] > 10)
            return false;
          TS_ModelConverter.Tools.Log.Info((object) ("Insulation On, Object " + cur.GetAsString((DbAttribute) DbAttributeInstance.REF) + ", levels " + (object) integerArray[0] + ", " + (object) integerArray[1] + " passed level 7-10"));
          return true;
        }
        Exception exception = (Exception) null;
        bool flag = TS_ModelConverter.Tools.GetBool(cur, (DbAttribute) DbAttributeInstance.TUFL, nameof (CheckLevel), ref exception) || obstruction == "On";
        if (exception != null)
          throw exception;
        return flag;
      }
      catch
      {
        return true;
      }
    }

    public static string GetObstruction()
    {
      Command command = Command.CreateCommand("VAR !!OBSTRUCT REPRE OBSTRUCT");
      command.Run();
      return command.GetPMLVariableString("OBSTRUCT");
    }

    public static string GetInsulation()
    {
      Command command = Command.CreateCommand("VAR !!INSULATION REPRE INSU");
      command.Run();
      return command.GetPMLVariableString("INSULATION");
    }

    public static bool CheckObstruction(DbElement cur, int obstruction)
    {
      try
      {
        return !cur.GetInteger((DbAttribute) DbAttributeInstance.OBST).Equals(obstruction);
      }
      catch
      {
        return true;
      }
    }

    public static bool CheckOpened(string name, out Form form)
    {
      foreach (Form openForm in (ReadOnlyCollectionBase) Application.OpenForms)
      {
        if (openForm.Text == name)
        {
          form = openForm;
          return true;
        }
      }
      form = (Form) null;
      return false;
    }

    public static Point Round(Point p)
    {
      return new Point(Math.Round(p.X, 1), Math.Round(p.Y, 1), Math.Round(p.Z, 1));
    }

    public static CoordinateSystem CreateStruCoord(DbElement stru)
    {
      Position position = stru.GetPosition((DbAttribute) DbAttributeInstance.POS, TS_ModelConverter.Tools.WrtQualifyer);
      Orientation orientation = stru.GetOrientation((DbAttribute) DbAttributeInstance.ORI, TS_ModelConverter.Tools.WrtQualifyer);
      Direction direction1 = orientation.XDir();
      Direction direction2 = orientation.YDir();
      return new CoordinateSystem()
      {
        Origin = new Point(position.get_X(), position.get_Y(), position.get_Z()),
        AxisX = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up()),
        AxisY = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up())
      };
    }

    public static bool SwapHandlesCheck(IfcObjectGeometryData geometryData)
    {
      string str = geometryData.ObjectType?.ToLower() ?? string.Empty;
      int num = 1;
      if (geometryData.ExtusionDirIfc.X < 0.0 || geometryData.ExtusionDirIfc.Y < 0.0 || geometryData.ExtusionDirIfc.Z < 0.0 || str == "column" && geometryData.EndPointTS.Z < geometryData.StartPointTS.Z)
        num = -1;
      if (geometryData.ProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcUShapeProfileDef || geometryData.ProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcLShapeProfileDef || (geometryData.ProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcZShapeProfileDef || geometryData.DetectedProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcUShapeProfileDef) || geometryData.DetectedProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcLShapeProfileDef || geometryData.DetectedProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcZShapeProfileDef || geometryData.ProfileEntityType == IFCObjectsReader.Data.IfcObject.EntityTypeEnum.IfcRectangleProfileDef && geometryData.ObjectType != null && str == "slab")
        return true;
      return num == -1 && (geometryData.ExtrusionItem.HasParametricProfile || str == "tapered");
    }

    public static Orientation CreateOrientation(Vector vX, Vector vY)
    {
      Vector v = vX.Cross(vY);
      v.Normalize();
      return Orientation.Create("Y is " + ((object) TS_ModelConverter.Tools.CreateDirection(vY)).ToString() + " and Z is " + ((object) TS_ModelConverter.Tools.CreateDirection(v)).ToString());
    }

    public static Direction CreateDirection(Vector v)
    {
      D3Vector d3Vector1 = D3Vector.Create(v.X, v.Y, v.Z);
      D3Vector d3Vector2 = D3Vector.Create(v.X, v.Y, v.Z);
      d3Vector2.ScaleBy(10.0);
      return Direction.Create(Position.Create(d3Vector1.get_X(), d3Vector1.get_Y(), d3Vector1.get_Z()), Position.Create(d3Vector2.get_X(), d3Vector2.get_Y(), d3Vector2.get_Z()));
    }

    public static bool AdjustAlignment(DbElement spre, ref Point alignment)
    {
      bool flag = false;
      if (!spre.get_IsNull())
      {
        foreach (DbElement member in spre.GetElement((DbAttribute) DbAttributeInstance.DTRE).Members())
        {
          string asString = member.GetAsString((DbAttribute) DbAttributeInstance.DKEY);
          if (asString.Equals("XOFF"))
          {
            DbExpression dbExpression = DbExpression.Parse(member.GetAsString((DbAttribute) DbAttributeInstance.PPRO));
            alignment.X = Math.Round(spre.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
            flag = true;
          }
          else if (asString.Equals("YOFF"))
          {
            DbExpression dbExpression = DbExpression.Parse(member.GetAsString((DbAttribute) DbAttributeInstance.PPRO));
            alignment.Y = Math.Round(spre.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
            flag = true;
          }
        }
      }
      return flag;
    }

    public static bool GetDtre(DbElement spre, string name, out double value)
    {
      bool flag = false;
      value = 0.0;
      if (!spre.get_IsNull())
      {
        foreach (DbElement member in spre.GetElement((DbAttribute) DbAttributeInstance.DTRE).Members())
        {
          if (member.GetAsString((DbAttribute) DbAttributeInstance.DKEY).Equals(name))
          {
            DbExpression dbExpression = DbExpression.Parse(member.GetAsString((DbAttribute) DbAttributeInstance.PPRO));
            value = Math.Round(spre.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
            flag = true;
          }
        }
      }
      return flag;
    }

    public static void CreatePosOri(DbElement newElement, string posString, string oriString)
    {
      DbElement element = CurrentElement.get_Element();
      CurrentElement.set_Element(newElement);
      Command.CreateCommand("POS " + posString).RunInPdms();
      Command.CreateCommand("ORI " + oriString).RunInPdms();
      CurrentElement.set_Element(element);
    }

    public static Point GetCenterPoint(List<Point> points)
    {
      Point MinPoint = new Point(points[0]);
      Point MaxPoint = new Point(points[0]);
      foreach (Point point in points)
      {
        if (point.X < MinPoint.X)
          MinPoint.X = point.X;
        if (point.Y < MinPoint.Y)
          MinPoint.Y = point.Y;
        if (point.Z < MinPoint.Z)
          MinPoint.Z = point.Z;
        if (point.X > MaxPoint.X)
          MaxPoint.X = point.X;
        if (point.Y > MaxPoint.Y)
          MaxPoint.Y = point.Y;
        if (point.Z > MaxPoint.Z)
          MaxPoint.Z = point.Z;
      }
      return new AABB(MinPoint, MaxPoint).GetCenterPoint();
    }

    public static List<Point> GetCenterPoint(
      Point p1,
      Point p2,
      Point p3,
      double radius,
      out Point center)
    {
      List<Point> pointList = new List<Point>();
      Vector vector1 = new Vector(p1 - p2);
      vector1.Normalize(Math.Abs(radius));
      Vector Vector = new Vector(p3 - p2);
      Vector.Normalize(Math.Abs(radius));
      Point point1 = new Point(p2 + (Point) vector1);
      Point point2 = new Point(p2 + (Point) Vector);
      double a = vector1.GetAngleBetween(Vector) / 2.0;
      if (radius < 0.0)
      {
        center = new Point(p2);
        pointList.Add(new Point(point1));
        pointList.Add(new Point(point2));
      }
      else
      {
        double NewLength1 = Distance.PointToPoint(point1, point2) / 2.0;
        Vector vector2 = new Vector(point2 - point1);
        vector2.Normalize(NewLength1);
        Vector vector3 = new Vector(new Point(point1 + (Point) vector2) - p2);
        double NewLength2 = Math.Abs(radius) / Math.Sin(a);
        vector3.Normalize(NewLength2);
        double NewLength3 = Math.Abs(radius) / Math.Tan(a);
        vector1.Normalize(NewLength3);
        Vector.Normalize(NewLength3);
        center = new Point(p2 + (Point) vector3);
        pointList.Add(new Point(p2 + (Point) vector1));
        pointList.Add(new Point(p2 + (Point) Vector));
      }
      return pointList;
    }

    public static List<Point> CreateCornerPoints(AABB vol, Matrix matrix)
    {
      return new List<Point>()
      {
        vol.MinPoint,
        new Point(vol.MinPoint.X, vol.MaxPoint.Y, vol.MinPoint.Z),
        new Point(vol.MaxPoint.X, vol.MaxPoint.Y, vol.MinPoint.Z),
        new Point(vol.MaxPoint.X, vol.MinPoint.Y, vol.MinPoint.Z),
        new Point(vol.MinPoint.X, vol.MinPoint.Y, vol.MaxPoint.Z),
        new Point(vol.MinPoint.X, vol.MaxPoint.Y, vol.MaxPoint.Z),
        vol.MaxPoint,
        new Point(vol.MaxPoint.X, vol.MinPoint.Y, vol.MaxPoint.Z)
      }.Select<Point, Point>((Func<Point, Point>) (p => matrix.Transform(p))).ToList<Point>();
    }

    public static List<GeometricPlane> CreatePlanes(AABB vol, string type)
    {
      List<GeometricPlane> geometricPlaneList = new List<GeometricPlane>();
      GeometricPlane geometricPlane1 = new GeometricPlane(vol.MinPoint, new Vector(0.0, 0.0, -1.0));
      GeometricPlane geometricPlane2 = new GeometricPlane(vol.MaxPoint, new Vector(0.0, 0.0, 1.0));
      GeometricPlane geometricPlane3 = new GeometricPlane(vol.MinPoint, new Vector(-1.0, 0.0, 0.0));
      GeometricPlane geometricPlane4 = new GeometricPlane(vol.MaxPoint, new Vector(1.0, 0.0, 0.0));
      GeometricPlane geometricPlane5 = new GeometricPlane(vol.MaxPoint, new Vector(0.0, 1.0, 0.0));
      GeometricPlane geometricPlane6 = new GeometricPlane(vol.MinPoint, new Vector(0.0, -1.0, 0.0));
      if (type.Equals("END"))
      {
        geometricPlaneList.Add(geometricPlane1);
        geometricPlaneList.Add(geometricPlane2);
      }
      else if (type.Equals("SIDES"))
      {
        geometricPlaneList.Add(geometricPlane3);
        geometricPlaneList.Add(geometricPlane4);
        geometricPlaneList.Add(geometricPlane5);
        geometricPlaneList.Add(geometricPlane6);
      }
      else
      {
        geometricPlaneList.Add(geometricPlane1);
        geometricPlaneList.Add(geometricPlane3);
        geometricPlaneList.Add(geometricPlane4);
        geometricPlaneList.Add(geometricPlane5);
        geometricPlaneList.Add(geometricPlane6);
        geometricPlaneList.Add(geometricPlane2);
      }
      return geometricPlaneList;
    }

    public static List<LineSegment> CreateLineSegments(List<Point> points)
    {
      return new List<LineSegment>()
      {
        new LineSegment(points[0], points[1]),
        new LineSegment(points[1], points[2]),
        new LineSegment(points[2], points[3]),
        new LineSegment(points[3], points[0]),
        new LineSegment(points[4], points[5]),
        new LineSegment(points[5], points[6]),
        new LineSegment(points[6], points[7]),
        new LineSegment(points[7], points[4]),
        new LineSegment(points[0], points[4]),
        new LineSegment(points[1], points[5]),
        new LineSegment(points[2], points[6]),
        new LineSegment(points[3], points[7])
      };
    }

    public static AABB CreateCutVolume(
      Matrix matrix,
      List<Point> cutPoints,
      List<Point> cornerPoints)
    {
      List<Point> list = cutPoints.Select<Point, Point>((Func<Point, Point>) (p => matrix.Transform(p))).ToList<Point>();
      list.AddRange(cornerPoints.Select<Point, Point>((Func<Point, Point>) (p => matrix.Transform(p))).Where<Point>((Func<Point, bool>) (p1 => p1.Z > 0.0)));
      Point MinPoint = new Point(list[0].X, list[0].Y, list[0].Z);
      Point MaxPoint = new Point(list[0].X, list[0].Y, list[0].Z);
      foreach (Point point in list)
      {
        if (point.X < MinPoint.X)
          MinPoint.X = point.X;
        if (point.Y < MinPoint.Y)
          MinPoint.Y = point.Y;
        if (point.Z < MinPoint.Z)
          MinPoint.Z = point.Z;
        if (point.X > MaxPoint.X)
          MaxPoint.X = point.X;
        if (point.Y > MaxPoint.Y)
          MaxPoint.Y = point.Y;
        if (point.Z > MaxPoint.Z)
          MaxPoint.Z = point.Z;
      }
      return new AABB(MinPoint, MaxPoint);
    }

    public static void CreatePosOri(
      DbElement newElement,
      Position pos,
      Orientation ori,
      DbElement owner)
    {
      Direction direction1 = ori.XDir();
      Direction direction2 = ori.YDir();
      Point Origin = new Point(pos.get_X(), pos.get_Y(), pos.get_Z());
      Vector AxisX = new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up());
      Vector AxisY = new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up());
      CoordinateSystem CoordSys1 = new CoordinateSystem(Origin, AxisX, AxisY);
      Position position = owner.GetPosition((DbAttribute) DbAttributeInstance.POS, 0, ((MDB) MDB.CurrentMDB).GetFirstWorld((DbType) 1));
      Orientation orientation = owner.GetOrientation((DbAttribute) DbAttributeInstance.ORI, 0, ((MDB) MDB.CurrentMDB).GetFirstWorld((DbType) 1));
      Direction direction3 = orientation.XDir();
      Direction direction4 = orientation.YDir();
      CoordinateSystem CoordSys2 = new CoordinateSystem(new Point(position.get_X(), position.get_Y(), position.get_Z()), new Vector(direction3.get_East(), direction3.get_North(), direction3.get_Up()), new Vector(direction4.get_East(), direction4.get_North(), direction4.get_Up()));
      Matrix matrix = MatrixFactory.ByCoordinateSystems(CoordSys1, CoordSys2);
      Point p = new Point(0.0, 0.0, 0.0);
      Point point1 = matrix.Transform(p);
      AxisX.Normalize(1000.0);
      AxisY.Normalize(1000.0);
      Point point2 = matrix.Transform(new Point(0.0, 0.0, 0.0));
      Point point3 = matrix.Transform(new Point(1000.0, 0.0, 0.0));
      Point point4 = matrix.Transform(new Point(0.0, 1000.0, 0.0));
      Direction direction5 = Direction.Create(Position.Create(point2.X, point2.Y, point2.Z), Position.Create(point3.X, point3.Y, point3.Z));
      Direction direction6 = Direction.Create(Position.Create(point2.X, point2.Y, point2.Z), Position.Create(point4.X, point4.Y, point4.Z));
      TS_ModelConverter.Tools.SetAttribute(newElement, (DbAttribute) DbAttributeInstance.POS, Position.Create(point1.X, point1.Y, point1.Z));
      TS_ModelConverter.Tools.SetAttribute(newElement, (DbAttribute) DbAttributeInstance.ORI, Orientation.Create(direction5, direction6));
    }

    public static void CreatePosOri(DbElement newElement, Position pos, Orientation ori)
    {
      string str1 = pos.ToString() + " WRT WORLD";
      string str2 = ori.ToString() + " WRT WORLD";
      Command.CreateCommand("!!ITEM = " + ((object) newElement).ToString()).RunInPdms();
      Command.CreateCommand("!!ITEM.POS = " + str1).RunInPdms();
      Command.CreateCommand("!!ITEM.ORI = " + str2).RunInPdms();
    }

    public static void CleanPolygon(ref List<double> chamfers, ref List<Point> points)
    {
      SortedList sortedList1 = new SortedList();
      SortedList sortedList2 = new SortedList();
      if (chamfers.Count.Equals(0))
      {
        chamfers.Clear();
        chamfers.AddRange(points.Select<Point, int>((Func<Point, int>) (p => 0)).Select<int, double>((Func<int, double>) (dummy => (double) dummy)));
      }
      else
      {
        int count = chamfers.Count;
        if (count.Equals(1))
        {
          double num = Math.Round(chamfers[0], 2);
          Point point = points[0];
          chamfers.Clear();
          chamfers.Add(num);
          chamfers.Add(num);
          chamfers.Add(num);
          chamfers.Add(num);
          points.Clear();
          points.Add(new Point(point.X - num, point.Y - num, point.Z));
          points.Add(new Point(point.X - num, point.Y + num, point.Z));
          points.Add(new Point(point.X + num, point.Y + num, point.Z));
          points.Add(new Point(point.X + num, point.Y - num, point.Z));
        }
        else
        {
          count = chamfers.Count;
          if (!count.Equals(points.Count))
            return;
          for (int index1 = 0; index1 < chamfers.Count; ++index1)
          {
            if (chamfers[index1] > 0.0)
              chamfers[index1] = -1.0 * (chamfers[index1] - 0.1);
            else if (chamfers[index1] < 0.0)
            {
              chamfers[index1] = -1.0 * (chamfers[index1] + 0.1);
              int index2 = index1 > 0 ? index1 - 1 : chamfers.Count - 1;
              int index3 = index2 > 0 ? index2 - 1 : chamfers.Count - 1;
              int index4 = index1 >= chamfers.Count - 1 ? 0 : index1 + 1;
              int index5 = index4 >= chamfers.Count - 1 ? 0 : index4 + 1;
              Vector vector1 = new Vector(points[index2] - points[index1]);
              Vector Vector1 = new Vector(points[index4] - points[index1]);
              if (vector1.GetAngleBetween(Vector1) > Math.PI / 2.0)
              {
                Vector Axis = new Vector(points[index3] - points[index2]).Cross(Vector1);
                Point Point = MatrixFactory.Rotate(vector1.GetAngleBetween(Vector1) / 2.0, Axis).Transform(new Point((Point) vector1));
                Vector Normal = new Vector(Point);
                GeometricPlane Plane = new GeometricPlane(points[index1] + Point, Normal);
                Line Line1 = new Line(points[index3], points[index2]);
                Line Line2 = new Line(points[index5], points[index4]);
                Point plane1 = Intersection.LineToPlane(Line1, Plane);
                Point plane2 = Intersection.LineToPlane(Line2, Plane);
                sortedList1.Add((object) index1, (object) index1);
                if (plane1 != (Point) null)
                {
                  points[index2] = plane1;
                  sortedList2.Add((object) index2, (object) chamfers[index1]);
                }
                if (!(plane2 == (Point) null))
                {
                  points[index4] = plane2;
                  sortedList2.Add((object) index4, (object) chamfers[index1]);
                }
              }
              else
              {
                Line Line = new Line(points[index3], points[index2]);
                Vector vector2 = new Vector(points[index2] - points[index3]);
                Vector Vector2 = new Vector(points[index5] - points[index4]);
                Vector Normal = vector2.Cross(Vector2).Cross(Vector2);
                GeometricPlane Plane = new GeometricPlane(points[index4], Normal);
                Point plane = Intersection.LineToPlane(Line, Plane);
                if (plane != (Point) null)
                  points[index1] = plane;
              }
            }
          }
          try
          {
            for (int index = 0; index < sortedList2.Count; ++index)
            {
              int key = (int) sortedList2.GetKey(index);
              double num = (double) sortedList2.GetValueList()[index];
              chamfers[key] = num;
            }
          }
          catch
          {
          }
          try
          {
            for (int index = sortedList1.Count - 1; index >= 0; --index)
            {
              int key = (int) sortedList1.GetKey(index);
              chamfers.RemoveAt(key);
              points.RemoveAt(key);
            }
          }
          catch
          {
          }
        }
      }
    }

    public static void CleanPolygonImport(ref List<double> chamfers, ref List<Point> points)
    {
      if (!chamfers.Any<double>((Func<double, bool>) (x => !x.Equals(0.0))))
        return;
      List<Point> pointList = new List<Point>();
      List<double> doubleList = new List<double>();
      for (int index = 0; index < chamfers.Count; ++index)
      {
        if (chamfers[index].Equals(0.0))
        {
          pointList.Add(points[index]);
          doubleList.Add(Math.Abs(chamfers[index]));
        }
        else
        {
          Point Point1 = index.Equals(0) ? points[points.Count - 1] : points[index - 1];
          Point point = points[index];
          Point Point2 = index.Equals(points.Count - 1) ? points[0] : points[index + 1];
          Arc arc = Arc.Create(Position.Create(Point1.X, Point1.Y, Point1.Z), Position.Create(point.X, point.Y, point.Z), Position.Create(Point2.X, Point2.Y, Point2.Z));
          Vector Direction1 = new Vector(arc.StartTangent().get_East(), arc.StartTangent().get_North(), arc.StartTangent().get_Up());
          Direction1.Normalize();
          Vector Direction2 = new Vector(-arc.EndTangent().get_East(), -arc.EndTangent().get_North(), -arc.EndTangent().get_Up());
          Direction2.Normalize();
          if (arc.get_EndAngle() - arc.get_StartAngle() < 120.0)
          {
            Point point1 = Intersection.LineToLine(new Line(Point1, Direction1), new Line(Point2, Direction2)).Point1;
            pointList.Add(point1);
            doubleList.Add(Math.Abs(chamfers[index]));
          }
          else
          {
            double num = arc.get_StartAngle() + (arc.get_EndAngle() - arc.get_StartAngle()) / 2.0;
            Position position = arc.Position(num);
            Direction direction = arc.AngleTangent(num);
            Point Point3 = new Point(position.get_X(), position.get_Y(), position.get_Z());
            Vector Direction3 = new Vector(direction.get_East(), direction.get_North(), direction.get_Up());
            Point point1_1 = Intersection.LineToLine(new Line(Point1, Direction1), new Line(Point3, new Vector((Point) (-1.0 * Direction3)))).Point1;
            pointList.Add(point1_1);
            doubleList.Add(Math.Abs(chamfers[index]));
            Point point1_2 = Intersection.LineToLine(new Line(Point3, Direction3), new Line(Point2, Direction2)).Point1;
            pointList.Add(point1_2);
            doubleList.Add(Math.Abs(chamfers[index]));
          }
        }
      }
      points.Clear();
      chamfers.Clear();
      points.AddRange((IEnumerable<Point>) pointList);
      chamfers.AddRange((IEnumerable<double>) doubleList);
    }

    public static string GetProperty(IFCObjectsReader.Data.IfcObject ifcObj, string propertyName)
    {
      foreach (KeyValuePair<string, Dictionary<string, IfcObjectPropertySetValue>> propertySetValue in ifcObj.ObjectPropertySetValues)
      {
        foreach (KeyValuePair<string, IfcObjectPropertySetValue> keyValuePair in propertySetValue.Value)
        {
          if (keyValuePair.Key.Equals(propertyName))
          {
            if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Integer))
              return keyValuePair.Value.IntegerValue.ToString();
            if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.String))
              return keyValuePair.Value.StringValue;
            if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Double))
              return keyValuePair.Value.RealValue.ToString((IFormatProvider) CultureInfo.CurrentCulture);
          }
        }
      }
      return string.Empty;
    }

    public static void CreateUda(
      DbElement cur,
      IIfcObject ifcObj,
      Mapping mapping,
      Discipline ownerDiscipline)
    {
      Dictionary<string, object> attributes = new Dictionary<string, object>();
      foreach (DataRow row in (InternalDataCollectionBase) mapping.UdaDs.Tables[0].Rows)
      {
        Discipline discipline = (Discipline) row[0];
        HierarchyLevel hierarchy = (HierarchyLevel) row[1];
        string str1 = row[2].ToString();
        DbElement hierarchyObject = TS_ModelConverter.Tools.GetHierarchyObject(cur, hierarchy);
        if (!DbElement.op_Equality(hierarchyObject, (DbElement) null) && ownerDiscipline.Equals((object) discipline))
        {
          DbAttribute dbAttribute = DbAttribute.GetDbAttribute(str1);
          if (DbAttribute.op_Equality(dbAttribute, (DbAttribute) null))
          {
            try
            {
              Command.CreateCommand("VAR !!VALUE ''").Run();
              string asString = hierarchyObject.GetAsString((DbAttribute) DbAttributeInstance.NAME);
              Command command = Command.CreateCommand("VAR !!VALUE " + str1 + " OF " + asString);
              command.Run();
              string pmlVariableString = command.GetPMLVariableString("VALUE");
              if (!string.IsNullOrEmpty(pmlVariableString))
              {
                string str2 = "PDMS " + (object) discipline;
                string key = hierarchy.Equals((object) HierarchyLevel.Object) ? str1 : ((int) hierarchy).ToString() + " " + str1;
                attributes.Add(key, (object) (IfcLabel) pmlVariableString);
              }
            }
            catch
            {
            }
          }
          if (DbAttribute.op_Inequality(dbAttribute, (DbAttribute) null))
          {
            try
            {
              string asString = hierarchyObject.GetAsString(DbAttribute.GetDbAttribute(str1));
              string str2 = "PDMS " + (object) discipline;
              string key = hierarchy.Equals((object) HierarchyLevel.Object) ? str1 : ((int) hierarchy).ToString() + " " + str1;
              attributes.Add(key, (object) (IfcLabel) asString);
            }
            catch
            {
            }
          }
        }
      }
      if (attributes.Count <= 0)
        return;
      IfcTools.AddProperties(ifcObj, "PDMS " + (object) ownerDiscipline, attributes);
    }

    public static void CreateUda(DbElement cur, IFCObjectsReader.Data.IfcObject ifcObj, Mapping mapping)
    {
      try
      {
        foreach (DataRow row in (InternalDataCollectionBase) mapping.UdaDs.Tables[0].Rows)
        {
          string str1 = row[0].ToString();
          string str2 = ((AttributeType) row[1]).ToString();
          string name = row[2].ToString();
          string str3 = str1.Contains(".") ? str1.Substring(0, str1.IndexOf(".")) : string.Empty;
          string str4 = str1.Contains(".") ? str1.Substring(str1.IndexOf(".") + 1) : str1;
          foreach (KeyValuePair<string, Dictionary<string, IfcObjectPropertySetValue>> propertySetValue in ifcObj.ObjectPropertySetValues)
          {
            if (propertySetValue.Key.Equals(str3))
            {
              foreach (KeyValuePair<string, IfcObjectPropertySetValue> keyValuePair in propertySetValue.Value)
              {
                if (keyValuePair.Key.ToUpper().Equals(str4.ToUpper()) && TS_ModelConverter.Tools.ExistsAttribute(cur, name))
                {
                  if (str2.Equals("Integer"))
                  {
                    int integerValue = keyValuePair.Value.IntegerValue;
                    TS_ModelConverter.Tools.SetAttribute(cur, DbAttribute.GetDbAttribute(name), integerValue);
                  }
                  else if (str2.Equals("Double"))
                  {
                    double realValue = keyValuePair.Value.RealValue;
                    TS_ModelConverter.Tools.SetAttribute(cur, DbAttribute.GetDbAttribute(name), realValue);
                  }
                  else if (str2.Equals("String"))
                  {
                    string str5 = string.Empty;
                    if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.String))
                      str5 = keyValuePair.Value.StringValue;
                    else if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Double))
                      str5 = keyValuePair.Value.RealValue.ToString((IFormatProvider) CultureInfo.CurrentCulture);
                    else if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Integer))
                      str5 = keyValuePair.Value.IntegerValue.ToString();
                    else if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Boolean))
                      str5 = keyValuePair.Value.BooleanValue.ToString();
                    else if (keyValuePair.Value.ValueType.Equals((object) IfcObjectPropertySetValue.TypeEnum.Logical))
                      str5 = keyValuePair.Value.LogicalValue.ToString();
                    if (!string.IsNullOrEmpty(str5))
                      TS_ModelConverter.Tools.SetAttribute(cur, DbAttribute.GetDbAttribute(name), str5);
                  }
                }
              }
            }
          }
        }
      }
      catch
      {
      }
    }

    public static void CreateCommand(DbElement element, string commandString)
    {
      DbElement element1 = CurrentElement.get_Element();
      CurrentElement.set_Element(element);
      Command.CreateCommand(commandString).RunInPdms();
      CurrentElement.set_Element(element1);
    }

    public static Guid CreateGuid(string guidString)
    {
      return new Guid(MD5.Create().ComputeHash(Encoding.Default.GetBytes(guidString)));
    }

    public static bool ExistsAttribute(DbElement cur, string name)
    {
      return ((IEnumerable<DbAttribute>) cur.GetAttributes()).Any<DbAttribute>((Func<DbAttribute, bool>) (attr =>
      {
        if (!attr.get_Name().ToUpper().Equals(name.ToUpper()))
          return attr.get_ShortName().ToUpper().Equals(name.ToUpper());
        return true;
      }));
    }

    public static DbElement GetElement(DbElement cur, string sourceGuid)
    {
      DbElementType[] dbElementTypeArray = new DbElementType[2]
      {
        (DbElementType) DbElementTypeInstance.SCTN,
        (DbElementType) DbElementTypeInstance.PANEL
      };
      DBElementCollection elementCollection = new DBElementCollection(cur);
      elementCollection.set_IncludeRoot(false);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
      {
        DbElement current = (DbElement) enumerator.get_Current();
        if (TS_ModelConverter.Tools.ExistsAttribute(current, ":TEKLA_GUID") && current.GetAsString(DbAttribute.GetDbAttribute(":TEKLA_GUID")).Equals(sourceGuid))
          return current;
      }
      return (DbElement) null;
    }

    public static string GetTeklaGuid(DbElement cur, string sourceGuid)
    {
      DbElementType[] dbElementTypeArray = new DbElementType[3]
      {
        (DbElementType) DbElementTypeInstance.SCTN,
        (DbElementType) DbElementTypeInstance.PANEL,
        (DbElementType) DbElementTypeInstance.GENSEC
      };
      DBElementCollection elementCollection = new DBElementCollection(cur);
      elementCollection.set_IncludeRoot(false);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
      {
        DbElement current = (DbElement) enumerator.get_Current();
        if (TS_ModelConverter.Tools.ExistsAttribute(current, ":TEKLA_IFCGUID") && (current.GetString(DbAttribute.GetDbAttribute(":TEKLA_IFCGUID")).Equals(sourceGuid) && TS_ModelConverter.Tools.ExistsAttribute(current, ":TEKLA_GUID")))
          return current.GetString(DbAttribute.GetDbAttribute(":TEKLA_GUID"));
      }
      return string.Empty;
    }

    public static FileInfo GetFileInfo(string path)
    {
      return path.Length > 0 ? new FileInfo(path) : (FileInfo) null;
    }

    public static bool IsPolygonClockWise(List<Point> polygon)
    {
      double num = 0.0;
      for (int index = 0; index < polygon.Count; ++index)
      {
        Point point1 = polygon[index];
        Point point2 = polygon[(index + 1) % polygon.Count];
        num += (point2.X - point1.X) * (point2.Y + point1.Y);
      }
      return num > 0.0;
    }

    public static string GetMacAddress()
    {
      string empty = string.Empty;
      foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
      {
        if (networkInterface.OperationalStatus == OperationalStatus.Up)
        {
          empty += networkInterface.GetPhysicalAddress().ToString();
          break;
        }
      }
      return empty;
    }

    public static bool SetVersion(DbElement cur, string version)
    {
      if (!TS_ModelConverter.Tools.ExistsAttribute(cur, ":TEKLA_VERSION"))
        return false;
      try
      {
        if (TS_ModelConverter.Tools.ExistsAttribute(cur, ":TEKLA_VERSION"))
        {
          TS_ModelConverter.Tools.SetAttribute(cur, DbAttribute.GetDbAttribute(":TEKLA_VERSION"), version);
          return true;
        }
      }
      catch (Exception ex)
      {
        TS_ModelConverter.Tools.Log.Error((object) "Error setting version");
        return false;
      }
      return false;
    }

    public static void StoreHierarchy(DbElement cur, ref Dictionary<string, object> attributes)
    {
      for (DbElement owner = cur.get_Owner(); !owner.get_IsNull(); owner = owner.get_Owner())
      {
        if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.SITE))
          attributes.Add("SITE", (object) (IfcLabel) owner.GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1));
        else if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.ZONE))
          attributes.Add("ZONE", (object) (IfcLabel) owner.GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1));
        else if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.STRUCTURE))
          attributes.Add("STRU", (object) (IfcLabel) owner.GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1));
        else if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.FRMWORK))
          attributes.Add("FRMW", (object) (IfcLabel) owner.GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1));
        else if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.SBFRAMEWORK))
          attributes.Add("SBFR", (object) (IfcLabel) owner.GetAsString((DbAttribute) DbAttributeInstance.NAME).Substring(1));
      }
    }

    public static bool GetHierarchyOwner(IFCObjectsReader.Data.IfcObject ifcObj, out DbElement owner)
    {
      string site = string.Empty;
      string zone = string.Empty;
      string stru = string.Empty;
      string str1 = string.Empty;
      string str2 = string.Empty;
      GuidChanger.ConvertIfcGuidToSystemGuid(new IfcGloballyUniqueId(ifcObj.SourceGuid)).ToString();
      owner = (DbElement) null;
      foreach (KeyValuePair<string, Dictionary<string, IfcObjectPropertySetValue>> propertySetValue in ifcObj.ObjectPropertySetValues)
      {
        if (propertySetValue.Key.Equals("Hierarchy"))
        {
          foreach (KeyValuePair<string, IfcObjectPropertySetValue> keyValuePair in propertySetValue.Value)
          {
            if (keyValuePair.Key.ToUpper().Equals("AVEVASITE"))
              site = keyValuePair.Value.StringValue.StartsWith("/") ? keyValuePair.Value.StringValue : "/" + keyValuePair.Value.StringValue;
            else if (keyValuePair.Key.ToUpper().Equals("AVEVAZONE"))
              zone = keyValuePair.Value.StringValue.StartsWith("/") ? keyValuePair.Value.StringValue : "/" + keyValuePair.Value.StringValue;
            else if (keyValuePair.Key.ToUpper().Equals("AVEVASTRU"))
              stru = keyValuePair.Value.StringValue.StartsWith("/") ? keyValuePair.Value.StringValue : "/" + keyValuePair.Value.StringValue;
            else if (keyValuePair.Key.ToUpper().Equals("AVEVAFRMW"))
              str1 = keyValuePair.Value.StringValue.StartsWith("/") ? keyValuePair.Value.StringValue : "/" + keyValuePair.Value.StringValue;
            else if (keyValuePair.Key.ToUpper().Equals("AVEVASBFR"))
              str2 = keyValuePair.Value.StringValue.StartsWith("/") ? keyValuePair.Value.StringValue : "/" + keyValuePair.Value.StringValue;
          }
        }
      }
      if (!string.IsNullOrEmpty(str2))
      {
        if (TS_ModelConverter.Tools.ExistsInModel(str2, out owner))
          return true;
        owner = TS_ModelConverter.Tools.GetHierarchyObject("SBFR", site, zone, stru, str1, str2);
        return true;
      }
      if (string.IsNullOrEmpty(str1))
        return false;
      if (TS_ModelConverter.Tools.ExistsInModel(str1, out owner))
        return true;
      owner = TS_ModelConverter.Tools.GetHierarchyObject("FRMW", site, zone, stru, str1, str2);
      return true;
    }

    public static DbElement GetHierarchyObject(
      string type,
      string site,
      string zone,
      string stru,
      string frmw,
      string sbfr)
    {
      bool flag1 = false;
      DbElement element1 = (DbElement) null;
      DbElement element2 = (DbElement) null;
      DbElement element3 = (DbElement) null;
      DbElement element4 = (DbElement) null;
      DbElement element5 = (DbElement) null;
      if (!string.IsNullOrEmpty(site))
      {
        if (TS_ModelConverter.Tools.ExistsInModel(site, out element1))
        {
          flag1 = true;
        }
        else
        {
          try
          {
            element1 = ((MDB) MDB.CurrentMDB).GetDB(CurrentElement.get_Element().DbNo()).get_World().Create(0, (DbElementType) DbElementTypeInstance.SITE);
            try
            {
              TS_ModelConverter.Tools.SetAttribute(element1, (DbAttribute) DbAttributeInstance.NAME, site);
            }
            catch
            {
              TS_ModelConverter.Tools.Log.Error((object) ("Could not set name on SITE: " + site));
            }
            flag1 = true;
          }
          catch
          {
            TS_ModelConverter.Tools.Log.Error((object) ("SITE " + site + " creation failed"));
            throw new Exception("SITE " + site + " creation failed");
          }
        }
      }
      bool flag2;
      if (flag1 && !string.IsNullOrEmpty(zone))
      {
        flag2 = false;
        if (TS_ModelConverter.Tools.ExistsInModel(zone, out element2))
        {
          flag1 = true;
        }
        else
        {
          try
          {
            element2 = element1.Create(0, (DbElementType) DbElementTypeInstance.ZONE);
            try
            {
              TS_ModelConverter.Tools.SetAttribute(element2, (DbAttribute) DbAttributeInstance.NAME, zone);
            }
            catch
            {
              TS_ModelConverter.Tools.Log.Error((object) ("Could not set name on ZONE: " + zone));
            }
            flag1 = true;
          }
          catch
          {
            TS_ModelConverter.Tools.Log.Error((object) ("ZONE " + zone + " creation failed"));
            throw new Exception("ZONE " + zone + " creation failed");
          }
        }
      }
      if (flag1 && !string.IsNullOrEmpty(stru))
      {
        flag2 = false;
        if (TS_ModelConverter.Tools.ExistsInModel(stru, out element3))
        {
          flag1 = true;
        }
        else
        {
          try
          {
            element3 = element2.Create(0, (DbElementType) DbElementTypeInstance.STRUCTURE);
            try
            {
              TS_ModelConverter.Tools.SetAttribute(element3, (DbAttribute) DbAttributeInstance.NAME, stru);
            }
            catch
            {
              TS_ModelConverter.Tools.Log.Error((object) ("Could not set name on STRU: " + stru));
            }
            flag1 = true;
          }
          catch
          {
            TS_ModelConverter.Tools.Log.Error((object) ("STRU " + stru + " creation failed"));
            throw new Exception("STRU " + stru + " creation failed");
          }
        }
      }
      if (flag1 && !string.IsNullOrEmpty(frmw))
      {
        flag2 = false;
        if (TS_ModelConverter.Tools.ExistsInModel(frmw, out element4))
        {
          flag1 = true;
        }
        else
        {
          try
          {
            element4 = element3.Create(0, (DbElementType) DbElementTypeInstance.FRMWORK);
            try
            {
              TS_ModelConverter.Tools.SetAttribute(element4, (DbAttribute) DbAttributeInstance.NAME, frmw);
            }
            catch
            {
              TS_ModelConverter.Tools.Log.Error((object) ("Could not set name on FRMW: " + frmw));
            }
            flag1 = true;
          }
          catch
          {
            TS_ModelConverter.Tools.Log.Error((object) ("FRMW " + frmw + " creation failed"));
            throw new Exception("FRMW " + frmw + " creation failed");
          }
        }
        if (type.Equals("FRMW"))
          return element4;
      }
      if (flag1 && !string.IsNullOrEmpty(sbfr))
      {
        if (!TS_ModelConverter.Tools.ExistsInModel(sbfr, out element5))
        {
          try
          {
            element5 = element4.Create(0, (DbElementType) DbElementTypeInstance.SBFRAMEWORK);
            try
            {
              TS_ModelConverter.Tools.SetAttribute(element5, (DbAttribute) DbAttributeInstance.NAME, sbfr);
            }
            catch
            {
              TS_ModelConverter.Tools.Log.Error((object) ("Could not set name on SBFR: " + frmw));
            }
          }
          catch
          {
            TS_ModelConverter.Tools.Log.Error((object) ("SBFR " + sbfr + " creation failed"));
            throw new Exception("SBFR " + sbfr + " creation failed");
          }
        }
        if (type.Equals("SBFR"))
          return element5;
      }
      return (DbElement) null;
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, string value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set string value " + value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, double value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set double value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, int value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set int value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, Position value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set Position value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool GetBool(
      DbElement element,
      DbAttribute attribute,
      string callingMethod,
      ref Exception exception)
    {
      try
      {
        return element.GetBool(attribute);
      }
      catch (Exception ex)
      {
        exception = ex;
        TS_ModelConverter.Tools.Log.Error((object) ("Could not get bool value for attribute " + attribute.get_Name() + ". Calling function " + callingMethod + ". " + ex.Message));
        return false;
      }
    }

    public static bool GetBool(DbElement element, DbAttribute attribute, string callingMethod)
    {
      try
      {
        return element.GetBool(attribute);
      }
      catch (Exception ex)
      {
        TS_ModelConverter.Tools.Log.Error((object) ("Could not get bool value for attribute " + attribute.get_Name() + ". Calling function " + callingMethod + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, Orientation value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set Orientation value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, DbElement value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set DbElement value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, double[] value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set double[] value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, Direction value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set Direction value " + (object) value + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static bool SetAttribute(DbElement element, DbAttribute attribute, bool value)
    {
      try
      {
        element.SetAttribute(attribute, value);
        return true;
      }
      catch (Exception ex)
      {
        string asString = element.GetAsString((DbAttribute) DbAttributeInstance.REF);
        TS_ModelConverter.Tools.Log.Error((object) ("Could not set bool value " + value.ToString() + "for attribute " + attribute.get_Name() + ", pdmsRef " + asString + ". " + ex.Message));
        return false;
      }
    }

    public static void SetColor(DbElement element, string color)
    {
      string str = "=" + (object) element.RefNo()[0] + "/" + (object) element.RefNo()[1];
      Command.CreateCommand("REM " + str + " ADD " + str + " COL " + color).Run();
    }

    public static Dictionary<string, DbElement> CollectExistingItems(
      DbElement cur)
    {
      Dictionary<string, DbElement> dictionary = new Dictionary<string, DbElement>();
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
      DBElementCollection elementCollection = new DBElementCollection(cur);
      elementCollection.set_IncludeRoot(true);
      elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
      DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
      while (enumerator.MoveNext())
      {
        DbElement current = (DbElement) enumerator.get_Current();
        if (TS_ModelConverter.Tools.ExistsAttribute(current, ":TEKLA_IFCGUID"))
        {
          string asString = current.GetAsString(DbAttribute.GetDbAttribute(":TEKLA_IFCGUID"));
          if (!string.IsNullOrEmpty(asString) && !asString.Equals("unset") && !dictionary.ContainsKey(asString))
            dictionary.Add(asString, current);
        }
      }
      return dictionary;
    }

    public static List<string> GetSites(
      string path,
      Mapping mapping,
      out List<string> claimList,
      out List<string> hierarchyNames)
    {
      claimList = new List<string>();
      hierarchyNames = new List<string>();
      Dictionary<string, string> breps;
      List<string> allGuids;
      List<string> profiles;
      List<string> materials;
      List<string> cutOfCuts;
      List<string> brepCuts;
      List<IFCObjectsReader.Data.IfcObject> ifcObjectList = new IfcDocument(path).GetIfcObjectList(out breps, out allGuids, out profiles, out materials, out cutOfCuts, out brepCuts, true, false);
      List<string> stringList = new List<string>();
      int num = 0;
      string str1 = "Ifc source GUIDs without hierarchy: \r\n";
      foreach (IFCObjectsReader.Data.IfcObject ifcObject in ifcObjectList)
      {
        if (ifcObject.ObjectPropertySetValues.ContainsKey("Hierarchy"))
        {
          if (!ifcObject.ObjectPropertySetValues["Hierarchy"].ContainsKey("AvevaSITE"))
          {
            ++num;
            str1 = str1 + ifcObject.SourceGuid + "\r\n";
          }
          else
          {
            IfcObjectPropertySetValue propertySetValue1;
            if (!ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSITE", out propertySetValue1))
            {
              ++num;
              str1 = str1 + ifcObject.SourceGuid + "\r\n";
            }
            else
            {
              if (!string.IsNullOrEmpty(propertySetValue1.StringValue))
              {
                string str2 = propertySetValue1.StringValue.StartsWith("/") ? propertySetValue1.StringValue.Substring(1) : propertySetValue1.StringValue;
                if (!stringList.Contains(str2))
                  stringList.Add(str2);
                if (!hierarchyNames.Contains(str2))
                  hierarchyNames.Add(str2);
              }
              else
              {
                ++num;
                str1 = str1 + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue2;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaZONE", out propertySetValue2))
              {
                if (!string.IsNullOrEmpty(propertySetValue2.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue2.StringValue))
                    hierarchyNames.Add(propertySetValue2.StringValue);
                }
                else
                {
                  ++num;
                  str1 = str1 + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str1 = str1 + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue3;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSTRU", out propertySetValue3))
              {
                if (!string.IsNullOrEmpty(propertySetValue3.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue3.StringValue))
                    hierarchyNames.Add(propertySetValue3.StringValue);
                }
                else
                {
                  ++num;
                  str1 = str1 + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str1 = str1 + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue4;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaFRMW", out propertySetValue4))
              {
                if (!string.IsNullOrEmpty(propertySetValue4.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue4.StringValue))
                    hierarchyNames.Add(propertySetValue4.StringValue);
                }
                else
                {
                  ++num;
                  str1 = str1 + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str1 = str1 + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue5;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSBFR", out propertySetValue5) && !string.IsNullOrEmpty(propertySetValue5.StringValue) && !hierarchyNames.Contains(propertySetValue5.StringValue))
                hierarchyNames.Add(propertySetValue5.StringValue);
            }
          }
        }
      }
      if (mapping.UpdateAvevaHierarchy && num > 0)
      {
        TS_ModelConverter.Tools.Log.Info((object) "Some members without SITE hierarchy definition. When hierarchies used then all members should have SITE definition. Use 'check hierarchies' in Tekla side");
        TS_ModelConverter.Tools.Log.Info((object) str1);
      }
      if (stringList.Count.Equals(1))
      {
        TS_ModelConverter.Tools.Log.Info((object) ("One SITE in import file: " + stringList[0]));
        if (!TS_ModelConverter.Tools.ExistsInModel(stringList[0]))
          return stringList;
        DbElement element = ((MDB) MDB.CurrentMDB).FindElement((DbType) 1, stringList[0].StartsWith("/") ? stringList[0] : "/" + stringList[0]);
        if (DbElement.op_Equality(element, (DbElement) null))
          return stringList;
        DBElementCollection elementCollection = new DBElementCollection(element);
        elementCollection.set_IncludeRoot(false);
        DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.get_Current();
          if (!DbElement.op_Equality(current, (DbElement) null))
          {
            string asString = current.GetAsString((DbAttribute) DbAttributeInstance.NAMN);
            for (int index = 0; index < hierarchyNames.Count; ++index)
            {
              if (asString == hierarchyNames[index])
              {
                bool flag1 = TS_ModelConverter.Tools.GetBool(current, (DbAttribute) DbAttributeInstance.MODDEL, nameof (GetSites));
                bool flag2 = TS_ModelConverter.Tools.GetBool(current, (DbAttribute) DbAttributeInstance.LOCK, nameof (GetSites));
                if (!flag1 | flag2)
                {
                  if (index > 0)
                    claimList.Add(current.GetAsString((DbAttribute) DbAttributeInstance.REF));
                  TS_ModelConverter.Tools.Log.Info((object) (current.GetAsString((DbAttribute) DbAttributeInstance.REF) + (flag2 ? " locked" : "") + (!flag1 ? " not deletable" : "")));
                }
              }
            }
          }
        }
      }
      return stringList;
    }

    public static List<string> GetSites(
      List<IFCObjectsReader.Data.IfcObject> list,
      Mapping mapping,
      string path,
      out List<string> claimList,
      out List<string> hierarchyNames)
    {
      claimList = new List<string>();
      hierarchyNames = new List<string>();
      List<string> stringList = new List<string>();
      int num = 0;
      string str = "Ifc source GUIDs without hierarchy: \r\n";
      foreach (IFCObjectsReader.Data.IfcObject ifcObject in list)
      {
        if (ifcObject.ObjectPropertySetValues.ContainsKey("Hierarchy"))
        {
          if (!ifcObject.ObjectPropertySetValues["Hierarchy"].ContainsKey("AvevaSITE"))
          {
            ++num;
            str = str + ifcObject.SourceGuid + "\r\n";
          }
          else
          {
            IfcObjectPropertySetValue propertySetValue1;
            if (!ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSITE", out propertySetValue1))
            {
              ++num;
              str = str + ifcObject.SourceGuid + "\r\n";
            }
            else
            {
              if (!string.IsNullOrEmpty(propertySetValue1.StringValue))
              {
                if (!stringList.Contains(propertySetValue1.StringValue))
                  stringList.Add(propertySetValue1.StringValue);
                if (!hierarchyNames.Contains(propertySetValue1.StringValue))
                  hierarchyNames.Add(propertySetValue1.StringValue);
              }
              else
              {
                ++num;
                str = str + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue2;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaZONE", out propertySetValue2))
              {
                if (!string.IsNullOrEmpty(propertySetValue2.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue2.StringValue))
                    hierarchyNames.Add(propertySetValue2.StringValue);
                }
                else
                {
                  ++num;
                  str = str + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str = str + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue3;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSTRU", out propertySetValue3))
              {
                if (!string.IsNullOrEmpty(propertySetValue3.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue3.StringValue))
                    hierarchyNames.Add(propertySetValue3.StringValue);
                }
                else
                {
                  ++num;
                  str = str + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str = str + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue4;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaFRMW", out propertySetValue4))
              {
                if (!string.IsNullOrEmpty(propertySetValue4.StringValue))
                {
                  if (!hierarchyNames.Contains(propertySetValue4.StringValue))
                    hierarchyNames.Add(propertySetValue4.StringValue);
                }
                else
                {
                  ++num;
                  str = str + ifcObject.SourceGuid + "\r\n";
                }
              }
              else
              {
                ++num;
                str = str + ifcObject.SourceGuid + "\r\n";
              }
              IfcObjectPropertySetValue propertySetValue5;
              if (ifcObject.ObjectPropertySetValues["Hierarchy"].TryGetValue("AvevaSBFR", out propertySetValue5) && !string.IsNullOrEmpty(propertySetValue5.StringValue) && !hierarchyNames.Contains(propertySetValue5.StringValue))
                hierarchyNames.Add(propertySetValue5.StringValue);
            }
          }
        }
      }
      if (mapping.UpdateAvevaHierarchy && num > 0)
      {
        TS_ModelConverter.Tools.Log.Info((object) "Some members without SITE hierarchy definition. When hierarchies used then all members should have SITE definition. Use 'check hierarchies' in Tekla side");
        TS_ModelConverter.Tools.Log.Info((object) str);
      }
      if (stringList.Count.Equals(1))
      {
        TS_ModelConverter.Tools.Log.Info((object) ("One SITE in import file: " + stringList[0]));
        if (!TS_ModelConverter.Tools.ExistsInModel(stringList[0]))
          return stringList;
        DBElementCollection elementCollection = new DBElementCollection(((MDB) MDB.CurrentMDB).FindElement((DbType) 1, "/" + stringList[0]));
        elementCollection.set_IncludeRoot(false);
        DBElementEnumerator enumerator = (DBElementEnumerator) elementCollection.GetEnumerator();
        while (enumerator.MoveNext())
        {
          DbElement current = (DbElement) enumerator.get_Current();
          string asString = current.GetAsString((DbAttribute) DbAttributeInstance.NAMN);
          for (int index = 0; index < hierarchyNames.Count; ++index)
          {
            if (asString == hierarchyNames[index])
            {
              bool flag1 = TS_ModelConverter.Tools.GetBool(current, (DbAttribute) DbAttributeInstance.MODDEL, nameof (GetSites));
              bool flag2 = TS_ModelConverter.Tools.GetBool(current, (DbAttribute) DbAttributeInstance.LOCK, nameof (GetSites));
              if (!flag1 | flag2)
              {
                if (index > 0)
                  claimList.Add(current.GetAsString((DbAttribute) DbAttributeInstance.REF));
                TS_ModelConverter.Tools.Log.Info((object) (current.GetAsString((DbAttribute) DbAttributeInstance.REF) + (flag2 ? " locked" : "") + (!flag1 ? " not deletable" : "")));
              }
            }
          }
        }
      }
      return stringList;
    }

    public static bool CheckHierarchyNames(IEnumerable<string> names, out List<string> wrongNames)
    {
      wrongNames = names.Where<string>((Func<string, bool>) (name => name.Contains(" "))).ToList<string>();
      return wrongNames.Count.Equals(0);
    }

    public static bool SortMembers(
      Mapping mapping,
      List<string> siteList1,
      bool deleteEmptyHierarchyElements1)
    {
      TS_ModelConverter.Tools.Log.Info((object) ("Starting hierarchy sorting: " + DateTime.Now.ToLongTimeString()));
      List<DbElement> dbElementList = new List<DbElement>();
      foreach (string site in mapping.SiteList)
      {
        DbElement element1;
        if (!TS_ModelConverter.Tools.ExistsInModel(site, out element1))
          return false;
        DbElementType[] dbElementTypeArray = new DbElementType[5]
        {
          (DbElementType) DbElementTypeInstance.SITE,
          (DbElementType) DbElementTypeInstance.ZONE,
          (DbElementType) DbElementTypeInstance.STRUCTURE,
          (DbElementType) DbElementTypeInstance.FRMWORK,
          (DbElementType) DbElementTypeInstance.SBFRAMEWORK
        };
        DBElementCollection elementCollection = new DBElementCollection(element1);
        elementCollection.set_IncludeRoot(true);
        elementCollection.set_Filter((BaseFilter) new TypeFilter(dbElementTypeArray));
        DBElementEnumerator enumerator1 = (DBElementEnumerator) elementCollection.GetEnumerator();
        while (enumerator1.MoveNext())
        {
          DbElement current1 = (DbElement) enumerator1.get_Current();
          if (!DbElement.op_Equality(current1, (DbElement) null))
          {
            if ((uint) current1.Members().Length > 0U)
            {
              try
              {
                IOrderedEnumerable<DbElement> orderedEnumerable = ((IEnumerable<DbElement>) current1.Members()).OrderBy<DbElement, string>((Func<DbElement, string>) (element => element.GetAsString((DbAttribute) DbAttributeInstance.NAME)));
                if (((object) current1.GetActualType()).Equals((object) DbElementTypeInstance.FRMWORK))
                {
                  using (IEnumerator<DbElement> enumerator2 = ((IEnumerable<DbElement>) orderedEnumerable).GetEnumerator())
                  {
                    while (((IEnumerator) enumerator2).MoveNext())
                    {
                      DbElement current2 = enumerator2.Current;
                      if (((object) current2.GetActualType()).Equals((object) DbElementTypeInstance.SBFRAMEWORK))
                        current2.InsertAfterLast(current1);
                    }
                  }
                }
                using (IEnumerator<DbElement> enumerator2 = ((IEnumerable<DbElement>) orderedEnumerable).GetEnumerator())
                {
                  while (((IEnumerator) enumerator2).MoveNext())
                  {
                    DbElement current2 = enumerator2.Current;
                    if (!((object) current2.GetActualType()).Equals((object) DbElementTypeInstance.SBFRAMEWORK))
                      current2.InsertAfterLast(current1);
                  }
                }
              }
              catch (Exception ex)
              {
                return false;
              }
            }
            else if (mapping.DeleteEmptyContainerElementsAfterImport)
              dbElementList.Add(current1);
          }
        }
      }
      TS_ModelConverter.Tools.Log.Info((object) ("Ending hierarchy sorting: " + DateTime.Now.ToLongTimeString()));
      using (List<DbElement>.Enumerator enumerator = dbElementList.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DbElement current = enumerator.Current;
          if (!((object) current).Equals((object) mapping.DeletedElements) && !((object) current).Equals((object) mapping.UnplacedElements) && current.get_IsDeleteable())
          {
            string asString = current.GetAsString((DbAttribute) DbAttributeInstance.NAME);
            if (current.get_IsDeleteable())
            {
              current.Delete();
              TS_ModelConverter.Tools.Log.Info((object) (asString + " is empty and deleted according to settings."));
            }
          }
        }
      }
      return true;
    }

    public static double EvaluateDouble(string expression, DbElement cur)
    {
      try
      {
        DbExpression dbExpression = DbExpression.Parse(expression);
        if (dbExpression != null)
          return Math.Round(cur.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
      }
      catch (Exception ex)
      {
        return 0.0;
      }
      return 0.0;
    }

    public static bool EvaluateDouble(string expression, DbElement cur, out double value)
    {
      value = 0.0;
      try
      {
        DbExpression dbExpression = DbExpression.Parse(expression);
        if (dbExpression != null)
        {
          value = Math.Round(cur.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
          return true;
        }
      }
      catch (Exception ex)
      {
        value = 0.0;
        return false;
      }
      return false;
    }

    private static DbElement GetHierarchyObject(DbElement cur, HierarchyLevel hierarchy)
    {
      if (hierarchy.Equals((object) HierarchyLevel.Object))
        return cur;
      for (DbElement owner = cur.get_Owner(); !owner.get_IsNull(); owner = owner.get_Owner())
      {
        if (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.FRMWORK) && hierarchy.Equals((object) HierarchyLevel.Frmw) || ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.BRANCH) && hierarchy.Equals((object) HierarchyLevel.Branch) || (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.PIPE) && hierarchy.Equals((object) HierarchyLevel.Pipe) || ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.HVAC) && hierarchy.Equals((object) HierarchyLevel.Hvac)) || (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.EQUIPMENT) && hierarchy.Equals((object) HierarchyLevel.Equipment) || ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.STRUCTURE) && hierarchy.Equals((object) HierarchyLevel.Stru) || (((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.ZONE) && hierarchy.Equals((object) HierarchyLevel.Zone) || ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.SITE) && hierarchy.Equals((object) HierarchyLevel.Site))))
          return owner;
      }
      return (DbElement) null;
    }

    private static double GetDoubleValue(string exprString, DbElement cur)
    {
      double num = 0.0;
      try
      {
        DbExpression dbExpression = DbExpression.Parse(exprString);
        if (dbExpression != null)
          num = Math.Round(cur.EvaluateDouble(dbExpression, (DbAttributeUnit) 939199), 2);
      }
      catch (Exception ex)
      {
      }
      return num;
    }
  }
}
