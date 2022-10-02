using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesOffsetSurface : IgesEntity, IIgesSurface
    {
        public override IgesEntityType EntityType { get { return IgesEntityType.OffsetSurface; } }

        public IgesVector Direction { get; set; }
        public double Distance { get; set; }
        public IgesEntity Surface { get; set; }

        public IgesOffsetSurface()
            : this(IgesVector.ZAxis, 0.0, null)
        {
        }

        public IgesOffsetSurface(IgesVector direction, double distance, IgesEntity surface)
            : base()
        {
            Direction = direction;
            Distance = distance;
            Surface = surface;
        }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            Direction = Vector(parameters, ref index);
            Distance = Double(parameters, index++);
            binder.BindEntity(Integer(parameters, index++), e => Surface = e);
            return index;
        }

        internal override IEnumerable<IgesEntity> GetReferencedEntities()
        {
            yield return Surface;
        }

        internal override void WriteParameters(List<object> parameters, IgesWriterBinder binder)
        {
            parameters.Add(Direction.X);
            parameters.Add(Direction.Y);
            parameters.Add(Direction.Z);
            parameters.Add(Distance);
            parameters.Add(binder.GetEntityId(Surface));
        }
    }
}
