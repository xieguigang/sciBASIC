Imports System.Collections.Generic

Namespace Hamming

    ''' <summary>
    ''' Hamming 聚类的统一中间表示：把位串 / 类别行 / 数值行统一编码成
    ''' 从 0 开始的类别码矩阵，并记录每一列（特征）的取值基数。
    ''' 
    ''' 所有取值都可以用类别码表示之后，距离计算就退化为「同位置取值是否相同」
    ''' 的比较；当所有列的基数都不大于 2 时，还可以打包成
    ''' <see cref="BitPattern"/> 走位运算快速路径。
    ''' </summary>
    Public Class HammingData

        Private ReadOnly _codes As Integer()()
        Private ReadOnly _cardinality As Integer()
        Private ReadOnly _featureNames As String()

        ''' <summary>
        ''' 类别码矩阵，``codes(sample)(feature)``，取值从 0 开始连续编码。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Codes As Integer()()
            Get
                Return _codes
            End Get
        End Property

        ''' <summary>
        ''' 每一列（特征）的取值基数，用于逐位众数统计时分配计数器。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Cardinality As Integer()
            Get
                Return _cardinality
            End Get
        End Property

        ''' <summary>
        ''' 特征（列）名称，未知时为 ``Nothing``。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FeatureNames As String()
            Get
                Return _featureNames
            End Get
        End Property

        ''' <summary>
        ''' 样本数量（行数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Samples As Integer
            Get
                Return _codes.Length
            End Get
        End Property

        ''' <summary>
        ''' 特征维数（列数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Dimension As Integer
            Get
                Return If(_codes.Length = 0, 0, _codes(0).Length)
            End Get
        End Property

        ''' <summary>
        ''' 是否所有列的取值基数都不大于 2（即可以打包成位模式走 popcount 快速路径）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsBinary As Boolean
            Get
                For i As Integer = 0 To _cardinality.Length - 1
                    If _cardinality(i) > 2 Then
                        Return False
                    End If
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' 从类别码矩阵构造中间表示。
        ''' </summary>
        ''' <param name="codes">类别码矩阵，行主序</param>
        ''' <param name="cardinality">每一列的取值基数；缺省时按矩阵中的最大值推断</param>
        ''' <param name="featureNames">列名，可选</param>
        Sub New(codes As Integer()(), Optional cardinality As Integer() = Nothing, Optional featureNames As String() = Nothing)
            If codes Is Nothing Then
                Throw New ArgumentNullException(NameOf(codes))
            End If
            If codes.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(codes))
            End If

            Dim width As Integer = codes(0).Length

            If width = 0 Then
                Throw New ArgumentException("the input data set has zero features!", NameOf(codes))
            End If

            For i As Integer = 0 To codes.Length - 1
                If codes(i) Is Nothing OrElse codes(i).Length <> width Then
                    Throw New ArgumentException($"the row {i} is not consistent with the first row: {(If(codes(i) Is Nothing, 0, codes(i).Length))} <> {width}!")
                End If
            Next

            _codes = codes
            _featureNames = featureNames

            If cardinality Is Nothing Then
                _cardinality = InferCardinality(codes, width)
            ElseIf cardinality.Length <> width Then
                Throw New ArgumentException($"the cardinality vector has {cardinality.Length} values, but the data set has {width} features!", NameOf(cardinality))
            Else
                _cardinality = cardinality
            End If
        End Sub

        Private Shared Function InferCardinality(codes As Integer()(), width As Integer) As Integer()
            Dim cardinality As Integer() = New Integer(width - 1) {}

            For j As Integer = 0 To width - 1
                Dim max As Integer = 0

                For i As Integer = 0 To codes.Length - 1
                    If codes(i)(j) > max Then
                        max = codes(i)(j)
                    End If
                Next

                cardinality(j) = max + 1
            Next

            Return cardinality
        End Function

        ''' <summary>
        ''' 转换为位模式集合。仅当 <see cref="IsBinary"/> 为 ``True`` 时可用。
        ''' </summary>
        ''' <returns></returns>
        ''' <exception cref="InvalidOperationException">数据集中存在取值基数大于 2 的列</exception>
        Public Function ToPatterns() As BitPattern()
            If Not IsBinary Then
                Throw New InvalidOperationException("the data set contains features with more than two categories, it cannot be packed as binary patterns!")
            End If

            Dim patterns As BitPattern() = New BitPattern(_codes.Length - 1) {}

            For i As Integer = 0 To _codes.Length - 1
                patterns(i) = BitPattern.FromIntegers(_codes(i))
            Next

            Return patterns
        End Function

        ''' <summary>
        ''' 回显为容易阅读的类别码矩阵文本。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return String.Join(" | ", _codes.Select(Function(row) String.Join(",", row.Select(Function(v) v.ToString))))
        End Function
    End Class

    ''' <summary>
    ''' 把外部的各种输入形式统一编码成 <see cref="HammingData"/>。
    ''' </summary>
    Public Module HammingEncoder

        ''' <summary>
        ''' 从 ``"10110010"`` 形式的二进制位串集合构造中间表示。
        ''' </summary>
        ''' <param name="rows">等长的二进制位串集合</param>
        ''' <returns></returns>
        Public Function FromBitStrings(rows As IEnumerable(Of String)) As HammingData
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If

            Dim bits As String() = rows.ToArray

            If bits.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(rows))
            End If

            Dim patterns As BitPattern() = New BitPattern(bits.Length - 1) {}

            For i As Integer = 0 To bits.Length - 1
                patterns(i) = BitPattern.FromString(bits(i))
            Next

            Return FromPatterns(patterns)
        End Function

        ''' <summary>
        ''' 从位模式集合构造中间表示（所有列的基数都为 2）。
        ''' </summary>
        ''' <param name="patterns">等长的位模式集合</param>
        ''' <returns></returns>
        Public Function FromPatterns(patterns As BitPattern()) As HammingData
            If patterns Is Nothing Then
                Throw New ArgumentNullException(NameOf(patterns))
            End If
            If patterns.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(patterns))
            End If

            Dim width As Integer = patterns(0).BitLength
            Dim codes As Integer()() = New Integer(patterns.Length - 1)() {}
            Dim cardinality As Integer() = New Integer(width - 1) {}

            For j As Integer = 0 To width - 1
                cardinality(j) = 1
            Next

            For i As Integer = 0 To patterns.Length - 1
                If patterns(i).BitLength <> width Then
                    Throw New ArgumentException($"the pattern {i} has {patterns(i).BitLength} bits, but the first pattern has {width} bits!", NameOf(patterns))
                End If

                codes(i) = New Integer(width - 1) {}

                For j As Integer = 0 To width - 1
                    If patterns(i).BitAt(j) Then
                        codes(i)(j) = 1
                        cardinality(j) = 2
                    End If
                Next
            Next

            Return New HammingData(codes, cardinality)
        End Function

        ''' <summary>
        ''' 从字符串类别行（定类变量）构造中间表示。
        ''' 
        ''' 每一列独立的取值字典按照**首次出现顺序**编码，因此同样的输入
        ''' 必然得到同样的类别码，便于结果复现。
        ''' </summary>
        ''' <param name="rows">类别取值矩阵，行主序</param>
        ''' <returns></returns>
        Public Function FromCategories(rows As String()()) As HammingData
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            If rows.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(rows))
            End If

            Dim width As Integer = If(rows(0) Is Nothing, 0, rows(0).Length)

            If width = 0 Then
                Throw New ArgumentException("the input data set has zero features!", NameOf(rows))
            End If

            Dim codes As Integer()() = New Integer(rows.Length - 1)() {}
            Dim cardinality As Integer() = New Integer(width - 1) {}

            For i As Integer = 0 To rows.Length - 1
                If rows(i) Is Nothing OrElse rows(i).Length <> width Then
                    Throw New ArgumentException($"the row {i} is not consistent with the first row!")
                End If

                codes(i) = New Integer(width - 1) {}
            Next

            For j As Integer = 0 To width - 1
                Dim values As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

                For i As Integer = 0 To rows.Length - 1
                    Dim value As String = If(rows(i)(j), "")

                    If Not values.ContainsKey(value) Then
                        values(value) = values.Count
                    End If

                    codes(i)(j) = values(value)
                Next

                cardinality(j) = values.Count
            Next

            Return New HammingData(codes, cardinality)
        End Function

        ''' <summary>
        ''' 从字符类别行（定类变量）构造中间表示。
        ''' </summary>
        ''' <param name="rows">字符类别矩阵，行主序</param>
        ''' <returns></returns>
        Public Function FromCategories(rows As Char()()) As HammingData
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If

            Dim buffer As String()() = New String(rows.Length - 1)() {}

            For i As Integer = 0 To rows.Length - 1
                If rows(i) Is Nothing Then
                    buffer(i) = Nothing
                Else
                    buffer(i) = rows(i).Select(Function(c) c.ToString).ToArray
                End If
            Next

            Return FromCategories(buffer)
        End Function

        ''' <summary>
        ''' 从数值矩阵构造中间表示。
        ''' </summary>
        ''' <param name="rows">数值矩阵，行主序</param>
        ''' <param name="binary">
        ''' 为 ``True``（缺省）时按照 <paramref name="threshold"/> 做二值化，等价于二值特征；
        ''' 为 ``False`` 时把每一列的相异取值排序后当作定类取值处理。
        ''' </param>
        ''' <param name="threshold">二值化阈值（严格大于阈值记为 1）</param>
        ''' <returns></returns>
        Public Function FromValues(rows As Double()(),
                                   Optional binary As Boolean = True,
                                   Optional threshold As Double = 0.5) As HammingData

            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            If rows.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(rows))
            End If

            Dim width As Integer = If(rows(0) Is Nothing, 0, rows(0).Length)

            If width = 0 Then
                Throw New ArgumentException("the input data set has zero features!", NameOf(rows))
            End If

            Dim codes As Integer()() = New Integer(rows.Length - 1)() {}
            Dim cardinality As Integer() = New Integer(width - 1) {}

            For i As Integer = 0 To rows.Length - 1
                If rows(i) Is Nothing OrElse rows(i).Length <> width Then
                    Throw New ArgumentException($"the row {i} is not consistent with the first row!")
                End If

                codes(i) = New Integer(width - 1) {}
            Next

            If binary Then
                For i As Integer = 0 To rows.Length - 1
                    For j As Integer = 0 To width - 1
                        codes(i)(j) = If(rows(i)(j) > threshold, 1, 0)
                    Next
                Next

                For j As Integer = 0 To width - 1
                    Dim max As Integer = 0

                    For i As Integer = 0 To rows.Length - 1
                        If codes(i)(j) > max Then
                            max = codes(i)(j)
                        End If
                    Next

                    cardinality(j) = max + 1
                Next
            Else
                For j As Integer = 0 To width - 1
                    Dim values As New List(Of Double)()

                    For i As Integer = 0 To rows.Length - 1
                        If Not values.Contains(rows(i)(j)) Then
                            Call values.Add(rows(i)(j))
                        End If
                    Next

                    Call values.Sort()

                    For i As Integer = 0 To rows.Length - 1
                        codes(i)(j) = values.IndexOf(rows(i)(j))
                    Next

                    cardinality(j) = values.Count
                Next
            End If

            Return New HammingData(codes, cardinality)
        End Function

        ''' <summary>
        ''' 从整数矩阵构造中间表示。
        ''' </summary>
        ''' <param name="rows">整数矩阵，行主序</param>
        ''' <param name="binary">为 ``True`` 时按「非 0 即 1」二值化，否则把相异取值排序后当作定类取值</param>
        ''' <returns></returns>
        Public Function FromValues(rows As Integer()(), Optional binary As Boolean = True) As HammingData
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If

            If binary Then
                Dim values As Double()() = New Double(rows.Length - 1)() {}

                For i As Integer = 0 To rows.Length - 1
                    If rows(i) Is Nothing Then
                        values(i) = Nothing
                    Else
                        values(i) = rows(i).Select(Function(v) If(v <> 0, 1.0, 0.0)).ToArray
                    End If
                Next

                Return FromValues(values, binary:=True, threshold:=0.5)
            Else
                Dim values As Double()() = New Double(rows.Length - 1)() {}

                For i As Integer = 0 To rows.Length - 1
                    If rows(i) Is Nothing Then
                        values(i) = Nothing
                    Else
                        values(i) = rows(i).Select(Function(v) CDbl(v)).ToArray
                    End If
                Next

                Return FromValues(values, binary:=False)
            End If
        End Function
    End Module
End Namespace
