Imports System.Collections.ObjectModel
Imports System.Runtime.CompilerServices
Imports std = System.Math

Public Module PCA

    <Extension>
    Public Function PrincipalComponentAnalysis(statObject As StatisticsObject, Optional maxPC As Integer = 5) As MultivariateAnalysisResult
        Dim dataArray = statObject.XScaled
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)
        If rowSize < maxPC Then maxPC = rowSize

        'PrincipalComponentAnalysisResult pcaBean = new PrincipalComponentAnalysisResult() {
        '    ScoreIdCollection = statObject.YIndexes,
        '    LoadingIdCollection = statObject.XIndexes,
        '    ScoreLabelCollection = statObject.YLabels,
        '    LoadingLabelCollecction = statObject.XLabels,
        '    ScoreBrushCollection = statObject.YColors,
        '    LoadingBrushCollection = statObject.XColors
        '};

        Dim tpMatrix = New Double(rowSize - 1, columnSize - 1) {}
        Dim mean, var, scoreOld, scoreNew, loading As Double()
        Dim sum, maxVar, threshold, scoreScalar, scoreVar, loadingScalar As Double, contributionOriginal As Double = 1
        Dim maxVarID As Integer

        Dim contributions = New ObservableCollection(Of Double)()
        Dim scores = New ObservableCollection(Of Double())()
        Dim loadings = New ObservableCollection(Of Double())()

        For i = 0 To maxPC - 1
            mean = New Double(columnSize - 1) {}
            var = New Double(columnSize - 1) {}
            scoreOld = New Double(rowSize - 1) {}
            scoreNew = New Double(rowSize - 1) {}
            loading = New Double(columnSize - 1) {}

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
            If i = 0 Then contributionOriginal = var.Sum

            maxVar = var.Max
            maxVarID = Array.IndexOf(var, maxVar)

            For j = 0 To rowSize - 1
                scoreOld(j) = dataArray(j, maxVarID)
            Next

            threshold = Double.MaxValue
            While threshold > 0.00000001
                scoreScalar = BasicMathematics.SumOfSquare(scoreOld)
                For j = 0 To columnSize - 1
                    sum = 0
                    For k = 0 To rowSize - 1
                        sum += dataArray(k, j) * scoreOld(k)
                    Next
                    loading(j) = sum / scoreScalar
                Next

                loadingScalar = BasicMathematics.RootSumOfSquare(loading)
                For j = 0 To columnSize - 1
                    loading(j) = loading(j) / loadingScalar
                Next

                For j = 0 To rowSize - 1
                    sum = 0
                    For k = 0 To columnSize - 1
                        sum += dataArray(j, k) * loading(k)
                    Next
                    scoreNew(j) = sum
                Next

                threshold = BasicMathematics.RootSumOfSquare(scoreNew, scoreOld)
                For j = 0 To scoreNew.Length - 1
                    scoreOld(j) = scoreNew(j)
                Next
            End While

            For j = 0 To columnSize - 1
                For k = 0 To rowSize - 1
                    tpMatrix(k, j) = scoreNew(k) * loading(j)
                    dataArray(k, j) = dataArray(k, j) - tpMatrix(k, j)
                Next
            Next
            scoreVar = BasicMathematics.Var(scoreNew)
            contributions.Add(scoreVar / contributionOriginal * 100)
            scores.Add(scoreNew)
            loadings.Add(loading)
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

End Module
