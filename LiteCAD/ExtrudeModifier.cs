using LiteCAD.BRep.Faces;
using LiteCAD.BRep.Surfaces;
using LiteCAD.Common;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD
{
    public class ExtrudeModifier : IDrawable, IEconomicsDetail, IPartContainer
    {
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
            Source = new Draft(item.Element("source").Element("draft"));
            Height = Helpers.ParseDecimal(item.Attribute("height").Value);
            Source.Parent = this;
            CreatePart();
            Childs.Add(Source);
            Id = FactoryHelper.NewId++;
        }

        private void CreatePart()
        {
            //draft->brep
            _part = new Part();
            var bottomFace = new BRepPlaneFace(_part) { Surface = new BRepPlane() { Normal = Source.Plane.Normal, Location = Source.Plane.Position } };

            //geta all wires
            var wires = Source.GetWires();
            foreach (var wire in wires)
            {
                List<BRep.BRepEdge> edges = new List<BRep.BRepEdge>();

                foreach (var item in wire)
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
                    var cc = edge.Curve as BRep.Curves.BRepLineCurve;
                    ne.Curve = new BRep.Curves.BRepLineCurve() { Point = cc.Point + shift, Vector = cc.Vector };
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

            _part.Faces.Add(bottomFace);
            _part.Faces.Add(topFace);
            _part.ExtractMesh();
        }

        decimal _height = 10;
        public decimal Height
        {
            get => _height;
            set
            {
                _height = value;
                CreatePart();
            }
        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }
        public string Name { get; set; } = "extrude";

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }

        public decimal TotalCutLength => Source.CalcTotalCutLength();

        public decimal Volume => Source.CalcArea() * Height;

        Draft Source;
        Part _part;


        public Part Part => _part;

        public int Id { get; set; }

        public void Draw()
        {
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
            writer.WriteLine($"<extrude id=\"{Id}\" height=\"{Height}\"><source>");

            Source.Store(writer);
            writer.WriteLine("</source></extrude>");
        }
    }
}