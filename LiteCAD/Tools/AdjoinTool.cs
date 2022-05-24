﻿using LiteCAD.BRep;
using LiteCAD.BRep.Faces;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LiteCAD.Tools
{
    public class AdjoinTool : AbstractTool
    {
        public AdjoinTool(IEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        BRep.BRepFace lastPickedFace;
        IntersectInfo inter1;
        IntersectInfo inter2;
        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (Editor.Pick == null) return;
            if (lastPickedFace != null)
            {
                var f = getFace(Editor.Pick);
                inter2 = Editor.Pick;
                if (f == null) return;

                //adjoint selected faces
                if (f is BRepPlaneFace pf2 && lastPickedFace is BRepPlaneFace pf1)
                {
                    /* List<PartInstance> list = new List<PartInstance>();
                     foreach (var item in Editor.Parts)
                     {
                         list.AddRange(item.GetAll((z) => z is PartInstance).OfType<PartInstance>());
                     }*/
                    //var drw1 = list.FirstOrDefault(z => z.Part.Part == pf1.Parent);
                    //var drw2 = list.FirstOrDefault(z => z.Part.Part == pf2.Parent);
                    var drw1 = inter1.Model as PartInstance;
                    var drw2 = inter2.Model as PartInstance;
                    //calc adjoint matrix
                    if (drw1 != null && drw2 != null)
                    {
                        var plane1 = pf1.Surface as BRep.Surfaces.BRepPlane;
                        var plane2 = pf2.Surface as BRep.Surfaces.BRepPlane;
                        var ploc1 = Vector3d.Transform(plane1.Location, drw1.Matrix.Calc());
                        var ploc2 = Vector3d.Transform(plane2.Location, drw2.Matrix.Calc());
                        var q1 = drw1.Matrix.Calc();
                        var q2 = drw2.Matrix.Calc();
                        var rot1 = q1.ExtractRotation();
                        var rot2 = q2.ExtractRotation();
                        var pnrm1 = Vector3d.Transform(plane1.Normal, rot1);
                        var pnrm2 = Vector3d.Transform(plane2.Normal, rot2);
                        
                        var cross1 = Vector3d.Cross(pnrm1, pnrm2);
                        float eps = 1e-8f;
                        if (Math.Abs(cross1.Length) < eps)
                        {//colinear
                         //just translate

                            if (!drw1.Frozen)
                            {
                                BRep.Surfaces.BRepPlane temp = new BRep.Surfaces.BRepPlane() { Location = ploc2, Normal = pnrm2 };
                                var proj = temp.GetProjPoint(ploc1);
                                proj = proj - ploc1;//shift
                                drw1.Matrix.Items.Add(new TranslateTransformChainItem() { Vector = proj });
                            }
                            else if (!drw2.Frozen)
                            {
                                BRep.Surfaces.BRepPlane temp = new BRep.Surfaces.BRepPlane() { Location = ploc1, Normal = pnrm1 };
                                var proj = temp.GetProjPoint(ploc2);
                                proj = proj - ploc2;//shift
                                drw2.Matrix.Items.Add(new TranslateTransformChainItem() { Vector = proj });
                            }
                        }
                        else
                        {//non-collinear

                        }
                    }
                }
                lastPickedFace = null;
                Editor.ResetTool();
            }
            else
            {
                lastPickedFace = getFace(Editor.Pick);
                inter1 = Editor.Pick;
            }
        }

        BRep.BRepFace getFace(IntersectInfo pick)
        {
            if (!(pick.Model is IPartContainer part && pick.Target != null)) return null;
            MeshNode frr = null;
            if (part is PartInstance pii)
            {
                var mtr1 = pii.Matrix.Calc();
                frr = part.Part.Nodes.FirstOrDefault(zzz => zzz.Contains(pick.Target, mtr1));
            }
            else
            {
                frr = part.Part.Nodes.FirstOrDefault(zzz => zzz.Contains(pick.Target));
            }

            //var frr = part.Part.Nodes.FirstOrDefault(zzz => zzz.Contains(pick.Target));
            if (frr == null) return null;
            var face = frr.Parent;
            return face;
            //return face.Wires.FirstOrDefault(yy => GeometryUtils.Contains(yy, pick.Target));
        }

        public override void MouseUp(MouseEventArgs e)
        {

        }

        public override void Select()
        {

        }

        public override void Update()
        {

        }
    }
}