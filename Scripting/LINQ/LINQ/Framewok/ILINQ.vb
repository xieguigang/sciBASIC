Namespace Framework

    ''' <summary>
    ''' LINQ Entity
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ILINQCollection

        ''' <summary>
        ''' Get a Collection of the target LINQ entity from file object.(从文件对象获取目标LINQ实体对象的集合)
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetCollection(FilePath As String) As Object()

        ''' <summary>
        ''' Get the type information of the element object in the linq entity collection.
        ''' (获取LINQ实体集合中的元素对象的类型信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetEntityType() As System.Type
    End Interface
End Namespace