Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module DoubleLinear

    <Extension>
    Public Function GetInputPoints(bestfit As IFitted) As PointF()
        Return bestfit.ErrorTest _
            .Select(Function(p) New PointF(p.X, p.Y)) _
            .ToArray
    End Function

    <Extension>
    Public Function AutoPointDeletion(points As IEnumerable(Of PointF),
                                      Optional weighted As Boolean = False,
                                      Optional max As Integer = -1) As IFitted

        Dim pointVec As PointF() = points.ToArray

        If max <= 0 Then
            max = pointVec.Length / 2 - 1
        End If
        If max <= 0 Then
            ' can not delete any more points
            Return pointVec.LinearRegression(weighted)
        End If

        ' evaluate R2 for each point removes
        Dim ref As Vector = pointVec.X
        Dim measure As Vector = pointVec.Y
        Dim R2 As Double = -9999
        Dim bestfit As IFitted
        Dim model As IFitted

        model = LinearRegression(measure, ref, weighted)
        bestfit = model

        If Not model Is Nothing AndAlso model.CorrelationCoefficient > R2 Then
            R2 = model.CorrelationCoefficient
            bestfit = model

            If R2 > 0.999 Then
                Return bestfit
            End If
        End If

        For p As Integer = 1 To max
            ' 循环删除一个点，取R2最大的
            Dim X, Y As Vector
            Dim RMax As Double = -9999
            Dim modelBest As IFitted = Nothing
            Dim bestX As Vector = Nothing
            Dim bestY As Vector = Nothing

            For i As Integer = 0 To measure.Length - 1
                X = measure.Delete({i})
                Y = ref.Delete({i})
                model = LinearRegression(X, Y, weighted)

                If Not model Is Nothing AndAlso model.CorrelationCoefficient > RMax Then
                    RMax = model.CorrelationCoefficient
                    modelBest = model
                    bestX = X
                    bestY = Y
                End If
            Next

            If RMax > R2 Then
                R2 = RMax
                bestfit = modelBest
                measure = bestX
                ref = bestY

                If R2 > 0.999 Then
                    Return bestfit
                End If
            End If
        Next

        Return bestfit
    End Function
End Module
