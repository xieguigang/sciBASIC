#Region "Microsoft.VisualBasic::b0f6fab94953628e1a152947ab656bbc, mime\application%netcdf\HDF5\structure\SymbolTableEntry.vb"

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

    '     Class SymbolTableEntry
    ' 
    '         Properties: address, cacheType, linkNameOffset, objectHeaderAddress, objectHeaderScratchpadFormat
    '                     scratchpadSpace, size, symbolicLinkScratchpadFormat, totalSymbolTableEntrySize
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


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class SymbolTableEntry
        Private m_address As Long
        Private m_linkNameOffset As Long
        Private m_objectHeaderAddress As Long
        Private m_cacheType As Integer
        Private m_reserved As Integer
        Private m_scratchpadSpace As Byte()
        '16
        Private m_size As Integer

        ' case m_cacheType = 1
        Private m_objectHeaderScratchpadFormat As ObjectHeaderScratchpadFormat
        ' case m_cacheType = 2
        Private m_symbolicLinkScratchpadFormat As SymbolicLinkScratchpadFormat

        Private m_totalSymbolTableEntrySize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address

            Me.m_linkNameOffset = ReadHelper.readO([in], sb)
            Me.m_objectHeaderAddress = ReadHelper.readO([in], sb)

            Me.m_totalSymbolTableEntrySize = sb.sizeOfOffsets * 2

            Me.m_cacheType = [in].readInt()
            Me.m_reserved = [in].readInt()

            Me.m_totalSymbolTableEntrySize += 8

            If Me.m_cacheType = 0 Then
                Me.m_scratchpadSpace = [in].readBytes(16)
            ElseIf Me.m_cacheType = 1 Then
                Me.m_objectHeaderScratchpadFormat = New ObjectHeaderScratchpadFormat([in], sb, [in].offset)

                ' skip 
                Dim size As Integer = Me.m_objectHeaderScratchpadFormat.totalObjectHeaderScratchpadFormatSize
                Dim remained As Integer = 16 - size
                [in].skipBytes(remained)
            ElseIf Me.m_cacheType = 2 Then
                Me.m_symbolicLinkScratchpadFormat = New SymbolicLinkScratchpadFormat([in], sb, [in].offset)

                ' skip
                Dim size As Integer = Me.m_symbolicLinkScratchpadFormat.totalSymbolicLinkScratchpadFormatSize
                Dim remained As Integer = 16 - size
                [in].skipBytes(remained)
            End If

            Me.m_totalSymbolTableEntrySize += 16

            If sb.sizeOfOffsets = 8 Then
                Me.m_size = 40
            Else
                Me.m_size = 32
            End If
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property linkNameOffset() As Long
            Get
                Return Me.m_linkNameOffset
            End Get
        End Property

        Public Overridable ReadOnly Property objectHeaderAddress() As Long
            Get
                Return Me.m_objectHeaderAddress
            End Get
        End Property

        Public Overridable ReadOnly Property cacheType() As Integer
            Get
                Return Me.m_cacheType
            End Get
        End Property

        Public Overridable ReadOnly Property scratchpadSpace() As Byte()
            Get
                Return Me.m_scratchpadSpace
            End Get
        End Property

        Public Overridable ReadOnly Property objectHeaderScratchpadFormat() As ObjectHeaderScratchpadFormat
            Get
                ' only work for cache type = 1
                Return Me.m_objectHeaderScratchpadFormat
            End Get
        End Property

        Public Overridable ReadOnly Property symbolicLinkScratchpadFormat() As SymbolicLinkScratchpadFormat
            Get
                ' only work for cache type = 2
                Return Me.m_symbolicLinkScratchpadFormat
            End Get
        End Property

        Public Overridable ReadOnly Property totalSymbolTableEntrySize() As Integer
            Get
                Return Me.m_totalSymbolTableEntrySize
            End Get
        End Property

        Public Overridable ReadOnly Property size() As Long
            Get
                Return Me.m_size
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("SymbolTableEntry >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("link name offset : " & Me.m_linkNameOffset)
            Console.WriteLine("object header address : " & Me.m_objectHeaderAddress)
            Console.WriteLine("cache type : " & Me.m_cacheType)
            Console.WriteLine("reserved : " & Me.m_reserved)

            If Me.m_cacheType = 0 Then
                Console.WriteLine("scratchpad space : " & (Me.m_scratchpadSpace(0) And &HFF).ToString("x") & (Me.m_scratchpadSpace(1) And &HFF).ToString("x") & (Me.m_scratchpadSpace(2) And &HFF).ToString("x") & (Me.m_scratchpadSpace(3) And &HFF).ToString("x") & (Me.m_scratchpadSpace(4) And &HFF).ToString("x") & (Me.m_scratchpadSpace(5) And &HFF).ToString("x") & (Me.m_scratchpadSpace(6) And &HFF).ToString("x") & (Me.m_scratchpadSpace(7) And &HFF).ToString("x") & (Me.m_scratchpadSpace(8) And &HFF).ToString("x") & (Me.m_scratchpadSpace(9) And &HFF).ToString("x") & (Me.m_scratchpadSpace(10) And &HFF).ToString("x") & (Me.m_scratchpadSpace(11) And &HFF).ToString("x") & (Me.m_scratchpadSpace(12) And &HFF).ToString("x") & (Me.m_scratchpadSpace(13) And &HFF).ToString("x") & (Me.m_scratchpadSpace(14) And &HFF).ToString("x") & (Me.m_scratchpadSpace(15) And &HFF).ToString("x"))
            ElseIf Me.m_cacheType = 1 Then
                Me.m_objectHeaderScratchpadFormat.printValues()
            ElseIf Me.m_cacheType = 2 Then
                Me.m_symbolicLinkScratchpadFormat.printValues()
            End If

            Console.WriteLine("total symbol table entry size : " & Me.m_totalSymbolTableEntrySize)
            Console.WriteLine("SymbolTableEntry <<<")
        End Sub
    End Class

End Namespace

