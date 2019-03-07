#Region "Microsoft.VisualBasic::9155ba338f12a3aa6e413b7eab8bfe10, mime\application%netcdf\HDF5\structure\Group.vb"

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

    '     Class Group
    ' 
    '         Properties: objects
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues, readGroup
    ' 
    ' 
    ' /********************************************************************************/

#End Region


'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class Group

        Private Shared NESTED_OBJECTS As New List(Of DataObjectFacade)()

        Private m_facade As DataObjectFacade

        Public Sub New([in] As BinaryReader, sb As Superblock, facade As DataObjectFacade)
            Me.m_facade = facade

            If facade.dataObject.groupMessage IsNot Nothing Then
                Dim gm As GroupMessage = facade.dataObject.groupMessage
                readGroup([in], sb, gm.bTreeAddress, gm.nameHeapAddress)
            End If
        End Sub

        Private Sub readGroup([in] As BinaryReader, sb As Superblock, bTreeAddress As Long, nameHeapAddress As Long)
            Dim nameHeap As New LocalHeap([in], sb, nameHeapAddress)
            Dim btree As New GroupBTree([in], sb, bTreeAddress)

            For Each s As SymbolTableEntry In btree.symbolTableEntries
                Dim sname As String = nameHeap.getString(CInt(s.linkNameOffset))
                If s.cacheType = 2 Then
                    Dim linkName As String = nameHeap.getString(CInt(s.linkNameOffset))
                    Dim dobj As New DataObjectFacade([in], sb, sname, linkName)
                    NESTED_OBJECTS.Add(dobj)
                Else
                    Dim dobj As New DataObjectFacade([in], sb, sname, s.objectHeaderAddress)
                    NESTED_OBJECTS.Add(dobj)
                End If
            Next
        End Sub

        Public Overridable ReadOnly Property objects() As List(Of DataObjectFacade)
            Get
                Return NESTED_OBJECTS
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("Group >>>")

            If NESTED_OBJECTS IsNot Nothing Then
                For Each dobj As DataObjectFacade In NESTED_OBJECTS
                    dobj.printValues()
                Next
            End If

            Console.WriteLine("Group <<<")
        End Sub
    End Class

End Namespace

