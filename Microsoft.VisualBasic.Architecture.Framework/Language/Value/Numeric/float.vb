#Region "Microsoft.VisualBasic::1f40ff0c8221f5dd1f693f374e0df200, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Value\Numeric\float.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Language

    ''' <summary>
    ''' <see cref="System.Double"/>
    ''' </summary>
    Public Class float : Inherits Value(Of Double)
        Implements IComparable
        Implements IConvertible
        Implements IEquatable(Of Double)

        Sub New(x#)
            value = x#
        End Sub

        Sub New()
            Me.New(0R)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim type As Type = obj.GetType

            If type.Equals(GetType(Double)) Then
                Return value.CompareTo(DirectCast(obj, Double))
            ElseIf type.Equals(GetType(float)) Then
                Return value.CompareTo(DirectCast(obj, float).value)
            Else
                Throw New Exception($"Miss-match of type:  {GetType(float).FullName} --> {type.FullName}")
            End If
        End Function

#Region "Numeric operators"

        ''' <summary>
        ''' n &lt; value &lt;= n2
        ''' 假若n 大于value，则返回最大值，上面的表达式肯定不成立
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator <(n#, x As float) As float
            If n >= x.value Then
                Return New float(Double.MaxValue)
            Else
                Return x
            End If
        End Operator

        Public Shared Operator *(n#, x As float) As Double
            Return n * x.value
        End Operator

        Public Overloads Shared Operator +(x As float, y As float) As Double
            Return x.value + y.value
        End Operator

        Public Overloads Shared Operator /(x As float, y As float) As Double
            Return x.value / y.value
        End Operator

        Public Overloads Shared Operator +(x#, y As float) As Double
            Return x + y.value
        End Operator

        Public Overloads Shared Widening Operator CType(x#) As float
            Return New float(x)
        End Operator

        Public Overloads Shared Operator <=(x As float, n As Double) As Boolean
            Return x.value <= n
        End Operator

        Public Overloads Shared Operator >=(x As float, n As Double) As Boolean
            Return x.value >= n
        End Operator

        Public Overloads Shared Operator /(x As float, n As Double) As Double
            Return x.value / n
        End Operator

        Public Shared Operator >(n As Double, x As float) As float
            Return x
        End Operator

        Public Shared Operator ^(x As float, power As Double) As Double
            Return x.value ^ power
        End Operator

        Public Overloads Shared Narrowing Operator CType(f As float) As Double
            Return f.value
        End Operator

        Public Overloads Shared Operator -(a As float, b As float) As Double
            Return a.value - b.value
        End Operator

        Public Overloads Shared Operator *(a As float, b As float) As Double
            Return a.value * b.value
        End Operator
#End Region

#Region "Implements IConvertible"

        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return TypeCode.Double
        End Function

        Public Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            If value = 0R Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Return ChrW(CInt(value))
        End Function

        Public Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Return CSByte(value)
        End Function

        Public Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Return CByte(value)
        End Function

        Public Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Return CShort(value)
        End Function

        Public Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Return CUShort(value)
        End Function

        Public Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Return CInt(value)
        End Function

        Public Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Return CUInt(value)
        End Function

        Public Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Return CLng(value)
        End Function

        Public Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Return CULng(value)
        End Function

        Public Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Return CSng(value)
        End Function

        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Return value
        End Function

        Public Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Return CDec(value)
        End Function

        Public Function ToDateTime(provider As IFormatProvider) As Date Implements IConvertible.ToDateTime
            Return Date.FromBinary(CLng(value))
        End Function

        Public Overloads Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return value.ToString(provider)
        End Function

        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Return CTypeDynamic(value, conversionType)
        End Function
#End Region

#Region "Implements IEquatable(Of Double)"
        Public Overloads Function Equals(other As Double) As Boolean Implements IEquatable(Of Double).Equals
            Return value = other
        End Function
#End Region

    End Class
End Namespace
