Imports System.IO
Imports System.Runtime.CompilerServices

Public Module Debugger

    <Extension>
    Public Sub WritePlsResult(x As MultivariateAnalysisResult, output As Stream)
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

    <Extension>
    Public Sub WriteOplsResult(x As MultivariateAnalysisResult, output As Stream)
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

    <Extension>
    Public Sub WritePcaResult(x As MultivariateAnalysisResult, output As Stream)

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
End Module
