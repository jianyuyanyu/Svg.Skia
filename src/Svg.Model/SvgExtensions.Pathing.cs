﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
#if USE_SKIASHARP
using SkiaSharp;
#else
using ShimSkiaSharp;
#endif
using Svg.Pathing;

namespace Svg.Model
{
    public static partial class SvgExtensions
    {
        [Flags]
        internal enum PathPointType : byte
        {
            Start = 0,
            Line = 1,
            Bezier = 3,
            Bezier3 = 3,
            PathTypeMask = 0x7,
            DashMode = 0x10,
            PathMarker = 0x20,
            CloseSubpath = 0x80
        }
#if USE_SKIASHARP
        internal static List<(SKPoint Point, byte Type)> GetPathTypes(this SKPath path)
        {
            // TODO: GetPathTypes
            throw new NotImplementedException();
        }
#else
        internal static List<(SKPoint Point, byte Type)> GetPathTypes(this SKPath path)
        {
            // System.Drawing.Drawing2D.GraphicsPath.PathTypes
            // System.Drawing.Drawing2D.PathPointType
            // byte -> PathPointType
            var pathTypes = new List<(SKPoint Point, byte Type)>();

            if (path.Commands is null)
            {
                return pathTypes;
            }
            (SKPoint Point, byte Type) lastPoint = (default, 0);
            foreach (var pathCommand in path.Commands)
            {
                switch (pathCommand)
                {
                    case MoveToPathCommand moveToPathCommand:
                        {
                            var point0 = new SKPoint(moveToPathCommand.X, moveToPathCommand.Y);
                            pathTypes.Add((point0, (byte)PathPointType.Start));
                            lastPoint = (point0, (byte)PathPointType.Start);
                        }
                        break;

                    case LineToPathCommand lineToPathCommand:
                        {
                            var point1 = new SKPoint(lineToPathCommand.X, lineToPathCommand.Y);
                            pathTypes.Add((point1, (byte)PathPointType.Line));
                            lastPoint = (point1, (byte)PathPointType.Line);
                        }
                        break;

                    case CubicToPathCommand cubicToPathCommand:
                        {
                            var point1 = new SKPoint(cubicToPathCommand.X0, cubicToPathCommand.Y0);
                            var point2 = new SKPoint(cubicToPathCommand.X1, cubicToPathCommand.Y1);
                            var point3 = new SKPoint(cubicToPathCommand.X2, cubicToPathCommand.Y2);
                            pathTypes.Add((point1, (byte)PathPointType.Bezier));
                            pathTypes.Add((point2, (byte)PathPointType.Bezier));
                            pathTypes.Add((point3, (byte)PathPointType.Bezier));
                            lastPoint = (point3, (byte)PathPointType.Bezier);
                        }
                        break;

                    case QuadToPathCommand quadToPathCommand:
                        {
                            var point1 = new SKPoint(quadToPathCommand.X0, quadToPathCommand.Y0);
                            var point2 = new SKPoint(quadToPathCommand.X1, quadToPathCommand.Y1);
                            pathTypes.Add((point1, (byte)PathPointType.Bezier));
                            pathTypes.Add((point2, (byte)PathPointType.Bezier));
                            lastPoint = (point2, (byte)PathPointType.Bezier);
                        }
                        break;

                    case ArcToPathCommand arcToPathCommand:
                        {
                            var point1 = new SKPoint(arcToPathCommand.X, arcToPathCommand.Y);
                            pathTypes.Add((point1, (byte)PathPointType.Bezier));
                            lastPoint = (point1, (byte)PathPointType.Bezier);
                        }
                        break;

                    case ClosePathCommand:
                        {
                            lastPoint = (lastPoint.Point, (byte)(lastPoint.Type | (byte)PathPointType.CloseSubpath));
                            pathTypes[pathTypes.Count - 1] = lastPoint;
                        }
                        break;

                    case AddPolyPathCommand addPolyPathCommand:
                        {
                            if (addPolyPathCommand.Points is { } && addPolyPathCommand.Points.Count > 0)
                            {
                                foreach (var nexPoint in addPolyPathCommand.Points)
                                {
                                    var point1 = new SKPoint(nexPoint.X, nexPoint.Y);
                                    pathTypes.Add((point1, (byte)PathPointType.Start));
                                }

                                var point = addPolyPathCommand.Points[addPolyPathCommand.Points.Count - 1];
                                lastPoint = (point, (byte)PathPointType.Line);
                            }
                        }
                        break;

                    default:
                        Debug.WriteLine($"Not implemented path point for {pathCommand?.GetType()} type.");
                        break;
                }
            }

            return pathTypes;
        }
#endif
        internal static SKPath? ToPath(this SvgPathSegmentList? svgPathSegmentList, SvgFillRule svgFillRule)
        {
            if (svgPathSegmentList is null || svgPathSegmentList.Count <= 0)
            {
                return default;
            }

            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var isEndFigure = false;
            var haveFigure = false;

            for (var i = 0; i < svgPathSegmentList.Count; i++)
            {
                var svgSegment = svgPathSegmentList[i];
                var isLast = i == svgPathSegmentList.Count - 1;

                switch (svgSegment)
                {
                    case SvgMoveToSegment svgMoveToSegment:
                        {
                            if (isEndFigure && haveFigure == false)
                            {
                                return default;
                            }

                            if (isLast)
                            {
                                return skPath;
                            }
                            else
                            {
                                if (svgPathSegmentList[i + 1] is SvgMoveToSegment)
                                {
                                    return skPath;
                                }

                                if (svgPathSegmentList[i + 1] is SvgClosePathSegment)
                                {
                                    return skPath;
                                }
                            }
                            isEndFigure = true;
                            haveFigure = false;
                            var x = svgMoveToSegment.Start.X;
                            var y = svgMoveToSegment.Start.Y;
                            skPath.MoveTo(x, y);
                        }
                        break;

                    case SvgLineSegment svgLineSegment:
                        {
                            if (isEndFigure == false)
                            {
                                return default;
                            }
                            haveFigure = true;
                            var x = svgLineSegment.End.X;
                            var y = svgLineSegment.End.Y;
                            skPath.LineTo(x, y);
                        }
                        break;

                    case SvgCubicCurveSegment svgCubicCurveSegment:
                        {
                            if (isEndFigure == false)
                            {
                                return default;
                            }
                            haveFigure = true;
                            var x0 = svgCubicCurveSegment.FirstControlPoint.X;
                            var y0 = svgCubicCurveSegment.FirstControlPoint.Y;
                            var x1 = svgCubicCurveSegment.SecondControlPoint.X;
                            var y1 = svgCubicCurveSegment.SecondControlPoint.Y;
                            var x2 = svgCubicCurveSegment.End.X;
                            var y2 = svgCubicCurveSegment.End.Y;
                            skPath.CubicTo(x0, y0, x1, y1, x2, y2);
                        }
                        break;

                    case SvgQuadraticCurveSegment svgQuadraticCurveSegment:
                        {
                            if (isEndFigure == false)
                            {
                                return default;
                            }
                            haveFigure = true;
                            var x0 = svgQuadraticCurveSegment.ControlPoint.X;
                            var y0 = svgQuadraticCurveSegment.ControlPoint.Y;
                            var x1 = svgQuadraticCurveSegment.End.X;
                            var y1 = svgQuadraticCurveSegment.End.Y;
                            skPath.QuadTo(x0, y0, x1, y1);
                        }
                        break;

                    case SvgArcSegment svgArcSegment:
                        {
                            if (isEndFigure == false)
                            {
                                return default;
                            }
                            haveFigure = true;
                            var rx = svgArcSegment.RadiusX;
                            var ry = svgArcSegment.RadiusY;
                            var xAxisRotate = svgArcSegment.Angle;
                            var largeArc = svgArcSegment.Size == SvgArcSize.Small ? SKPathArcSize.Small : SKPathArcSize.Large;
                            var sweep = svgArcSegment.Sweep == SvgArcSweep.Negative ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise;
                            var x = svgArcSegment.End.X;
                            var y = svgArcSegment.End.Y;
                            skPath.ArcTo(rx, ry, xAxisRotate, largeArc, sweep, x, y);
                        }
                        break;

                    case SvgClosePathSegment _:
                        {
                            if (isEndFigure == false)
                            {
                                return default;
                            }
                            if (haveFigure == false)
                            {
                                return default;
                            }
                            isEndFigure = false;
                            haveFigure = false;
                            skPath.Close();
                        }
                        break;
                }
            }

            if (isEndFigure && haveFigure == false)
            {
                return default;
            }

            return skPath;
        }

        internal static SKPath? ToPath(this SvgPointCollection svgPointCollection, SvgFillRule svgFillRule, bool isClosed, SKRect skViewport)
        {
            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var skPoints = new SKPoint[svgPointCollection.Count / 2];

            for (var i = 0; i + 1 < svgPointCollection.Count; i += 2)
            {
                var x = svgPointCollection[i].ToDeviceValue(UnitRenderingType.Other, null, skViewport);
                var y = svgPointCollection[i + 1].ToDeviceValue(UnitRenderingType.Other, null, skViewport);
                skPoints[i / 2] = new SKPoint(x, y);
            }

            skPath.AddPoly(skPoints, isClosed);

            return skPath;
        }

        internal static SKPath? ToPath(this SvgRectangle svgRectangle, SvgFillRule svgFillRule, SKRect skViewport)
        {
            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var x = svgRectangle.X.ToDeviceValue(UnitRenderingType.Horizontal, svgRectangle, skViewport);
            var y = svgRectangle.Y.ToDeviceValue(UnitRenderingType.Vertical, svgRectangle, skViewport);
            var width = svgRectangle.Width.ToDeviceValue(UnitRenderingType.Horizontal, svgRectangle, skViewport);
            var height = svgRectangle.Height.ToDeviceValue(UnitRenderingType.Vertical, svgRectangle, skViewport);
            var rx = svgRectangle.CornerRadiusX.ToDeviceValue(UnitRenderingType.Horizontal, svgRectangle, skViewport);
            var ry = svgRectangle.CornerRadiusY.ToDeviceValue(UnitRenderingType.Vertical, svgRectangle, skViewport);

            if (width <= 0f || height <= 0f)
            {
                return default;
            }

            if (rx < 0f && ry < 0f)
            {
                rx = 0f;
                ry = 0f;
            }

            if (rx == 0f || ry == 0f)
            {
                rx = 0f;
                ry = 0f;
            }

            if (rx < 0f)
            {
                rx = Math.Abs(rx);
            }

            if (ry < 0f)
            {
                ry = Math.Abs(ry);
            }

            if (rx > 0f)
            {
                var halfWidth = width / 2f;
                if (rx > halfWidth)
                {
                    rx = halfWidth;
                }
            }

            if (ry > 0f)
            {
                var halfHeight = height / 2f;
                if (ry > halfHeight)
                {
                    ry = halfHeight;
                }
            }

            var isRound = rx > 0f && ry > 0f;
            var skRectBounds = SKRect.Create(x, y, width, height);

            if (isRound)
            {
                skPath.AddRoundRect(skRectBounds, rx, ry);
            }
            else
            {
                skPath.AddRect(skRectBounds);
            }

            return skPath;
        }

        internal static SKPath? ToPath(this SvgCircle svgCircle, SvgFillRule svgFillRule, SKRect skViewport)
        {
            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var cx = svgCircle.CenterX.ToDeviceValue(UnitRenderingType.Horizontal, svgCircle, skViewport);
            var cy = svgCircle.CenterY.ToDeviceValue(UnitRenderingType.Vertical, svgCircle, skViewport);
            var radius = svgCircle.Radius.ToDeviceValue(UnitRenderingType.Other, svgCircle, skViewport);

            if (radius <= 0f)
            {
                return default;
            }

            skPath.AddCircle(cx, cy, radius);

            return skPath;
        }

        internal static SKPath? ToPath(this SvgEllipse svgEllipse, SvgFillRule svgFillRule, SKRect skViewport)
        {
            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var cx = svgEllipse.CenterX.ToDeviceValue(UnitRenderingType.Horizontal, svgEllipse, skViewport);
            var cy = svgEllipse.CenterY.ToDeviceValue(UnitRenderingType.Vertical, svgEllipse, skViewport);
            var rx = svgEllipse.RadiusX.ToDeviceValue(UnitRenderingType.Other, svgEllipse, skViewport);
            var ry = svgEllipse.RadiusY.ToDeviceValue(UnitRenderingType.Other, svgEllipse, skViewport);

            if (rx <= 0f || ry <= 0f)
            {
                return default;
            }

            var skRectBounds = SKRect.Create(cx - rx, cy - ry, rx + rx, ry + ry);

            skPath.AddOval(skRectBounds);

            return skPath;
        }

        internal static SKPath? ToPath(this SvgLine svgLine, SvgFillRule svgFillRule, SKRect skViewport)
        {
            var fillType = svgFillRule == SvgFillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            var skPath = new SKPath
            {
                FillType = fillType
            };

            var x0 = svgLine.StartX.ToDeviceValue(UnitRenderingType.Horizontal, svgLine, skViewport);
            var y0 = svgLine.StartY.ToDeviceValue(UnitRenderingType.Vertical, svgLine, skViewport);
            var x1 = svgLine.EndX.ToDeviceValue(UnitRenderingType.Horizontal, svgLine, skViewport);
            var y1 = svgLine.EndY.ToDeviceValue(UnitRenderingType.Vertical, svgLine, skViewport);

            skPath.MoveTo(x0, y0);
            skPath.LineTo(x1, y1);

            return skPath;
        }
    }
}
