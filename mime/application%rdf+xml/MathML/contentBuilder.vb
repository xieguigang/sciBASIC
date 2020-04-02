Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
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
        Public Function ParseXml(mathML As XmlReader) As BinaryExpression
            mathML.MoveToContent()
            mathML.moveToElementName("lambda")
            mathML.moveToElementName("apply")

            mathML = XmlReader.Create(New MemoryStream(Encoding.UTF8.GetBytes(mathML.ReadInnerXml)))
            mathML.MoveToContent()

            Return mathML.parseInternal()
        End Function

        <Extension>
        Private Function parseInternal(mathML As XmlReader) As BinaryExpression
            Dim [operator] = mathML.Name
            Dim left, right As BinaryExpression

            mathML.moveToElementName("apply")


        End Function

        <Extension>
        Private Sub moveToElementName(xml As XmlReader, name As String)
            Do While xml.Name <> name AndAlso Not xml.EOF
                Call xml.Read()
            Loop
        End Sub
    End Module
End Namespace