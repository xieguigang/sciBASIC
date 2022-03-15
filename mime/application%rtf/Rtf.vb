#Region "Microsoft.VisualBasic::3c33ff5f0b5afdb466bf4476dca09c06, sciBASIC#\mime\application%rtf\Rtf.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 209
    '    Code Lines: 112
    ' Comment Lines: 64
    '   Blank Lines: 33
    '     File Size: 8.40 KB


    ' Class Rtf
    ' 
    '     Properties: GlobalFormat, Length
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: __getMetaDataStr, __toRTF, GetColor, GetFont, InternalSetFormat
    '               Save, SetFormat
    ' 
    '     Sub: (+3 Overloads) AppendLine, AppendText
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language

''' <summary>
''' Rich text format document object model.(带有格式描述信息的文本文档的对象模型)
''' </summary>
''' <remarks></remarks>
Public Class Rtf

    Dim _TextBuilder As New StringBuilder(4096)
    Protected Friend __textMetaSrcCache As String
    Friend __formattedRegions As New List(Of FormatedRegion)

    Public Property GlobalFormat As Font

    Public ReadOnly Property Length As Integer
        Get
            Return _TextBuilder.Length
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
    Public Function SetFormat(start%, selectLength%, FontStyle As Font) As Boolean
        Dim right As Integer = start + selectLength
        Dim formatted As New FormatedRegion(start, right, FontStyle, Me)
        Dim LQuery As List(Of FormatedRegion) =
            LinqAPI.MakeList(Of FormatedRegion) <= From region As FormatedRegion
                                                   In __formattedRegions
                                                   Where region.Contains(right) OrElse
                                                       region.Contains(start) ' 查找出重叠的区域
                                                   Select region


        LQuery += From region As FormatedRegion
                  In __formattedRegions
                  Where formatted.Contains(region.Start) OrElse
                      formatted.Contains(region.Right)
                  Select region  ' 可能当前的设置区域完全的包括了一些原有的区域

        If LQuery.IsNullOrEmpty Then
            ' 目标区域还没有设置任何格式，则新建一个格式
            Call __formattedRegions.Add(formatted)
        Else ' 已经设置了格式了，则将目标格式区域截断
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
        Dim Start = _TextBuilder.Length

        Call _TextBuilder.Append(text)
        Call Me.__formattedRegions.Add(New FormatedRegion(Start, Start + Len(text), If(Format Is Nothing, GlobalFormat, Format), Me))
    End Sub

    Public Sub AppendLine()
        Call _TextBuilder.AppendLine()
    End Sub

    ''' <summary>
    ''' 向文档末尾追加一行带有格式标记的文本
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="Format"></param>
    ''' <remarks></remarks>
    Public Sub AppendLine(text As String, Optional Format As Font = Nothing)
        Dim Start = _TextBuilder.Length

        Call _TextBuilder.AppendLine(text)
        Call Me.__formattedRegions.Add(New FormatedRegion(Start, Start + Len(text), If(Format Is Nothing, GlobalFormat, Format), Me))
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
    Private Function __toRTF() As String
        Dim doc As New StringBuilder(4096)

        doc.AppendLine(__getMetaDataStr) '生成元数据
        __textMetaSrcCache = _TextBuilder.ToString       '更新数据缓存

        For Each Region In Me.__formattedRegions
            Call doc.Append(Region.GenerateDocumentText)
        Next
        Call doc.Append("}")

        Return doc.ToString
    End Function

    Const __metaData$ =
"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052{\fonttbl%font_meta%}
{\colortbl ;%cl_meta%}
{\*\generator gcmodeller %version%;}\viewkind4\uc1\pard\brdrb\brdrs\brdrw20\brsp80 "

    ''' <summary>
    ''' ``{\f1\fnil\fcharset0 %font.name%;}``
    ''' </summary>
    Const FontToken As String = "{\f1\fnil\fcharset0 %font.name%;}"

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
    Private Function __getMetaDataStr() As String
        Dim Colors = (From Region In Me.__formattedRegions Select Region.Font.FontColor Distinct).ToArray
        _colorMetas = Colors
        Dim Fonts As List(Of String) = New List(Of String) From {Me.GlobalFormat.FontFamilyName}
        Call Fonts.AddRange((From region In Me.__formattedRegions Select region.Font.FontFamilyName Distinct).ToArray)
        _fontMetas = Fonts.Distinct.ToArray

        Dim MetaBuilder As StringBuilder = New StringBuilder(__metaData)
        Call MetaBuilder.Replace("%cl_meta%", String.Join(" ", (From cl As Color In _colorMetas Select Font.FontColorToString(cl.R, cl.G, cl.B))).ToArray)
        Call MetaBuilder.Replace("%version%", My.Application.Info.Version.ToString)
        Call MetaBuilder.Replace("%font_meta%", String.Join("", (From ffName As String In _fontMetas Select FontToken.Replace("%font.name%", ffName)).ToArray))

        Return MetaBuilder.ToString
    End Function

    Friend _colorMetas As Color()
    Friend _fontMetas As String()

    Friend Function GetColor(Font As Font) As String
        Dim i As Integer = Array.IndexOf(_colorMetas, Font.FontColor)
        If i = -1 Then
            Return "\cf0"
        Else
            Return "\cf" & (i + 1).ToString
        End If
    End Function

    Friend Function GetFont(Font As Font) As String
        Dim i As Integer = Array.IndexOf(_fontMetas, Font.FontFamilyName)

        If i = -1 Then
            Return "\f0"
        Else
            Return "\f" & i.ToString
        End If
    End Function

    Public Function Save(path As String, Optional Encoding As Encoding = Nothing) As Boolean
        Return __toRTF.SaveTo(path, Encoding)
    End Function
End Class
