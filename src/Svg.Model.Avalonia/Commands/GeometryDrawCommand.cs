﻿using AM = Avalonia.Media;

namespace Svg.Model.Avalonia
{
    internal class GeometryDrawCommand : DrawCommand
    {
        public readonly AM.IBrush? Brush;
        public readonly AM.IPen? Pen;
        public readonly AM.Geometry? Geometry;

        public GeometryDrawCommand(AM.IBrush? brush, AM.IPen? pen, AM.Geometry? geometry)
        {
            Brush = brush;
            Pen = pen;
            Geometry = geometry;
        }
    }
}
