Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' Discriminative NMF 的 console 自测：基于合成的三类非负数据，检查分解维度、
''' 因子非负性、重构误差、收敛性以及判别表示的类间/类内散度比与最近质心准确率。
''' </summary>
''' <remarks>
''' 所有断言经由 <see cref="DebugAudit"/> 记录，结束时通过
''' <see cref="DebugAudit.SaveReport(String)"/> 导出 JSON 报告。
''' </remarks>
Public Module DNMFAudit

    Private Const ReportName As String = "dnmf-test-report.json"
    Private Const ClassCount As Integer = 3
    Private Const SamplesPerClass As Integer = 20
    Private Const FeatureCount As Integer = 12
    Private Const FactorRank As Integer = 3

    ''' <summary>运行全部 DNMF 审计用例并生成测试报告。</summary>
    ''' <returns>失败断言的数量（0 表示全部通过）。</returns>
    Public Function RunAll() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096
        Call DebugAudit.Mark("DNMF audit started")

        Try
            Dim rnd As New Random(20261119)
            Dim A As NumericMatrix = Nothing
            Dim labels As Integer() = Nothing

            Call MakeSyntheticData(rnd, A, labels)

            Dim model As DNMF = DNMF.Factorisation(A, labels,
                                                   k:=FactorRank,
                                                   max_iterations:=400,
                                                   tolerance:=0.0000001,
                                                   epsilon:=0.00000001,
                                                   lambdaWithin:=0.2,
                                                   lambdaBetween:=0.2,
                                                   randomSeed:=42,
                                                   tqdm:=False)

            Call DebugAudit.Mark("dnmf.cost", model.cost)
            Call DebugAudit.Mark("dnmf.iterations", model.errors.Length)

            DebugAudit.AreEqual(FactorRank, model.rank, "rank should match k")
            DebugAudit.Test(model.W IsNot Nothing AndAlso model.H IsNot Nothing, "W and H should be created")
            DebugAudit.Test(model.W.RowDimension = A.RowDimension AndAlso
                            model.W.ColumnDimension = FactorRank, "W should be m x k")
            DebugAudit.Test(model.H.RowDimension = FactorRank AndAlso
                            model.H.ColumnDimension = A.ColumnDimension, "H should be k x n")
            DebugAudit.Test(model.errors IsNot Nothing AndAlso model.errors.Length > 0, "errors should be recorded")
            DebugAudit.Test(IsFiniteAndNonNegative(model.W), "W should be finite and non-negative")
            DebugAudit.Test(IsFiniteAndNonNegative(model.H), "H should be finite and non-negative")
            DebugAudit.Test(Double.IsFinite(model.cost) AndAlso model.cost >= 0, "cost should be finite and non-negative")

            If model.errors.Length > 1 Then
                DebugAudit.Test(model.errors(model.errors.Length - 1) <= model.errors(0),
                                "cost should not increase after multiplicative updates")
            End If

            Dim relativeError As Double = ReconstructionRelativeError(A, model)
            Call DebugAudit.Mark("dnmf.relative_reconstruction_error", relativeError)
            DebugAudit.Test(relativeError < 0.25, $"relative reconstruction error should be small, got {relativeError:F4}")

            Dim learnedRatio As Double = ScatterRatio(model.W, labels)
            Dim rawRatio As Double = ScatterRatio(A, labels)
            Call DebugAudit.Mark("dnmf.scatter_ratio", learnedRatio)
            Call DebugAudit.Mark("raw.scatter_ratio", rawRatio)
            DebugAudit.Test(learnedRatio > 5, $"learned representation should be discriminative, ratio = {learnedRatio:F4}")

            Dim centroidAccuracy As Double = NearestCentroidAccuracy(model.W, labels)
            Call DebugAudit.Mark("dnmf.centroid_accuracy", centroidAccuracy)
            DebugAudit.Test(centroidAccuracy >= 0.9,
                            $"nearest-centroid accuracy should be at least 90%, got {centroidAccuracy:P2}")

            Call CheckInvalidArguments(A, labels)
        Catch ex As Exception
            Call DebugAudit.Mark("dnmf.audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"DNMF audit should not throw unhandled exceptions: {ex.Message}")
        End Try

        Call DebugAudit.SaveReport(reportPath)
        Call Console.WriteLine($"   [audit] report -> {reportPath}")
        Call Console.WriteLine($"   [audit] {DebugAudit.FailureCount} failure(s)")

        Return DebugAudit.FailureCount
    End Function

    Private Sub MakeSyntheticData(rnd As Random, ByRef A As NumericMatrix, ByRef labels As Integer())
        Dim m As Integer = ClassCount * SamplesPerClass
        Dim rows(m - 1)() As Double
        Dim prototypes(ClassCount - 1)() As Double
        ReDim labels(m - 1)

        For c As Integer = 0 To ClassCount - 1
            prototypes(c) = New Double(FeatureCount - 1) {}

            For j As Integer = 0 To FeatureCount - 1
                Dim value As Double = 0.05

                If j \ 4 = c Then
                    value += 0.8
                End If

                If j Mod ClassCount = c Then
                    value += 0.15
                End If

                prototypes(c)(j) = value
            Next
        Next

        For i As Integer = 0 To m - 1
            Dim classId As Integer = i \ SamplesPerClass
            labels(i) = classId
            rows(i) = New Double(FeatureCount - 1) {}

            For j As Integer = 0 To FeatureCount - 1
                rows(i)(j) = prototypes(classId)(j) * (0.85 + 0.3 * rnd.NextDouble()) + rnd.NextDouble() * 0.05
            Next
        Next

        A = New NumericMatrix(rows)
    End Sub

    Private Sub CheckInvalidArguments(A As NumericMatrix, labels As Integer())
        Dim wrongLabels(labels.Length - 2) As Integer
        Array.Copy(labels, wrongLabels, wrongLabels.Length)
        DebugAudit.CheckThrows(Of ArgumentException)(Sub() DNMF.Factorisation(A, wrongLabels, k:=FactorRank, tqdm:=False),
                                                     "label length mismatch should be rejected")

        Dim oneLabel(labels.Length - 1) As Integer
        DebugAudit.CheckThrows(Of ArgumentException)(Sub() DNMF.Factorisation(A, oneLabel, k:=FactorRank, tqdm:=False),
                                                     "single-class labels should be rejected")
    End Sub

    Private Function IsFiniteAndNonNegative(matrix As NumericMatrix) As Boolean
        For i As Integer = 0 To matrix.RowDimension - 1
            For j As Integer = 0 To matrix.ColumnDimension - 1
                If Not Double.IsFinite(matrix(i, j)) OrElse matrix(i, j) < 0 Then
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    Private Function ReconstructionRelativeError(A As NumericMatrix, model As DNMF) As Double
        Dim V As NumericMatrix = DirectCast(model.W.DotProduct(model.H), NumericMatrix)
        Dim sumSquareError As Double = 0
        Dim sumSquare As Double = 0

        For i As Integer = 0 To A.RowDimension - 1
            For j As Integer = 0 To A.ColumnDimension - 1
                Dim difference As Double = A(i, j) - V(i, j)
                sumSquareError += difference * difference
                sumSquare += A(i, j) * A(i, j)
            Next
        Next

        Return If(sumSquare > 0, sumSquareError / sumSquare, sumSquareError)
    End Function

    Private Function ScatterRatio(matrix As NumericMatrix, labels As Integer()) As Double
        Dim k As Integer = matrix.ColumnDimension
        Dim overallMean(k - 1) As Double
        Dim means(ClassCount - 1)() As Double
        Dim counts(ClassCount - 1) As Integer

        For c As Integer = 0 To ClassCount - 1
            means(c) = New Double(k - 1) {}
        Next

        For i As Integer = 0 To matrix.RowDimension - 1
            Dim c As Integer = labels(i)
            counts(c) += 1

            For j As Integer = 0 To k - 1
                means(c)(j) += matrix(i, j)
                overallMean(j) += matrix(i, j)
            Next
        Next

        For j As Integer = 0 To k - 1
            overallMean(j) /= matrix.RowDimension
        Next

        Dim within As Double = 0
        Dim between As Double = 0

        For c As Integer = 0 To ClassCount - 1
            For j As Integer = 0 To k - 1
                means(c)(j) /= counts(c)
            Next

            For i As Integer = 0 To matrix.RowDimension - 1
                If labels(i) <> c Then
                    Continue For
                End If

                For j As Integer = 0 To k - 1
                    Dim withinDifference As Double = matrix(i, j) - means(c)(j)
                    Dim betweenDifference As Double = means(c)(j) - overallMean(j)
                    within += withinDifference * withinDifference
                    between += betweenDifference * betweenDifference
                Next
            Next
        Next

        Return If(within > 0, between / within, Double.PositiveInfinity)
    End Function

    Private Function NearestCentroidAccuracy(matrix As NumericMatrix, labels As Integer()) As Double
        Dim k As Integer = matrix.ColumnDimension
        Dim centroids(ClassCount - 1)() As Double
        Dim counts(ClassCount - 1) As Integer

        For c As Integer = 0 To ClassCount - 1
            centroids(c) = New Double(k - 1) {}
        Next

        For i As Integer = 0 To matrix.RowDimension - 1
            Dim c As Integer = labels(i)
            counts(c) += 1

            For j As Integer = 0 To k - 1
                centroids(c)(j) += matrix(i, j)
            Next
        Next

        For c As Integer = 0 To ClassCount - 1
            For j As Integer = 0 To k - 1
                centroids(c)(j) /= counts(c)
            Next
        Next

        Dim correct As Integer = 0

        For i As Integer = 0 To matrix.RowDimension - 1
            Dim bestClass As Integer = -1
            Dim bestDistance As Double = Double.PositiveInfinity

            For c As Integer = 0 To ClassCount - 1
                Dim distance As Double = 0

                For j As Integer = 0 To k - 1
                    Dim difference As Double = matrix(i, j) - centroids(c)(j)
                    distance += difference * difference
                Next

                If distance < bestDistance Then
                    bestDistance = distance
                    bestClass = c
                End If
            Next

            If bestClass = labels(i) Then
                correct += 1
            End If
        Next

        Return correct / CDbl(matrix.RowDimension)
    End Function

End Module
