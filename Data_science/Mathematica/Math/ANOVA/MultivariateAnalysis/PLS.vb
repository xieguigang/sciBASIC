#Region "Microsoft.VisualBasic::2a105b3287b5862ec94dc6ddcbc6a79b, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\PLS.vb"

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

    '   Total Lines: 693
    '    Code Lines: 534
    ' Comment Lines: 44
    '   Blank Lines: 115
    '     File Size: 24.84 KB


    ' Class PLS
    ' 
    '     Function: getOptimalFoldValue, GetPlsCoefficients, GetPredictedYvariables, GetVips, getWeightLoading
    '               isOptimaized, PartialLeastSquares, (+2 Overloads) PlsMatrixUpdate, (+2 Overloads) PlsPressCalculation
    ' 
    '     Sub: DivideMatrixToTrainTest, OplsVectorsCalculations, PlsCrossValidation, PlsModeling, PlsVectorsCalculations
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Public Class PLS

#Region "pls"

    ''' <summary>
    ''' PLS-DA analysis
    ''' </summary>
    ''' <param name="statObject"></param>
    ''' <param name="component"></param>
    ''' <returns></returns>
    Public Shared Function PartialLeastSquares(statObject As StatisticsObject, Optional component As Integer = -1) As MultivariateAnalysisResult
        Dim dataArray = statObject.CopyX()
        Dim yArray = statObject.CopyY()

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites
        Dim maxLV = rowSize
        Dim nFold = getOptimalFoldValue(rowSize)

        Dim ss = New List(Of Double)()
        Dim press = New List(Of Double)()
        Dim total = New List(Of Double)()
        Dim q2 = New List(Of Double)()
        Dim q2cum = New List(Of Double)()
        Dim optfactor = 1

        PlsCrossValidation(yArray, dataArray, maxLV, nFold, ss, press, total, q2, q2cum, optfactor)

        If component > 0 Then optfactor = component
        If optfactor < 2 Then optfactor = 2

        ' again, initialization
        dataArray = statObject.CopyX()
        yArray = statObject.CopyY()

        Dim ssPred, cPred As Double()
        Dim wPred, pPred, tPred, uPred As Double(,)

        PlsModeling(optfactor, yArray, dataArray, ssPred, wPred, pPred, cPred, tPred, uPred)

        ' calculate coefficients
        Dim coeffVector = GetPlsCoefficients(optfactor, pPred, wPred, cPred)

        ' calculate vips
        Dim vip = GetVips(wPred, ssPred)

        ' yPred
        Dim yPred = GetPredictedYvariables(coeffVector, statObject)
        Dim ySumofSqure = BasicMathematics.ErrorOfSquareVs2(statObject.YVariables, yPred)
        Dim rmsee = std.Sqrt(ySumofSqure / (rowSize - optfactor - 1))

        Dim maResult = New MultivariateAnalysisResult() With {
            .StatisticsObject = statObject,
            .NFold = nFold,
            .SsCVs = New ObservableCollection(Of Double)(ss),
            .Presses = New ObservableCollection(Of Double)(press),
            .Totals = New ObservableCollection(Of Double)(total),
            .Q2Values = New ObservableCollection(Of Double)(q2),
            .Q2Cums = New ObservableCollection(Of Double)(q2cum),
            .OptimizedFactor = optfactor,
            .SsPreds = New ObservableCollection(Of Double)(ssPred),
            .CPreds = New ObservableCollection(Of Double)(cPred),
            .Coefficients = New ObservableCollection(Of Double)(coeffVector),
            .Vips = New ObservableCollection(Of Double)(vip),
            .PredictedYs = New ObservableCollection(Of Double)(yPred),
            .Rmsee = rmsee,
            .analysis = GetType(PLS)
        }

        For i = 0 To optfactor - 1
            Dim wArray = New Double(columnSize - 1) {}
            Dim pArray = New Double(columnSize - 1) {}
            For j = 0 To columnSize - 1
                wArray(j) = wPred(i, j)
                pArray(j) = pPred(i, j)
            Next
            maResult.WPreds.Add(wArray)
            maResult.PPreds.Add(pArray)
        Next

        For i = 0 To optfactor - 1
            Dim uArray = New Double(rowSize - 1) {}
            Dim tArray = New Double(rowSize - 1) {}
            For j = 0 To rowSize - 1
                uArray(j) = uPred(i, j)
                tArray(j) = tPred(i, j)
            Next
            maResult.UPreds.Add(uArray)
            maResult.TPreds.Add(tArray)
        Next

        Return maResult
    End Function

    Friend Shared Function getOptimalFoldValue(rowSize As Integer) As Integer
        If rowSize < 7 Then
            Return rowSize
        Else
            Return 7
        End If
    End Function

    Private Shared Function GetPredictedYvariables(coeffVector As Double(), statObject As StatisticsObject) As Double()

        Dim yPreds = New Double(statObject.RowSize() - 1) {}
        For i = 0 To statObject.RowSize() - 1
            Dim s = 0.0
            For j = 0 To statObject.ColumnSize() - 1
                s += coeffVector(j) * statObject.XScaled(i, j)
            Next
            yPreds(i) = statObject.YBackTransform(s)
        Next

        Return yPreds
    End Function

    Public Shared Function GetVips(wPred As Double(,), ssPred As Double()) As Double()
        Dim optfactor = wPred.GetLength(0) ' optfactor
        Dim columnSize = wPred.GetLength(1) ' metabolites

        Dim vip = New Double(columnSize - 1) {}
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To optfactor - 1
                s += std.Pow(wPred(j, i), 2) * (ssPred(j) - ssPred(j + 1)) * columnSize / (ssPred(0) - ssPred(optfactor))
            Next
            vip(i) = std.Sqrt(s)
        Next
        Return vip
    End Function

    Public Shared Function GetPlsCoefficients(optfactor As Integer, pPred As Double(,), wPred As Double(,), cPred As Double()) As Double()
        Dim columnSize = pPred.GetLength(1) ' metabolites

        Dim coeffVector = New Double(columnSize - 1) {}
#Region ""

        Dim pwMatrix = New Double(optfactor - 1, optfactor - 1) {}
        For i = 0 To optfactor - 1
            For j = 0 To optfactor - 1
                Dim s = 0.0
                For k = 0 To columnSize - 1
                    s += pPred(i, k) * wPred(j, k)
                Next
                pwMatrix(i, j) = s
            Next
        Next

        Dim pwMatrixLU = New NumericMatrix(pwMatrix).LUD
        Dim pwMatrixInv = DirectCast(pwMatrixLU.Solve(NumericMatrix.Identity(pwMatrixLU.Pivot.Length)), NumericMatrix).Inverse

        Dim wStar = New Double(optfactor - 1, columnSize - 1) {}
        For i = 0 To optfactor - 1
            For j = 0 To columnSize - 1
                Dim s = 0.0
                For k = 0 To optfactor - 1
                    s += wPred(k, j) * pwMatrixInv(k, i)
                Next
                wStar(i, j) = s
            Next
        Next

        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To optfactor - 1
                s += wStar(j, i) * cPred(j)
            Next
            coeffVector(i) = s
        Next
#End Region

        Return coeffVector
    End Function

    Private Shared Sub PlsModeling(optfactor As Integer, yArray As Double(), dataArray As Double(,),
                                   <Out> ByRef ssPred As Double(),
                                   <Out> ByRef wPred As Double(,),
                                   <Out> ByRef pPred As Double(,),
                                   <Out> ByRef cPred As Double(),
                                   <Out> ByRef tPred As Double(,),
                                   <Out> ByRef uPred As Double(,))

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim ssPredList = New List(Of Double)()
        Dim cPredList = New List(Of Double)()
        Dim wPredList = New List(Of Double())()
        Dim pPredList = New List(Of Double())()
        Dim tPredList = New List(Of Double())()
        Dim uPredList = New List(Of Double())()

        Dim currentSS = BasicMathematics.SumOfSquare(yArray)
        ssPredList.Add(currentSS)

        ' modeling
#Region ""
        For i = 0 To optfactor - 1
            ' t: score vector calculation
            ' u: y score vector calculation
            ' p: loading vector calculation
            ' w: weight (X) factor calculation
            ' c: weight (Y) factor calculation

            Dim u, t, p, w As Double()
            Dim c As Double

            PlsVectorsCalculations(yArray, dataArray, u, w, t, c, p)
            dataArray = PlsMatrixUpdate(t, p, dataArray)
            yArray = PlsMatrixUpdate(t, c, yArray)

            wPredList.Add(w)
            pPredList.Add(p)
            cPredList.Add(c)
            tPredList.Add(t)
            uPredList.Add(u)

            currentSS = BasicMathematics.SumOfSquare(yArray)
            ssPredList.Add(currentSS)
        Next
#End Region

        ssPred = ssPredList.ToArray()
        cPred = cPredList.ToArray()
        wPred = New Double(optfactor - 1, columnSize - 1) {}
        pPred = New Double(optfactor - 1, columnSize - 1) {}
        For i = 0 To optfactor - 1
            For j = 0 To columnSize - 1
                wPred(i, j) = wPredList(i)(j)
                pPred(i, j) = pPredList(i)(j)
            Next
        Next

        tPred = New Double(optfactor - 1, rowSize - 1) {}
        uPred = New Double(optfactor - 1, rowSize - 1) {}
        For i = 0 To optfactor - 1
            For j = 0 To rowSize - 1
                tPred(i, j) = tPredList(i)(j)
                uPred(i, j) = uPredList(i)(j)
            Next
        Next
    End Sub

    Friend Shared Function getWeightLoading(yArray As Double(), dataArray As Double(,)) As Double()
        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim uScalar = 0.0
        Dim u = New Double(yArray.Length - 1) {}

        ' score initialize
        For i = 0 To yArray.Length - 1
            u(i) = yArray(i)
            uScalar += std.Pow(u(i), 2)
        Next

        Dim w = New Double(columnSize - 1) {} ' weight (X) factor calculation
#Region ""
        ' weight factor initialization
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To rowSize - 1
                s += dataArray(j, i) * u(j)
            Next
            w(i) = s / uScalar
        Next

        ' weight scalar
        Dim wScalar = BasicMathematics.RootSumOfSquare(w)

        ' weight vector
        For i = 0 To columnSize - 1
            w(i) = w(i) / wScalar
        Next
#End Region

        Return w
    End Function

    Public Shared Sub PlsCrossValidation(yArray As Double(),
                                         dataArray As Double(,),
                                         maxLV As Integer,
                                         nFold As Integer,
                                         <Out> ByRef ss As List(Of Double),
                                         <Out> ByRef press As List(Of Double),
                                         <Out> ByRef total As List(Of Double),
                                         <Out> ByRef q2 As List(Of Double),
                                         <Out> ByRef q2cum As List(Of Double),
                                         <Out> ByRef optfactor As Integer)
        ss = New List(Of Double)()
        press = New List(Of Double)()
        total = New List(Of Double)()
        q2 = New List(Of Double)()
        q2cum = New List(Of Double)()
        optfactor = 1

        For i = 0 To maxLV - 1
            Dim currentSS = BasicMathematics.SumOfSquare(yArray)
            ss.Add(currentSS)
            Dim currentPress = PlsPressCalculation(nFold, dataArray, yArray)
            press.Add(currentPress)
            q2.Add(1 - press(i) / ss(i))
            If i = 0 Then
                total.Add(press(i) / ss(i))
                q2cum.Add(1 - press(i) / ss(i))
            Else
                total.Add(total(i - 1) * press(i) / ss(i))
                q2cum.Add(1 - total(i - 1) * press(i) / ss(i))
            End If

            If isOptimaized(yArray.Length, ss, press, total, q2, q2cum, optfactor) Then
                Exit For
            End If

            ' t: score vector calculation
            ' u: y score vector calculation
            ' p: loading vector calculation
            ' w: weight (X) factor calculation
            ' c: weight (Y) factor calculation

            Dim u, t, p, w As Double()
            Dim c As Double

            PlsVectorsCalculations(yArray, dataArray, u, w, t, c, p)
            dataArray = PlsMatrixUpdate(t, p, dataArray)
            yArray = PlsMatrixUpdate(t, c, yArray)
        Next
    End Sub

    Friend Shared Function isOptimaized(size As Integer,
                                        ss As List(Of Double),
                                        press As List(Of Double),
                                        total As List(Of Double),
                                        q2 As List(Of Double),
                                        q2cum As List(Of Double),
                                        <Out>
                                        ByRef optfactor As Integer) As Boolean

        Dim latestQ2cum = q2cum(q2cum.Count - 1)
        Dim latestQ2 = q2(q2.Count - 1)

        ' rule 1
        If size > 100 Then
            If latestQ2cum <= 0.0 Then
                optfactor = q2.Count - 1
                If optfactor = 0 Then Return True
                ss.RemoveAt(ss.Count - 1)
                press.RemoveAt(press.Count - 1)
                total.RemoveAt(total.Count - 1)
                q2.RemoveAt(q2.Count - 1)
                q2cum.RemoveAt(q2cum.Count - 1)

                Return True
            End If
        Else
            If latestQ2cum <= 0.05 Then
                optfactor = q2.Count - 1
                If optfactor = 0 Then Return True
                ss.RemoveAt(ss.Count - 1)
                press.RemoveAt(press.Count - 1)
                total.RemoveAt(total.Count - 1)
                q2.RemoveAt(q2.Count - 1)
                q2cum.RemoveAt(q2cum.Count - 1)

                Return True
            End If
        End If

        ' rule 2
        Dim limit = 0.0
        If size >= 25 Then
            limit = size * 0.2 * 0.01
        Else
            limit = std.Sqrt(size) * 0.01
        End If

        If latestQ2 < limit Then
            optfactor = q2.Count - 1
            If optfactor = 0 Then Return True
            ss.RemoveAt(ss.Count - 1)
            press.RemoveAt(press.Count - 1)
            total.RemoveAt(total.Count - 1)
            q2.RemoveAt(q2.Count - 1)
            q2cum.RemoveAt(q2cum.Count - 1)

            Return True
        End If

        ' rule N4
        Dim explained = total(total.Count - 1) / total(0)
        If explained < 0.01 Then
            optfactor = q2.Count - 1
            If optfactor = 0 Then Return True
            ss.RemoveAt(ss.Count - 1)
            press.RemoveAt(press.Count - 1)
            total.RemoveAt(total.Count - 1)
            q2.RemoveAt(q2.Count - 1)
            q2cum.RemoveAt(q2cum.Count - 1)

            Return True
        End If

        ' rule N5 defined by Hiroshi
        If q2cum.Count > 1 Then
            Dim diff = std.Abs(q2cum(q2cum.Count - 1) - q2cum(q2cum.Count - 2)) * 100
            If diff < 2 Then
                optfactor = q2.Count - 1
                Return True
            End If
        End If
        optfactor = q2.Count
        Return False
    End Function

    Friend Shared Function PlsMatrixUpdate(t As Double(), p As Double(), dataArray As Double(,)) As Double(,)
        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim nArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                nArray(i, j) = dataArray(i, j) - t(i) * p(j)
            Next
        Next
        Return nArray
    End Function

    Friend Shared Function PlsMatrixUpdate(t As Double(), c As Double, yArray As Double()) As Double()
        Dim size = yArray.Length
        Dim nArray = New Double(size - 1) {}

        For i = 0 To size - 1
            nArray(i) = yArray(i) - t(i) * c
        Next
        Return nArray
    End Function

    Friend Shared Sub OplsVectorsCalculations(yArray As Double(),
                                               dataArray As Double(,),
                                               w As Double(),
                                               <Out> ByRef u As Double(),
                                               <Out> ByRef t As Double(),
                                               <Out> ByRef c As Double,
                                               <Out> ByRef p As Double(),
                                               <Out> ByRef wo As Double(),
                                               <Out> ByRef [to] As Double(),
                                               <Out> ByRef po As Double())

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        u = New Double(rowSize - 1) {}
        'w = new double[columnSize];
        t = New Double(rowSize - 1) {}
        c = 0.0
        p = New Double(columnSize - 1) {}

        'PlsVectorsCalculations(yArray, dataArray, out u, out w, out t, out c, out p);
        t = New Double(rowSize - 1) {} ' score vector calculation
#Region ""
        ' score initialization
        For i = 0 To rowSize - 1
            Dim s = 0.0
            For j = 0 To columnSize - 1
                s += dataArray(i, j) * w(j)
            Next
            t(i) = s
        Next

#End Region

        ' score scalar
        Dim tScalar = BasicMathematics.SumOfSquare(t)

        ' Debug.WriteLine("Y array" & ASCII.TAB & String.Join(ASCII.TAB, yArray))
        ' Debug.WriteLine("T array" & ASCII.TAB & String.Join(ASCII.TAB, t))

        c = BasicMathematics.InnerProduct(yArray, t) / tScalar ' weight (Y) factor calculation
        p = New Double(columnSize - 1) {} ' loading vector calculation

#Region ""
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To rowSize - 1
                s += dataArray(j, i) * t(j)
            Next
            p(i) = s / tScalar
        Next
#End Region
        For i = 0 To rowSize - 1
            u(i) = yArray(i) / c
        Next

        ' wo calc
        wo = New Double(columnSize - 1) {}
        Dim wp = BasicMathematics.InnerProduct(w, p)
        Dim w2 = BasicMathematics.SumOfSquare(w)
        For i = 0 To columnSize - 1
            wo(i) = p(i) - wp / w2 * w(i)
        Next

        Dim wonorm = BasicMathematics.RootSumOfSquare(wo)
        For i = 0 To columnSize - 1
            wo(i) /= wonorm
        Next


        ' score initialization
        [to] = New Double(rowSize - 1) {}
        For i = 0 To rowSize - 1
            Dim s = 0.0
            For j = 0 To columnSize - 1
                s += dataArray(i, j) * wo(j)
            Next
            [to](i) = s
        Next

        ' score scalar
        Dim toScalar = BasicMathematics.SumOfSquare([to])

        po = New Double(columnSize - 1) {} ' loading vector calculation

#Region ""
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To rowSize - 1
                s += dataArray(j, i) * [to](j)
            Next
            po(i) = s / toScalar
        Next
#End Region
    End Sub

    Public Shared Sub PlsVectorsCalculations(yArray As Double(), dataArray As Double(,), <Out> ByRef u As Double(), <Out> ByRef w As Double(), <Out> ByRef t As Double(), <Out> ByRef c As Double, <Out> ByRef p As Double())

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim uScalar = 0.0
        u = New Double(yArray.Length - 1) {}

        ' score initialize
        For i = 0 To yArray.Length - 1
            u(i) = yArray(i)
            uScalar += std.Pow(u(i), 2)
        Next

        w = New Double(columnSize - 1) {} ' weight (X) factor calculation
#Region ""
        ' weight factor initialization
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To rowSize - 1
                s += dataArray(j, i) * u(j)
            Next
            w(i) = s / uScalar
        Next

        ' weight scalar
        Dim wScalar = BasicMathematics.RootSumOfSquare(w)

        ' weight vector
        For i = 0 To columnSize - 1
            w(i) = w(i) / wScalar
        Next
#End Region

        t = New Double(rowSize - 1) {} ' score vector calculation
#Region ""
        ' score initialization
        For i = 0 To rowSize - 1
            Dim s = 0.0
            For j = 0 To columnSize - 1
                s += dataArray(i, j) * w(j)
            Next
            t(i) = s
        Next

#End Region

        ' score scalar
        Dim tScalar = BasicMathematics.SumOfSquare(t)

        c = BasicMathematics.InnerProduct(yArray, t) / tScalar ' weight (Y) factor calculation
        p = New Double(columnSize - 1) {} ' loading vector calculation

#Region ""
        For i = 0 To columnSize - 1
            Dim s = 0.0
            For j = 0 To rowSize - 1
                s += dataArray(j, i) * t(j)
            Next
            p(i) = s / tScalar
        Next
#End Region
    End Sub


    Public Shared Function PlsPressCalculation(cvNumber As Integer, cvFold As Integer, dataArray As Double(,), yArray As Double()) As Double

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites

        Dim trainMatrix, testMatrix As Double(,)
        Dim trainYvalues, testYvalues As Double()
        DivideMatrixToTrainTest(yArray, dataArray, cvFold, cvNumber, trainMatrix, testMatrix, trainYvalues, testYvalues)

        Dim uTrain, tTrain, pTrain, wTrain As Double()
        Dim cTrain As Double

        PlsVectorsCalculations(trainYvalues, trainMatrix, uTrain, wTrain, tTrain, cTrain, pTrain)

        Dim testSize = testYvalues.Length
        Dim tTest = New Double(testSize - 1) {}
        For i = 0 To testSize - 1
            Dim s = 0.0
            For j = 0 To columnSize - 1
                s += testMatrix(i, j) * wTrain(j)
            Next
            tTest(i) = s
        Next

        Dim yPred = New Double(testSize - 1) {}
        For i = 0 To testSize - 1
            yPred(i) = cTrain * tTest(i)
        Next
        Dim press = BasicMathematics.ErrorOfSquareVs2(testYvalues, yPred)
        Return press
    End Function

    Public Shared Function PlsPressCalculation(cvFold As Integer, dataArray As Double(,), yArray As Double()) As Double

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites
        Dim press = 0.0
        For i = 0 To cvFold - 1
            press += PlsPressCalculation(i, cvFold, dataArray, yArray)
        Next

        Return press
    End Function

    Public Shared Sub DivideMatrixToTrainTest(yArray As Double(),
                                              dataArray As Double(,),
                                              cvFold As Integer,
                                              cvNumber As Integer,
                                              <Out> ByRef trainMatrix As Double(,),
                                              <Out> ByRef testMatrix As Double(,),
                                              <Out> ByRef trainYvalues As Double(),
                                              <Out> ByRef testYvalues As Double())

        Dim rowSize = dataArray.GetLength(0) ' files
        Dim columnSize = dataArray.GetLength(1) ' metabolites
        Dim testSize = 0

        For i = 0 To rowSize - 1
            Dim isTestData = If(i Mod cvFold = cvNumber, True, False)
            If isTestData Then testSize += 1
        Next

        trainMatrix = New Double(rowSize - testSize - 1, columnSize - 1) {}
        testMatrix = New Double(testSize - 1, columnSize - 1) {}

        trainYvalues = New Double(rowSize - testSize - 1) {}
        testYvalues = New Double(testSize - 1) {}

        Dim trainCounter = 0
        Dim testCounter = 0

        For i = 0 To rowSize - 1
            Dim isTestData = If(i Mod cvFold = cvNumber, True, False)
            If isTestData Then
                For j = 0 To columnSize - 1
                    testMatrix(testCounter, j) = dataArray(i, j)
                Next
                testYvalues(testCounter) = yArray(i)
                testCounter += 1
            Else
                For j = 0 To columnSize - 1
                    trainMatrix(trainCounter, j) = dataArray(i, j)
                Next
                trainYvalues(trainCounter) = yArray(i)
                trainCounter += 1
            End If
        Next
    End Sub
#End Region
End Class
