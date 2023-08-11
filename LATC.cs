// Decompiled with JetBrains decompiler
// Type: vatsys.LATC
// Assembly: vatSys, Version=0.4.8114.34539, Culture=neutral, PublicKeyToken=null
// MVID: E82FB2F8-DAB0-42FD-91AA-1C44F8E62564
// Assembly location: E:\vatsys\bin\vatSys.exe
// XML documentation location: E:\vatsys\bin\vatSys.xml

using System;
using System.Collections.Generic;
using System.Linq;

namespace vatsys
{
    internal class LATC
    {
        public List<LATC.Segment> Segments = new List<LATC.Segment>();
        public DateTime Timeout = DateTime.MaxValue;

        public LATC(Track track1, Track track2, int value) => this.CalculateLATC(track1, track2, value);

        public LATC()
        {
        }

        public void CalculateLATC(Track track1, Track track2, int value)
        {
            FDP2.FDR fdr1 = track1.GetFDR();
            FDP2.FDR fdr2 = track2.GetFDR();
            if (fdr1 == null || fdr2 == null)
                return;
            this.Segments.AddRange((IEnumerable<LATC.Segment>)this.CalculateAreaOfConflict(fdr1, fdr2, value));
            this.Segments.AddRange((IEnumerable<LATC.Segment>)this.CalculateAreaOfConflict(fdr2, fdr1, value));
        }

        private List<LATC.Segment> CalculateAreaOfConflict(FDP2.FDR fdr1, FDP2.FDR fdr2, int value)
        {
            List<LATC.Segment> segs = new List<LATC.Segment>();
            List<FDP2.FDR.ExtractedRoute.Segment> list1 = fdr1.ParsedRoute.ToList().Where<FDP2.FDR.ExtractedRoute.Segment>((Func<FDP2.FDR.ExtractedRoute.Segment, bool>)(s => s.Type == FDP2.FDR.ExtractedRoute.Segment.SegmentTypes.WAYPOINT)).ToList<FDP2.FDR.ExtractedRoute.Segment>();
            List<FDP2.FDR.ExtractedRoute.Segment> fdr2Route = fdr2.ParsedRoute.ToList().Where<FDP2.FDR.ExtractedRoute.Segment>((Func<FDP2.FDR.ExtractedRoute.Segment, bool>)(s => s.Type == FDP2.FDR.ExtractedRoute.Segment.SegmentTypes.WAYPOINT)).ToList<FDP2.FDR.ExtractedRoute.Segment>();
            for (int index1 = 1; index1 < list1.Count; ++index1)
            {
                List<Coordinate> polygon = this.CreatePolygon(list1[index1 - 1].Intersection.LatLong, list1[index1].Intersection.LatLong, value);
                for (int j = 1; j < fdr2Route.Count; j++)
                {
                    List<Coordinate> source = new List<Coordinate>();
                    List<Coordinate> coordinateList = new List<Coordinate>();
                    source.AddRange((IEnumerable<Coordinate>)this.CalculatePolygonIntersections(polygon, fdr2Route[j - 1].Intersection.LatLong, fdr2Route[j].Intersection.LatLong));
                    int num1 = 0;
                    int num2 = 0;
                    foreach (Coordinate coordinate in source.ToList<Coordinate>())
                    {
                        if (Conversions.IsLatLonOnGC(fdr2Route[j - 1].Intersection.LatLong, fdr2Route[j].Intersection.LatLong, coordinate))
                        {
                            coordinateList.Add(coordinate);
                        }
                        else
                        {
                            double track = Conversions.CalculateTrack(fdr2Route[j - 1].Intersection.LatLong, fdr2Route[j].Intersection.LatLong);
                            if (Math.Abs(track - Conversions.CalculateTrack(fdr2Route[j - 1].Intersection.LatLong, coordinate)) > 90.0)
                                ++num1;
                            if (Math.Abs(track - Conversions.CalculateTrack(coordinate, fdr2Route[j].Intersection.LatLong)) > 90.0)
                                ++num2;
                        }
                    }
                    if (num1 % 2 != 0 && num2 % 2 != 0)
                    {
                        coordinateList.Clear();
                        coordinateList.Add(fdr2Route[j - 1].Intersection.LatLong);
                        coordinateList.Add(fdr2Route[j].Intersection.LatLong);
                    }
                    else if (num2 % 2 != 0)
                        coordinateList.Add(fdr2Route[j].Intersection.LatLong);
                    else if (num1 % 2 != 0)
                        coordinateList.Add(fdr2Route[j - 1].Intersection.LatLong);
                    coordinateList.Sort((Comparison<Coordinate>)((x, y) => Conversions.CalculateDistance(fdr2Route[j - 1].Intersection.LatLong, x).CompareTo(Conversions.CalculateDistance(fdr2Route[j - 1].Intersection.LatLong, y))));
                    for (int index2 = 1; index2 < coordinateList.Count; index2 += 2)
                    {
                        LATC.Segment seg = new LATC.Segment();
                        seg.startLatlong = coordinateList[index2 - 1];
                        seg.endLatlong = coordinateList[index2];
                        List<LATC.Segment> list2 = segs.Where<LATC.Segment>(closure_0 ?? (closure_0 = (Func<LATC.Segment, bool>)(s => s.routeSegment == fdr2Route[j]))).Where<LATC.Segment>((Func<LATC.Segment, bool>)(s => Conversions.CalculateDistance(s.startLatlong, fdr2Route[j - 1].Intersection.LatLong) < Conversions.CalculateDistance(seg.startLatlong, fdr2Route[j - 1].Intersection.LatLong) && Conversions.CalculateDistance(s.endLatlong, fdr2Route[j - 1].Intersection.LatLong) > Conversions.CalculateDistance(seg.startLatlong, fdr2Route[j - 1].Intersection.LatLong) || Conversions.CalculateDistance(s.endLatlong, fdr2Route[j - 1].Intersection.LatLong) > Conversions.CalculateDistance(seg.endLatlong, fdr2Route[j - 1].Intersection.LatLong) && Conversions.CalculateDistance(s.startLatlong, fdr2Route[j - 1].Intersection.LatLong) < Conversions.CalculateDistance(seg.endLatlong, fdr2Route[j - 1].Intersection.LatLong) || Conversions.CalculateDistance(s.startLatlong, fdr2Route[j - 1].Intersection.LatLong) > Conversions.CalculateDistance(seg.startLatlong, fdr2Route[j - 1].Intersection.LatLong) && Conversions.CalculateDistance(s.endLatlong, fdr2Route[j - 1].Intersection.LatLong) < Conversions.CalculateDistance(seg.endLatlong, fdr2Route[j - 1].Intersection.LatLong) || Conversions.CalculateDistance(s.startLatlong, seg.startLatlong) < 0.01 || Conversions.CalculateDistance(s.endLatlong, seg.endLatlong) < 0.01)).ToList<LATC.Segment>();
                        if (list2.Count > 0)
                        {
                            foreach (LATC.Segment segment in list2)
                            {
                                if (Conversions.CalculateDistance(segment.endLatlong, fdr2Route[j - 1].Intersection.LatLong) < Conversions.CalculateDistance(seg.endLatlong, fdr2Route[j - 1].Intersection.LatLong))
                                    segment.endLatlong = seg.endLatlong;
                                if (Conversions.CalculateDistance(seg.startLatlong, fdr2Route[j - 1].Intersection.LatLong) < Conversions.CalculateDistance(segment.startLatlong, fdr2Route[j - 1].Intersection.LatLong))
                                    segment.startLatlong = seg.startLatlong;
                            }
                        }
                        else
                        {
                            seg.callsign = fdr2.Callsign;
                            seg.routeSegment = fdr2Route[j];
                            segs.Add(seg);
                        }
                    }
                }
            }
            for (int i = 0; i < segs.Count; i++)
            {
                if (!segs.Exists((Predicate<LATC.Segment>)(s => Conversions.CalculateDistance(segs[i].startLatlong, s.endLatlong) < 0.01)))
                    segs[i].startTime = FDP2.GetSystemEstimateAtPosition(fdr2, segs[i].startLatlong, segs[i].routeSegment);
                if (!segs.Exists((Predicate<LATC.Segment>)(s => Conversions.CalculateDistance(segs[i].endLatlong, s.startLatlong) < 0.01)))
                    segs[i].endTime = FDP2.GetSystemEstimateAtPosition(fdr2, segs[i].endLatlong, segs[i].routeSegment);
            }
            return segs;
        }

        private List<Coordinate> CreatePolygon(Coordinate point1, Coordinate point2, int value)
        {
            List<Coordinate> polygon = new List<Coordinate>();
            double track = Conversions.CalculateTrack(point1, point2);
            double num1 = track - 90.0;
            for (int index = 0; index <= 180; index += 10)
            {
                double heading = num1 - (double)index;
                Coordinate fromBearingRange = Conversions.CalculateLLFromBearingRange(point1, (double)value, heading);
                polygon.Add(fromBearingRange);
            }
            double num2 = track + 90.0;
            for (int index = 0; index <= 180; index += 10)
            {
                double heading = num2 - (double)index;
                Coordinate fromBearingRange = Conversions.CalculateLLFromBearingRange(point2, (double)value, heading);
                polygon.Add(fromBearingRange);
            }
            polygon.Add(polygon[0]);
            return polygon;
        }

        private List<Coordinate> CalculatePolygonIntersections(
          List<Coordinate> polygon,
          Coordinate point1,
          Coordinate point2)
        {
            List<Coordinate> polygonIntersections = new List<Coordinate>();
            for (int index = 1; index < polygon.Count; ++index)
            {
                List<Coordinate> gcIntersectionLl = Conversions.CalculateAllGCIntersectionLL(polygon[index - 1], polygon[index], point1, point2);
                if (gcIntersectionLl != null)
                    polygonIntersections.AddRange((IEnumerable<Coordinate>)gcIntersectionLl);
            }
            for (int index = 0; index < polygonIntersections.Count; ++index)
            {
                Coordinate intsect = polygonIntersections[index];
                polygonIntersections.RemoveAll((Predicate<Coordinate>)(c => c != intsect && Conversions.CalculateDistance(intsect, c) < 0.01));
            }
            return polygonIntersections;
        }

        public class Segment
        {
            public string callsign;
            public Coordinate startLatlong;
            public Coordinate endLatlong;
            public DateTime startTime = DateTime.MaxValue;
            public DateTime endTime = DateTime.MaxValue;
            public FDP2.FDR.ExtractedRoute.Segment routeSegment;
        }
    }
}
