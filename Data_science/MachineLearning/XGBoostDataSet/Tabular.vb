#Region "Microsoft.VisualBasic::437fdd296f49d14ae7d3c678dd574fdf, Data_science\MachineLearning\XGBoostDataSet\Tabular.vb"

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

    '   Total Lines: 208
    '    Code Lines: 154
    ' Comment Lines: 10
    '   Blank Lines: 44
    '     File Size: 7.11 KB


    ' Module Tabular
    ' 
    '     Function: ReadTestData, ReadTrainData, ReadValidationData
    ' 
    '     Sub: (+3 Overloads) first_scan, (+3 Overloads) second_scan
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Public Module Tabular

#Region "ReadTestData"

    Public Function ReadTestData(file As String) As TestData
        Dim data As New TestData

        Call data.first_scan(file)
        Call data.second_scan(file)

        Return data
    End Function

    <Extension>
    Private Sub first_scan(data As TestData, file As String)
        Using br As New StreamReader(file)
            Dim header As String = br.ReadLine()
            data.feature_dim = header.Split(","c).Length
            Dim line As New Value(Of String)
            data.dataset_size = 0

            While Not (line = br.ReadLine()) Is Nothing
                data.dataset_size += 1
            End While
        End Using
    End Sub

    <Extension>
    Private Sub second_scan(data As TestData, file As String)
        data.origin_feature = RectangularArray.Matrix(Of Single)(data.dataset_size, data.feature_dim)

        Using br As New StreamReader(file)
            br.ReadLine()

            For row = 0 To data.dataset_size - 1
                Dim strs As String() = br.ReadLine().Split(","c)

                For col = 0 To data.feature_dim - 1

                    If strs(col).Equals("") Then
                        data.origin_feature(row)(col) = TestData.NA
                    Else
                        data.origin_feature(row)(col) = Single.Parse(strs(col))
                    End If
                Next
            Next
        End Using
    End Sub
#End Region

#Region "ReadValidationData"

    Public Function ReadValidationData(file As String) As ValidationData
        Dim data As New ValidationData

        data.first_scan(file)
        data.second_scan(file)

        Return data
    End Function

    <Extension>
    Private Sub first_scan(data As ValidationData, file As String)
        Using br As New StreamReader(file)
            Dim header As String = br.ReadLine()
            data.feature_dim = header.Split(","c).Length - 1
            Dim line As New Value(Of String)
            data.dataset_size = 0

            While Not (line = br.ReadLine()) Is Nothing
                data.dataset_size += 1
            End While
        End Using
    End Sub

    <Extension>
    Private Sub second_scan(data As ValidationData, file As String)
        data.label = New Double(data.dataset_size - 1) {}
        data.origin_feature = RectangularArray.Matrix(Of Single)(data.dataset_size, data.feature_dim)

        Using br As New StreamReader(file)
            br.ReadLine()

            For row = 0 To data.dataset_size - 1
                Dim strs As String() = br.ReadLine().Split(","c)
                data.label(row) = Single.Parse(strs(strs.Length - 1))

                For col = 0 To data.feature_dim - 1

                    If strs(col).Equals("") Then
                        data.origin_feature(row)(col) = ValidationData.NA
                    Else
                        data.origin_feature(row)(col) = Single.Parse(strs(col))
                    End If
                Next
            Next
        End Using
    End Sub
#End Region

#Region "ReadTrainData"

    Public Function ReadTrainData(file As String, categorical_features As IEnumerable(Of String)) As TrainData
        Dim data As New TrainData(categorical_features)

        Call data.first_scan(file)
        Call data.second_scan(file)

        Return data
    End Function

    ''' <summary>
    ''' to obtain: feature_dim, dataset_size,missing_count,cat_features_dim
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="file"></param>
    <Extension>
    Private Sub first_scan(data As TrainData, file As String)
        Using br As New StreamReader(file)
            Dim header As String = br.ReadLine()
            Dim columns = header.Split(","c)
            data.feature_dim = columns.Length - 1

            For i = 0 To columns.Length - 1

                If data.cat_features_names.Contains(columns(i)) Then
                    data.cat_features_cols.Add(i)
                End If
            Next

            For i = 0 To data.feature_dim - 1
                data.missing_count.Add(0)
            Next

            Dim line As New Value(Of String)
            data.dataset_size = 0

            While Not (line = br.ReadLine()) Is Nothing
                Dim strs = line.Split(","c)
                data.dataset_size += 1

                For i = 0 To data.feature_dim - 1

                    If strs(i).Equals("") Then
                        data.missing_count(i) = data.missing_count(i) + 1
                    End If
                Next
            End While
        End Using
    End Sub

    ''' <summary>
    ''' to obtain:feature_value_index,label,missing_index,origin_feature,cat_features_values
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="file"></param>
    <Extension>
    Private Sub second_scan(data As TrainData, file As String)
        data.label = New Double(data.dataset_size - 1) {}
        data.missing_index = New Integer(data.feature_dim - 1)() {}
        data.feature_value_index = New Single(data.feature_dim - 1)()() {}

        For i = 0 To data.feature_dim - 1
            Dim cnt = data.missing_count(i)
            data.missing_index(i) = New Integer(cnt - 1) {}
            data.feature_value_index(i) = RectangularArray.Matrix(Of Single)(data.dataset_size - cnt, 2)
        Next

        data.origin_feature = RectangularArray.Matrix(Of Single)(data.dataset_size, data.feature_dim)


        Dim br As StreamReader = New StreamReader(file)
        br.ReadLine()
        Dim cur_index = New Integer(data.feature_dim - 1) {}
        Dim cur_missing_index = New Integer(data.feature_dim - 1) {}
        Arrays.fill(cur_index, 0)
        Arrays.fill(cur_missing_index, 0)

        For row = 0 To data.dataset_size - 1
            Dim strs As String() = br.ReadLine().Split(","c)
            data.label(row) = Single.Parse(strs(strs.Length - 1))

            For col = 0 To data.feature_dim - 1

                If strs(col).Equals("") Then
                    data.missing_index(col)(cur_missing_index(col)) = row
                    cur_missing_index(col) += 1
                    data.origin_feature(row)(col) = TrainData.NA
                Else
                    data.feature_value_index(col)(cur_index(col))(0) = Single.Parse(strs(col))
                    data.feature_value_index(col)(cur_index(col))(1) = row
                    cur_index(col) += 1
                    data.origin_feature(row)(col) = Single.Parse(strs(col))
                End If
            Next
        Next
    End Sub
#End Region

End Module
