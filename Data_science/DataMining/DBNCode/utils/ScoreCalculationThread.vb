Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn

Namespace utils

    Public Class ScoreCalculationThread
        Private t As Integer
        Private i_init As Integer
        Private i_final As Integer
        Private parentSets As IList(Of IList(Of Integer))
        Private observations As Observations
        Private scoresMatrix As Double()()()
        Private parentNodesPast As IList(Of IList(Of IList(Of Integer)))
        Private parentNodes As IList(Of IList(Of IList(Of IList(Of Integer))))
        Private numBestScores As Integer()()
        Private numBestScoresPast As Integer()
        Private n As Integer
        Private sf As ScoringFunction
        Private stationaryProcess As Boolean






        ''' <param name="t"> </param>
        ''' <param name="i_init"> </param>
        ''' <param name="i_final"> </param>
        ''' <param name="parentSets"> </param>
        ''' <param name="observations"> </param>
        ''' <param name="scoresMatrix"> </param>
        ''' <param name="parentNodesPast"> </param>
        ''' <param name="parentNodes"> </param>
        ''' <param name="numBestScores"> </param>
        ''' <param name="numBestScoresPast"> </param>
        ''' <param name="n"> </param>
        ''' <param name="sf"> </param>
        ''' <param name="stationaryProcess"> </param>
        Public Sub New(t As Integer, i_init As Integer, i_final As Integer, n As Integer, parentSets As IList(Of IList(Of Integer)), observations As Observations, scoresMatrix As Double()()(), parentNodesPast As IList(Of IList(Of IList(Of Integer))), parentNodes As IList(Of IList(Of IList(Of IList(Of Integer)))), numBestScores As Integer()(), numBestScoresPast As Integer(), sf As ScoringFunction, stationaryProcess As Boolean?)
            MyBase.New()
            Me.t = t
            Me.i_init = i_init
            Me.i_final = i_final
            Me.parentSets = parentSets
            Me.observations = observations
            Me.scoresMatrix = scoresMatrix
            Me.parentNodesPast = parentNodesPast
            Me.parentNodes = parentNodes
            Me.numBestScores = numBestScores
            Me.numBestScoresPast = numBestScoresPast
            Me.n = n
            Me.sf = sf

            Me.stationaryProcess = CBool(stationaryProcess)
        End Sub






        Public Overridable Sub run()
            For i = i_init To i_final - 1
                ' System.out.println("evaluating node " + i + "/" + n);
                Dim bestScore = Double.NegativeInfinity


                For Each parentSet In parentSets
                    Dim score = If(stationaryProcess, sf.evaluate(observations, parentSet, i), sf.evaluate(observations, t, parentSet, i))
                    ' System.out.println("Xi:" + i + " ps:" + parentSet +
                    ' " score:" + score);
                    If bestScore < score Then
                        bestScore = score
                        parentNodesPast(t)(i) = parentSet
                        numBestScoresPast(i) = 1
                    ElseIf bestScore = score Then
                        numBestScoresPast(i) += 1
                    End If
                Next


                'System.out.println("Finished parents past");
                For j = 0 To n - 1
                    scoresMatrix(t)(i)(j) = -bestScore
                Next
                For j = 0 To n - 1
                    If i <> j Then
                        bestScore = Double.NegativeInfinity
                        For Each parentSet In parentSets
                            Dim score = If(stationaryProcess, sf.evaluate(observations, parentSet, j, i), sf.evaluate(observations, t, parentSet, j, i))
                            ' System.out.println("Xi:" + i + " Xj:" + j +
                            ' " ps:" + parentSet + " score:" + score);
                            If bestScore < score Then
                                bestScore = score
                                parentNodes(t)(i)(j) = parentSet
                                numBestScores(i)(j) = 1
                            ElseIf bestScore = score Then
                                numBestScores(i)(j) += 1
                            End If
                        Next

                        scoresMatrix(t)(i)(j) += bestScore

                    End If
                Next
            Next
        End Sub


    End Class
End Namespace
