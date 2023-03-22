Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Text

Namespace Turtle

    ''' <summary>
    ''' A simple key-value pair tuple data
    ''' </summary>
    Public Class ttl_property

        <XmlAttribute>
        Public Property subject As String
        <XmlText>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return $"{subject}: {value}"
        End Function

        Public Shared Iterator Function LoadTuples(file As Stream) As IEnumerable(Of ttl_property)
            Dim line As Value(Of String) = ""
            Dim reader As New StreamReader(file)
            Dim data As NamedValue(Of String)
            Dim subj As String
            Dim value As String

            Do While (line = reader.ReadLine) IsNot Nothing
                If line.Value.Length = 0 OrElse line.StartsWith("@prefix") Then
                    Continue Do
                End If

                data = line.GetTagValue(vbTab, trim:=False)
                subj = data.Name
                data = data.Value.GetTagValue(vbTab)
                value = data.Value.Trim("."c, " "c, ASCII.TAB)

                Yield New ttl_property With {
                    .subject = subj,
                    .value = value
                }
            Loop
        End Function

    End Class
End Namespace