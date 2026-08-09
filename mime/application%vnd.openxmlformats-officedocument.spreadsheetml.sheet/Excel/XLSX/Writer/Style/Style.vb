#Region "Microsoft.VisualBasic::cc22734b5615c8d77741389ed2790804, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\Style.vb"

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

    '   Total Lines: 208
    '    Code Lines: 116 (55.77%)
    ' Comment Lines: 70 (33.65%)
    '    - Xml Docs: 91.43%
    ' 
    '   Blank Lines: 22 (10.58%)
    '     File Size: 9.70 KB


    '     Class Style
    ' 
    '         Properties: CurrentBorder, CurrentCellXf, CurrentFill, CurrentFont, CurrentNumberFormat
    '                     IsInternalStyle, Name
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Append, Copy, CopyStyle, GetHashCode, ToString
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

    End Class

End Namespace
