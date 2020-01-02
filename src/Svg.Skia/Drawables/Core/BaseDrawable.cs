﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Parts of this source file are adapted from the https://github.com/vvvv/SVG
using System;
using SkiaSharp;

namespace Svg.Skia
{
    internal abstract class BaseDrawable : SKDrawable
    {
        protected CompositeDisposable _disposable = new CompositeDisposable();
        protected bool _canDraw;
        protected bool _ignoreDisplay;
        protected bool _antialias;
        protected SKRect _skBounds;
        protected SKMatrix _skMatrix;

        protected bool CanDraw(SvgVisualElement svgVisualElement, bool ignoreDisplay)
        {
            bool visible = svgVisualElement.Visible == true;
            bool display = ignoreDisplay ? true : !string.Equals(svgVisualElement.Display, "none", StringComparison.OrdinalIgnoreCase);
            return visible && display;
        }
    }
}
