Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace HTML

    Public Class Image

        Public Property Border As String
        Public Property HSpace As String
        Public Property Alt As String
        Public Property Source As String
        Public Property Align As String

        Public Overrides Function ToString() As String
            Dim attrs As StringBuilder = New StringBuilder(1024)

            If Not String.IsNullOrEmpty(Border) Then Call attrs.Append($"{NameOf(Border)}=""{Border}"" ")
            If Not String.IsNullOrEmpty(HSpace) Then Call attrs.Append($"{NameOf(HSpace)}=""{HSpace}"" ")
            If Not String.IsNullOrEmpty(Alt) Then Call attrs.Append($"{NameOf(Alt)}=""{Alt}"" ")
            If Not String.IsNullOrEmpty(Source) Then Call attrs.Append($"src=""{Source}"" ")
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
            Dim src As String = img.ImageSource

            Return New Image With {
            ._Text = img,
            .Source = src
        }
        End Function

        Public Shared Function GetImages(html As String) As Image()
            Dim data As String() = Regex.Matches(html, WebServiceUtils.IMAGE_SOURCE, RegexICSng).ToArray
            Dim res As Image() = data.ToArray(Function(tag) Image.ResParser(tag))
            Return res
        End Function

    End Class
End Namespace