''' <summary>
''' 常微分方程求解结果
''' </summary>
Public Class ODESolution

    Private _times As New List(Of Double)()
    Private _states As New List(Of NVector)()

    ''' <summary>
    ''' 添加一个解点
    ''' </summary>
    Public Sub AddPoint(t As Double, y As NVector)
        _times.Add(t)
        _states.Add(New NVector(y))
    End Sub

    ''' <summary>
    ''' 获取时间点数量
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _times.Count
        End Get
    End Property

    ''' <summary>
    ''' 获取时间数组
    ''' </summary>
    Public ReadOnly Property Times As Double()
        Get
            Return _times.ToArray()
        End Get
    End Property

    ''' <summary>
    ''' 获取指定索引的状态
    ''' </summary>
    Default Public ReadOnly Property Item(index As Integer) As NVector
        Get
            Return _states(index)
        End Get
    End Property

    ''' <summary>
    ''' 获取指定分量的时间序列
    ''' </summary>
    Public Function GetComponent(componentIndex As Integer) As Double()
        Dim result(_times.Count - 1) As Double
        For i As Integer = 0 To _times.Count - 1
            result(i) = _states(i)(componentIndex)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 导出到CSV格式
    ''' </summary>
    Public Function ToCSV() As String
        If _times.Count = 0 Then Return String.Empty

        Dim sb As New Text.StringBuilder()
        Dim n As Integer = _states(0).Length

        ' 表头
        sb.Append("Time")
        For i As Integer = 0 To n - 1
            sb.Append($",y{i}")
        Next
        sb.AppendLine()

        ' 数据
        For i As Integer = 0 To _times.Count - 1
            sb.Append(_times(i).ToString("G10"))
            For j As Integer = 0 To n - 1
                sb.Append(",")
                sb.Append(_states(i)(j).ToString("G10"))
            Next
            sb.AppendLine()
        Next

        Return sb.ToString()
    End Function

End Class
