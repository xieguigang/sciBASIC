#Region "Microsoft.VisualBasic::974a9bdef0a53a05edf2705702144fe0, Data\BinaryData\DataStorage\HDF5\structure\GroupNode.vb"

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
    '         Properties: entryNumber, signature, symbols, validSignature, version
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.[Structure]

    Public Class GroupNode : Inherits HDF5Ptr

        Public Shared ReadOnly GROUPNODE_SIGNATURE As Byte() = New CharStream() From {"S"c, "N"c, "O"c, "D"c}

        Private ReadOnly Property validSignature() As Boolean
            Get
                For i As Integer = 0 To 3
                    If Me.signature(i) <> GROUPNODE_SIGNATURE(i) Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        Public Overridable ReadOnly Property signature() As Byte()
        Public Overridable ReadOnly Property version() As Integer
        Public Overridable ReadOnly Property entryNumber() As Integer
        Public Overridable ReadOnly Property symbols() As List(Of SymbolTableEntry)

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.signature = [in].readBytes(4)

            If Not Me.validSignature Then
                Throw New IOException("signature is not valid")
            End If

            Me.version = [in].readByte()
            [in].skipBytes(1)

            Me.entryNumber = [in].readShort()

            Me.symbols = New List(Of SymbolTableEntry)()

            Dim entryPos As Long = [in].offset
            For i As Integer = 0 To Me.entryNumber - 1
                Dim entry As New SymbolTableEntry([in], sb, entryPos)
                entryPos += entry.size
                If entry.objectHeaderAddress <> 0 Then
                    symbols.Add(entry)
                End If
            Next
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupNode >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("signature : " &
                              (Me.signature(0) And &HFF).ToString("x") &
                              (Me.signature(1) And &HFF).ToString("x") &
                              (Me.signature(2) And &HFF).ToString("x") &
                              (Me.signature(3) And &HFF).ToString("x"))

            console.WriteLine("version : " & Me.version)
            console.WriteLine("entry number : " & Me.entryNumber)

            For i As Integer = 0 To Me.symbols.Count - 1
                symbols(i).printValues(console)
            Next

            console.WriteLine("GroupNode <<<")
        End Sub
    End Class

End Namespace
