Imports System.Text

Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Class representing a Border entry. The Border entry is used to define frames and cell borders
    ''' </summary>
    Public Class Border
        Inherits AbstractStyle
        ''' <summary>
        ''' Default border style as constant
        ''' </summary>
        Public Shared ReadOnly DEFAULT_BORDER_STYLE As StyleValue = StyleValue.none

        ''' <summary>
        ''' Default border color as constant
        ''' </summary>
        Public Shared ReadOnly DEFAULT_COLOR As String = ""

        ''' <summary>
        ''' Defines the bottomColor
        ''' </summary>
        Private m_bottomColor As String

        ''' <summary>
        ''' Defines the diagonalColor
        ''' </summary>
        Private m_diagonalColor As String

        ''' <summary>
        ''' Defines the leftColor
        ''' </summary>
        Private m_leftColor As String

        ''' <summary>
        ''' Defines the rightColor
        ''' </summary>
        Private m_rightColor As String

        ''' <summary>
        ''' Defines the topColor
        ''' </summary>
        Private m_topColor As String

        ''' <summary>
        ''' Gets or sets the color code of the bottom border. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property BottomColor As String
            Get
                Return m_bottomColor
            End Get
            Set(value As String)
                m_bottomColor = Fill.NormalizeColor(value, True, True)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the style of bottom cell border
        ''' </summary>
        <Append>
        Public Property BottomStyle As StyleValue

        ''' <summary>
        ''' Gets or sets the color code of the diagonal lines. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property DiagonalColor As String
            Get
                Return m_diagonalColor
            End Get
            Set(value As String)
                m_diagonalColor = Fill.NormalizeColor(value, True, True)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether DiagonalDown
        ''' Gets or sets whether the downwards diagonal line is used. If true, the line is used
        ''' </summary>
        <Append>
        Public Property DiagonalDown As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether DiagonalUp
        ''' Gets or sets whether the upwards diagonal line is used. If true, the line is used
        ''' </summary>
        <Append>
        Public Property DiagonalUp As Boolean

        ''' <summary>
        ''' Gets or sets the style of the diagonal lines
        ''' </summary>
        <Append>
        Public Property DiagonalStyle As StyleValue

        ''' <summary>
        ''' Gets or sets the color code of the left border. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property LeftColor As String
            Get
                Return m_leftColor
            End Get
            Set(value As String)
                m_leftColor = Fill.NormalizeColor(value, True, True)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the style of left cell border
        ''' </summary>
        <Append>
        Public Property LeftStyle As StyleValue

        ''' <summary>
        ''' Gets or sets the color code of the right border. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property RightColor As String
            Get
                Return m_rightColor
            End Get
            Set(value As String)
                m_rightColor = Fill.NormalizeColor(value, True, True)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the style of right cell border
        ''' </summary>
        <Append>
        Public Property RightStyle As StyleValue

        ''' <summary>
        ''' Gets or sets the color code of the top border. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property TopColor As String
            Get
                Return m_topColor
            End Get
            Set(value As String)
                m_topColor = Fill.NormalizeColor(value, True, True)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the style of top cell border
        ''' </summary>
        <Append>
        Public Property TopStyle As StyleValue

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Border"/> class
        ''' </summary>
        Public Sub New()
            BottomColor = DEFAULT_COLOR
            TopColor = DEFAULT_COLOR
            LeftColor = DEFAULT_COLOR
            RightColor = DEFAULT_COLOR
            DiagonalColor = DEFAULT_COLOR
            LeftStyle = DEFAULT_BORDER_STYLE
            RightStyle = DEFAULT_BORDER_STYLE
            TopStyle = DEFAULT_BORDER_STYLE
            BottomStyle = DEFAULT_BORDER_STYLE
            DiagonalStyle = DEFAULT_BORDER_STYLE
            DiagonalDown = False
            DiagonalUp = False
        End Sub

        ''' <summary>
        ''' Returns a hash code for this instance
        ''' </summary>
        ''' <returns>The <see cref="Integer"/>.</returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode As Integer = -153001865
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(BottomColor)
            hashCode = hashCode * -1521134295 + BottomStyle.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(DiagonalColor)
            hashCode = hashCode * -1521134295 + DiagonalDown.GetHashCode()
            hashCode = hashCode * -1521134295 + DiagonalUp.GetHashCode()
            hashCode = hashCode * -1521134295 + DiagonalStyle.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(LeftColor)
            hashCode = hashCode * -1521134295 + LeftStyle.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(RightColor)
            hashCode = hashCode * -1521134295 + RightStyle.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(TopColor)
            hashCode = hashCode * -1521134295 + TopStyle.GetHashCode()
            Return hashCode
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one without casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Overrides Function Copy() As AbstractStyle
            Dim lCopy As Border = New Border()
            lCopy.BottomColor = BottomColor
            lCopy.BottomStyle = BottomStyle
            lCopy.DiagonalColor = DiagonalColor
            lCopy.DiagonalDown = DiagonalDown
            lCopy.DiagonalStyle = DiagonalStyle
            lCopy.DiagonalUp = DiagonalUp
            lCopy.LeftColor = LeftColor
            lCopy.LeftStyle = LeftStyle
            lCopy.RightColor = RightColor
            lCopy.RightStyle = RightStyle
            lCopy.TopColor = TopColor
            lCopy.TopStyle = TopStyle
            Return lCopy
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one with casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Function CopyBorder() As Border
            Return CType(Copy(), Border)
        End Function

        ''' <summary>
        ''' Override toString method
        ''' </summary>
        ''' <returns>String of a class.</returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("""Border"": {" & vbLf)
            AddPropertyAsJson(sb, "BottomStyle", BottomStyle)
            AddPropertyAsJson(sb, "DiagonalColor", DiagonalColor)
            AddPropertyAsJson(sb, "DiagonalDown", DiagonalDown)
            AddPropertyAsJson(sb, "DiagonalStyle", DiagonalStyle)
            AddPropertyAsJson(sb, "DiagonalUp", DiagonalUp)
            AddPropertyAsJson(sb, "LeftColor", LeftColor)
            AddPropertyAsJson(sb, "LeftStyle", LeftStyle)
            AddPropertyAsJson(sb, "RightColor", RightColor)
            AddPropertyAsJson(sb, "RightStyle", RightStyle)
            AddPropertyAsJson(sb, "TopColor", TopColor)
            AddPropertyAsJson(sb, "TopStyle", TopStyle)
            Call AddPropertyAsJson(sb, "HashCode", GetHashCode(), True)
            sb.Append(vbLf & "}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to determine whether the object has no values but the default values (means: is empty and must not be processed)
        ''' </summary>
        ''' <returns>True if empty, otherwise false.</returns>
        Public Function IsEmpty() As Boolean
            Dim state = True
            If Not BottomColor = DEFAULT_COLOR Then
                state = False
            End If
            If Not (TopColor = DEFAULT_COLOR) Then
                state = False
            End If
            If Not (LeftColor = DEFAULT_COLOR) Then
                state = False
            End If
            If Not (RightColor = DEFAULT_COLOR) Then
                state = False
            End If
            If Not (DiagonalColor = DEFAULT_COLOR) Then
                state = False
            End If
            If LeftStyle <> DEFAULT_BORDER_STYLE Then
                state = False
            End If
            If RightStyle <> DEFAULT_BORDER_STYLE Then
                state = False
            End If
            If TopStyle <> DEFAULT_BORDER_STYLE Then
                state = False
            End If
            If BottomStyle <> DEFAULT_BORDER_STYLE Then
                state = False
            End If
            If DiagonalStyle <> DEFAULT_BORDER_STYLE Then
                state = False
            End If
            If DiagonalDown Then
                state = False
            End If
            If DiagonalUp Then
                state = False
            End If
            Return state
        End Function

        ''' <summary>
        ''' Gets the border style name from the enum
        ''' </summary>
        ''' <param name="style">Enum to process.</param>
        ''' <returns>The valid value of the border style as String.</returns>
        Public Shared Function GetStyleName(style As StyleValue) As String
            Dim output = ""
            Select Case style
                Case StyleValue.hair
                    output = "hair"
                Case StyleValue.dotted
                    output = "dotted"
                Case StyleValue.dashDotDot
                    output = "dashDotDot"
                Case StyleValue.dashDot
                    output = "dashDot"
                Case StyleValue.dashed
                    output = "dashed"
                Case StyleValue.thin
                    output = "thin"
                Case StyleValue.mediumDashDotDot
                    output = "mediumDashDotDot"
                Case StyleValue.slantDashDot
                    output = "slantDashDot"
                Case StyleValue.mediumDashDot
                    output = "mediumDashDot"
                Case StyleValue.mediumDashed
                    output = "mediumDashed"
                Case StyleValue.medium
                    output = "medium"
                Case StyleValue.thick
                    output = "thick"
                Case StyleValue.s_double
                    output = "double"
                    ' Default / none is already handled (ignored)
            End Select
            Return output
        End Function
    End Class

End Namespace