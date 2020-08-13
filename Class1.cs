#region namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Data;
#endregion // namespaces

namespace FindLabel
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 :  IExternalCommand
    {
        public string IncomingValue { get; set; } // vai buscar e define o valor de IncomingValue
        public List<List<string>> LabelsFound = new List<List<string>>(); // Lista dos Labels encontrados

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application; // app define a aplicacao
            UIDocument uidoc = app.ActiveUIDocument; // uidoc define o documento da aplicacao

            Form1 accessF1 = new Form1(commandData); // nova instancia accessF1 do tipo Form1
            accessF1.ShowDialog(); // mostra a caixa de dialogo do Formulario
            
            return Result.Succeeded; // termino da aplicacao no Revit
        }

        public void FindLabelView(string label, UIDocument ActiveUIDocument) // define o metotdo FindLabelView com dois argumentos label e ActiveUIDocument
        {
            Document doc = ActiveUIDocument.Document; // doc define o documento ativo na aplicacao
            UIDocument uidoc = ActiveUIDocument; // uidoc define o documento da aplicacao
            
            bool IsLabelFound = false; // IsLabelFound e falso
            bool IsAnyLabelFound = false; // IsAnyLabelFound e falso

            List<View> views = new List<View>(); // Lista das vistas dos Labels encontrados
            List<Dimension> dimeensions = new List<Dimension>(); // Lista das dimensões dos Labels encontrados

            List<ElementId> gs = new List<ElementId>(); // Lista das ids dos elementos generic form onde se encontram uma ou mais instâncias do label
            List<IList<ElementId>> cgs = new List<IList<ElementId>>(); // Lista das ids dos elementos (collector) generic form onde se encontram uma ou mais instâncias do label

            #region View specific dimensions

            IEnumerable<Element> collectorD = new FilteredElementCollector(doc).OfClass(typeof(Dimension)); // Lista enumerada de cada Dimensao presente no documento

            FamilyParameter fp = null; // define fp do  tipo FamilyParameter como null
            
            foreach (Dimension dim in collectorD) // para cada dimensao dim no collectorD
            {
                bool IsViewSpec = dim.ViewSpecific; // se dim pertence a uma vista específica

                if (IsViewSpec)
                {
                    try
                    {
                        fp = dim.FamilyLabel; // fp do tipo FamilyParameter e o FamilyLabel de dim
                    }
                    catch
                    {
                        continue; // continua pois dim nao possui FamilyLabal
                    }

                    if (fp != null && fp.Definition.Name == label) // fp nao e null e o nome da definicao de fp do tipo string e igual a label
                    {
                        ICollection<ElementId> selecteddim = new List<ElementId>(new ElementId[] { dim.Id });
                        uidoc.Selection.SetElementIds(selecteddim); // selecciona o elemento dim
                        
                        ReferenceArray dimref = dim.References; // Returns an array of geometric references to which the dimension is attached
                        Location dimloc = dim.Location; // finds the physical location of an element within a project
                        
                        View ownerview = doc.GetElement(dim.OwnerViewId) as View; // ownerview e a vista onde se encontra dim
                        string viewname = ownerview.Name;

                        IsLabelFound = true; // O label foi encontrado

                        if (IsLabelFound == true)
                        {
                            LabelsFound.Add(new List<string> { IncomingValue, viewname }); // Adiciona o label e o nome da vista a lista LabelsFound

                            views.Add(ownerview); // Adiciona a vista deste Label a lista views
                            dimeensions.Add(dim); // Adiciona a dimensao deste Label a lista views

                            IsAnyLabelFound = true; // Um label foi encontrado
                        }
                    }
                }
            }

            #endregion // View specific dimensions

            #region Dimensions inside edit mode

            IEnumerable<ElementId> collectorG = new FilteredElementCollector(doc).OfClass(typeof(GenericForm)).ToElementIds(); // Lista enumerada de cada Forma Generica presente no documento
            List<View> collectorV = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().ToList(); // Lista que contem todas as vistas

            bool IsSketchFound = false;

            foreach (ElementId g in collectorG)
            {
                Dictionary<ElementId, ElementId> sketchRefs = new Dictionary<ElementId, ElementId>();
                ICollection<ElementId> dimsRefs = new List<ElementId>();

                Transaction t = new Transaction(doc, "fake");
                t.Start();
                ICollection<ElementId> ids = doc.Delete(g);
                t.RollBack();

                foreach (ElementId id in ids)
                {
                    Element e = doc.GetElement(id);

                    IsSketchFound = false;

                    if (e is Dimension)
                    {
                        try
                        {
                            Dimension dim = e as Dimension;
                            fp = dim.FamilyLabel; // fp do tipo FamilyParameter e o FamilyLabel de dim
                        }
                        catch
                        {
                            continue; // termina de avaliar esta dimensao pois nao possui FamilyLabal
                        }

                        if (fp != null && fp.Definition.Name == label) // fp nao e null e o nome da definicao de fp do tipo string e igual a label
                        {
                            //If this dimension is not corresponding to the label continue;
                            ReferenceArray ra = (e as Dimension).References;
                            bool IsReferenceFound = false;
                            foreach (Reference r in ra)
                            {
                                if (IsReferenceFound == false)
                                {
                                    dimsRefs.Add(r.ElementId);
                                    IsReferenceFound = true;
                                }
                            }
                        }
                    }

                    else if (e is Sketch)
                    {
                        Sketch s = e as Sketch;
                        CurveArrArray caa = s.Profile;
                        foreach (CurveArray ca in caa)
                        {
                            if (IsSketchFound == false)
                            {
                                foreach (Curve c in ca)
                                {
                                    if (sketchRefs.ContainsKey(c.Reference.ElementId) || sketchRefs.ContainsKey(s.Id))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        sketchRefs.Add(c.Reference.ElementId, s.Id);
                                        IsSketchFound = true;
                                    }
                                }
                            }
                        }
                    }
                }
                //Now we have retrieved all the dimensions in this GenericForm
                //that has the selected label, and also all the sketch references
                //of this GenericForm

                IsLabelFound = false;

                foreach (ElementId id in dimsRefs)
                {
                    if (!sketchRefs.ContainsKey(id)) continue;
                    ElementId sketchId = sketchRefs[id];
                    Sketch sketch = doc.GetElement(sketchId) as Sketch;
                    SketchPlane sp = sketch.SketchPlane;

                    foreach (View view in collectorV)
                    {
                        if (sketch.SketchPlane.GetPlane().Normal.IsAlmostEqualTo(view.ViewDirection) && IsLabelFound == false)
                        // comparacao entre o sketch e com todas as vistas contidas no projeto
                        {
                            uidoc.ActiveView = view; // abre a vista onde encontra dim

                            Element e = doc.GetElement(g); // elemento onde se encontra a label

                            string viewname = view.Name; // viewname do tipo string e o nome da view

                            IList<ElementId> cg = new List<ElementId>(new ElementId[] {g} ); // IList -> ICollector para selecionar o elemento e de id g

                            IsLabelFound = true; // O label foi encontrado
                            
                            if (IsLabelFound == true)
                            {
                                LabelsFound.Add( new List<string> { label, g.ToString(), viewname });

                                // gs e a lista com todas as ids das generic forms gs onde se encontram uma ou mais instâncias do label
                                gs.Add(g);

                                //cgs e a lista dos elementos de coletor de generic form cg onde se encontra uma ou mais instâncias do label
                                cgs.Add(cg);

                                IsAnyLabelFound = true; // Um label foi encontrado
                            }
                        }
                    }
                }
            }
            #endregion // Dimensions inside edit mode

            if (IsAnyLabelFound == true) // se IsLabelFound e true
            {
                TaskDialog.Show("FindLabel",
                    string.Format("Número de dimensões encontradas: {0}.", LabelsFound.Count())); // mostra a caixa de dialogo que confirma quantos labels foram encontrados

                int n = 0;
                int n2 = 0;

                foreach (var sublist in LabelsFound)
                {
                    DataGridViewRevit accessDGVR = new DataGridViewRevit(); // nova instancia accessDGVR do tipo DataGridViewRevit

                    if (sublist.Count() == 2)
                    {
                        uidoc.ActiveView = views[n]; // abre a vista onde encontra dim
                        UIView uiview = uidoc.GetOpenUIViews().FirstOrDefault(q => q.ViewId == dimeensions[n].OwnerViewId); // uiview e a vista aberta onde encontra dim 
                        BoundingBoxXYZ bbox = dimeensions[n].get_BoundingBox(views[n]); // Dimensões da janela da vista 
                        uiview.ZoomAndCenterRectangle(bbox.Min, bbox.Max); // abre a vista uivew segundo as dimensões da janela definidas em bbox
                        
                        // mostra a caixa de dialogo que confirma que o label foi encontrado
                        TaskDialog.Show("FindLabel",
                                string.Format("Foi encontrada uma dimensao com a etiqueta {0} na vista {1}.", sublist[0], sublist[1]));

                        ++n; // n+1
                    }
                    else if (sublist.Count() == 3)
                    {
                        uidoc.Selection.SetElementIds(cgs[n2]); // selecao do elemento e de id g
                        uidoc.ShowElements(gs[n2]); // zoom to fit no elemento e de id g

                        // mostra a caixa de dialogo que confirma que o label foi encontrado
                        TaskDialog.Show("FindLabel",
                            string.Format("Foi encontrada uma dimensao com a etiqueta {0} no modo edicao do elemento selecionado, com a id {1}, na vista {2}.", sublist[0], sublist[1], sublist[2]));

                        ++n2; // n2+1
                    }
                }
            }

            if (IsAnyLabelFound == false) // se IsLabelFound e falso
            {
                TaskDialog.Show("FindLabel",
                    string.Format("Nao existe nenhuma dimensao associada a etiqueta {0}.", IncomingValue)); // mostra a caixa de dialogo que confirma que o label nao foi encontrado
            }
        }
    
        public List<List<string>> ReturnLabelsFoundList()
        {
            return LabelsFound;
        }
    }
}
