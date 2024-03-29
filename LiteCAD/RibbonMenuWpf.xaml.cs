﻿using BREP.BRep;
using LiteCAD.BRep;
using LiteCAD.Common;
using LiteCAD.DraftEditor;
using LiteCAD.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiteCAD
{
    /// <summary>
    /// Interaction logic for RibbonMenuWpf.xaml
    /// </summary>
    public partial class RibbonMenuWpf : UserControl
    {
        public RibbonMenuWpf()
        {
            InitializeComponent();

            RibbonWin.Loaded += RibbonWin_Loaded;
        }

        private void RibbonWin_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        public Form1 Form => Form1.Form;

        private void adjointButton_Click(object sender, RoutedEventArgs e)
        {
            Form.adjointUI();
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            Form.openUI();
        }


        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {

            Form.selectorUI();

        }


        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            Form.fitAll();

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Form.ExitDraft();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Form.ExportDraftToDxf();
        }

        private void Rectangle_Click(object sender, RoutedEventArgs e)
        {
            Form.RectangleStart();

        }

        private void RibbonButton_Click_3(object sender, RoutedEventArgs e)
        {
            Form.Extrude();

        }

        private void RibbonButton_Click_4(object sender, RoutedEventArgs e)
        {
            Form.SaveAs();
        }

        private void Line_Click(object sender, RoutedEventArgs e)
        {
            Form.LineStart();
        }

        private void Circle_Click(object sender, RoutedEventArgs e)
        {
            Form.CircleStart();
        }

        private void CloseLine_Click(object sender, RoutedEventArgs e)
        {
            Form.de.CloseLine();
        }

        private void linearConstrBtn_Click(object sender, RoutedEventArgs e)
        {
            Form.SetTool(new LinearConstraintTool(Form.de));
        }

        private void Selection_Click(object sender, RoutedEventArgs e)
        {
            Form.selectorUI();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ab.ShowDialog();

        }

        private void RibbonToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RibbonToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Form.ShowNormalsToggle(showNormalsToggle.IsChecked.Value);
        }

        private void RibbonButton_Click_5(object sender, RoutedEventArgs e)
        {
            Form.ResetCamera();

        }

        private void RibbonButton_Click_6(object sender, RoutedEventArgs e)
        {
            Form.ViewX();
        }

        private void allSizesInline_Click(object sender, RoutedEventArgs e)
        {
            Form.de.Backup();
            var aa = Form.de.Draft.Helpers.OfType<LinearConstraintHelper>();
            foreach (var item in aa)
            {
                item.Shift = 0;
            }
        }

        private void ponitAnchor_Click(object sender, RoutedEventArgs e)
        {
            Form.PointAnchor();
        }

        private void solveCsp_Click(object sender, RoutedEventArgs e)
        {
            Form.SolveCSP();
        }

        private void randSolve_Click(object sender, RoutedEventArgs e)
        {
            Form.RandomSolve();
        }

        private void topoConstr_Click(object sender, RoutedEventArgs e)
        {
            Form.AddTopologyContstraint();
        }

        private void RibbonButton_Click_7(object sender, RoutedEventArgs e)
        {
            Form.ViewY();
        }

        private void RibbonButton_Click_8(object sender, RoutedEventArgs e)
        {
            Form.ViewZ();
        }

        private void undo_Click(object sender, RoutedEventArgs e)
        {
            Form.Undo();
        }

        private void offset_Click(object sender, RoutedEventArgs e)
        {
            Form.de.OffsetUI();
        }

        private void RibbonButton_Click_9(object sender, RoutedEventArgs e)
        {
            Form.camera1.CamUp *= -1;
        }

        private void RibbonButton_Click_10(object sender, RoutedEventArgs e)
        {
            var d = Form.camera1.CameraFrom - Form.camera1.CameraTo;
            Form.camera1.CamFrom = Form.camera1.CameraTo - d;
        }

        private void vertConstrBtn_Click(object sender, RoutedEventArgs e)
        {
            Form.SetTool(new VerticalConstraintTool(Form.de));
        }

        private void horConstrBtn_Click(object sender, RoutedEventArgs e)
        {
            Form.SetTool(new HorizontalConstraintTool(Form.de));
        }

        private void equalConstr_Click(object sender, RoutedEventArgs e)
        {
            Form.SetTool(new EqualsConstraintTool(Form.de));
        }

        private void hVis_Click(object sender, RoutedEventArgs e)
        {
            Form.de.ShowHelpers = !hVis.IsChecked.Value;
        }

        private void flipH_Click(object sender, RoutedEventArgs e)
        {
            Form.de.FlipHorizontal();
        }

        private void flipV_Click(object sender, RoutedEventArgs e)
        {
            Form.de.FlipVertical();
        }

        private void translate_Click(object sender, RoutedEventArgs e)
        {
            Form.de.TranslateUI();
        }

        private void RibbonButton_Click_11(object sender, RoutedEventArgs e)
        {
            Form.Scene = new LiteCADScene();
            Form.InfoPanel.Clear();
            
            BRepFace.NewId = 0;
            FactoryHelper.NewId = 0;
            Form.de.SetDraft(new Common.Draft());
            Form.updateList();
            Form.ResetCamera();

        }

        private void array_Click(object sender, RoutedEventArgs e)
        {
            Form.de.ArrayUI();
            
        }

        private void Image_Click(object sender, RoutedEventArgs e)
        {
            Form.de.AddImage();
        }

        private void CutEdge_Click(object sender, RoutedEventArgs e)
        {
            Form.CutEdgeStart();
        }

        private void RibbonButton_Click_12(object sender, RoutedEventArgs e)
        {
            Form.Backup();
        }

        private void Hex_Click(object sender, RoutedEventArgs e)
        {
            Form.HexStart();
        }

        private void RibbonButton_Click_13(object sender, RoutedEventArgs e)
        {
            Form.Merge();
        }
    }
}
