#region namespaces
using System;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
#endregion // namespaces

namespace FindLabel
{
    public partial class Form1 : System.Windows.Forms.Form // ambiguidade: invocação da Class Form de SWF e não de Revit.DB
    {
        UIDocument uidoc; // define uidoc como do tipo UIDocument

        List<string> items = new List<string>();

        public Form1(ExternalCommandData commandData) // define o método Form1 com um argumento commandData do tipo ExternalCommandData
        {
            UIApplication app = commandData.Application; // define app do tipo UIApplication como a propriedade Application de commandData
            uidoc = app.ActiveUIDocument; // define uidoc como a propriedade ActiveUIDocument de app
            Document doc = app.ActiveUIDocument.Document; // doc define o documento ativo na aplicação

            FamilyManager mgr = doc.FamilyManager;

            int n = mgr.Parameters.Size;

            Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);
            
            foreach (FamilyParameter fp in mgr.Parameters)
            {
                ParameterType fpType = fp.Definition.ParameterType;
                string name = fp.Definition.Name;
                if(fpType == ParameterType.Length || fpType == ParameterType.Angle || fpType == ParameterType.BarDiameter ||
                    fpType == ParameterType.PipeDimension || fpType == ParameterType.Slope || fpType == ParameterType.PipingSlope)
                {
                    fps.Add(name, fp);
                    items.Add(name);
                }
            }
            
            InitializeComponent(); // inicia o Formulário

            foreach (string i in items)
            {
                comboBox1.Items.AddRange(new object[] { i });
            }
        }

        public Class1 accessC1 = new Class1(); // nova instancia accessC1 do tipo Class1

        private void button1_Click(object sender, EventArgs e) // método a ser executado quando button1 é ativado
        {
            accessC1.FindLabelView(accessC1.IncomingValue, uidoc); // executa o método FindLabelView (definido em Class1) com dois argumentos IncomingValue e uidoc
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; //Prevents manual input
            accessC1.IncomingValue = comboBox1.Text; // define IncomingValue (definido em Class1) como o texto inserido na textBox1
        }
    }
}
