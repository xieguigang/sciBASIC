#Region "Microsoft.VisualBasic::dff8b8855b823b9787546b54b5850aca, mime\application%rtf\Omml\HTML.vb"

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

    '     Class HTML
    ' 
    '         Properties: Head
    ' 
    '         Function: InternalCreateDocument, SaveDocument
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization

Namespace Omml

    ''' <summary>
    ''' Omml: office microsoft word xml
    ''' </summary>
    ''' <remarks></remarks>
    <XmlRoot("html", Namespace:="http://www.w3.org/TR/REC-html40")>
    Public Class HTML

        Public Const WORD_XML_NAMESPACE As String = "xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"""

        Public Property Head As Head

        Public Function SaveDocument(path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Private Function InternalCreateDocument() As String
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
