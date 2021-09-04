Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Public Module Conversion

    <Extension>
    Public Function ToTrainingSet(matrix As DoubleTagged(Of Single())(), columns As String(), categorical_features As IEnumerable(Of String)) As TrainData
        Dim data As New TrainData(categorical_features)
        Dim rowLine As Single()

        data.feature_dim = matrix(Scan0).Value.Length

        For i As Integer = 0 To data.feature_dim - 1
            If data.cat_features_names.Contains(columns(i)) Then
                data.cat_features_cols.Add(i)
            End If
        Next

        For i As Integer = 0 To data.feature_dim - 1
            data.missing_count.Add(0)
        Next

        data.dataset_size = 0

        For Each line As DoubleTagged(Of Single()) In matrix
            data.dataset_size += 1
            rowLine = line.Value

            For i = 0 To data.feature_dim - 1
                If rowLine(i) = TrainData.NA OrElse rowLine(i).IsNaNImaginary Then
                    data.missing_count(i) = data.missing_count(i) + 1
                End If
            Next
        Next

        data.label = New Double(data.dataset_size - 1) {}
        data.missing_index = New Integer(data.feature_dim - 1)() {}
        data.feature_value_index = New Single(data.feature_dim - 1)()() {}

        For i As Integer = 0 To data.feature_dim - 1
            Dim cnt = data.missing_count(i)
            data.missing_index(i) = New Integer(cnt - 1) {}
            data.feature_value_index(i) = MAT(Of Single)(data.dataset_size - cnt, 2)
        Next

        data.origin_feature = MAT(Of Single)(data.dataset_size, data.feature_dim)

        Dim cur_index = New Integer(data.feature_dim - 1) {}
        Dim cur_missing_index = New Integer(data.feature_dim - 1) {}

        Arrays.fill(cur_index, 0)
        Arrays.fill(cur_missing_index, 0)

        For row As Integer = 0 To data.dataset_size - 1
            data.label(row) = matrix(row).Tag
            rowLine = matrix(row).Value

            For col = 0 To data.feature_dim - 1
                If rowLine(col) = TrainData.NA OrElse rowLine(col).IsNaNImaginary Then
                    data.missing_index(col)(cur_missing_index(col)) = row
                    cur_missing_index(col) += 1
                    data.origin_feature(row)(col) = TrainData.NA
                Else
                    data.feature_value_index(col)(cur_index(col))(0) = rowLine(col)
                    data.feature_value_index(col)(cur_index(col))(1) = row
                    cur_index(col) += 1
                    data.origin_feature(row)(col) = rowLine(col)
                End If
            Next
        Next

        Return data
    End Function

    <Extension>
    Public Function ToValidateSet(matrix As DoubleTagged(Of Single())()) As ValidationData
        Dim data As New ValidationData
        Dim rowLine As Single()

        data.feature_dim = matrix(Scan0).Value.Length
        data.dataset_size = matrix.Length
        data.label = New Double(data.dataset_size - 1) {}
        data.origin_feature = MAT(Of Single)(data.dataset_size, data.feature_dim)

        For row = 0 To data.dataset_size - 1
            rowLine = matrix(row).Value
            data.label(row) = matrix(row).Tag

            For col = 0 To data.feature_dim - 1
                If rowLine(col) = ValidationData.NA OrElse rowLine(col).IsNaNImaginary Then
                    data.origin_feature(row)(col) = ValidationData.NA
                Else
                    data.origin_feature(row)(col) = rowLine(col)
                End If
            Next
        Next

        Return data
    End Function

    <Extension>
    Public Function ToTestDataSet(matrix As Single()()) As TestData
        Return New TestData With {
           .dataset_size = matrix.Length,
           .feature_dim = matrix(Scan0).Length,
           .origin_feature = matrix
        }
    End Function
End Module
