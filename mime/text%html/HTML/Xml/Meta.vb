#Region "Microsoft.VisualBasic::05d4c277fd9e7f964847fecdc4b29d87, mime\text%html\HTML\Xml\Meta.vb"

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

    '     Class CSS
    ' 
    '         Properties: id, style, type
    ' 
    '         Function: Generator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HTML.XmlMeta

    ''' <summary>
    ''' Html之中的CSS样式
    ''' </summary>
    Public Class CSS

        ''' <summary>
        ''' ``text/css``
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property type As String
            Get
                Return "text/css"
            End Get
            Set(value As String)
                ' ReadOnly, Do Nothing
            End Set
        End Property

        <XmlAttribute> Public Property id As String
        ''' <summary>
        ''' 具体的CSS内容
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property style As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function Generator(classId As IEnumerable(Of String), attrs As Dictionary(Of String, String)) As String
            Dim sb As New StringBuilder(String.Join(", ", classId.Select(Function(s) "." & s).ToArray))

            Call sb.AppendLine("{")
            For Each attr In attrs
                Call sb.AppendLine($"   {attr.Key}: {attr.Value};")
            Next
            Call sb.AppendLine("}")

            Return sb.ToString
        End Function
    End Class
End Namespace
