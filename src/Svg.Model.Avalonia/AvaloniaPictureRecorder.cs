﻿using A = Avalonia;
using AM = Avalonia.Media;
using AVMI = Avalonia.Visuals.Media.Imaging;

namespace Svg.Model.Avalonia
{
    public static class AvaloniaPictureRecorder
    {
        private static void Record(this CanvasCommand canvasCommand, AvaloniaPicture avaloniaPicture)
        {
            if (avaloniaPicture == null || avaloniaPicture.Commands == null)
            {
                return;
            }

            switch (canvasCommand)
            {
                case ClipPathCanvasCommand clipPathCanvasCommand:
                    {
                        var path = clipPathCanvasCommand.Path.ToGeometry(false);
                        if (path != null)
                        {
                            // TODO: clipPathCanvasCommand.Operation;
                            // TODO: clipPathCanvasCommand.Antialias;
                            avaloniaPicture.Commands.Add(new GeometryClipDrawCommand(path));
                        }
                    }
                    break;
                case ClipRectCanvasCommand clipRectCanvasCommand:
                    {
                        var rect = clipRectCanvasCommand.Rect.ToSKRect();
                        // TODO: clipRectCanvasCommand.Operation;
                        // TODO: clipRectCanvasCommand.Antialias;
                        avaloniaPicture.Commands.Add(new ClipDrawCommand(rect));
                    }
                    break;
                case SaveCanvasCommand _:
                    {
                        // TODO:
                        avaloniaPicture.Commands.Add(new SaveDrawCommand());
                    }
                    break;
                case RestoreCanvasCommand _:
                    {
                        // TODO:
                        avaloniaPicture.Commands.Add(new RestoreDrawCommand());
                    }
                    break;
                case SetMatrixCanvasCommand setMatrixCanvasCommand:
                    {
                        var matrix = setMatrixCanvasCommand.Matrix.ToMatrix();
                        avaloniaPicture.Commands.Add(new SetTransformDrawCommand(matrix));
                    }
                    break;
                case SaveLayerCanvasCommand saveLayerCanvasCommand:
                    {
                        // TODO:
                        avaloniaPicture.Commands.Add(new SaveLayerDrawCommand());
                    }
                    break;
                case DrawImageCanvasCommand drawImageCanvasCommand:
                    {
                        if (drawImageCanvasCommand.Image != null)
                        {
                            var image = drawImageCanvasCommand.Image.ToBitmap();
                            var source = drawImageCanvasCommand.Source.ToSKRect();
                            var dest = drawImageCanvasCommand.Dest.ToSKRect();
                            var bitmapInterpolationMode = drawImageCanvasCommand.Paint?.FilterQuality.ToBitmapInterpolationMode() ?? AVMI.BitmapInterpolationMode.Default;
                            avaloniaPicture.Commands.Add(new ImageDrawCommand(image, source, dest, bitmapInterpolationMode));
                        }
                    }
                    break;
                case DrawPathCanvasCommand drawPathCanvasCommand:
                    {
                        if (drawPathCanvasCommand.Path != null && drawPathCanvasCommand.Paint != null)
                        {
                            (var brush, var pen) = drawPathCanvasCommand.Paint.ToBrushAndPen();

                            if (drawPathCanvasCommand.Path.Commands?.Count == 1)
                            {
                                var pathCommand = drawPathCanvasCommand.Path.Commands[0];
                                var success = false;

                                switch (pathCommand)
                                {
                                    case AddRectPathCommand addRectPathCommand:
                                        {
                                            var rect = addRectPathCommand.Rect.ToSKRect();
                                            avaloniaPicture.Commands.Add(new RectangleDrawCommand(brush, pen, rect, 0, 0));
                                            success = true;
                                        }
                                        break;
                                    case AddRoundRectPathCommand addRoundRectPathCommand:
                                        {
                                            var rect = addRoundRectPathCommand.Rect.ToSKRect();
                                            var rx = addRoundRectPathCommand.Rx;
                                            var ry = addRoundRectPathCommand.Ry;
                                            avaloniaPicture.Commands.Add(new RectangleDrawCommand(brush, pen, rect, rx, ry));
                                            success = true;
                                        }
                                        break;
                                    case AddOvalPathCommand addOvalPathCommand:
                                        {
                                            var rect = addOvalPathCommand.Rect.ToSKRect();
                                            var ellipseGeometry = new AM.EllipseGeometry(rect);
                                            avaloniaPicture.Commands.Add(new GeometryDrawCommand(brush, pen, ellipseGeometry));
                                            success = true;
                                        }
                                        break;
                                    case AddCirclePathCommand addCirclePathCommand:
                                        {
                                            var x = addCirclePathCommand.X;
                                            var y = addCirclePathCommand.Y;
                                            var radius = addCirclePathCommand.Radius;
                                            var rect = new A.Rect(x - radius, y - radius, radius + radius, radius + radius);
                                            var ellipseGeometry = new AM.EllipseGeometry(rect);
                                            avaloniaPicture.Commands.Add(new GeometryDrawCommand(brush, pen, ellipseGeometry));
                                            success = true;
                                        }
                                        break;
                                    case AddPolyPathCommand addPolyPathCommand:
                                        {
                                            if (addPolyPathCommand.Points != null)
                                            {
                                                var points = addPolyPathCommand.Points.ToPoints();
                                                var close = addPolyPathCommand.Close;
                                                var polylineGeometry = new AM.PolylineGeometry(points, close);
                                                avaloniaPicture.Commands.Add(new GeometryDrawCommand(brush, pen, polylineGeometry));
                                                success = true;
                                            }
                                        }
                                        break;
                                }

                                if (success)
                                {
                                    break;
                                }
                            }

                            if (drawPathCanvasCommand.Path.Commands?.Count == 2)
                            {
                                var pathCommand1 = drawPathCanvasCommand.Path.Commands[0];
                                var pathCommand2 = drawPathCanvasCommand.Path.Commands[1];

                                if (pathCommand1 is MoveToPathCommand moveTo && pathCommand2 is LineToPathCommand lineTo)
                                {
                                    var p1 = new A.Point(moveTo.X, moveTo.Y);
                                    var p2 = new A.Point(lineTo.X, lineTo.Y);
                                    avaloniaPicture.Commands.Add(new LineDrawCommand(pen, p1, p2));
                                    break;
                                }
                            }

                            var geometry = drawPathCanvasCommand.Path.ToGeometry(brush != null);
                            if (geometry != null)
                            {
                                avaloniaPicture.Commands.Add(new GeometryDrawCommand(brush, pen, geometry));
                            }
                        }
                    }
                    break;
                case DrawPositionedTextCanvasCommand drawPositionedTextCanvasCommand:
                    {
                        // TODO:
                    }
                    break;
                case DrawTextCanvasCommand drawTextCanvasCommand:
                    {
                        if (drawTextCanvasCommand.Paint != null)
                        {
                            (var brush, _) = drawTextCanvasCommand.Paint.ToBrushAndPen();
                            var text = drawTextCanvasCommand.Paint.ToFormattedText(drawTextCanvasCommand.Text);
                            var x = drawTextCanvasCommand.X;
                            var y = drawTextCanvasCommand.Y;
                            var origin = new A.Point(x, y - drawTextCanvasCommand.Paint.TextSize);
                            avaloniaPicture.Commands.Add(new TextDrawCommand(brush, origin, text));
                        }
                    }
                    break;
                case DrawTextOnPathCanvasCommand drawTextOnPathCanvasCommand:
                    {
                        // TODO:
                    }
                    break;
                default:
                    break;
            }
        }

        public static AvaloniaPicture Record(this Picture picture)
        {
            var avaloniaPicture = new AvaloniaPicture();

            if (picture.Commands == null)
            {
                return avaloniaPicture;
            }

            foreach (var canvasCommand in picture.Commands)
            {
                canvasCommand.Record(avaloniaPicture);
            }

            return avaloniaPicture;
        }
    }
}
