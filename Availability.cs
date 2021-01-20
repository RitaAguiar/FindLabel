#region namespaces
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
#endregion //namespaces

namespace FindLabel
{
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                Document doc = applicationData.ActiveUIDocument.Document;

                // Following good SOA practices, first validate incoming parameters
                if (doc == null)
                {
                    //throw new ArgumentNullException("doc");
                    return false;
                }

                else if (doc.IsFamilyDocument)
                {
                    //throw new Exception("This plugin cannot be run on a family document.");
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
