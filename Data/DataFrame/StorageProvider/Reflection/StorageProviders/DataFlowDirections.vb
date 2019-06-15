Namespace StorageProvider.Reflection

    Public Enum DataFlowDirections
        ''' <summary>
        ''' 需要从对象之中读取数据，需要将数据写入文件的时候使用
        ''' </summary>
        ''' <remarks></remarks>
        ReadDataFromObject
        ''' <summary>
        ''' 需要相对象写入数据，从文件之中加载数据的时候使用
        ''' </summary>
        ''' <remarks></remarks>
        WriteDataToObject
    End Enum
End Namespace