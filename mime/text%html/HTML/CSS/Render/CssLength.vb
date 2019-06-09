#Region "Microsoft.VisualBasic::60f7837a32c487036f918513e7d5152a, mime\text%html\HTML\CSS\Render\CssLength.vb"

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

    '     Class CssLength
    ' 
    ' 
    '         Enum CssUnit
    ' 
    '             Centimeters, Ems, Ex, Inches, Milimeters
    '             None, Picas, Pixels, Points
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: HasError, IsPercentage, IsRelative, Length, Number
    '                 Unit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ConvertEmToPixels, ConvertEmToPoints, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports System.Globalization

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents and gets info about a CSS Length
    ''' </summary>
    ''' <remarks>
    ''' http://www.w3.org/TR/CSS21/syndata.html#length-units
    ''' </remarks>
    Public Class CssLength
#Region "Enum"

        ''' <summary>
        ''' Represents the possible units of the CSS lengths
        ''' </summary>
        ''' <remarks>
        ''' http://www.w3.org/TR/CSS21/syndata.html#length-units
        ''' </remarks>
        Public Enum CssUnit
            None

            Ems

            Pixels

            Ex

            Inches

            Centimeters

            Milimeters

            Points

            Picas
        End Enum

#End Region

#Region "Fields"
        Private _number As Single
        Private _isRelative As Boolean
        Private _unit As CssUnit
        Private _length As String
        Private _isPercentage As Boolean
        Private _hasError As Boolean

#End Region

#Region "Ctor"

        ''' <summary>
        ''' Creates a new CssLength from a length specified on a CSS style sheet or fragment
        ''' </summary>
        ''' <param name="length">Length as specified in the Style Sheet or style fragment</param>
        Public Sub New(length As String)
            _length = length
            _number = 0F
            _unit = CssUnit.None
            _isPercentage = False

            'Return zero if no length specified, zero specified
            If String.IsNullOrEmpty(length) OrElse length = "0" Then
                Return
            End If

            'If percentage, use ParseNumber
            If length.EndsWith("%") Then
                _number = CssValue.ParseNumber(length, 1)
                _isPercentage = True
                Return
            End If

            'If no units, has error
            If length.Length < 3 Then
                Single.TryParse(length, _number)
                _hasError = True
                Return
            End If

            'Get units of the length
            Dim u As String = length.Substring(length.Length - 2, 2)

            'Number of the length
            Dim number As String = length.Substring(0, length.Length - 2)

            'TODO: Units behave different in paper and in screen!
            Select Case u
                Case CssConstants.Em
                    _unit = CssUnit.Ems
                    _isRelative = True

                Case CssConstants.Ex
                    _unit = CssUnit.Ex
                    _isRelative = True

                Case CssConstants.Px
                    _unit = CssUnit.Pixels
                    _isRelative = True

                Case CssConstants.Mm
                    _unit = CssUnit.Milimeters

                Case CssConstants.Cm
                    _unit = CssUnit.Centimeters

                Case CssConstants.[In]
                    _unit = CssUnit.Inches

                Case CssConstants.Pt
                    _unit = CssUnit.Points

                Case CssConstants.Pc
                    _unit = CssUnit.Picas

                Case Else
                    _hasError = True
                    Return
            End Select

            If Not Single.TryParse(number, System.Globalization.NumberStyles.Number, NumberFormatInfo.InvariantInfo, _number) Then
                _hasError = True

            End If
        End Sub

#End Region

#Region "Props"

        ''' <summary>
        ''' Gets the number in the length
        ''' </summary>
        Public ReadOnly Property Number() As Single
            Get
                Return _number
            End Get
        End Property

        ''' <summary>
        ''' Gets if the length has some parsing error
        ''' </summary>
        Public ReadOnly Property HasError() As Boolean
            Get
                Return _hasError
            End Get
        End Property


        ''' <summary>
        ''' Gets if the length represents a precentage (not actually a length)
        ''' </summary>
        Public ReadOnly Property IsPercentage() As Boolean
            Get
                Return _isPercentage
            End Get
        End Property


        ''' <summary>
        ''' Gets if the length is specified in relative units
        ''' </summary>
        Public ReadOnly Property IsRelative() As Boolean
            Get
                Return _isRelative
            End Get
        End Property

        ''' <summary>
        ''' Gets the unit of the length
        ''' </summary>
        Public ReadOnly Property Unit() As CssUnit
            Get
                Return _unit
            End Get
        End Property

        ''' <summary>
        ''' Gets the length as specified in the string
        ''' </summary>
        Public ReadOnly Property Length() As String
            Get
                Return _length
            End Get
        End Property


#End Region

#Region "Methods"

        ''' <summary>
        ''' If length is in Ems, returns its value in points
        ''' </summary>
        ''' <param name="emSize">Em size factor to multiply</param>
        ''' <returns>Points size of this em</returns>
        ''' <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        Public Function ConvertEmToPoints(emSize As Single) As CssLength
            If HasError Then
                Throw New InvalidOperationException("Invalid length")
            End If
            If Unit <> CssUnit.Ems Then
                Throw New InvalidOperationException("Length is not in ems")
            End If

            Return New CssLength(String.Format("{0}pt", Convert.ToSingle(Number * emSize).ToString("0.0", NumberFormatInfo.InvariantInfo)))
        End Function

        ''' <summary>
        ''' If length is in Ems, returns its value in pixels
        ''' </summary>
        ''' <param name="pixelFactor">Pixel size factor to multiply</param>
        ''' <returns>Pixels size of this em</returns>
        ''' <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        Public Function ConvertEmToPixels(pixelFactor As Single) As CssLength
            If HasError Then
                Throw New InvalidOperationException("Invalid length")
            End If
            If Unit <> CssUnit.Ems Then
                Throw New InvalidOperationException("Length is not in ems")
            End If

            Return New CssLength(String.Format("{0}px", Convert.ToSingle(Number * pixelFactor).ToString("0.0", NumberFormatInfo.InvariantInfo)))
        End Function

        ''' <summary>
        ''' Returns the length formatted ready for CSS interpreting.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If HasError Then
                Return String.Empty
            ElseIf IsPercentage Then
                Return String.Format(NumberFormatInfo.InvariantInfo, "{0}%", Number)
            Else
                Dim u As String = String.Empty

                Select Case Unit
                    Case CssUnit.None

                    Case CssUnit.Ems
                        u = "em"

                    Case CssUnit.Pixels
                        u = "px"

                    Case CssUnit.Ex
                        u = "ex"

                    Case CssUnit.Inches
                        u = "in"

                    Case CssUnit.Centimeters
                        u = "cm"

                    Case CssUnit.Milimeters
                        u = "mm"

                    Case CssUnit.Points
                        u = "pt"

                    Case CssUnit.Picas
                        u = "pc"

                End Select

                Return String.Format(NumberFormatInfo.InvariantInfo, "{0}{1}", Number, u)
            End If
        End Function

#End Region
    End Class
End Namespace
