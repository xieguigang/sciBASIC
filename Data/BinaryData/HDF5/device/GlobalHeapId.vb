#Region "Microsoft.VisualBasic::47c19f624f8a723d30a8ec3d35c5361e, Data\BinaryData\HDF5\device\GlobalHeapId.vb"

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

    '   Total Lines: 28
    '    Code Lines: 22 (78.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 721 B


    '     Class GlobalHeapId
    ' 
    '         Properties: heapAddress, index, isNull
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace device

    Public Class GlobalHeapId

        Public ReadOnly Property heapAddress As Long
        Public ReadOnly Property index As Integer

        Public ReadOnly Property isNull As Boolean
            Get
                Return heapAddress = 0 AndAlso index = 0
            End Get
        End Property

        Sub New(address&, index As Integer)
            Me.index = index
            Me.heapAddress = address
        End Sub

        Public Overrides Function ToString() As String
            If isNull Then
                Return "null"
            Else
                Return $"[{index}] &{heapAddress}"
            End If
        End Function
    End Class

End Namespace
