#Region "Microsoft.VisualBasic::e28945a8a635a3a752b9377f7d3f24bd, ..\sciBASIC#\Data_science\Mathematical\Math\uncheckInteger.vb"

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

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' unchecked arithmetic
''' </summary>
Public Module UncheckInteger

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
    Public Function uncheckedInteger(bytes As BigInteger) As Integer
        Return BitConverter.ToInt32(bytes.ToByteArray, Scan0)
    End Function
End Module

