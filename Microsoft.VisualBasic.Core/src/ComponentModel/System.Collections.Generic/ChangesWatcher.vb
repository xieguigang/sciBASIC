#Region "Microsoft.VisualBasic::45b0679e782c9fe3e8e5d9767b561dba, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\ChangesWatcher.vb"

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

    '   Total Lines: 62
    '    Code Lines: 49
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.82 KB


    '     Class ChangesWatcher
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Items, ToString
    ' 
    '         Sub: DoRefresh, Refresh
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection

    Public Class ChangesWatcher

        Dim exists As Index(Of String)
        Dim getItems As Func(Of IEnumerable(Of String))
        Dim lock As Boolean = False

        Public Event AddNew(newItem As String)
        Public Event Remove(item As String)

        Sub New(getItems As Func(Of IEnumerable(Of String)))
            Me.getItems = getItems
            Me.exists = getItems().Distinct.ToArray
        End Sub

        Private Sub DoRefresh()
            Dim pendings As String() = getItems().Distinct.ToArray

            SyncLock exists
                For Each newItem As String In pendings
                    If Not newItem Like exists Then
                        RaiseEvent AddNew(newItem)
                    End If
                Next
            End SyncLock

            Dim pendingIndex As Index(Of String) = pendings

            SyncLock exists
                For Each item As String In exists.Objects
                    If Not item Like pendingIndex Then
                        RaiseEvent Remove(item)
                    End If
                Next
            End SyncLock

            exists = pendingIndex
        End Sub

        Public Sub Refresh()
            If Not lock Then
                lock = True
                DoRefresh()
                lock = False
            End If
        End Sub

        Public Function Items() As IEnumerable(Of String)
            SyncLock exists
                Return exists.Objects
            End SyncLock
        End Function

        Public Overrides Function ToString() As String
            SyncLock exists
                Return $"Exists {exists.Count} items."
            End SyncLock
        End Function

    End Class
End Namespace
