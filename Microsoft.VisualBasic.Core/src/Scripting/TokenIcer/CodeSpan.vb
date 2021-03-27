Namespace Scripting.TokenIcer

    ''' <summary>
    ''' 目标Token对象在原始代码文本之中的定位位置
    ''' </summary>
    Public Class CodeSpan

        ''' <summary>
        ''' 源代码中的起始位置 
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property start As Integer
        ''' <summary>
        ''' 源代码中的结束位置
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property stops As Integer
        ''' <summary>
        ''' 在代码文本的行号
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property line As Integer

        Public Overrides Function ToString() As String
            Return $"[{start}, {[stops]}] at line {line}"
        End Function

    End Class
End Namespace