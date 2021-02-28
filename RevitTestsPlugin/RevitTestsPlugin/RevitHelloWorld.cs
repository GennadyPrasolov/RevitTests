using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace RevitTestsPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RevitHelloWorld : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
                       ref string message,
                      ElementSet elements)
        {
            MessageBox.Show("Hello revit");
            return Result.Succeeded;
        }

    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class GetElementsInfo : IExternalCommand
    {
        public Result Execute(
         ExternalCommandData commandData,
                      ref string message,
                     ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Reference pickedRef = null;

            Selection sel = uiApp.ActiveUIDocument.Selection;
            pickedRef = sel.PickObject(ObjectType.Element, "Select element");

            Element elem = doc.GetElement(pickedRef);

            String prompt = "Show parameters in selected Element: \n\r";
            StringBuilder st = new StringBuilder();

            foreach (Parameter para in elem.Parameters)
            {
                st.AppendLine(getParameterInformation(para, doc));
            }

            // Give the user some information
            TaskDialog.Show("Revit", prompt + st.ToString());

            return Result.Succeeded;
        }



        String getParameterInformation(Parameter para, Document document)
        {
            string defName = para.Definition.Name + "\t : ";
            string defValue = string.Empty;
            // Use different method to get parameter data according to the storage type
            switch (para.StorageType)
            {
                case StorageType.Double:
                    //covert the number into Metric
                    defValue = para.AsValueString();
                    break;
                case StorageType.ElementId:
                    //find out the name of the element
                    Autodesk.Revit.DB.ElementId id = para.AsElementId();
                    if (id.IntegerValue >= 0)
                    {
                        defValue = document.GetElement(id).Name;
                    }
                    else
                    {
                        defValue = id.IntegerValue.ToString();
                    }
                    break;
                case StorageType.Integer:
                    if (ParameterType.YesNo == para.Definition.ParameterType)
                    {
                        if (para.AsInteger() == 0)
                        {
                            defValue = "False";
                        }
                        else
                        {
                            defValue = "True";
                        }
                    }
                    else
                    {
                        defValue = para.AsInteger().ToString();
                    }
                    break;
                case StorageType.String:
                    defValue = para.AsString();
                    break;
                default:
                    defValue = "Unexposed parameter.";
                    break;
            }

            return defName + defValue;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class GetFamilyInfo : IExternalCommand
    {
        public Result Execute(
        ExternalCommandData commandData,
                     ref string message,
                    ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Reference pickedRef = null;

            Selection sel = uiApp.ActiveUIDocument.Selection;
            pickedRef = sel.PickObject(ObjectType.Element, "Select element");

            Element elem = doc.GetElement(pickedRef);
            Family family = elem as Family;

            StringBuilder msg = new StringBuilder();

            if (family == null)
            {
                msg.AppendLine("There is not a family");
            }
            else
            {
                msg.AppendLine("Selected element's family name is : " + family.Name);
                ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();

                if (familySymbolIds.Count == 0)
                {
                    msg.AppendLine("Contains no family symbols.");
                }
                else
                {
                    msg.AppendLine("The family symbols contained in this family are : ");

                    // Get family symbols which is contained in this family
                    foreach (ElementId id in familySymbolIds)
                    {
                        FamilySymbol familySymbol = family.Document.GetElement(id) as FamilySymbol;
                        // Get family symbol name
                        msg.AppendLine("\nName: " + familySymbol.Name);
                        foreach (ElementId materialId in familySymbol.GetMaterialIds(false))
                        {
                            Material material = familySymbol.Document.GetElement(materialId) as Material;
                            msg.AppendLine("\nMaterial : " + material.Name);
                        }
                    }
                }
            }

            TaskDialog.Show("Revit", msg.ToString());
            return Result.Succeeded;
        }
    }


    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class GetAllFamilies : IExternalCommand
    {
        public Result Execute(
        ExternalCommandData commandData,
                     ref string message,
                    ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                Document doc = uiApp.ActiveUIDocument.Document;
                FilteredElementCollector families = new FilteredElementCollector(doc).OfClass(typeof(Family));

                StringBuilder msg = new StringBuilder();

                foreach (Family family in families)
                {
                    msg.Append("\n" + family.Name);
                }

                TaskDialog.Show("Revit", msg.ToString());

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

}