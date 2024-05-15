#Region "Microsoft.VisualBasic::980bb25b4a501eb3f1124930c1671267, Data_science\Mathematica\Math\Math\Numerics\UncheckedInteger.vb"

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

    '   Total Lines: 76
    '    Code Lines: 54
    ' Comment Lines: 7
    '   Blank Lines: 15
    '     File Size: 2.57 KB


    '     Structure UncheckedInteger
    ' 
    '         Properties: UncheckUInt32, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: *, +, (+2 Overloads) Xor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Numerics

    ''' <summary>
    ''' > https://stackoverflow.com/questions/2403154/fastest-way-to-do-an-unchecked-integer-addition-in-vb-net
    ''' </summary>
    <StructLayout(LayoutKind.Explicit)>
    Public Structure UncheckedInteger

        <FieldOffset(0)>
        Private longValue As Long
        <FieldOffset(0)>
        Private intValueLo As Integer
        <FieldOffset(4)>
        Private intValueHi As Integer

        ''' <summary>
        ''' The integer value
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As Integer
            Get
                Return intValueLo
            End Get
            Set(value As Integer)
                longValue = value
            End Set
        End Property

        Public ReadOnly Property UncheckUInt32 As UInt32
            Get
                Dim bytes As Byte() = BitConverter.GetBytes(intValueLo)
                Dim uint As UInt32 = BitConverter.ToUInt32(bytes, Scan0)

                Return uint
            End Get
        End Property

        Private Sub New(newLongValue As Long)
            longValue = newLongValue
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Public Overloads Shared Widening Operator CType(value As Integer) As UncheckedInteger
            Return New UncheckedInteger(CLng(value))
        End Operator

        Public Overloads Shared Widening Operator CType(value As Long) As UncheckedInteger
            Return New UncheckedInteger(value)
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedInteger) As Long
            Return value.longValue
        End Operator

        Public Overloads Shared Widening Operator CType(value As UncheckedInteger) As Integer
            Return value.intValueLo
        End Operator

        Public Overloads Shared Operator +(a As UncheckedInteger, b As Integer) As UncheckedInteger
            Return New UncheckedInteger(a.longValue + b)
        End Operator

        Public Overloads Shared Operator *(x As UncheckedInteger, y As Integer) As UncheckedInteger
            Return New UncheckedInteger(x.longValue * y)
        End Operator

        Public Overloads Shared Operator Xor(x As UncheckedInteger, y As Integer) As UncheckedInteger
            Return New UncheckedInteger(x.longValue Xor y)
        End Operator
    End Structure
End Namespace
