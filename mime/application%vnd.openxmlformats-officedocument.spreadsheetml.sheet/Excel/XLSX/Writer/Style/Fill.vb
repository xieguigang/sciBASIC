Imports System.Text
Imports System.Text.RegularExpressions

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Class representing a Fill (background) entry. The Fill entry is used to define background colors and fill patterns
    ''' </summary>
    Public Class Fill : Inherits AbstractStyle

        ''' <summary>
        ''' Default Color (foreground or background)
        ''' </summary>
        Public Shared ReadOnly DEFAULT_COLOR As String = "FF000000"

        ''' <summary>
        ''' Default index color
        ''' </summary>
        Public Shared ReadOnly DEFAULT_INDEXED_COLOR As Integer = 64

        ''' <summary>
        ''' Default pattern
        ''' </summary>
        Public Shared ReadOnly DEFAULT_PATTERN_FILL As PatternValue = PatternValue.none

        ''' <summary>
        ''' Defines the backgroundColor
        ''' </summary>
        Private m_backgroundColor As String = DEFAULT_COLOR

        ''' <summary>
        ''' Defines the foregroundColor
        ''' </summary>
        Private m_foregroundColor As String = DEFAULT_COLOR

        ''' <summary>
        ''' Gets or sets the background color of the fill. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property BackgroundColor As String
            Get
                Return m_backgroundColor
            End Get
            Set(value As String)
                m_backgroundColor = NormalizeColor(value, True)
                If PatternFill = PatternValue.none Then
                    PatternFill = PatternValue.solid
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the foreground color of the fill. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property ForegroundColor As String
            Get
                Return m_foregroundColor
            End Get
            Set(value As String)
                m_foregroundColor = NormalizeColor(value, True)
                If PatternFill = PatternValue.none Then
                    PatternFill = PatternValue.solid
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the indexed color (Default is 64)
        ''' </summary>
        <Append>
        Public Property IndexedColor As Integer

        ''' <summary>
        ''' Gets or sets the pattern type of the fill (Default is none)
        ''' </summary>
        <Append>
        Public Property PatternFill As PatternValue

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Fill"/> class
        ''' </summary>
        Public Sub New()
            ' NOTE: the color fields are assigned directly instead of through their
            ' properties. The property setters implicitly promote a "none" pattern to
            ' "solid", which would turn every freshly created (i.e. empty) Fill into an
            ' opaque black one. Writing the backing fields keeps the default Fill truly
            ' empty, which is what both the XML writer and AbstractStyle.CopyProperties
            ' (which compares against a pristine reference instance) expect.
            m_foregroundColor = DEFAULT_COLOR
            m_backgroundColor = DEFAULT_COLOR
            IndexedColor = DEFAULT_INDEXED_COLOR
            PatternFill = DEFAULT_PATTERN_FILL
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Fill"/> class
        ''' </summary>
        ''' <param name="foreground">Foreground color of the fill.</param>
        ''' <param name="background">Background color of the fill.</param>
        Public Sub New(foreground As String, background As String)
            m_backgroundColor = NormalizeColor(background, True)
            m_foregroundColor = NormalizeColor(foreground, True)
            IndexedColor = DEFAULT_INDEXED_COLOR
            PatternFill = PatternValue.solid
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Fill"/> class
        ''' </summary>
        ''' <param name="value">Color value.</param>
        ''' <param name="filltype">Fill type (fill or pattern).</param>
        Public Sub New(value As String, filltype As FillType)
            If filltype = FillType.fillColor Then
                m_backgroundColor = DEFAULT_COLOR
                m_foregroundColor = NormalizeColor(value, True)
            Else
                m_backgroundColor = NormalizeColor(value, True)
                m_foregroundColor = DEFAULT_COLOR
            End If
            IndexedColor = DEFAULT_INDEXED_COLOR
            PatternFill = PatternValue.solid
        End Sub

        ''' <summary>
        ''' Override toString method
        ''' </summary>
        ''' <returns>String of a class.</returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("""Fill"": {" & vbLf)
            AddPropertyAsJson(sb, "BackgroundColor", BackgroundColor)
            AddPropertyAsJson(sb, "ForegroundColor", ForegroundColor)
            AddPropertyAsJson(sb, "IndexedColor", IndexedColor)
            AddPropertyAsJson(sb, "PatternFill", PatternFill)
            Call AddPropertyAsJson(sb, "HashCode", GetHashCode(), True)
            sb.Append(vbLf & "}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one without casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Overrides Function Copy() As AbstractStyle
            Dim lCopy As Fill = New Fill()
            ' Assign the backing fields directly and restore the pattern afterwards, so
            ' the auto-promotion inside the property setters cannot turn a copied "none"
            ' fill into a solid one.
            lCopy.m_backgroundColor = BackgroundColor
            lCopy.m_foregroundColor = ForegroundColor
            lCopy.IndexedColor = IndexedColor
            lCopy.PatternFill = PatternFill
            Return lCopy
        End Function

        ''' <summary>
        ''' Returns a hash code for this instance
        ''' </summary>
        ''' <returns>The <see cref="Integer"/>.</returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode = -1564173520
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(BackgroundColor)
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(ForegroundColor)
            hashCode = hashCode * -1521134295 + IndexedColor.GetHashCode()
            hashCode = hashCode * -1521134295 + PatternFill.GetHashCode()
            Return hashCode
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one with casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Function CopyFill() As Fill
            Return CType(Copy(), Fill)
        End Function

        ''' <summary>
        ''' Sets the color and the depending fill type
        ''' </summary>
        ''' <param name="value">color value.</param>
        ''' <param name="filltype">fill type (fill or pattern).</param>
        Public Sub SetColor(value As String, filltype As FillType)
            If filltype = FillType.fillColor Then
                m_backgroundColor = DEFAULT_COLOR
                m_foregroundColor = NormalizeColor(value, True)
            Else
                m_backgroundColor = NormalizeColor(value, True)
                m_foregroundColor = DEFAULT_COLOR
            End If
            PatternFill = PatternValue.solid
        End Sub

        ''' <summary>
        ''' Gets the pattern name from the enum
        ''' </summary>
        ''' <param name="pattern">Enum to process.</param>
        ''' <returns>The valid value of the pattern as String.</returns>
        Public Shared Function GetPatternName(pattern As PatternValue) As String
            Dim output As String
            Select Case pattern
                Case PatternValue.none
                    output = "none"
                Case PatternValue.solid
                    output = "solid"
                Case PatternValue.darkGray
                    output = "darkGray"
                Case PatternValue.mediumGray
                    output = "mediumGray"
                Case PatternValue.lightGray
                    output = "lightGray"
                Case PatternValue.gray0625
                    output = "gray0625"
                Case PatternValue.gray125
                    output = "gray125"
                Case Else
                    output = "none"
            End Select
            Return output
        End Function

        ''' <summary>
        ''' Validates the passed string, whether it is a valid RGB value that can be used for Fills or Fonts
        ''' </summary>
        ''' <param name="hexCode">Hex string to check.</param>
        ''' <param name="useAlpha">If true, two additional characters (total 8) are expected as alpha value.</param>
        ''' <param name="allowEmpty">Optional parameter that allows null or empty as valid values.</param>
        Public Shared Sub ValidateColor(hexCode As String, useAlpha As Boolean, Optional allowEmpty As Boolean = False)
            NormalizeColor(hexCode, useAlpha, allowEmpty)
        End Sub

        ''' <summary>
        ''' Validates and normalizes a color expression into the canonical upper case
        ''' AARRGGBB (or RRGGBB) form that is expected inside the generated XML
        ''' </summary>
        ''' <remarks>
        ''' Accepted notations are <c>AARRGGBB</c>, <c>RRGGBB</c> and both of them with a
        ''' leading CSS style hash (<c>#AARRGGBB</c> / <c>#RRGGBB</c>). When
        ''' <paramref name="useAlpha"/> is true a missing alpha channel is completed with
        ''' a fully opaque <c>FF</c>.
        ''' </remarks>
        ''' <param name="hexCode">Hex string to normalize.</param>
        ''' <param name="useAlpha">If true, an alpha value is expected or added (total 8 characters).</param>
        ''' <param name="allowEmpty">Optional parameter that allows null or empty as valid values.</param>
        ''' <returns>The normalized, upper case hex value, or an empty string for an allowed empty input.</returns>
        Public Shared Function NormalizeColor(hexCode As String, useAlpha As Boolean, Optional allowEmpty As Boolean = False) As String
            Dim value As String = If(hexCode, String.Empty).Trim()
            If value.StartsWith("#") Then
                value = value.Substring(1)
            End If
            If value.Length = 0 Then
                If allowEmpty Then
                    Return String.Empty
                End If
                Throw New StyleException("A general style exception occurred", "The color expression was null or empty")
            End If
            ' Anchored on purpose: an unanchored pattern would happily accept a substring
            ' match and let invalid characters slip through into the XML.
            If Not Regex.IsMatch(value, "^[a-fA-F0-9]+$") Then
                Throw New StyleException("A general style exception occurred", "The expression '" & hexCode & "' is not a valid hex value")
            End If
            If useAlpha Then
                If value.Length = 6 Then
                    value = "FF" & value
                End If
                If value.Length <> 8 Then
                    Throw New StyleException("A general style exception occurred", "The value '" & hexCode & "' is invalid. A valid value must contain six or eight hex characters")
                End If
            ElseIf value.Length <> 6 Then
                Throw New StyleException("A general style exception occurred", "The value '" & hexCode & "' is invalid. A valid value must contain six hex characters")
            End If
            Return value.ToUpperInvariant()
        End Function

        ''' <summary>
        ''' Gets the color that Excel actually renders for this fill
        ''' </summary>
        ''' <remarks>
        ''' For a solid fill the visible color is taken from the <c>fgColor</c> element,
        ''' <c>bgColor</c> is ignored by Excel. Callers of this class however tend to
        ''' express the cell shading through <see cref="BackgroundColor"/>, so that value
        ''' takes precedence and <see cref="ForegroundColor"/> only acts as fallback for
        ''' the internal <see cref="SetColor"/> / built-in style code paths.
        ''' </remarks>
        ''' <returns>The effective color as AARRGGBB, or an empty string when no explicit color was set.</returns>
        Public Function GetEffectiveFillColor() As String
            If Not String.IsNullOrEmpty(BackgroundColor) AndAlso Not String.Equals(BackgroundColor, DEFAULT_COLOR) Then
                Return BackgroundColor
            End If
            If Not String.IsNullOrEmpty(ForegroundColor) AndAlso Not String.Equals(ForegroundColor, DEFAULT_COLOR) Then
                Return ForegroundColor
            End If
            ' Both colors are still at their default. Only honor that default when the
            ' pattern was set explicitly, otherwise the fill is simply undefined.
            If PatternFill <> PatternValue.none AndAlso Not String.IsNullOrEmpty(ForegroundColor) Then
                Return ForegroundColor
            End If
            Return String.Empty
        End Function

        ''' <summary>
        ''' Gets a value indicating whether this fill produces any visible shading and
        ''' therefore has to be written to the style sheet
        ''' </summary>
        ''' <returns>True if the fill has to be applied to a cell.</returns>
        Public Function HasVisibleFill() As Boolean
            Return PatternFill <> PatternValue.none
        End Function
    End Class

End Namespace