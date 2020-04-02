Imports System.IO
Imports System.Text
Imports System.Xml
Imports Microsoft.VisualBasic.Language

Namespace MathML

    Public Class BinaryExpression

        Public Property [operator] As String

        Public Property applyleft As [Variant](Of BinaryExpression, String)
        Public Property applyright As [Variant](Of BinaryExpression, String)

        Public Overrides Function ToString() As String
            Dim left As String
            Dim right As String

            If applyleft Like GetType(String) Then
                left = applyleft.TryCast(Of String)
            Else
                left = $"( {applyleft.TryCast(Of BinaryExpression).ToString} )"
            End If

            If applyright Like GetType(String) Then
                right = applyright.TryCast(Of String)
            Else
                right = $"( {applyright.TryCast(Of BinaryExpression).ToString} )"
            End If

            Return $"{left} {[operator]} {right}"
        End Function

        Public Shared Function FromMathML(xml As String) As BinaryExpression
            Using buffer As New MemoryStream(Encoding.UTF8.GetBytes(xml))
                Dim reader As XmlReader = XmlReader.Create(buffer)
                Dim exp As BinaryExpression = contentBuilder.ParseXml(mathML:=reader)

                Return exp
            End Using
        End Function

    End Class
End Namespace