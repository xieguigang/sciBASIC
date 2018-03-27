Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module FeatureProjection

    ''' <summary>
    ''' 利用多项式线性拟合将不等长的向量投影为指定维度的向量
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="dimension%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Project(points As IEnumerable(Of PointF), dimension%) As Vector
        With points.ToArray
            Dim fit = LeastSquares.PolyFit(.X, .Y, poly_n:=dimension)
            Dim projection As Vector = fit.Polynomial.Factors

            Return projection
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Project(vector As Vector, dimension%) As Vector
        Return vector _
            .Select(Function(d, i) New PointF(i, d)) _
            .Project(dimension)
    End Function
End Module
