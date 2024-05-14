#Region "Microsoft.VisualBasic::65be7b3ff75ffd49eb84baef0025d1d2, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLS\BIFF\VB6.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 850 B


    '     Module VB6
    ' 
    '         Sub: (+3 Overloads) Put
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace XLS.BIFF

    Module VB6

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(Of T As Structure)(file As BinaryWriter, struct As T)
            Call file.Write(struct)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(file As BinaryWriter, ints As Short())
            For Each i As Short In ints
                Call file.Write(i)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(file As BinaryWriter, b As Byte)
            Call file.Write(b)
        End Sub
    End Module
End Namespace
