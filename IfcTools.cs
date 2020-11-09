// Decompiled with JetBrains decompiler
// Type: TS_ModelConverter.IfcTools
// Assembly: TS_ModelConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F81CDA0D-1216-40ED-828A-BAF333294439
// Assembly location: C:\TS_PDMS\12.0SP6\TS-PDMS_Library\TS_ModelConverter.dll

using Aveva.Pdms.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Technology.IfcLib;

namespace TS_ModelConverter
{
  public static class IfcTools
  {
    private static char[] conversionTable = new char[64]
    {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'A',
      'B',
      'C',
      'D',
      'E',
      'F',
      'G',
      'H',
      'I',
      'J',
      'K',
      'L',
      'M',
      'N',
      'O',
      'P',
      'Q',
      'R',
      'S',
      'T',
      'U',
      'V',
      'W',
      'X',
      'Y',
      'Z',
      'a',
      'b',
      'c',
      'd',
      'e',
      'f',
      'g',
      'h',
      'i',
      'j',
      'k',
      'l',
      'm',
      'n',
      'o',
      'p',
      'q',
      'r',
      's',
      't',
      'u',
      'v',
      'w',
      'x',
      'y',
      'z',
      '_',
      '$'
    };
    private static IfcDatabaseAPI db;
    private static IIfcRepresentationContext defaultContext;
    private static IIfcOwnerHistory defaultUser;
    private static IIfcLocalPlacement defaultPlacement;
    private static IIfcSpatialStructureElement defaultSpatial;
    private static List<IIfcPresentationStyleAssignment> defaultStyle;

    public static IfcDatabaseAPI IfcDatabase
    {
      get
      {
        if (IfcTools.db == null)
        {
          IfcTools.db = new IfcDatabaseAPI();
          IfcTools.db.CreateModel(IfcSchema.IFC2X3);
        }
        return IfcTools.db;
      }
    }

    public static void SetupModel(string fileName)
    {
      IfcTools.defaultUser = IfcTools.CreateDefaultOwner();
      IfcTools.defaultContext = IfcTools.CreateDefaultContext();
      IfcTools.defaultStyle = IfcTools.CreateDefaultStyle();
      IfcTools.defaultSpatial = IfcTools.CreateSpatialStructure(IfcTools.CreateProject());
      IfcTools.SetDefaultHeaderInfo(fileName);
    }

    public static void ClearDb()
    {
      IfcTools.db = (IfcDatabaseAPI) null;
    }

    public static IIfcCartesianPoint CreatePoint(double x, double y, double z)
    {
      List<IfcLengthMeasure> Coordinates = new List<IfcLengthMeasure>()
      {
        (IfcLengthMeasure) x,
        (IfcLengthMeasure) y,
        (IfcLengthMeasure) z
      };
      return IfcTools.db.CreateIfcCartesianPoint(Coordinates);
    }

    public static IIfcCartesianPoint CreatePoint(double x, double y)
    {
      List<IfcLengthMeasure> Coordinates = new List<IfcLengthMeasure>()
      {
        (IfcLengthMeasure) x,
        (IfcLengthMeasure) y
      };
      return IfcTools.db.CreateIfcCartesianPoint(Coordinates);
    }

    public static IIfcDirection CreateDirection(double x, double y, double z)
    {
      List<double> DirectionRatios = new List<double>()
      {
        x,
        y,
        z
      };
      return IfcTools.db.CreateIfcDirection(DirectionRatios);
    }

    public static IIfcDirection CreateDirection(double x, double y)
    {
      List<double> DirectionRatios = new List<double>()
      {
        x,
        y
      };
      return IfcTools.db.CreateIfcDirection(DirectionRatios);
    }

    public static void BindElementsToModel(List<IIfcProduct> elements)
    {
      IfcTools.db.CreateIfcRelContainedInSpatialStructure(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, elements, IfcTools.defaultSpatial);
    }

    public static IIfcAxis2Placement3D CreateAxis2Placement3D(
      IIfcCartesianPoint origo,
      IIfcDirection axisZ,
      IIfcDirection axisX)
    {
      return IfcTools.db.CreateIfcAxis2Placement3D(origo, axisZ, axisX);
    }

    public static IIfcAxis2Placement2D CreateAxis2Placement2D(
      IIfcCartesianPoint origo,
      IIfcDirection axisX)
    {
      return IfcTools.db.CreateIfcAxis2Placement2D(origo, axisX);
    }

    public static IIfcPlane CreatePlane(IIfcAxis2Placement3D position)
    {
      return IfcTools.db.CreateIfcPlane(position);
    }

    public static IIfcHalfSpaceSolid CreateHalfSpaceSolid(
      IIfcPlane plane,
      bool agreementFlag)
    {
      return IfcTools.db.CreateIfcHalfSpaceSolid((IIfcSurface) plane, new bool?(agreementFlag));
    }

    public static IIfcBooleanClippingResult CreateBooleanClippingResult(
      IfcBooleanOperator booleanOperator,
      IfcBooleanOperand firstOperand,
      IfcBooleanOperand secondOperand)
    {
      return IfcTools.db.CreateIfcBooleanClippingResult(new IfcBooleanOperator?(booleanOperator), firstOperand, secondOperand);
    }

    public static IIfcRepresentation CreateShapeRepresentation(
      IIfcRepresentationItem geometryItem,
      string identifier,
      string type)
    {
      return (IIfcRepresentation) IfcTools.db.CreateIfcShapeRepresentation(IfcTools.defaultContext, (IfcLabel) identifier, (IfcLabel) type, new List<IIfcRepresentationItem>()
      {
        geometryItem
      });
    }

    public static IIfcProductDefinitionShape CreateIfcProductDefinitionShape(
      IfcLabel label,
      IfcText text,
      List<IIfcRepresentation> representations)
    {
      return IfcTools.db.CreateIfcProductDefinitionShape(label, text, representations);
    }

    public static IIfcBeam CreateBeam(
      IfcAxis2Placement beamLocation,
      IIfcProductRepresentation beamRepresentation,
      Guid guid)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, beamLocation);
      return IfcTools.db.CreateIfcBeam(IfcTools.ConvertToIfcGuid(guid), IfcTools.defaultUser, (IfcLabel) "BEAM", (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement, beamRepresentation, (IfcIdentifier) null);
    }

    public static IIfcColumn CreateColumn(
      IfcAxis2Placement beamLocation,
      IIfcProductRepresentation beamRepresentation,
      Guid guid)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, beamLocation);
      return IfcTools.db.CreateIfcColumn(IfcTools.ConvertToIfcGuid(guid), IfcTools.defaultUser, (IfcLabel) "COLUMN", (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement, beamRepresentation, (IfcIdentifier) null);
    }

    public static IIfcElement CreateItem(
      IfcAxis2Placement location,
      IIfcProductRepresentation representation,
      Guid guid,
      bool isColumn)
    {
      if (isColumn)
        return (IIfcElement) IfcTools.CreateColumn(location, representation, guid);
      return (IIfcElement) IfcTools.CreateBeam(location, representation, guid);
    }

    public static IIfcProfileDef CreateProfile(
      List<Point> polygonPoints,
      List<double> chamfers,
      string name)
    {
      List<IIfcCartesianPoint> Points = new List<IIfcCartesianPoint>();
      if (polygonPoints.Count > 0)
      {
        Points.AddRange(polygonPoints.Select<Point, IIfcCartesianPoint>((Func<Point, IIfcCartesianPoint>) (p => IfcTools.CreatePoint(p.X, p.Y))));
        Points.Add(Points[0]);
      }
      IIfcPolyline ifcPolyline = IfcTools.IfcDatabase.CreateIfcPolyline(Points);
      return (IIfcProfileDef) IfcTools.IfcDatabase.CreateIfcArbitraryClosedProfileDef(new IfcProfileTypeEnum?(IfcProfileTypeEnum.IFC_AREA), (IfcLabel) name, (IIfcCurve) ifcPolyline);
    }

    public static IIfcBuildingElementProxy CreatePenetration(
      IfcAxis2Placement location,
      IIfcProductRepresentation representation,
      Guid guid)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, location);
      IfcIdentifier ifcIdentifier = (IfcIdentifier) "ProvisionForVoid";
      return IfcTools.db.CreateIfcBuildingElementProxy(IfcTools.ConvertToIfcGuid(guid), IfcTools.defaultUser, (IfcLabel) "PDMS Provision for void", (IfcText) "Made by PDMS Interoperability Tool", (IfcLabel) "ProvisionForVoid", (IIfcObjectPlacement) ifcLocalPlacement, representation, (IfcIdentifier) null, new IfcElementCompositionEnum?(IfcElementCompositionEnum.IFC_ELEMENT));
    }

    public static IIfcSphere CreateSphere(
      IIfcAxis2Placement3D location,
      IfcPositiveLengthMeasure radius)
    {
      return IfcTools.db.CreateIfcSphere(location, radius);
    }

    public static IIfcRevolvedAreaSolid CreateCurvedBeam(
      IIfcProfileDef profileDef,
      IIfcAxis2Placement3D location,
      IIfcAxis1Placement axis,
      IfcPlaneAngleMeasure angle)
    {
      return IfcTools.db.CreateIfcRevolvedAreaSolid(profileDef, location, axis, angle);
    }

    public static IIfcRevolvedAreaSolid CreateRevolvedAreaSolid(
      IIfcProfileDef profileDef,
      IIfcAxis2Placement3D location,
      IIfcAxis1Placement axis,
      IfcPlaneAngleMeasure angle)
    {
      return IfcTools.db.CreateIfcRevolvedAreaSolid(profileDef, location, axis, angle);
    }

    public static IIfcBooleanClippingResult CreateClipPlane(
      IfcBooleanOperand solid,
      Direction cutplane,
      Vector planeVector,
      Point alignment)
    {
      IIfcHalfSpaceSolid halfSpaceSolid = IfcTools.CreateHalfSpaceSolid(IfcTools.CreatePlane(IfcTools.CreateAxis2Placement3D(IfcTools.CreatePoint(-alignment.X, -alignment.Y, 0.0), IfcTools.CreateDirection(Math.Round(cutplane.get_East(), 4), Math.Round(cutplane.get_North(), 4), Math.Round(cutplane.get_Up(), 4)), IfcTools.CreateDirection(Math.Round(planeVector.X, 4), Math.Round(planeVector.Y, 4), Math.Round(planeVector.Z, 4)))), false);
      return IfcTools.CreateBooleanClippingResult(IfcBooleanOperator.IFC_DIFFERENCE, solid, IfcSelect.IfcBooleanOperand(halfSpaceSolid));
    }

    public static void AddQuantities(IIfcObject element)
    {
      List<IIfcObject> RelatedObjects = new List<IIfcObject>();
      RelatedObjects.Add(element);
      List<IIfcPhysicalQuantity> Quantities = new List<IIfcPhysicalQuantity>()
      {
        (IIfcPhysicalQuantity) IfcTools.db.CreateIfcQuantityLength((IfcLabel) "width", (IfcText) null, (IIfcNamedUnit) null, (IfcLengthMeasure) 200.0)
      };
      IIfcElementQuantity ifcElementQuantity = IfcTools.db.CreateIfcElementQuantity(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) "BaseQuantities", (IfcText) null, (IfcLabel) null, Quantities);
      IfcTools.db.CreateIfcRelDefinesByProperties(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, RelatedObjects, (IIfcPropertySetDefinition) ifcElementQuantity);
    }

    public static void AddProperties(
      IIfcObject element,
      string propertySetName,
      Dictionary<string, object> attributes)
    {
      List<IIfcObject> RelatedObjects = new List<IIfcObject>()
      {
        element
      };
      List<IIfcProperty> HasProperties = new List<IIfcProperty>();
      foreach (KeyValuePair<string, object> attribute in attributes)
      {
        if (attribute.Value is IfcLabel)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcLabel) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
        else if (attribute.Value is IfcText)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcText) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
        else if (attribute.Value is IfcPositiveLengthMeasure)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcPositiveLengthMeasure) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
        else if (attribute.Value is IfcLengthMeasure)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcLengthMeasure) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
        else if (attribute.Value is IfcMassMeasure)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcMassMeasure) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
        else if (attribute.Value is IfcVolumeMeasure)
        {
          IIfcProperty propertySingleValue = (IIfcProperty) IfcTools.db.CreateIfcPropertySingleValue((IfcIdentifier) attribute.Key, (IfcText) null, (IfcValue) ((IfcVolumeMeasure) attribute.Value), (IfcUnit) null);
          HasProperties.Add(propertySingleValue);
        }
      }
      IIfcPropertySet ifcPropertySet = IfcTools.db.CreateIfcPropertySet(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) propertySetName, (IfcText) null, HasProperties);
      IfcTools.db.CreateIfcRelDefinesByProperties(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, RelatedObjects, (IIfcPropertySetDefinition) ifcPropertySet);
    }

    public static void AddColorToElement(IIfcElement element, string color)
    {
      List<IIfcPresentationStyleAssignment> Styles = new List<IIfcPresentationStyleAssignment>();
      Styles.Add(IfcTools.CreateDefaultColorAssignment(color));
      IIfcRepresentationItem representationItem = IfcTools.FetchRepresentation(element);
      if (representationItem == null)
        return;
      IfcTools.db.CreateIfcStyledItem(representationItem, Styles, (IfcLabel) null);
    }

    public static void CreateRelVoidsElement(IIfcElement element, IIfcOpeningElement opening)
    {
      IfcTools.db.CreateIfcRelVoidsElement(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, element, (IIfcFeatureElementSubtraction) opening);
    }

    public static IIfcExtrudedAreaSolid CreateExtrudedAreaSolid(
      IIfcProfileDef sweptArea,
      IIfcAxis2Placement3D position,
      IIfcDirection extrudedDirection,
      IfcPositiveLengthMeasure depth)
    {
      return IfcTools.db.CreateIfcExtrudedAreaSolid(sweptArea, position, extrudedDirection, depth);
    }

    public static IIfcRelAssociatesMaterial CreateMaterial(
      IIfcObject element,
      string material)
    {
      try
      {
        List<IIfcRoot> RelatedObjects = new List<IIfcRoot>();
        RelatedObjects.Add((IIfcRoot) element);
        IIfcMaterial ifcMaterial = IfcTools.db.CreateIfcMaterial(new IfcLabel(material));
        return IfcTools.db.CreateIfcRelAssociatesMaterial(IfcTools.GenerateIfcGuid(), (IIfcOwnerHistory) null, (IfcLabel) null, (IfcText) null, RelatedObjects, new IfcMaterialSelect(ifcMaterial));
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
        return (IIfcRelAssociatesMaterial) null;
      }
    }

    public static IIfcPolyline CreatePolyline(List<IIfcCartesianPoint> points)
    {
      return IfcTools.db.CreateIfcPolyline(points);
    }

    public static IIfcFace CreatePolygon(IIfcFaceBound contour)
    {
      List<IIfcFaceBound> Bounds = new List<IIfcFaceBound>()
      {
        contour
      };
      return IfcTools.db.CreateIfcFace(Bounds);
    }

    public static IIfcCompositeCurve CreateChamferedCurvedPolyline(
      List<Point> points,
      List<double> chamfers)
    {
      List<List<Point>> segments = IfcTools.CreateSegments(points, chamfers);
      IfcTools.RemoveZeroRadius(ref segments, 1.0);
      IfcTools.RemoveZeroSegment(ref segments, 1.0);
      IfcTools.AdjustSegmentPoints(ref segments);
      IfcTools.AddSegmentToGap(ref segments);
      List<IIfcCompositeCurveSegment> compositeCurve = IfcTools.CreateCompositeCurve(segments);
      return IfcTools.db.CreateIfcCompositeCurve(compositeCurve, new LogicalEnum?(LogicalEnum.IFC_FALSE));
    }

    public static IIfcCompositeCurve CreateChamferedCurvedPolyline(
      List<Point> points,
      List<double> chamfers,
      string separate)
    {
      List<IIfcCompositeCurveSegment> Segments = new List<IIfcCompositeCurveSegment>();
      if (points.Count.Equals(2))
      {
        IIfcPolyline ifcPolyline = IfcTools.db.CreateIfcPolyline(new List<IIfcCartesianPoint>()
        {
          IfcTools.CreatePoint(points[0].X, points[0].Y, points[1].Z),
          IfcTools.CreatePoint(points[1].X, points[1].Y, points[1].Z)
        });
        IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.db.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) ifcPolyline);
        Segments.Add(compositeCurveSegment);
      }
      else
      {
        Vector vector = new Vector(points[0] - points[1]);
        vector.Normalize();
        Vector Vector = new Vector(points[2] - points[1]);
        Vector.Normalize();
        double angleBetween = vector.GetAngleBetween(Vector);
        IIfcCartesianPoint point = IfcTools.CreatePoint(points[1].X, points[1].Y);
        IIfcDirection ifcDirection = IfcTools.db.CreateIfcDirection(new List<double>()
        {
          vector.X,
          vector.Y
        });
        IfcAxis2Placement Position = new IfcAxis2Placement(IfcTools.db.CreateIfcAxis2Placement2D(point, ifcDirection));
        IIfcCircle ifcCircle1 = IfcTools.db.CreateIfcCircle(Position, new IfcPositiveLengthMeasure(chamfers[1]));
        IfcDatabaseAPI db = IfcTools.db;
        IIfcCircle ifcCircle2 = ifcCircle1;
        List<IfcTrimmingSelect> Trim1 = new List<IfcTrimmingSelect>();
        Trim1.Add((IfcTrimmingSelect) new IfcParameterValue(0.0));
        List<IfcTrimmingSelect> Trim2 = new List<IfcTrimmingSelect>();
        Trim2.Add((IfcTrimmingSelect) new IfcParameterValue(angleBetween));
        bool? SenseAgreement = new bool?(true);
        IfcTrimmingPreference? MasterRepresentation = new IfcTrimmingPreference?(IfcTrimmingPreference.IFC_PARAMETER);
        IIfcTrimmedCurve ifcTrimmedCurve = db.CreateIfcTrimmedCurve((IIfcCurve) ifcCircle2, Trim1, Trim2, SenseAgreement, MasterRepresentation);
        IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.db.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) ifcTrimmedCurve);
        Segments.Add(compositeCurveSegment);
      }
      return IfcTools.db.CreateIfcCompositeCurve(Segments, new LogicalEnum?(LogicalEnum.IFC_FALSE));
    }

    public static IIfcArbitraryClosedProfileDef CreateArbitraryClosedProfileDef(
      IfcProfileTypeEnum profileType,
      IIfcPolyline vertices)
    {
      return IfcTools.db.CreateIfcArbitraryClosedProfileDef(new IfcProfileTypeEnum?(profileType), (IfcLabel) "Profile Name", (IIfcCurve) vertices);
    }

    public static IIfcArbitraryClosedProfileDef CreateArbitraryClosedProfileDef(
      IfcProfileTypeEnum profileType,
      IIfcCurve vertices)
    {
      return IfcTools.db.CreateIfcArbitraryClosedProfileDef(new IfcProfileTypeEnum?(profileType), (IfcLabel) "Profile Name", vertices);
    }

    public static IIfcPlate CreatePlate(
      IfcAxis2Placement plateLocation,
      IIfcProductRepresentation plateRepresentation)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, plateLocation);
      return IfcTools.db.CreateIfcPlate(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) "PLATE", (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement, plateRepresentation, (IfcIdentifier) null);
    }

    public static IIfcPlate CreatePlate(
      IfcAxis2Placement plateLocation,
      IIfcProductRepresentation plateRepresentation,
      Guid guid)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, plateLocation);
      return IfcTools.db.CreateIfcPlate(IfcTools.ConvertToIfcGuid(guid), IfcTools.defaultUser, (IfcLabel) "PLATE", (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement, plateRepresentation, (IfcIdentifier) null);
    }

    public static IIfcOpeningElement CreateOpeningElement(
      IfcAxis2Placement openingOrigin,
      IIfcProductRepresentation openingRepresentation)
    {
      IIfcLocalPlacement ifcLocalPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) IfcTools.defaultPlacement, openingOrigin);
      return IfcTools.db.CreateIfcOpeningElement(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IfcLabel) "Recess", (IIfcObjectPlacement) ifcLocalPlacement, openingRepresentation, (IfcIdentifier) null);
    }

    public static IIfcRightCircularCylinder CreateCylinder(
      IIfcAxis2Placement3D position,
      IfcPositiveLengthMeasure height,
      IfcPositiveLengthMeasure radius)
    {
      return IfcTools.db.CreateIfcRightCircularCylinder(position, height, radius);
    }

    public static void AddMaterialToElement(List<IIfcRoot> elements, string materialname)
    {
      IIfcMaterial ifcMaterial = IfcTools.db.CreateIfcMaterial((IfcLabel) materialname);
      if (ifcMaterial == null)
        return;
      IfcTools.db.CreateIfcRelAssociatesMaterial(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, elements, new IfcMaterialSelect(ifcMaterial));
    }

    public static IfcGloballyUniqueId ConvertToIfcGuid(Guid inGuid)
    {
      byte[] byteArray = inGuid.ToByteArray();
      byte[] numArray1 = new byte[4]
      {
        byteArray[15],
        byteArray[14],
        byteArray[13],
        (byte) 0
      };
      byte[] numArray2 = new byte[4]
      {
        byteArray[12],
        byteArray[11],
        byteArray[10],
        (byte) 0
      };
      byte[] numArray3 = new byte[4]
      {
        byteArray[9],
        byteArray[8],
        byteArray[6],
        (byte) 0
      };
      byte[] numArray4 = new byte[4]
      {
        byteArray[7],
        byteArray[4],
        byteArray[5],
        (byte) 0
      };
      byte[] numArray5 = new byte[4]
      {
        byteArray[0],
        byteArray[1],
        byteArray[2],
        (byte) 0
      };
      byte[] numArray6 = new byte[4]
      {
        byteArray[3],
        (byte) 0,
        (byte) 0,
        (byte) 0
      };
      uint uint32_1 = BitConverter.ToUInt32(numArray1, 0);
      uint uint32_2 = BitConverter.ToUInt32(numArray2, 0);
      uint uint32_3 = BitConverter.ToUInt32(numArray3, 0);
      uint uint32_4 = BitConverter.ToUInt32(numArray4, 0);
      uint uint32_5 = BitConverter.ToUInt32(numArray5, 0);
      return new IfcGloballyUniqueId(IfcTools.ToBase64(BitConverter.ToUInt32(numArray6, 0), 8) + IfcTools.ToBase64(uint32_5, 24) + IfcTools.ToBase64(uint32_4, 24) + IfcTools.ToBase64(uint32_3, 24) + IfcTools.ToBase64(uint32_2, 24) + IfcTools.ToBase64(uint32_1, 24));
    }

    private static void SetDefaultHeaderInfo(string fileName)
    {
      string fullName = Assembly.GetAssembly(IfcTools.db.GetType()).FullName;
      string PreprosessorVersion = fullName.Remove(fullName.IndexOf(',', fullName.IndexOf(',') + 1));
      IfcTools.db.SetHeader("ViewDefinition [CoordinationView]", "2;1", fileName, IfcTools.CurrentTime(), Environment.UserName, "User organization", PreprosessorVersion, "OrigSystem", string.Empty, "IFC2X3");
    }

    private static IIfcProject CreateProject()
    {
      IfcTools.GenerateIfcGuid();
      List<IIfcRepresentationContext> RepresentationContexts = new List<IIfcRepresentationContext>()
      {
        IfcTools.defaultContext
      };
      IIfcUnitAssignment globalUnits = IfcTools.CreateGlobalUnits();
      return IfcTools.db.CreateIfcProject(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) "Project name", (IfcText) null, (IfcLabel) null, (IfcLabel) null, (IfcLabel) null, RepresentationContexts, globalUnits);
    }

    private static IIfcOwnerHistory CreateDefaultOwner()
    {
      string userName = Environment.UserName;
      IIfcOrganization ifcOrganization = IfcTools.db.CreateIfcOrganization((IfcIdentifier) null, (IfcLabel) "Tekla", (IfcText) null, (List<IIfcActorRole>) null, (List<IIfcAddress>) null);
      IIfcPersonAndOrganization personAndOrganization = IfcTools.db.CreateIfcPersonAndOrganization(IfcTools.db.CreateIfcPerson((IfcIdentifier) userName, (IfcLabel) "FamilyName", (IfcLabel) null, (List<IfcLabel>) null, (List<IfcLabel>) null, (List<IfcLabel>) null, (List<IIfcActorRole>) null, (List<IIfcAddress>) null), ifcOrganization, (List<IIfcActorRole>) null);
      IIfcApplication ifcApplication = IfcTools.db.CreateIfcApplication(ifcOrganization, (IfcLabel) "1.0", (IfcLabel) "ifcApp", (IfcIdentifier) "PdmsToIfc");
      return IfcTools.db.CreateIfcOwnerHistory(personAndOrganization, ifcApplication, new IfcStateEnum?(), new IfcChangeActionEnum?(IfcChangeActionEnum.IFC_NOCHANGE), (IfcTimeStamp) null, (IIfcPersonAndOrganization) null, (IIfcApplication) null, IfcTools.GetTimeStamp());
    }

    private static IIfcRepresentationContext CreateDefaultContext()
    {
      IIfcGeometricRepresentationContext representationContext = IfcTools.db.CreateIfcGeometricRepresentationContext((IfcLabel) null, (IfcLabel) "Model", (IfcDimensionCount) 3, new double?(1E-07), new IfcAxis2Placement(IfcTools.CreateDefaultPlacement()), (IIfcDirection) null);
      IfcTools.db.CreateIfcGeometricRepresentationSubContext(new IfcLabel("Body"), new IfcLabel("Model"), (IfcDimensionCount) null, new double?(), (IfcAxis2Placement) null, (IIfcDirection) null, representationContext, (IfcPositiveRatioMeasure) 1.0, new IfcGeometricProjectionEnum?(IfcGeometricProjectionEnum.IFC_MODEL_VIEW), (IfcLabel) null);
      return (IIfcRepresentationContext) representationContext;
    }

    private static IIfcAxis2Placement3D CreateDefaultPlacement()
    {
      IIfcCartesianPoint point = IfcTools.CreatePoint(0.0, 0.0, 0.0);
      IIfcDirection direction1 = IfcTools.CreateDirection(0.0, 0.0, 1.0);
      IIfcDirection direction2 = IfcTools.CreateDirection(1.0, 0.0, 0.0);
      return IfcTools.db.CreateIfcAxis2Placement3D(point, direction1, direction2);
    }

    private static IIfcSpatialStructureElement CreateSpatialStructure(
      IIfcProject project)
    {
      IfcAxis2Placement RelativePlacement1 = IfcSelect.IfcAxis2Placement(IfcTools.CreateDefaultPlacement());
      IIfcLocalPlacement ifcLocalPlacement1 = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) null, RelativePlacement1);
      IIfcSite ifcSite = IfcTools.db.CreateIfcSite(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement1, (IIfcProductRepresentation) null, (IfcLabel) null, new IfcElementCompositionEnum?(IfcElementCompositionEnum.IFC_ELEMENT), (IfcCompoundPlaneAngleMeasure) null, (IfcCompoundPlaneAngleMeasure) null, (IfcLengthMeasure) null, (IfcLabel) null, (IIfcPostalAddress) null);
      List<IIfcObjectDefinition> RelatedObjects1 = new List<IIfcObjectDefinition>()
      {
        (IIfcObjectDefinition) ifcSite
      };
      IfcAxis2Placement RelativePlacement2 = IfcSelect.IfcAxis2Placement(IfcTools.CreateDefaultPlacement());
      IIfcLocalPlacement ifcLocalPlacement2 = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) ifcLocalPlacement1, RelativePlacement2);
      IfcTools.db.CreateIfcRelAggregatesIFC2X3(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IIfcObjectDefinition) project, RelatedObjects1);
      IIfcPostalAddress ifcPostalAddress = IfcTools.db.CreateIfcPostalAddress(new IfcAddressTypeEnum?(IfcAddressTypeEnum.IFC_OFFICE), (IfcText) "Tekla Office", (IfcLabel) "Office", (IfcLabel) "A1", (List<IfcLabel>) null, (IfcLabel) "PL100", (IfcLabel) "Espoo", (IfcLabel) "Uusimaa", (IfcLabel) "000", (IfcLabel) "FINLAND");
      IIfcBuilding ifcBuilding = IfcTools.db.CreateIfcBuilding(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) ifcLocalPlacement2, (IIfcProductRepresentation) null, (IfcLabel) null, new IfcElementCompositionEnum?(IfcElementCompositionEnum.IFC_ELEMENT), (IfcLengthMeasure) 10.0, (IfcLengthMeasure) 63.0, ifcPostalAddress);
      List<IIfcObjectDefinition> RelatedObjects2 = new List<IIfcObjectDefinition>()
      {
        (IIfcObjectDefinition) ifcBuilding
      };
      IfcAxis2Placement RelativePlacement3 = IfcSelect.IfcAxis2Placement(IfcTools.CreateDefaultPlacement());
      IfcTools.defaultPlacement = IfcTools.db.CreateIfcLocalPlacement((IIfcObjectPlacement) ifcLocalPlacement2, RelativePlacement3);
      IfcTools.db.CreateIfcRelAggregatesIFC2X3(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IIfcObjectDefinition) ifcSite, RelatedObjects2);
      IIfcBuildingStorey ifcBuildingStorey = IfcTools.db.CreateIfcBuildingStorey(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IfcLabel) null, (IIfcObjectPlacement) IfcTools.defaultPlacement, (IIfcProductRepresentation) null, (IfcLabel) null, new IfcElementCompositionEnum?(IfcElementCompositionEnum.IFC_ELEMENT), (IfcLengthMeasure) null);
      List<IIfcObjectDefinition> RelatedObjects3 = new List<IIfcObjectDefinition>()
      {
        (IIfcObjectDefinition) ifcBuildingStorey
      };
      IfcTools.db.CreateIfcRelAggregatesIFC2X3(IfcTools.GenerateIfcGuid(), IfcTools.defaultUser, (IfcLabel) null, (IfcText) null, (IIfcObjectDefinition) ifcBuilding, RelatedObjects3);
      return (IIfcSpatialStructureElement) ifcBuildingStorey;
    }

    private static List<IIfcPresentationStyleAssignment> CreateDefaultStyle()
    {
      return new List<IIfcPresentationStyleAssignment>()
      {
        IfcTools.CreateDefaultColorAssignment()
      };
    }

    private static IIfcPresentationStyleAssignment CreateDefaultColorAssignment()
    {
      IIfcColourRgb ifcColourRgbIfC2X3 = IfcTools.db.CreateIfcColourRgbIFC2X3((IfcLabel) "red", (IfcNormalisedRatioMeasure) 0.8, (IfcNormalisedRatioMeasure) 0.2, (IfcNormalisedRatioMeasure) 0.2);
      List<IfcSurfaceStyleElementSelect> Styles1 = new List<IfcSurfaceStyleElementSelect>()
      {
        new IfcSurfaceStyleElementSelect((IIfcSurfaceStyleShading) IfcTools.db.CreateIfcSurfaceStyleRendering(ifcColourRgbIfC2X3, (IfcNormalisedRatioMeasure) 0.0, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcSpecularHighlightSelect) null, new IfcReflectanceMethodEnum?(IfcReflectanceMethodEnum.IFC_NOTDEFINED)))
      };
      List<IfcPresentationStyleSelect> Styles2 = new List<IfcPresentationStyleSelect>()
      {
        new IfcPresentationStyleSelect(IfcTools.db.CreateIfcSurfaceStyleIFC2X3((IfcLabel) null, new IfcSurfaceSide?(IfcSurfaceSide.IFC_POSITIVE), Styles1))
      };
      return IfcTools.db.CreateIfcPresentationStyleAssignment(Styles2);
    }

    private static IfcGloballyUniqueId GenerateIfcGuid()
    {
      return IfcTools.ConvertToIfcGuid(Guid.NewGuid());
    }

    private static string CurrentTime()
    {
      DateTime universalTime = DateTime.Now.ToUniversalTime();
      return universalTime.Year.ToString() + "-" + universalTime.Month.ToString("d2") + "-" + universalTime.Day.ToString("d2") + "T" + universalTime.Hour.ToString("d2") + ":" + universalTime.Minute.ToString("d2") + ":" + universalTime.Second.ToString("d2");
    }

    private static IfcTimeStamp GetTimeStamp()
    {
      return new IfcTimeStamp((int) Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
    }

    private static string ToBase64(uint base10Bit24, int bitsLeft)
    {
      string str1 = string.Empty;
      if (bitsLeft > 0)
      {
        uint num = base10Bit24 & 63U;
        string str2 = str1 + IfcTools.conversionTable[(int) num].ToString();
        str1 = IfcTools.ToBase64(base10Bit24 >> 6, bitsLeft - 6) + str2;
      }
      return str1;
    }

    private static IIfcUnitAssignment CreateGlobalUnits()
    {
      List<IfcUnit> Units = new List<IfcUnit>()
      {
        new IfcUnit((IIfcNamedUnit) IfcTools.db.CreateIfcSIUnit((IIfcDimensionalExponents) null, new IfcUnitEnum?(IfcUnitEnum.IFC_LENGTHUNIT), new IfcSIPrefix?(IfcSIPrefix.IFC_MILLI), new IfcSIUnitName?(IfcSIUnitName.IFC_METRE))),
        new IfcUnit((IIfcNamedUnit) IfcTools.db.CreateIfcSIUnit((IIfcDimensionalExponents) null, new IfcUnitEnum?(IfcUnitEnum.IFC_PLANEANGLEUNIT), new IfcSIPrefix?(), new IfcSIUnitName?(IfcSIUnitName.IFC_RADIAN)))
      };
      return IfcTools.db.CreateIfcUnitAssignment(Units);
    }

    private static IIfcPresentationStyleAssignment CreateDefaultColorAssignment(
      string colorString)
    {
      List<double> doubleList = new List<double>();
      if (colorString.Equals("red"))
      {
        doubleList.Add(1.0);
        doubleList.Add(0.27);
        doubleList.Add(0.0);
      }
      else if (colorString.Equals("blue"))
      {
        doubleList.Add(0.41);
        doubleList.Add(0.35);
        doubleList.Add(0.8);
      }
      else if (colorString.Equals("green"))
      {
        doubleList.Add(0.18);
        doubleList.Add(0.55);
        doubleList.Add(0.34);
      }
      else if (colorString.Equals("yellow"))
      {
        doubleList.Add(1.0);
        doubleList.Add(0.84);
        doubleList.Add(0.0);
      }
      else if (colorString.Equals("steelblue"))
      {
        doubleList.Add(0.28);
        doubleList.Add(0.53);
        doubleList.Add(0.67);
      }
      else if (colorString.Equals("beige"))
      {
        doubleList.Add(0.96);
        doubleList.Add(0.64);
        doubleList.Add(0.38);
      }
      else if (colorString.Equals("brown"))
      {
        doubleList.Add(0.63);
        doubleList.Add(0.32);
        doubleList.Add(0.18);
      }
      else if (colorString.Equals("grey"))
      {
        doubleList.Add(0.54);
        doubleList.Add(0.52);
        doubleList.Add(0.51);
      }
      else if (colorString.Equals("pink"))
      {
        doubleList.Add(1.0);
        doubleList.Add(0.75);
        doubleList.Add(0.8);
      }
      else if (colorString.Equals("purple"))
      {
        doubleList.Add(0.63);
        doubleList.Add(0.13);
        doubleList.Add(0.94);
      }
      else if (colorString.Equals("lightblue"))
      {
        doubleList.Add(0.0);
        doubleList.Add(0.81);
        doubleList.Add(0.82);
      }
      else
      {
        doubleList.Add(0.5);
        doubleList.Add(0.5);
        doubleList.Add(0.5);
      }
      IIfcColourRgb ifcColourRgbIfC2X3 = IfcTools.db.CreateIfcColourRgbIFC2X3((IfcLabel) colorString, (IfcNormalisedRatioMeasure) doubleList[0], (IfcNormalisedRatioMeasure) doubleList[1], (IfcNormalisedRatioMeasure) doubleList[2]);
      List<IfcSurfaceStyleElementSelect> Styles1 = new List<IfcSurfaceStyleElementSelect>()
      {
        new IfcSurfaceStyleElementSelect((IIfcSurfaceStyleShading) IfcTools.db.CreateIfcSurfaceStyleRendering(ifcColourRgbIfC2X3, (IfcNormalisedRatioMeasure) 0.0, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcColourOrFactor) null, (IfcSpecularHighlightSelect) null, new IfcReflectanceMethodEnum?(IfcReflectanceMethodEnum.IFC_NOTDEFINED)))
      };
      List<IfcPresentationStyleSelect> Styles2 = new List<IfcPresentationStyleSelect>()
      {
        new IfcPresentationStyleSelect(IfcTools.db.CreateIfcSurfaceStyleIFC2X3((IfcLabel) null, new IfcSurfaceSide?(IfcSurfaceSide.IFC_POSITIVE), Styles1))
      };
      return IfcTools.db.CreateIfcPresentationStyleAssignment(Styles2);
    }

    private static IIfcRepresentationItem FetchRepresentation(
      IIfcElement element)
    {
      IIfcProductRepresentation representation1 = element.Representation;
      if (representation1 != null)
      {
        foreach (IIfcRepresentation representation2 in representation1.Representations)
        {
          if (representation2.Items != null)
          {
            foreach (IIfcRepresentationItem representationItem in representation2.Items)
            {
              if (representationItem is IIfcSolidModel)
                return representationItem;
              if (representationItem is IIfcBooleanClippingResult)
                return (IIfcRepresentationItem) ((IIfcBooleanClippingResult) representationItem).FirstOperand.sIfcSolidModel;
            }
          }
        }
      }
      return (IIfcRepresentationItem) null;
    }

    private static List<List<Point>> CreateSegments(
      List<Point> points,
      List<double> chamfers)
    {
      List<List<Point>> pointListList = new List<List<Point>>();
      for (int index1 = 0; index1 < points.Count; ++index1)
      {
        int index2 = index1 > 0 ? index1 - 1 : points.Count - 1;
        int index3 = index1 > points.Count - 1 ? 0 : index1;
        int index4 = index2 > 0 ? index2 - 1 : points.Count - 1;
        int index5 = index3 >= points.Count - 1 ? 0 : index3 + 1;
        Point point1 = points[index2];
        Point point2 = points[index3];
        Point point3 = points[index4];
        Point point4 = points[index5];
        Math.Abs(chamfers[index3]);
        Point center1 = (Point) null;
        Point center2 = (Point) null;
        List<Point> pointList1 = new List<Point>();
        List<Point> pointList2 = new List<Point>();
        double chamfer = chamfers[index2];
        if (!chamfer.Equals(0.0))
          pointList1.AddRange((IEnumerable<Point>) Tools.GetCenterPoint(point3, point1, point2, chamfers[index2], out center1));
        chamfer = chamfers[index3];
        if (!chamfer.Equals(0.0))
          pointList2.AddRange((IEnumerable<Point>) Tools.GetCenterPoint(point1, point2, point4, chamfers[index3], out center2));
        if (center1 != (Point) null)
          point1 = new Point(pointList1[1]);
        if (center2 != (Point) null)
          point2 = new Point(pointList2[0]);
        if (!point1.Equals((object) point2))
          pointListList.Add(new List<Point>()
          {
            point1,
            point2
          });
        if (center2 != (Point) null)
        {
          Vector vector1 = new Vector(point2 - center2);
          Vector vector2 = new Vector(new Point(pointList2[1]) - center2);
          double point5 = Distance.PointToPoint(point2, center2);
          vector2.Normalize(point5);
          Point point6 = new Point(center2 + (Point) vector2);
          pointListList.Add(new List<Point>()
          {
            point2,
            center2,
            point6
          });
        }
      }
      return pointListList;
    }

    private static List<IIfcCompositeCurveSegment> CreateCompositeCurve(
      List<List<Point>> segments)
    {
      List<IIfcCompositeCurveSegment> compositeCurveSegmentList = new List<IIfcCompositeCurveSegment>();
      foreach (List<Point> segment in segments)
      {
        int count = segment.Count;
        if (count.Equals(2))
        {
          IIfcPolyline ifcPolyline = IfcTools.db.CreateIfcPolyline(new List<IIfcCartesianPoint>()
          {
            IfcTools.CreatePoint(segment[0].X, segment[0].Y),
            IfcTools.CreatePoint(segment[1].X, segment[1].Y)
          });
          IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.db.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(true), (IIfcCurve) ifcPolyline);
          compositeCurveSegmentList.Add(compositeCurveSegment);
        }
        else
        {
          count = segment.Count;
          if (count.Equals(3))
          {
            Vector vector1 = new Vector(segment[0] - segment[1]);
            vector1.Normalize();
            Vector Vector = new Vector(segment[2] - segment[1]);
            Vector.Normalize();
            double point1 = Distance.PointToPoint(segment[0], segment[1]);
            bool flag = true;
            Vector vector2 = new Vector((Point) vector1);
            if (vector1.Cross(Vector).Z < 0.0)
            {
              vector2 = new Vector((Point) Vector);
              flag = false;
            }
            double angleBetween = vector1.GetAngleBetween(Vector);
            IIfcCartesianPoint point2 = IfcTools.CreatePoint(segment[1].X, segment[1].Y);
            IIfcDirection ifcDirection = IfcTools.db.CreateIfcDirection(new List<double>()
            {
              vector2.X,
              vector2.Y
            });
            IfcAxis2Placement Position = new IfcAxis2Placement(IfcTools.db.CreateIfcAxis2Placement2D(point2, ifcDirection));
            IIfcCircle ifcCircle1 = IfcTools.db.CreateIfcCircle(Position, new IfcPositiveLengthMeasure(point1));
            IfcDatabaseAPI db = IfcTools.db;
            IIfcCircle ifcCircle2 = ifcCircle1;
            List<IfcTrimmingSelect> Trim1 = new List<IfcTrimmingSelect>();
            Trim1.Add((IfcTrimmingSelect) new IfcParameterValue(0.0));
            List<IfcTrimmingSelect> Trim2 = new List<IfcTrimmingSelect>();
            Trim2.Add((IfcTrimmingSelect) new IfcParameterValue(angleBetween));
            bool? SenseAgreement = new bool?(true);
            IfcTrimmingPreference? MasterRepresentation = new IfcTrimmingPreference?(IfcTrimmingPreference.IFC_PARAMETER);
            IIfcTrimmedCurve ifcTrimmedCurve = db.CreateIfcTrimmedCurve((IIfcCurve) ifcCircle2, Trim1, Trim2, SenseAgreement, MasterRepresentation);
            IIfcCompositeCurveSegment compositeCurveSegment = IfcTools.db.CreateIfcCompositeCurveSegment(new IfcTransitionCode?(IfcTransitionCode.IFC_CONTINUOUS), new bool?(flag), (IIfcCurve) ifcTrimmedCurve);
            compositeCurveSegmentList.Add(compositeCurveSegment);
          }
        }
      }
      return compositeCurveSegmentList;
    }

    private static void RemoveZeroRadius(ref List<List<Point>> segments, double delta)
    {
      List<List<Point>> pointListList = new List<List<Point>>();
      for (int index = 0; index < segments.Count; ++index)
      {
        if (segments[index].Count.Equals(3))
        {
          if (Distance.PointToPoint(segments[index][0], segments[index][1]) > delta)
            pointListList.Add(segments[index]);
        }
        else
          pointListList.Add(segments[index]);
      }
      segments.Clear();
      segments.AddRange((IEnumerable<List<Point>>) pointListList);
    }

    private static void RemoveZeroSegment(ref List<List<Point>> segments, double delta)
    {
      for (int index1 = 0; index1 < segments.Count; ++index1)
      {
        int index2 = index1 > 0 ? index1 - 1 : segments.Count - 1;
        int index3 = index1 > segments.Count - 1 ? 0 : index1;
        int index4 = index3 >= segments.Count - 1 ? 0 : index3 + 1;
        List<Point> pointList1 = segments[index2];
        List<Point> pointList2 = segments[index3];
        List<Point> pointList3 = segments[index4];
        int count;
        if (!pointList1[pointList1.Count - 1].Equals((object) pointList2[0]))
        {
          count = pointList1.Count;
          if (count.Equals(3))
            pointList2[0] = pointList1[pointList1.Count - 1];
          else
            pointList1[pointList1.Count - 1] = pointList2[0];
        }
        if (!pointList2[pointList2.Count - 1].Equals((object) pointList3[0]))
        {
          count = pointList2.Count;
          if (count.Equals(3))
            pointList3[0] = pointList2[pointList2.Count - 1];
          else
            pointList2[pointList2.Count - 1] = pointList3[0];
        }
      }
    }

    private static void AdjustSegmentPoints(ref List<List<Point>> segments)
    {
      for (int index1 = 0; index1 < segments.Count; ++index1)
      {
        int index2 = index1 > 0 ? index1 - 1 : segments.Count - 1;
        int index3 = index1 > segments.Count - 1 ? 0 : index1;
        int num1 = index3 >= segments.Count - 1 ? 0 : index3 + 1;
        List<Point> pointList1 = segments[index2];
        List<Point> pointList2 = segments[index3];
        int count = pointList1.Count;
        if (count.Equals(2))
          pointList1[1] = new Point(pointList2[0]);
        count = pointList1.Count;
        int num2;
        if (count.Equals(3))
        {
          count = pointList2.Count;
          num2 = count.Equals(2) ? 1 : 0;
        }
        else
          num2 = 0;
        if (num2 != 0)
          pointList2[0] = new Point(pointList1[2]);
      }
    }

    private static void AddSegmentToGap(ref List<List<Point>> segments)
    {
      double num1 = 0.0;
      List<List<Point>> pointListList = new List<List<Point>>();
      for (int index1 = 0; index1 < segments.Count; ++index1)
      {
        int index2 = index1 > 0 ? index1 - 1 : segments.Count - 1;
        int index3 = index1 > segments.Count - 1 ? 0 : index1;
        List<Point> pointList1 = segments[index2];
        List<Point> pointList2 = segments[index3];
        int count = pointList1.Count;
        int num2;
        if (count.Equals(3))
        {
          count = pointList2.Count;
          num2 = count.Equals(3) ? 1 : 0;
        }
        else
          num2 = 0;
        if (num2 != 0)
        {
          if (Distance.PointToPoint(pointList1[2], pointList2[0]) > num1)
          {
            pointListList.Add(new List<Point>()
            {
              new Point(pointList1[2]),
              new Point(pointList2[0])
            });
            pointListList.Add(segments[index1]);
          }
          else
            pointListList.Add(segments[index1]);
        }
        else
          pointListList.Add(segments[index1]);
      }
      segments.Clear();
      segments.AddRange((IEnumerable<List<Point>>) pointListList);
    }
  }
}
