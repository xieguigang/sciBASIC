Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.MachineVision
Imports std = System.Math

Module TestPolygonAlignment

    Sub Main()

        Dim rect As New Rectangle(80, 100, 23, 28)

        ' 创建两个多边形
        Dim sourcePoly As New Polygon2D(rect)
        Dim targetPoly As New Polygon2D(rect)

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

        Dim plot1 As New Bitmap(200, 200)

        Using gfx As Graphics = Graphics.FromImage(plot1)
            Call gfx.FillPolygon(Brushes.Red, sourcePoly.AsEnumerable.ToArray)
            Call gfx.DrawPolygon(Pens.Blue, targetPoly.AsEnumerable.ToArray)
        End Using

        Call plot1.SaveAs("./test_alignment.png")

        Pause()
    End Sub
End Module
