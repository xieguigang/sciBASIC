' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxExtensions.vb
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports MlpModel = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MultilayerPerceptron

Namespace ONNX

    ''' <summary>
    ''' ONNX 读写的便捷入口，覆盖最常见的三类用法：网络存盘、网络加载、模型结构查看。
    ''' </summary>
    ''' <remarks>
    ''' 这一层只是对 <see cref="OnnxExport"/> / <see cref="OnnxImport"/> / <see cref="OnnxModel"/> 的薄封装，
    ''' 目的是让调用方不必记住三种不同的入口函数。
    ''' </remarks>
    Public Module OnnxExtensions

        ''' <summary>
        ''' 把卷积网络导出为 ONNX 文件。
        ''' </summary>
        ''' <param name="cnn">待导出的网络。</param>
        ''' <param name="path">目标文件路径。</param>
        ''' <param name="options">导出选项。</param>
        <Extension>
        Public Sub SaveOnnx(cnn As ConvolutionalNN, path As String, Optional options As OnnxExportOptions = Nothing)
            Call OnnxExport.Save(cnn, path, options)
        End Sub

        ''' <summary>
        ''' 把卷积网络转换为 ONNX 模型对象（不写文件）。
        ''' </summary>
        ''' <param name="cnn">待导出的网络。</param>
        ''' <param name="options">导出选项。</param>
        <Extension>
        Public Function ToOnnx(cnn As ConvolutionalNN, Optional options As OnnxExportOptions = Nothing) As OnnxModel
            Return OnnxExport.ToModel(cnn, options)
        End Function

        ''' <summary>
        ''' 从 ONNX 文件还原卷积网络。
        ''' </summary>
        ''' <param name="path">ONNX 文件路径。</param>
        ''' <param name="options">导入选项。</param>
        ''' <returns>卷积网络对象。</returns>
        Public Function ReadCNN(path As String, Optional options As OnnxImportOptions = Nothing) As ConvolutionalNN
            Return OnnxImport.Load(path, options)
        End Function

        ''' <summary>
        ''' 读取 ONNX 文件（只解析结构，不做推理）。
        ''' </summary>
        ''' <param name="path">ONNX 文件路径。</param>
        ''' <returns>模型对象。</returns>
        Public Function ReadModel(path As String) As OnnxModel
            Return OnnxModel.Load(path)
        End Function

        ''' <summary>把多层感知机导出为 ONNX 文件。</summary>
        <Extension>
        Public Sub SaveOnnx(mlp As MlpModel, path As String, Optional options As OnnxExportOptions = Nothing)
            Call OnnxMlpExport.Save(mlp, path, options)
        End Sub

        ''' <summary>把多层感知机转换为 ONNX 模型对象（不写文件）。</summary>
        <Extension>
        Public Function ToOnnx(mlp As MlpModel, Optional options As OnnxExportOptions = Nothing) As OnnxModel
            Return OnnxMlpExport.ToModel(mlp, options)
        End Function

        ''' <summary>从 ONNX 文件还原多层感知机。</summary>
        Public Function ReadMLP(path As String, Optional options As OnnxImportOptions = Nothing) As MlpModel
            Return OnnxMlpImport.Load(path, options)
        End Function

        ''' <summary>使用内置求值器对一个单输入 ONNX 模型执行推理。</summary>
        Public Function EvaluateOnnx(model As OnnxModel, input As Double()) As Double()
            Return New OnnxGraphEvaluator(model).RunSingle(input)
        End Function

    End Module

End Namespace
