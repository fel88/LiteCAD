using LiteCAD.BRep;
using LiteCAD.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using BREP.BRep;
using BREP.BRep.Curves;
using BREP.BRep.Faces;
using BREP.BRep.Surfaces;

namespace LiteCAD
{
    public class ExtrudeModifier : IDrawable, IEconomicsDetail, IMesh, IBREPPartContainer, IMeshNodesContainer, ICommandsContainer
    {
        public ICommand[] Commands => new ICommand[] { new ExtractDraftCommand() };
        public class ExtractDraftCommand : ICommand
        {
            public string Name => "extract draft";

            public Action<IDrawable, object> Process => (z, editor) =>
            {
                var d = z as ExtrudeModifier;
                StringWriter sw = new StringWriter();
                d.Source.Store(sw);
                var d1 = new Draft(XElement.Parse(sw.ToString()));
                Form1.Form.Parts.Add(d1);
                Form1.Form.updateList();
            };
        }

        MeshNode[] IMeshNodesContainer.Nodes
        {
            get
            {
                var mtr = Matrix.Calc();
                List<MeshNode> ret = new List<MeshNode>();
                foreach (var item in Part.Nodes)
                {
                    var mn = new MeshNode() { Parent = item.Parent };
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

        BREPPart IBREPPartContainer.Part => _part;

        public ExtrudeModifier(XElement item)
        {
            ctor = true;
            if (item.Attribute("visible") != null)
                Visible = bool.Parse(item.Attribute("visible").Value);

            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            if (item.Element("transform") != null)
                _matrix.RestoreXml(item.Element("transform"));

            if (item.Attribute("color") != null)
            {
                var rgb = item.Attribute("color").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                Color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }

            Source = new Draft(item.Element("source").Element("draft"));
            Height = Helpers.ParseDecimal(item.Attribute("height").Value);
            Source.Parent = this;
            CreatePart();
            Childs.Add(Source);
            if (item.Attribute("id") != null)
            {
                Id = int.Parse(item.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
            ctor = false;

        }

        public void CreatePart()
        {
            //draft->brep
            _part = new BREPPart(new Part());
            var bottomFace = new BRepPlaneFace(_part.Part)
            {                
                Surface = new BRepPlane() { Normal = Source.Plane.Normal, Location = Source.Plane.Position }
            };
            _part.Faces.Add(bottomFace);

            var basis = Source.Plane.GetBasis();
            var top = Vector3d.Cross(basis[0], basis[1]);

            //geta all wires
            var wires = Source.GetWires();
            foreach (var wire in wires)
            {
                List<BRepEdge> edges = new List<BRepEdge>();

                if (wire.Any(z => z is DraftLine))
                {
                    foreach (var item in wire.OfType<DraftLine>())
                    {
                        var diff = item.V0.Location - item.V1.Location;
                        var p = new OpenTK.Vector3d(item.V0.Location.X, item.V0.Location.Y, 0);
                        var pe = new OpenTK.Vector3d(item.V1.Location.X, item.V1.Location.Y, 0);
                        edges.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
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
                        if (item.SpecificAngles)
                        {
                            var pp = item.GetPoints();
                            for (int ii = 1; ii <= pp.Length; ii++)
                            {
                                var p0 = pp[ii - 1];
                                var p1 = pp[ii % pp.Length];
                                var diff = p0 - p1;
                                var p = new Vector3d(p0.X, p0.Y, 0);
                                var pe = new Vector3d(p1.X, p1.Y, 0);
                                edges.Add(new BRepEdge()
                                {
                                    Curve = new BRepLineCurve()
                                    {
                                        Point = p,
                                        Vector = new Vector3d(diff.X, diff.Y, 0)
                                    },
                                    Start = p,
                                    End = pe
                                });
                            }
                        }
                        else
                        {
                            var p = new OpenTK.Vector3d(item.Center.X + (double)item.Radius, item.Center.Y, 0);
                            var dir2 = (p.Xy - item.Center.Location);
                            var dir3 = new Vector3d(dir2.X, dir2.Y, 0);

                            edges.Add(new BRepEdge()
                            {
                                Curve = new BRepCircleCurve()
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
                }
                bottomFace.Wires.Add(new BRepWire() { Edges = edges });
            }

            var shift = Source.Plane.Normal * (double)Height;
            var topFace = new BRepPlaneFace(_part.Part)
            {
                Parent = _part.Part,
                Surface = new BRepPlane() { Normal = -Source.Plane.Normal, Location = Source.Plane.Position + Source.Plane.Normal * (double)Height }
            };

            foreach (var item in bottomFace.Wires)
            {
                List<BRepEdge> edges = new List<BRepEdge>();
                foreach (var edge in item.Edges)
                {
                    var ne = new BRepEdge();
                    edges.Add(ne);
                    ne.Start = edge.Start + shift;
                    ne.End = edge.End + shift;
                    if (edge.Curve is BRepLineCurve)
                    {
                        var cc = edge.Curve as BRepLineCurve;
                        ne.Curve = new BRepLineCurve() { Point = cc.Point + shift, Vector = cc.Vector };
                    }
                    else if (edge.Curve is BRepCircleCurve)
                    {
                        var cc = edge.Curve as BRepCircleCurve;

                        ne.Curve = new BRepCircleCurve()
                        {
                            Dir = cc.Dir,
                            Radius = cc.Radius,
                            Location = cc.Location + shift,
                            SweepAngle = cc.SweepAngle,
                            Axis = top
                        };
                    }
                }
                topFace.Wires.Add(new BRepWire() { Edges = edges });
            }
            //side faces 

            List<BRepWire> outter = new List<BRepWire>();
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
                    if ((item.Curve is BRepLineCurve))
                    {
                        List<BRepEdge> edges2 = new List<BRepEdge>();

                        var dir = (item.End - item.Start).Normalized();
                        dir = new Vector3d(-dir.Y, dir.X, 0);
                        var sideFace = new BRepPlaneFace(_part.Part)
                        {
                            Parent = _part.Part,
                            Surface = new BRepPlane()
                            {
                                Normal = -dir,
                                Location = item.Start
                            }
                        };
                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = (item.End - item.Start).Normalized()
                            },
                            Start = item.Start,
                            End = item.End
                        });

                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = (item.End - item.Start).Normalized()
                            },
                            Start = item.End,
                            End = item.End + shift
                        });
                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = (item.End - item.Start).Normalized()
                            },
                            Start = item.End + shift,
                            End = item.Start + shift
                        });
                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = (item.End - item.Start).Normalized()
                            },
                            Start = item.Start + shift,
                            End = item.Start
                        });
                        sideFace.Wires.Add(new BRepWire() { Edges = edges2 });
                        _part.Faces.Add(sideFace);
                    }
                    else
                    if ((item.Curve is BRepCircleCurve cc))
                    {
                        //4 edges required. 2 circle+2 seam
                        List<BRepEdge> edges2 = new List<BRepEdge>();
                        var sideFace = new BRepCylinderSurfaceFace(_part.Part)
                        {
                            Surface = new BRepCylinder()
                            {
                                RefDir = cc.Dir.Normalized(),
                                Radius = cc.Radius,
                                Axis = cc.Axis,
                                Location = cc.Location
                            }
                        };


                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepCircleCurve()
                            {
                                Dir = cc.Dir,
                                SweepAngle = cc.SweepAngle,
                                Axis = cc.Axis,
                                Location = cc.Location + shift,
                                Radius = cc.Radius
                            },
                            Start = item.Start + shift,
                            End = item.End + shift
                        });

                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start + shift,
                                Vector = cc.Axis
                            },
                            Start = item.Start + shift,
                            End = item.End
                        });

                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepCircleCurve()
                            {
                                Dir = cc.Dir,
                                SweepAngle = cc.SweepAngle,
                                Axis = cc.Axis,
                                Location = cc.Location,
                                Radius = cc.Radius
                            },
                            Start = item.Start,
                            End = item.End
                        });

                        edges2.Add(new BRepEdge()
                        {
                            Curve = new BRepLineCurve()
                            {
                                Point = item.Start,
                                Vector = cc.Axis
                            },
                            Start = item.Start,
                            End = item.Start + shift
                        });

                        sideFace.Wires.Add(new BRepWire() { Edges = edges2 });
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
                return ((decimal)area * Height) / (1000m * 1000m * 1000m);
                return Source.CalcArea() * Height;
            }
        }

        Draft Source;
        BREPPart _part;


        public BREPPart Part => _part;

        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }
        public ProduceOperation Operation { get; set; }
        public Color Color { get; set; } = Color.LightGray;
        public int Z { get; set; }
        public bool Frozen { get; set; }

        public void Draw()
        {
            if (!Visible) return;
            if (Part == null) return;
            GL.Color3(Color);
            GL.Enable(EnableCap.ColorMaterial);
            GL.PushMatrix();
            Matrix4d dd = _matrix.Calc();
            GL.MultMatrix(ref dd);
            Part.Draw();

            GL.PopMatrix();
            GL.Disable(EnableCap.ColorMaterial);
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
            writer.WriteLine($"<extrude id=\"{Id}\" height=\"{Height}\" visible=\"{Visible}\" name=\"{Name}\" color=\"{Color.R};{Color.G};{Color.B}\">");
            writer.WriteLine("<transform>");
            _matrix.StoreXml(writer);
            writer.WriteLine("</transform>");
            writer.WriteLine("<source>");
            Source.Store(writer);
            writer.WriteLine("</source></extrude>");
        }

        public IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            return Childs.SelectMany(z => z.GetAll(p)).Union(new[] { this }).ToArray();
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            var nodes = ((IMeshNodesContainer)this).Nodes;
            List<Vector3d> ret = new List<Vector3d>();
            foreach (var item in nodes)
            {
                foreach (var item2 in item.Triangles)
                {
                    ret.AddRange(item2.Vertices.Select(zz => zz.Position));
                }
            }
            return ret.ToArray();
        }
    }
}