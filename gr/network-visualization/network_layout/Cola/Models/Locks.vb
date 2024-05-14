#Region "Microsoft.VisualBasic::896464d20029246de733dd95d4944b3f, gr\network-visualization\network_layout\Cola\Models\Locks.vb"

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

    '   Total Lines: 46
    '    Code Lines: 21
    ' Comment Lines: 19
    '   Blank Lines: 6
    '     File Size: 1.33 KB


    '     Class Locks
    ' 
    '         Properties: isEmpty
    ' 
    '         Sub: add, apply, clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Cola

    ''' <summary>
    ''' Descent respects a collection of locks over nodes that should not move
    ''' </summary>
    Public Class Locks

        Public locks As Dictionary(Of Integer, Double())

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>false if no locks exist</returns>
        Public ReadOnly Property isEmpty() As Boolean
            Get
                Return locks.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' add a lock on the node at index id
        ''' </summary>
        ''' <param name="id">index of node to be locked</param>
        ''' <param name="x">required position for node</param>
        Public Sub add(id%, x As Double())
            Me.locks(id) = x
        End Sub

        ''' <summary>
        ''' clear all locks
        ''' </summary>
        Public Sub clear()
            Me.locks = New Dictionary(Of Integer, Double())()
        End Sub

        ''' <summary>
        ''' perform an operation on each lock
        ''' </summary>
        ''' <param name="f"></param>
        Public Sub apply(f As Action(Of Integer, Double()))
            For Each l In Me.locks.Keys
                f(l, Me.locks(l))
            Next
        End Sub
    End Class
End Namespace
