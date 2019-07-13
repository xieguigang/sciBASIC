#Region "Microsoft.VisualBasic::0d1b251284fda7fe774793bd92fae0ee, Microsoft.VisualBasic.Core\Serialization\Abstract.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization

    ''' <summary>
    ''' 数据类型转换方法的句柄对象
    ''' </summary>
    ''' <param name="data">源之中的数据，由于源是一个TEXT格式的数据文件，故而这里的数据类型为字符串，通过本句柄对象可以将字符串数据映射为其他的复杂数据类型</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Delegate Function IStringParser(data As String) As Object
    Public Delegate Function IStringParser(Of T)(data As String) As T
    Public Delegate Function IObjectBuilder(data As String, schema As Type) As Object

    ''' <summary>
    ''' 将目标对象序列化为文本字符串的字符串构造方法
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Delegate Function IStringBuilder(data As Object) As String

    Public Delegate Function IToString(Of T)(obj As T) As String

End Namespace
