#Region "Microsoft.VisualBasic::LinearTransforms.vb, Data_science\Mathematica\Math\Math\Algebra\Vector\LinearTransforms.vb"

' Copyright (c) 2018 GPL3 Licensed


' /********************************************************************************/

' Code Statistics:

'   Module LinearTransforms

'   Function: Reflection2D, Reflection3D, Rotation2D, Rotation3D, Scaling, Scaling2D
'             Translation, Transform

' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace LinearAlgebra

    ''' <summary>
    ''' 线性变换辅助模块
    ''' 
    ''' 提供旋转、平移、缩放、反射等常见几何变换的变换矩阵工厂方法，
    ''' 所有的变换矩阵都基于<see cref="NumericMatrix"/>实现，
    ''' 通过矩阵与向量的乘法施加于<see cref="Vector"/>对象
    ''' </summary>
    ''' <remarks>
    ''' 变换矩阵工厂方法均只生成新的矩阵对象，不会修改参与运算的原始向量对象；
    ''' 使用<see cref="LinearTransforms.Transform"/>方法将变换矩阵施加于向量
    ''' </remarks>
    <HideModuleName>
    Public Module LinearTransforms

        ''' <summary>
        ''' 创建二维旋转矩阵：绕原点逆时针旋转<paramref name="theta"/>弧度
        ''' </summary>
        ''' <param name="theta">旋转角度（弧度制）</param>
        ''' <returns>一个2x2的旋转矩阵</returns>
        Public Function Rotation2D(theta As Double) As NumericMatrix
            Dim c As Double = std.Cos(theta)
            Dim s As Double = std.Sin(theta)

            Return New NumericMatrix({
                New Double() {c, -s},
                New Double() {s, c}
            })
        End Function

        ''' <summary>
        ''' 创建三维旋转矩阵：绕指定的坐标轴逆时针旋转<paramref name="theta"/>弧度
        ''' </summary>
        ''' <param name="theta">旋转角度（弧度制）</param>
        ''' <param name="axis">旋转轴：``"x"``/``"y"``/``"z"``（大小写不敏感）</param>
        ''' <returns>一个3x3的旋转矩阵</returns>
        Public Function Rotation3D(theta As Double, axis As String) As NumericMatrix
            If axis Is Nothing Then
                Throw New ArgumentNullException(NameOf(axis))
            End If

            Dim c As Double = std.Cos(theta)
            Dim s As Double = std.Sin(theta)

            Select Case axis.Trim.ToLower
                Case "x"
                    Return New NumericMatrix({
                        New Double() {1, 0, 0},
                        New Double() {0, c, -s},
                        New Double() {0, s, c}
                    })
                Case "y"
                    Return New NumericMatrix({
                        New Double() {c, 0, s},
                        New Double() {0, 1, 0},
                        New Double() {-s, 0, c}
                    })
                Case "z"
                    Return New NumericMatrix({
                        New Double() {c, -s, 0},
                        New Double() {s, c, 0},
                        New Double() {0, 0, 1}
                    })
                Case Else
                    Throw New ArgumentException($"不支持的旋转轴：{axis}，只支持x/y/z", NameOf(axis))
            End Select
        End Function

        ''' <summary>
        ''' 创建二维缩放矩阵
        ''' </summary>
        ''' <param name="sx">x方向缩放因子</param>
        ''' <param name="sy">y方向缩放因子</param>
        ''' <returns>一个2x2的对角缩放矩阵</returns>
        Public Function Scaling2D(sx As Double, sy As Double) As NumericMatrix
            Return New NumericMatrix({
                New Double() {sx, 0},
                New Double() {0, sy}
            })
        End Function

        ''' <summary>
        ''' 创建三维缩放矩阵（对角矩阵）
        ''' </summary>
        ''' <param name="scaleFactors">
        ''' 各个坐标轴方向上的缩放因子，参数的数量决定了矩阵的维度大小
        ''' </param>
        ''' <returns>一个n x n的对角缩放矩阵</returns>
        Public Function Scaling(ParamArray scaleFactors As Double()) As NumericMatrix
            If scaleFactors Is Nothing OrElse scaleFactors.Length = 0 Then
                Throw New ArgumentException("缩放因子不允许为空", NameOf(scaleFactors))
            End If

            Dim n As Integer = scaleFactors.Length
            Dim buffer As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                buffer(i) = New Double(n - 1) {}
                buffer(i)(i) = scaleFactors(i)
            Next

            Return New NumericMatrix(buffer)
        End Function

        ''' <summary>
        ''' 创建二维反射矩阵：关于经过原点、方向角为<paramref name="theta"/>的直线做镜像反射
        ''' </summary>
        ''' <param name="theta">镜像直线的方向角度（弧度制，相对x轴正方向）</param>
        ''' <returns>一个2x2的反射矩阵</returns>
        Public Function Reflection2D(theta As Double) As NumericMatrix
            Dim c As Double = std.Cos(2 * theta)
            Dim s As Double = std.Sin(2 * theta)

            Return New NumericMatrix({
                New Double() {c, s},
                New Double() {s, -c}
            })
        End Function

        ''' <summary>
        ''' 创建n维反射矩阵（Householder变换）：关于法向量为<paramref name="normal"/>的超平面做镜像反射，
        ''' 即``I - 2 n n^T``
        ''' </summary>
        ''' <param name="normal">镜像超平面的法向量（不需要预先单位化）</param>
        ''' <returns>一个n x n的反射矩阵</returns>
        Public Function Reflection3D(normal As Vector) As NumericMatrix
            If normal Is Nothing Then
                Throw New ArgumentNullException(NameOf(normal))
            ElseIf normal.L2Norm() < Double.Epsilon Then
                Throw New ArgumentException("镜像平面的法向量不能为零向量", NameOf(normal))
            End If

            Dim n As Integer = normal.Length
            Dim unit As Vector = normal / normal.L2Norm()
            Dim buffer As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                buffer(i) = New Double(n - 1) {}

                For j As Integer = 0 To n - 1
                    buffer(i)(j) = If(i = j, 1.0, 0.0) - 2.0 * unit(i) * unit(j)
                Next
            Next

            Return New NumericMatrix(buffer)
        End Function

        ''' <summary>
        ''' 创建平移变换矩阵（仿射变换，使用齐次坐标表示）：
        ''' 输入n维偏移向量，返回(n+1)x(n+1)的齐次平移矩阵
        ''' </summary>
        ''' <param name="offset">平移偏移量向量</param>
        ''' <returns>一个(n+1)x(n+1)的齐次坐标平移矩阵</returns>
        ''' <remarks>
        ''' 齐次坐标平移矩阵不能直接与n维向量做矩阵乘法，
        ''' 请使用<see cref="LinearTransforms.Transform"/>方法施加于向量，
        ''' 该方法会自动完成齐次坐标的展开与降维
        ''' </remarks>
        Public Function Translation(offset As Vector) As NumericMatrix
            If offset Is Nothing OrElse offset.Length = 0 Then
                Throw New ArgumentException("平移偏移量向量不允许为空", NameOf(offset))
            End If

            Dim n As Integer = offset.Length
            Dim size As Integer = n + 1
            Dim buffer As Double()() = New Double(size - 1)() {}

            For i As Integer = 0 To size - 1
                buffer(i) = New Double(size - 1) {}
                buffer(i)(i) = 1.0
            Next

            For i As Integer = 0 To n - 1
                buffer(i)(n) = offset(i)
            Next

            Return New NumericMatrix(buffer)
        End Function

        ''' <summary>
        ''' 将变换矩阵施加于向量：计算``M * v``
        ''' </summary>
        ''' <param name="m">变换矩阵</param>
        ''' <param name="v">被变换的向量</param>
        ''' <returns>
        ''' 变换之后的一个新的向量对象，不会修改原始向量
        ''' </returns>
        ''' <remarks>
        ''' 当<paramref name="m"/>的列数为向量长度加一的时候，自动按齐次坐标进行处理：
        ''' 将向量扩展为齐次坐标形式完成乘法运算之后再降维输出，
        ''' 以支持<see cref="LinearTransforms.Translation"/>所生成的仿射平移矩阵
        ''' </remarks>
        <Extension>
        Public Function Transform(m As NumericMatrix, v As Vector) As Vector
            If m Is Nothing Then
                Throw New ArgumentNullException(NameOf(m))
            ElseIf v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v))
            ElseIf m.ColumnDimension <> v.Length AndAlso m.ColumnDimension <> v.Length + 1 Then
                Throw New ArgumentException(
                    $"变换矩阵的列数({m.ColumnDimension})与向量维度({v.Length})不匹配")
            End If

            If m.ColumnDimension = v.Length + 1 Then
                ' 齐次坐标处理：将n维向量扩展为n+1维（末位补1），完成矩阵乘法之后再降维
                Dim homogeneous As Double() = New Double(v.Length) {}

                For i As Integer = 0 To v.Length - 1
                    homogeneous(i) = v(i)
                Next

                homogeneous(v.Length) = 1.0

                Dim result As Vector = m.DotMultiply(New Vector(homogeneous))
                Dim transformed As New Vector(v.Length)

                For i As Integer = 0 To v.Length - 1
                    transformed(i) = result(i)
                Next

                Return transformed
            Else
                Return m.DotMultiply(v)
            End If
        End Function
    End Module
End Namespace
