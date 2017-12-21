Namespace ComponentModel.DataSourceModel

    Public Enum PropertyAccess As Byte
        NotSure = 0
        Readable = 2
        Writeable = 4
        ReadWrite = Readable And Writeable
    End Enum

    ''' <summary>
    ''' 在数据框数据映射操作之中是否忽略掉这个属性或者方法？
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class DataIgnoredAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' Class field reader
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tout"></typeparam>
    ''' <param name="[in]"></param>
    ''' <returns></returns>
    Public Delegate Function Projector(Of T, Tout)([in] As T) As Tout

End Namespace