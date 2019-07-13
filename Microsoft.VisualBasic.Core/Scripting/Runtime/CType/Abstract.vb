#Region "Microsoft.VisualBasic::1771c61acba8743dfc6cd73f25951ffe, Microsoft.VisualBasic.Core\Scripting\Runtime\CType\Abstract.vb"

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

    '     Interface IParser
    ' 
    '         Function: ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.Runtime

    ''' <summary>
    ''' Custom user object parser
    ''' </summary>
    Public Interface IParser

        ''' <summary>
        ''' 将目标对象序列化为文本字符串
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Function ToString(obj As Object) As String

        ''' <summary>
        ''' 从Csv文件之中所读取出来的字符串之中解析出目标对象
        ''' </summary>
        ''' <param name="content"></param>
        ''' <returns></returns>
        Function TryParse(content As String) As Object

    End Interface
End Namespace
