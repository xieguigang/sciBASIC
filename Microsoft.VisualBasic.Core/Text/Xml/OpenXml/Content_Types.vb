#Region "Microsoft.VisualBasic::7cd16e63540b8754b215831f05c7c152, Microsoft.VisualBasic.Core\Text\Xml\OpenXml\Content_Types.vb"

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

    '     Class ContentTypes
    ' 
    '         Properties: [Default], [Overrides]
    ' 
    '         Function: ToString
    ' 
    '     Structure Type
    ' 
    '         Properties: ContentType, Extension, PartName
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.OpenXml

    ''' <summary>
    ''' ``[Content_Types].xml``
    ''' </summary>
    ''' 
    <XmlRoot("Types", Namespace:="http://schemas.openxmlformats.org/package/2006/content-types")>
    Public Class ContentTypes

        <XmlElement> Public Property [Default] As Type()
        <XmlElement("Override")>
        Public Property [Overrides] As List(Of Type)

        Public Overrides Function ToString() As String
            Return [Overrides] _
                .Select(Function(t) t.PartName) _
                .ToArray _
                .GetJson
        End Function
    End Class

    Public Structure Type

        <XmlAttribute> Public Property Extension As String
        <XmlAttribute> Public Property ContentType As String
        <XmlAttribute> Public Property PartName As String

        Public Overrides Function ToString() As String
            If PartName.StringEmpty Then
                Return ContentType
            Else
                Return $"({PartName}) {ContentType}"
            End If
        End Function
    End Structure
End Namespace
