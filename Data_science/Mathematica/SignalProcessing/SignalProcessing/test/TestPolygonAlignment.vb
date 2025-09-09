Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.MachineVision
Imports std = System.Math

Module TestPolygonAlignment

    Sub Main()

        Dim rect As New Rectangle(80, 100, 23, 28)
        Dim rect2 As New Rectangle(180, 200, 50, 28)
        Dim rect3 As New Rectangle(100, 80, 50, 120)

        ' 创建两个多边形
        Dim sourcePoly As New Polygon2D(rect, rect2, rect3)
        Dim targetPoly As New Polygon2D(rect, rect2, rect3)

        Call GeomTransform.Rotate(sourcePoly.xpoints, sourcePoly.ypoints, std.PI / 3)
        Call GeomTransform.Scale(sourcePoly.xpoints, sourcePoly.ypoints, 5, 3)
        Call GeomTransform.Translate(sourcePoly.xpoints, sourcePoly.ypoints, -3, 5.2)

        ' 执行对齐
        Dim args As Transform = RANSACPointAlignment.AlignPolygons(
    sourcePoly,
    targetPoly,
    iterations:=1000,
    distanceThreshold:=0.5
)

        ' 输出结果
        Console.WriteLine($"旋转角度: {args.theta} 弧度 ~ {std.PI / 3}")
        Console.WriteLine($"平移量: tx={args.tx}, ty={args.ty}")
        Console.WriteLine($"缩放因子: sx={args.scalex}, sy={args.scaley}")

        Dim plot1 As New Bitmap(500, 500)

        Using gfx As Graphics = Graphics.FromImage(plot1)
            Call gfx.FillPolygon(Brushes.Red, sourcePoly.AsEnumerable.ToArray)

            Call GeomTransform.Rotate(sourcePoly.xpoints, sourcePoly.ypoints, args.theta)
            Call GeomTransform.Scale(sourcePoly.xpoints, sourcePoly.ypoints, args.scalex, args.scaley)
            Call GeomTransform.Translate(sourcePoly.xpoints, sourcePoly.ypoints, args.tx, args.ty)

            Call gfx.DrawPolygon(Pens.Blue, targetPoly.AsEnumerable.ToArray)
            Call gfx.DrawPolygon(Pens.Green, sourcePoly.AsEnumerable.ToArray)
        End Using

        Call plot1.SaveAs("./test_alignment.png")

        Pause()
    End Sub
End Module
