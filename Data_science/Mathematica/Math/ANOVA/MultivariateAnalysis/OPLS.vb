#Region "Microsoft.VisualBasic::983991e36624fea88dd926589289f132, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\OPLS.vb"

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

    '   Total Lines: 370
    '    Code Lines: 274
    ' Comment Lines: 37
    '   Blank Lines: 59
    '     File Size: 14.89 KB


    ' Class OPLS
    ' 
    '     Function: convertToFilteredX, GetPredictedYvariables, GetStdevOfFilteredXArray, OrthogonalProjectionsToLatentStructures
    ' 
    '     Sub: OplsCrossValidation, OplsModeling
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports std = System.Math

Public Class OPLS

    ''' <summary>
    ''' Do OPLS-DA analysis
    ''' </summary>
    ''' <param name="statObject"></param>
    ''' <param name="component"></param>
    ''' <returns></returns>
    Public Shared Function OrthogonalProjectionsToLatentStructures(statObject As StatisticsObject, Optional component As Integer = -1) As MultivariateAnalysisResult
        Dim dataArray = statObject.CopyX()
        Dim yArray = statObject.CopyY()

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites
        Dim maxLV = rowSize
        Dim nFold = PLS.getOptimalFoldValue(rowSize)

        Dim ss = New List(Of Double)()
        Dim press = New List(Of Double)()
        Dim total = New List(Of Double)()
        Dim q2 = New List(Of Double)()
        Dim q2cum = New List(Of Double)()
        Dim biofactor = 1 ' must be 1
        Dim orthofactor = 1 ' should be optimized
        OplsCrossValidation(yArray, dataArray, maxLV, nFold, ss, press, total, q2, q2cum, orthofactor)

        If component > 0 Then orthofactor = component
        If orthofactor > rowSize * 0.5 - 1 Then orthofactor = CInt(rowSize * 0.5 - 1)
        If orthofactor < 1 Then orthofactor = 1

        ' again, initialization
        dataArray = statObject.CopyX()
        yArray = statObject.CopyY()

        Dim ssPred, cPred As Double()
        Dim wPred, pPred, tPred, uPred, woPred, poPred, toPred, filteredArray As Double(,)

        OplsModeling(biofactor, orthofactor, yArray, dataArray, ssPred, wPred, pPred, cPred, tPred, uPred, woPred, poPred, toPred, filteredArray)

        'for (int i = 0; i < columnSize; i++) {
        '    Debug.WriteLine("Loadings\t" + wPred[0, i] + "\t" + pPred[0, i]);
        '}

        ' calculate coefficients
        Dim coeffVector = PLS.GetPlsCoefficients(biofactor, pPred, wPred, cPred)

        ' calculate vips
        Dim vip = PLS.GetVips(wPred, ssPred)

        ' calculate stdev of filtered x array
        Dim stdevFilteredXs = GetStdevOfFilteredXArray(filteredArray)

        ' yPred
        Dim yPred = GetPredictedYvariables(coeffVector, statObject, woPred, poPred)
        Dim ySumofSqure = BasicMathematics.ErrorOfSquareVs2(statObject.YVariables, yPred)
        Dim rmsee = std.Sqrt(ySumofSqure / (rowSize - biofactor - 1))

        Dim plsresult = New MultivariateAnalysisResult() With {
            .StatisticsObject = statObject,
            .analysis = GetType(OPLS),
            .NFold = nFold,
            .SsCVs = New ObservableCollection(Of Double)(ss),
            .Presses = New ObservableCollection(Of Double)(press),
            .Totals = New ObservableCollection(Of Double)(total),
            .Q2Values = New ObservableCollection(Of Double)(q2),
            .Q2Cums = New ObservableCollection(Of Double)(q2cum),
            .OptimizedFactor = biofactor,
            .OptimizedOrthoFactor = orthofactor,
            .SsPreds = New ObservableCollection(Of Double)(ssPred),
            .CPreds = New ObservableCollection(Of Double)(cPred),
            .Coefficients = New ObservableCollection(Of Double)(coeffVector),
            .Vips = New ObservableCollection(Of Double)(vip),
            .PredictedYs = New ObservableCollection(Of Double)(yPred),
            .Rmsee = rmsee,
            .StdevFilteredXs = New ObservableCollection(Of Double)(stdevFilteredXs),
            .FilteredXArray = filteredArray
        }

        For i = 0 To orthofactor - 1
            Dim woArray = New Double(columnSize - 1) {}
            Dim poArray = New Double(columnSize - 1) {}
            For j = 0 To columnSize - 1
                woArray(j) = woPred(i, j)
                poArray(j) = poPred(i, j)
            Next
            plsresult.WoPreds.Add(woArray)
            plsresult.PoPreds.Add(poArray)
        Next

        For i = 0 To orthofactor - 1
            Dim toArray = New Double(rowSize - 1) {}
            For j = 0 To rowSize - 1
                toArray(j) = toPred(i, j)
            Next
            plsresult.ToPreds.Add(toArray)
        Next

        For i = 0 To biofactor - 1
            Dim uArray = New Double(rowSize - 1) {}
            Dim tArray = New Double(rowSize - 1) {}
            For j = 0 To rowSize - 1
                uArray(j) = uPred(i, j)
                tArray(j) = tPred(i, j)
            Next
            plsresult.UPreds.Add(uArray)
            plsresult.TPreds.Add(tArray)

            If i = 0 Then
                plsresult.stdevT = BasicMathematics.Stdev(tArray)
            End If
        Next

        For i = 0 To biofactor - 1
            Dim wArray = New Double(columnSize - 1) {}
            Dim pArray = New Double(columnSize - 1) {}
            Dim pcoeffArray = New Double(columnSize - 1) {}
            Dim pcovArray = New Double(columnSize - 1) {}
            For j = 0 To columnSize - 1
                wArray(j) = wPred(i, j)
                pArray(j) = pPred(i, j)

                For k = 0 To rowSize - 1
                    pcovArray(j) += tPred(i, k) * filteredArray(k, j) / (rowSize - 1)
                Next

                'pcoeffArray[j] = pArray[j] / plsresult.StdevFilteredXs[j] / plsresult.stdevT;
                pcoeffArray(j) = pcovArray(j) / plsresult.StdevFilteredXs(j) / plsresult.stdevT
            Next
            plsresult.WPreds.Add(wArray)
            plsresult.PPreds.Add(pArray)
            plsresult.PPredCoeffs.Add(pcoeffArray)
            plsresult.PPredCovs.Add(pcovArray)
        Next

        Return plsresult
    End Function

    Private Shared Function GetStdevOfFilteredXArray(dataArray As Double(,)) As Double()
        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim stdevs = New Double(columnSize - 1) {}
        For i = 0 To columnSize - 1
            Dim xArray = New Double(rowSize - 1) {}
            For j = 0 To rowSize - 1
                xArray(j) = dataArray(j, i)
            Next
            stdevs(i) = BasicMathematics.Stdev(xArray)
        Next

        Return stdevs
    End Function

    Public Shared Sub OplsModeling(biofactor As Integer, orthofactor As Integer, yArray As Double(), dataArray As Double(,),
                                   <Out> ByRef ssPred As Double(),
                                   <Out> ByRef wPred As Double(,),
                                   <Out> ByRef pPred As Double(,),
                                   <Out> ByRef cPred As Double(),
                                   <Out> ByRef tPred As Double(,),
                                   <Out> ByRef uPred As Double(,),
                                   <Out> ByRef woPred As Double(,),
                                   <Out> ByRef poPred As Double(,),
                                   <Out> ByRef toPred As Double(,),
                                   <Out> ByRef filteredXMatrix As Double(,))

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim ssPredList = New List(Of Double)()
        Dim cPredList = New List(Of Double)()
        Dim wPredList = New List(Of Double())()
        Dim pPredList = New List(Of Double())()
        Dim tPredList = New List(Of Double())()
        Dim uPredList = New List(Of Double())()
        Dim woPredList = New List(Of Double())()
        Dim poPredList = New List(Of Double())()
        Dim toPredList = New List(Of Double())()

        Dim originalY = New Double(yArray.Length - 1) {}
        For i = 0 To yArray.Length - 1
            originalY(i) = yArray(i)
        Next

        Dim currentSS = BasicMathematics.SumOfSquare(yArray)
        ssPredList.Add(currentSS)

        Dim wfirst = PLS.getWeightLoading(yArray, dataArray) ' to keep weight loading for the first trial of pls
        ' modeling
#Region ""
        For i = 0 To orthofactor - 1

            ' t: score vector calculation
            ' u: y score vector calculation
            ' p: loading vector calculation
            ' w: weight (X) factor calculation
            ' c: weight (Y) factor calculation
            ' to: score vector calculation
            ' po: loading vector calculation
            ' wo: weight (X) factor calculation

            Dim u, t, p, w, [to], po, wo As Double()
            Dim c As Double

            PLS.OplsVectorsCalculations(yArray, dataArray, wfirst, u, t, c, p, wo, [to], po)
            dataArray = PLS.PlsMatrixUpdate([to], po, dataArray)
            yArray = PLS.PlsMatrixUpdate([to], c, yArray)
            'Debug.WriteLine("C value\t" + c);
            woPredList.Add(wo)
            poPredList.Add(po)
            toPredList.Add([to])
        Next
#End Region

        ' save filtered x array
        filteredXMatrix = dataArray

        ' ss value calc
        currentSS = BasicMathematics.SumOfSquare(yArray)
        ssPredList.Add(currentSS)

        ' finally, pls modeling is performed to the filtered matrix by othrogonal components
        Dim uf, tf, pf, wf, tof, pof, wof As Double()
        Dim cf As Double
        PLS.OplsVectorsCalculations(yArray, dataArray, wfirst, uf, tf, cf, pf, wof, tof, pof)
        wPredList.Add(wfirst)
        pPredList.Add(pf)
        cPredList.Add(cf)
        tPredList.Add(tf)
        uPredList.Add(uf)

        ssPred = ssPredList.ToArray()
        cPred = cPredList.ToArray()
        wPred = New Double(biofactor - 1, columnSize - 1) {}
        pPred = New Double(biofactor - 1, columnSize - 1) {}
        For i = 0 To biofactor - 1
            For j = 0 To columnSize - 1
                wPred(i, j) = wPredList(i)(j)
                pPred(i, j) = pPredList(i)(j)
            Next
        Next

        tPred = New Double(biofactor - 1, rowSize - 1) {}
        uPred = New Double(biofactor - 1, rowSize - 1) {}
        For i = 0 To biofactor - 1
            For j = 0 To rowSize - 1
                tPred(i, j) = tPredList(i)(j)
                uPred(i, j) = uPredList(i)(j)
            Next
        Next

        woPred = New Double(orthofactor - 1, columnSize - 1) {}
        poPred = New Double(orthofactor - 1, columnSize - 1) {}
        For i = 0 To orthofactor - 1
            For j = 0 To columnSize - 1
                woPred(i, j) = woPredList(i)(j)
                poPred(i, j) = poPredList(i)(j)
            Next
        Next

        toPred = New Double(orthofactor - 1, rowSize - 1) {}
        For i = 0 To orthofactor - 1
            For j = 0 To rowSize - 1
                toPred(i, j) = toPredList(i)(j)
            Next
        Next
    End Sub

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
