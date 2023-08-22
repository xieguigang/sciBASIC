Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Public Class MultivariateAnalysisResult
    Public Sub New()
    End Sub

    ' model set
    Public Property StatisticsObject As StatisticsObject
    ' Public Property MultivariateAnalysisOption As MultivariateAnalysisOption = MultivariateAnalysisOption.Plsda

    ' cv result
    Public Property NFold As Integer = 7
    Public Property OptimizedFactor As Integer = 0
    Public Property OptimizedOrthoFactor As Integer = 0
    Public Property SsCVs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Presses As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Totals As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Q2Values As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Q2Cums As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()


    ' modeled set
    Public Property SsPreds As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property CPreds As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property UPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property TPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property WPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()

    Public Property Coefficients As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Vips As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property PredictedYs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Rmsee As Double = 0.0

    ' opls
    Public Property ToPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property WoPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PoPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property stdevT As Double = 0.0
    Public Property StdevFilteredXs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property FilteredXArray As Double(,)
    Public Property PPredCovs As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PPredCoeffs As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()

    ' pca
    Public Property Contributions As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()

    Public Sub WritePlsResult(output As String)
        Using sw As StreamWriter = New StreamWriter(output, False, Encoding.ASCII)
            sw.WriteLine("Method" & Microsoft.VisualBasic.Constants.vbTab & "PLS")
            sw.WriteLine("Optimized factor" & Microsoft.VisualBasic.Constants.vbTab & OptimizedFactor.ToString())
            sw.WriteLine()
            sw.WriteLine("Cross validation N fold" & Microsoft.VisualBasic.Constants.vbTab & NFold.ToString())
            sw.WriteLine("Component" & Microsoft.VisualBasic.Constants.vbTab & "SSCV" & Microsoft.VisualBasic.Constants.vbTab & "PRESS" & Microsoft.VisualBasic.Constants.vbTab & "Q2" & Microsoft.VisualBasic.Constants.vbTab & "Q2cum")
            For i = 0 To Presses.Count - 1
                sw.WriteLine((i + 1).ToString() & Microsoft.VisualBasic.Constants.vbTab & SsCVs(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Presses(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Q2Values(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Q2Cums(i).ToString())
            Next
            sw.WriteLine()

            Dim scoreSeq = New List(Of String)()
            Dim loadSeq = New List(Of String)()

            For i = 0 To OptimizedFactor - 1
                scoreSeq.Add("T" & (i + 1).ToString())
                loadSeq.Add("P" & (i + 1).ToString())
            Next

            scoreSeq.Add("Y experiment")
            scoreSeq.Add("Y predicted")
            loadSeq.Add("VIP")
            loadSeq.Add("Coefficients")

            Dim scoreSeqString = String.Join(Microsoft.VisualBasic.Constants.vbTab, scoreSeq)
            Dim loadSeqString = String.Join(Microsoft.VisualBasic.Constants.vbTab, loadSeq)

            'header set
            Dim tpredSize = TPreds.Count
            Dim toPredSize = ToPreds.Count
            Dim metSize = StatisticsObject.XIndexes.Count
            Dim fileSize = StatisticsObject.YIndexes.Count

            sw.WriteLine("Score" & Microsoft.VisualBasic.Constants.vbTab & scoreSeqString)

            'Scores
            For i = 0 To fileSize - 1
                Dim tList = New List(Of Double)()
                For j = 0 To TPreds.Count - 1
                    tList.Add(TPreds(j)(i))
                Next
                tList.Add(StatisticsObject.YVariables(i))
                tList.Add(PredictedYs(i))

                sw.WriteLine(StatisticsObject.YLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, tList))
            Next
            sw.WriteLine()

            'Loadings
            sw.WriteLine("Loading" & Microsoft.VisualBasic.Constants.vbTab & loadSeqString)
            For i = 0 To metSize - 1
                Dim pList = New List(Of Double)()
                For j = 0 To PPreds.Count - 1
                    pList.Add(PPreds(j)(i))
                Next
                pList.Add(Vips(i))
                pList.Add(Coefficients(i))

                sw.WriteLine(StatisticsObject.XLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, pList))
            Next
        End Using
    End Sub

    Public Sub WriteOplsResult(output As String)
        Using sw As StreamWriter = New StreamWriter(output, False, Encoding.ASCII)
            sw.WriteLine("Method" & Microsoft.VisualBasic.Constants.vbTab & "OPLS")
            sw.WriteLine("Optimized biological factor" & Microsoft.VisualBasic.Constants.vbTab & OptimizedFactor.ToString())
            sw.WriteLine("Optimized orthogonal factor" & Microsoft.VisualBasic.Constants.vbTab & OptimizedOrthoFactor.ToString())
            sw.WriteLine()
            sw.WriteLine("Cross validation N fold" & Microsoft.VisualBasic.Constants.vbTab & NFold.ToString())
            sw.WriteLine("Component" & Microsoft.VisualBasic.Constants.vbTab & "SSCV" & Microsoft.VisualBasic.Constants.vbTab & "PRESS" & Microsoft.VisualBasic.Constants.vbTab & "Q2" & Microsoft.VisualBasic.Constants.vbTab & "Q2cum")
            For i = 0 To Presses.Count - 1
                sw.WriteLine((i + 1).ToString() & Microsoft.VisualBasic.Constants.vbTab & SsCVs(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Presses(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Q2Values(i).ToString() & Microsoft.VisualBasic.Constants.vbTab & Q2Cums(i).ToString())
            Next
            sw.WriteLine()

            Dim scoreSeq = New List(Of String)()
            Dim loadSeq = New List(Of String)()

            For i = 0 To OptimizedFactor - 1
                scoreSeq.Add("T" & (i + 1).ToString())
                loadSeq.Add("P" & (i + 1).ToString())
            Next

            For i = 0 To OptimizedOrthoFactor - 1
                scoreSeq.Add("To" & (i + 1).ToString())
                loadSeq.Add("Po" & (i + 1).ToString())
            Next

            scoreSeq.Add("Y experiment")
            scoreSeq.Add("Y predicted")
            loadSeq.Add("VIP")
            loadSeq.Add("Coefficients")

            Dim scoreSeqString = String.Join(Microsoft.VisualBasic.Constants.vbTab, scoreSeq)
            Dim loadSeqString = String.Join(Microsoft.VisualBasic.Constants.vbTab, loadSeq)

            'header set
            Dim tpredSize = TPreds.Count
            Dim toPredSize = ToPreds.Count
            Dim metSize = StatisticsObject.XIndexes.Count
            Dim fileSize = StatisticsObject.YIndexes.Count

            sw.WriteLine("Score" & Microsoft.VisualBasic.Constants.vbTab & scoreSeqString)

            'Scores
            For i = 0 To fileSize - 1
                Dim tList = New List(Of Double)()
                For j = 0 To TPreds.Count - 1
                    tList.Add(TPreds(j)(i))
                Next
                For j = 0 To ToPreds.Count - 1
                    tList.Add(ToPreds(j)(i))
                Next
                tList.Add(StatisticsObject.YVariables(i))
                tList.Add(PredictedYs(i))

                sw.WriteLine(StatisticsObject.YLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, tList))
            Next
            sw.WriteLine()

            'Loadings
            sw.WriteLine("Loading" & Microsoft.VisualBasic.Constants.vbTab & loadSeqString)
            For i = 0 To metSize - 1
                Dim pList = New List(Of Double)()
                For j = 0 To PPreds.Count - 1
                    pList.Add(PPreds(j)(i))
                Next
                For j = 0 To PoPreds.Count - 1
                    pList.Add(PoPreds(j)(i))
                Next
                pList.Add(Vips(i))
                pList.Add(Coefficients(i))

                sw.WriteLine(StatisticsObject.XLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, pList))
            Next
        End Using
    End Sub

    Public Sub WritePcaResult(output As String)

        Using sw As StreamWriter = New StreamWriter(output, False, Encoding.ASCII)
            'header set
            sw.WriteLine("Contribution")
            For i = 0 To Contributions.Count - 1
                sw.WriteLine((i + 1).ToString() & Microsoft.VisualBasic.Constants.vbTab & Contributions(i).ToString())
            Next
            sw.WriteLine()

            Dim compSize = Contributions.Count
            Dim filesize = StatisticsObject.YLabels.Count
            Dim metsize = StatisticsObject.XLabels.Count
            Dim compSequence = New List(Of Integer)()
            For i = 0 To compSize - 1
                compSequence.Add(i + 1)
            Next
            Dim compSeqString = String.Join(Microsoft.VisualBasic.Constants.vbTab, compSequence)

            'header set
            sw.WriteLine("Score" & Microsoft.VisualBasic.Constants.vbTab & compSeqString)

            For i = 0 To filesize - 1
                Dim tList = New List(Of Double)()
                For j = 0 To compSize - 1
                    tList.Add(TPreds(j)(i))
                Next
                sw.WriteLine(StatisticsObject.YLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, tList))
            Next

            sw.WriteLine()

            'header set
            sw.WriteLine("Loading" & Microsoft.VisualBasic.Constants.vbTab & compSeqString)

            For i = 0 To metsize - 1
                Dim pList = New List(Of Double)()
                For j = 0 To compSize - 1
                    pList.Add(PPreds(j)(i))
                Next
                sw.WriteLine(StatisticsObject.XLabels(i) & Microsoft.VisualBasic.Constants.vbTab & String.Join(Microsoft.VisualBasic.Constants.vbTab, pList))
            Next
        End Using
    End Sub
End Class
