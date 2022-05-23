﻿using LiteCAD.BRep;
using LiteCAD.BRep.Faces;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class ExtrudeModifier : IDrawable, IEconomicsDetail, IPartContainer, IMeshNodesContainer
    {
        MeshNode[] IMeshNodesContainer.Nodes
        {
            get
            {
                var mtr = Matrix.Calc();
                List<MeshNode> ret = new List<MeshNode>();
                foreach (var item in Part.Nodes)
                {
                    var mn = new MeshNode();
                    foreach (var titem in item.Triangles)
                    {
                        mn.Triangles.Add(titem.Multiply(mtr));
                    }
                    ret.Add(mn);
                }
                //Part.Nodes[0].Triangles[0].Multiply(mtr)
                return ret.ToArray();
            }
        }

        public ExtrudeModifier(Draft draft)
        {
            Source = draft;
            Source.Parent = this;
            CreatePart();
            Childs.Add(draft);
            Id = FactoryHelper.NewId++;

        }

        Part IPartContainer.Part => _part;

        public ExtrudeModifier(XElement item)
        {
            ctor = true;
            if (item.Attribute("visible") != null)            
                Visible = bool.Parse(item.Attribute("visible").Value);

            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            Source = new Draft(item.Element("source").Element("draft"));
            Height = Helpers.ParseDecimal(item.Attribute("height").Value);
            Source.Parent = this;
            CreatePart();
            Childs.Add(Source);
            Id = int.Parse(item.Attribute("id").Value);
            ctor = false;

        }

        private void CreatePart()
        {
            //draft->brep
            _part = new Part();
            var bottomFace = new BRepPlaneFace(_part) { Surface = new BRepPlane() { Normal = Source.Plane.Normal, Location = Source.Plane.Position } };
            _part.Faces.Add(bottomFace);

            var basis = Source.Plane.GetBasis();
            var top = Vector3d.Cross(basis[0], basis[1]);

            //geta all wires
            var wires = Source.GetWires();
            foreach (var wire in wires)
            {
                List<BRep.BRepEdge> edges = new List<BRep.BRepEdge>();

                if (wire.Any(z => z is DraftLine))
                {
                    foreach (var item in wire.OfType<DraftLine>())
                {
                    var diff = item.V0.Location - item.V1.Location;
                    var p = new OpenTK.Vector3d(item.V0.Location.X, item.V0.Location.Y, 0);
                    var pe = new OpenTK.Vector3d(item.V1.Location.X, item.V1.Location.Y, 0);
                    edges.Add(new BRep.BRepEdge()
                    {
                        Curve = new BRep.Curves.BRepLineCurve()
                        {
                            Point = p,
                            Vector = new OpenTK.Vector3d(diff.X, diff.Y, 0)
                        },
                        Start = p,
                        End = pe
                    });
                }
                }
                else if (wire.Any(z => z is DraftEllipse))
                {
                    foreach (var item in wire.OfType<DraftEllipse>())
                    {
                        var p = new OpenTK.Vector3d(item.Center.X + (double)item.Radius, item.Center.Y, 0);
                        var dir2 = (p.Xy - item.Center.Location);
                        var dir3 = new Vector3d(dir2.X, dir2.Y, 0);

                        edges.Add(new BRep.BRepEdge()
                        {
                            Curve = new BRep.Curves.BRepCircleCurve()
                            {
                                Dir = dir3,
                                Radius = (double)item.Radius,
                                Axis = top,
                                SweepAngle = Math.PI * 2,
                                Location = new Vector3d(item.Center.X, item.Center.Y, 0)
                            },
                            Start = p,
                            End = p
                        }); ;
                    }
                }
                bottomFace.Wires.Add(new BRep.BRepWire() { Edges = edges });

            }
            var shift = Source.Plane.Normal * (double)Height;
            var topFace = new BRepPlaneFace(_part) { Surface = new BRepPlane() { Normal = -Source.Plane.Normal, Location = Source.Plane.Position + Source.Plane.Normal * (double)Height } };

            foreach (var item in bottomFace.Wires)
            {
                List<BRep.BRepEdge> edges = new List<BRep.BRepEdge>();
                foreach (var edge in item.Edges)
                {
                    var ne = new BRep.BRepEdge();
                    edges.Add(ne);
                    ne.Start = edge.Start + shift;
                    ne.End = edge.End + shift;
                    if (edge.Curve is BRep.Curves.BRepLineCurve)
                    {
                    var cc = edge.Curve as BRep.Curves.BRepLineCurve;
                    ne.Curve = new BRep.Curves.BRepLineCurve() { Point = cc.Point + shift, Vector = cc.Vector };
                }
                    else if (edge.Curve is BRep.Curves.BRepCircleCurve)
                    {
                        var cc = edge.Curve as BRep.Curves.BRepCircleCurve;

                        ne.Curve = new BRep.Curves.BRepCircleCurve()
                        {
                            Dir = cc.Dir,
                            Radius = cc.Radius,
                            Location = cc.Location + shift,
                            SweepAngle = cc.SweepAngle,
                            Axis = top
                        };
                    }
                }
                topFace.Wires.Add(new BRep.BRepWire() { Edges = edges });
            }
            //side faces 

            List<BRep.BRepWire> outter = new List<BRep.BRepWire>();
            foreach (var item in bottomFace.Wires)
            {
                var pp = item.Edges.SelectMany(z => new[] { z.End }).ToArray();
                bool good = true;
                foreach (var w in bottomFace.Wires)
                {
                    if (item == w) continue;
                    var pp2 = w.Edges.SelectMany(z => new[] { z.End }).ToArray();
                    if (pp.Any(u => GeometryUtils.pnpoly(pp2.Select(z => new Vector2d(z.X, z.Y)).ToArray(), u.X, u.Y)))
                    {
                        good = false;
                        break;
                    }
                }
                if (good)
                {
                    outter.Add(item);
                }
            }

            foreach (var wire in bottomFace.Wires)
                foreach (var item in wire.Edges)
                {
                    if ((item.Curve is BRep.Curves.BRepLineCurve))
                    {
                    List<BRep.BRepEdge> edges2 = new List<BRep.BRepEdge>();

                    var dir = (item.End - item.Start).Normalized();
                    dir = new Vector3d(-dir.Y, dir.X, 0);
                    var sideFace = new BRepPlaneFace(_part)
                    {
                        Surface = new BRepPlane()
                        {
                            Normal = -dir,
                            Location = item.Start
                        }
                    };
                    edges2.Add(new BRep.BRepEdge()
                    {
                        Curve = new BRep.Curves.BRepLineCurve()
                        {
                            Point = item.Start,
                            Vector = (item.End - item.Start).Normalized()
                        },
                        Start = item.Start,
                        End = item.End
                    });

                    edges2.Add(new BRep.BRepEdge()
                    {
                        Curve = new BRep.Curves.BRepLineCurve()
                        {
                            Point = item.Start,
                            Vector = (item.End - item.Start).Normalized()
                        },
                        Start = item.End,
                        End = item.End + shift
                    });
                    edges2.Add(new BRep.BRepEdge()
                    {
                        Curve = new BRep.Curves.BRepLineCurve()
                        {
                            Point = item.Start,
                            Vector = (item.End - item.Start).Normalized()
                        },
                        Start = item.End + shift,
                        End = item.Start + shift
                    });
                    edges2.Add(new BRep.BRepEdge()
                    {
                        Curve = new BRep.Curves.BRepLineCurve()
                        {
                            Point = item.Start,
                            Vector = (item.End - item.Start).Normalized()
                        },
                        Start = item.Start + shift,
                        End = item.Start
                    });
                    sideFace.Wires.Add(new BRep.BRepWire() { Edges = edges2 });
                    _part.Faces.Add(sideFace);
                }
                    else
                    if (false && (item.Curve is BRep.Curves.BRepCircleCurve cc))
                    {
                        //4 edges required. 2 circle+2 seam
                        List<BRep.BRepEdge> edges2 = new List<BRep.BRepEdge>();
                        var sideFace = new BRepCylinderSurfaceFace(_part)
                        {
                            Surface = new BRepCylinder()
                            {
                                RefDir = cc.Dir,
                                Radius = cc.Radius,
                                Axis = cc.Axis,
                                Location = cc.Location
                            }
                        };


                        edges2.Add(new BRep.BRepEdge()
                        {
                            Curve = new BRep.Curves.BRepCircleCurve()
                            {
                                Dir = cc.Dir,
                                SweepAngle = cc.SweepAngle,
                                Axis = cc.Axis,
                                Location = cc.Location + shift,
                                Radius = cc.Radius
                            },
                            Start = item.Start + shift,
                            End = item.Start + shift
                        });

                        edges2.Add(new BRep.BRepEdge()
                        {
                            Curve = new BRep.Curves.BRepLineCurve()
                            {
                                Point = item.Start + shift,
                                Vector = cc.Axis
                            },
                            Start = item.Start + shift,
                            End = item.Start
                        });

                        edges2.Add(new BRep.BRepEdge()
                        {
                            Curve = new BRep.Curves.BRepCircleCurve()
                            {
                                Dir = cc.Dir,
                                SweepAngle = cc.SweepAngle,
                                Axis = cc.Axis,
                                Location = cc.Location,
                                Radius = cc.Radius
                            },
                            Start = item.Start,
                            End = item.Start
                        });

                        edges2.Add(new BRep.BRepEdge()
                        {
                            Curve = new BRep.Curves.BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = cc.Axis
                            },
                            Start = item.Start,
                            End = item.Start + shift
                        });

                        sideFace.Wires.Add(new BRep.BRepWire() { Edges = edges2 });
                        _part.Faces.Add(sideFace);
                    }
                }
            
            _part.Faces.Add(topFace);
            _part.ExtractMesh();
        }

        decimal _height = 10;
        bool ctor = false;
        public decimal Height
        {
            get => _height;
            set
            {
                _height = value;
                if (!ctor)
                    CreatePart();
            }
        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }
        public string Name { get; set; } = "extrude";

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }

        public decimal TotalCutLength => Source.CalcTotalCutLength();

        public decimal Volume
        {
            get
            {
                var mn = Part.Faces.First().ExtractMesh();
                var area = mn.Triangles.Sum(z => z.Area());
                //var area = Part.Nodes.First().Triangles.Sum(z => z.Area());
                return ((decimal)area * Height)/(1000m*1000m*1000m);
                return Source.CalcArea() * Height;
            }
        }

        Draft Source;
        Part _part;


        public Part Part => _part;

        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }
        public ProduceOperation Operation { get; set; }

        public void Draw()
        {
            if (!Visible) return;
            if (Part == null) return;
            Part.Draw();
        }

        public void RemoveChild(IDrawable dd)
        {
            if (Source != dd) return;
            Childs.Remove(dd);
            Source = null;
            _part = null;
        }

        public void Store(TextWriter writer)
        {
            writer.WriteLine($"<extrude id=\"{Id}\" height=\"{Height}\" visible=\"{Visible}\" name=\"{Name}\"><source>");
            Source.Store(writer);
            writer.WriteLine("</source></extrude>");
        }

        public IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            return Childs.SelectMany(z => z.GetAll(p)).Union(new[] { this}).ToArray();
        }
    }
}