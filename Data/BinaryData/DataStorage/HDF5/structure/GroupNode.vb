#Region "Microsoft.VisualBasic::2ed7a83939f80aeff4bd32660502ba69, Data\BinaryData\DataStorage\HDF5\structure\GroupNode.vb"

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

    '     Class GroupNode
    ' 
    '         Properties: entryNumber, magic, symbols, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues
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
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct

    ''' <summary>
    ''' A group is an object internal to the file that allows arbitrary nesting of objects within the 
    ''' file (including other groups). A group maps a set of link names in the group to a set of 
    ''' relative file addresses of objects in the file. Certain metadata for an object to which the 
    ''' group points can be cached in the group’s symbol table entry in addition to being in the 
    ''' object’s header.
    ''' 
    ''' An HDF5 Object name space can be stored hierarchically by partitioning the name into components 
    ''' And storing Each component As a link In a group. The link For a non-ultimate component points 
    ''' To the group containing the Next component. The link For the last component points To the 
    ''' Object being named.
    '''
    ''' One implementation Of a group Is a collection Of symbol table nodes indexed by a B-tree. 
    ''' Each symbol table node contains entries For one Or more links. If an attempt Is made To add 
    ''' a link To an already full symbol table node containing 2K entries, Then the node Is split 
    ''' And one node contains K symbols And the other contains K+1 symbols.
    ''' </summary>
    Public Class GroupNode : Inherits HDF5Ptr
        Implements IMagicBlock

        Const signature = "SNOD"

        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Public ReadOnly Property version() As Integer
        Public ReadOnly Property entryNumber() As Integer
        Public ReadOnly Property symbols() As List(Of SymbolTableEntry)

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Throw New IOException("signature is not valid")
            Else
                Me.version = [in].readByte()
            End If

            [in].skipBytes(1)

            Me.entryNumber = [in].readShort()

            Me.symbols = New List(Of SymbolTableEntry)()

            Dim entryPos As Long = [in].offset

            For i As Integer = 0 To Me.entryNumber - 1
                Dim entry As New SymbolTableEntry(sb, entryPos)

                entryPos += entry.size

                If entry.objectHeaderAddress <> 0 Then
                    symbols.Add(entry)
                End If
            Next
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Dim signature As Byte() = Encoding.ASCII.GetBytes(magic)

            console.WriteLine("GroupNode >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("signature : " &
                              (signature(0) And &HFF).ToString("x") &
                              (signature(1) And &HFF).ToString("x") &
                              (signature(2) And &HFF).ToString("x") &
                              (signature(3) And &HFF).ToString("x"))

            console.WriteLine("version : " & Me.version)
            console.WriteLine("entry number : " & Me.entryNumber)

            For i As Integer = 0 To Me.symbols.Count - 1
                symbols(i).printValues(console)
            Next

            console.WriteLine("GroupNode <<<")
        End Sub
    End Class

End Namespace
