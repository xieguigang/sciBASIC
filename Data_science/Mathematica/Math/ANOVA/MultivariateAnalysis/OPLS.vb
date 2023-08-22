Imports System.Runtime.InteropServices
Imports std = System.Math

Public Class OPLS

    Private Shared Function GetPredictedYvariables(coeffVector As Double(), statObject As StatisticsObject, woPred As Double(,), poPred As Double(,)) As Double()
        Dim yPreds = New Double(statObject.RowSize() - 1) {}
        For i = 0 To statObject.RowSize() - 1

            ' x vector
            Dim xnew = New Double(statObject.ColumnSize() - 1) {}
            For j = 0 To statObject.ColumnSize() - 1
                xnew(j) = statObject.XScaled(i, j)
            Next

            xnew = convertToFilteredX(xnew, woPred, poPred)

            Dim s = 0.0
            For j = 0 To statObject.ColumnSize() - 1
                s += coeffVector(j) * xnew(j)
            Next
            yPreds(i) = statObject.YBackTransform(s)
        Next

        Return yPreds
    End Function

    Private Shared Function convertToFilteredX(xnew As Double(), woPred As Double(,), poPred As Double(,)) As Double()

        For j = 0 To woPred.GetLength(0) - 1
            Dim tneworth = 0.0
            Dim wo2 = 0.0
            For k = 0 To xnew.Length - 1
                tneworth += xnew(k) * woPred(j, k)
                wo2 += std.Pow(woPred(j, k), 2)
            Next
            tneworth /= wo2

            For k = 0 To xnew.Length - 1
                xnew(k) = xnew(k) - poPred(j, k) * tneworth
            Next
        Next

        Return xnew
    End Function

    Private Shared Sub OplsCrossValidation(yArray As Double(),
                                           dataArray As Double(,),
                                           maxLV As Integer,
                                           nFold As Integer,
                                           <Out> ByRef ss As List(Of Double),
                                           <Out> ByRef press As List(Of Double),
                                           <Out> ByRef total As List(Of Double),
                                           <Out> ByRef q2 As List(Of Double),
                                           <Out> ByRef q2cum As List(Of Double),
                                           <Out> ByRef orthofactor As Integer)

        ss = New List(Of Double)()
        press = New List(Of Double)()
        total = New List(Of Double)()
        q2 = New List(Of Double)()
        q2cum = New List(Of Double)()
        orthofactor = 1

        Dim w = PLS.getWeightLoading(yArray, dataArray) ' this w is fixed for opls modeling

        For i = 0 To maxLV - 1
            Dim currentSS = BasicMathematics.SumOfSquare(yArray)
            ss.Add(currentSS)
            Dim currentPress = PLS.PlsPressCalculation(nFold, dataArray, yArray) ' same in opls
            press.Add(currentPress)
            q2.Add(1 - press(i) / ss(i))
            If i = 0 Then
                total.Add(press(i) / ss(i))
                q2cum.Add(1 - press(i) / ss(i))
            Else
                total.Add(total(i - 1) * press(i) / ss(i))
                q2cum.Add(1 - total(i - 1) * press(i) / ss(i))
            End If

            If PLS.isOptimaized(yArray.Length, ss, press, total, q2, q2cum, orthofactor) Then
                orthofactor -= 1 ' because the optimal value includes the biological componemnt
                Exit For
            End If

            ' t: score vector calculation
            ' u: y score vector calculation
            ' p: loading vector calculation
            ' w: weight (X) factor calculation
            ' c: weight (Y) factor calculation
            ' to: score vector calculation
            ' po: loading vector calculation
            ' wo: weight (X) factor calculation

            Dim u, t, p, [to], po, wo As Double()
            Dim c As Double

            PLS.OplsVectorsCalculations(yArray, dataArray, w, u, t, c, p, wo, [to], po)
            dataArray = PLS.PlsMatrixUpdate([to], po, dataArray)
            yArray = PLS.PlsMatrixUpdate([to], c, yArray)
        Next
    End Sub

End Class
