//
// (C) Copyright 2003-2019 by Autodesk, Inc. All rights reserved.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.

//
// AUTODESK PROVIDES THIS PROGRAM 'AS IS' AND WITH ALL ITS FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable. 

#region namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.UI;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
#endregion //namespaces

namespace FindLabel
{
    // Implements the Revit add-in interface IExternalApplication
    class Application : IExternalApplication
    {
        // Message used as ribbon panel button tooltip 
        // and displayed by the external command
        public const string Message =
          "Find labels within dimensions associated to modeled elements.";

        static void AddRibbonPanel(
          UIControlledApplication a)
        {
            // Method to add Tab and Panel 
            RibbonPanel panel = ribbonPanel(a);

            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData data = new PushButtonData(
              "Labels", "Find Label", path, "FindLabel.Command");

            Bitmap bitmapicon16 = Properties.Resources.icon16;
            BitmapSource icon16 = BitmapToBitmapSource(bitmapicon16);

            Bitmap bitmapicon32 = Properties.Resources.icon32;
            BitmapSource icon32 = BitmapToBitmapSource(bitmapicon32);

            PushButton pushbutton = panel.AddItem(data) as PushButton;

            pushbutton.Image = icon16;
            pushbutton.LargeImage = icon32;
            pushbutton.ToolTip = Message;
        }

        #region BmpImageSource Method
        public ImageSource BmpImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }
        #endregion BmpImageSource Method

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        // Convert a Bitmap to a BitmapSource
        static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return retval;
        }
                
        public static RibbonPanel ribbonPanel(UIControlledApplication a)
        {
            string tab = "Rita Aguiar Plugins"; // Tab name
                                                // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch { }
            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, "Finishes");
            }
            catch { }
            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == "Finishes")
                {
                    ribbonPanel = p;
                }
            }
            //return panel 
            return ribbonPanel;
        }

        // class instance
        public static Application thisApp = null;
        // ModelessForm instance
        private FindLabelForm formFL;

        #region IExternalApplication Members

        // Implements the OnShutdown event
        public Result OnShutdown(UIControlledApplication application)
        {
            if (formFL != null && formFL.Visible)
            {
                formFL.Close();
            }

            return Result.Succeeded;
        }

        // Implements the OnStartup event
        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);
            formFL = null;   // no dialog needed yet; the command will bring it
            thisApp = this;  // static access to this application instance

            return Result.Succeeded;
        }

        //   This method creates and shows a modeless dialog, unless it already exists.
        //   The external command invokes this on the end-user's request

        public void ShowForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (formFL == null || formFL.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                RequestHandler handler = new RequestHandler();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible for disposing them, eventually.
                formFL = new FindLabelForm(exEvent, handler, uiapp);
                formFL.Show();
            }
        }

        //   Waking up the dialog from its waiting state.
        public void WakeFormUp()
        {
            if (formFL != null)
            {
                formFL.WakeUp();
            }
        }
        #endregion IExternalApplication Members
    }
}