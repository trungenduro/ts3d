// Decompiled with JetBrains decompiler
// Type: TS_ModelConverter.Primitive
// Assembly: TS_ModelConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F81CDA0D-1216-40ED-828A-BAF333294439
// Assembly location: C:\TS_PDMS\12.0SP6\TS-PDMS_Library\TS_ModelConverter.dll

using Aveva.Pdms.Database;
using Aveva.Pdms.Geometry;
using IFCObjectsReader;
using IFCObjectsReader.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Technology.IfcLib;

namespace TS_ModelConverter
{
  public class Primitive
  {
    private DbElement _primitive;

    public Primitive()
    {
    }

    public Primitive(DbElement cur)
    {
      this._primitive = cur;
      this.Position = cur.GetPosition((DbAttribute) DbAttributeInstance.POS, Tools.WrtQualifyer);
      this.Orientation = cur.GetOrientation((DbAttribute) DbAttributeInstance.ORI, Tools.WrtQualifyer);
      Direction direction1 = this.Orientation.XDir();
      Direction direction2 = this.Orientation.YDir();
      Direction direction3 = this.Orientation.ZDir();
      this.DirX = IfcTools.CreateDirection(direction1.East, direction1.get_North(), direction1.get_Up());
      this.DirY = IfcTools.CreateDirection(direction2.East(), direction2.get_North(), direction2.get_Up());
      this.DirZ = IfcTools.CreateDirection(direction3.get_East(), direction3.get_North(), direction3.get_Up());
      this.Reference = cur.RefNo()[0].ToString() + "/" + (object) cur.RefNo()[1];
    }

    public IIfcDirection DirX { get; set; }

    public IIfcDirection DirY { get; set; }

    public IIfcDirection DirZ { get; set; }

    public Position Position { get; set; }

    public Orientation Orientation { get; set; }

    public string Reference { get; set; }

    public void CreateExtrusion(DbElement owner, IfcObjectGeometryData primitive, bool chamfers)
    {
      DbElement dbElement1 = owner.Create(0, (DbElementType) DbElementTypeInstance.EXTRUSION);
      try
      {
        Vector xdirTs = primitive.XDirTS;
        xdirTs.Normalize();
        Vector ydirTs = primitive.YDirTS;
        ydirTs.Normalize();
        CoordinateSystem CoordSys = new CoordinateSystem(primitive.PolygonPointsTS[0], xdirTs, ydirTs);
        Matrix matrix = MatrixFactory.ToCoordinateSystem(CoordSys);
        Orientation orientation = Tools.CreateOrientation(xdirTs, ydirTs);
        Position pos = Position.Create(CoordSys.Origin.X, CoordSys.Origin.Y, CoordSys.Origin.Z);
        Tools.CreatePosOri(dbElement1, pos, orientation);
        double num1 = Math.Round(Distance.PointToPoint(primitive.StartPointTS, primitive.EndPointTS), 0);
        Tools.SetAttribute(dbElement1, (DbAttribute) DbAttributeInstance.HEIG, num1);
        DbElement dbElement2 = dbElement1.Create(0, (DbElementType) DbElementTypeInstance.LOOP);
        int index = 0;
        List<Point> points;
        List<double> chamfers1;
        new PolygonCreator().GetPolygonPointsArcMiddelePoint(primitive, out points, out chamfers1);
        List<Point> list = points.Select<Point, Point>((Func<Point, Point>) (p => matrix.Transform(p))).ToList<Point>();
        Tools.CleanPolygonImport(ref chamfers1, ref list);
        foreach (Point point in list)
        {
          DbElement element = dbElement2.Create(0, (DbElementType) DbElementTypeInstance.VERTEX);
          Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.POS, Position.Create(point.X, point.Y, point.Z));
          double num2 = chamfers ? Math.Floor(chamfers1[index]) : 0.0;
          Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.FRAD, num2);
          ++index;
        }
      }
      catch
      {
        dbElement1.Delete();
      }
    }

    public void CreateCylinder(DbElement owner, IfcObjectGeometryData primitive)
    {
      DbElement element = owner.Create(0, (DbElementType) DbElementTypeInstance.CYLINDER);
      try
      {
        Position position1 = Tools.GetPosition("POS WRT WORLD", ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.TMPLATE) ? owner.get_Owner() : owner);
        Orientation orientation1 = Tools.GetOrientation("ORI WRT WORLD", ((object) owner.GetActualType()).Equals((object) DbElementTypeInstance.TMPLATE) ? owner.get_Owner() : owner);
        Direction direction1 = orientation1.XDir();
        Direction direction2 = orientation1.YDir();
        Matrix coordinateSystem = MatrixFactory.ToCoordinateSystem(new CoordinateSystem(new Point(position1.get_X(), position1.get_Y(), position1.get_Z()), new Vector(direction1.get_East(), direction1.get_North(), direction1.get_Up()), new Vector(direction2.get_East(), direction2.get_North(), direction2.get_Up())));
        double num1 = Math.Max(primitive.XDimension, primitive.YDimension);
        Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.DIAM, num1);
        Point startPointTs = primitive.StartPointTS;
        Point endPointTs = primitive.EndPointTS;
        new Vector(endPointTs - startPointTs).Normalize();
        AABB aabb = new AABB(startPointTs, endPointTs);
        Point p = new Point(aabb.GetCenterPoint().X, aabb.GetCenterPoint().Y, aabb.GetCenterPoint().Z);
        Point point1 = coordinateSystem.Transform(startPointTs);
        Point point2 = coordinateSystem.Transform(endPointTs);
        Point point3 = coordinateSystem.Transform(p);
        Orientation orientation2 = Orientation.Create("Z  IS " + ((object) Direction.Create(Position.Create(point1.X, point1.Y, point1.Z), Position.Create(point2.X, point2.Y, point2.Z))).ToString());
        Position position2 = Position.Create(
            .X, point3.Y, point3.Z);
        Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.POS, position2);
        Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.ORI, orientation2);
        double num2 = Math.Round(Distance.PointToPoint(primitive.StartPointTS, primitive.EndPointTS), 0);
        Tools.SetAttribute(element, (DbAttribute) DbAttributeInstance.HEIG, num2);
      }
      catch
      {
        element.Delete();
      }
    }

    public void CreateBox(DbElement owner, IfcObjectGeometryData primitive)
    {
      DbElement dbElement = owner.Create(0, (DbElementType) DbElementTypeInstance.BOX);
      try
      {
        AABB aabb = new AABB(primitive.StartPointTS, primitive.EndPointTS);
        Point point = new Point(aabb.GetCenterPoint().X, aabb.GetCenterPoint().Y, aabb.GetCenterPoint().Z);
        Position pos = Position.Create(point.X, point.Y, point.Z);
        Vector xdirTs = primitive.XDirTS;
        xdirTs.Normalize();
        Vector ydirTs = primitive.YDirTS;
        ydirTs.Normalize();
        Orientation orientation = Tools.CreateOrientation(xdirTs, ydirTs);
        Tools.CreatePosOri(dbElement, pos, orientation);
        double num1 = Math.Round(primitive.XDimension, 0);
        Tools.SetAttribute(dbElement, (DbAttribute) DbAttributeInstance.XLEN, num1);
        double num2 = Math.Round(primitive.YDimension, 0);
        Tools.SetAttribute(dbElement, (DbAttribute) DbAttributeInstance.YLEN, num2);
        double num3 = Math.Round(Distance.PointToPoint(primitive.StartPointTS, primitive.EndPointTS), 0);
        Tools.SetAttribute(dbElement, (DbAttribute) DbAttributeInstance.ZLEN, num3);
      }
      catch
      {
        dbElement.Delete();
      }
    }

    public IIfcElement CreateCylinder(
      ref List<IIfcProduct> elements,
      double height,
      double radius,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirY));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, -height / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(0.0, -1.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateExtrudedAreaSolid((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) radius), axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) height), "Body", "SweptSolid")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "yellow");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateSlopedCylinder(
      ref List<IIfcProduct> elements,
      double height,
      double radius,
      double botX,
      double botY,
      double topX,
      double topY,
      Guid guid)
    {
      double num1 = Math.Max(radius * Math.Tan(Math.Abs(botX * Math.PI / 180.0)), radius * Math.Tan(Math.Abs(botY * Math.PI / 180.0)));
      double num2 = Math.Max(radius * Math.Tan(Math.Abs(topX * Math.PI / 180.0)), radius * Math.Tan(Math.Abs(topY * Math.PI / 180.0)));
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirY));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, -height / 2.0 - num1), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0));
      IIfcExtrudedAreaSolid extrudedAreaSolid = IfcTools.CreateExtrudedAreaSolid((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) radius), axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) (height + num1 + num2));
      IIfcBooleanClippingResult booleanClippingResult1 = (IIfcBooleanClippingResult) null;
      IIfcBooleanClippingResult booleanClippingResult2 = (IIfcBooleanClippingResult) null;
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        booleanClippingResult2 == null ? (booleanClippingResult1 == null ? IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) extrudedAreaSolid, "Body", "SweptSolid") : IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) booleanClippingResult1, "Body", "Clipping")) : IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) booleanClippingResult2, "Body", "Clipping")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "green");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateBox(
      ref List<IIfcProduct> elements,
      double lengthX,
      double lengthY,
      double lengthZ,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirY));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, -lengthZ / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(0.0, -1.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateExtrudedAreaSolid((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) lengthX, (IfcPositiveLengthMeasure) lengthY), axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) lengthZ), "Body", "SweptSolid")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "brown");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateExtrusion(
      ref List<IIfcProduct> elements,
      List<Point> points,
      List<double> chamfers,
      double height,
      Guid guid)
    {
      IfcAxis2Placement plateLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirY));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0), IfcTools.CreateDirection(0.0, 0.0, 1.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateExtrudedAreaSolid((IIfcProfileDef) IfcTools.CreateArbitraryClosedProfileDef(IfcProfileTypeEnum.IFC_AREA, (IIfcCurve) IfcTools.CreateChamferedCurvedPolyline(points, chamfers)), axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) height), "Body", "SweptSolid")
      });
      IIfcPlate plate = IfcTools.CreatePlate(plateLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) plate);
      IfcTools.AddColorToElement((IIfcElement) plate, "brown");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) plate
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) plate, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) plate);
      return (IIfcElement) plate;
    }

    public IIfcElement CreateRevolution(
      ref List<IIfcProduct> elements,
      List<Point> points,
      List<double> chamfers,
      double angle,
      Guid guid)
    {
      IIfcProfileDef profile = IfcTools.CreateProfile(points, chamfers, "REVO");
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirY, this.DirX));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateCurvedBeam(profile, axis2Placement3D, IfcTools.IfcDatabase.CreateIfcAxis1Placement(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)), (IfcPlaneAngleMeasure) angle), "Body", "SweptSolid")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "beige");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateCircularTorus(
      ref List<IIfcProduct> elements,
      double radius,
      double bendingRadius,
      double angle,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirX, this.DirY));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, bendingRadius), IfcTools.CreateDirection(1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateCurvedBeam((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) radius), axis2Placement3D, IfcTools.IfcDatabase.CreateIfcAxis1Placement(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)), (IfcPlaneAngleMeasure) angle), "Body", "SweptSolid")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "yellow");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateRectangularTorus(
      ref List<IIfcProduct> elements,
      double width,
      double height,
      double bendingRadius,
      double angle,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirX, this.DirY));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, bendingRadius), IfcTools.CreateDirection(1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateCurvedBeam((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) height, (IfcPositiveLengthMeasure) width), axis2Placement3D, IfcTools.IfcDatabase.CreateIfcAxis1Placement(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)), (IfcPlaneAngleMeasure) angle), "Body", "SweptSolid")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "brown");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreateSnout(
      ref List<IIfcProduct> elements,
      double height,
      double diaBot,
      double diaTop,
      double deltaX,
      double deltaY,
      Guid guid)
    {
      List<IIfcCompositeCurveSegment> Segments = new List<IIfcCompositeCurveSegment>();
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirX));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      List<IIfcAxis2Placement3D> CrossSectionPositions = new List<IIfcAxis2Placement3D>();
      List<IIfcProfileDef> CrossSections = new List<IIfcProfileDef>();
      CrossSectionPositions.Add(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(-deltaX / 2.0, -deltaY / 2.0, -height / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)));
      CrossSectionPositions.Add(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(deltaX / 2.0, deltaY / 2.0, height / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)));
      CrossSections.Add((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) (diaBot / 2.0)));
      CrossSections.Add((IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcCircleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) (diaTop / 2.0)));
      IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.IfcDatabase.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) IfcTools.IfcDatabase.CreateIfcPolyline(new List<IIfcCartesianPoint>()
      {
        IfcTools.CreatePoint(-deltaX / 2.0, -deltaY / 2.0, -height / 2.0),
        IfcTools.CreatePoint(deltaX / 2.0, deltaY / 2.0, height / 2.0)
      }));
      Segments.Add(compositeCurveSegment);
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.IfcDatabase.CreateIfcSectionedSpine(IfcTools.IfcDatabase.CreateIfcCompositeCurve(Segments, new LogicalEnum?(LogicalEnum.IFC_FALSE)), CrossSections, CrossSectionPositions), "Body", "SectionedSpine")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "yellow");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreatePyramid(
      ref List<IIfcProduct> elements,
      double height,
      double botX,
      double botY,
      double topX,
      double topY,
      double deltaX,
      double deltaY,
      Guid guid)
    {
      List<IIfcCompositeCurveSegment> Segments = new List<IIfcCompositeCurveSegment>();
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirX));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      List<IIfcAxis2Placement3D> CrossSectionPositions = new List<IIfcAxis2Placement3D>()
      {
        IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(-deltaX / 2.0, -deltaY / 2.0, -height / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)),
        IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(deltaX / 2.0, deltaY / 2.0, height / 2.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0))
      };
      List<IIfcProfileDef> CrossSections = new List<IIfcProfileDef>()
      {
        (IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) botX, (IfcPositiveLengthMeasure) botY),
        (IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) topX, (IfcPositiveLengthMeasure) topY)
      };
      IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.IfcDatabase.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) IfcTools.IfcDatabase.CreateIfcPolyline(new List<IIfcCartesianPoint>()
      {
        IfcTools.CreatePoint(-deltaX / 2.0, -deltaY / 2.0, -height / 2.0),
        IfcTools.CreatePoint(deltaX / 2.0, deltaY / 2.0, height / 2.0)
      }));
      Segments.Add(compositeCurveSegment);
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.IfcDatabase.CreateIfcSectionedSpine(IfcTools.IfcDatabase.CreateIfcCompositeCurve(Segments, new LogicalEnum?(LogicalEnum.IFC_FALSE)), CrossSections, CrossSectionPositions), "Body", "SectionedSpine")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "green");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreatePyramid(
      ref List<IIfcProduct> elements,
      Point position1,
      Point position2,
      double botX,
      double botY,
      double topX,
      double topY,
      Guid guid)
    {
      List<IIfcCompositeCurveSegment> Segments = new List<IIfcCompositeCurveSegment>();
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirX));
      IIfcAxis2Placement2D axis2Placement2D = IfcTools.CreateAxis2Placement2D(IfcTools.CreatePoint(0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0));
      List<IIfcAxis2Placement3D> CrossSectionPositions = new List<IIfcAxis2Placement3D>()
      {
        IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(position1.X, position1.Y, position1.Z), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)),
        IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(position2.X, position2.Y, position2.Z), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0))
      };
      List<IIfcProfileDef> CrossSections = new List<IIfcProfileDef>()
      {
        (IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) botX, (IfcPositiveLengthMeasure) botY),
        (IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcRectangleProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) string.Empty, axis2Placement2D, (IfcPositiveLengthMeasure) topX, (IfcPositiveLengthMeasure) topY)
      };
      IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.IfcDatabase.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) IfcTools.IfcDatabase.CreateIfcPolyline(new List<IIfcCartesianPoint>()
      {
        IfcTools.CreatePoint(position1.X, position1.Y, position1.Z),
        IfcTools.CreatePoint(position2.X, position2.Y, position2.Z)
      }));
      Segments.Add(compositeCurveSegment);
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.IfcDatabase.CreateIfcSectionedSpine(IfcTools.IfcDatabase.CreateIfcCompositeCurve(Segments, new LogicalEnum?(LogicalEnum.IFC_FALSE)), CrossSections, CrossSectionPositions), "Body", "SectionedSpine")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "green");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcElement CreatePolyHedron(
      ref List<IIfcProduct> elements,
      List<List<Point>> faces,
      Guid guid)
    {
      List<IIfcFace> CfsFaces = new List<IIfcFace>();
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirX));
      foreach (List<Point> face in faces)
      {
        face.Reverse();
        List<IIfcCartesianPoint> Polygon = new List<IIfcCartesianPoint>();
        foreach (Point point in face)
          Polygon.Insert(0, IfcTools.CreatePoint(point.X, point.Y, point.Z));
        CfsFaces.Add(IfcTools.CreatePolygon((IIfcFaceBound) IfcTools.IfcDatabase.CreateIfcFaceOuterBound((IIfcLoop) IfcTools.IfcDatabase.CreateIfcPolyLoop(Polygon), new bool?(true))));
      }
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.IfcDatabase.CreateIfcFacetedBrep(IfcTools.IfcDatabase.CreateIfcClosedShell(CfsFaces)), "Body", "Brep")
      });
      IIfcBeam beam = IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
      IfcTools.AddQuantities((IIfcObject) beam);
      IfcTools.AddColorToElement((IIfcElement) beam, "green");
      IfcTools.AddMaterialToElement(new List<IIfcRoot>()
      {
        (IIfcRoot) beam
      }, "Undefined");
      if (!string.IsNullOrEmpty(this.Reference))
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
          {
            "PDMS_ID",
            (object) (IfcLabel) this.Reference
          }
        };
        IfcTools.AddProperties((IIfcObject) beam, "PDMS Common", attributes);
      }
      elements.Add((IIfcProduct) beam);
      return (IIfcElement) beam;
    }

    public IIfcBeam CreateStraight(
      ref List<IIfcProduct> elements,
      IIfcProfileDef profile,
      double length,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirZ, this.DirX));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 0.0, 1.0), IfcTools.CreateDirection(1.0, 0.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateExtrudedAreaSolid(profile, axis2Placement3D, IfcTools.CreateDirection(0.0, 0.0, 1.0), (IfcPositiveLengthMeasure) length), "Body", "SweptSolid")
      });
      return IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
    }

    public IIfcBeam CreateCurve(
      ref List<IIfcProduct> elements,
      IIfcProfileDef profile,
      double radius,
      double angle,
      Guid guid)
    {
      IfcAxis2Placement beamLocation = new IfcAxis2Placement(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirX, this.DirZ));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0));
      IIfcProductDefinitionShape productDefinitionShape = IfcTools.IfcDatabase.CreateIfcProductDefinitionShape((IfcLabel) null, (IfcText) null, new List<IIfcRepresentation>()
      {
        IfcTools.CreateShapeRepresentation((IIfcRepresentationItem) IfcTools.CreateCurvedBeam(profile, axis2Placement3D, IfcTools.IfcDatabase.CreateIfcAxis1Placement(IfcTools.CreatePoint(0.0, 0.0, 0.0), IfcTools.CreateDirection(1.0, 0.0, 0.0)), (IfcPlaneAngleMeasure) angle), "Body", "SweptSolid")
      });
      return IfcTools.CreateBeam(beamLocation, (IIfcProductRepresentation) productDefinitionShape, guid);
    }

    public IIfcRevolvedAreaSolid CreateSolid(
      IIfcProfileDef profile,
      double radius,
      double angle)
    {
      IIfcAxis1Placement ifcAxis1Placement = IfcTools.IfcDatabase.CreateIfcAxis1Placement(IfcTools.CreatePoint(radius, 0.0, 0.0), IfcTools.CreateDirection(0.0, 1.0, 0.0));
      IIfcAxis2Placement3D axis2Placement3D = IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(this.Position.get_X(), this.Position.get_Y(), this.Position.get_Z()), this.DirY, this.DirX);
      return IfcTools.CreateRevolvedAreaSolid(profile, axis2Placement3D, ifcAxis1Placement, (IfcPlaneAngleMeasure) angle);
    }
  }
}
