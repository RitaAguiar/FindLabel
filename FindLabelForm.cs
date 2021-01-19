#region namespaces
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
#endregion // namespaces

namespace FindLabel
{
    public partial class FindLabelForm : System.Windows.Forms.Form // ambiguidade: invocacao da Class Form de SWF e nao de Revit.DB
    {
        // In this sample, the dialog owns the handler and the event objects,
        // but it is not a requirement. They may as well be static properties
        // of the application.

        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        //Variables
        string labelSelected;

        //Dialog instantiation
        public FindLabelForm(ExternalEvent exEvent, RequestHandler handler, UIApplication uiapp) // define o metodo Form1 com um argumento commandData do tipo ExternalCommandData
        {
            Document doc = uiapp.ActiveUIDocument.Document; // doc define o documento ativo na aplicacao

            FamilyManager mgr = doc.FamilyManager;

            Dictionary<string, FamilyParameter> fps = GetFamilyParameters(mgr);

            List<string> labelNames = new List<string>(fps.Keys);

            InitializeComponent(); // inicia o Formulario

            m_Handler = handler;
            m_ExEvent = exEvent;

            #region comboBoxLabels

            comboBoxLabels.Enabled = true;
            comboBoxLabels.Items.Insert(0, "Select Label");
            comboBoxLabels.SelectedItem = 0;
            comboBoxLabels.Text = "Select Label";

            foreach (string i in labelNames)
            {
                comboBoxLabels.Items.AddRange(new object[] { i });
            }

            #endregion //comboBoxlabels
        }

        #region GetFamilyParameters Method

        public Dictionary<string, FamilyParameter> GetFamilyParameters(FamilyManager mgr)
        {
            int n = mgr.Parameters.Size;
            Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);

            foreach (FamilyParameter fp in mgr.Parameters)
            {
                ParameterType fpType = fp.Definition.ParameterType;
                string name = fp.Definition.Name;
                if (fpType == ParameterType.Length || fpType == ParameterType.Angle || fpType == ParameterType.BarDiameter ||
                    fpType == ParameterType.PipeDimension || fpType == ParameterType.Slope || fpType == ParameterType.PipingSlope)
                {
                    fps.Add(name, fp);
                }
            }
            return fps;
        }

        #endregion //GetFamilyParameters Method

        #region Form Items

        private void Form1_Load(object sender, EventArgs e)
        {
            // Turn off validation when a control loses focus. This will be inherited by child
            // controls on the form, enabling us to validate the entire form when the 
            // button is clicked instead of one control at a time.
            AutoValidate = AutoValidate.Disable;
            //Add event handlers
            comboBoxLabels.CausesValidation = true;
            comboBoxLabels.Validating += new CancelEventHandler(comboBoxLabels_Validating);
            //Add child to controls
            Controls.Add(comboBoxLabels);
            //Add event handler
            buttonRun.Click += new EventHandler(buttonRun_Click_1);
        }

        #region Label

        private void comboBoxLabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxLabels.DropDownStyle = ComboBoxStyle.DropDownList; //Prevents manual input
            if (comboBoxLabels.Text != "Select Label")
                labelSelected = comboBoxLabels.Text; // define labelSelected como o texto inserido na comboBoxLabels
        }

        private void comboBoxLabels_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(comboBoxLabels.Text) ||
                string.IsNullOrWhiteSpace(comboBoxLabels.Text) ||
                comboBoxLabels.Text == "Select Label")
            {
                //Cancel the event and select the comboBox to be corrected by the user
                e.Cancel = true;
                comboBoxLabels.Select(0, comboBoxLabels.Text.Length);
                //Set the Error Provider error with the text to display
                errorProvider1.SetError(comboBoxLabels, "Please select a label!");
            }
        }
        private void comboBoxLabels_Validated(object sender, CancelEventArgs e)
        {
            //If all conditions have been met, clear the ErrorProvider of errors
            e.Cancel = false;
            errorProvider1.SetError(comboBoxLabels, null);
        }

        #endregion //Label

        #region Run

        private void buttonRun_Click_1(object sender, EventArgs e) // metodo a ser executado quando button1 e ativado
        {
            //save InputData static variables
            InputData.LabelValue = labelSelected;

            if (ValidateChildren(ValidationConstraints.Enabled))
            {
                //desabilita o botao Run enquanto a tarefa e executada
                buttonRun.Enabled = false;

                //CALLS MAIN METHOD
                MakeRequest(RequestId.FindLabel);

                //habilita o botao Run quando a tarefa e terminada
                buttonRun.Enabled = true;
            }
        }

        #endregion //Run

        #endregion //Form Items

        #region Form Events

        // Form closed event handler
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // we own both the event and the handler
            // we should dispose it before we are closed
            m_ExEvent.Dispose();
            m_ExEvent = null;
            m_Handler = null;

            // do not forget to call the base class
            base.OnFormClosed(e);
        }

        //   Control enabler / disabler 
        private void EnableCommands(bool status)
        {
            foreach (System.Windows.Forms.Control ctrl in Controls)
            {
                ctrl.Enabled = status;
            }
            if (!status)
            {
                this.buttonExit.Enabled = true;
            }
        }

        //A private helper method to make a request
        //and put the dialog to sleep at the same time.

        //    It is expected that the process which executes the request 
        //   (the Idling helper in this particular case) will also
        //   wake the dialog up after finishing the execution.

        private void MakeRequest(RequestId request)
        {
            m_Handler.Request.Make(request);
            m_ExEvent.Raise();
            DozeOff();
        }

        //DozeOff -> disable all controls (but the Exit button)
        private void DozeOff()
        {
            EnableCommands(false);
        }

        //WakeUp -> enable all controls
        public void WakeUp()
        {
            EnableCommands(true);
        }

        //Exit - closing the dialog
        private void buttonExit_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        #endregion //Form Events
    }
}