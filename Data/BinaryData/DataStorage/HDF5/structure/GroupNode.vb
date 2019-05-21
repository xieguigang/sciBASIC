#Region "Microsoft.VisualBasic::4a9e4f3df0825b64707fbc62728e00f6, Data\BinaryData\DataStorage\HDF5\structure\GroupNode.vb"

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
    '         Properties: address, entryNumber, signature, symbols, validSignature
    '                     version
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
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    Public Class GroupNode

        Public Shared ReadOnly GROUPNODE_SIGNATURE As Byte() = New CharStream() From {"S"c, "N"c, "O"c, "D"c}

        Private m_address As Long
        Private m_signature As Byte()
        Private m_version As Integer
        Private m_entryNumber As Integer

        Private m_symbols As List(Of SymbolTableEntry)

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address
            Me.m_signature = [in].readBytes(4)

            If Not Me.validSignature Then
                Throw New IOException("signature is not valid")
            End If

            Me.m_version = [in].readByte()
            [in].skipBytes(1)

            Me.m_entryNumber = [in].readShort()

            Me.m_symbols = New List(Of SymbolTableEntry)()

            Dim entryPos As Long = [in].offset
            For i As Integer = 0 To Me.m_entryNumber - 1
                Dim entry As New SymbolTableEntry([in], sb, entryPos)
                entryPos += entry.size
                If entry.objectHeaderAddress <> 0 Then
                    m_symbols.Add(entry)
                End If
            Next
        End Sub

        Private ReadOnly Property validSignature() As Boolean
            Get
                For i As Integer = 0 To 3
                    If Me.m_signature(i) <> GROUPNODE_SIGNATURE(i) Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property signature() As Byte()
            Get
                Return Me.m_signature
            End Get
        End Property

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property entryNumber() As Integer
            Get
                Return Me.m_entryNumber
            End Get
        End Property

        Public Overridable ReadOnly Property symbols() As List(Of SymbolTableEntry)
            Get
                Return Me.m_symbols
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("GroupNode >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("signature : " & (Me.m_signature(0) And &HFF).ToString("x") & (Me.m_signature(1) And &HFF).ToString("x") & (Me.m_signature(2) And &HFF).ToString("x") & (Me.m_signature(3) And &HFF).ToString("x"))

            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("entry number : " & Me.m_entryNumber)

            For i As Integer = 0 To Me.m_symbols.Count - 1
                Me.m_symbols(i).printValues()
            Next

            Console.WriteLine("GroupNode <<<")
        End Sub
    End Class

End Namespace
