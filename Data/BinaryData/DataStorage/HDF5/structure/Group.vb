#Region "Microsoft.VisualBasic::f6a2ec6dd98a364b46e2bff07d9e94d7, Data\BinaryData\DataStorage\HDF5\structure\Group.vb"

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
' 
'         Function: ToString
' 
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


Imports Microsoft.VisualBasic.Serialization.JSON
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    ''' <summary>
    ''' A group of <see cref="DataObjectFacade"/>
    ''' </summary>
    Public Class Group : Implements IFileDump

        Shared ReadOnly NESTED_OBJECTS As New List(Of DataObjectFacade)()

        Dim m_facade As DataObjectFacade

        Public Overridable ReadOnly Property objects() As List(Of DataObjectFacade)
            Get
                Return NESTED_OBJECTS
            End Get
        End Property

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
            Dim dobj As DataObjectFacade
            Dim linkName As String

            For Each symbol As SymbolTableEntry In btree.symbolTableEntries
                Dim name As String = nameHeap.getString(CInt(symbol.linkNameOffset))

                If symbol.cacheType = 2 Then
                    linkName = nameHeap.getString(CInt(symbol.linkNameOffset))
                    dobj = New DataObjectFacade([in], sb, name, linkName)
                Else
                    dobj = New DataObjectFacade([in], sb, name, symbol.objectHeaderAddress)
                End If

                Call NESTED_OBJECTS.Add(dobj)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return m_facade.symbolName & "\" & objects _
                .Select(Function(o) o.symbolName) _
                .GetJson
        End Function

        Private Sub printValues(console As System.IO.StringWriter) Implements IFileDump.printValues
            console.WriteLine("Group >>>")

            If NESTED_OBJECTS IsNot Nothing Then
                For Each dobj As DataObjectFacade In NESTED_OBJECTS
                    dobj.printValues(console)
                Next
            End If

            console.WriteLine("Group <<<")
        End Sub
    End Class

End Namespace
