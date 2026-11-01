#Region "Microsoft.VisualBasic::73195c0eca46598fc2f17c4b2e6202bb, Data_science\MachineLearning\xgboost\gbm\Dart.vb"

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

    '   Total Lines: 39
    '    Code Lines: 26 (66.67%)
    ' Comment Lines: 4 (10.26%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 9 (23.08%)
    '     File Size: 1.24 KB


    '     Class Dart
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: pred
    ' 
    '         Sub: loadModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util
Imports std = System.Math

Namespace gbm

    ''' <summary>
    ''' Gradient boosted DART tree implementation.
    ''' </summary>
    <Serializable>
    Public Class Dart : Inherits GBTree

        Private weightDrop As Single()

        Friend Sub New()
            ' do nothing
        End Sub

        Public Overrides Sub loadModel(reader As ModelReader, with_pbuffer As Boolean)
            MyBase.loadModel(reader, with_pbuffer)

            If mparam.num_trees <> 0 Then
                Dim size As Long = reader.readLong()
                weightDrop = reader.readFloatArray(size)
            End If
        End Sub

        Friend Overrides Function pred(feat As FVec, bst_group As Integer, root_index As Integer, ntree_limit As Integer) As Double
            Dim trees = _groupTrees(bst_group)
            Dim treeleft = If(ntree_limit = 0, trees.Length, std.Min(ntree_limit, trees.Length))
            Dim psum As Double = 0
            ' dropout 权重是按**全局树序**保存的：旧实现直接用组内局部下标 i 取值，
            ' 在多输出组（num_class > 1）时会取错权重。
            Dim globalIndex = If(_groupTreeIndex Is Nothing, Nothing, _groupTreeIndex(bst_group))

            For i = 0 To treeleft - 1
                Dim w As Double = 1

                If weightDrop IsNot Nothing Then
                    Dim idx As Integer = If(globalIndex Is Nothing, i, globalIndex(i))

                    If idx >= 0 AndAlso idx < weightDrop.Length Then
                        w = weightDrop(idx)
                    End If
                End If

                psum += w * trees(i).getLeafValue(feat, root_index)
            Next

            Return psum
        End Function
    End Class
End Namespace
