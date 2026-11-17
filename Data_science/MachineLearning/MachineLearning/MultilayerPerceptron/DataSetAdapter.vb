' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\MultilayerPerceptron\DataSetAdapter.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace MultilayerPerceptron

    ''' <summary>
    ''' 多层感知机与项目现有归一化数据集抽象之间的适配层。
    ''' </summary>
    Public Module DataSetAdapter

        ''' <summary>
        ''' 使用归一化数据集训练模型；特征来自 <c>Sample.vector</c>，标签来自 <c>Sample.target</c>。
        ''' </summary>
        Public Sub Fit(model As MultilayerPerceptron, dataset As DataSet,
                       Optional options As TrainingOptions = Nothing)
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If dataset Is Nothing Then
                Throw New ArgumentNullException(NameOf(dataset))
            End If

            Dim samples As Sample() = GetSamples(dataset)
            Call model.Fit(Features(model, samples), Labels(model, samples), options)
        End Sub

        ''' <summary>
        ''' 使用样本数据集合训练模型；先转换为带归一化矩阵的标准 <see cref="DataSet"/>。
        ''' </summary>
        Public Sub Fit(model As MultilayerPerceptron, samples As IEnumerable(Of SampleData),
                       Optional options As TrainingOptions = Nothing)
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If samples Is Nothing Then
                Throw New ArgumentNullException(NameOf(samples))
            End If

            Dim list As SampleData() = samples.ToArray()

            If list.Length = 0 Then
                Throw New ArgumentException("训练样本集合不能为空")
            End If

            Call Fit(model, SampleData.CreateDataSet(list), options)
        End Sub

        ''' <summary>从数据集中提取特征矩阵，形状为 [样本数, 输入维度]。</summary>
        Public Function Features(model As MultilayerPerceptron, dataset As DataSet) As NumericMatrix
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If dataset Is Nothing Then
                Throw New ArgumentNullException(NameOf(dataset))
            End If

            Return Features(model, GetSamples(dataset))
        End Function

        ''' <summary>从数据集中提取标签矩阵；分类任务会自动把标量类别编号转换为 one-hot。</summary>
        Public Function Labels(model As MultilayerPerceptron, dataset As DataSet) As NumericMatrix
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If dataset Is Nothing Then
                Throw New ArgumentNullException(NameOf(dataset))
            End If

            Return Labels(model, GetSamples(dataset))
        End Function

        Private Function GetSamples(dataset As DataSet) As Sample()
            If dataset.DataSamples Is Nothing OrElse dataset.DataSamples.size = 0 Then
                Throw New ArgumentException("数据集中没有样本")
            End If

            If dataset.NormalizeMatrix Is Nothing Then
                Return dataset.DataSamples.items
            End If

            Return dataset.PopulateNormalizedSamples().ToArray()
        End Function

        Private Function Features(model As MultilayerPerceptron, samples As Sample()) As NumericMatrix
            Dim rows As Double()() = New Double(samples.Length - 1)() {}

            For i As Integer = 0 To samples.Length - 1
                Dim vector As Double() = samples(i).vector

                If vector Is Nothing OrElse vector.Length <> model.InputSize Then
                    Throw New ArgumentException($"第 {i} 个样本的特征维度应为 {model.InputSize}")
                End If

                rows(i) = DirectCast(vector.Clone(), Double())
            Next

            Return New NumericMatrix(rows)
        End Function

        Private Function Labels(model As MultilayerPerceptron, samples As Sample()) As NumericMatrix
            Dim rows As Double()() = New Double(samples.Length - 1)() {}

            For i As Integer = 0 To samples.Length - 1
                Dim target As Double() = samples(i).target
                Dim row As Double() = New Double(model.OutputSize - 1) {}

                If target Is Nothing Then
                    Throw New ArgumentException($"第 {i} 个样本缺少标签数据")
                End If

                If target.Length = model.OutputSize Then
                    Call Array.Copy(target, row, row.Length)
                ElseIf model.Task = MlpTask.Classification AndAlso target.Length = 1 Then
                    Dim index As Integer = CInt(std.Round(target(0)))

                    If index < 0 OrElse index >= model.OutputSize Then
                        Throw New ArgumentException($"第 {i} 个样本的类别编号 {index} 超出输出范围")
                    End If

                    row(index) = 1.0
                Else
                    Dim count As Integer = std.Min(target.Length, row.Length)
                    Call Array.Copy(target, row, count)
                End If

                rows(i) = row
            Next

            Return New NumericMatrix(rows)
        End Function

    End Module

    Partial Public Class MultilayerPerceptron

        ''' <summary>使用归一化数据集训练模型。</summary>
        Public Sub Fit(dataset As DataSet, Optional options As TrainingOptions = Nothing)
            Call DataSetAdapter.Fit(Me, dataset, options)
        End Sub

        ''' <summary>使用样本数据集合训练模型。</summary>
        Public Sub Fit(samples As IEnumerable(Of SampleData), Optional options As TrainingOptions = Nothing)
            Call DataSetAdapter.Fit(Me, samples, options)
        End Sub

    End Class

End Namespace
