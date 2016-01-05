''' <summary>
''' 当所需要进行加载的数据的量非常大的时候，则可以使用本方法进行延时按需加载
''' </summary>
''' <typeparam name="TOutput"></typeparam>
''' <remarks></remarks>
Public Class LazyLoader(Of TOutput As Class, TSource)

    Public Delegate Function DataLoadMethod(source As TSource) As TOutput
    Public Delegate Function DataWriteMethod(source As TSource, obj As TOutput) As Boolean

    ''' <summary>
    ''' Gets the value from the data source <see cref="URL"></see>
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value As TOutput
        Get
            If _InternalData Is Nothing Then
                Call InternalLoadData()
            End If

            Return _InternalData
        End Get
        Set(value As TOutput)
            _InternalData = value
        End Set
    End Property

    Const DATA_LOADED_MESSAGE As String = "[LATE_LOADER_MSG]  {0} data load done!   //{1}; ({2})   ........{3}ms."

    Dim _url As TSource
    Dim _MethodInfo As DataLoadMethod
    Dim _InternalData As TOutput

    Private Sub InternalLoadData()
        Dim sw As Stopwatch = Stopwatch.StartNew
        _InternalData = _MethodInfo(_url)
        Call Console.WriteLine(DATA_LOADED_MESSAGE, URL.ToString.ToFileURL, _InternalData.GetType.FullName, _MethodInfo.ToString, sw.ElapsedMilliseconds)
    End Sub

    ''' <summary>
    ''' The data source.(数据源)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL As TSource
        Get
            Return _url
        End Get
        Set(value As TSource)
            _url = value
            _InternalData = Nothing
        End Set
    End Property

    Sub New(url As TSource, p As DataLoadMethod)
        _url = url
        _MethodInfo = p
    End Sub

    Public Overrides Function ToString() As String
        Return _url.ToString.ToFileURL
    End Function

    ''' <summary>
    ''' Write the data back onto the filesystem.(将数据回写进入文件系统之中)
    ''' </summary>
    ''' <param name="WriteMethod"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function WriteData(WriteMethod As DataWriteMethod) As Boolean
        Return WriteMethod(URL, _InternalData)
    End Function

    Public Shared Narrowing Operator CType(obj As LazyLoader(Of TOutput, TSource)) As TOutput
        Return obj.Value
    End Operator
End Class

Public Class Lazy(Of TOut)

    Protected _InternalDelegate As Func(Of TOut)
    Protected _InternalCache As TOut

    Public ReadOnly Property Value As TOut
        Get
            If _InternalCache Is Nothing Then
                _InternalCache = _InternalDelegate()
            End If

            Return _InternalCache
        End Get
    End Property

    Sub New(value As TOut)
        Me._InternalCache = value
    End Sub

    Sub New(Source As Func(Of TOut))
        _InternalDelegate = Source
    End Sub

    Public Overrides Function ToString() As String
        If _InternalCache Is Nothing Then
            Return GetType(TOut).FullName
        Else
            Return _InternalCache.ToString
        End If
    End Function
End Class