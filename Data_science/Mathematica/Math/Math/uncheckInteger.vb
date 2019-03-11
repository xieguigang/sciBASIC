#Region "Microsoft.VisualBasic::ce21ae77673d03b29e7e8960798e49da, Data_science\Mathematica\Math\Math\uncheckInteger.vb"

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

    ' Module UncheckIntegerExtensions
    ' 
    '     Function: unchecked, uncheckedInteger, uncheckedLong, uncheckedULong
    ' 
    ' Structure UncheckedInteger
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: ToString
    '     Operators: (+2 Overloads) -, (+2 Overloads) *, (+2 Overloads) /, (+2 Overloads) ^, (+2 Overloads) +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' unchecked arithmetic
''' </summary>
Public Module UncheckIntegerExtensions

    Public Function unchecked(u&) As BigInteger
        Return New BigInteger(u)
    End Function

    <Extension>
    Public Function uncheckedULong(bytes As BigInteger) As ULong
        Dim data As Byte() = bytes.ToByteArray
        If data.Length < 8 Then
            data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
        End If
        Return BitConverter.ToUInt64(data, Scan0)
    End Function

    <Extension>
    Public Function uncheckedLong(bytes As BigInteger) As Long
        Dim data As Byte() = bytes.ToByteArray
        If data.Length < 8 Then
            data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
        End If
        Return BitConverter.ToInt64(data, Scan0)
    End Function

    <Extension>
    Public Function uncheckedInteger(bytes As BigInteger) As Integer
        Return BitConverter.ToInt32(bytes.ToByteArray, Scan0)
    End Function
End Module

Public Structure UncheckedInteger

    Dim Value As BigInteger

    Sub New(i%)
        Value = New BigInteger(i)
    End Sub

    Sub New(l&)
        Value = New BigInteger(l)
    End Sub

    Sub New(s As Short)
        Value = New BigInteger(s)
    End Sub

    Public Overrides Function ToString() As String
        Return Value.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator +(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator +(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value + i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator -(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value - i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator -(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i - unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator *(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value * i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator *(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i * unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator /(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger((unchecked.Value / i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator /(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger((i / unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator ^(unchecked As UncheckedInteger, i%) As UncheckedInteger
        Return New UncheckedInteger(BigInteger.Pow(unchecked.Value, i).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator ^(i%, unchecked As UncheckedInteger) As UncheckedInteger
        Return New UncheckedInteger(BigInteger.Pow(i, unchecked.Value).uncheckedInteger)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Integer
        Return unchecked.Value.uncheckedInteger
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(unchecked As UncheckedInteger) As Long
        Return unchecked.Value.uncheckedLong
    End Operator

End Structure
