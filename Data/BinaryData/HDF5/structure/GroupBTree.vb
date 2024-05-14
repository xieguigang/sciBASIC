#Region "Microsoft.VisualBasic::33e00dbb22de5be0100aa8d6782e5564, Data\BinaryData\HDF5\structure\GroupBTree.vb"

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

    '   Total Lines: 89
    '    Code Lines: 57
    ' Comment Lines: 9
    '   Blank Lines: 23
    '     File Size: 2.95 KB


    '     Class GroupBTree
    ' 
    '         Properties: magic, symbolTableEntries
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
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.BTree
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class GroupBTree : Inherits HDF5Ptr
        Implements IMagicBlock

        Public Const signature$ = "TREE"

        Public ReadOnly Property symbolTableEntries As List(Of SymbolTableEntry)
        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.symbolTableEntries = New List(Of SymbolTableEntry)()

            Dim entryList As New List(Of BTreeEntry)()
            Dim node As GroupNode

            Call readAllEntries(sb, address, entryList)

            For Each e As BTreeEntry In entryList
                node = New GroupNode(sb, e.targetAddress)
                symbolTableEntries.AddRange(node.symbols)
            Next
        End Sub

        Private Sub readAllEntries(sb As Superblock, address As Long, entryList As List(Of BTreeEntry))
            Dim [in] As BinaryReader = sb.FileReader(address)

            _magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Throw New IOException("signature is not valid")
            End If

            Dim type As Integer = [in].readByte()
            Dim level As Integer = [in].readByte()
            Dim entryNum As Integer = [in].readShort()

            Dim leftAddress As Long = ReadHelper.readO([in], sb)
            Dim rightAddress As Long = ReadHelper.readO([in], sb)
            Dim myEntries As New List(Of BTreeEntry)()

            For i As Integer = 0 To entryNum - 1
                myEntries.Add(New BTreeEntry(sb, [in].offset))
            Next

            If level = 0 Then
                entryList.AddRange(myEntries)
            Else
                For Each entry As BTreeEntry In myEntries
                    readAllEntries(sb, entry.targetAddress, entryList)
                Next
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupBTree >>>")
            console.WriteLine("address : " & Me.m_address)

            For i As Integer = 0 To symbolTableEntries.Count - 1
                symbolTableEntries(i).printValues(console)
            Next

            console.WriteLine("GroupBTree <<<")
        End Sub
    End Class

End Namespace
