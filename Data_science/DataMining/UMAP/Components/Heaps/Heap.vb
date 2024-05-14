#Region "Microsoft.VisualBasic::542e03adcecfe64f1ba36741ed9ca295, Data_science\DataMining\UMAP\Components\Heaps\Heap.vb"

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

    '   Total Lines: 18
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 502 B


    ' Class Heap
    ' 
    '     Sub: Add
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public NotInheritable Class Heap

    ReadOnly _values As New List(Of Double()())

    Default Public ReadOnly Property Item(index As Integer) As Double()()
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return _values(index)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(value As Double()())
        Call _values.Add(value)
    End Sub
End Class
