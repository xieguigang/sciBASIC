#Region "Microsoft.VisualBasic::85adef4378964ac99939700574f2504d, Microsoft.VisualBasic.Core\Language\Value\Numeric\f64.vb"

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

    '     Class f64
    ' 
    '         Properties: Hex
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, Equals, GetTypeCode, ToBoolean, ToByte
    '                   ToChar, ToDateTime, ToDecimal, ToDouble, ToInt16
    '                   ToInt32, ToInt64, ToSByte, ToSingle, (+2 Overloads) ToString
    '                   ToType, ToUInt16, ToUInt32, ToUInt64
    '         Operators: (+2 Overloads) -, (+2 Overloads) *, (+2 Overloads) /, ^, (+3 Overloads) +
    '                    (+2 Overloads) <, <=, (+2 Overloads) >, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language

    ''' <summary>
    ''' <see cref="Double"/>
    ''' </summary>
    Public Class f64 : Inherits Value(Of Double)
        Implements IComparable
        Implements IConvertible
        Implements IEquatable(Of Double)

        Public ReadOnly Property Hex As String
            Get
                Dim bytes As Byte() = BitConverter _
                    .GetBytes(Value) _
                    .Reverse _
                    .ToArray
                Dim h$ = bytes _
                    .Select(Function(b) b.ToString("X2")) _
                    .JoinBy("")

                Return h
            End Get
        End Property

        Sub New(x#)
            Value = x#
        End Sub

        Sub New()
            Me.New(0R)
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return Value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(f64)) Then
                Return Value.CompareTo(DirectCast(obj, f64).Value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(f64).FullName} --> {type.FullName}")
            End If
        End Function

#Region "Numeric operators"

        ' 20190618 请注意，在这里的符号运算与VBInteger有着明显不同
        ' VBInteger侧重于index offset的应用，所以符号运算可能会对原来的值做出修改
        ' 而在这里因为侧重的是数学运算的应用，所以在这里的所有的符号运算都不会修改VBDouble对象原来的值

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n#, x As f64) As f64
            If n >= x.Value Then
                Return New f64(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator <(x As f64, y As Double) As f64
            Return x.Value < y
        End Operator

        Public Shared Operator >(x As f64, y As Double) As f64
            Return x.Value > y
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(n#, x As f64) As Double
            Return n * x.Value
        End Operator

        Public Overloads Shared Operator +(x As f64, y As Double) As Double
            Return x.Value + y
        End Operator

        Public Overloads Shared Operator +(x As f64, y As f64) As Double
            Return x.Value + y.Value
        End Operator

        Public Overloads Shared Operator /(x As f64, y As f64) As Double
            Return x.Value / y.Value
        End Operator

        Public Overloads Shared Operator +(x#, y As f64) As Double
            Return x + y.Value
        End Operator

        Public Overloads Shared Widening Operator CType(x As Double) As f64
            Return New f64(x)
        End Operator

        Public Overloads Shared Operator <=(x As f64, n As Double) As Boolean
            Return x.Value <= n
        End Operator

        Public Overloads Shared Operator >=(x As f64, n As Double) As Boolean
            Return x.Value >= n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator /(x As f64, n As Double) As Double
            Return x.Value / n
        End Operator

        Public Shared Operator >(n As Double, x As f64) As f64
            Return x
        End Operator

        Public Shared Operator ^(x As f64, power As Double) As Double
            Return x.Value ^ power
        End Operator

        Public Overloads Shared Narrowing Operator CType(f As f64) As Double
            Return f.Value
        End Operator

        Public Overloads Shared Operator -(a As f64, b As f64) As Double
            Return a.Value - b.Value
        End Operator

        ''' <summary>
        ''' 值相减
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(a As Double, b As f64) As Double
            Return a - b.Value
        End Operator

        Public Overloads Shared Operator *(a As f64, b As f64) As Double
            Return a.Value * b.Value
        End Operator
#End Region

#Region "Implements IConvertible"

        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return TypeCode.Double
        End Function

        Public Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            If Value = 0R Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Return ChrW(CInt(Value))
        End Function

        Public Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Return CSByte(Value)
        End Function

        Public Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Return CByte(Value)
        End Function

        Public Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Return CShort(Value)
        End Function

        Public Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Return CUShort(Value)
        End Function

        Public Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Return CInt(Value)
        End Function

        Public Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Return CUInt(Value)
        End Function

        Public Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Return CLng(Value)
        End Function

        Public Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Return CULng(Value)
        End Function

        Public Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Return CSng(Value)
        End Function

        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Return Value
        End Function

        Public Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Return CDec(Value)
        End Function

        Public Function ToDateTime(provider As IFormatProvider) As Date Implements IConvertible.ToDateTime
            Return Date.FromBinary(CLng(Value))
        End Function

        Public Overloads Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return Value.ToString(provider)
        End Function

        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Return CTypeDynamic(Value, conversionType)
        End Function
#End Region

#Region "Implements IEquatable(Of Double)"
        Public Overloads Function Equals(other As Double) As Boolean Implements IEquatable(Of Double).Equals
            Return Value = other
        End Function
#End Region

    End Class
End Namespace
