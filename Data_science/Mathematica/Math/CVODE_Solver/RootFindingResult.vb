''' <summary>
''' 根查找结果
''' </summary>
Public Class RootFindingResult
    ''' <summary>
    ''' 是否找到根
    ''' </summary>
    Public Property Found As Boolean

    ''' <summary>
    ''' 根所在的时间
    ''' </summary>
    Public Property RootTime As Double

    ''' <summary>
    ''' 根的索引
    ''' </summary>
    Public Property RootIndex As Integer

    ''' <summary>
    ''' 根处的状态
    ''' </summary>
    Public Property State As NVector
End Class