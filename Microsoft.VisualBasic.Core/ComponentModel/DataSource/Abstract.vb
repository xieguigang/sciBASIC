#Region "Microsoft.VisualBasic::9e217abf5ba3f2929fef576bfb42c6e8, Microsoft.VisualBasic.Core\ComponentModel\DataSource\Abstract.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum PropertyAccess
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class DataIgnoredAttribute
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
