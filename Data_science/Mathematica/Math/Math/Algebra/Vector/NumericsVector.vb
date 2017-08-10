#Region "Microsoft.VisualBasic::9d03ef852c447a7db61c44fe4a33ed3a, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Vector\NumericsVector.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' Numerics vector converts for numeric types like:
    ''' <see cref="Integer"/>, <see cref="Long"/>, <see cref="ULong"/>, <see cref="Byte"/>, <see cref="Single"/>
    ''' </summary>
    Public Module NumericsVector

        <Extension> Public Function AsVector(Of T As {Structure, IComparable, IComparable(Of T), IEquatable(Of T), IConvertible, IFormattable})(source As IEnumerable(Of T)) As Vector
            Return New Vector(source.Select(Function(x) CDbl(CObj(x))))
        End Function

        <Extension> Public Function AsInteger(vector As Vector) As Integer()
            Return vector.Select(Function(x) CInt(x)).ToArray
        End Function

        <Extension> Public Function AsLong(vector As Vector) As Long()
            Return vector.Select(Function(x) CLng(x)).ToArray
        End Function

        <Extension> Public Function AsSingle(vector As Vector) As Single()
            Return vector.Select(Function(x) CSng(x)).ToArray
        End Function

        <Extension> Public Function AsUInteger(vector As Vector) As UInteger()
            Return vector.Select(Function(x) CUInt(x)).ToArray
        End Function

        <Extension> Public Function AsULong(vector As Vector) As ULong()
            Return vector.Select(Function(x) CULng(x)).ToArray
        End Function

        <Extension> Public Function AsUShort(vector As Vector) As UShort()
            Return vector.Select(Function(x) CUShort(x)).ToArray
        End Function

        <Extension> Public Function AsBytes(vector As Vector) As Byte()
            Return vector.Select(Function(x) CByte(x)).ToArray
        End Function

        <Extension> Public Function AsSByte(vector As Vector) As SByte()
            Return vector.Select(Function(x) CSByte(x)).ToArray
        End Function
    End Module
End Namespace
