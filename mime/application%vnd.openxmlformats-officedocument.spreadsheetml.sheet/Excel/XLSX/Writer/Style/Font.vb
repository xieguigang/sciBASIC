Imports System.Text

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Class representing a Font entry. The Font entry is used to define text formatting
    ''' </summary>
    Public Class Font : Inherits AbstractStyle

        ''' <summary>
        ''' Default font family as constant
        ''' </summary>
        Public Shared ReadOnly DEFAULT_FONT_NAME As String = "Calibri"

        ''' <summary>
        ''' Maximum possible font size
        ''' </summary>
        Public Shared ReadOnly MIN_FONT_SIZE As Single = 1.0F

        ''' <summary>
        ''' Minimum possible font size
        ''' </summary>
        Public Shared ReadOnly MAX_FONT_SIZE As Single = 409.0F

        ''' <summary>
        ''' Default font size
        ''' </summary>
        Public Shared ReadOnly DEFAULT_FONT_SIZE As Single = 11.0F

        ''' <summary>
        ''' Default font family
        ''' </summary>
        Public Shared ReadOnly DEFAULT_FONT_FAMILY As String = "2"

        ''' <summary>
        ''' Default font scheme
        ''' </summary>
        Public Shared ReadOnly DEFAULT_FONT_SCHEME As SchemeValue = SchemeValue.minor

        ''' <summary>
        ''' Default vertical alignment
        ''' </summary>
        Public Shared ReadOnly DEFAULT_VERTICAL_ALIGN As VerticalAlignValue = VerticalAlignValue.none

        ''' <summary>
        ''' Defines the size
        ''' </summary>
        Private sizeField As Single

        ''' <summary>
        ''' Defines the name
        ''' </summary>
        Private nameField As String = DEFAULT_FONT_NAME

        ''' <summary>
        ''' Defines the colorTheme
        ''' </summary>
        Private colorThemeField As Integer

        ''' <summary>
        ''' Defines the colorValue
        ''' </summary>
        Private colorValueField As String

        ''' <summary>
        ''' Gets or sets a value indicating whether Bold
        ''' Gets or sets whether the font is bold. If true, the font is declared as bold
        ''' </summary>
        <Append>
        Public Property Bold As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether Italic
        ''' Gets or sets whether the font is italic. If true, the font is declared as italic
        ''' </summary>
        <Append>
        Public Property Italic As Boolean

        ''' <summary>
        ''' Gets or sets the underline style of the font. If set to <a cref="UnderlineValue.none">none</a> no underline will be applied (default)
        ''' </summary>
        <Append>
        Public Property Underline As UnderlineValue = UnderlineValue.none

        ''' <summary>
        ''' Gets or sets the char set of the Font (Default is empty)
        ''' </summary>
        <Append>
        Public Property Charset As String

        ''' <summary>
        ''' Gets or sets the font color theme (Default is 1 = Light)
        ''' </summary>
        <Append>
        Public Property ColorTheme As Integer
            Get
                Return colorThemeField
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New StyleException("A general style exception occurred", "The color theme number " & value.ToString() & " is invalid. Should be >0")
                End If
                colorThemeField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the color code of the font color. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' Gets or sets the color code of the font color. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
        ''' </summary>
        <Append>
        Public Property ColorValue As String
            Get
                Return colorValueField
            End Get
            Set(value As String)
                Fill.ValidateColor(value, True, True)
                colorValueField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Family
        ''' Gets or sets the font family (Default is 2 = Swiss)
        ''' </summary>
        <Append>
        Public Property Family As String

        ''' <summary>
        ''' Gets a value indicating whether IsDefaultFont
        ''' Gets whether the font is equal to the default font
        ''' </summary>
        <Append(Ignore:=True)>
        Public ReadOnly Property IsDefaultFont As Boolean
            Get
                Dim temp As Font = New Font()
                Return Equals(temp)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the font name (Default is Calibri)
        ''' </summary>
        <Append>
        Public Property Name As String
            Get
                Return nameField
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(nameField) Then
                    Throw New StyleException("A general style exception occurred", "The font name was null or empty")
                End If
                nameField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the font scheme (Default is minor)
        ''' </summary>
        <Append>
        Public Property Scheme As SchemeValue

        ''' <summary>
        ''' Gets or sets the font size. Valid range is from 1 to 409
        ''' </summary>
        <Append>
        Public Property Size As Single
            Get
                Return sizeField
            End Get
            Set(value As Single)
                If value < MIN_FONT_SIZE Then
                    sizeField = MIN_FONT_SIZE
                ElseIf value > MAX_FONT_SIZE Then
                    sizeField = MAX_FONT_SIZE
                Else
                    sizeField = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether Strike
        ''' Gets or sets whether the font is struck through. If true, the font is declared as strike-through
        ''' </summary>
        <Append>
        Public Property Strike As Boolean

        ''' <summary>
        ''' Gets or sets the alignment of the font (Default is none)
        ''' </summary>
        <Append>
        Public Property VerticalAlign As VerticalAlignValue

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Font"/> class
        ''' </summary>
        Public Sub New()
            sizeField = DEFAULT_FONT_SIZE
            Name = DEFAULT_FONT_NAME
            Family = DEFAULT_FONT_FAMILY
            ColorTheme = 1
            ColorValue = String.Empty
            Charset = String.Empty
            Scheme = DEFAULT_FONT_SCHEME
            VerticalAlign = DEFAULT_VERTICAL_ALIGN
        End Sub

        ''' <summary>
        ''' Override toString method
        ''' </summary>
        ''' <returns>String of a class.</returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("""Font"": {" & vbLf)
            AddPropertyAsJson(sb, "Bold", Bold)
            AddPropertyAsJson(sb, "Charset", Charset)
            AddPropertyAsJson(sb, "ColorTheme", ColorTheme)
            AddPropertyAsJson(sb, "ColorValue", ColorValue)
            AddPropertyAsJson(sb, "VerticalAlign", VerticalAlign)
            AddPropertyAsJson(sb, "Family", Family)
            AddPropertyAsJson(sb, "Italic", Italic)
            AddPropertyAsJson(sb, "Name", Name)
            AddPropertyAsJson(sb, "Scheme", Scheme)
            AddPropertyAsJson(sb, "Size", Size)
            AddPropertyAsJson(sb, "Strike", Strike)
            AddPropertyAsJson(sb, "Underline", Underline)
            Call AddPropertyAsJson(sb, "HashCode", GetHashCode(), True)
            sb.Append(vbLf & "}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one without casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Overrides Function Copy() As AbstractStyle
            Dim lCopy As Font = New Font()
            lCopy.Bold = Bold
            lCopy.Charset = Charset
            lCopy.ColorTheme = ColorTheme
            lCopy.ColorValue = ColorValue
            lCopy.VerticalAlign = VerticalAlign
            lCopy.Family = Family
            lCopy.Italic = Italic
            lCopy.Name = Name
            lCopy.Scheme = Scheme
            lCopy.Size = Size
            lCopy.Strike = Strike
            lCopy.Underline = Underline
            Return lCopy
        End Function

        ''' <summary>
        ''' Returns a hash code for this instance
        ''' </summary>
        ''' <returns>The <see cref="Integer"/>.</returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode = -924704582
            hashCode = hashCode * -1521134295 + sizeField.GetHashCode()
            hashCode = hashCode * -1521134295 + Bold.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(Charset)
            hashCode = hashCode * -1521134295 + ColorTheme.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(ColorValue)
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(Family)
            hashCode = hashCode * -1521134295 + Italic.GetHashCode()
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(Name)
            hashCode = hashCode * -1521134295 + Scheme.GetHashCode()
            hashCode = hashCode * -1521134295 + Strike.GetHashCode()
            hashCode = hashCode * -1521134295 + Underline.GetHashCode()
            hashCode = hashCode * -1521134295 + VerticalAlign.GetHashCode()
            Return hashCode
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one with casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Function CopyFont() As Font
            Return CType(Copy(), Font)
        End Function
    End Class


End Namespace