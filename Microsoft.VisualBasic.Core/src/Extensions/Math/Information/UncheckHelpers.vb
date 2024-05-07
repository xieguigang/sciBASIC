#Region "Microsoft.VisualBasic::dae89f686d7e481105d44408dc3ba7fc, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//Extensions/Math/Information/UncheckHelpers.vb"

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

    '   Total Lines: 37
    '    Code Lines: 32
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.23 KB


    '     Module UncheckHelpers
    ' 
    '         Function: ToTruncateInt32, ToTruncateInt64
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
    End Module
End Namespace
