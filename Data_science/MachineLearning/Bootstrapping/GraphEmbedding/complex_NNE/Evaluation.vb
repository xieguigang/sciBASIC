#Region "Microsoft.VisualBasic::589701c5731af8a8f3a81f45c184b540, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\complex_NNE\Evaluation.vb"

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

    '   Total Lines: 144
    '    Code Lines: 135
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 7.08 KB


    '     Class Evaluation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: calculateMetrics
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct

Namespace GraphEmbedding.complex_NNE

    Public Class Evaluation
        Public lstTestTriples As TripleSet
        Public lstAllTriples As Dictionary(Of String, Boolean)
        Public Real_MatrixE As Matrix
        Public Real_MatrixR As Matrix
        Public Imag_MatrixE As Matrix
        Public Imag_MatrixR As Matrix
        Public dMRR As Double
        Public dMeanRank As Double
        Public dMedian As Double
        Public dHits1 As Double
        Public dHits3 As Double
        Public dHits5 As Double
        Public dHits10 As Double

        Public Sub New(inLstTestTriples As TripleSet, inLstAllTriples As Dictionary(Of String, Boolean), in_Real_MatrixE As Matrix, in_Real_MatrixR As Matrix, in_Imag_MatrixE As Matrix, in_Imag_MatrixR As Matrix)
            lstTestTriples = inLstTestTriples
            lstAllTriples = inLstAllTriples
            Real_MatrixE = in_Real_MatrixE
            Real_MatrixR = in_Real_MatrixR
            Imag_MatrixE = in_Imag_MatrixE
            Imag_MatrixR = in_Imag_MatrixR
        End Sub

        Public Overridable Sub calculateMetrics()
            Dim iNumberOfEntities As Integer = Real_MatrixE.rows()
            Dim iNumberOfFactors As Integer = Real_MatrixE.columns()
            Dim iList As List(Of Double) = New List(Of Double)()

            Dim iCnt = 0
            Dim avgMRR = 0.0
            Dim avgMeanRank = 0.0
            Dim avgHits1 = 0, avgHits3 = 0, avgHits5 = 0, avgHits10 = 0
            For iID = 0 To lstTestTriples.triples() - 1
                Dim iRelation As Integer = lstTestTriples.get(iID).relation()
                Dim iHead As Integer = lstTestTriples.get(iID).head()
                Dim iTail As Integer = lstTestTriples.get(iID).tail()
                Dim dTargetValue = 0.0
                For p = 0 To iNumberOfFactors - 1
                    dTargetValue += Real_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) - Imag_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) + Real_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p) + Imag_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p)
                Next

                Dim iLeftRank = 1
                Dim iLeftIdentical = 0
                For iLeftID = 0 To iNumberOfEntities - 1
                    Dim dValue = 0.0
                    Dim negTiple As String = iLeftID.ToString() & vbTab & iRelation.ToString() & vbTab & iTail.ToString()
                    If Not lstAllTriples.ContainsKey(negTiple) Then
                        For p = 0 To iNumberOfFactors - 1
                            dValue += Real_MatrixE.get(iLeftID, p) * Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) - Imag_MatrixE.get(iLeftID, p) * Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) + Real_MatrixE.get(iLeftID, p) * Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p) + Imag_MatrixE.get(iLeftID, p) * Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p)
                        Next
                        If dValue > dTargetValue Then
                            iLeftRank += 1
                        End If
                        If dValue = dTargetValue Then
                            iLeftIdentical += 1
                        End If
                    End If
                Next
                Dim dLeftRank = (2.0 * iLeftRank + iLeftIdentical) / 2.0
                Dim iLeftHits1 = 0, iLeftHits3 = 0, iLeftHits5 = 0, iLeftHits10 = 0
                If dLeftRank <= 1.0 Then
                    iLeftHits1 = 1
                End If
                If dLeftRank <= 3.0 Then
                    iLeftHits3 = 1
                End If
                If dLeftRank <= 5.0 Then
                    iLeftHits5 = 1
                End If
                If dLeftRank <= 10.0 Then
                    iLeftHits10 = 1
                End If
                avgMRR += 1.0 / dLeftRank
                avgMeanRank += dLeftRank
                avgHits1 += iLeftHits1
                avgHits3 += iLeftHits3
                avgHits5 += iLeftHits5
                avgHits10 += iLeftHits10
                iList.Add(dLeftRank)
                iCnt += 1

                Dim iRightRank = 1
                Dim iRightIdentical = 0
                For iRightID = 0 To iNumberOfEntities - 1
                    Dim dValue = 0.0
                    Dim negTiple As String = iHead.ToString() & vbTab & iRelation.ToString() & vbTab & iRightID.ToString()
                    If Not lstAllTriples.ContainsKey(negTiple) Then
                        For p = 0 To iNumberOfFactors - 1
                            dValue += Real_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iRightID, p) - Imag_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iRightID, p) + Real_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iRightID, p) + Imag_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iRightID, p)
                        Next
                        If dValue > dTargetValue Then
                            iRightRank += 1
                        End If
                        If dValue = dTargetValue Then
                            iRightIdentical += 1
                        End If
                    End If
                Next
                Dim dRightRank = (2.0 * iRightRank + iRightIdentical) / 2.0
                Dim iRightHits1 = 0, iRightHits3 = 0, iRightHits5 = 0, iRightHits10 = 0
                If dRightRank <= 1.0 Then
                    iRightHits1 = 1
                End If
                If dRightRank <= 3.0 Then
                    iRightHits3 = 1
                End If
                If dRightRank <= 5.0 Then
                    iRightHits5 = 1
                End If
                If dRightRank <= 10.0 Then
                    iRightHits10 = 1
                End If
                avgMRR += 1.0 / dRightRank
                avgMeanRank += dRightRank
                avgHits1 += iRightHits1
                avgHits3 += iRightHits3
                avgHits5 += iRightHits5
                avgHits10 += iRightHits10
                iList.Add(dRightRank)
                iCnt += 1
            Next
            dMRR = avgMRR / iCnt
            dMeanRank = avgMeanRank / iCnt
            dHits1 = avgHits1 / iCnt
            dHits3 = avgHits3 / iCnt
            dHits5 = avgHits5 / iCnt
            dHits10 = avgHits10 / iCnt

            iList.Sort()
            Dim indx As Integer = iList.Count / 2
            If iList.Count Mod 2 = 0 Then
                dMedian = (iList(indx - 1) + iList(indx)) / 2.0
            Else
                dMedian = iList(indx)
            End If
        End Sub
    End Class

End Namespace
