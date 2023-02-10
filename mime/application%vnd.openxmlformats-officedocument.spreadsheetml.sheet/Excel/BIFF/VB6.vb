#Region "Microsoft.VisualBasic::31b00fa7b240fae4fb40b91573aa7474, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\BIFF\VB6.vb"

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

    '   Total Lines: 33
    '    Code Lines: 23
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 1.17 KB


    '     Module VB6
    ' 
    '         Function: MKI
    ' 
    '         Sub: (+2 Overloads) Put
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace BIFF

    Module VB6

        'the memory copy API is used in the MKI$ function which converts an integer
        'value to a 2-byte string value to write to the file. (used by the Horizontal
        'Page Break function).
        Private Declare Sub CopyMemory Lib "KERNEL32" Alias "RtlMoveMemory" (lpvDest As Object, lpvSource As Object, ByVal cbCopy As Long)

        Friend Function MKI$(x As Integer)
            'used for writing integer array values to the disk file
            Dim temp$ = Space$(2)
            CopyMemory(temp$, x%, 2)
            MKI$ = temp$
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(Of T As Structure)(file As BinaryWriter, struct As T)
            Call file.Write(struct)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(file As BinaryWriter, b As Byte)
            Call file.Write(b)
        End Sub
    End Module
End Namespace
