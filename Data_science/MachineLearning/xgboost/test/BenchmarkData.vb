Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports sysmath = System.Math

''' <summary>
''' 确定性合成数据集。
''' </summary>
Public Class SyntheticSet

    ''' <summary>
    ''' 全部样本的特征矩阵
    ''' </summary>
    Public Property Rows As Single()()
    ''' <summary>
    ''' 全部样本的标签（0/1）
    ''' </summary>
    Public Property Labels As Double()
    Public Property Columns As String()
    Public Property Categorical As String()

    Public Property Train As TrainData
    Public Property Validation As ValidationData
    ''' <summary>
    ''' 训练集部分（前 1 - validationRatio 的行）
    ''' </summary>
    Public Property TrainRows As Single()()
    Public Property TrainLabels As Double()
    ''' <summary>
    ''' 验证集部分
    ''' </summary>
    Public Property ValidationRows As Single()()
    Public Property ValidationLabels As Double()
End Class

''' <summary>
''' 合成数据生成器：可注入缺失率、类别列比例与随机种子，保证回归可复现。
''' </summary>
Public Module BenchmarkData

    ''' <summary>
    ''' 类别取值刻意使用**非整数编码**（0.5 / 1.5 / 2.5 / 3.5）：
    ''' 旧实现会经过 <c>CStr -&gt; CDbl -&gt; CInt</c> 截断，把 0.5 / 1.5 折叠到同一整数，
    ''' 从而让训练期与预测期走进不同分支。
    ''' </summary>
    Public ReadOnly Property CategoryLevels As Double() = {0.5R, 1.5R, 2.5R, 3.5R}

    Public Function Create(n As Integer, featureCount As Integer, catColumns As Integer(),
                           missingRate As Double, seed As Integer,
                           Optional validationRatio As Double = 0.25) As SyntheticSet
        Dim rand As New Random(seed)
        Dim rows As Single()() = New Single(n - 1)() {}
        Dim labels As Double() = New Double(n - 1) {}
        Dim catSet As New HashSet(Of Integer)(catColumns)
        Dim catColumn As Integer = If(catColumns IsNot Nothing AndAlso catColumns.Length > 0, catColumns(0), 0)

        For i As Integer = 0 To n - 1
            Dim v As Single() = New Single(featureCount - 1) {}

            For f As Integer = 0 To featureCount - 1

                If rand.NextDouble() < missingRate Then
                    v(f) = Single.NaN
                ElseIf catSet.Contains(f) Then
                    v(f) = CSng(CategoryLevels(rand.Next(CategoryLevels.Length)))
                Else
                    v(f) = CSng(rand.NextDouble() * 2 - 1)
                End If
            Next

            Dim score As Double =
                1.2 * num(v, 0) + 0.7 * num(v, 1) - 0.5 * num(v, 2) +
                0.3 * num(v, 3) - 0.2 * num(v, 4)

            ' 类别列的效应：高档编码带来更强的正贡献
            If catSet.Contains(catColumn) AndAlso Not Single.IsNaN(v(catColumn)) AndAlso v(catColumn) >= 2.5 Then
                score += 1.2
            Else
                score -= 0.3
            End If

            score += (rand.NextDouble() - 0.5) * 0.4

            rows(i) = v
            labels(i) = If(score > 0, 1.0, 0.0)
        Next

        Dim columns As String() = New String(featureCount - 1) {}

        For f As Integer = 0 To featureCount - 1
            columns(f) = $"f{f}"
        Next

        Dim categorical As String() = New String(catColumns.Length - 1) {}

        For i As Integer = 0 To catColumns.Length - 1
            categorical(i) = columns(catColumns(i))
        Next

        Dim validSize As Integer = sysmath.Max(CInt(n * validationRatio), 1)
        Dim trainSize As Integer = n - validSize
        Dim trainTags As DoubleTagged(Of Single())() = New DoubleTagged(Of Single())(trainSize - 1) {}
        Dim validTags As DoubleTagged(Of Single())() = New DoubleTagged(Of Single())(validSize - 1) {}
        Dim trainMatrix As Single()() = New Single(trainSize - 1)() {}
        Dim trainLabel As Double() = New Double(trainSize - 1) {}
        Dim validMatrix As Single()() = New Single(validSize - 1)() {}
        Dim validLabels As Double() = New Double(validSize - 1) {}

        For i As Integer = 0 To trainSize - 1
            trainTags(i) = New DoubleTagged(Of Single())(labels(i), rows(i))
            trainMatrix(i) = rows(i)
            trainLabel(i) = labels(i)
        Next

        For i As Integer = 0 To validSize - 1
            Dim src As Integer = trainSize + i

            validTags(i) = New DoubleTagged(Of Single())(labels(src), rows(src))
            validMatrix(i) = rows(src)
            validLabels(i) = labels(src)
        Next

        Return New SyntheticSet With {
            .Rows = rows,
            .Labels = labels,
            .Columns = columns,
            .Categorical = categorical,
            .Train = Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Conversion.ToTrainingSet(trainTags, columns, categorical),
            .Validation = Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Conversion.ToValidateSet(validTags),
            .TrainRows = trainMatrix,
            .TrainLabels = trainLabel,
            .ValidationRows = validMatrix,
            .ValidationLabels = validLabels
        }
    End Function

    ''' <summary>
    ''' 生成一个「标签完全由类别列决定」的数据集，用于验证类别分裂路径是否真的被启用。
    ''' </summary>
    Public Function CreateCategoricalOnly(n As Integer, seed As Integer) As SyntheticSet
        Dim rand As New Random(seed)
        Dim rows As Single()() = New Single(n - 1)() {}
        Dim labels As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim level As Double = CategoryLevels(rand.Next(CategoryLevels.Length))
            Dim noise As Double = rand.NextDouble() * 0.1

            rows(i) = New Single() {CSng(level), CSng(noise)}
            ' 只有「高档编码」为正类
            labels(i) = If(level >= 2.5, 1.0, 0.0)
        Next

        Dim columns As String() = {"f0", "f1"}
        Dim categorical As String() = {"f0"}

        Return build(rows, labels, columns, categorical, n, rand)
    End Function

    Private Function build(rows As Single()(), labels As Double(), columns As String(),
                           categorical As String(), n As Integer, rand As Random) As SyntheticSet
        Dim validSize As Integer = sysmath.Max(n \ 4, 1)
        Dim trainSize As Integer = n - validSize
        Dim trainTags As DoubleTagged(Of Single())() = New DoubleTagged(Of Single())(trainSize - 1) {}
        Dim validTags As DoubleTagged(Of Single())() = New DoubleTagged(Of Single())(validSize - 1) {}
        Dim trainMatrix As Single()() = New Single(trainSize - 1)() {}
        Dim trainLabel As Double() = New Double(trainSize - 1) {}
        Dim validMatrix As Single()() = New Single(validSize - 1)() {}
        Dim validLabels As Double() = New Double(validSize - 1) {}

        For i As Integer = 0 To trainSize - 1
            trainTags(i) = New DoubleTagged(Of Single())(labels(i), rows(i))
            trainMatrix(i) = rows(i)
            trainLabel(i) = labels(i)
        Next

        For i As Integer = 0 To validSize - 1
            Dim src As Integer = trainSize + i

            validTags(i) = New DoubleTagged(Of Single())(labels(src), rows(src))
            validMatrix(i) = rows(src)
            validLabels(i) = labels(src)
        Next

        Return New SyntheticSet With {
            .Rows = rows,
            .Labels = labels,
            .Columns = columns,
            .Categorical = categorical,
            .Train = Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Conversion.ToTrainingSet(trainTags, columns, categorical),
            .Validation = Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet.Conversion.ToValidateSet(validTags),
            .TrainRows = trainMatrix,
            .TrainLabels = trainLabel,
            .ValidationRows = validMatrix,
            .ValidationLabels = validLabels
        }
    End Function

    Private Function num(v As Single(), index As Integer) As Double
        If index >= v.Length OrElse Single.IsNaN(v(index)) Then
            Return 0
        End If

        Return v(index)
    End Function
End Module
