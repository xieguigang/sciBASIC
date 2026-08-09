#Region "Microsoft.VisualBasic::38071ea4268380f4a45a584850d9f4b4, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\CellXf.vb"

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

    '   Total Lines: 229
    '    Code Lines: 127 (55.46%)
    ' Comment Lines: 77 (33.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 25 (10.92%)
    '     File Size: 9.39 KB


    '     Class CellXf
    ' 
    '         Properties: Alignment, ForceApplyAlignment, Hidden, HorizontalAlign, Indent
    '                     Locked, TextDirection, TextRotation, VerticalAlign
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CalculateInternalRotation, Copy, CopyCellXf, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace XLSX.Writer.Styling


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

End Namespace
