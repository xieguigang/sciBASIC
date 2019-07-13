#Region "Microsoft.VisualBasic::0f2d318c28f007ae235731f5caea7bc9, mime\text%html\HTML\Image.vb"

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

    '     Class Image
    ' 
    '         Properties: Align, Alt, Border, HSpace, source
    '                     Text
    ' 
    '         Function: GetImages, ResParser, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace HTML

    Public Class Image

        <XmlAttribute> Public Property Border As String
        <XmlAttribute> Public Property HSpace As String
        <XmlAttribute> Public Property Alt As String
        <XmlAttribute> Public Property source As String
        <XmlAttribute> Public Property Align As String

        Public Overrides Function ToString() As String
            Dim attrs As StringBuilder = New StringBuilder(1024)

            If Not String.IsNullOrEmpty(Border) Then Call attrs.Append($"{NameOf(Border)}=""{Border}"" ")
            If Not String.IsNullOrEmpty(HSpace) Then Call attrs.Append($"{NameOf(HSpace)}=""{HSpace}"" ")
            If Not String.IsNullOrEmpty(Alt) Then Call attrs.Append($"{NameOf(Alt)}=""{Alt}"" ")
            If Not String.IsNullOrEmpty(source) Then Call attrs.Append($"src=""{source}"" ")
            If Not String.IsNullOrEmpty(Align) Then Call attrs.Append($"{NameOf(Align)}=""{Align}"" ")

            Return $"<img {attrs.ToString} />"
        End Function

        ''' <summary>
        ''' 这个Image对象的原始的html字符串
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="img">&lt;img> html标签</param>
        ''' <returns></returns>
        Public Shared Function ResParser(img As String) As Image
            Dim src As String = img.src

            Return New Image With {
                ._Text = img,
                .source = src
            }
        End Function

        Public Shared Function GetImages(html As String) As Image()
            Dim data As String() = Regex.Matches(html, TagAttributeParser.imgHtmlTagPattern, RegexICSng).ToArray
            Dim res As Image() = data.Select(Function(tag) Image.ResParser(tag)).ToArray
            Return res
        End Function
    End Class
End Namespace
