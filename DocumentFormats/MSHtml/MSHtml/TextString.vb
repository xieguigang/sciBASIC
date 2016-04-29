Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Marshal

Public Class TextString

    Public Property Font As Font
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Class

Public Module TextAPI

    ' html -->  <font face="Microsoft YaHei" size="1.5"><strong>text</strong><b><i>value</i></b></font> 
    ' 解析上述的表达式会产生一个栈，根据html标记来赋值字符串的gdi+属性

    ''' <summary>
    ''' 执行html栈空间解析
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Public Function TryParse(html As String, Optional defaultFont As Font = Nothing) As TextString()
        Dim chars As New List(Of Char)
        Dim str As New Pointer(Of Char)(html.ToCharArray)
        Dim tokens As New List(Of TextString)

        If defaultFont Is Nothing Then
            defaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Regular)
        End If

        ' 在这里假设所有的文本之中的<符号已经被转义为&lt;

        Dim c As Char
        Dim bold As Boolean = False
        Dim italic As Boolean = False
        Dim curFont As Font = defaultFont

        Do While Not str.EndRead
            c = +str

            If c = "<"c Then  ' 遇到了一个html标签的起始符号
                c = +str

                If c = "/"c Then  ' 这个是一个结束的标记
                    Dim tag As String = str.__nextEndTag
                Else ' 这个是一个html标记的开始
                    Dim tag As HtmlElement = str.__nextTag
                    Dim tagName As String = tag.Name.ToLower

                    Select Case tagName
                        Case "font"
                            curFont = tag.__setFont(bold, italic, defaultFont)
                        Case "strong", "b"
                            bold = True
                        Case "i"
                            italic = True
                        Case "br"
                            chars += vbLf
                    End Select
                End If
            End If
        Loop

        Return tokens
    End Function

    Const FontFaceTag As String = "face"
    Const FontSizeTag As String = "size"

    <Extension>
    Private Function __setFont(font As HtmlElement, bold As Boolean, italic As Boolean, [default] As Font) As Font
        Dim name As String = font(FontFaceTag).Value
        Dim size As String = font(FontSizeTag).Value

        If String.IsNullOrEmpty(name) Then
            name = [default].Name
        End If
        If String.IsNullOrEmpty(size) Then
            size = [default].Size
        End If

        Dim style As FontStyle
        If bold Then
            style += FontStyle.Bold
        End If
        If italic Then
            style += FontStyle.Italic
        End If
        If Not bold AndAlso Not italic Then
            style = FontStyle.Regular
        End If

        Return New Font(name, Scripting.CTypeDynamic(Of Single)(size), style)
    End Function

    <Extension>
    Private Function __nextEndTag(str As Pointer(Of Char)) As String
        Dim chars As New List(Of Char)

        Do While Not str.EndRead AndAlso str.Current <> ">"c
            chars += (+str)
        Loop

        Return New String(chars)
    End Function

    <Extension>
    Public Function __nextTag(str As Pointer(Of Char)) As HtmlElement

    End Function
End Module
