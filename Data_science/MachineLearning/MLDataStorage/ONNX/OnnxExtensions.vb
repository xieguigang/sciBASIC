' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxExtensions.vb
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.CNN

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

    End Module

End Namespace
