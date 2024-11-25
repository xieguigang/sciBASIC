#Region "Microsoft.VisualBasic::2bd1d737689ed3c09f491052104c0078, gr\Drawing-net4.8\FontMetrics.vb"

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

    '   Total Lines: 105
    '    Code Lines: 67 (63.81%)
    ' Comment Lines: 20 (19.05%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 18 (17.14%)
    '     File Size: 3.29 KB


    '     Class FontMetrics
    ' 
    '         Properties: Font, Graphics, Height
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) GetStringBounds
    ' 
    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) FontMetrics
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

#If NET8_0_OR_GREATER Then
Imports Font = Microsoft.VisualBasic.Imaging.Font
#End If

Namespace Drawing2D.Text

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' only works for the gdi+ graphics context
    ''' </remarks>
    Public Class FontMetrics

        Public ReadOnly Property Font As Font
        ''' <summary>
        ''' The default gdi+ graphics context
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Graphics As Graphics
        ''' <summary>
        ''' 在当前的字体条件下面的，使用默认的<see cref="Graphics"/>上下文的文本行高
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(font As Font, g As GDICanvas)
            Call Me.New(font, g.Graphics)
        End Sub

        Sub New(font As Font, g As Graphics)
            Dim sfont As System.Drawing.Font = Nothing

            Me.Font = font

#If NET8_0_OR_GREATER Then
            sfont = font.CTypeFontObject
#Else
            sfont = font
#End If

            Height = g.MeasureString("1", sfont).Height
            Graphics = g
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(font As CSSFont, g As GDICanvas)
            Me.New(g.LoadEnvironment().GetFont(font), g)
        End Sub

        ''' <summary>
        ''' Using another graphics context
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Shared Function GetStringBounds(s As String, font As Font, g As Graphics) As RectangleF
            Dim sfont As System.Drawing.Font = Nothing

#If NET8_0_OR_GREATER Then
            sfont = font.CTypeFontObject
#Else
            sfont = font
#End If

            Return New RectangleF(New Point, g.MeasureString(s, sfont))
        End Function

        Public Function GetStringBounds(s As String) As RectangleF
            Return GetStringBounds(s, Font, Graphics)
        End Function

        Public Shared Narrowing Operator CType(f As FontMetrics) As Font
            Return f.Font
        End Operator
    End Class

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FontMetrics(g As Graphics2D) As FontMetrics
            Return New FontMetrics(g.Font, g)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FontMetrics(g As IGraphics, font As Font) As FontMetrics
            Select Case g.GetType
                Case GetType(Graphics2D)
                    Return New FontMetrics(font, DirectCast(g, Graphics2D))
                Case Else
                    Throw New InvalidCastException(g.GetType.FullName)
            End Select
        End Function
    End Module
End Namespace
