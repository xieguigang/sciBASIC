#Region "Microsoft.VisualBasic::017a797f732df2bd5aaad568a463f093, Data_science\Graph\EMD\JFastEMD.vb"

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

    '   Total Lines: 368
    '    Code Lines: 249 (67.66%)
    ' Comment Lines: 79 (21.47%)
    '    - Xml Docs: 36.71%
    ' 
    '   Blank Lines: 40 (10.87%)
    '     File Size: 14.32 KB


    '     Class JFastEMD
    ' 
    '         Function: distance, emdHat, emdHatImplLongLongInt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

'
' This class computes the Earth Mover's Distance, using the EMD-HAT algorithm
' created by Ofir Pele and Michael Werman.
' 
' This implementation is strongly based on the C++ code by the same authors,
' that can be found here:
' http://www.cs.huji.ac.il/~ofirpele/FastEMD/code/
' 
' Some of the author's comments on the original were kept or edited for 
' this context.
'

Namespace EMD

    ''' <summary>
    ''' Earth Mover's Distance
    ''' 
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' @author Ofir Pele
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/telmomenezes/JFastEMD
    ''' </remarks>
    Public Class JFastEMD

        ''' <summary>
        ''' This interface is similar to Rubner's interface. See:
        ''' http://www.cs.duke.edu/~tomasi/software/emd.htm
        ''' 
        ''' To get the same results as Rubner's code you should set extra_mass_penalty to 0,
        ''' and divide by the minimum of the sum of the two signature's weights. However, I
        ''' suggest not to do this as you lose the metric property and more importantly, in my
        ''' experience the performance is better with emd_hat. for more on the difference
        ''' between emd and emd_hat, see the paper:
        ''' A Linear Time Histogram Metric for Improved SIFT Matching
        ''' Ofir Pele, Michael Werman
        ''' ECCV 2008
        ''' 
        ''' To get shorter running time, set the ground distance function to
        ''' be a thresholded distance. For example: min(L2, T). Where T is some threshold.
        ''' Note that the running time is shorter with smaller T values. Note also that
        ''' thresholding the distance will probably increase accuracy. Finally, a thresholded
        ''' metric is also a metric. See paper:
        ''' Fast and Robust Earth Mover's Distances
        ''' Ofir Pele, Michael Werman
        ''' ICCV 2009
        ''' 
        ''' If you use this code, please cite the papers.
        ''' </summary>
        Public Shared Function distance(signature1 As Signature, signature2 As Signature, extraMassPenalty As Double) As Double

            Dim P As List(Of Double) = New List(Of Double)()
            Dim Q As List(Of Double) = New List(Of Double)()
            For i = 0 To signature1.NumberOfFeatures + signature2.NumberOfFeatures - 1
                P.Add(0.0)
                Q.Add(0.0)
            Next
            For i = 0 To signature1.NumberOfFeatures - 1
                P(i) = signature1.Weights(i)
            Next
            For j = 0 To signature2.NumberOfFeatures - 1
                Q(j + signature1.NumberOfFeatures) = signature2.Weights(j)
            Next

            Dim C As List(Of List(Of Double)) = New List(Of List(Of Double))()
            For i = 0 To P.Count - 1
                Dim vec As List(Of Double) = New List(Of Double)()
                For j = 0 To P.Count - 1
                    vec.Add(0.0)
                Next
                C.Add(vec)
            Next
            For i = 0 To signature1.NumberOfFeatures - 1
                For j = 0 To signature2.NumberOfFeatures - 1
                    Dim dist = signature1.Features(i).groundDist(signature2.Features(j))

                    C(i)(j + signature1.NumberOfFeatures) = dist
                    C(j + signature1.NumberOfFeatures)(i) = dist
                Next
            Next

            Return emdHat(P, Q, C, extraMassPenalty)
        End Function


        Private Shared Function emdHatImplLongLongInt(Pc As List(Of Long), Qc As List(Of Long), C As List(Of List(Of Long)), extraMassPenalty As Long) As Long

            Dim N = Pc.Count

            ' Ensuring that the supplier - P, have more mass.
            ' Note that we assume here that C is symmetric
            Dim P As List(Of Long)
            Dim Q As List(Of Long)
            Dim absDiffSumPSumQ As Long
            Dim sumP As Long = 0
            Dim sumQ As Long = 0
            For i As Integer = 0 To N - 1
                sumP += Pc(i)
            Next
            For i As Integer = 0 To N - 1
                sumQ += Qc(i)
            Next
            If sumQ > sumP Then
                P = Qc
                Q = Pc
                absDiffSumPSumQ = sumQ - sumP
            Else
                P = Pc
                Q = Qc
                absDiffSumPSumQ = sumP - sumQ
            End If

            ' creating the b vector that contains all vertexes
            Dim b As List(Of Long) = New List(Of Long)()
            For i As Integer = 0 To 2 * N + 2 - 1
                b.Add(0L)
            Next
            Dim THRESHOLD_NODE = 2 * N
            Dim ARTIFICIAL_NODE = 2 * N + 1 ' need to be last !
            For i As Integer = 0 To N - 1
                b(i) = P(i)
            Next
            For i As Integer = N To 2 * N - 1
                b(i) = Q(i - N)
            Next

            ' remark*) I put here a deficit of the extra mass, as mass that flows
            ' to the threshold node
            ' can be absorbed from all sources with cost zero (this is in reverse
            ' order from the paper,
            ' where incoming edges to the threshold node had the cost of the
            ' threshold and outgoing
            ' edges had the cost of zero)
            ' This also makes sum of b zero.
            b(THRESHOLD_NODE) = -absDiffSumPSumQ
            b(ARTIFICIAL_NODE) = 0L

            Dim maxC As Long = 0
            For i As Integer = 0 To N - 1
                For j As Integer = 0 To N - 1
                    If C(i)(j) > maxC Then
                        maxC = C(i)(j)
                    End If
                Next
            Next
            If extraMassPenalty = -1 Then
                extraMassPenalty = maxC
            End If

            Dim sourcesThatFlowNotOnlyToThresh As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim sinksThatGetFlowNotOnlyFromThresh As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim preFlowCost As Long = 0

            ' regular edges between sinks and sources without threshold edges
            Dim lC As List(Of IList(Of Edge)) = New List(Of IList(Of Edge))()
            For i As Integer = 0 To b.Count - 1
                lC.Add(New List(Of Edge)())
            Next
            For i As Integer = 0 To N - 1
                If b(i) = 0 Then
                    Continue For
                End If
                For j = 0 To N - 1
                    If b(j + N) = 0 Then
                        Continue For
                    End If
                    If C(i)(j) = maxC Then
                        Continue For
                    End If
                    lC(i).Add(New Edge(j + N, C(i)(j)))
                Next
            Next

            ' checking which are not isolated
            For i As Integer = 0 To N - 1
                If b(i) = 0 Then
                    Continue For
                End If
                For j = 0 To N - 1
                    If b(j + N) = 0 Then
                        Continue For
                    End If
                    If C(i)(j) = maxC Then
                        Continue For
                    End If
                    sourcesThatFlowNotOnlyToThresh.Add(i)
                    sinksThatGetFlowNotOnlyFromThresh.Add(j + N)
                Next
            Next

            ' converting all sinks to negative
            For i As Integer = N To 2 * N - 1
                b(i) = -b(i)
            Next

            ' add edges from/to threshold node,
            ' note that costs are reversed to the paper (see also remark* above)
            ' It is important that it will be this way because of remark* above.
            Dim idx = 0

            While idx < N
                lC(idx).Add(New Edge(THRESHOLD_NODE, 0))
                idx += 1
            End While
            Dim jdx = 0

            While jdx < N
                lC(THRESHOLD_NODE).Add(New Edge(jdx + N, maxC))
                jdx += 1
            End While

            ' artificial arcs - Note the restriction that only one edge i,j is
            ' artificial so I ignore it...
            For i = 0 To ARTIFICIAL_NODE - 1
                lC(i).Add(New Edge(ARTIFICIAL_NODE, maxC + 1))
                lC(ARTIFICIAL_NODE).Add(New Edge(i, maxC + 1))
            Next

            ' remove nodes with supply demand of 0
            ' and vertexes that are connected only to the
            ' threshold vertex
            Dim currentNodeName = 0
            ' Note here it should be vector<int> and not vector<int>
            ' as I'm using -1 as a special flag !!!
            Dim REMOVE_NODE_FLAG = -1
            Dim nodesNewNames As List(Of Integer) = New List(Of Integer)()
            Dim nodesOldNames As List(Of Integer) = New List(Of Integer)()
            For i = 0 To b.Count - 1
                nodesNewNames.Add(REMOVE_NODE_FLAG)
                nodesOldNames.Add(0)
            Next
            For i = 0 To N * 2 - 1
                If b(i) <> 0 Then
                    If sourcesThatFlowNotOnlyToThresh.Contains(i) OrElse sinksThatGetFlowNotOnlyFromThresh.Contains(i) Then
                        nodesNewNames(i) = currentNodeName
                        nodesOldNames.Add(i)
                        currentNodeName += 1
                    Else
                        If i >= N Then
                            preFlowCost -= b(i) * maxC
                        End If
                        b(THRESHOLD_NODE) = b(THRESHOLD_NODE) + b(i) ' add mass(i<N) or deficit (i>=N)
                    End If
                End If
            Next
            nodesNewNames(THRESHOLD_NODE) = currentNodeName
            nodesOldNames.Add(THRESHOLD_NODE)
            currentNodeName += 1
            nodesNewNames(ARTIFICIAL_NODE) = currentNodeName
            nodesOldNames.Add(ARTIFICIAL_NODE)
            currentNodeName += 1

            Dim bb As List(Of Long) = New List(Of Long)()
            For i As Integer = 0 To currentNodeName - 1
                bb.Add(0L)
            Next
            Dim ji = 0
            For i = 0 To b.Count - 1
                If nodesNewNames(i) <> REMOVE_NODE_FLAG Then
                    bb(ji) = b(i)
                    ji += 1
                End If
            Next

            Dim cc As List(Of IList(Of Edge)) = New List(Of IList(Of Edge))()
            For i = 0 To bb.Count - 1
                cc.Add(New List(Of Edge)())
            Next
            For i = 0 To lC.Count - 1
                If nodesNewNames(i) = REMOVE_NODE_FLAG Then
                    Continue For
                End If
                For Each it In lC(i)
                    If nodesNewNames(it._to) <> REMOVE_NODE_FLAG Then
                        cc(nodesNewNames(i)).Add(New Edge(nodesNewNames(it._to), it._cost))
                    End If
                Next
            Next

            Dim mcf As MinCostFlow = New MinCostFlow()

            Dim myDist As Long

            Dim flows As List(Of IList(Of Edge0)) = New List(Of IList(Of Edge0))(bb.Count)
            For i = 0 To bb.Count - 1
                flows.Add(New List(Of Edge0)())
            Next

            Dim mcfDist = mcf.compute(bb, cc, flows)

            myDist = preFlowCost + mcfDist + absDiffSumPSumQ * extraMassPenalty ' emd-hat extra mass penalty

            Return myDist
        End Function

        Private Shared Function emdHat(P As List(Of Double), Q As List(Of Double), C As List(Of List(Of Double)), extraMassPenalty As Double) As Double

            ' This condition should hold:
            ' ( 2^(sizeof(CONVERT_TO_T*8)) >= ( MULT_FACTOR^2 )
            ' Note that it can be problematic to check it because
            ' of overflow problems. I simply checked it with Linux calc
            ' which has arbitrary precision.
            Dim MULT_FACTOR As Double = 1000000

            ' Constructing the input
            Dim N = P.Count
            Dim iP As List(Of Long) = New List(Of Long)()
            Dim iQ As List(Of Long) = New List(Of Long)()
            Dim iC As List(Of List(Of Long)) = New List(Of List(Of Long))()
            For i = 0 To N - 1
                iP.Add(0L)
                iQ.Add(0L)
                Dim vec As List(Of Long) = New List(Of Long)()
                For j = 0 To N - 1
                    vec.Add(0L)
                Next
                iC.Add(vec)
            Next

            ' Converting to CONVERT_TO_T
            Dim sumP = 0.0
            Dim sumQ = 0.0
            Dim maxC = C(0)(0)
            For i = 0 To N - 1
                sumP += P(i)
                sumQ += Q(i)
                For j = 0 To N - 1
                    If C(i)(j) > maxC Then
                        maxC = C(i)(j)
                    End If
                Next
            Next
            Dim minSum = std.Min(sumP, sumQ)
            Dim maxSum = std.Max(sumP, sumQ)
            Dim PQnormFactor = MULT_FACTOR / maxSum
            Dim CnormFactor = MULT_FACTOR / maxC

            For i = 0 To N - 1
                iP(i) = CLng(std.Floor(P(i) * PQnormFactor + 0.5))
                iQ(i) = CLng(std.Floor(Q(i) * PQnormFactor + 0.5))
                For j = 0 To N - 1
                    If C(i)(j) = 0.0 Then
                        iC(i)(j) = CLng(0.5)
                    Else
                        iC(i)(j) = CLng(std.Floor(C(i)(j) * CnormFactor + 0.5))
                    End If
                Next
            Next

            ' computing distance without extra mass penalty
            Dim dist As Double = emdHatImplLongLongInt(iP, iQ, iC, 0)
            ' unnormalize
            dist = dist / PQnormFactor
            dist = dist / CnormFactor

            ' adding extra mass penalty
            If extraMassPenalty = -1 Then
                extraMassPenalty = maxC
            End If
            dist += (maxSum - minSum) * extraMassPenalty

            Return dist
        End Function
    End Class
End Namespace

