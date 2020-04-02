Imports System.Xml

Namespace MathML

    Module contentBuilder

        Public Function ToString(lambda As Apply) As String

        End Function

        ''' <summary>
        ''' 因为反序列化存在一个元素顺序的bug，所以在这里不可以通过反序列化来进行表达式的解析
        ''' </summary>
        ''' <param name="mathML"></param>
        ''' <returns></returns>
        Public Function ParseXml(mathML As XmlTextReader) As BinaryExpression

        End Function
    End Module
End Namespace