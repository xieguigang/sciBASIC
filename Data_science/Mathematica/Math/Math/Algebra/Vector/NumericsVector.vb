#Region "Microsoft.VisualBasic::0efa1f5bd6f293589c4f4edb9840c885, Data_science\Mathematica\Math\Math\Algebra\Vector\NumericsVector.vb"

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

    '   Total Lines: 97
    '    Code Lines: 43 (44.33%)
    ' Comment Lines: 43 (44.33%)
    '    - Xml Docs: 97.67%
    ' 
    '   Blank Lines: 11 (11.34%)
    '     File Size: 4.17 KB


    '     Module NumericsVector
    ' 
    '         Function: AsBytes, AsInteger, AsLong, AsSByte, AsSingle
    '                   AsUInteger, AsULong, AsUShort, AsVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    ''' <summary>
    ''' Numerics vector converts for numeric types like:
    ''' <see cref="Integer"/>, <see cref="Long"/>, <see cref="ULong"/>, <see cref="Byte"/>, <see cref="Single"/>
    ''' </summary>
    Public Module NumericsVector

        ''' <summary>
        ''' Convert the numeric collection as a math vector
        ''' </summary>
        ''' <typeparam name="T">
        ''' should be a numeric type
        ''' </typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function AsVector(Of T As {Structure, IComparable, IComparable(Of T), IEquatable(Of T), IConvertible, IFormattable})(source As IEnumerable(Of T)) As Vector
            Return New Vector(source.Select(Function(x) CDbl(CObj(x))))
        End Function

        ''' <summary>
        ''' numeric to integer values
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsInteger(vector As Vector) As Integer()
            Return vector.Select(Function(x) CInt(x)).ToArray
        End Function

        ''' <summary>
        ''' A helper function for cast current <see cref="Vector"/> as a numeric array in <see cref="Long"/> type.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsLong(vector As Vector) As Long()
            Return vector.Select(Function(x) CLng(x)).ToArray
        End Function

        ''' <summary>
        ''' A helper function for cast current <see cref="Vector"/> as a numeric array in <see cref="Single"/> type.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsSingle(vector As Vector) As Single()
            Return vector.Select(Function(x) CSng(x)).ToArray
        End Function

        ''' <summary>
        ''' A helper function for cast current <see cref="Vector"/> as a numeric array in <see cref="UInteger"/> type.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsUInteger(vector As Vector) As UInteger()
            Return vector.Select(Function(x) CUInt(x)).ToArray
        End Function

        ''' <summary>
        ''' A helper function for cast current <see cref="Vector"/> as a numeric array in <see cref="ULong"/> type.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsULong(vector As Vector) As ULong()
            Return vector.Select(Function(x) CULng(x)).ToArray
        End Function

        ''' <summary>
        ''' A helper function for cast current <see cref="Vector"/> as a numeric array in <see cref="UShort"/> type.
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsUShort(vector As Vector) As UShort()
            Return vector.Select(Function(x) CUShort(x)).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsBytes(vector As Vector) As Byte()
            Return vector.Select(Function(x) CByte(x)).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function AsSByte(vector As Vector) As SByte()
            Return vector.Select(Function(x) CSByte(x)).ToArray
        End Function
    End Module
End Namespace
