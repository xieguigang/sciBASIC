Namespace StorageProvider

    Public Interface ISchema

        ''' <summary>
        ''' 从数据源之中解析出来得到的域列表
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer)
        Function GetOrdinal(name As String) As Integer
    End Interface
End Namespace