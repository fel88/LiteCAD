using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points3D = new List<Vector3d>();

        public Draft()
        {
            Plane = new PlaneHelper() { Normal = Vector3d.UnitZ, Position = Vector3d.Zero };
            _inited = true;
        }

        public Draft(XElement el)
        {
            Restore(el);
            _inited = true;
        }

        public void Restore(XElement el)
        {
            Elements.Clear();
            Plane = new PlaneHelper(el.Element("plane"));
            Name = el.Attribute("name").Value;
            foreach (var item2 in el.Elements())
            {
                if (item2.Name == "point")
                {
                    DraftPoint dl = new DraftPoint(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "line")
                {
                    DraftLine dl = new DraftLine(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "ellipse")
                {
                    DraftEllipse dl = new DraftEllipse(item2, this);
                    AddElement(dl);
                }
            }

            var constr = el.Element("constraints");
            if (constr != null)
            {
                Type[] types = new[] {
                typeof(LinearConstraint),
                typeof(VerticalConstraint),
                typeof(HorizontalConstraint)
            };
                foreach (var item in constr.Elements())
                {
                    var fr = types.FirstOrDefault(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                    if (fr == null) continue;
                    var v = Activator.CreateInstance(fr, new object[] { item, this }) as DraftConstraint;
                    //if (v is IXmlStorable xx)
                    //{
                    //    xx.RestoreXml(item);
                    //}
                    AddConstraint(v);
                }
            }

            EndEdit();
        }

        public DraftElement[][] GetWires()
        {
            var remains = DraftLines.Where(z => !z.Dummy).ToList();
            List<List<DraftLine>> ret1 = new List<List<DraftLine>>();

            while (remains.Any())
            {
                List<DraftLine> added = new List<DraftLine>();
                foreach (var rem in remains)
                {
                    bool good = false;

                    foreach (var item in ret1)
                    {
                        var arr1 = item.SelectMany(z => new[] { z.V0, z.V1 }).ToArray();
                        if (arr1.Contains(rem.V0) || arr1.Contains(rem.V1))
                        {
                            item.Add(rem);
                            good = true;
                            break;
                        }
                    }
                    if (good)
                    {
                        added.Add(rem);
                    }
                }
                if (added.Count == 0)
                {
                    ret1.Add(new List<DraftLine>());
                    ret1.Last().Add(remains[0]);
                    added.Add(remains[0]);
                }
                foreach (var item in added)
                {
                    remains.Remove(item);
                }
            }

            List<List<DraftElement>> ret = new List<List<DraftElement>>();
            var remains1 = DraftEllipses.Where(z => !z.Dummy).ToList();

            ret.AddRange(ret1.Select(z => z.Select(u => u as DraftElement).ToList()));
            foreach (var item in remains1)
            {
                ret.Add(new List<DraftElement>()
                {
                    item
                });
            }

            return ret.Select(z => z.ToArray()).ToArray();
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<draft name=\"{Name}\">");
            Plane.Store(writer);
            foreach (var item in DraftPoints)
            {
                item.Store(writer);
            }
            foreach (var item in Elements.Except(DraftPoints))
            {
                item.Store(writer);
            }
            writer.WriteLine("<constraints>");
            foreach (var item in Constraints)
            {
                item.Store(writer);
            }
            writer.WriteLine("</constraints>");
            writer.WriteLine("<helpers>");
            foreach (var item in Helpers)
            {
                item.Store(writer);
            }
            writer.WriteLine("</helpers>");
            writer.WriteLine("</draft>");
        }

        bool _inited = false;
        public void RecalcConstraints()
        {
            if (!_inited) return;

            var lc = Constraints.Where(z => z.Enabled).ToArray();
            int counter = 0;
            int limit = 100;
            StringWriter sw = new StringWriter();
            Store(sw);
            var elem = XElement.Parse(sw.ToString());

            while (true)
            {
                counter++;
                if (lc.All(z => z.IsSatisfied())) break;
                //preserve Refs?
                //Restore(elem);

                if (counter > limit)
                {
                    DebugHelpers.Error("constraints satisfaction error");
                    break;
                }

                foreach (var item in lc)
                {
                    if (item.IsSatisfied())
                        continue;

                    item.RandomUpdate();
                }
            }
        }

        public List<DraftElement> Elements = new List<DraftElement>();
        public List<IDraftHelper> Helpers = new List<IDraftHelper>();
        public List<DraftConstraint> Constraints = new List<DraftConstraint>();
        public void AddHelper(IDraftHelper h)
        {
            Helpers.Add(h);
            h.Parent = this;
        }

        public Action<DraftConstraint> ConstraintAdded;
        public void AddConstraint(DraftConstraint h)
        {
            Constraints.Add(h);
            RecalcConstraints();
            ConstraintAdded?.Invoke(h);
        }

        public decimal CalcTotalCutLength()
        {
            decimal ret = 0;
            ret += DraftEllipses.Sum(z => z.CutLength());
            foreach (var item in DraftLines)
            {
                ret += (decimal)((item.V0.Location - item.V1.Location).Length);
            }
            return ret;
        }

        public PlaneHelper Plane;

        public decimal CalcArea()
        {
            //get nest
            //calc area

            return 0;
        }

        public Vector2d[] Points => DraftPoints.Select(z => z.Location).ToArray();
        public DraftPoint[] DraftPoints => Elements.OfType<DraftPoint>().ToArray();
        public DraftLine[] DraftLines => Elements.OfType<DraftLine>().ToArray();
        public DraftEllipse[] DraftEllipses => Elements.OfType<DraftEllipse>().ToArray();
        public void EndEdit()
        {
            //2d->3d
            var basis = Plane.GetBasis();
            Points3D.Clear();
            foreach (var item in DraftLines)
            {
                Points3D.Add(basis[0] * item.V0.X + basis[1] * item.V0.Y + Plane.Position);
                Points3D.Add(basis[0] * item.V1.X + basis[1] * item.V1.Y + Plane.Position);
            }
        }
        public override void Draw()
        {
            GL.Begin(PrimitiveType.Lines);
            foreach (var item in Points3D)
            {
                GL.Vertex3(item);
            }
            GL.End();
        }
        public override void RemoveChild(IDrawable dd)
        {
            if (dd is IDraftHelper dh)
            {
                Helpers.Remove(dh);
                Constraints.Remove(dh.Constraint);
            }


            Childs.Remove(dd);
        }

        public void AddElement(DraftElement h)
        {
            if (Elements.Contains(h))
                return;

            Elements.Add(h);
            h.Parent = this;
        }

        public void RemoveElement(DraftElement de)
        {
            if (de is DraftPoint dp)
            {
                var ww = Elements.OfType<DraftLine>().Where(z => z.V0 == dp || z.V1 == dp).ToArray();
                foreach (var item in ww)
                {
                    Elements.Remove(item);
                }
            }
            Constraints.RemoveAll(z => z.ContainsElement(de));
            Helpers.RemoveAll(z => z.Constraint.ContainsElement(de));
            Elements.Remove(de);
        }

    }
}