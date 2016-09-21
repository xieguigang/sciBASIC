#Region "Microsoft.VisualBasic::90e3f017fdf916b0e2fc573baf05ebc9, ..\visualbasic_App\DocumentFormats\DocumentFormat.Word\Rtf.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Imaging

Namespace RichTextFormatDocument

    ''' <summary>
    ''' Rich text format document object model.(带有格式描述信息的文本文档的对象模型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Rtf : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        Friend InternalFormattedRegions As List(Of FormatedRegion) = New List(Of FormatedRegion)
        Dim _InternalTextBuilder As StringBuilder = New StringBuilder(4096)
        Protected Friend _InternalTextMetaSrcCache As String

        Public Property GlobalFormat As Font

        Public ReadOnly Property Length As Integer
            Get
                Return _InternalTextBuilder.Length
            End Get
        End Property

        Sub New()
            GlobalFormat = New Font(10, False, FontFace.MicrosoftYaHei, False, False, Drawing.Color.Black)
        End Sub

        Sub New(GlobalFormat As Font)
            Me.GlobalFormat = GlobalFormat
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="FontName">Font family name.(字体名称)</param>
        ''' <param name="Size">Font size.(字体大小)</param>
        ''' <param name="Color">Font Color.(字体颜色)</param>
        ''' <remarks></remarks>
        Sub New(FontName As String, Size As Integer, Color As System.Drawing.Color)
            GlobalFormat = New Font(size:=Size, Bold:=False, Color:=Color, Italic:=False, Name:=FontName, Underline:=False)
        End Sub

        ''' <summary>
        ''' Set format value to a selected region in the text document. the previous format which was exists on the target 
        ''' document region will be covered by the newly format <paramref name="FontStyle"></paramref>.
        ''' (先前的格式会被后面设置的格式所覆盖)
        ''' </summary>
        ''' <param name="start">The region start location.</param>
        ''' <param name="selectLength">The formated region text length.</param>
        ''' <param name="FontStyle">The format will be applied on the target selected region.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 目标区域<paramref name="start"></paramref> -&gt; <paramref name="selectLength"></paramref>可能与某一个设置了字体的区域重合一部分，也可能完全包含有其他的多个区域
        ''' </remarks>
        Public Function SetFormat(start As Integer, selectLength As Integer, FontStyle As Font) As Boolean
            Dim Right As Integer = start + selectLength
            Dim LQuery = (From Region As FormatedRegion
                          In InternalFormattedRegions
                          Where Region.Contains(Right) OrElse Region.Contains(start)
                          Select Region).ToList '重叠的区域
            Dim FormattedRegion As New FormatedRegion(start, Right, FontStyle, Me)

            Call LQuery.AddRange((From Region As FormatedRegion
                                  In InternalFormattedRegions
                                  Where FormattedRegion.Contains(Region.Start) OrElse FormattedRegion.Contains(Region.Right)
                                  Select Region).ToArray)  '可能当前的设置区域完全的包括了一些原有的区域

            If LQuery.IsNullOrEmpty Then
                '目标区域还没有设置任何格式，则新建一个格式
                Call InternalFormattedRegions.Add(FormattedRegion)
            Else '已经设置了格式了，则将目标格式区域截断
                Return InternalSetFormat(start, selectLength, FontStyle, LQuery.ToArray)
            End If

            Return True
        End Function

        ''' <summary>
        ''' Append the target text value on to the last region of the document with the specific text format.(向文档末尾追加一段带有格式标记的文本)
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="Format"></param>
        ''' <remarks></remarks>
        Public Sub AppendText(text As String, Optional Format As Font = Nothing)
            Dim Start = _InternalTextBuilder.Length

            Call _InternalTextBuilder.Append(text)
            Call Me.InternalFormattedRegions.Add(New FormatedRegion(Start, Start + Len(text), If(Format Is Nothing, GlobalFormat, Format), Me))
        End Sub

        Public Sub AppendLine()
            Call _InternalTextBuilder.AppendLine()
        End Sub

        ''' <summary>
        ''' 向文档末尾追加一行带有格式标记的文本
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="Format"></param>
        ''' <remarks></remarks>
        Public Sub AppendLine(text As String, Optional Format As Font = Nothing)
            Dim Start = _InternalTextBuilder.Length

            Call _InternalTextBuilder.AppendLine(text)
            Call Me.InternalFormattedRegions.Add(New FormatedRegion(Start, Start + Len(text), If(Format Is Nothing, GlobalFormat, Format), Me))
        End Sub

        Public Sub AppendLine(text As String, Color As System.Drawing.Color)
            Call AppendLine(text, Font.FromExistsValue(GlobalFormat, Color))
        End Sub

        Private Function InternalSetFormat(start As Integer, selectLength As Integer, FontStyle As Font, OverlapsRegion As FormatedRegion()) As Boolean
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' 生成一个含有格式描述的文本文件，即将模型数据保存为rtf文档
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InternalGenerateDocument() As String
            Dim _DocumentBuilder As StringBuilder = New StringBuilder(4096)

            _DocumentBuilder.AppendLine(InternalGenerateMetaData) '生成元数据
            _InternalTextMetaSrcCache = _InternalTextBuilder.ToString       '更新数据缓存

            For Each Region In Me.InternalFormattedRegions
                Call _DocumentBuilder.Append(Region.GenerateDocumentText)
            Next
            Call _DocumentBuilder.Append("}")

            Return _DocumentBuilder.ToString
        End Function

        Private Shared ReadOnly _InternalMetaData As String = <RTF_META>{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052{\fonttbl%font_meta%}
{\colortbl ;%cl_meta%}
{\*\generator gcmodeller %version%;}\viewkind4\uc1\pard\brdrb\brdrs\brdrw20\brsp80 </RTF_META>

        Const FONT_TOKEN As String = "{\f1\fnil\fcharset0 %font.name%;}"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' {
        '''   \rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052
        '''   {
        '''     \fonttbl
        '''     {
        '''        \f0\fnil\fcharset0 Cambria;
        '''     }
        '''   }
        '''   {
        '''     \colortbl ;%cl_meta%;
        '''   }
        '''   {
        '''     \*\generator Msftedit 5.41.21.2510;
        '''   }
        ''' </remarks>
        Private Function InternalGenerateMetaData() As String
            Dim Colors = (From Region In Me.InternalFormattedRegions Select Region.Font.FontColor Distinct).ToArray
            _InternalColorMeta = Colors
            Dim Fonts As List(Of String) = New List(Of String) From {Me.GlobalFormat.FontFamilyName}
            Call Fonts.AddRange((From region In Me.InternalFormattedRegions Select region.Font.FontFamilyName Distinct).ToArray)
            _InternalFontMeta = Fonts.Distinct.ToArray

            Dim MetaBuilder As StringBuilder = New StringBuilder(_InternalMetaData)
            Call MetaBuilder.Replace("%cl_meta%", String.Join(" ", (From cl As System.Drawing.Color In _InternalColorMeta Select Font.FontColorToString(cl.R, cl.G, cl.B))).ToArray)
            Call MetaBuilder.Replace("%version%", My.Application.Info.Version.ToString)
            Call MetaBuilder.Replace("%font_meta%", String.Join("", (From ffName As String In _InternalFontMeta Select FONT_TOKEN.Replace("%font.name%", ffName)).ToArray))

            Return MetaBuilder.ToString
        End Function

        Friend _InternalColorMeta As System.Drawing.Color()
        Friend _InternalFontMeta As System.String()

        Friend Function GetColor(Font As Font) As String
            Dim i As Integer = Array.IndexOf(_InternalColorMeta, Font.FontColor)
            If i = -1 Then
                Return "\cf0"
            Else
                Return "\cf" & (i + 1).ToString
            End If
        End Function

        Friend Function GetFont(Font As Font) As String
            Dim i As Integer = Array.IndexOf(_InternalFontMeta, Font.FontFamilyName)

            If i = -1 Then
                Return "\f0"
            Else
                Return "\f" & i.ToString
            End If
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean
            FilePath = getPath(FilePath)
            Return InternalGenerateDocument.SaveTo(FilePath, Encoding)
        End Function
    End Class

    Public Class FormatedRegion

        Friend RTFDocument As Rtf

        Public Property Font As Font

        Public ReadOnly Property Text As String
            Get
#Const DEBUG = 0

#If DEBUG Then
            Try
#End If
                Return Mid(RTFDocument._InternalTextMetaSrcCache, Start, Right - Start + 1)
#If DEBUG Then
            Catch ex As Exception
                Call Console.WriteLine("[DEBUG] ({0}, {1})", Start, Right)
                Throw
            End Try
#End If
            End Get
        End Property

        Dim _Start As Integer
        Dim _Right As Integer

        Public ReadOnly Property Start As Integer
            Get
                Return _Start
            End Get
        End Property

        Public ReadOnly Property Right As Integer
            Get
                Return _Right
            End Get
        End Property

        Sub New(Start As Integer, Right As Integer, Font As Font, Document As Rtf)
            _Start = Start
            _Right = Right
            Me.Font = Font
            Me.RTFDocument = Document

            If Start <= 0 Then
                _Start = 1
            End If
            If Right > RTFDocument.Length Then
                _Right = RTFDocument.Length
            End If
        End Sub

        Public Function Contains(p As Integer) As Boolean
            Return p >= Start AndAlso p <= Right
        End Function

        Public Overrides Function ToString() As String
            Return Text
        End Function

        Public Function GenerateDocumentText() As String
            Return Font.GenerateRTFTAG(Me)
        End Function

        ''' <summary>
        ''' 是否具有分段的标识符
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HaveParFlag As Boolean
            Get
                Dim TextCache As String = Text

                If InStr(TextCache, vbCrLf) > 0 OrElse InStr(TextCache, vbCr) > 0 OrElse InStr(TextCache, vbLf) > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Font style of the selected text region.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Font : Inherits ComponentModels.Font

        Public Const RTF_LF As String = "\par"
        Public Const RTF_FONT_STYLE_BOLD As String = "\b"
        Public Const RTF_FONT_STYLE_ITALIC As String = "\i"
        Public Const RTF_FONT_SIZE As String = "\fs"
        Public Const RTF_FONT_STYLE_UNDER_LINE As String = "\ul"
        Public Const RTF_FONT_STYLE_NONE_UNDER_LINE As String = "\ulnone"

        Sub New()
        End Sub

        Sub New(size As Integer, Bold As Boolean, Name As String, Italic As Boolean, Underline As Boolean, Color As System.Drawing.Color)
            FontSize = size
            FontBold = Bold
            FontFamilyName = Name
            FontItalic = Italic
            FontColor = Color
            FontUnderline = Underline
        End Sub

        Sub New(size As Integer, Bold As Boolean, Name As String, Italic As Boolean, Underline As Boolean)
            FontSize = size
            FontBold = Bold
            FontFamilyName = Name
            FontItalic = Italic
            FontColor = Drawing.Color.Black
            FontUnderline = Underline
        End Sub

        ''' <summary>
        ''' Normal font style
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="Name"></param>
        ''' <remarks></remarks>
        Sub New(size As Integer, Name As String)
            FontSize = size
            FontFamilyName = Name
            Me.FontBold = False
            Me.FontColor = Drawing.Color.Black
            Me.FontItalic = False
        End Sub

        Public Overrides Function ToString() As String
            Return FontFamilyName
        End Function

        Public Shared Function FromExistsValue(Font As Font, Color As System.Drawing.Color) As Font
            Dim value = Font.Clone
            value.FontColor = Color
            Return value
        End Function

        Public Function GenerateRTFTAG(Region As FormatedRegion) As String
            Dim Text As String = Region.Text

            If Region.HaveParFlag Then
                Dim p As Integer = InStr(Text, vbLf)

                Do While p > 0
                    Text = Text.Insert(p - 1, Font.RTF_LF)
                    p = InStr(p + 6, Text, vbLf)
                Loop
            End If

            '设置字体的具体格式
            Dim Style As String = Font.RTF_FONT_SIZE & Me.FontSize.ToString

            If Me.FontBold Then
                Style &= Font.RTF_FONT_STYLE_BOLD
                Text &= Font.RTF_FONT_STYLE_BOLD & "0"
            End If

            If Me.FontItalic Then
                Style &= Font.RTF_FONT_STYLE_ITALIC
                Text &= Font.RTF_FONT_STYLE_ITALIC & "0"
            End If

            If Me.FontUnderline Then
                Style &= Font.RTF_FONT_STYLE_UNDER_LINE
                Text &= Font.RTF_FONT_STYLE_NONE_UNDER_LINE
            End If

            Style &= Region.RTFDocument.GetColor(Me)
            Style &= Region.RTFDocument.GetFont(Me)

            Return Style & Text
        End Function

        Public Function Clone() As Font
            Return DirectCast(Me.MemberwiseClone, Font)
        End Function

        Public Shared Function FontColorToString(R As Integer, G As Integer, B As Integer) As String
            Return String.Format("\red{0}\green{1}\blue{2};", R, G, B)
        End Function

        Public Overloads Shared Function ToString(Color As System.Drawing.Color) As String
            Return FontColorToString(Color.R, Color.G, Color.B)
        End Function
    End Class
End Namespace
