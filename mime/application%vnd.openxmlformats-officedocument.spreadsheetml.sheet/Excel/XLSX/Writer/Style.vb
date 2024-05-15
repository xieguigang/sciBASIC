#Region "Microsoft.VisualBasic::ba3c65cf3300c9fe5a3609c8945b98d1, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style.vb"

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

    '   Total Lines: 2149
    '    Code Lines: 1227
    ' Comment Lines: 749
    '   Blank Lines: 173
    '     File Size: 96.94 KB


    '     Class Style
    ' 
    '         Properties: BottomColor, BottomStyle, CurrentBorder, CurrentCellXf, CurrentFill
    '                     CurrentFont, CurrentNumberFormat, DiagonalColor, DiagonalDown, DiagonalStyle
    '                     DiagonalUp, IsInternalStyle, LeftColor, LeftStyle, Name
    '                     RightColor, RightStyle, TopColor, TopStyle
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Append, (+2 Overloads) Copy, CopyBorder, CopyStyle, (+2 Overloads) GetHashCode
    '                   GetStyleName, IsEmpty, (+2 Overloads) ToString
    '         Class Border
    ' 
    ' 
    '             Enum StyleValue
    ' 
    '                 dashDot, dashDotDot, dashed, dotted, hair
    '                 medium, mediumDashDot, mediumDashDotDot, mediumDashed, none
    '                 s_double, slantDashDot, thick, thin
    ' 
    ' 
    ' 
    ' 
    ' 
    '         Class CellXf
    ' 
    ' 
    '             Enum HorizontalAlignValue
    ' 
    '                 center, centerContinuous, distributed, fill, general
    '                 justify, left, none, right
    ' 
    ' 
    ' 
    '             Enum TextBreakValue
    ' 
    '                 none, shrinkToFit, wrapText
    ' 
    '  
    ' 
    ' 
    ' 
    '             Enum TextDirectionValue
    ' 
    '                 horizontal, vertical
    ' 
    '  
    ' 
    ' 
    ' 
    '             Enum VerticalAlignValue
    ' 
    '                 bottom, center, distributed, justify, none
    '                 top
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Alignment, ForceApplyAlignment, Hidden, HorizontalAlign, Indent
    '                 Locked, TextDirection, TextRotation, VerticalAlign
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CalculateInternalRotation, Copy, CopyCellXf, GetHashCode, ToString
    ' 
    '         Class Fill
    ' 
    ' 
    '             Enum FillType
    ' 
    '                 fillColor, patternColor
    ' 
    ' 
    ' 
    '             Enum PatternValue
    ' 
    '                 darkGray, gray0625, gray125, lightGray, mediumGray
    '                 none, solid
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: BackgroundColor, ForegroundColor, IndexedColor, PatternFill
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: Copy, CopyFill, GetHashCode, GetPatternName, ToString
    ' 
    '     Sub: SetColor, ValidateColor
    ' 
    '         Class Font
    ' 
    ' 
    '             Enum SchemeValue
    ' 
    '                 major, minor, none
    ' 
    ' 
    ' 
    '             Enum VerticalAlignValue
    ' 
    '                 bottom, center, distributed, justify, none
    '                 top
    ' 
    '  
    ' 
    ' 
    ' 
    '             Enum UnderlineValue
    ' 
    '                 doubleAccounting, none, singleAccounting, u_double, u_single
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Bold, Charset, ColorTheme, ColorValue, Family
    '                 IsDefaultFont, Italic, Name, Scheme, Size
    '                 Strike, Underline, VerticalAlign
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Copy, CopyFont, GetHashCode, ToString
    ' 
    '         Class NumberFormat
    ' 
    ' 
    '             Enum FormatNumber
    ' 
    ' 
    ' 
    ' 
    '             Enum FormatRange
    ' 
    '                 custom_format, defined_format, invalid, undefined
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: CustomFormatCode, CustomFormatID, IsCustomFormat, Number
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Copy, CopyNumberFormat, GetHashCode, IsDateFormat, IsTimeFormat
    '               ToString, TryParseFormatNumber
    ' 
    '         Class BasicStyles
    ' 
    ' 
    '             Enum StyleEnum
    ' 
    '                 bold, boldItalic, borderFrame, borderFrameHeader, dateFormat
    '                 dottedFill_0_125, doubleUnderline, italic, mergeCellStyle, roundFormat
    '                 strike, timeFormat, underline
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Bold, BoldItalic, BorderFrame, BorderFrameHeader, DateFormat
    '                 DottedFill_0_125, DoubleUnderline, Italic, MergeCellStyle, RoundFormat
    '                 Strike, TimeFormat, Underline
    ' 
    '     Function: ColorizedBackground, ColorizedText, Font, GetStyle
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class AbstractStyle
    ' 
    '         Properties: InternalID
    ' 
    '         Function: CompareTo, Equals, HandleProperties
    ' 
    '         Sub: AddPropertyAsJson, CopyProperties
    '         Class AppendAttribute
    ' 
    '             Properties: Ignore, NestedProperty
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  PicoXLSX is a small .NET library to generate XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
'  Copyright Raphael Stoeckli © 2023
'  This library is licensed under the MIT License.
'  You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
' 

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a Style with sub classes within a style sheet. An instance of this class is only a container for the different sub-classes. These sub-classes contain the actual styling information
    ''' </summary>
    Public Class Style
        Inherits AbstractStyle
        ''' <summary>
        ''' Defines the internalStyle
        ''' </summary>
        Private ReadOnly internalStyle As Boolean

        ''' <summary>
        ''' Gets or sets the current Border object of the style
        ''' </summary>
        <Append(NestedProperty:=True)>
        Public Property CurrentBorder As Border

        ''' <summary>
        ''' Gets or sets the current CellXf object of the style
        ''' </summary>
        <Append(NestedProperty:=True)>
        Public Property CurrentCellXf As CellXf

        ''' <summary>
        ''' Gets or sets the current Fill object of the style
        ''' </summary>
        <Append(NestedProperty:=True)>
        Public Property CurrentFill As Fill

        ''' <summary>
        ''' Gets or sets the current Font object of the style
        ''' </summary>
        <Append(NestedProperty:=True)>
        Public Property CurrentFont As Font

        ''' <summary>
        ''' Gets or sets the current NumberFormat object of the style
        ''' </summary>
        <Append(NestedProperty:=True)>
        Public Property CurrentNumberFormat As NumberFormat

        ''' <summary>
        ''' Gets or sets the name of the informal style. If not defined, the automatically calculated hash will be used as name
        ''' </summary>
        <Append(Ignore:=True)>
        Public Property Name As String

        ''' <summary>
        ''' Gets a value indicating whether IsInternalStyle
        ''' Gets whether the style is system internal. Such styles are not meant to be altered
        ''' </summary>
        <Append(Ignore:=True)>
        Public ReadOnly Property IsInternalStyle As Boolean
            Get
                Return internalStyle
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Style"/> class
        ''' </summary>
        Public Sub New()
            CurrentBorder = New Border()
            CurrentCellXf = New CellXf()
            CurrentFill = New Fill()
            CurrentFont = New Font()
            CurrentNumberFormat = New NumberFormat()
            Name = GetHashCode().ToString()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Style"/> class
        ''' </summary>
        ''' <param name="name">Name of the style.</param>
        Public Sub New(name As String)
            CurrentBorder = New Border()
            CurrentCellXf = New CellXf()
            CurrentFill = New Fill()
            CurrentFont = New Font()
            CurrentNumberFormat = New NumberFormat()
            Me.Name = name
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Style"/> class
        ''' </summary>
        ''' <param name="name">Name of the style.</param>
        ''' <param name="forcedOrder">Number of the style for sorting purpose. The style will be placed at this position (internal use only).</param>
        ''' <param name="internalStyle">If true, the style is marked as internal.</param>
        Public Sub New(name As String, forcedOrder As Integer, internalStyle As Boolean)
            CurrentBorder = New Border()
            CurrentCellXf = New CellXf()
            CurrentFill = New Fill()
            CurrentFont = New Font()
            CurrentNumberFormat = New NumberFormat()
            Me.Name = name
            InternalID = forcedOrder
            Me.internalStyle = internalStyle
        End Sub

        ''' <summary>
        ''' Appends the specified style parts to the current one. The parts can be instances of sub-classes like Border or CellXf or a Style instance. Only the altered properties of the specified style or style part that differs from a new / untouched style instance will be appended. This enables method chaining
        ''' </summary>
        ''' <param name="styleToAppend">The style to append or a sub-class of Style.</param>
        ''' <returns>Current style with appended style parts.</returns>
        Public Function Append(styleToAppend As AbstractStyle) As Style
            If styleToAppend Is Nothing Then
                Return Me
            End If
            If styleToAppend.GetType() Is GetType(Border) Then
                CurrentBorder.CopyProperties(CType(styleToAppend, Border), New Border())
            ElseIf styleToAppend.GetType() Is GetType(CellXf) Then
                CurrentCellXf.CopyProperties(CType(styleToAppend, CellXf), New CellXf())
            ElseIf styleToAppend.GetType() Is GetType(Fill) Then
                CurrentFill.CopyProperties(CType(styleToAppend, Fill), New Fill())
            ElseIf styleToAppend.GetType() Is GetType(Font) Then
                CurrentFont.CopyProperties(CType(styleToAppend, Font), New Font())
            ElseIf styleToAppend.GetType() Is GetType(NumberFormat) Then
                CurrentNumberFormat.CopyProperties(CType(styleToAppend, NumberFormat), New NumberFormat())
            ElseIf styleToAppend.GetType() Is GetType(Style) Then
                CurrentBorder.CopyProperties(CType(styleToAppend, Style).CurrentBorder, New Border())
                CurrentCellXf.CopyProperties(CType(styleToAppend, Style).CurrentCellXf, New CellXf())
                CurrentFill.CopyProperties(CType(styleToAppend, Style).CurrentFill, New Fill())
                CurrentFont.CopyProperties(CType(styleToAppend, Style).CurrentFont, New Font())
                CurrentNumberFormat.CopyProperties(CType(styleToAppend, Style).CurrentNumberFormat, New NumberFormat())
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Override toString method
        ''' </summary>
        ''' <returns>String of a class instance.</returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("{" & vbLf & """Style"": {" & vbLf)
            AddPropertyAsJson(sb, "Name", Name)
            Call AddPropertyAsJson(sb, "HashCode", GetHashCode())
            sb.Append(CStr(CurrentBorder.ToString())).Append("," & vbLf)
            sb.Append(CStr(CurrentCellXf.ToString())).Append("," & vbLf)
            sb.Append(CStr(CurrentFill.ToString())).Append("," & vbLf)
            sb.Append(CStr(CurrentFont.ToString())).Append("," & vbLf)
            sb.Append(CStr(CurrentNumberFormat.ToString())).Append(vbLf & "}" & vbLf & "}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Returns a hash code for this instance
        ''' </summary>
        ''' <returns>The <see cref="Integer"/>.</returns>
        Public Overrides Function GetHashCode() As Integer
            If CurrentBorder Is Nothing OrElse CurrentCellXf Is Nothing OrElse CurrentFill Is Nothing OrElse CurrentFont Is Nothing OrElse CurrentNumberFormat Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The hash of the style could not be created because one or more components are missing as references")
            End If

            Dim p = 241
            Dim r = 1
            r *= p + CurrentBorder.GetHashCode()
            r *= p + CurrentCellXf.GetHashCode()
            r *= p + CurrentFill.GetHashCode()
            r *= p + CurrentFont.GetHashCode()
            r *= p + CurrentNumberFormat.GetHashCode()
            Return r
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one without casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Overrides Function Copy() As AbstractStyle
            If CurrentBorder Is Nothing OrElse CurrentCellXf Is Nothing OrElse CurrentFill Is Nothing OrElse CurrentFont Is Nothing OrElse CurrentNumberFormat Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style could not be copied because one or more components are missing as references")
            End If
            Dim lCopy As Style = New Style()
            lCopy.CurrentBorder = CurrentBorder.CopyBorder()
            lCopy.CurrentCellXf = CurrentCellXf.CopyCellXf()
            lCopy.CurrentFill = CurrentFill.CopyFill()
            lCopy.CurrentFont = CurrentFont.CopyFont()
            lCopy.CurrentNumberFormat = CurrentNumberFormat.CopyNumberFormat()
            Return lCopy
        End Function

        ''' <summary>
        ''' Method to copy the current object to a new one with casting
        ''' </summary>
        ''' <returns>Copy of the current object without the internal ID.</returns>
        Public Function CopyStyle() As Style
            Return CType(Copy(), Style)
        End Function

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
            Private bottomColorField As String

            ''' <summary>
            ''' Defines the diagonalColor
            ''' </summary>
            Private diagonalColorField As String

            ''' <summary>
            ''' Defines the leftColor
            ''' </summary>
            Private leftColorField As String

            ''' <summary>
            ''' Defines the rightColor
            ''' </summary>
            Private rightColorField As String

            ''' <summary>
            ''' Defines the topColor
            ''' </summary>
            Private topColorField As String

            ''' <summary>
            ''' Enum for the border style
            ''' </summary>
            Public Enum StyleValue
                ''' <summary>no border</summary>
                none
                ''' <summary>hair border</summary>
                hair
                ''' <summary>dotted border</summary>
                dotted
                ''' <summary>dashed border with double-dots</summary>
                dashDotDot
                ''' <summary>dash-dotted border</summary>
                dashDot
                ''' <summary>dashed border</summary>
                dashed
                ''' <summary>thin border</summary>
                thin
                ''' <summary>medium-dashed border with double-dots</summary>
                mediumDashDotDot
                ''' <summary>slant dash-dotted border</summary>
                slantDashDot
                ''' <summary>medium dash-dotted border</summary>
                mediumDashDot
                ''' <summary>medium dashed border</summary>
                mediumDashed
                ''' <summary>medium border</summary>
                medium
                ''' <summary>thick border</summary>
                thick
                ''' <summary>double border</summary>
                s_double
            End Enum

            ''' <summary>
            ''' Gets or sets the color code of the bottom border. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
            ''' </summary>
            <Append>
            Public Property BottomColor As String
                Get
                    Return bottomColorField
                End Get
                Set(value As String)
                    Fill.ValidateColor(value, True, True)
                    bottomColorField = value
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
                    Return diagonalColorField
                End Get
                Set(value As String)
                    Fill.ValidateColor(value, True, True)
                    diagonalColorField = value
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
                    Return leftColorField
                End Get
                Set(value As String)
                    Fill.ValidateColor(value, True, True)
                    leftColorField = value
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
                    Return rightColorField
                End Get
                Set(value As String)
                    Fill.ValidateColor(value, True, True)
                    rightColorField = value
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
                    Return topColorField
                End Get
                Set(value As String)
                    Fill.ValidateColor(value, True, True)
                    topColorField = value
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

        ''' <summary>
        ''' Class representing an XF entry. The XF entry is used to make reference to other style instances like Border or Fill and for the positioning of the cell content
        ''' </summary>
        Public Class CellXf
            Inherits AbstractStyle
            ''' <summary>
            ''' Default horizontal align value as constant
            ''' </summary>
            Public Shared ReadOnly DEFAULT_HORIZONTAL_ALIGNMENT As HorizontalAlignValue = HorizontalAlignValue.none

            ''' <summary>
            ''' Default text break value as constant
            ''' </summary>
            Public Shared ReadOnly DEFAULT_ALIGNMENT As TextBreakValue = TextBreakValue.none

            ''' <summary>
            ''' Default text direction value as constant
            ''' </summary>
            Public Shared ReadOnly DEFAULT_TEXT_DIRECTION As TextDirectionValue = TextDirectionValue.horizontal

            ''' <summary>
            ''' Default vertical align value as constant
            ''' </summary>
            Public Shared ReadOnly DEFAULT_VERTICAL_ALIGNMENT As VerticalAlignValue = VerticalAlignValue.none

            ''' <summary>
            ''' Enum for the horizontal alignment of a cell 
            ''' </summary>
            Public Enum HorizontalAlignValue
                ''' <summary>Content will be aligned left</summary>
                left
                ''' <summary>Content will be aligned in the center</summary>
                center
                ''' <summary>Content will be aligned right</summary>
                right
                ''' <summary>Content will fill up the cell</summary>
                fill
                ''' <summary>justify alignment</summary>
                justify
                ''' <summary>General alignment</summary>
                general
                ''' <summary>Center continuous alignment</summary>
                centerContinuous
                ''' <summary>Distributed alignment</summary>
                distributed
                ''' <summary>No alignment. The alignment will not be used in a style</summary>
                none
            End Enum

            ''' <summary>
            ''' Enum for text break options
            ''' </summary>
            Public Enum TextBreakValue
                ''' <summary>Word wrap is active</summary>
                wrapText
                ''' <summary>Text will be resized to fit the cell</summary>
                shrinkToFit
                ''' <summary>Text will overflow in cell</summary>
                none
            End Enum

            ''' <summary>
            ''' Enum for the general text alignment direction
            ''' </summary>
            Public Enum TextDirectionValue
                ''' <summary>Text direction is horizontal (default)</summary>
                horizontal
                ''' <summary>Text direction is vertical</summary>
                vertical
            End Enum

            ''' <summary>
            ''' Enum for the vertical alignment of a cell 
            ''' </summary>
            Public Enum VerticalAlignValue
                ''' <summary>Content will be aligned on the bottom (default)</summary>
                bottom
                ''' <summary>Content will be aligned on the top</summary>
                top
                ''' <summary>Content will be aligned in the center</summary>
                center
                ''' <summary>justify alignment</summary>
                justify
                ''' <summary>Distributed alignment</summary>
                distributed
                ''' <summary>No alignment. The alignment will not be used in a style</summary>
                none
            End Enum

            ''' <summary>
            ''' Defines the textRotation
            ''' </summary>
            Private textRotationField As Integer

            ''' <summary>
            ''' Defines the textDirection
            ''' </summary>
            Private textDirectionField As TextDirectionValue

            ''' <summary>
            ''' Defines the indent
            ''' </summary>
            Private indentField As Integer

            ''' <summary>
            ''' Gets or sets a value indicating whether ForceApplyAlignment
            ''' Gets or sets whether the applyAlignment property (used to merge cells) will be defined in the XF entry of the style. If true, applyAlignment will be defined
            ''' </summary>
            <Append>
            Public Property ForceApplyAlignment As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether Hidden
            ''' Gets or sets whether the hidden property (used for protection or hiding of cells) will be defined in the XF entry of the style. If true, hidden will be defined
            ''' </summary>
            <Append>
            Public Property Hidden As Boolean

            ''' <summary>
            ''' Gets or sets the horizontal alignment of the style
            ''' </summary>
            <Append>
            Public Property HorizontalAlign As HorizontalAlignValue

            ''' <summary>
            ''' Gets or sets a value indicating whether Locked
            ''' Gets or sets whether the locked property (used for locking / protection of cells or worksheets) will be defined in the XF entry of the style. If true, locked will be defined
            ''' </summary>
            <Append>
            Public Property Locked As Boolean

            ''' <summary>
            ''' Gets or sets the text break options of the style
            ''' </summary>
            <Append>
            Public Property Alignment As TextBreakValue

            ''' <summary>
            ''' Gets or sets the direction of the text within the cell
            ''' </summary>
            <Append>
            Public Property TextDirection As TextDirectionValue
                Get
                    Return textDirectionField
                End Get
                Set(value As TextDirectionValue)
                    textDirectionField = value
                    CalculateInternalRotation()
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the text rotation in degrees (from +90 to -90)
            ''' </summary>
            <Append>
            Public Property TextRotation As Integer
                Get
                    Return textRotationField
                End Get
                Set(value As Integer)
                    textRotationField = value
                    TextDirection = TextDirectionValue.horizontal
                    CalculateInternalRotation()
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the vertical alignment of the style
            ''' </summary>
            <Append>
            Public Property VerticalAlign As VerticalAlignValue

            ''' <summary>
            ''' Gets or sets the indentation in case of left, right or distributed alignment. If 0, no alignment is applied
            ''' </summary>
            <Append>
            Public Property Indent As Integer
                Get
                    Return indentField
                End Get
                Set(value As Integer)
                    If value >= 0 Then
                        indentField = value
                    Else
                        Throw New StyleException("A general style exception occurred", "The indent value '" & value.ToString() & "' is not valid. It must be >= 0")
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Initializes a new instance of the <see cref="CellXf"/> class
            ''' </summary>
            Public Sub New()
                HorizontalAlign = DEFAULT_HORIZONTAL_ALIGNMENT
                Alignment = DEFAULT_ALIGNMENT
                textDirectionField = DEFAULT_TEXT_DIRECTION
                VerticalAlign = DEFAULT_VERTICAL_ALIGNMENT
                textRotationField = 0
                Indent = 0
            End Sub

            ''' <summary>
            ''' Method to calculate the internal text rotation. The text direction and rotation are handled internally by the text rotation value
            ''' </summary>
            ''' <returns>Returns the valid rotation in degrees for internal use (LowLevel).</returns>
            Friend Function CalculateInternalRotation() As Integer
                If textRotationField < -90 OrElse textRotationField > 90 Then
                    Throw New FormatException("The rotation value (" & textRotationField.ToString() & "°) is out of range. Range is form -90° to +90°")
                End If
                If textDirectionField = TextDirectionValue.vertical Then
                    textRotationField = 255
                    Return textRotationField
                Else
                    If textRotationField >= 0 Then
                        Return textRotationField
                    Else
                        Return 90 - textRotationField
                    End If
                End If
            End Function

            ''' <summary>
            ''' Override toString method
            ''' </summary>
            ''' <returns>String of a class instance.</returns>
            Public Overrides Function ToString() As String
                Dim sb As StringBuilder = New StringBuilder()
                sb.Append("""StyleXF"": {" & vbLf)
                AddPropertyAsJson(sb, "HorizontalAlign", HorizontalAlign)
                AddPropertyAsJson(sb, "Alignment", Alignment)
                AddPropertyAsJson(sb, "TextDirection", TextDirection)
                AddPropertyAsJson(sb, "TextRotation", TextRotation)
                AddPropertyAsJson(sb, "VerticalAlign", VerticalAlign)
                AddPropertyAsJson(sb, "ForceApplyAlignment", ForceApplyAlignment)
                AddPropertyAsJson(sb, "Locked", Locked)
                AddPropertyAsJson(sb, "Hidden", Hidden)
                AddPropertyAsJson(sb, "Indent", Indent)
                Call AddPropertyAsJson(sb, "HashCode", GetHashCode(), True)
                sb.Append(vbLf & "}")
                Return sb.ToString()
            End Function

            ''' <summary>
            ''' Returns a hash code for this instance
            ''' </summary>
            ''' <returns>The <see cref="Integer"/>.</returns>
            Public Overrides Function GetHashCode() As Integer
                Dim hashCode = 626307906
                hashCode = hashCode * -1521134295 + ForceApplyAlignment.GetHashCode()
                hashCode = hashCode * -1521134295 + Hidden.GetHashCode()
                hashCode = hashCode * -1521134295 + HorizontalAlign.GetHashCode()
                hashCode = hashCode * -1521134295 + Locked.GetHashCode()
                hashCode = hashCode * -1521134295 + Alignment.GetHashCode()
                hashCode = hashCode * -1521134295 + TextDirection.GetHashCode()
                hashCode = hashCode * -1521134295 + TextRotation.GetHashCode()
                hashCode = hashCode * -1521134295 + VerticalAlign.GetHashCode()
                hashCode = hashCode * -1521134295 + Indent.GetHashCode()
                Return hashCode
            End Function

            ''' <summary>
            ''' Method to copy the current object to a new one without casting
            ''' </summary>
            ''' <returns>Copy of the current object without the internal ID.</returns>
            Public Overrides Function Copy() As AbstractStyle
                Dim lCopy As CellXf = New CellXf()
                lCopy.HorizontalAlign = HorizontalAlign
                lCopy.Alignment = Alignment
                lCopy.TextDirection = TextDirection
                lCopy.TextRotation = TextRotation
                lCopy.VerticalAlign = VerticalAlign
                lCopy.ForceApplyAlignment = ForceApplyAlignment
                lCopy.Locked = Locked
                lCopy.Hidden = Hidden
                lCopy.Indent = Indent
                Return lCopy
            End Function

            ''' <summary>
            ''' Method to copy the current object to a new one with casting
            ''' </summary>
            ''' <returns>Copy of the current object without the internal ID.</returns>
            Public Function CopyCellXf() As CellXf
                Return CType(Copy(), CellXf)
            End Function
        End Class

        ''' <summary>
        ''' Class representing a Fill (background) entry. The Fill entry is used to define background colors and fill patterns
        ''' </summary>
        Public Class Fill
            Inherits AbstractStyle
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
            ''' Enum for the type of the color
            ''' </summary>
            Public Enum FillType
                ''' <summary>Color defines a pattern color </summary>
                patternColor
                ''' <summary>Color defines a solid fill color </summary>
                fillColor
            End Enum

            ''' <summary>
            ''' Enum for the pattern values
            ''' </summary>
            Public Enum PatternValue
                ''' <summary>No pattern (default)</summary>
                none
                ''' <summary>Solid fill (for colors)</summary>
                solid
                ''' <summary>Dark gray fill</summary>
                darkGray
                ''' <summary>Medium gray fill</summary>
                mediumGray
                ''' <summary>Light gray fill</summary>
                lightGray
                ''' <summary>6.25% gray fill</summary>
                gray0625
                ''' <summary>12.5% gray fill</summary>
                gray125
            End Enum

            ''' <summary>
            ''' Defines the backgroundColor
            ''' </summary>
            Private backgroundColorField As String = DEFAULT_COLOR

            ''' <summary>
            ''' Defines the foregroundColor
            ''' </summary>
            Private foregroundColorField As String = DEFAULT_COLOR

            ''' <summary>
            ''' Gets or sets the background color of the fill. The value is expressed as hex string with the format AARRGGBB. AA (Alpha) is usually FF
            ''' </summary>
            <Append>
            Public Property BackgroundColor As String
                Get
                    Return backgroundColorField
                End Get
                Set(value As String)
                    ValidateColor(value, True)
                    backgroundColorField = value
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
                    Return foregroundColorField
                End Get
                Set(value As String)
                    ValidateColor(value, True)
                    foregroundColorField = value
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
                IndexedColor = DEFAULT_INDEXED_COLOR
                PatternFill = DEFAULT_PATTERN_FILL
                ForegroundColor = DEFAULT_COLOR
                BackgroundColor = DEFAULT_COLOR
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the <see cref="Fill"/> class
            ''' </summary>
            ''' <param name="foreground">Foreground color of the fill.</param>
            ''' <param name="background">Background color of the fill.</param>
            Public Sub New(foreground As String, background As String)
                BackgroundColor = background
                ForegroundColor = foreground
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
                    backgroundColorField = DEFAULT_COLOR
                    ForegroundColor = value
                Else
                    BackgroundColor = value
                    foregroundColorField = DEFAULT_COLOR
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
                lCopy.BackgroundColor = BackgroundColor
                lCopy.ForegroundColor = ForegroundColor
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
                    backgroundColorField = DEFAULT_COLOR
                    ForegroundColor = value
                Else
                    BackgroundColor = value
                    foregroundColorField = DEFAULT_COLOR
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
                If String.IsNullOrEmpty(hexCode) Then
                    If allowEmpty Then
                        Return
                    End If
                    Throw New StyleException("A general style exception occurred", "The color expression was null or empty")
                End If
                Dim length As Integer
                length = If(useAlpha, 8, 6)
                If hexCode Is Nothing OrElse hexCode.Length <> length Then
                    Throw New StyleException("A general style exception occurred", "The value '" & hexCode & "' is invalid. A valid value must contain six hex characters")
                End If
                If Not Regex.IsMatch(hexCode, "[a-fA-F0-9]{6,8}") Then
                    Throw New StyleException("A general style exception occurred", "The expression '" & hexCode & "' is not a valid hex value")
                End If
            End Sub
        End Class

        ''' <summary>
        ''' Class representing a Font entry. The Font entry is used to define text formatting
        ''' </summary>
        Public Class Font
            Inherits AbstractStyle
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
            ''' Enum for the font scheme
            ''' </summary>
            Public Enum SchemeValue
                ''' <summary>Font scheme is major</summary>
                major
                ''' <summary>Font scheme is minor (default)</summary>
                minor
                ''' <summary>No Font scheme is used</summary>
                none
            End Enum

            ''' <summary>
            ''' Enum for the vertical alignment of the text from base line
            ''' </summary>
            Public Enum VerticalAlignValue
                ' baseline, // Maybe not used in Excel
                ''' <summary>Text will be rendered as subscript</summary>
                subscript
                ''' <summary>Text will be rendered as superscript</summary>
                superscript
                ''' <summary>Text will be rendered normal</summary>
                none
            End Enum

            ''' <summary>
            ''' Enum for the style of the underline property of a stylized text
            ''' </summary>
            Public Enum UnderlineValue
                ''' <summary>Text contains a single underline</summary>
                u_single
                ''' <summary>Text contains a double underline</summary>
                u_double
                ''' <summary>Text contains a single, accounting underline</summary>
                singleAccounting
                ''' <summary>Text contains a double, accounting underline</summary>
                doubleAccounting
                ''' <summary>Text contains no underline (default)</summary>
                none
            End Enum

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

        ''' <summary>
        ''' Factory class with the most important predefined styles
        ''' </summary>
        Public NotInheritable Class BasicStyles
            ''' <summary>
            ''' Enum with style selection
            ''' </summary>
            Private Enum StyleEnum
                ''' <summary>Format text bold</summary>
                bold
                ''' <summary>Format text italic</summary>
                italic
                ''' <summary>Format text bold and italic</summary>
                boldItalic
                ''' <summary>Format text with an underline</summary>
                underline
                ''' <summary>Format text with a double underline</summary>
                doubleUnderline
                ''' <summary>Format text with a strike-through</summary>
                strike
                ''' <summary>Format number as date</summary>
                dateFormat
                ''' <summary>Format number as time</summary>
                timeFormat
                ''' <summary>Rounds number as an integer</summary>
                roundFormat
                ''' <summary>Format cell with a thin border</summary>
                borderFrame
                ''' <summary>Format cell with a thin border and a thick bottom line as header cell</summary>
                borderFrameHeader
                ''' <summary>Special pattern fill style for compatibility purpose </summary>
                dottedFill_0_125
                ''' <summary>Style to apply on merged cells </summary>
                mergeCellStyle
            End Enum

            ''' <summary>
            ''' Defines the bold, italic, boldItalic, underline, doubleUnderline, strike, dateFormat, timeFormat, roundFormat, borderFrame, borderFrameHeader, dottedFill_0_125, mergeCellStyle
            ''' </summary>
            Private Shared boldField, italicField, boldItalicField, underlineField, doubleUnderlineField, strikeField, dateFormatField, timeFormatField, roundFormatField, borderFrameField, borderFrameHeaderField, dottedFill_0_125Field, mergeCellStyleField As Style

            ''' <summary>
            ''' Gets the Bold
            ''' </summary>
            Public Shared ReadOnly Property Bold As Style
                Get
                    Return GetStyle(StyleEnum.bold)
                End Get
            End Property

            ''' <summary>
            ''' Gets the BoldItalic
            ''' </summary>
            Public Shared ReadOnly Property BoldItalic As Style
                Get
                    Return GetStyle(StyleEnum.boldItalic)
                End Get
            End Property

            ''' <summary>
            ''' Gets the BorderFrame
            ''' </summary>
            Public Shared ReadOnly Property BorderFrame As Style
                Get
                    Return GetStyle(StyleEnum.borderFrame)
                End Get
            End Property

            ''' <summary>
            ''' Gets the BorderFrameHeader
            ''' </summary>
            Public Shared ReadOnly Property BorderFrameHeader As Style
                Get
                    Return GetStyle(StyleEnum.borderFrameHeader)
                End Get
            End Property

            ''' <summary>
            ''' Gets the DateFormat
            ''' </summary>
            Public Shared ReadOnly Property DateFormat As Style
                Get
                    Return GetStyle(StyleEnum.dateFormat)
                End Get
            End Property

            ''' <summary>
            ''' Gets the TimeFormat
            ''' </summary>
            Public Shared ReadOnly Property TimeFormat As Style
                Get
                    Return GetStyle(StyleEnum.timeFormat)
                End Get
            End Property

            ''' <summary>
            ''' Gets the DoubleUnderline
            ''' </summary>
            Public Shared ReadOnly Property DoubleUnderline As Style
                Get
                    Return GetStyle(StyleEnum.doubleUnderline)
                End Get
            End Property

            ''' <summary>
            ''' Gets the DottedFill_0_125
            ''' </summary>
            Public Shared ReadOnly Property DottedFill_0_125 As Style
                Get
                    Return GetStyle(StyleEnum.dottedFill_0_125)
                End Get
            End Property

            ''' <summary>
            ''' Gets the Italic
            ''' </summary>
            Public Shared ReadOnly Property Italic As Style
                Get
                    Return GetStyle(StyleEnum.italic)
                End Get
            End Property

            ''' <summary>
            ''' Gets the MergeCellStyle
            ''' </summary>
            Public Shared ReadOnly Property MergeCellStyle As Style
                Get
                    Return GetStyle(StyleEnum.mergeCellStyle)
                End Get
            End Property

            ''' <summary>
            ''' Gets the RoundFormat
            ''' </summary>
            Public Shared ReadOnly Property RoundFormat As Style
                Get
                    Return GetStyle(StyleEnum.roundFormat)
                End Get
            End Property

            ''' <summary>
            ''' Gets the Strike
            ''' </summary>
            Public Shared ReadOnly Property Strike As Style
                Get
                    Return GetStyle(StyleEnum.strike)
                End Get
            End Property

            ''' <summary>
            ''' Gets the Underline
            ''' </summary>
            Public Shared ReadOnly Property Underline As Style
                Get
                    Return GetStyle(StyleEnum.underline)
                End Get
            End Property

            ''' <summary>
            ''' Method to maintain the styles and to create singleton instances
            ''' </summary>
            ''' <param name="value">Enum value to maintain.</param>
            ''' <returns>The style according to the passed enum value.</returns>
            Private Shared Function GetStyle(value As StyleEnum) As Style
                Dim s As Style = Nothing
                Select Case value
                    Case StyleEnum.bold
                        If boldField Is Nothing Then
                            boldField = New Style()
                            boldField.CurrentFont.Bold = True
                        End If
                        s = boldField
                    Case StyleEnum.italic
                        If italicField Is Nothing Then
                            italicField = New Style()
                            italicField.CurrentFont.Italic = True
                        End If
                        s = italicField
                    Case StyleEnum.boldItalic
                        If boldItalicField Is Nothing Then
                            boldItalicField = New Style()
                            boldItalicField.CurrentFont.Italic = True
                            boldItalicField.CurrentFont.Bold = True
                        End If
                        s = boldItalicField
                    Case StyleEnum.underline
                        If underlineField Is Nothing Then
                            underlineField = New Style()
                            underlineField.CurrentFont.Underline = Style.Font.UnderlineValue.u_single
                        End If
                        s = underlineField
                    Case StyleEnum.doubleUnderline
                        If doubleUnderlineField Is Nothing Then
                            doubleUnderlineField = New Style()
                            doubleUnderlineField.CurrentFont.Underline = Style.Font.UnderlineValue.u_double
                        End If
                        s = doubleUnderlineField
                    Case StyleEnum.strike
                        If strikeField Is Nothing Then
                            strikeField = New Style()
                            strikeField.CurrentFont.Strike = True
                        End If
                        s = strikeField
                    Case StyleEnum.dateFormat
                        If dateFormatField Is Nothing Then
                            dateFormatField = New Style()
                            dateFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_14
                        End If
                        s = dateFormatField
                    Case StyleEnum.timeFormat
                        If timeFormatField Is Nothing Then
                            timeFormatField = New Style()
                            timeFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_21
                        End If
                        s = timeFormatField
                    Case StyleEnum.roundFormat
                        If roundFormatField Is Nothing Then
                            roundFormatField = New Style()
                            roundFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_1
                        End If
                        s = roundFormatField
                    Case StyleEnum.borderFrame
                        If borderFrameField Is Nothing Then
                            borderFrameField = New Style()
                            borderFrameField.CurrentBorder.TopStyle = Border.StyleValue.thin
                            borderFrameField.CurrentBorder.BottomStyle = Border.StyleValue.thin
                            borderFrameField.CurrentBorder.LeftStyle = Border.StyleValue.thin
                            borderFrameField.CurrentBorder.RightStyle = Border.StyleValue.thin
                        End If
                        s = borderFrameField
                    Case StyleEnum.borderFrameHeader
                        If borderFrameHeaderField Is Nothing Then
                            borderFrameHeaderField = New Style()
                            borderFrameHeaderField.CurrentBorder.TopStyle = Border.StyleValue.thin
                            borderFrameHeaderField.CurrentBorder.BottomStyle = Border.StyleValue.medium
                            borderFrameHeaderField.CurrentBorder.LeftStyle = Border.StyleValue.thin
                            borderFrameHeaderField.CurrentBorder.RightStyle = Border.StyleValue.thin
                            borderFrameHeaderField.CurrentFont.Bold = True
                        End If
                        s = borderFrameHeaderField
                    Case StyleEnum.dottedFill_0_125
                        If dottedFill_0_125Field Is Nothing Then
                            dottedFill_0_125Field = New Style()
                            dottedFill_0_125Field.CurrentFill.PatternFill = Fill.PatternValue.gray125
                        End If
                        s = dottedFill_0_125Field
                    Case StyleEnum.mergeCellStyle
                        If mergeCellStyleField Is Nothing Then
                            mergeCellStyleField = New Style()
                            mergeCellStyleField.CurrentCellXf.ForceApplyAlignment = True
                        End If
                        s = mergeCellStyleField
                End Select
                Return s.CopyStyle() ' Copy makes basic styles immutable
            End Function

            ''' <summary>
            ''' Gets a style to colorize the text of a cell
            ''' </summary>
            ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
            ''' <returns>Style with font color definition.</returns>
            Public Shared Function ColorizedText(rgb As String) As Style
                Fill.ValidateColor(rgb, False)
                Dim s As Style = New Style()
                s.CurrentFont.ColorValue = "FF" & rgb.ToUpper()
                Return s
            End Function

            ''' <summary>
            ''' Gets a style to colorize the background of a cell
            ''' </summary>
            ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
            ''' <returns>Style with background color definition.</returns>
            Public Shared Function ColorizedBackground(rgb As String) As Style
                Fill.ValidateColor(rgb, False)
                Dim s As Style = New Style()
                s.CurrentFill.SetColor("FF" & rgb.ToUpper(), Fill.FillType.fillColor)
                Return s
            End Function

            ''' <summary>
            ''' Gets a style with a user defined font
            ''' </summary>
            ''' <param name="fontName">Name of the font.</param>
            ''' <param name="fontSize">Size of the font in points (optional; default 11).</param>
            ''' <param name="isBold">If true, the font will be bold (optional; default false).</param>
            ''' <param name="isItalic">If true, the font will be italic (optional; default false).</param>
            ''' <returns>Style with font definition.</returns>
            Public Shared Function Font(fontName As String, Optional fontSize As Integer = 11, Optional isBold As Boolean = False, Optional isItalic As Boolean = False) As Style
                Dim s As Style = New Style()
                s.CurrentFont.Name = fontName
                s.CurrentFont.Size = fontSize
                s.CurrentFont.Bold = isBold
                s.CurrentFont.Italic = isItalic
                Return s
            End Function
        End Class
    End Class

    ''' <summary>
    ''' Class represents an abstract style component
    ''' </summary>
    Public MustInherit Class AbstractStyle
        Implements IComparable(Of AbstractStyle)
        ''' <summary>
        ''' Gets or sets the internal ID for sorting purpose in the Excel style document (nullable)
        ''' </summary>
        <Append(Ignore:=True)>
        Public Property InternalID As Integer?

        ''' <summary>
        ''' Abstract method to copy a component (dereferencing)
        ''' </summary>
        ''' <returns>Returns a copied component.</returns>
        Public MustOverride Function Copy() As AbstractStyle

        ''' <summary>
        ''' Internal method to copy altered properties from a source object. The decision whether a property is copied is dependent on a untouched reference object
        ''' </summary>
        ''' <typeparam name="T">Style or sub-class of Style that extends AbstractStyle.</typeparam>
        ''' <param name="source">Source object with properties to copy.</param>
        ''' <param name="reference">Reference object to decide whether the properties from the source objects are altered or not.</param>
        Friend Sub CopyProperties(Of T As AbstractStyle)(source As T, reference As T)
            If source Is Nothing OrElse [GetType]() IsNot source.GetType() AndAlso [GetType]() IsNot reference.GetType() Then
                Throw New StyleException("CopyPropertyException", "The objects of the source, target and reference for style appending are not of the same type")
            End If
            Dim infos As PropertyInfo() = [GetType]().GetProperties()
            Dim sourceInfo As PropertyInfo
            Dim referenceInfo As PropertyInfo
            Dim attributes As IEnumerable(Of AppendAttribute)
            For Each info As PropertyInfo In infos
                attributes = CType(info.GetCustomAttributes(GetType(AppendAttribute)), IEnumerable(Of AppendAttribute))
                If attributes.Any() AndAlso Not HandleProperties(attributes) Then
                    Continue For
                End If
                sourceInfo = source.GetType().GetProperty(info.Name)
                referenceInfo = reference.GetType().GetProperty(info.Name)
                If Not sourceInfo.GetValue(source).Equals(referenceInfo.GetValue(reference)) Then
                    info.SetValue(Me, sourceInfo.GetValue(source))
                End If
            Next
        End Sub

        ''' <summary>
        ''' Method to check whether a property is considered or skipped
        ''' </summary>
        ''' <param name="attributes">Collection of attributes to check.</param>
        ''' <returns>Returns false as soon a property of the collection is marked as ignored or nested.</returns>
        Private Shared Function HandleProperties(attributes As IEnumerable(Of AppendAttribute)) As Boolean
            For Each attribute In attributes
                If attribute.Ignore OrElse attribute.NestedProperty Then
                    Return False ' skip property
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' Method to compare two objects for sorting purpose
        ''' </summary>
        ''' <param name="other">Other object to compare with this object.</param>
        ''' <returns>-1 if the other object is bigger. 0 if both objects are equal. 1 if the other object is smaller.</returns>
        Public Function CompareTo(other As AbstractStyle) As Integer Implements IComparable(Of AbstractStyle).CompareTo
            If Not InternalID.HasValue Then
                Return -1
            ElseIf other Is Nothing OrElse Not other.InternalID.HasValue Then
                Return 1
            Else
                Return InternalID.Value.CompareTo(other.InternalID.Value)
            End If
        End Function

        ''' <summary>
        ''' Method to compare two objects for sorting purpose
        ''' </summary>
        ''' <param name="other">Other object to compare with this object.</param>
        ''' <returns>True if both objects are equal, otherwise false.</returns>
        Public Overloads Function Equals(other As AbstractStyle) As Boolean
            Return GetHashCode() = other.GetHashCode()
        End Function

        ''' <summary>
        ''' Append a JSON property for debug purpose (used in the ToString methods) to the passed string builder
        ''' </summary>
        ''' <param name="sb">String builder.</param>
        ''' <param name="name">Property name.</param>
        ''' <param name="value">Property value.</param>
        ''' <param name="terminate">If true, no comma and newline will be appended.</param>
        Friend Shared Sub AddPropertyAsJson(sb As StringBuilder, name As String, value As Object, Optional terminate As Boolean = False)
            sb.Append("""").Append(name).Append(""": ")
            If value Is Nothing Then
                sb.Append("""""")
            Else
                sb.Append("""").Append(value.ToString().Replace("""", "\""")).Append("""")
            End If
            If Not terminate Then
                sb.Append("," & vbLf)
            End If
        End Sub

        ''' <summary>
        ''' Attribute designated to control the copying of style properties
        ''' </summary>
        Public Class AppendAttribute
            Inherits Attribute
            ''' <summary>
            ''' Gets or sets a value indicating whether Ignore
            ''' Indicates whether the property annotated with the attribute is ignored during the copying of properties
            ''' </summary>
            Public Property Ignore As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether NestedProperty
            ''' Indicates whether the property annotated with the attribute is a nested property. Nested properties are ignored during the copying of properties but can be broken down to its sub-properties
            ''' </summary>
            Public Property NestedProperty As Boolean

            ''' <summary>
            ''' Initializes a new instance of the <see cref="AppendAttribute"/> class
            ''' </summary>
            Public Sub New()
                Ignore = False
                NestedProperty = False
            End Sub
        End Class
    End Class
End Namespace
