'parse the csv file, get features and label. the format: feature1,feature2,...,label
'first scan, get the feature dimension, dataset size, count of missing value for each feature
'second scan, get each feature's (value,index) and missing value indexes
'if we use ArrayList,only one scanning is needed, but it is memory consumption

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java

Namespace train
    Public Class Data
        'we use -Double.MAX_VALUE to represent missing value
        Public Shared NULL As Single = -Single.MaxValue
    End Class

    Public Class TrainData : Inherits Data

        Public feature_value_index As Single()()()
        Public label As Double()
        Public missing_index As Integer()()
        Public feature_dim As Integer
        Public dataset_size As Integer
        Private missing_count As List(Of Integer?) = New List(Of Integer?)()
        Public origin_feature As Single()()
        Private cat_features_names As List(Of String)
        Public cat_features_cols As List(Of Integer?) = New List(Of Integer?)()

        Public Sub New(file As String, categorical_features As List(Of String))
            cat_features_names = categorical_features
            first_scan(file)
            second_scan(file)
        End Sub

        'to obtain: feature_dim, dataset_size,missing_count,cat_features_dim
        Private Sub first_scan(file As String)
            Try
                Dim br As StreamReader = New StreamReader(file)
                Dim header As String = br.ReadLine()
                Dim columns = header.Split(","c)
                feature_dim = columns.Length - 1

                For i = 0 To columns.Length - 1

                    If cat_features_names.Contains(columns(i)) Then
                        cat_features_cols.Add(i)
                    End If
                Next

                For i = 0 To feature_dim - 1
                    missing_count.Add(0)
                Next

                Dim line As New Value(Of String)
                dataset_size = 0

                While Not (line = br.ReadLine()) Is Nothing
                    Dim strs = line.Split(","c)
                    dataset_size += 1

                    For i = 0 To feature_dim - 1

                        If strs(i).Equals("") Then
                            missing_count(i) = missing_count(i) + 1
                        End If
                    Next
                End While

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub

        'to obtain:feature_value_index,label,missing_index,origin_feature,cat_features_values
        Private Sub second_scan(file As String)
            label = New Double(dataset_size - 1) {}
            missing_index = New Integer(feature_dim - 1)() {}
            feature_value_index = New Single(feature_dim - 1)()() {}

            For i = 0 To feature_dim - 1
                Dim cnt = missing_count(i).Value
                missing_index(i) = New Integer(cnt - 1) {}
                feature_value_index(i) = MAT(Of Single)(dataset_size - cnt, 2)
            Next

            origin_feature = MAT(Of Single)(dataset_size, feature_dim)

            Try
                Dim br As StreamReader = New StreamReader(file)
                br.ReadLine()
                Dim cur_index = New Integer(feature_dim - 1) {}
                Dim cur_missing_index = New Integer(feature_dim - 1) {}
                Arrays.fill(cur_index, 0)
                Arrays.fill(cur_missing_index, 0)

                For row = 0 To dataset_size - 1
                    Dim strs As String() = br.ReadLine().Split(",")
                    label(row) = Single.Parse(strs(strs.Length - 1))

                    For col = 0 To feature_dim - 1

                        If strs(col).Equals("") Then
                            missing_index(col)(cur_missing_index(col)) = row
                            cur_missing_index(col) += 1
                            origin_feature(row)(col) = Data.NULL
                        Else
                            feature_value_index(col)(cur_index(col))(0) = Single.Parse(strs(col))
                            feature_value_index(col)(cur_index(col))(1) = row
                            cur_index(col) += 1
                            origin_feature(row)(col) = Single.Parse(strs(col))
                        End If
                    Next
                Next

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub
    End Class

    Friend Class ValidationData
        Inherits Data

        Public feature_dim As Integer
        Public dataset_size As Integer
        Public origin_feature As Single()()
        Public label As Double()

        Public Sub New(file As String)
            first_scan(file)
            second_scan(file)
        End Sub

        Private Sub first_scan(file As String)
            Try
                Dim br As StreamReader = New StreamReader(file)
                Dim header As String = br.ReadLine()
                feature_dim = header.Split(","c).Length - 1
                Dim line As New Value(Of String)
                dataset_size = 0

                While Not (line = br.ReadLine()) Is Nothing
                    dataset_size += 1
                End While

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub

        Private Sub second_scan(file As String)
            label = New Double(dataset_size - 1) {}
            origin_feature = MAT(Of Single)(dataset_size, feature_dim)

            Try
                Dim br As StreamReader = New StreamReader(file)
                br.ReadLine()

                For row = 0 To dataset_size - 1
                    Dim strs As String() = br.ReadLine().Split(",")
                    label(row) = Single.Parse(strs(strs.Length - 1))

                    For col = 0 To feature_dim - 1

                        If strs(col).Equals("") Then
                            origin_feature(row)(col) = Data.NULL
                        Else
                            origin_feature(row)(col) = Single.Parse(strs(col))
                        End If
                    Next
                Next

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub
    End Class

    Friend Class TestData
        Inherits Data

        Public feature_dim As Integer
        Public dataset_size As Integer
        Public origin_feature As Single()()

        Public Sub New(file As String)
            first_scan(file)
            second_scan(file)
        End Sub

        Private Sub first_scan(file As String)
            Try
                Dim br As StreamReader = New StreamReader(file)
                Dim header As String = br.ReadLine()
                feature_dim = header.Split(","c).Length
                Dim line As New Value(Of String)
                dataset_size = 0

                While Not (line = br.ReadLine()) Is Nothing
                    dataset_size += 1
                End While

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub

        Private Sub second_scan(file As String)
            origin_feature = MAT(Of Single)(dataset_size, feature_dim)

            Try
                Dim br As StreamReader = New StreamReader(file)
                br.ReadLine()

                For row = 0 To dataset_size - 1
                    Dim strs As String() = br.ReadLine().Split(",")

                    For col = 0 To feature_dim - 1

                        If strs(col).Equals("") Then
                            origin_feature(row)(col) = Data.NULL
                        Else
                            origin_feature(row)(col) = Single.Parse(strs(col))
                        End If
                    Next
                Next

            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try
        End Sub
    End Class
End Namespace
