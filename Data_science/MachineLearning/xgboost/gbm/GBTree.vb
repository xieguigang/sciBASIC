#Region "Microsoft.VisualBasic::8410467057d1bbc5a08d25f3776a9064, Data_science\MachineLearning\xgboost\gbm\GBTree.vb"

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

    '   Total Lines: 146
    '    Code Lines: 101 (69.18%)
    ' Comment Lines: 15 (10.27%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 30 (20.55%)
    '     File Size: 5.64 KB


    '     Class GBTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: pred, predict, predictLeaf, predictSingle, predPath
    ' 
    '         Sub: loadModel
    '         Class ModelParam
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: predBufferSize
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.tree
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util
Imports stdNum = System.Math

Namespace gbm

    ''' <summary>
    ''' Gradient boosted tree implementation.
    ''' </summary>
    <Serializable>
    Public Class GBTree
        Inherits GBBase

        Friend mparam As ModelParam
        Private trees As RegTree()
        Private tree_info As Integer()
        Friend _groupTrees As RegTree()()

        Friend Sub New()
            ' do nothing
        End Sub

        Public Overrides Sub loadModel(reader As ModelReader, with_pbuffer As Boolean)
            mparam = New ModelParam(reader)
            trees = New RegTree(mparam.num_trees - 1) {}

            For i = 0 To mparam.num_trees - 1
                trees(i) = New RegTree()
                trees(i).loadModel(reader)
            Next

            tree_info = If(mparam.num_trees > 0, reader.readIntArray(mparam.num_trees), New Integer(-1) {})

            If mparam.num_pbuffer <> 0 AndAlso with_pbuffer Then
                reader.skip(4 * mparam.predBufferSize())
                reader.skip(4 * mparam.predBufferSize())
            End If

            _groupTrees = New RegTree(mparam.num_output_group - 1)() {}

            For i = 0 To mparam.num_output_group - 1
                Dim treeCount = 0

                For j = 0 To tree_info.Length - 1

                    If tree_info(j) = i Then
                        treeCount += 1
                    End If
                Next

                _groupTrees(i) = New RegTree(treeCount - 1) {}
                treeCount = 0

                For j = 0 To tree_info.Length - 1

                    If tree_info(j) = i Then
                        _groupTrees(i)(stdNum.Min(Threading.Interlocked.Increment(treeCount), treeCount - 1)) = trees(j)
                    End If
                Next
            Next
        End Sub

        Public Overrides Function predict(feat As FVec, ntree_limit As Integer) As Double()
            Dim preds = New Double(mparam.num_output_group - 1) {}

            For gid = 0 To mparam.num_output_group - 1
                preds(gid) = pred(feat, gid, 0, ntree_limit)
            Next

            Return preds
        End Function

        Public Overrides Function predictSingle(feat As FVec, ntree_limit As Integer) As Double
            If mparam.num_output_group <> 1 Then
                Throw New InvalidOperationException("Can't invoke predictSingle() because this model outputs multiple values: " & mparam.num_output_group)
            End If

            Return pred(feat, 0, 0, ntree_limit)
        End Function

        Friend Overridable Function pred(feat As FVec, bst_group As Integer, root_index As Integer, ntree_limit As Integer) As Double
            Dim trees = _groupTrees(bst_group)
            Dim treeleft = If(ntree_limit = 0, trees.Length, stdNum.Min(ntree_limit, trees.Length))
            Dim psum As Double = 0

            For i = 0 To treeleft - 1
                psum += trees(i).getLeafValue(feat, root_index)
            Next

            Return psum
        End Function

        Public Overrides Function predictLeaf(feat As FVec, ntree_limit As Integer) As Integer()
            Return predPath(feat, 0, ntree_limit)
        End Function

        Friend Overridable Function predPath(feat As FVec, root_index As Integer, ntree_limit As Integer) As Integer()
            Dim treeleft = If(ntree_limit = 0, trees.Length, stdNum.Min(ntree_limit, trees.Length))
            Dim leafIndex = New Integer(treeleft - 1) {}

            For i = 0 To treeleft - 1
                leafIndex(i) = trees(i).getLeafIndex(feat, root_index)
            Next

            Return leafIndex
        End Function

        <Serializable>
        Friend Class ModelParam
            ' ! \brief number of trees 
            Friend ReadOnly num_trees As Integer
            ' ! \brief number of root: default 0, means single tree 
            Friend ReadOnly num_roots As Integer
            ' ! \brief number of features to be used by trees 
            Friend ReadOnly num_feature As Integer
            ' ! \brief size of predicton buffer allocated used for buffering 
            Friend ReadOnly num_pbuffer As Long
            ' !
            '  \brief how many output group a single instance can produce
            '   this affects the behavior of number of output we have:
            '     suppose we have n instance and k group, output will be k*n
            ' 
            Friend ReadOnly num_output_group As Integer
            ' ! \brief size of leaf vector needed in tree 
            Friend ReadOnly size_leaf_vector As Integer
            ' ! \brief reserved parameters 
            Friend ReadOnly reserved As Integer()

            Friend Sub New(reader As ModelReader)
                num_trees = reader.readInt()
                num_roots = reader.readInt()
                num_feature = reader.readInt()
                reader.readInt() ' read padding
                num_pbuffer = reader.readLong()
                num_output_group = reader.readInt()
                size_leaf_vector = reader.readInt()
                reserved = reader.readIntArray(31)
                reader.readInt() ' read padding
            End Sub

            Friend Overridable Function predBufferSize() As Long
                Return num_output_group * num_pbuffer * (size_leaf_vector + 1)
            End Function
        End Class
    End Class
End Namespace
