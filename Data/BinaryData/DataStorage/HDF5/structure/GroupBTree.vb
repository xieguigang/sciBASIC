#Region "Microsoft.VisualBasic::15f5c17cc71933c99de1a64d0ecca53e, mime\application%netcdf\HDF5\structure\GroupBTree.vb"

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

'     Class GroupBTree
' 
'         Properties: symbolTableEntries
' 
'         Constructor: (+1 Overloads) Sub New
'         Sub: printValues, readAllEntries
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    Public Class GroupBTree

        Public Shared ReadOnly SIGNATURE As Byte() = New CharStream() From {"T"c, "R"c, "E"c, "E"c}

        Private m_address As Long

        Private m_SymbolTableEntries As List(Of SymbolTableEntry)

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_SymbolTableEntries = New List(Of SymbolTableEntry)()

            Dim entryList As New List(Of BTreeEntry)()
            readAllEntries([in], sb, address, entryList)

            For Each e As BTreeEntry In entryList
                Dim node As New GroupNode([in], sb, e.targetAddress)
                m_SymbolTableEntries.AddRange(node.symbols)
            Next
        End Sub

        Private Sub readAllEntries([in] As BinaryReader, sb As Superblock, address As Long, entryList As List(Of BTreeEntry))
            [in].offset = address

            Dim signature__1 As Byte() = [in].readBytes(4)
            For i As Integer = 0 To 3
                If signature__1(i) <> SIGNATURE(i) Then
                    Throw New IOException("signature is not valid")
                End If
            Next

            Dim type As Integer = [in].readByte()
            Dim level As Integer = [in].readByte()
            Dim entryNum As Integer = [in].readShort()

            Dim leftAddress As Long = ReadHelper.readO([in], sb)
            Dim rightAddress As Long = ReadHelper.readO([in], sb)

            Dim myEntries As New List(Of BTreeEntry)()
            For i As Integer = 0 To entryNum - 1
                myEntries.Add(New BTreeEntry([in], sb, [in].offset))
            Next

            If level = 0 Then
                entryList.AddRange(myEntries)
            Else
                For Each entry As BTreeEntry In myEntries
                    readAllEntries([in], sb, entry.targetAddress, entryList)
                Next
            End If
        End Sub

        Public Overridable ReadOnly Property symbolTableEntries() As List(Of SymbolTableEntry)
            Get
                Return Me.m_SymbolTableEntries
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("GroupBTree >>>")
            Console.WriteLine("address : " & Me.m_address)

            For i As Integer = 0 To Me.m_SymbolTableEntries.Count - 1
                Me.m_SymbolTableEntries(i).printValues()
            Next

            Console.WriteLine("GroupBTree <<<")
        End Sub
    End Class

End Namespace
