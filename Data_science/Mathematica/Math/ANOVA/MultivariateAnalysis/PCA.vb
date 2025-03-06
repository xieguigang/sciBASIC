#Region "Microsoft.VisualBasic::68a213f095ee28c5ecaf294463f90e69, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\PCA.vb"

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

    '   Total Lines: 233
    '    Code Lines: 159 (68.24%)
    ' Comment Lines: 29 (12.45%)
    '    - Xml Docs: 24.14%
    ' 
    '   Blank Lines: 45 (19.31%)
    '     File Size: 7.86 KB


    ' Module PCA
    ' 
    '     Function: CalculateComponent, PrincipalComponentAnalysis
    '     Class LoadingTask
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Solve
    ' 
    '     Class ScoreTask
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Solve
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Public Module PCA

    ''' <summary>
    ''' PCA analysis
    ''' </summary>
    ''' <param name="statObject"></param>
    ''' <param name="maxPC"></param>
    ''' <param name="cutoff"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PrincipalComponentAnalysis(statObject As StatisticsObject,
                                               Optional maxPC As Integer = 5,
                                               Optional cutoff As Double = 0.0000001) As MultivariateAnalysisResult
        Dim dataArray = statObject.XScaled
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        If rowSize < maxPC Then
            maxPC = rowSize
        End If

        'PrincipalComponentAnalysisResult pcaBean = new PrincipalComponentAnalysisResult() {
        '    ScoreIdCollection = statObject.YIndexes,
        '    LoadingIdCollection = statObject.XIndexes,
        '    ScoreLabelCollection = statObject.YLabels,
        '    LoadingLabelCollecction = statObject.XLabels,
        '    ScoreBrushCollection = statObject.YColors,
        '    LoadingBrushCollection = statObject.XColors
        '};

        Dim tpMatrix = New Double(rowSize - 1, columnSize - 1) {}
        Dim contributions = New ObservableCollection(Of Double)()
        Dim scores = New ObservableCollection(Of Double())()
        Dim loadings = New ObservableCollection(Of Double())()
        Dim t0 As Date
        Dim t1 As Date
        Dim ticks As Integer

        For i As Integer = 0 To maxPC - 1
            Call VBDebugger.EchoLine($"Calculate component {i + 1}...")

            t0 = Now
            ticks = dataArray.CalculateComponent(i, columnSize, rowSize, cutoff, tpMatrix, contributions, scores, loadings)
            t1 = Now

            Call VBDebugger.EchoLine($"Cost {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)} and run {ticks} loop")
        Next

        Dim maResult = New MultivariateAnalysisResult() With {
            .StatisticsObject = statObject,
            .analysis = GetType(PCA),
            .NFold = 0,
            .Contributions = contributions,
            .TPreds = scores,
            .PPreds = loadings
        }

        Return maResult
    End Function

    <Extension>
    Private Function CalculateComponent(ByRef dataArray As Double(,),
                                        i As Integer,
                                        columnSize As Integer,
                                        rowSize As Integer,
                                        cutoff As Double,
                                        ByRef tpMatrix As Double(,),
                                        ByRef contributions As ObservableCollection(Of Double),
                                        ByRef scores As ObservableCollection(Of Double()),
                                        ByRef loadings As ObservableCollection(Of Double())) As Integer

        Dim mean = New Double(columnSize - 1) {}
        Dim var = New Double(columnSize - 1) {}
        Dim scoreOld = New Double(rowSize - 1) {}
        Dim scoreNew = New Double(rowSize - 1) {}
        Dim loading = New Double(columnSize - 1) {}
        Dim sum As Double
        Dim contributionOriginal As Double = 1

        For j = 0 To columnSize - 1
            sum = 0
            For k = 0 To rowSize - 1
                sum += dataArray(k, j)
            Next
            mean(j) = sum / rowSize
        Next

        For j = 0 To columnSize - 1
            sum = 0
            For k = 0 To rowSize - 1
                sum += std.Pow(dataArray(k, j) - mean(j), 2)
            Next
            var(j) = sum / (rowSize - 1)
        Next

        If i = 0 Then
            contributionOriginal = var.Sum
        End If

        Dim maxVar = var.Max
        Dim maxVarID = Array.IndexOf(var, maxVar)

        For j = 0 To rowSize - 1
            scoreOld(j) = dataArray(j, maxVarID)
        Next

        Dim threshold = Double.MaxValue
        Dim loadingVector As New LoadingTask(columnSize, rowSize) With {
            .dataArray = dataArray,
            .loading = loading,
            .scoreOld = scoreOld
        }
        Dim scoreVector As New ScoreTask(rowSize, columnSize) With {
            .dataArray = dataArray,
            .loading = loading,
            .scoreNew = scoreNew
        }

        i = 0

        While threshold > cutoff
            Dim scoreScalar = BasicMathematics.SumOfSquare(scoreOld)

            loadingVector.scoreScalar = scoreScalar
            loadingVector.Run()

            'For j = 0 To columnSize - 1
            '    sum = 0
            '    For k = 0 To rowSize - 1
            '        sum += dataArray(k, j) * scoreOld(k)
            '    Next
            '    loading(j) = sum / scoreScalar
            'Next

            Dim loadingScalar = BasicMathematics.RootSumOfSquare(loading)

            For j = 0 To columnSize - 1
                loading(j) = loading(j) / loadingScalar
            Next

            Call scoreVector.Run()

            'For j = 0 To rowSize - 1
            '    sum = 0
            '    For k = 0 To columnSize - 1
            '        sum += dataArray(j, k) * loading(k)
            '    Next
            '    scoreNew(j) = sum
            'Next

            threshold = BasicMathematics.RootSumOfSquare(scoreNew, scoreOld)

            For j = 0 To scoreNew.Length - 1
                scoreOld(j) = scoreNew(j)
            Next

            i += 1
        End While

        For j = 0 To columnSize - 1
            For k = 0 To rowSize - 1
                tpMatrix(k, j) = scoreNew(k) * loading(j)
                dataArray(k, j) = dataArray(k, j) - tpMatrix(k, j)
            Next
        Next

        Dim scoreVar = BasicMathematics.Var(scoreNew)

        contributions.Add(scoreVar / contributionOriginal * 100)
        scores.Add(scoreNew)
        loadings.Add(loading)

        Return i
    End Function

    Private Class LoadingTask : Inherits VectorTask

        ReadOnly rowSize As Integer

        Public loading As Double()
        Public dataArray As Double(,)
        Public scoreScalar As Double
        Public scoreOld As Double()

        Sub New(columnSize As Integer, rowSize As Integer)
            Call MyBase.New(columnSize)
            Me.rowSize = rowSize
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim sum As Double

            For j As Integer = start To ends
                sum = 0
                For k = 0 To rowSize - 1
                    sum += dataArray(k, j) * scoreOld(k)
                Next
                loading(j) = sum / scoreScalar
            Next
        End Sub
    End Class

    Private Class ScoreTask : Inherits VectorTask

        ReadOnly columnSize As Integer

        Public dataArray As Double(,)
        Public loading As Double()
        Public scoreNew As Double()

        Sub New(rowSize As Integer, columnSize As Integer)
            Call MyBase.New(rowSize)
            Me.columnSize = columnSize
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim sum As Double

            For j As Integer = start To ends
                sum = 0
                For k = 0 To columnSize - 1
                    sum += dataArray(j, k) * loading(k)
                Next
                scoreNew(j) = sum
            Next
        End Sub
    End Class
End Module
