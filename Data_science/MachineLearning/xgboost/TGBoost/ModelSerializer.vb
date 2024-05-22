#Region "Microsoft.VisualBasic::d8d8e8a7c25b9caf43c6bc2657b86b81, Data_science\MachineLearning\xgboost\TGBoost\ModelSerializer.vb"

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

    '   Total Lines: 220
    '    Code Lines: 152 (69.09%)
    ' Comment Lines: 31 (14.09%)
    '    - Xml Docs: 29.03%
    ' 
    '   Blank Lines: 37 (16.82%)
    '     File Size: 8.50 KB


    '     Class ModelSerializer
    ' 
    '         Function: load_model, save_model, serializeInternalNode, serializeLeafNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values

Namespace train
    ' 
    ' Serialize the GBM model into txt file, unserialize the txt file into GBM model.
    ' 
    ' the content format in the txt file:
    ' first_round_prediction
    ' tree[tree_index]:
    ' internal_node_index:[feature_name,feature_type,split value || split values],missing_go_to=0|1|2
    ' leaf_node_index:leaf=leaf_score
    ' 
    ' for example:
    ' 0.5000
    ' tree[0]:
    ' 1:[7,num,30.6000],missing_go_to=0
    ' 2:[9,cat,1,3,5],missing_go_to=1
    ' 4:leaf=0.3333
    ' 
    ' tree[1]:
    ' 1:[4,num,10.9000],missing_go_to=2
    ' 2:leaf=0.9900
    ' 

    Public Class ModelSerializer

        Private Shared Function serializeLeafNode(node As TreeNode) As String
            Dim sb As New StringBuilder()
            sb.Append(node.index)
            sb.Append(":leaf=")
            sb.Append(String.Format("{0:F6}", node.leaf_score))
            Return sb.ToString()
        End Function

        Private Shared Function serializeInternalNode(node As TreeNode) As String
            Dim sb As New StringBuilder()

            sb.Append(node.index)
            sb.Append(":[")
            sb.Append(node.split_feature & ",")

            If node.split_left_child_catvalue Is Nothing Then
                sb.Append("num,")
                sb.Append(String.Format("{0:F6}", node.split_threshold))
                sb.Append("],")
            Else
                sb.Append("cat")

                For Each catvalue As Double In node.split_left_child_catvalue
                    sb.Append("," & catvalue)
                Next

                sb.Append("],")
            End If

            If node.nan_go_to = 0 Then
                sb.Append("missing_go_to=0")
            ElseIf node.nan_go_to = 1 Then
                sb.Append("missing_go_to=1")
            ElseIf node.nan_go_to = 2 Then
                sb.Append("missing_go_to=2")
            Else

                If node.left_child.num_sample > node.right_child.num_sample Then
                    sb.Append("missing_go_to=1")
                Else
                    sb.Append("missing_go_to=2")
                End If
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Serialize the GBM model into txt file
        ''' </summary>
        ''' <param name="gbm"></param>
        ''' <returns></returns>
        Public Shared Iterator Function save_model(gbm As GBM) As IEnumerable(Of String)
            Dim first_round_predict As Double = gbm.first_round_pred
            Dim eta As Double = gbm.eta
            Dim loss As Loss = gbm.loss
            Dim trees As List(Of Tree) = gbm.trees

            Yield "first_round_predict=" & first_round_predict
            Yield "eta=" & eta

            If TypeOf loss Is LogisticLoss Then
                Yield "logloss"
            Else
                Yield "squareloss"
            End If

            For i As Integer = 1 To trees.Count
                Dim tree As Tree = trees(i - 1)
                Dim root As TreeNode = tree.root
                Dim queue As New List(Of TreeNode)()

                Yield "tree[" & i & "]:"

                queue.Add(root)

                While queue.Count > 0
                    Dim cur_level_num = queue.Count

                    While cur_level_num <> 0
                        cur_level_num -= 1
                        Dim node As TreeNode = queue.Poll

                        If node.is_leaf Then
                            Yield ModelSerializer.serializeLeafNode(node)
                        Else
                            Yield ModelSerializer.serializeInternalNode(node)
                            queue.Add(node.left_child)

                            If node.nan_child IsNot Nothing Then
                                queue.Add(node.nan_child)
                            End If

                            queue.Add(node.right_child)
                        End If
                    End While
                End While
            Next

            Yield "tree[end]"
        End Function

        ''' <summary>
        ''' unserialize the txt file into GBM model.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function load_model(txtLine As String()) As GBM
            Dim i As i32 = 0
            Dim first_round_predict As Double = Double.Parse(txtLine(++i).Split("="c)(1))
            Dim eta As Double = Double.Parse(txtLine(++i).Split("="c)(1))
            Dim loss As Loss = Nothing

            If txtLine(++i) = "logloss" Then
                loss = New LogisticLoss()
            Else
                loss = New SquareLoss()
            End If

            Dim trees As List(Of Tree) = New List(Of Tree)()
            Dim line As New Value(Of String)
            Dim map As Dictionary(Of Integer?, TreeNode) = New Dictionary(Of Integer?, TreeNode)()

            While Not (line = txtLine.ElementAtOrDefault(++i)) Is Nothing
                If line.StartsWith("tree") Then
                    'store this tree,clear map
                    If map.Count > 0 Then
                        Dim queue As New List(Of TreeNode)()
                        Dim root As TreeNode = map.GetValueOrNull(1)
                        queue.Add(root)

                        While queue.Count > 0
                            Dim cur_level_num = queue.Count

                            While cur_level_num <> 0
                                cur_level_num -= 1
                                Dim node As TreeNode = queue.Poll

                                If Not node.is_leaf Then
                                    node.left_child = map.GetValueOrNull(3 * node.index - 1)
                                    node.right_child = map.GetValueOrNull(3 * node.index + 1)
                                    queue.Add(node.left_child)
                                    queue.Add(node.right_child)

                                    If map.ContainsKey(3 * node.index) Then
                                        node.nan_child = map.GetValueOrNull(3 * node.index)
                                        queue.Add(node.nan_child)
                                    End If
                                End If
                            End While
                        End While

                        trees.Add(New Tree(root))
                        map.Clear()
                    End If
                Else
                    'store this node into map
                    Dim index = Integer.Parse(line.Split(":"c)(0))

                    If line.Split(":"c)(1).StartsWith("leaf") Then
                        Dim leaf_score = Double.Parse(line.Split(":"c)(CInt(1)).Split("="c)(1))
                        Dim node As TreeNode = New TreeNode(index, leaf_score)
                        map(index) = node
                    Else
                        Dim nan_go_to = Double.Parse(line.Split("="c)(1))
                        Dim split_info = line.Split(":"c)(1).Split("]"c)(0)
                        split_info = split_info.Substring(1)
                        Dim strs = split_info.Split(","c)
                        Dim split_feature = Integer.Parse(strs(0))

                        If strs(1).Equals("num") Then
                            Dim split_threshold = Double.Parse(strs(2))
                            Dim node As TreeNode = New TreeNode(index, split_feature, split_threshold, nan_go_to)
                            map(index) = node
                        Else
                            Dim split_left_child_catvalue As New List(Of Double)()

                            For j As Integer = 2 To strs.Length - 1
                                split_left_child_catvalue.Add(Double.Parse(strs(j)))
                            Next

                            Dim node As TreeNode = New TreeNode(index, split_feature, split_left_child_catvalue, nan_go_to)
                            map(index) = node
                        End If
                    End If
                End If
            End While

            Return New GBM(trees, loss, first_round_predict, eta)
        End Function
    End Class
End Namespace
