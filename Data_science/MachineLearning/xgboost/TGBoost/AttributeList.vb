#Region "Microsoft.VisualBasic::28b0a00da3f3a8509077b01799e57940, Data_science\MachineLearning\xgboost\TGBoost\AttributeList.vb"

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

    '   Total Lines: 102
    '    Code Lines: 74
    ' Comment Lines: 7
    '   Blank Lines: 21
    '     File Size: 3.85 KB


    '     Class AttributeList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: clean_up, initialize_cutting_inds_thresholds, sort_attribute_list
    '         Class ComparatorAnonymousInnerClass
    ' 
    '             Function: compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace train

    Public Class AttributeList

        Public feature_dim As Integer
        Private attribute_list As Single()()()
        Public missing_value_attribute_list As Integer()()
        Public cutting_inds As Integer()()()
        Public cutting_thresholds As Single()()
        Public origin_feature As Single()()
        Public cat_features_cols As List(Of Integer)

        Public Sub New(data As TrainData)
            missing_value_attribute_list = data.missing_index
            feature_dim = data.feature_dim
            attribute_list = data.feature_value_index
            origin_feature = data.origin_feature
            cat_features_cols = data.cat_features_cols

            Call sort_attribute_list()
            Call initialize_cutting_inds_thresholds()
            Call clean_up()
        End Sub

        ''' <summary>
        ''' pre-sort: for each feature,sort (value,index) by the value
        ''' </summary>
        Private Sub sort_attribute_list()
            Dim comparer As New ComparatorAnonymousInnerClass

            For i As Integer = 0 To feature_dim - 1
                Array.Sort(attribute_list(i), comparer)
            Next
        End Sub

        Private Class ComparatorAnonymousInnerClass : Implements IComparer(Of Single())

            Public Overridable Function compare(a As Single(), b As Single()) As Integer Implements IComparer(Of Single()).Compare
                Return a(0).CompareTo(b(0))
            End Function
        End Class

        Private Sub initialize_cutting_inds_thresholds()
            cutting_inds = New Integer(feature_dim - 1)()() {}
            cutting_thresholds = New Single(feature_dim - 1)() {}

            For i As Integer = 0 To feature_dim - 1
                ' for this feature, get its cutting index
                Dim list As New List(Of Integer)()
                Dim last_index = 0

                For j As Integer = 0 To attribute_list(i).Length - 1
                    If attribute_list(i)(j)(0) = attribute_list(i)(last_index)(0) Then
                        last_index = j
                    Else
                        list.Add(last_index)
                        last_index = j
                    End If
                Next

                ' for this feature,store its cutting threshold
                cutting_thresholds(i) = New Single(list.Count + 1 - 1) {}

                For t As Integer = 0 To cutting_thresholds(i).Length - 1 - 1
                    cutting_thresholds(i)(t) = attribute_list(i)(list(t))(0)
                Next

                Dim index As Integer = list.Count - 1

                If index = -1 Then
                    index = 0
                Else
                    index = list(index) + 1
                End If

                cutting_thresholds(i)(list.Count) = attribute_list(i)(index)(0)

                ' for this feature,store inds of each interval
                ' list.size()+1 interval
                cutting_inds(i) = New Integer(list.Count + 1 - 1)() {}
                list.Insert(0, -1)
                list.Add(attribute_list(i).Length - 1)

                For k As Integer = 0 To cutting_inds(i).Length - 1
                    Dim start_ind As Integer = list(k) + 1
                    Dim end_ind = list(k + 1)

                    cutting_inds(i)(k) = New Integer(end_ind - start_ind + 1 - 1) {}

                    For m As Integer = 0 To cutting_inds(i)(k).Length - 1
                        cutting_inds(i)(k)(m) = CInt(attribute_list(i)(start_ind + m)(1))
                    Next
                Next
            Next
        End Sub

        Private Sub clean_up()
            Erase attribute_list
            attribute_list = Nothing
        End Sub
    End Class
End Namespace
