#Region "Microsoft.VisualBasic::ade59e8aaf858a06d9158fc9af56f6d3, Microsoft.VisualBasic.Core\src\Extensions\Math\Percentage.vb"

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

    '   Total Lines: 204
    '    Code Lines: 140
    ' Comment Lines: 29
    '   Blank Lines: 35
    '     File Size: 8.00 KB


    '     Structure Percentage
    ' 
    '         Properties: Denominator, FractionExpr, Numerator, One, Value
    '                     ZERO
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) CompareTo, Equals, GetTypeCode, ToBoolean, ToByte
    '                   ToChar, ToDateTime, ToDecimal, ToDouble, ToInt16
    '                   ToInt32, ToInt64, ToSByte, ToSingle, (+3 Overloads) ToString
    '                   ToType, ToUInt16, ToUInt32, ToUInt64, TryParse
    '         Operators: <, >
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports r = System.Text.RegularExpressions.Regex

Namespace Math

    ''' <summary>
    ''' 分数，百分比
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure Percentage : Implements IComparable, IFormattable, IConvertible, IComparable(Of [Double]), IEquatable(Of [Double])

        ''' <summary>
        ''' 分子
        ''' </summary>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Numerator As Double
        ''' <summary>
        ''' 分母
        ''' </summary>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Denominator As Double

        ''' <summary>
        ''' <see cref="Numerator"></see>/<see cref="Denominator"></see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <SoapIgnore> Public ReadOnly Property Value As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Numerator = 0R Then
                    Return 0
                Else
                    Return Numerator / Denominator
                End If
            End Get
        End Property

        <SoapIgnore> Public ReadOnly Property FractionExpr As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return $"{Numerator}/{Denominator}"
            End Get
        End Property

        ''' <summary>
        ''' <paramref name="n"/> / <paramref name="d"/>
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="d"></param>
        Sub New(n As Double, d As Double)
            Numerator = n
            Denominator = d
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}/{1} ({2}%)", Numerator, Denominator, Value)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Text">``\d+[/]\d+ \(\d+[%]\)``</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function TryParse(Text As String) As Percentage
            If String.IsNullOrEmpty(Text) Then
                Return ZERO
            End If

            Dim matchs$() = r.Matches(Text, "\d+").ToArray
            Dim n As Double = matchs(0).RegexParseDouble
            Dim d As Double = matchs(1).RegexParseDouble

            Return New Percentage With {
                .Numerator = n,
                .Denominator = d
            }
        End Function

        Public Shared ReadOnly Property ZERO As New Percentage(0, 1)
        Public Shared ReadOnly Property One As New Percentage(1, 1)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(value As Percentage) As Double
            Return value.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(value As Percentage, n As Double) As Boolean
            Return value.Value > n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(value As Percentage, n As Double) As Boolean
            Return value.Value < n
        End Operator

#Region "Public Interface"
        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            End If
            If obj.GetType Is GetType(Double) Then
                Return Value.CompareTo(DirectCast(obj, Double))
            ElseIf obj.GetType Is GetType(Percentage) Then
                Return Value.CompareTo(DirectCast(obj, Percentage).Value)
            Else
                Return 1
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function ToString(format As String, formatProvider As IFormatProvider) As String Implements IFormattable.ToString
            Return Value.ToString(format, formatProvider)
        End Function

#Region "Implements IConvertible"
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return TypeCode.Double
        End Function

        Private Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            Throw New NotImplementedException()
        End Function

        Private Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Throw New NotImplementedException()
        End Function

        Private Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Throw New NotImplementedException()
        End Function

        Private Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Throw New NotImplementedException()
        End Function

        Private Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Throw New NotImplementedException()
        End Function

        Private Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Throw New NotImplementedException()
        End Function

        Private Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Throw New NotImplementedException()
        End Function

        Private Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Throw New NotImplementedException()
        End Function

        Private Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Throw New NotImplementedException()
        End Function

        Private Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Throw New NotImplementedException()
        End Function

        Private Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Throw New NotImplementedException()
        End Function

        Private Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Throw New NotImplementedException()
        End Function

        Private Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Throw New NotImplementedException()
        End Function

        Private Function ToDateTime(provider As IFormatProvider) As Date Implements IConvertible.ToDateTime
            Throw New NotImplementedException()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return Value.ToString(provider)
        End Function

        Private Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Throw New NotImplementedException()
        End Function
#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CompareTo(other As Double) As Integer Implements IComparable(Of Double).CompareTo
            Return Value.CompareTo(other)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals(other As Double) As Boolean Implements IEquatable(Of Double).Equals
            Return Value.Equals(other)
        End Function
#End Region
    End Structure
End Namespace
