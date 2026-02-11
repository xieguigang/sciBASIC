#Region "Microsoft.VisualBasic::e7d467520d817e014f4f748dc57c40a7, Data_science\Mathematica\Math\Math\Algebra\Helpers\PCAEllipseResult.vb"

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

    '   Total Lines: 389
    '    Code Lines: 211 (54.24%)
    ' Comment Lines: 117 (30.08%)
    '    - Xml Docs: 70.94%
    ' 
    '   Blank Lines: 61 (15.68%)
    '     File Size: 14.76 KB


    '     Class PCAEllipseResult
    ' 
    '         Properties: Area, Center, ConfidenceLevel, CovarianceMatrix, EigenValues
    '                     EigenVectors, RotationAngle, SemiMajorAxis, SemiMinorAxis
    ' 
    '         Function: CreateShape
    ' 
    '     Class PCAEllipseCalculator
    ' 
    ' 
    '         Enum EllipseType
    ' 
    '             Euclidean, Normal, T
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: CalculateEllipse, CalculateScaleFactor, ChiSquareQuantile, EigenDecompose, FQuantile
    '               NormalizeAngle, NormalizeVector, NormalQuantile
    ' 
    '     Class EigenDecompositionResult
    ' 
    '         Properties: EigenValues, EigenVectors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::PCA_Ellipse_Calculator"
' Author: Assistant
' Description: 统计椭圆计算模块，用于PCA可视化中的分组椭圆绘制
' Mathematical Basis: 基于协方差矩阵和多元正态分布的置信椭圆
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace LinearAlgebra
    ''' <summary>
    ''' PCA统计椭圆计算结果
    ''' 基于协方差矩阵和分布假设的数据椭圆/置信椭圆
    ''' </summary>
    Public Class PCAEllipseResult
        ''' <summary>
        ''' 椭圆中心点（均值）
        ''' </summary>
        Public Property Center As PointF

        ''' <summary>
        ''' 长半轴长度
        ''' </summary>
        Public Property SemiMajorAxis As Single

        ''' <summary>
        ''' 短半轴长度
        ''' </summary>
        Public Property SemiMinorAxis As Single

        ''' <summary>
        ''' 旋转角度（弧度，从X轴逆时针方向）
        ''' </summary>
        Public Property RotationAngle As Single

        ''' <summary>
        ''' 置信水平（例如0.95表示95%置信椭圆）
        ''' </summary>
        Public Property ConfidenceLevel As Single

        ''' <summary>
        ''' 协方差矩阵 [2x2]
        ''' </summary>
        Public Property CovarianceMatrix As Double(,)

        ''' <summary>
        ''' 特征值数组 [2]
        ''' </summary>
        Public Property EigenValues As Double()

        ''' <summary>
        ''' 特征向量矩阵 [2x2]，每列是一个特征向量
        ''' </summary>
        Public Property EigenVectors As Double(,)

        ''' <summary>
        ''' 计算椭圆面积
        ''' </summary>
        Public ReadOnly Property Area As Double
            Get
                Return std.PI * SemiMajorAxis * SemiMinorAxis
            End Get
        End Property

        ''' <summary>
        ''' 创建椭圆形状对象
        ''' </summary>
        Public Function CreateShape() As EllipseShape
            Return New EllipseShape(SemiMajorAxis, SemiMinorAxis, Center)
        End Function
    End Class

    ''' <summary>
    ''' PCA椭圆计算器
    ''' </summary>
    Public Class PCAEllipseCalculator

        ''' <summary>
        ''' 椭圆类型枚举
        ''' </summary>
        Public Enum EllipseType
            ''' <summary>
            ''' 基于多元正态分布的置信椭圆
            ''' </summary>
            Normal
            ''' <summary>
            ''' 基于多元t分布的数据椭圆（更稳健，对离群点不敏感）
            ''' </summary>
            T
            ''' <summary>
            ''' 欧几里得距离圆（不考虑协方差结构）
            ''' </summary>
            Euclidean
        End Enum

        ''' <summary>
        ''' 计算PCA统计椭圆
        ''' </summary>
        ''' <param name="points">PCA得分散点集合</param>
        ''' <param name="confidenceLevel">置信水平（0-1之间，默认0.95）</param>
        ''' <param name="ellipseType">椭圆类型（默认Normal）</param>
        ''' <param name="degreesOfFreedom">t分布的自由度（默认None，自动计算）</param>
        ''' <returns>PCA椭圆计算结果</returns>
        Public Shared Function CalculateEllipse(
        points As PointF(),
        Optional confidenceLevel As Single = 0.95F,
        Optional ellipseType As EllipseType = EllipseType.Normal,
        Optional degreesOfFreedom As Integer? = Nothing
    ) As PCAEllipseResult

            ' 参数验证
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("至少需要3个点来计算椭圆", NameOf(points))
            End If

            If confidenceLevel <= 0 OrElse confidenceLevel >= 1 Then
                Throw New ArgumentException("置信水平必须在0和1之间", NameOf(confidenceLevel))
            End If

            ' 步骤1：计算均值（椭圆中心）
            Dim meanX As Double = 0, meanY As Double = 0
            For Each pt As PointF In points
                meanX += pt.X
                meanY += pt.Y
            Next
            meanX /= points.Length
            meanY /= points.Length

            ' 步骤2：计算协方差矩阵
            Dim covXX As Double = 0, covYY As Double = 0, covXY As Double = 0
            For Each pt As PointF In points
                Dim dx As Double = pt.X - meanX
                Dim dy As Double = pt.Y - meanY
                covXX += dx * dx
                covYY += dy * dy
                covXY += dx * dy
            Next

            ' 使用无偏估计（除以n-1）
            Dim n As Integer = points.Length
            covXX /= (n - 1)
            covYY /= (n - 1)
            covXY /= (n - 1)

            ' 构建协方差矩阵
            Dim covarianceMatrix As Double(,) = {
            {covXX, covXY},
            {covXY, covYY}
        }

            ' 步骤3：对协方差矩阵进行特征分解
            Dim eigenDecomposition As EigenDecompositionResult = EigenDecompose(covarianceMatrix)

            ' 步骤4：根据椭圆类型计算缩放因子
            Dim scaleFactor As Double = CalculateScaleFactor(
            confidenceLevel,
            ellipseType,
            n,
            degreesOfFreedom
        )

            ' 步骤5：计算半轴长度
            Dim lambda1 As Double = eigenDecomposition.EigenValues(0) ' 较大特征值
            Dim lambda2 As Double = eigenDecomposition.EigenValues(1) ' 较小特征值

            ' 半轴长度 = sqrt(缩放因子 * 特征值)
            Dim semiMajorAxis As Double = std.Sqrt(scaleFactor * lambda1)
            Dim semiMinorAxis As Double = std.Sqrt(scaleFactor * lambda2)

            ' 步骤6：计算旋转角度（从较大特征值对应的特征向量方向）
            Dim majorAxisVector As Double() = eigenDecomposition.EigenVectors.GetColumn(0) ' 第一列是较大特征值对应的特征向量
            Dim rotationAngle As Double = std.Atan2(majorAxisVector(1), majorAxisVector(0))

            ' 规范化角度到[0, π)
            rotationAngle = NormalizeAngle(rotationAngle)

            ' 构建结果对象
            Return New PCAEllipseResult With {
            .Center = New PointF(CSng(meanX), CSng(meanY)),
            .SemiMajorAxis = CSng(semiMajorAxis),
            .SemiMinorAxis = CSng(semiMinorAxis),
            .RotationAngle = CSng(rotationAngle),
            .ConfidenceLevel = confidenceLevel,
            .CovarianceMatrix = covarianceMatrix,
            .EigenValues = eigenDecomposition.EigenValues,
            .EigenVectors = eigenDecomposition.EigenVectors
        }
        End Function

        ''' <summary>
        ''' 2x2对称矩阵的特征分解
        ''' </summary>
        Private Shared Function EigenDecompose(matrix As Double(,)) As EigenDecompositionResult
            ' 对于2x2对称矩阵，可以使用解析解
            Dim a As Double = matrix(0, 0)
            Dim b As Double = matrix(0, 1)
            Dim c As Double = matrix(1, 1)

            ' 计算特征值
            Dim trace As Double = a + c
            Dim det As Double = a * c - b * b
            Dim discriminant As Double = std.Sqrt(trace * trace - 4 * det)

            Dim lambda1 As Double = (trace + discriminant) / 2 ' 较大特征值
            Dim lambda2 As Double = (trace - discriminant) / 2 ' 较小特征值

            ' 计算特征向量
            Dim vec1 As Double() = Nothing, vec2 As Double() = Nothing

            If std.Abs(b) > 0.0000000001 Then
                ' b不为0的情况
                vec1 = New Double() {b, lambda1 - a}
                vec2 = New Double() {b, lambda2 - a}
            ElseIf std.Abs(lambda1 - a) > 0.0000000001 Then
                ' b为0，但lambda1不等于a的情况
                vec1 = New Double() {1, 0}
                vec2 = New Double() {0, 1}
            Else
                ' 特殊情况：对角矩阵
                vec1 = New Double() {1, 0}
                vec2 = New Double() {0, 1}
            End If

            ' 归一化特征向量
            vec1 = NormalizeVector(vec1)
            vec2 = NormalizeVector(vec2)

            Return New EigenDecompositionResult With {
            .EigenValues = New Double() {lambda1, lambda2},
            .EigenVectors = New Double(,) {
                {vec1(0), vec2(0)},
                {vec1(1), vec2(1)}
            }
        }
        End Function

        ''' <summary>
        ''' 计算缩放因子
        ''' </summary>
        Private Shared Function CalculateScaleFactor(
        confidenceLevel As Single,
        ellipseType As EllipseType,
        sampleSize As Integer,
        Optional degreesOfFreedom As Integer? = Nothing
    ) As Double

            Select Case ellipseType
                Case EllipseType.Normal
                    ' 正态分布：使用卡方分布分位数
                    Return ChiSquareQuantile(confidenceLevel, 2)

                Case EllipseType.T
                    ' t分布：使用F分布分位数
                    Dim df As Integer = If(degreesOfFreedom.HasValue, degreesOfFreedom.Value, sampleSize - 1)
                    Dim fQuantile As Double = PCAEllipseCalculator.FQuantile(confidenceLevel, 2, df)
                    Return 2 * (df - 1) / (df * (df - 2)) * fQuantile

                Case EllipseType.Euclidean
                    ' 欧几里得圆：基于半径
                    Return std.Sqrt(ChiSquareQuantile(confidenceLevel, 2))

                Case Else
                    Throw New ArgumentException($"不支持的椭圆类型: {ellipseType}")
            End Select
        End Function

        ''' <summary>
        ''' 卡方分布分位数近似计算
        ''' 使用Wilson-Hilferty近似
        ''' </summary>
        Private Shared Function ChiSquareQuantile(p As Double, df As Integer) As Double
            If p <= 0 OrElse p >= 1 Then
                Throw New ArgumentException("概率p必须在0和1之间", NameOf(p))
            End If

            ' Wilson-Hilferty近似
            Dim zp As Double = NormalQuantile(p)
            Dim chiSq As Double = df * std.Pow(1 - 2 / (9 * df) + zp * std.Sqrt(2 / (9 * df)), 3)

            Return chiSq
        End Function

        ''' <summary>
        ''' F分布分位数近似计算
        ''' </summary>
        Private Shared Function FQuantile(p As Double, df1 As Integer, df2 As Integer) As Double
            If p <= 0 OrElse p >= 1 Then
                Throw New ArgumentException("概率p必须在0和1之间", NameOf(p))
            End If

            ' 使用正态近似
            Dim zp As Double = NormalQuantile(p)
            Dim m As Double = df2 - 2
            If m <= 0 Then m = 1

            Dim fApprox As Double = df2 / df1 * (1 + 2 / df2 * (1 + std.Sqrt(1 + 4 / df2) * zp)) ^ 2

            Return std.Max(0.001, fApprox)
        End Function

        ''' <summary>
        ''' 标准正态分布分位数近似计算
        ''' 使用Beasley-Springer-Moro近似
        ''' </summary>
        Private Shared Function NormalQuantile(p As Double) As Double
            ' 对称性处理
            If p > 0.5 Then
                Return -NormalQuantile(1 - p)
            End If

            If p <= 0 Then Return Double.PositiveInfinity
            If p >= 1 Then Return Double.NegativeInfinity

            ' Beasley-Springer-Moro近似的常数
            Dim a As Double() = {-39.696830286653757, 220.9460984245205,
                              -275.92851044696869, 138.357751867269,
                              -30.66479806614716, 2.5066282774592392}

            Dim b As Double() = {-54.476098798224058, 161.58583685804089,
                              -155.69897985988661, 66.80131188771972,
                              -13.280681552885721}

            Dim c As Double() = {-0.0077848940024302926, -0.32239645804113648,
                              -2.4007582771618381, -2.5497325393437338,
                               4.3746641414649678, 2.9381639826987831}

            Dim d As Double() = {0.0077846957090414622, 0.32246712907003983,
                              2.445134137142996, 3.7544086619074162}

            Dim q As Double = std.Sqrt(-2 * std.Log(p))
            Dim x As Double

            ' 分段近似
            If p <= 0.02425 Then
                ' p很小时的近似
                Dim q1 As Double = 1 / ((c(0) * q + c(1)) * q + c(2))
                x = (((((d(0) * q + d(1)) * q + d(2)) * q + d(3)) * q + 1)) * q1
            ElseIf p <= 0.97575 Then
                ' 中间区域的近似
                Dim q1 As Double = ((c(3) * q + c(4)) * q + c(5))
                Dim q2 As Double = ((d(2) * q + d(3)) * q + 1)
                x = q1 / q2
            Else
                ' p接近1时的近似
                Dim q1 As Double = 1 / ((a(0) * q + a(1)) * q + a(2))
                Dim q2 As Double = (((((b(0) * q + b(1)) * q + b(2)) * q + b(3)) * q + b(4)) * q + 1)
                x = q1 * q2
            End If

            Return x
        End Function

        ''' <summary>
        ''' 向量归一化
        ''' </summary>
        Private Shared Function NormalizeVector(vec As Double()) As Double()
            Dim norm As Double = std.Sqrt(vec(0) * vec(0) + vec(1) * vec(1))
            If norm < 0.0000000001 Then
                Return New Double() {1, 0}
            End If

            Return New Double() {vec(0) / norm, vec(1) / norm}
        End Function

        ''' <summary>
        ''' 角度规范化到[0, π)
        ''' </summary>
        Private Shared Function NormalizeAngle(angle As Double) As Double
            Dim normalized As Double = angle Mod std.PI
            If normalized < 0 Then
                normalized += std.PI
            End If
            Return normalized
        End Function
    End Class

    ''' <summary>
    ''' 特征分解结果
    ''' </summary>
    Friend Class EigenDecompositionResult
        Public Property EigenValues As Double()
        Public Property EigenVectors As Double(,) ' 每列是一个特征向量
    End Class

End Namespace

