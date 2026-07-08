Imports System.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdf = System.Math

''' <summary>
''' 每维度特征的 z-score 标准化器。
''' 
''' 训练阶段按特征维计算均值/标准差，推理阶段复用同一套参数，
''' 解决各维度特征量纲差异大导致的梯度不稳定问题。
''' 零方差维度标准差置 1，避免除零。
''' </summary>
Public Class Standardizer

    Public Property mean As Double()
    Public Property std As Double()

    Sub New()
    End Sub

    ''' <summary>
    ''' 用一组训练样本拟合均值与标准差
    ''' </summary>
    Public Sub Fit(samples As IEnumerable(Of Double()))
        Dim list = samples.ToList()

        If list.Count = 0 Then
            mean = Nothing
            std = Nothing
            Return
        End If

        Dim dimSize = list(0).Length
        mean = New Double(dimSize - 1) {}
        std = New Double(dimSize - 1) {}

        For d As Integer = 0 To dimSize - 1
            Dim m = list.Average(Function(v) v(d))
            Dim variance = list.Average(Function(v) (v(d) - m) * (v(d) - m))
            mean(d) = m
            std(d) = If(variance <= 0, 1.0, stdf.Sqrt(variance))
        Next
    End Sub

    ''' <summary>
    ''' 对单个特征向量做标准化：(x - mean) / std
    ''' </summary>
    Public Function Transform(input As Double()) As Double()
        If mean Is Nothing OrElse std Is Nothing Then
            Return input
        End If

        Dim out = New Double(input.Length - 1) {}

        For i As Integer = 0 To input.Length - 1
            out(i) = (input(i) - mean(i)) / std(i)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 保存标准化参数到 JSON 文件
    ''' </summary>
    Public Sub Save(path As String)
        Call File.WriteAllText(path, Me.GetJson)
    End Sub

    ''' <summary>
    ''' 从 JSON 文件加载标准化参数
    ''' </summary>
    Public Shared Function Load(path As String) As Standardizer
        Dim snap = File.ReadAllText(path).LoadJSON(Of Standardizer)
        Return snap
    End Function

End Class
