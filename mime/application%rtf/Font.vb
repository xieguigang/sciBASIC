#Region "Microsoft.VisualBasic::3d3b2d733e4ab8cc3fbf15e81284b8ac, sciBASIC#\mime\application%rtf\Font.vb"

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

    '   Total Lines: 108
    '    Code Lines: 77
    ' Comment Lines: 11
    '   Blank Lines: 20
    '     File Size: 3.37 KB


    ' Class Font
    ' 
    '     Constructor: (+4 Overloads) Sub New
    '     Function: Clone, FontColorToString, FromExistsValue, GenerateRTFTAG, (+2 Overloads) ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

''' <summary>
''' Font style of the selected text region.
''' </summary>
''' <remarks></remarks>
Public Class Font : Inherits Models.Font

    Public Const RTF_LF As String = "\par"
    Public Const RTF_FONT_STYLE_BOLD As String = "\b"
    Public Const RTF_FONT_STYLE_ITALIC As String = "\i"
    Public Const RTF_FONT_SIZE As String = "\fs"
    Public Const RTF_FONT_STYLE_UNDER_LINE As String = "\ul"
    Public Const RTF_FONT_STYLE_NONE_UNDER_LINE As String = "\ulnone"

    Sub New()
    End Sub

    Sub New(size%, Bold As Boolean, Name$, Italic As Boolean, Underline As Boolean, Color As Color)
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

    Public Shared Function FromExistsValue(Font As Font, Color As Color) As Font
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

    Public Overloads Shared Function ToString(Color As Color) As String
        Return FontColorToString(Color.R, Color.G, Color.B)
    End Function
End Class
