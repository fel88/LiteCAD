using System;
using System.Collections.Generic;

namespace PolyBoolCS
{
    public class PolyBool
    {
        #region Core api

        public SegmentList segments(Polygon poly)
        {
            var i = new Intersecter(true);

            foreach (var region in poly.regions)
            {
                i.addRegion(region);
            }

            var result = i.calculate(poly.inverted);
            result.inverted = poly.inverted;

            return result;
        }

        public CombinedSegmentLists combine(SegmentList segments1, SegmentList segments2)
        {
            var i = new Intersecter(false);

            return new CombinedSegmentLists()
            {
                combined = i.calculate(
                    segments1, segments1.inverted,
                    segments2, segments2.inverted
                ),
                inverted1 = segments1.inverted,
                inverted2 = segments2.inverted
            };
        }

        public SegmentList selectUnion(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.union(combined.combined);
            result.inverted = combined.inverted1 || combined.inverted2;

            return result;
        }

        public SegmentList selectIntersect(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.intersect(combined.combined);
            result.inverted = combined.inverted1 && combined.inverted2;

            return result;
        }

        public SegmentList selectDifference(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.difference(combined.combined);
            result.inverted = combined.inverted1 && !combined.inverted2;

            return result;
        }

        public SegmentList selectDifferenceRev(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.differenceRev(combined.combined);
            result.inverted = !combined.inverted1 && combined.inverted2;

            return result;
        }

        public SegmentList selectXor(CombinedSegmentLists combined)
        {
            var result = SegmentSelector.xor(combined.combined);
            result.inverted = combined.inverted1 != combined.inverted2;

            return result;
        }

        public Polygon polygon(SegmentList segments)
        {
            var chain = new SegmentChainer().chain(segments);

            return new Polygon()
            {
                regions = chain,
                inverted = segments.inverted
            };
        }

        #endregion

        #region Helper functions for common operations

        public Polygon union(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectUnion);
        }

        public Polygon intersect(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectIntersect);
        }

        public Polygon difference(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectDifference);
        }

        public Polygon differenceRev(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectDifferenceRev);
        }

        public Polygon xor(Polygon poly1, Polygon poly2)
        {
            return operate(poly1, poly2, selectXor);
        }

        #endregion

        #region Private utility functions

        private Polygon operate(Polygon poly1, Polygon poly2, Func<CombinedSegmentLists, SegmentList> selector)
        {
            var seg1 = segments(poly1);
            var seg2 = segments(poly2);
            var comb = combine(seg1, seg2);

            var seg3 = selector(comb);

            return polygon(seg3);
        }

        #endregion
    }

    public class EventNode
    {
        public bool isStart;
        public Point pt;
        public Segment seg;
        public bool primary;
        public EventNode other;
        public StatusNode status;

        #region Linked List Node

        public EventNode next;
        public EventNode prev;

        public void remove()
        {
            prev.next = next;

            if (next != null)
            {
                next.prev = prev;
            }

            prev = null;
            next = null;
        }

        #endregion

    }

    public class StatusNode
    {
        public EventNode ev;

        #region Linked List Node 

        public StatusNode next;
        public StatusNode prev;

        public void remove()
        {
            prev.next = next;

            if (next != null)
            {
                next.prev = prev;
            }

            prev = null;
            next = null;
        }

        #endregion
    }

    public class StatusLinkedList
    {
        #region Fields

        private StatusNode root = new StatusNode();

        #endregion

        #region Public properties

        public bool isEmpty { get { return root.next == null; } }

        public StatusNode head { get { return root.next; } }

        #endregion

        #region Public functions

        public bool exists(StatusNode node)
        {
            if (node == null || node == root)
                return false;

            return true;
        }

        public Transition findTransition(EventNode ev)
        {
            var prev = root;
            var here = root.next;

            while (here != null)
            {
                if (findTransitionPredicate(ev, here))
                    break;

                prev = here;
                here = here.next;
            }

            return new Transition()
            {
                before = prev == root ? null : prev.ev,
                after = here != null ? here.ev : null,
                here = here,
                prev = prev
            };
        }

        public StatusNode insert(Transition surrounding, EventNode ev)
        {
            var prev = surrounding.prev;
            var here = surrounding.here;

            var node = new StatusNode() { ev = ev };

            node.prev = prev;
            node.next = here;
            prev.next = node;

            if (here != null)
            {
                here.prev = node;
            }

            return node;
        }

        #endregion

        #region Private utilty functions 

        private bool findTransitionPredicate(EventNode ev, StatusNode here)
        {
            var comp = statusCompare(ev, here.ev);
            return comp > 0;
        }

        private int statusCompare(EventNode ev1, EventNode ev2)
        {
            var a1 = ev1.seg.start;
            var a2 = ev1.seg.end;
            var b1 = ev2.seg.start;
            var b2 = ev2.seg.end;

            if (Epsilon.pointsCollinear(a1, b1, b2))
            {
                if (Epsilon.pointsCollinear(a2, b1, b2))
                    return 1;//eventCompare(true, a1, a2, true, b1, b2);

                return Epsilon.pointAboveOrOnLine(a2, b1, b2) ? 1 : -1;
            }

            return Epsilon.pointAboveOrOnLine(a1, b1, b2) ? 1 : -1;
        }

        #endregion
    }

    public class EventLinkedList
    {
        #region Fields

        private EventNode root = new EventNode();

        #endregion

        #region Public properties

        public bool isEmpty { get { return root.next == null; } }

        public EventNode head { get { return root.next; } }

        #endregion

        #region Public functions

        public void insertBefore(EventNode node, Point other_pt)
        {
            var last = root;
            var here = root.next;

            while (here != null)
            {
                if (insertBeforePredicate(here, node, ref other_pt))
                {
                    node.prev = here.prev;
                    node.next = here;
                    here.prev.next = node;
                    here.prev = node;

                    return;
                }

                last = here;
                here = here.next;
            }

            last.next = node;
            node.prev = last;
            node.next = null;
        }

        #endregion

        #region Private utility functions 

        private bool insertBeforePredicate(EventNode here, EventNode ev, ref Point other_pt)
        {
            // should ev be inserted before here?
            var comp = eventCompare(
                ev.isStart,
                ref ev.pt,
                ref other_pt,
                here.isStart,
                ref here.pt,
                ref here.other.pt
            );

            return comp < 0;
        }

        private int eventCompare(bool p1_isStart, ref Point p1_1, ref Point p1_2, bool p2_isStart, ref Point p2_1, ref Point p2_2)
        {
            // compare the selected points first
            var comp = Epsilon.pointsCompare(p1_1, p2_1);
            if (comp != 0)
                return comp;

            // the selected points are the same

            if (Epsilon.pointsSame(p1_2, p2_2)) // if the non-selected points are the same too...
                return 0; // then the segments are equal

            if (p1_isStart != p2_isStart) // if one is a start and the other isn't...
                return p1_isStart ? 1 : -1; // favor the one that isn't the start

            // otherwise, we'll have to calculate which one is below the other manually
            return Epsilon.pointAboveOrOnLine(
                p1_2,
                p2_isStart ? p2_1 : p2_2, // order matters
                p2_isStart ? p2_2 : p2_1
            ) ? 1 : -1;
        }

        #endregion
    }


    /// <summary>
    /// this is the core work-horse
    /// </summary>
    public class Intersecter
    {
        #region Private fields

        private bool selfIntersection = false;


        private EventLinkedList event_root = new EventLinkedList();
        private StatusLinkedList status_root;

        #endregion

        #region Constructor

        public Intersecter(bool selfIntersection)
        {
            this.selfIntersection = selfIntersection;

        }

        #endregion

        #region Segment creation

        public Segment segmentNew(Point start, Point end)
        {
            return new Segment()
            {
                id = -1,
                start = start,
                end = end,
                myFill = new SegmentFill(),
                otherFill = null
            };
        }

        public Segment segmentCopy(Point start, Point end, Segment seg)
        {
            return new Segment()
            {
                id = -1,
                start = start,
                end = end,
                myFill = new SegmentFill()
                {
                    above = seg.myFill.above,
                    below = seg.myFill.below
                },
                otherFill = null
            };
        }

        #endregion

        #region Event logic

        public void eventAdd(EventNode ev, Point other_pt)
        {
            event_root.insertBefore(ev, other_pt);
        }

        public EventNode eventAddSegmentStart(Segment seg, bool primary)
        {
            var ev_start = new EventNode()
            {
                isStart = true,
                pt = seg.start,
                seg = seg,
                primary = primary,
                other = null,
                status = null
            };

            eventAdd(ev_start, seg.end);

            return ev_start;
        }

        public EventNode eventAddSegmentEnd(EventNode ev_start, Segment seg, bool primary)
        {
            var ev_end = new EventNode()
            {
                isStart = false,
                pt = seg.end,
                seg = seg,
                primary = primary,
                other = ev_start,
                status = null
            };

            ev_start.other = ev_end;

            eventAdd(ev_end, ev_start.pt);

            return ev_end;
        }

        public EventNode eventAddSegment(Segment seg, bool primary)
        {
            var ev_start = eventAddSegmentStart(seg, primary);
            eventAddSegmentEnd(ev_start, seg, primary);

            return ev_start;
        }

        public void eventUpdateEnd(EventNode ev, Point end)
        {
            // slides an end backwards
            //   (start)------------(end)    to:
            //   (start)---(end)



            ev.other.remove();
            ev.seg.end = end;
            ev.other.pt = end;
            eventAdd(ev.other, ev.pt);
        }

        public EventNode eventDivide(EventNode ev, Point pt)
        {
            var ns = segmentCopy(pt, ev.seg.end, ev.seg);
            eventUpdateEnd(ev, pt);

            return eventAddSegment(ns, ev.primary);
        }

        #endregion

        #region Calculation

        public SegmentList calculate(bool inverted = false)
        {
            if (!selfIntersection)
            {
                throw new Exception("This function is only intended to be called when selfIntersection = true");
            }

            return calculate_INTERNAL(inverted, false);
        }

        public SegmentList calculate(SegmentList segments1, bool inverted1, SegmentList segments2, bool inverted2)
        {
            if (selfIntersection)
            {
                throw new Exception("This function is only intended to be called when selfIntersection = false");
            }

            // segmentsX come from the self-intersection API, or this API
            // invertedX is whether we treat that list of segments as an inverted polygon or not
            // returns segments that can be used for further operations
            for (int i = 0; i < segments1.Count; i++)
            {
                eventAddSegment(segments1[i], true);
            }
            for (int i = 0; i < segments2.Count; i++)
            {
                eventAddSegment(segments2[i], false);
            }

            return calculate_INTERNAL(inverted1, inverted2);
        }

        public void addRegion(PointList region)
        {
            if (!selfIntersection)
            {
                throw new Exception("The addRegion() function is only intended for use when selfIntersection = false");
            }

            // Ensure that the polygon is fully closed (the start point and end point are exactly the same)
            if (!Epsilon.pointsSame(region[region.Count - 1], region[0]))
            {
                region.Add(region[0]);
            }

            // regions are a list of points:
            //  [ [0, 0], [100, 0], [50, 100] ]
            // you can add multiple regions before running calculate
            var pt1 = new Point();
            var pt2 = region[region.Count - 1];

            for (var i = 0; i < region.Count; i++)
            {
                pt1 = pt2;
                pt2 = region[i];

                var forward = Epsilon.pointsCompare(pt1, pt2);
                if (forward == 0) // points are equal, so we have a zero-length segment
                    continue; // just skip it

                eventAddSegment(
                    segmentNew(
                        forward < 0 ? pt1 : pt2,
                        forward < 0 ? pt2 : pt1
                    ),
                    true
                );
            }
        }

        private Transition statusFindSurrounding(EventNode ev)
        {
            return status_root.findTransition(ev);
        }

        private EventNode checkIntersection(EventNode ev1, EventNode ev2)
        {
            // returns the segment equal to ev1, or false if nothing equal

            var seg1 = ev1.seg;
            var seg2 = ev2.seg;
            var a1 = seg1.start;
            var a2 = seg1.end;
            var b1 = seg2.start;
            var b2 = seg2.end;



            Intersection intersect;
            if (!Epsilon.linesIntersect(a1, a2, b1, b2, out intersect))
            {
                // segments are parallel or coincident

                // if points aren't collinear, then the segments are parallel, so no intersections
                if (!Epsilon.pointsCollinear(a1, a2, b1))
                    return null;

                // otherwise, segments are on top of each other somehow (aka coincident)

                if (Epsilon.pointsSame(a1, b2) || Epsilon.pointsSame(a2, b1))
                    return null; // segments touch at endpoints... no intersection

                var a1_equ_b1 = Epsilon.pointsSame(a1, b1);
                var a2_equ_b2 = Epsilon.pointsSame(a2, b2);

                if (a1_equ_b1 && a2_equ_b2)
                    return ev2; // segments are exactly equal

                var a1_between = !a1_equ_b1 && Epsilon.pointBetween(a1, b1, b2);
                var a2_between = !a2_equ_b2 && Epsilon.pointBetween(a2, b1, b2);

                // handy for debugging:
                // buildLog.log({
                //	a1_equ_b1: a1_equ_b1,
                //	a2_equ_b2: a2_equ_b2,
                //	a1_between: a1_between,
                //	a2_between: a2_between
                // });

                if (a1_equ_b1)
                {
                    if (a2_between)
                    {
                        //  (a1)---(a2)
                        //  (b1)----------(b2)
                        eventDivide(ev2, a2);
                    }
                    else
                    {
                        //  (a1)----------(a2)
                        //  (b1)---(b2)
                        eventDivide(ev1, b2);
                    }

                    return ev2;
                }
                else if (a1_between)
                {
                    if (!a2_equ_b2)
                    {
                        // make a2 equal to b2
                        if (a2_between)
                        {
                            //         (a1)---(a2)
                            //  (b1)-----------------(b2)
                            eventDivide(ev2, a2);
                        }
                        else
                        {
                            //         (a1)----------(a2)
                            //  (b1)----------(b2)
                            eventDivide(ev1, b2);
                        }
                    }

                    //         (a1)---(a2)
                    //  (b1)----------(b2)
                    eventDivide(ev2, a1);
                }
            }
            else
            {
                // otherwise, lines intersect at i.pt, which may or may not be between the endpoints

                // is A divided between its endpoints? (exclusive)
                if (intersect.alongA == 0)
                {
                    if (intersect.alongB == -1) // yes, at exactly b1
                        eventDivide(ev1, b1);
                    else if (intersect.alongB == 0) // yes, somewhere between B's endpoints
                        eventDivide(ev1, intersect.pt);
                    else if (intersect.alongB == 1) // yes, at exactly b2
                        eventDivide(ev1, b2);
                }

                // is B divided between its endpoints? (exclusive)
                if (intersect.alongB == 0)
                {
                    if (intersect.alongA == -1) // yes, at exactly a1
                        eventDivide(ev2, a1);
                    else if (intersect.alongA == 0) // yes, somewhere between A's endpoints (exclusive)
                        eventDivide(ev2, intersect.pt);
                    else if (intersect.alongA == 1) // yes, at exactly a2
                        eventDivide(ev2, a2);
                }
            }

            return null;
        }

        private EventNode checkBothIntersections(EventNode ev, EventNode above, EventNode below)
        {
            if (above != null)
            {
                var eve = checkIntersection(ev, above);
                if (eve != null)
                    return eve;
            }

            if (below != null)
            {
                return checkIntersection(ev, below);
            }

            return null;
        }

        private SegmentList calculate_INTERNAL(bool primaryPolyInverted, bool secondaryPolyInverted)
        {
            //
            // main event loop
            //
            var segments = new SegmentList();
            status_root = new StatusLinkedList();

            while (!event_root.isEmpty)
            {
                var ev = (EventNode)event_root.head;


                if (ev.isStart)
                {

                    var surrounding = statusFindSurrounding(ev);
                    var above = surrounding.before != null ? surrounding.before : null;
                    var below = surrounding.after != null ? surrounding.after : null;



                    var eve = checkBothIntersections(ev, above, below);
                    if (eve != null)
                    {
                        // ev and eve are equal
                        // we'll keep eve and throw away ev

                        // merge ev.seg's fill information into eve.seg

                        if (selfIntersection)
                        {
                            var toggle = false; // are we a toggling edge?
                            if (ev.seg.myFill.below == null)
                                toggle = true;
                            else
                                toggle = ev.seg.myFill.above != ev.seg.myFill.below;

                            // merge two segments that belong to the same polygon
                            // think of this as sandwiching two segments together, where `eve.seg` is
                            // the bottom -- this will cause the above fill flag to toggle
                            if (toggle)
                            {
                                eve.seg.myFill.above = !eve.seg.myFill.above;
                            }
                        }
                        else
                        {
                            // merge two segments that belong to different polygons
                            // each segment has distinct knowledge, so no special logic is needed
                            // note that this can only happen once per segment in this phase, because we
                            // are guaranteed that all self-intersections are gone
                            eve.seg.otherFill = ev.seg.myFill;
                        }


                        ev.other.remove();
                        ev.remove();
                    }

                    if (event_root.head != ev)
                    {
                        // something was inserted before us in the event queue, so loop back around and
                        // process it before continuing


                        continue;
                    }

                    //
                    // calculate fill flags
                    //
                    if (selfIntersection)
                    {
                        var toggle = false; // are we a toggling edge?

                        // if we are a new segment...
                        if (ev.seg.myFill.below == null)
                            // then we toggle
                            toggle = true;
                        else
                            // we are a segment that has previous knowledge from a division
                            toggle = ev.seg.myFill.above != ev.seg.myFill.below; // calculate toggle

                        // next, calculate whether we are filled below us
                        if (below == null)
                        {
                            // if nothing is below us...
                            // we are filled below us if the polygon is inverted
                            ev.seg.myFill.below = primaryPolyInverted;
                        }
                        else
                        {
                            // otherwise, we know the answer -- it's the same if whatever is below
                            // us is filled above it
                            ev.seg.myFill.below = below.seg.myFill.above;
                        }

                        // since now we know if we're filled below us, we can calculate whether
                        // we're filled above us by applying toggle to whatever is below us
                        if (toggle)
                            ev.seg.myFill.above = !ev.seg.myFill.below.Value;
                        else
                            ev.seg.myFill.above = ev.seg.myFill.below.Value;
                    }
                    else
                    {
                        // now we fill in any missing transition information, since we are all-knowing
                        // at this point

                        if (ev.seg.otherFill == null)
                        {
                            // if we don't have other information, then we need to figure out if we're
                            // inside the other polygon
                            var inside = false;
                            if (below == null)
                            {
                                // if nothing is below us, then we're inside if the other polygon is
                                // inverted
                                inside = ev.primary ? secondaryPolyInverted : primaryPolyInverted;
                            }
                            else
                            {
                                // otherwise, something is below us
                                // so copy the below segment's other polygon's above
                                if (ev.primary == below.primary)
                                    inside = below.seg.otherFill.above;
                                else
                                    inside = below.seg.myFill.above;
                            }

                            ev.seg.otherFill = new SegmentFill()
                            {
                                above = inside,
                                below = inside
                            };
                        }
                    }



                    // insert the status and remember it for later removal
                    ev.other.status = status_root.insert(surrounding, ev);
                }
                else
                {
                    var st = ev.status;

                    if (st == null)
                    {
                        throw new Exception("PolyBool: Zero-length segment detected; your epsilon is probably too small or too large");
                    }

                    // removing the status will create two new adjacent edges, so we'll need to check
                    // for those
                    if (status_root.exists(st.prev) && status_root.exists(st.next))
                        checkIntersection(st.prev.ev, st.next.ev);



                    // remove the status
                    st.remove();

                    // if we've reached this point, we've calculated everything there is to know, so
                    // save the segment for reporting
                    if (!ev.primary)
                    {
                        // make sure `seg.myFill` actually points to the primary polygon though
                        var s = ev.seg.myFill;
                        ev.seg.myFill = ev.seg.otherFill;
                        ev.seg.otherFill = s;
                    }

                    segments.Add(ev.seg);
                }

                // remove the event and continue
                event_root.head.remove();
            }



            return segments;
        }

        #endregion
    }
    /// <summary>
    /// Provides the raw computation functions that takes epsilon into account.
    /// zero is defined to be between (-epsilon, epsilon) exclusive
    /// </summary>
    public class Epsilon
    {
        #region Static variables

        private static double eps = 1e-10;

        #endregion

        #region Public functions

        public static bool pointAboveOrOnLine(Point pt, Point left, Point right)
        {
            var Ax = left.x;
            var Ay = left.y;
            var Bx = right.x;
            var By = right.y;
            var Cx = pt.x;
            var Cy = pt.y;

            return (Bx - Ax) * (Cy - Ay) - (By - Ay) * (Cx - Ax) >= -eps;
        }

        public static bool pointBetween(Point pt, Point left, Point right)
        {
            // p must be collinear with left->right
            // returns false if p == left, p == right, or left == right
            var d_py_ly = pt.y - left.y;
            var d_rx_lx = right.x - left.x;
            var d_px_lx = pt.x - left.x;
            var d_ry_ly = right.y - left.y;

            var dot = d_px_lx * d_rx_lx + d_py_ly * d_ry_ly;

            // if `dot` is 0, then `p` == `left` or `left` == `right` (reject)
            // if `dot` is less than 0, then `p` is to the left of `left` (reject)
            if (dot < eps)
                return false;

            var sqlen = d_rx_lx * d_rx_lx + d_ry_ly * d_ry_ly;

            // if `dot` > `sqlen`, then `p` is to the right of `right` (reject)
            // therefore, if `dot - sqlen` is greater than 0, then `p` is to the right of `right` (reject)
            if (dot - sqlen > -eps)
                return false;

            return true;
        }

        public static bool pointsSameX(Point p1, Point p2)
        {
            return Math.Abs(p1.x - p2.x) < eps;
        }

        public static bool pointsSameY(Point p1, Point p2)
        {
            return Math.Abs(p1.y - p2.y) < eps;
        }

        public static bool pointsSame(Point p1, Point p2)
        {
            return
                Math.Abs(p1.x - p2.x) < eps &&
                Math.Abs(p1.y - p2.y) < eps;
        }

        public static int pointsCompare(Point p1, Point p2)
        {
            // returns -1 if p1 is smaller, 1 if p2 is smaller, 0 if equal
            if (pointsSameX(p1, p2))
                return pointsSameY(p1, p2) ? 0 : (p1.y < p2.y ? -1 : 1);

            return p1.x < p2.x ? -1 : 1;
        }

        public static bool pointsCollinear(Point p1, Point p2, Point p3)
        {
            // does pt1->pt2->pt3 make a straight line?
            // essentially this is just checking to see if the slope(pt1->pt2) === slope(pt2->pt3)
            // if slopes are equal, then they must be collinear, because they share pt2
            var dx1 = p1.x - p2.x;
            var dy1 = p1.y - p2.y;
            var dx2 = p2.x - p3.x;
            var dy2 = p2.y - p3.y;

            return Math.Abs(dx1 * dy2 - dx2 * dy1) < eps;
        }

        public static bool linesIntersect(Point a0, Point a1, Point b0, Point b1, out Intersection intersection)
        {
            // returns false if the lines are coincident (e.g., parallel or on top of each other)
            //
            // returns an object if the lines intersect:
            //   {
            //     pt: [x, y],    where the intersection point is at
            //     alongA: where intersection point is along A,
            //     alongB: where intersection point is along B
            //   }
            //
            //  alongA and alongB will each be one of: -2, -1, 0, 1, 2
            //
            //  with the following meaning:
            //
            //    -2   intersection point is before segment's first point
            //    -1   intersection point is directly on segment's first point
            //     0   intersection point is between segment's first and second points (exclusive)
            //     1   intersection point is directly on segment's second point
            //     2   intersection point is after segment's second point

            var adx = a1.x - a0.x;
            var ady = a1.y - a0.y;
            var bdx = b1.x - b0.x;
            var bdy = b1.y - b0.y;

            var axb = adx * bdy - ady * bdx;
            if (Math.Abs(axb) < eps)
            {
                intersection = Intersection.Empty;
                return false; // lines are coincident
            }

            var dx = a0.x - b0.x;
            var dy = a0.y - b0.y;

            var A = (bdx * dy - bdy * dx) / axb;
            var B = (adx * dy - ady * dx) / axb;

            intersection = new Intersection()
            {
                alongA = 0,
                alongB = 0,
                pt = new Point()
                {
                    x = a0.x + A * adx,
                    y = a0.y + A * ady
                }
            };

            // categorize where intersection point is along A and B

            if (A <= -eps)
                intersection.alongA = -2;
            else if (A < eps)
                intersection.alongA = -1;
            else if (A - 1 <= -eps)
                intersection.alongA = 0;
            else if (A - 1 < eps)
                intersection.alongA = 1;
            else
                intersection.alongA = 2;

            if (B <= -eps)
                intersection.alongB = -2;
            else if (B < eps)
                intersection.alongB = -1;
            else if (B - 1 <= -eps)
                intersection.alongB = 0;
            else if (B - 1 < eps)
                intersection.alongB = 1;
            else
                intersection.alongB = 2;

            return true;
        }

        #endregion
    }
    /// <summary>
    /// converts a list of segments into a list of regions, while also removing unnecessary vertices
    /// </summary>
    public class SegmentChainer
    {
        #region Fields

        private List<PointList> chains;
        private List<PointList> regions;

        private Match first_match;
        private Match second_match;
        private Match next_match;

        #endregion

        #region Public functions

        public List<PointList> chain(SegmentList segments)
        {

            this.chains = new List<PointList>();
            this.regions = new List<PointList>();

            foreach (var seg in segments)
            {
                var pt1 = seg.start;
                var pt2 = seg.end;

                if (Epsilon.pointsSame(pt1, pt2))
                {
                    Console.WriteLine("PolyBool: Warning: Zero-length segment detected; your epsilon is probably too small or too large");
                    continue;
                }



                first_match = new Match()
                {
                    index = 0,
                    matches_head = false,
                    matches_pt1 = false
                };

                second_match = new Match()
                {
                    index = 0,
                    matches_head = false,
                    matches_pt1 = false
                };

                next_match = first_match;

                for (var i = 0; i < chains.Count; i++)
                {
                    var chain = chains[i];
                    var head = chain[0];
                    var head2 = chain[1];
                    var tail = chain[chain.Count - 1];
                    var tail2 = chain[chain.Count - 2];

                    if (Epsilon.pointsSame(head, pt1))
                    {
                        if (setMatch(i, true, true))
                            break;
                    }
                    else if (Epsilon.pointsSame(head, pt2))
                    {
                        if (setMatch(i, true, false))
                            break;
                    }
                    else if (Epsilon.pointsSame(tail, pt1))
                    {
                        if (setMatch(i, false, true))
                            break;
                    }
                    else if (Epsilon.pointsSame(tail, pt2))
                    {
                        if (setMatch(i, false, false))
                            break;
                    }
                }

                if (next_match == first_match)
                {
                    // we didn't match anything, so create a new chain
                    chains.Add(new PointList() { pt1, pt2 });



                    continue;
                }

                if (next_match == second_match)
                {
                    // we matched a single chain



                    // add the other point to the apporpriate end, and check to see if we've closed the
                    // chain into a loop

                    var index = first_match.index;
                    var pt = first_match.matches_pt1 ? pt2 : pt1; // if we matched pt1, then we add pt2, etc
                    var addToHead = first_match.matches_head; // if we matched at head, then add to the head

                    var chain = chains[index];
                    var grow = addToHead ? chain[0] : chain[chain.Count - 1];
                    var grow2 = addToHead ? chain[1] : chain[chain.Count - 2];
                    var oppo = addToHead ? chain[chain.Count - 1] : chain[0];
                    var oppo2 = addToHead ? chain[chain.Count - 2] : chain[1];

                    if (Epsilon.pointsCollinear(grow2, grow, pt))
                    {
                        // grow isn't needed because it's directly between grow2 and pt:
                        // grow2 ---grow---> pt
                        if (addToHead)
                        {

                            chain.RemoveAt(0);
                        }
                        else
                        {


                            chain.RemoveAt(chain.Count - 1);
                        }
                        grow = grow2; // old grow is gone... new grow is what grow2 was
                    }

                    if (Epsilon.pointsSame(oppo, pt))
                    {
                        // we're closing the loop, so remove chain from chains
                        chains.RemoveAt(index);

                        if (Epsilon.pointsCollinear(oppo2, oppo, grow))
                        {
                            // oppo isn't needed because it's directly between oppo2 and grow:
                            // oppo2 ---oppo--->grow
                            if (addToHead)
                            {


                                chain.RemoveAt(chain.Count - 1);
                            }
                            else
                            {


                                chain.RemoveAt(0);
                            }
                        }



                        // we have a closed chain!
                        regions.Add(chain);
                        continue;
                    }

                    // not closing a loop, so just add it to the apporpriate side
                    if (addToHead)
                    {


                        chain.Insert(0, pt);
                    }
                    else
                    {


                        chain.Add(pt);
                    }

                    continue;
                }

                // otherwise, we matched two chains, so we need to combine those chains together

                var F = first_match.index;
                var S = second_match.index;



                var reverseF = chains[F].Count < chains[S].Count; // reverse the shorter chain, if needed
                if (first_match.matches_head)
                {
                    if (second_match.matches_head)
                    {
                        if (reverseF)
                        {
                            // <<<< F <<<< --- >>>> S >>>>
                            reverseChain(F);
                            // >>>> F >>>> --- >>>> S >>>>
                            appendChain(F, S);
                        }
                        else
                        {
                            // <<<< F <<<< --- >>>> S >>>>
                            reverseChain(S);
                            // <<<< F <<<< --- <<<< S <<<<   logically same as:
                            // >>>> S >>>> --- >>>> F >>>>
                            appendChain(S, F);
                        }
                    }
                    else
                    {
                        // <<<< F <<<< --- <<<< S <<<<   logically same as:
                        // >>>> S >>>> --- >>>> F >>>>
                        appendChain(S, F);
                    }
                }
                else
                {
                    if (second_match.matches_head)
                    {
                        // >>>> F >>>> --- >>>> S >>>>
                        appendChain(F, S);
                    }
                    else
                    {
                        if (reverseF)
                        {
                            // >>>> F >>>> --- <<<< S <<<<
                            reverseChain(F);
                            // <<<< F <<<< --- <<<< S <<<<   logically same as:
                            // >>>> S >>>> --- >>>> F >>>>
                            appendChain(S, F);
                        }
                        else
                        {
                            // >>>> F >>>> --- <<<< S <<<<
                            reverseChain(S);
                            // >>>> F >>>> --- >>>> S >>>>
                            appendChain(F, S);
                        }
                    }
                }
            }

            return regions;
        }

        #endregion

        #region Private utility functions

        private void reverseChain(int index)
        {


            chains[index].Reverse(); // gee, that's easy
        }

        private bool setMatch(int index, bool matches_head, bool matches_pt1)
        {
            // return true if we've matched twice
            next_match.index = index;
            next_match.matches_head = matches_head;
            next_match.matches_pt1 = matches_pt1;

            if (next_match == first_match)
            {
                next_match = second_match;
                return false;
            }

            next_match = null;

            return true; // we've matched twice, we're done here
        }

        private void appendChain(int index1, int index2)
        {
            // index1 gets index2 appended to it, and index2 is removed
            var chain1 = chains[index1];
            var chain2 = chains[index2];
            var tail = chain1[chain1.Count - 1];
            var tail2 = chain1[chain1.Count - 2];
            var head = chain2[0];
            var head2 = chain2[1];

            if (Epsilon.pointsCollinear(tail2, tail, head))
            {
                // tail isn't needed because it's directly between tail2 and head
                // tail2 ---tail---> head


                chain1.RemoveAt(chain1.Count - 1);
                tail = tail2; // old tail is gone... new tail is what tail2 was
            }

            if (Epsilon.pointsCollinear(tail, head, head2))
            {
                // head isn't needed because it's directly between tail and head2
                // tail ---head---> head2


                chain2.RemoveAt(0);
            }


            chain1.AddRange(chain2);
            chains.RemoveAt(index2);
        }

        #endregion

        #region Nested types

        private class Match
        {
            public int index;
            public bool matches_head;
            public bool matches_pt1;
        }

        #endregion
    }
    public class SegmentSelector
    {
        #region Selection tables


        private static readonly int[] union_select_table = {
            0, 2, 1, 0,
            2, 2, 0, 0,
            1, 0, 1, 0,
            0, 0, 0, 0
        };


        private static readonly int[] intersect_select_table = {
            0, 0, 0, 0,
            0, 2, 0, 2,
            0, 0, 1, 1,
            0, 2, 1, 0
        };


        private static readonly int[] difference_select_table = {
            0, 0, 0, 0,
            2, 0, 2, 0,
            1, 1, 0, 0,
            0, 1, 2, 0
        };


        private static readonly int[] differenceRev_select_table = {
            0, 2, 1, 0,
            0, 0, 1, 1,
            0, 2, 0, 2,
            0, 0, 0, 0
        };


        private static readonly int[] xor_select_table = {
            0, 2, 1, 0,
            2, 0, 0, 1,
            1, 0, 0, 2,
            0, 1, 2, 0
        };

        #endregion

        #region Public functions

        public static SegmentList union(SegmentList segments)
        {
            return select(segments, union_select_table);
        }

        public static SegmentList intersect(SegmentList segments)
        {
            return select(segments, intersect_select_table);
        }

        public static SegmentList difference(SegmentList segments)
        {
            return select(segments, difference_select_table);
        }

        public static SegmentList differenceRev(SegmentList segments)
        {
            return select(segments, differenceRev_select_table);
        }

        public static SegmentList xor(SegmentList segments)
        {
            return select(segments, xor_select_table);
        }

        #endregion

        #region Private functions

        private static SegmentList select(SegmentList segments, int[] selection)
        {
            var result = new SegmentList();

            foreach (var seg in segments)
            {
                var index =
                    (seg.myFill.above ? 8 : 0) +
                    (seg.myFill.below.Value ? 4 : 0) +
                    ((seg.otherFill != null && seg.otherFill.above) ? 2 : 0) +
                    ((seg.otherFill != null && seg.otherFill.below.Value) ? 1 : 0);

                if (selection[index] != 0)
                {
                    // copy the segment to the results, while also calculating the fill status
                    result.Add(new Segment()
                    {
                        id = -1,
                        start = seg.start,
                        end = seg.end,
                        myFill = new SegmentFill()
                        {
                            above = selection[index] == 1, // 1 if filled above
                            below = selection[index] == 2  // 2 if filled below
                        },
                        otherFill = null
                    });
                }
            };



            return result;
        }

        #endregion
    }
    public struct Transition
    {
        public EventNode before;
        public EventNode after;

        public StatusNode prev;
        public StatusNode here;
    }

    public class Segment
    {
        public int id = -1;
        public Point start;
        public Point end;
        public SegmentFill myFill;
        public SegmentFill otherFill;

        public Segment()
        {
            myFill = new SegmentFill();
        }

    }

    public class SegmentFill
    {
        // NOTE: This is kind of asinine, but the original javascript code used (below === null) to determine that the edge had not 
        // yet been processed, and treated below as a standard true/false in every other case, necessitating the use of a nullable 
        // bool here.

        public bool above;
        public bool? below;

    }

    public struct Intersection
    {
        public static readonly Intersection Empty = new Intersection();

        //  alongA and alongB will each be one of: -2, -1, 0, 1, 2
        //
        //  with the following meaning:
        //
        //    -2   intersection point is before segment's first point
        //    -1   intersection point is directly on segment's first point
        //     0   intersection point is between segment's first and second points (exclusive)
        //     1   intersection point is directly on segment's second point
        //     2   intersection point is after segment's second point

        /// <summary>
        /// where the intersection point is at
        /// </summary>
        public Point pt;

        /// <summary>
        /// where intersection point is along A
        /// </summary>
        public float alongA;

        /// <summary>
        /// where intersection point is along B
        /// </summary>
        public float alongB;
    }


    public class Polygon
    {
        public List<PointList> regions = null;
        public bool inverted = false;

    }

    public struct Point
    {
        public double x;
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class PointList : List<Point>
    {
        public PointList()
            : base()
        {
        }

        public PointList(int capacity)
            : base(capacity)
        {
        }

    }


    public class CombinedSegmentLists
    {
        public SegmentList combined;
        public bool inverted1;
        public bool inverted2;
    }

    public class SegmentList : List<Segment>
    {
        public bool inverted = false;

        public SegmentList()
            : base()
        {
        }

        public SegmentList(int capacity)
            : base(capacity)
        {
        }
    }
}

