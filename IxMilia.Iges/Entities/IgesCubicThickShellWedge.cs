namespace IxMilia.Iges.Entities
{
    public class IgesCubicThickShellWedge : IgesFiniteElement
    {
        public override IgesElementEdgeOrder EdgeOrder { get { return IgesElementEdgeOrder.Cubic; } }

        public IgesPoint P1 { get; set; }
        public IgesPoint P1P2Control1 { get; set; }
        public IgesPoint P1P2Control2 { get; set; }
        public IgesPoint P2 { get; set; }
        public IgesPoint P2P3Control1 { get; set; }
        public IgesPoint P2P3Control2 { get; set; }
        public IgesPoint P3 { get; set; }
        public IgesPoint P3P1Control1 { get; set; }
        public IgesPoint P3P1Control2 { get; set; }
        public IgesPoint P4 { get; set; }
        public IgesPoint P4P5Control1 { get; set; }
        public IgesPoint P4P5Control2 { get; set; }
        public IgesPoint P5 { get; set; }
        public IgesPoint P5P6Control1 { get; set; }
        public IgesPoint P5P6Control2 { get; set; }
        public IgesPoint P6 { get; set; }
        public IgesPoint P6P4Control1 { get; set; }
        public IgesPoint P6P4Control2 { get; set; }

        public IgesCubicThickShellWedge(
            IgesPoint p1,
            IgesPoint p1P2Control1,
            IgesPoint p1P2Control2,
            IgesPoint p2,
            IgesPoint p2P3Control1,
            IgesPoint p2P3Control2,
            IgesPoint p3,
            IgesPoint p3P1Control1,
            IgesPoint p3P1Control2,
            IgesPoint p4,
            IgesPoint p4P5Control1,
            IgesPoint p4P5Control2,
            IgesPoint p5,
            IgesPoint p5P6Control1,
            IgesPoint p5P6Control2,
            IgesPoint p6,
            IgesPoint p6P4Control1,
            IgesPoint p6P4Control2)
            : base(IgesTopologyType.CubicThickShellWedge)
        {
            P1 = p1;
            P1P2Control1 = p1P2Control1;
            P1P2Control2 = p1P2Control2;
            P2 = p2;
            P2P3Control1 = p2P3Control1;
            P2P3Control2 = p2P3Control2;
            P3 = p3;
            P3P1Control1 = p3P1Control1;
            P3P1Control2 = p3P1Control2;
            P4 = p4;
            P4P5Control1 = p4P5Control1;
            P4P5Control2 = p4P5Control2;
            P5 = p5;
            P5P6Control1 = p5P6Control1;
            P5P6Control2 = p5P6Control2;
            P6 = p6;
            P6P4Control1 = p6P4Control1;
            P6P4Control2 = p6P4Control2;
        }

        protected override void AddNodes()
        {
            InternalNodes.Add(new IgesNode(P1));
            InternalNodes.Add(new IgesNode(P1P2Control1));
            InternalNodes.Add(new IgesNode(P1P2Control2));
            InternalNodes.Add(new IgesNode(P2));
            InternalNodes.Add(new IgesNode(P2P3Control1));
            InternalNodes.Add(new IgesNode(P2P3Control2));
            InternalNodes.Add(new IgesNode(P3));
            InternalNodes.Add(new IgesNode(P3P1Control1));
            InternalNodes.Add(new IgesNode(P3P1Control2));
            InternalNodes.Add(new IgesNode(P4));
            InternalNodes.Add(new IgesNode(P4P5Control1));
            InternalNodes.Add(new IgesNode(P4P5Control2));
            InternalNodes.Add(new IgesNode(P5));
            InternalNodes.Add(new IgesNode(P5P6Control1));
            InternalNodes.Add(new IgesNode(P5P6Control2));
            InternalNodes.Add(new IgesNode(P6));
            InternalNodes.Add(new IgesNode(P6P4Control1));
            InternalNodes.Add(new IgesNode(P6P4Control2));
        }

        internal static IgesCubicThickShellWedge FromDummy(IgesFiniteElementDummy dummy)
        {
            return new IgesCubicThickShellWedge(
                GetNodeOffset(dummy, 0),
                GetNodeOffset(dummy, 1),
                GetNodeOffset(dummy, 2),
                GetNodeOffset(dummy, 3),
                GetNodeOffset(dummy, 4),
                GetNodeOffset(dummy, 5),
                GetNodeOffset(dummy, 6),
                GetNodeOffset(dummy, 7),
                GetNodeOffset(dummy, 8),
                GetNodeOffset(dummy, 9),
                GetNodeOffset(dummy, 10),
                GetNodeOffset(dummy, 11),
                GetNodeOffset(dummy, 12),
                GetNodeOffset(dummy, 13),
                GetNodeOffset(dummy, 14),
                GetNodeOffset(dummy, 15),
                GetNodeOffset(dummy, 16),
                GetNodeOffset(dummy, 17));
        }
    }
}
