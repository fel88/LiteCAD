using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LiteCAD.Common
{
    public abstract class AbstractSceneObject : ISceneObject
    {
        public static IMessageReporter MessageReporter;
        public AbstractSceneObject()
        {
            Id = FactoryHelper.NewId++;
        }
        public bool Frozen { get; set; }

        public AbstractSceneObject(XElement item)
        {
            if (item.Attribute("id") != null)
            {
                Id = int.Parse(item.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
        }

        public string Name { get; set; }

        public abstract void Draw(GpuDrawingContext ctx);

        public virtual void RemoveChild(ISceneObject dd)
        {
            Childs.Remove(dd);
        }

        public virtual void Store(TextWriter writer)
        {

        }

        public virtual ISceneObject[] GetAll(Predicate<ISceneObject> p)
        {
            if (Childs.Count == 0)
                return [this];

            return new[] { this }.Union(Childs.SelectMany(z => z.GetAll(p))).ToArray();
        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }

        public List<ISceneObject> Childs { get; set; } = new List<ISceneObject>();

        public ISceneObject Parent { get; set; }
        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }
        public int Z { get; set; }
    }
}