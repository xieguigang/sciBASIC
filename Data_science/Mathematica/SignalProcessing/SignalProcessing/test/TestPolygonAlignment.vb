Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.MachineVision

Module TestPolygonAlignment

    Sub Main()

        ' 创建两个多边形
        Dim sourcePoly As New Polygon2D({0, 1, 1, 0}, {0, 0, 1, 1})
        Dim targetPoly As New Polygon2D({2, 3, 3, 2}, {1, 1, 2, 2})
        ' 执行对齐
        Dim args As (theta As Double, tx As Double, ty As Double, scale As Double) = RANSACPointAlignment.AlignPolygons(
    sourcePoly,
    targetPoly,
    iterations:=1000,
    distanceThreshold:=0.5
)

        ' 输出结果
        Console.WriteLine($"旋转角度: {args.theta} 弧度")
        Console.WriteLine($"平移量: tx={args.tx}, ty={args.ty}")
        Console.WriteLine($"缩放因子: sx={args.scale}, sy={args.scale}")

        Pause()
    End Sub
End Module
