#Region "Microsoft.VisualBasic::c8e61874a260bcf2315f620776751bb1, Data_science\DataMining\DataMining\Kernel\BayesianBeliefNetwork\BNInfer.vb"

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

    '   Total Lines: 143
    '    Code Lines: 103 (72.03%)
    ' Comment Lines: 10 (6.99%)
    '    - Xml Docs: 70.00%
    ' 
    '   Blank Lines: 30 (20.98%)
    '     File Size: 5.22 KB


    '     Class BNInfer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class BElim
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindMaxNodeId, GetBelief, Sum
    ' 
    '         Sub: PrepareBuckets
    '         Class Bucket
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Kernel.BayesianBeliefNetwork

    Public MustInherit Class BNInfer

        Public BeliefNetwork As BeliefNetwork

        Protected Sub New(net As BeliefNetwork)
            BeliefNetwork = net
        End Sub

        Public MustOverride Function GetBelief(x As Integer(), conditions As Integer()) As Double

        Public Overrides Function ToString() As String
            Return BeliefNetwork.ToString
        End Function
    End Class

    Public NotInheritable Class BElim : Inherits BNInfer

        Dim _Buckets As List(Of Bucket) = New List(Of Bucket)

        Public Sub New(net As BeliefNetwork)
            Call MyBase.New(net)
            Call PrepareBuckets()
        End Sub

        Private Class Bucket

            Public Sub New(i As Integer)
                id = i
            End Sub

            Public id As Integer
            Public parentNodes As List(Of BeliefNode) = New List(Of BeliefNode)
            Public childBuckets As List(Of Bucket) = New List(Of Bucket)

            Public Overrides Function ToString() As String
                Return String.Format("Id:='{0}'", id)
            End Function
        End Class

        ''' <summary>
        ''' 计算条件概率: P(x|conditions)，对于计算对象x和计算条件condition二者的元素必须要错开。对于错开部分的空的元素请使用-1来填充
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="conditions"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GetBelief(x As Integer(), conditions As Integer()) As Double
            Dim norm As Double = 1.0

            Call MyBase.BeliefNetwork.ResetNodes()

            If Not conditions.IsNullOrEmpty Then
                Call BeliefNetwork.SetNodes(conditions)
                norm = Sum(0, 0)
            End If
            Call BeliefNetwork.SetNodes(x)

            Return Sum(0, 0) / norm
        End Function

        Private Sub PrepareBuckets()
            Dim nodes As BeliefNode() = BeliefNetwork.Nodes

            For i As Integer = 0 To nodes.Count - 1
                _Buckets.Add(New Bucket(i))
            Next

            ' go through all buckets from buttom up
            For i As Integer = nodes.Count - 1 To 0 Step -1
                Dim theNode As BeliefNode = nodes(i)
                Dim theBuck As Bucket = _Buckets(i)

                For Each node As BeliefNode In theNode.Parents
                    theBuck.parentNodes.Add(node)
                Next

                For Each nxtBuck As Bucket In theBuck.childBuckets
                    For Each node As BeliefNode In nxtBuck.parentNodes
                        If node.Id <> i AndAlso Not theBuck.parentNodes.Contains(node) Then
                            theBuck.parentNodes.Add(node)
                        End If
                    Next
                Next

                Dim max_nid As Integer = FindMaxNodeId(theBuck.parentNodes)
                If max_nid >= 0 Then
                    _Buckets(max_nid).childBuckets.Add(theBuck)
                End If
            Next
        End Sub

        Protected Function Sum(nid As Integer, para As Integer) As Double
#If DEBUG Then
            Call FileIO.FileSystem.WriteAllText("x:\debug.txt", String.Format("Stack in => {0} |", nid), True)
#End If
            Dim theNode As BeliefNode = BeliefNetwork._bnNodes(nid)
            Dim theBuck As Bucket = _Buckets(nid)
            Dim p_cnt As Integer = theNode.Parents.Count
            Dim cond As Integer = (para And ((1 << p_cnt) - 1))
            Dim pr As Double = 0.0

            ' sum over all possible values
            For e As Integer = 0 To 1
                If theNode.Evidence <> -1 AndAlso theNode.Evidence <> e Then
                    Continue For
                End If

                Dim tmpPr As Double = theNode.CP_Table(cond, e)

                ' count child bucket's contribution
                For Each nxtBuck As Bucket In theBuck.childBuckets
                    Dim next_para As Integer = 0

                    For j As Integer = 0 To nxtBuck.parentNodes.Count - 1
                        Dim pnode As BeliefNode = nxtBuck.parentNodes(j)
                        Dim pos As Integer = theBuck.parentNodes.IndexOf(pnode)
                        next_para += If((pos >= 0), ((para >> pos And 1) << j), (e << j))
                    Next
                    tmpPr *= Sum(nxtBuck.id, next_para)
                Next
                pr += tmpPr
            Next

#If DEBUG Then
            Call FileIO.FileSystem.WriteAllText("x:\debug.txt", String.Format("Stack out => {0}" & vbCrLf, nid), True)
#End If

            Return pr
        End Function

        Private Function FindMaxNodeId(nodes As List(Of BeliefNode)) As Integer
            Dim max As Integer = -1
            For Each node As BeliefNode In nodes
                If node.Id > max Then
                    max = node.Id
                End If
            Next
            Return max
        End Function
    End Class
End Namespace
