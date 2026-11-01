Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

''' <summary>
''' CSV 表格数据读取（训练 / 验证 / 测试）。
'''
''' 缺失值识别统一走 <see cref="IsMissingToken"/>，并且不再构造
''' <c>TrainData.feature_value_index</c>（每 (样本,特征) 一个两元素 <c>Single()</c> 对象，
''' 10 万 × 100 的数据集约 400MB）：训练期的列主序结构由
''' <c>ColumnBlock</c> 直接从原始矩阵构建。
''' </summary>
Public Module Tabular

    ''' <summary>
    ''' 常见缺失值记号
    ''' </summary>
    Private ReadOnly MISSING_TOKENS As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "",
        "na",
        "n/a",
        "nan",
        "null",
        "nil",
        "none",
        "?"
    }

    ''' <summary>
    ''' 判断单元格是否表示缺失值
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension>
    Public Function IsMissingToken(value As String) As Boolean
        If value Is Nothing Then
            Return True
        End If

        Return MISSING_TOKENS.Contains(value.Trim())
    End Function

    ''' <summary>
    ''' 解析特征值（缺失返回 <c>Nothing</c>）
    ''' </summary>
    Private Function parseFeature(value As String) As Single?
        If IsMissingToken(value) Then
            Return Nothing
        End If

        Return Single.Parse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Function parseLabel(value As String) As Double
        If IsMissingToken(value) Then
            Throw New ArgumentException($"标签列不能为缺失值：'{value}'")
        End If

        Return Double.Parse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

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
                    Dim value = parseFeature(strs(col))

                    data.origin_feature(row)(col) = If(value.HasValue, value.Value, TestData.NA)
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
                data.label(row) = parseLabel(strs(strs.Length - 1))

                For col = 0 To data.feature_dim - 1
                    Dim value = parseFeature(strs(col))

                    data.origin_feature(row)(col) = If(value.HasValue, value.Value, ValidationData.NA)
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

            ' 只扫描特征列：旧实现用含标签列的下标循环，若标签列名出现在
            ' categorical_features 列表中，会得到等于 feature_dim 的越界列下标。
            For i = 0 To data.feature_dim - 1

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

                    If IsMissingToken(strs(i)) Then
                        data.missing_count(i) = data.missing_count(i) + 1
                    End If
                Next
            End While
        End Using
    End Sub

    ''' <summary>
    ''' to obtain: label, missing_index, origin_feature
    '''
    ''' 不再构造 <c>feature_value_index</c>：训练期的列主序结构由 <c>ColumnBlock</c> 直接
    ''' 从 <c>origin_feature</c> 构建，省去约 400MB 的「每单元一个对象」开销。
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="file"></param>
    <Extension>
    Private Sub second_scan(data As TrainData, file As String)
        data.label = New Double(data.dataset_size - 1) {}
        data.missing_index = New Integer(data.feature_dim - 1)() {}

        For i = 0 To data.feature_dim - 1
            Dim cnt = data.missing_count(i)
            data.missing_index(i) = New Integer(cnt - 1) {}
        Next

        data.origin_feature = RectangularArray.Matrix(Of Single)(data.dataset_size, data.feature_dim)

        Using br As New StreamReader(file)
            br.ReadLine()

            Dim cur_missing_index = New Integer(data.feature_dim - 1) {}

            For row = 0 To data.dataset_size - 1
                Dim strs As String() = br.ReadLine().Split(","c)
                data.label(row) = parseLabel(strs(strs.Length - 1))

                For col = 0 To data.feature_dim - 1
                    Dim value = parseFeature(strs(col))

                    If value.HasValue Then
                        data.origin_feature(row)(col) = value.Value
                    Else
                        If cur_missing_index(col) < data.missing_index(col).Length Then
                            data.missing_index(col)(cur_missing_index(col)) = row
                        End If

                        cur_missing_index(col) += 1
                        data.origin_feature(row)(col) = TrainData.NA
                    End If
                Next
            Next
        End Using
    End Sub
#End Region

End Module
