#Region "Microsoft.VisualBasic::4d23632054e0e7d52d20c253a38afc1c, Data_science\Mathematica\Math\Math\Algebra\Helpers\EllipseFitResult.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 159
    '    Code Lines: 92 (57.86%)
    ' Comment Lines: 44 (27.67%)
    '    - Xml Docs: 65.91%
    ' 
    '   Blank Lines: 23 (14.47%)
    '     File Size: 6.04 KB


    '     Class EllipseFitResult
    ' 
    '         Properties: Center, Coefficients, RotationAngle, SemiMajorAxis, SemiMinorAxis
    ' 
    '         Function: BuildDesignMatrix, CreateShape, ExtractEllipseParameters, FitEllipse, NormalizeAngle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace LinearAlgebra

    Public Class EllipseFitResult

        ''' <summary>
        ''' 椭圆的中心点（h,k）
        ''' </summary>
        ''' <returns></returns>
        Public Property Center As PointF
        ''' <summary>
        ''' 长半轴
        ''' </summary>
        ''' <returns></returns>
        Public Property SemiMajorAxis As Single
        ''' <summary>
        ''' 短半轴
        ''' </summary>
        ''' <returns></returns>
        Public Property SemiMinorAxis As Single
        ''' <summary>
        ''' 旋转角度（弧度）
        ''' </summary>
        ''' <returns></returns>
        Public Property RotationAngle As Single
        ''' <summary>
        ''' 原始系数向量 [A, B, C, D, E, F]
        ''' </summary>
        ''' <returns></returns>
        Public Property Coefficients As Double()

        ''' <summary>
        ''' 计算并返回椭圆的面积。
        ''' </summary>
        ''' <returns>椭圆的面积，类型为 Double。</returns>
        Public ReadOnly Property Area As Double
            Get
                ' 椭圆面积公式: π * a * b
                ' 使用 Math.PI 获取高精度的圆周率
                ' 结果使用 Double 类型以保证精度
                Return std.PI * Me.SemiMajorAxis * Me.SemiMinorAxis
            End Get
        End Property

        Public Function CreateShape() As EllipseShape
            Return New EllipseShape(SemiMajorAxis, SemiMinorAxis, Center)
        End Function

        Public Shared Function FitEllipse(points As PointF(), Optional strict As Boolean = True) As EllipseFitResult
            If points.TryCount < 6 Then
                Return Nothing
            Else
                ' 步骤1：构建设计矩阵M
                Dim M As NumericMatrix = BuildDesignMatrix(points)
                ' 步骤2：对M进行SVD分解
                Dim svd As New SingularValueDecomposition(M)
                ' 步骤3：取V的最后一列作为椭圆系数向量
                Dim V As NumericMatrix = svd.V.Transpose()
                Dim coefficients As Vector = V(V.ColumnDimension - 1, byRow:=False)

                ' 步骤4：从系数向量提取椭圆几何参数
                Return ExtractEllipseParameters(coefficients, strict)
            End If
        End Function

        ''' <summary>
        ''' 构建设计矩阵 M = [x², xy, y², x, y, 1]
        ''' </summary>
        Private Shared Function BuildDesignMatrix(points As PointF()) As NumericMatrix
            Dim rowCount As Integer = points.Length
            Dim M As New NumericMatrix(rowCount, 6)

            For i As Integer = 0 To rowCount - 1
                Dim x As Double = points(i).X
                Dim y As Double = points(i).Y

                M(i, 0) = x * x ' x²
                M(i, 1) = x * y ' xy
                M(i, 2) = y * y ' y²
                M(i, 3) = x     ' x
                M(i, 4) = y     ' y
                M(i, 5) = 1.0   ' 常数项
            Next

            Return M
        End Function

        Const InvalidEllipseFit As String = "The fitting result of {0} is not an ellipse (it could be a hyperbola or a parabola)."

        ''' <summary>
        ''' 从系数向量提取椭圆几何参数
        ''' </summary>
        Private Shared Function ExtractEllipseParameters(coefficients As Vector, strict As Boolean) As EllipseFitResult
            ' 提取系数：v = [A, B, C, D, E, F]^T
            Dim A As Double = coefficients(0)
            Dim B As Double = coefficients(1)
            Dim C As Double = coefficients(2)
            Dim D As Double = coefficients(3)
            Dim E As Double = coefficients(4)
            Dim F As Double = coefficients(5)

            ' 验证是否为椭圆 (B² - 4AC < 0)
            Dim discriminant As Double = B * B - 4 * A * C

            If discriminant >= 0 Then
                Dim msg As String = String.Format(InvalidEllipseFit, $"v = [{coefficients.JoinBy(", ")}]^T")

                If strict Then
                    Throw New InvalidOperationException(msg)
                Else
                    Call msg.warning
                    Return Nothing
                End If
            End If

            ' 步骤4.1：计算中心点 (h, k)
            Dim denominator As Double = B * B - 4 * A * C
            Dim h As Double = (2 * C * D - B * E) / denominator
            Dim k As Double = (2 * A * E - B * D) / denominator

            ' 步骤4.2：计算旋转角度 θ
            Dim theta As Double = 0.5 * std.Atan2(B, A - C)

            ' 步骤4.3：计算平移后的常数项 F'
            Dim F_prime As Double = A * h * h + B * h * k + C * k * k + D * h + E * k + F

            ' 步骤4.4：计算二次型矩阵的特征值
            Dim Q As New NumericMatrix(2, 2)
            Q(0, 0) = A
            Q(0, 1) = B / 2
            Q(1, 0) = B / 2
            Q(1, 1) = C

            Dim eigenValues As New EigenvalueDecomposition(Q)
            Dim eigenReals As Double() = eigenValues.RealEigenvalues
            Dim lambda1 As Double = eigenReals(0)
            Dim lambda2 As Double = eigenReals(1)

            ' 确保特征值顺序正确（λ1 ≤ λ2）
            If lambda1 > lambda2 Then
                Dim temp As Double = lambda1
                lambda1 = lambda2
                lambda2 = temp
            End If

            ' 步骤4.5：计算长轴和短轴
            Dim semiMajorAxis As Double = std.Sqrt(-F_prime / lambda1)  ' 长轴
            Dim semiMinorAxis As Double = std.Sqrt(-F_prime / lambda2)  ' 短轴

            ' 确保长轴≥短轴
            If semiMajorAxis < semiMinorAxis Then
                Dim temp As Double = semiMajorAxis
                semiMajorAxis = semiMinorAxis
                semiMinorAxis = temp
                ' 调整角度（长轴方向）
                theta += std.PI / 2
            End If

            ' 规范化角度到 [0, π) 范围
            theta = NormalizeAngle(theta)

            Return New EllipseFitResult With {
                .Center = New PointF(CSng(h), CSng(k)),
                .SemiMajorAxis = semiMajorAxis,
                .SemiMinorAxis = semiMinorAxis,
                .RotationAngle = theta,
                .Coefficients = coefficients
            }
        End Function

        ''' <summary>
        ''' 规范化角度到 [0, π) 范围
        ''' </summary>
        Private Shared Function NormalizeAngle(angle As Double) As Double
            Dim normalized As Double = angle Mod std.PI
            If normalized < 0 Then
                normalized += std.PI
            End If
            Return normalized
        End Function
    End Class
End Namespace
