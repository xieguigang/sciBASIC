#Region "Microsoft.VisualBasic::f6026190b4239d5777df9f2f7cdcd484, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style.vb"

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

    '   Total Lines: 2151
    '    Code Lines: 1224 (56.90%)
    ' Comment Lines: 751 (34.91%)
    '    - Xml Docs: 98.93%
    ' 
    '   Blank Lines: 176 (8.18%)
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

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Class representing a Style with sub classes within a style sheet. An instance of this class
    ''' is only a container for the different sub-classes. These sub-classes contain the actual
    ''' styling information
    ''' </summary>
    Public Class Style : Inherits AbstractStyle

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

    End Class

End Namespace
