#Region "Microsoft.VisualBasic::d7aee119c62904e9c7c6105ba85e09d3, Microsoft.VisualBasic.Core\src\Extensions\Math\Information\UncheckHelpers.vb"

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

    '   Total Lines: 43
    '    Code Lines: 37 (86.05%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (13.95%)
    '     File Size: 1.49 KB


    '     Module UncheckHelpers
    ' 
    '         Function: SingleToInt32Bits, ToTruncateInt32, ToTruncateInt64, UnsafeTruncateInteger
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Language

Namespace Math.Information

    Public Module UncheckHelpers

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToTruncateInt64(bi As BigInteger) As Long
            With bi.ToByteArray
                If .Length < HeapSizeOf.long Then
                    Return CType(bi, Long)
                Else
                    Return BitConverter.ToInt64(.ByRef, Scan0)
                End If
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToTruncateInt32(bi As BigInteger) As Integer
            With bi.ToByteArray
                If .Length < HeapSizeOf.int Then
                    Return CType(bi, Long)
                Else
                    Return BitConverter.ToInt32(.ByRef, Scan0)
                End If
            End With
        End Function

        Public Function SingleToInt32Bits(value As Single) As Integer
            Return BitConverter.ToInt32(BitConverter.GetBytes(value), Scan0)
        End Function

        <Extension>
        Public Function UnsafeTruncateInteger(val As ULong) As Integer
            Return BitConverter.ToInt32(BitConverter.GetBytes(val), Scan0)
        End Function
    End Module
End Namespace
