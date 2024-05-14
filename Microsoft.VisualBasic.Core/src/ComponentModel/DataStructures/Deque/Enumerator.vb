#Region "Microsoft.VisualBasic::ed9fc06dfa15f5d676d8e5033794d4c7, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Deque\Enumerator.vb"

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

    '   Total Lines: 68
    '    Code Lines: 43
    ' Comment Lines: 12
    '   Blank Lines: 13
    '     File Size: 2.09 KB


    '     Class Enumerator
    ' 
    '         Properties: anyCurrent, Current
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: MoveNext
    ' 
    '         Sub: Dispose, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection.Deque

    Friend Class Enumerator(Of S) : Implements IEnumerator(Of S)

        ''' <summary>
        ''' initialize with -1 to ensure that InvalidOperationException 
        ''' is thrown when Current is called befor the first call of 
        ''' MoveNext
        ''' </summary>
        Dim curIndex As Integer = -1

        ''' <summary>
        ''' version of Deque(Of T) this Enumerator is enumerating from the moment this enumerator has been created
        ''' </summary>
        ''' 
        Dim version As Long

        ''' <summary>
        ''' Deque(Of T) this enumerator is enumerating
        ''' </summary>
        Dim que As Deque(Of S)

        Public Sub New(que As Deque(Of S), version As Long)
            Me.version = version
            Me.que = que
        End Sub

        Public ReadOnly Property Current As S Implements IEnumerator(Of S).Current
            Get
                If curIndex < 0 OrElse curIndex >= que.Count OrElse version <> que.version Then
                    Throw New InvalidOperationException()
                Else
                    Return que(curIndex)
                End If
            End Get
        End Property

        Private ReadOnly Property anyCurrent As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            que = Nothing
            curIndex = Nothing
            version = Nothing
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If version <> que.version Then
                Throw New InvalidOperationException()
            End If

            curIndex += 1

            If curIndex >= que.Count Then
                Return False
            End If

            Return True
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            curIndex = 0
        End Sub
    End Class
End Namespace
