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

            For i = 0 To feature_dim - 1
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

            For i = 0 To feature_dim - 1
                ' for this feature, get its cutting index
                Dim list As New List(Of Integer)()
                Dim last_index = 0

                For j = 0 To attribute_list(i).Length - 1

                    If attribute_list(i)(j)(0) = attribute_list(i)(last_index)(0) Then
                        last_index = j
                    Else
                        list.Add(last_index)
                        last_index = j
                    End If
                Next

                ' for this feature,store its cutting threshold
                cutting_thresholds(i) = New Single(list.Count + 1 - 1) {}

                For t = 0 To cutting_thresholds(i).Length - 1 - 1
                    cutting_thresholds(i)(t) = attribute_list(i)(list(t))(0)
                Next

                cutting_thresholds(i)(list.Count) = attribute_list(i)(list(list.Count - 1) + 1)(0)

                ' for this feature,store inds of each interval
                ' list.size()+1 interval
                cutting_inds(i) = New Integer(list.Count + 1 - 1)() {}
                list.Insert(0, -1)
                list.Add(attribute_list(i).Length - 1)

                For k = 0 To cutting_inds(i).Length - 1
                    Dim start_ind As Integer = list(k) + 1
                    Dim end_ind = list(k + 1)

                    cutting_inds(i)(k) = New Integer(end_ind - start_ind + 1 - 1) {}

                    For m = 0 To cutting_inds(i)(k).Length - 1
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
