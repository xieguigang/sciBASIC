Imports System.Runtime.InteropServices
Imports System.Text

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Class representing a NumberFormat entry. The NumberFormat entry is used to define cell formats like currency or date
    ''' </summary>
    Public Class NumberFormat
        Inherits AbstractStyle
        ''' <summary>
        ''' Start ID for custom number formats as constant
        ''' </summary>
        Public Const CUSTOMFORMAT_START_NUMBER As Integer = 164

        ''' <summary>
        ''' Default format number as constant
        ''' </summary>
        Public Shared ReadOnly DEFAULT_NUMBER As FormatNumber = FormatNumber.none

        ''' <summary>
        ''' Enum for predefined number formats
        ''' </summary>
        ''' <remarks>There are other predefined formats (e.g. 43 and 44) that are not listed. The declaration of such formats is done in the number formats section of the style document, whereas the officially listed ones are implicitly used and not declared in the style document</remarks>
        Public Enum FormatNumber
            ''' <summary>No format / Default</summary>
            none = 0
            ''' <summary>Format: 0</summary>
            format_1 = 1
            ''' <summary>Format: 0.00</summary>
            format_2 = 2
            ''' <summary>Format: #,##0</summary>
            format_3 = 3
            ''' <summary>Format: #,##0.00</summary>
            format_4 = 4
            ''' <summary>Format: $#,##0_);($#,##0)</summary>
            format_5 = 5
            ''' <summary>Format: $#,##0_);[Red]($#,##0)</summary>
            format_6 = 6
            ''' <summary>Format: $#,##0.00_);($#,##0.00)</summary>
            format_7 = 7
            ''' <summary>Format: $#,##0.00_);[Red]($#,##0.00)</summary>
            format_8 = 8
            ''' <summary>Format: 0%</summary>
            format_9 = 9
            ''' <summary>Format: 0.00%</summary>
            format_10 = 10
            ''' <summary>Format: 0.00E+00</summary>
            format_11 = 11
            ''' <summary>Format: # ?/?</summary>
            format_12 = 12
            ''' <summary>Format: # ??/??</summary>
            format_13 = 13
            ''' <summary>Format: m/d/yyyy</summary>
            format_14 = 14
            ''' <summary>Format: d-mmm-yy</summary>
            format_15 = 15
            ''' <summary>Format: d-mmm</summary>
            format_16 = 16
            ''' <summary>Format: mmm-yy</summary>
            format_17 = 17
            ''' <summary>Format: mm AM/PM</summary>
            format_18 = 18
            ''' <summary>Format: h:mm:ss AM/PM</summary>
            format_19 = 19
            ''' <summary>Format: h:mm</summary>
            format_20 = 20
            ''' <summary>Format: h:mm:ss</summary>
            format_21 = 21
            ''' <summary>Format: m/d/yyyy h:mm</summary>
            format_22 = 22
            ''' <summary>Format: #,##0_);(#,##0)</summary>
            format_37 = 37
            ''' <summary>Format: #,##0_);[Red](#,##0)</summary>
            format_38 = 38
            ''' <summary>Format: #,##0.00_);(#,##0.00)</summary>
            format_39 = 39
            ''' <summary>Format: #,##0.00_);[Red](#,##0.00)</summary>
            format_40 = 40
            ''' <summary>Format: mm:ss</summary>
            format_45 = 45
            ''' <summary>Format: [h]:mm:ss</summary>
            format_46 = 46
            ''' <summary>Format: mm:ss.0</summary>
            format_47 = 47
            ''' <summary>Format: ##0.0E+0</summary>
            format_48 = 48
            ''' <summary>Format: #</summary>
            format_49 = 49
            ''' <summary>Custom Format (ID 164 and higher)</summary>
            custom = 164
        End Enum

        ''' <summary>
        ''' Range or validity of the format number
        ''' </summary>
        Public Enum FormatRange
            ''' <summary>
            ''' Format from 0 to 164 (with gaps)
            ''' </summary>
            defined_format
            ''' <summary>
            ''' Custom defined formats from 164 and higher. Although 164 is already custom, it is still defined as enum value
            ''' </summary>
            custom_format
            ''' <summary>
            ''' Probably invalid format numbers (e.g. negative value)
            ''' </summary>
            invalid
            ''' <summary>
            ''' Values between 0 and 164 that are not defined as enum value. This may be caused by changes of the OOXML specifications or Excel versions that have encoded loaded files
            ''' </summary>
            undefined
        End Enum

        ''' <summary>
        ''' Defines the customFormatID
        ''' </summary>
        Private customFormatIDField As Integer

        ''' <summary>
        ''' Defines the customFormatCode
        ''' </summary>
        Private customFormatCodeField As String

        ''' <summary>
        ''' Gets or sets the raw custom format code in the notation of Excel. <b>The code is not escaped automatically</b>
        ''' </summary>
        ''' <remarks>Currently, there is no auto-escaping applied to custom format strings. For instance, to add a white space, internally it is escaped by a backspace (\ ).
        ''' To get a valid custom format code, this escaping must be applied manually, according to OOXML specs: Part 1 - Fundamentals And Markup Language Reference, Chapter 18.8.31</remarks>
        <Append>
        Public Property CustomFormatCode As String
            Get
                Return customFormatCodeField
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(value) Then
                    Throw New FormatException("A custom format code cannot be null or empty")
                End If
                customFormatCodeField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the format number of the custom format. Must be higher or equal then predefined custom number (164)
        ''' </summary>
        <Append>
        Public Property CustomFormatID As Integer
            Get
                Return customFormatIDField
            End Get
            Set(value As Integer)
                If value < CUSTOMFORMAT_START_NUMBER Then
                    Throw New StyleException("A general style exception occurred", "The number '" & value.ToString() & "' is not a valid custom format ID. Must be at least " & CUSTOMFORMAT_START_NUMBER.ToString())
                End If
                customFormatIDField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether IsCustomFormat
        ''' Gets whether the number format is a custom format (higher or equals 164). If true, the format is custom
        ''' </summary>
        <Append(Ignore:=True)>
        Public ReadOnly Property IsCustomFormat As Boolean
            Get
                If Number = FormatNumber.custom Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the format number. Set this to custom (164) in case of custom number formats
        ''' </summary>
        <Append>
        Public Property Number As FormatNumber

        ''' <summary>
        ''' Initializes a new instance of the <see cref="NumberFormat"/> class
        ''' </summary>
        Public Sub New()
            Number = DEFAULT_NUMBER
            customFormatCodeField = String.Empty
            CustomFormatID = CUSTOMFORMAT_START_NUMBER
        End Sub

        ''' <summary>
        ''' Determines whether a defined style format number represents a date (or date and time)
        ''' </summary>
        ''' <param name="number">Format number to check.</param>
        ''' <returns>True if the format represents a date, otherwise false.</returns>
        Public Shared Function IsDateFormat(number As FormatNumber) As Boolean
            Select Case number
                Case FormatNumber.format_14, FormatNumber.format_15, FormatNumber.format_16, FormatNumber.format_17, FormatNumber.format_22
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Determines whether a defined style format number represents a time)
        ''' </summary>
        ''' <param name="number">Format number to check.</param>
        ''' <returns>True if the format represents a time, otherwise false.</returns>
        Public Shared Function IsTimeFormat(number As FormatNumber) As Boolean
            Select Case number
                Case FormatNumber.format_18, FormatNumber.format_19, FormatNumber.format_20, FormatNumber.format_21, FormatNumber.format_45, FormatNumber.format_46, FormatNumber.format_47
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Tries to parse registered format numbers. If the parsing fails, it is assumed that the number is a custom format number (164 or higher) and 'custom' is returned
        ''' </summary>
        ''' <param name="number">Raw number to parse.</param>
        ''' <param name="formatNumber">Out parameter with the parsed format enum value. If parsing failed, 'custom' will be returned.</param>
        ''' <returns>Format range. Will return 'invalid' if out of any range (e.g. negative value).</returns>
        Public Shared Function TryParseFormatNumber(number As Integer, <Out> ByRef formatNumber As FormatNumber) As FormatRange

            Dim isDefined = [Enum].IsDefined(GetType(FormatNumber), number)
            If isDefined Then
                formatNumber = CType(number, FormatNumber)
                Return FormatRange.defined_format
            End If
            If number < 0 Then
                formatNumber = FormatNumber.none
                Return FormatRange.invalid
            ElseIf number > 0 AndAlso number < CUSTOMFORMAT_START_NUMBER Then
                formatNumber = FormatNumber.none
                Return FormatRange.undefined
            Else
                formatNumber = FormatNumber.custom
                Return FormatRange.custom_format
            End If
        End Function

        ''' <summary>
        ''' Override toString method
        ''' </summary>
        ''' <returns>String of a class.</returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("""NumberFormat"": {" & vbLf)
            AddPropertyAsJson(sb, "CustomFormatCode", CustomFormatCode)
            AddPropertyAsJson(sb, "CustomFormatID", CustomFormatID)
            AddPropertyAsJson(sb, "Number", Number)
            Call AddPropertyAsJson(sb, "HashCode", GetHashCode(), True)
            sb.Append(vbLf & "}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one without casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Overrides Function Copy() As AbstractStyle
            Dim lCopy As NumberFormat = New NumberFormat()
            lCopy.customFormatCodeField = customFormatCodeField
            lCopy.CustomFormatID = CustomFormatID
            lCopy.Number = Number
            Return lCopy
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one with casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Function CopyNumberFormat() As NumberFormat
            Return CType(Copy(), NumberFormat)
        End Function

        ''' <summary>
        ''' Returns a hash code for this instance
        ''' </summary>
        ''' <returns>The <see cref="Integer"/>.</returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode = 495605284
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(CustomFormatCode)
            hashCode = hashCode * -1521134295 + CustomFormatID.GetHashCode()
            hashCode = hashCode * -1521134295 + Number.GetHashCode()
            Return hashCode
        End Function
    End Class


End Namespace