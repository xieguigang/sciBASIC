#Region "Microsoft.VisualBasic::c05213e2e579f274491f78d88e79b431, Data_science\MachineLearning\xgboost\TGBoost\TreeNode.vb"

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

    '   Total Lines: 199
    '    Code Lines: 150
    ' Comment Lines: 24
    '   Blank Lines: 25
    '     File Size: 7.91 KB


    '     Class TreeNode
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: get_best_feature_threshold_gain
    ' 
    '         Sub: clean_up, Grad_add, Grad_setter, Hess_add, Hess_setter
    '              (+2 Overloads) internal_node_setter, leaf_node_setter, num_sample_add, set_categorical_feature_best_split, update_best_split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'        about TreeNode.index, an example:
'                    1
'           2        3       4
'        5  6  7   8 9 10  11 12 13
'
'        index of the root node is 1,
'        its left child's index is 3*root.index-1,
'        its right child's index is 3*root.index+1,
'        the middle child is nan_child, its index is 3*root.index

Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Language

Namespace train
    Public Class TreeNode

        Public index As Integer
        Public depth As Integer
        Public feature_dim As Integer
        Public is_leaf As Boolean
        Public num_sample As Integer
        'the gradient/hessian sum of the samples fall into this tree node
        Public Grad As Double
        Public Hess As Double
        'for split finding, record the gradient/hessian sum of the left
        Public G_left As Double()
        Public H_left As Double()
        'when split finding, record the best threshold, gain, missing value's branch for each feature
        Private best_thresholds As Double()
        Private best_gains As Double()
        Private best_nan_go_to As Double()
        Public nan_go_to As Double
        'some data fall into this tree node
        'gradient sum, hessian sum of those with missing value for each feature
        Public Grad_missing As Double()
        Public Hess_missing As Double()
        'internal node
        Public split_feature As Integer
        Public split_threshold As Double
        Public split_left_child_catvalue As List(Of Double)
        Public nan_child As TreeNode
        Public left_child As TreeNode
        Public right_child As TreeNode
        'leaf node
        Friend leaf_score As Double
        'for categorical feature,store (col,(value,(grad_sum,hess_sum)))
        Public cat_feature_col_value_GH As New Dictionary(Of String, Dictionary(Of String, Double()))()
        Private cat_feature_col_leftcatvalue As New Dictionary(Of String, List(Of Integer))()

        Public Sub New(index As Integer, leaf_score As Double)
            'leaf node construct
            is_leaf = True
            Me.index = index
            Me.leaf_score = leaf_score
        End Sub

        Public Sub New(index As Integer, split_feature As Integer, split_threshold As Double, nan_go_to As Double)
            'internal node construct,numeric split feature
            is_leaf = False
            Me.index = index
            Me.split_feature = split_feature
            Me.split_threshold = split_threshold
            Me.nan_go_to = nan_go_to
        End Sub

        Public Sub New(index As Integer, split_feature As Integer, split_left_child_catvalue As List(Of Double), nan_go_to As Double)
            'internal node construct,categorical split feature
            is_leaf = False
            Me.index = index
            Me.split_feature = split_feature
            Me.split_left_child_catvalue = split_left_child_catvalue
            Me.nan_go_to = nan_go_to
        End Sub

        Public Sub New(index As Integer, depth As Integer, feature_dim As Integer, is_leaf As Boolean)
            Me.index = index
            Me.depth = depth
            Me.feature_dim = feature_dim
            Me.is_leaf = is_leaf
            G_left = New Double(feature_dim - 1) {}
            H_left = New Double(feature_dim - 1) {}
            best_thresholds = New Double(feature_dim - 1) {}
            best_gains = New Double(feature_dim - 1) {}
            best_nan_go_to = New Double(feature_dim - 1) {}
            Grad_missing = New Double(feature_dim - 1) {}
            Hess_missing = New Double(feature_dim - 1) {}
            Arrays.fill(best_gains, -Double.MaxValue)
        End Sub

        Public Overridable Sub Grad_add(value As Double)
            Grad += value
        End Sub

        Public Overridable Sub Hess_add(value As Double)
            Hess += value
        End Sub

        Public Overridable Sub num_sample_add(value As Double)
            num_sample += CInt(value)
        End Sub

        Public Overridable Sub Grad_setter(value As Double)
            Grad = value
        End Sub

        Public Overridable Sub Hess_setter(value As Double)
            Hess = value
        End Sub

        Public Overridable Sub update_best_split(col As Integer, threshold As Double, gain As Double, nan_go_to As Double)
            If gain > best_gains(col) Then
                best_gains(col) = gain
                best_thresholds(col) = threshold
                best_nan_go_to(col) = nan_go_to
            End If
        End Sub

        Public Overridable Sub set_categorical_feature_best_split(col As Integer, left_child_catvalue As List(Of Integer), gain As Double, nan_go_to As Double)
            best_gains(col) = gain
            best_nan_go_to(col) = nan_go_to

            SyncLock cat_feature_col_leftcatvalue
                cat_feature_col_leftcatvalue(col.ToString) = left_child_catvalue
            End SyncLock
        End Sub

        Public Overridable Function get_best_feature_threshold_gain() As List(Of Double)
            Dim best_feature = 0
            Dim max_gain = -Double.MaxValue

            For i = 0 To feature_dim - 1

                If best_gains(i) > max_gain Then
                    max_gain = best_gains(i)
                    best_feature = i
                End If
            Next
            'consider categorical feature
            Dim ret As New List(Of Double)()
            ret.Add(CDbl(best_feature))
            ret.Add(max_gain)
            ret.Add(best_nan_go_to(best_feature))

            If cat_feature_col_leftcatvalue.ContainsKey(best_feature.ToString) Then
                For Each catvalue As Double In cat_feature_col_leftcatvalue.GetValueOrNull(best_feature.ToString)
                    ret.Add(catvalue)
                Next
            Else
                ret.Add(best_thresholds(best_feature))
            End If

            Return ret
        End Function

        Public Overridable Sub internal_node_setter(feature As Double, threshold As Double, nan_go_to As Double, nan_child As TreeNode, left_child As TreeNode, right_child As TreeNode, is_leaf As Boolean)
            split_feature = CInt(feature)
            split_threshold = threshold
            Me.nan_go_to = nan_go_to
            Me.nan_child = nan_child
            Me.left_child = left_child
            Me.right_child = right_child
            Me.is_leaf = is_leaf
            clean_up()
        End Sub

        Public Overridable Sub internal_node_setter(feature As Double, left_child_catvalue As List(Of Double), nan_go_to As Double, nan_child As TreeNode, left_child As TreeNode, right_child As TreeNode, is_leaf As Boolean)
            split_feature = CInt(feature)
            split_left_child_catvalue = left_child_catvalue
            Me.nan_go_to = nan_go_to
            Me.nan_child = nan_child
            Me.left_child = left_child
            Me.right_child = right_child
            Me.is_leaf = is_leaf
            clean_up()
        End Sub

        Public Overridable Sub leaf_node_setter(leaf_score As Double, is_leaf As Boolean)
            Me.is_leaf = is_leaf
            Me.leaf_score = leaf_score
            clean_up()
        End Sub

        ''' <summary>
        ''' release memory
        ''' </summary>
        Private Sub clean_up()
            Erase best_thresholds
            Erase best_gains
            Erase best_nan_go_to
            Erase G_left
            Erase H_left

            cat_feature_col_value_GH.Clear()
            cat_feature_col_value_GH = Nothing
            cat_feature_col_leftcatvalue.Clear()
            cat_feature_col_leftcatvalue = Nothing
        End Sub
    End Class
End Namespace
