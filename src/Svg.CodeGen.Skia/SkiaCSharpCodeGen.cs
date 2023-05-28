﻿#nullable enable
using System.Text;
using ShimSkiaSharp;

namespace Svg.CodeGen.Skia;

public static class SkiaCSharpCodeGen
{
    public static string Generate(SKPicture picture, string namespaceName, string className)
    {
        var counter = new SkiaCSharpCodeGenCounter();

        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine($"");
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine($"{{");
        sb.AppendLine($"    using System;");
        sb.AppendLine($"    using SkiaSharp;");
        sb.AppendLine($"");
        sb.AppendLine($"    public static class {className}");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        public static SKPicture Picture {{ get; }}");
        sb.AppendLine($"");
        sb.AppendLine($"        static {className}()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            Picture = Record();");
        sb.AppendLine($"        }}");
        sb.AppendLine($"");
        sb.AppendLine($"        private static SKPicture Record()");
        sb.AppendLine($"        {{");

        var indent = "            ";

        var counterPicture = ++counter.Picture;
        picture.ToSKPicture(counter, sb, indent);

        sb.AppendLine($"{indent}return {counter.PictureVarName}{counterPicture};");

        sb.AppendLine($"        }}");
        sb.AppendLine($"");
        sb.AppendLine($"        public static void Draw(SKCanvas {counter.CanvasVarName})");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            {counter.CanvasVarName}.DrawPicture(Picture);");
        sb.AppendLine($"        }}");
        sb.AppendLine($"    }}");
        sb.AppendLine($"}}");

        var code = sb.ToString();
        return code;
    }
}