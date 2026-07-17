#Region "Microsoft.VisualBasic::fe148b9d11f9452541b0e46403ff1df7, Data\BinaryData\HDF5\structure\Group.vb"

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

    '   Total Lines: 91
    '    Code Lines: 52 (57.14%)
    ' Comment Lines: 20 (21.98%)
    '    - Xml Docs: 65.00%
    ' 
    '   Blank Lines: 19 (20.88%)
    '     File Size: 3.06 KB


    '     Class Group
    ' 
    '         Properties: attributes, objects
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


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace struct

    ''' <summary>
    ''' A group of <see cref="DataObjectFacade"/>. a group is an object header that contains a message 
    ''' that points to a local heap (for storing the links to objects in the group) and to a B-tree 
    ''' (which indexes the links).
    ''' 
    ''' （类似于文件夹）
    ''' </summary>
    Public Class Group : Implements IFileDump

        ''' <summary>
        ''' 这个folder的parent节点
        ''' </summary>
        Dim m_facade As DataObjectFacade

        ''' <summary>
        ''' Nested objects
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property objects() As New List(Of DataObjectFacade)

        Public ReadOnly Property attributes As AttributeMessage()
            Get
                Return m_facade.attributes
            End Get
        End Property

        Public Sub New(sb As Superblock, facade As DataObjectFacade)
            m_facade = facade

            Dim gm As GroupMessage = facade.dataObject.groupMessage

            If gm IsNot Nothing Then
                ' v1 "old style" group: symbol table + local heap + B-tree.
                readGroup(sb, gm.bTreeAddress, gm.nameHeapAddress)
            Else
                ' v2 "new style" group: links are stored as Link messages (compact) or in a
                ' fractal heap indexed by a v2 B-tree (dense).
                Call readGroupV2(sb, facade)
            End If
        End Sub

        ''' <summary>
        ''' Builds the child list for a version 2 (or later) object header. Compact groups keep
        ''' their links as Link messages; dense groups keep them in a fractal heap and are not
        ''' yet implemented here.
        ''' </summary>
        Private Sub readGroupV2(sb As Superblock, facade As DataObjectFacade)
            Dim msgs = facade.dataObject.messages

            If msgs Is Nothing Then
                Return
            End If

            Dim dense As Boolean = False

            For Each msg As ObjectHeaderMessage In msgs
                Dim t As ObjectHeaderMessages = msg.headerMessageType.type

                If t = ObjectHeaderMessages.GroupInfo OrElse t = ObjectHeaderMessages.LinkInfo Then
                    dense = True
                End If
            Next

            If dense Then
                Throw New NotImplementedException("dense (fractal heap) groups are not yet implemented")
            End If

            ' Compact group: each child is a Link message with a hard link to its object header.
            For Each msg As ObjectHeaderMessage In msgs
                If msg.headerMessageType Is ObjectHeaderMessageType.Link AndAlso msg.linkMessage IsNot Nothing Then
                    Dim link = msg.linkMessage

                    If link.linkType = 0 Then
                        ' hard link -> object header address
                        objects.Add(New DataObjectFacade(sb, link.linkName, link.linkAddress))
                    End If
                End If
            Next
        End Sub

        Private Sub readGroup(sb As Superblock, bTreeAddress As Long, nameHeapAddress As Long)
            Dim nameHeap As New LocalHeap(sb, nameHeapAddress)
            Dim btree As New GroupBTree(sb, bTreeAddress)
            Dim dobj As DataObjectFacade
            Dim linkName As String

            For Each symbol As SymbolTableEntry In btree.symbolTableEntries
                Dim name As String = nameHeap.getString(CInt(symbol.linkNameOffset))

                If symbol.cacheType = 2 Then
                    linkName = nameHeap.getString(CInt(symbol.linkNameOffset))
                    dobj = New DataObjectFacade(sb, name, linkName)
                Else
                    dobj = New DataObjectFacade(sb, name, symbol.objectHeaderAddress)
                End If

                Call objects.Add(dobj)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return m_facade.symbolName & "\" & objects _
                .Select(Function(o) o.symbolName) _
                .GetJson
        End Function

        Private Sub printValues(console As TextWriter) Implements IFileDump.printValues
            console.WriteLine("Group >>>")

            If objects IsNot Nothing Then
                For Each dobj As DataObjectFacade In objects
                    dobj.printValues(console)
                Next
            End If

            console.WriteLine("Group <<<")
        End Sub
    End Class

End Namespace
