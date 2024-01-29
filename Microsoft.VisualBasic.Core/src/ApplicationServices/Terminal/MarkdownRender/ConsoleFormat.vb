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

        Public ReadOnly Property IsDefault() As Boolean
            Get
                Return Not Foreground.HasValue AndAlso
                    Not Background.HasValue AndAlso
                    Not Bold AndAlso
                    Not Underline AndAlso
                    Not Inverted
            End Get
        End Property

        Sub New(Optional Foreground As AnsiColor = Nothing,
                Optional Background As AnsiColor = Nothing,
                Optional Bold As Boolean = False,
                Optional Underline As Boolean = False,
                Optional Inverted As Boolean = False)

            Me.Foreground = Foreground
            Me.Background = Background
            Me.Bold = Bold
            Me.Underline = Underline
            Me.Inverted = Inverted
        End Sub

        Public Sub SetConfig(render As MarkdownRender)
            render.currentStyle = Me
            render.styleStack.Push(Me)
        End Sub

        Public Overloads Function Equals(other As ConsoleFormat) As Boolean Implements IEquatable(Of ConsoleFormat).Equals
            'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom Equals where 'other' is by-ref
            Return Foreground = other.Foreground AndAlso
                Background = other.Background AndAlso
                Bold = other.Bold AndAlso
                Underline = other.Underline AndAlso
                Inverted = other.Inverted
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
            Return New ConsoleFormat(Foreground, Background, Bold, Underline, Inverted)
        End Function
    End Class
End Namespace