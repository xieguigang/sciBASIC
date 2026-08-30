#Region "Microsoft.VisualBasic::f754f991bd4497ef00d111d3ffb4db67, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\ANSI\ConsoleFormat.vb"

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

    '   Total Lines: 178
    '    Code Lines: 105 (58.99%)
    ' Comment Lines: 52 (29.21%)
    '    - Xml Docs: 84.62%
    ' 
    '   Blank Lines: 21 (11.80%)
    '     File Size: 7.54 KB


    '     Class ConsoleFormat
    ' 
    '         Properties: Background, BackgroundCode, Bold, Foreground, ForegroundCode
    '                     Inverted, IsDefault, None, Strikeout, Underline
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Clone, Combine, Equals, HtmlColorCode, PushStyle
    '                   ToString
    ' 
    '         Sub: Apply, SetConfig
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' define the <see cref="TextSpan.style"/> for print on the console.
    ''' </summary>
    Public Class ConsoleFormat
        Implements IEquatable(Of ConsoleFormat)
        Implements ICloneable(Of ConsoleFormat)

        Public Shared ReadOnly Property None() As ConsoleFormat
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ForegroundCode() As String
            Get
                Return Foreground?.GetCode(AnsiColor.Type.Foreground)
            End Get
        End Property

        Public ReadOnly Property BackgroundCode() As String
            Get
                Return Background?.GetCode(AnsiColor.Type.Background)
            End Get
        End Property

        Public Property Foreground As AnsiColor?
        Public Property Background As AnsiColor?
        Public Property Bold As Boolean
        Public Property Underline As Boolean
        Public Property Inverted As Boolean
        ''' <summary>
        ''' the strike-through text decoration, which is mapped to the ``SGR 9``
        ''' ansi escape code. It is required by the markdown ``~~deleted~~`` span.
        ''' </summary>
        ''' <returns></returns>
        Public Property Strikeout As Boolean

        Public ReadOnly Property IsDefault() As Boolean
            Get
                Return Not Foreground.HasValue AndAlso
                    Not Background.HasValue AndAlso
                    Not Bold AndAlso
                    Not Underline AndAlso
                    Not Inverted AndAlso
                    Not Strikeout
            End Get
        End Property

        Sub New(Optional Foreground As AnsiColor = Nothing,
                Optional Background As AnsiColor = Nothing,
                Optional Bold As Boolean = False,
                Optional Underline As Boolean = False,
                Optional Inverted As Boolean = False,
                Optional Strikeout As Boolean = False)

            Me.Foreground = Foreground
            Me.Background = Background
            Me.Bold = Bold
            Me.Underline = Underline
            Me.Inverted = Inverted
            Me.Strikeout = Strikeout
        End Sub

        ''' <summary>
        ''' apply this style as the current style of the given <paramref name="render"/>.
        ''' </summary>
        ''' <param name="render"></param>
        ''' <remarks>
        ''' This method just assigns the <see cref="MarkdownRender.currentStyle"/>, it 
        ''' does **not** push the style into the <see cref="MarkdownRender.styleStack"/>, 
        ''' so that it is safe to be called from the <see cref="MarkdownRender"/> style 
        ''' restore routine without growing the stack.
        ''' </remarks>
        Public Sub Apply(render As MarkdownRender)
            render.currentStyle = Me
        End Sub

        ''' <summary>
        ''' push this style into the <see cref="MarkdownRender.styleStack"/> and then 
        ''' apply it as the current style.
        ''' </summary>
        ''' <param name="render"></param>
        ''' <returns>returns itself for the method chaining.</returns>
        Public Function PushStyle(render As MarkdownRender) As ConsoleFormat
            Call render.styleStack.Push(Me)
            Call Apply(render)

            Return Me
        End Function

        ''' <summary>
        ''' legacy API of <see cref="PushStyle"/>, which is kept here for the
        ''' backward compatibility.
        ''' </summary>
        ''' <param name="render"></param>
        Public Sub SetConfig(render As MarkdownRender)
            Call PushStyle(render)
        End Sub

        Public Overloads Function Equals(other As ConsoleFormat) As Boolean Implements IEquatable(Of ConsoleFormat).Equals
            If other Is Nothing Then
                ' the theme style is optional, so that a nothing reference is a
                ' very common case here and must not throw a null reference exception
                Return False
            End If

            'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom Equals where 'other' is by-ref
            Return Foreground = other.Foreground AndAlso
                Background = other.Background AndAlso
                Bold = other.Bold AndAlso
                Underline = other.Underline AndAlso
                Inverted = other.Inverted AndAlso
                Strikeout = other.Strikeout
        End Function

        ''' <summary>
        ''' <see cref="AnsiEscapeCodes.ToAnsiEscapeSequenceSlow"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(Me)
        End Function

        Public Shared Widening Operator CType(colors As (fore As ConsoleColor, back As ConsoleColor)) As ConsoleFormat
            Return New ConsoleFormat With {
                .Foreground = colors.fore,
                .Background = colors.back
            }
        End Operator

        Public Shared Function HtmlColorCode(color As ConsoleColor) As String
            Return Drawing.Color.FromName(color.ToString).ToHtmlColor
        End Function

        Public Function Clone() As ConsoleFormat Implements ICloneable(Of ConsoleFormat).Clone
            Return New ConsoleFormat(Foreground, Background, Bold, Underline, Inverted, Strikeout)
        End Function

        ''' <summary>
        ''' merges the <paramref name="child"/> style on top of the 
        ''' <paramref name="parent"/> style.
        ''' </summary>
        ''' <param name="parent">the base style, e.g. the block quote style.</param>
        ''' <param name="child">
        ''' the inline style, e.g. the bold style. Only the fields that are set by 
        ''' the child style can override the parent ones, so that a bold span inside 
        ''' of a block quote still keeps the background color of the block quote.
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Combine(parent As ConsoleFormat, child As ConsoleFormat) As ConsoleFormat
            If child Is Nothing Then
                Return parent
            End If
            If parent Is Nothing Then
                Return child
            End If

            ' the AnsiColor is a structure, so that an unassigned color is a default
            ' value instead of a null reference: the HasValue property of such a
            ' default value is True but its ansi code is Nothing. The color codes
            ' must be tested here, or the parent color will be dropped by the
            ' unassigned child color.
            Return New ConsoleFormat With {
                .Foreground = If(child.ForegroundCode IsNot Nothing, child.Foreground, parent.Foreground),
                .Background = If(child.BackgroundCode IsNot Nothing, child.Background, parent.Background),
                .Bold = parent.Bold OrElse child.Bold,
                .Underline = parent.Underline OrElse child.Underline,
                .Inverted = parent.Inverted OrElse child.Inverted,
                .Strikeout = parent.Strikeout OrElse child.Strikeout
            }
        End Function
    End Class
End Namespace
