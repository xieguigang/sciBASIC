#Region "Microsoft.VisualBasic::20157b1c1fd6d6c84ee14d2e8c98a95d, mime\application%netcdf\HDF5\HDF5Reader.vb"

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

'     Class HDF5Reader
' 
'         Properties: chunks, dataBTree, datasetName, fileName, headerSize
'                     layout, reader
' 
'         Constructor: (+2 Overloads) Sub New
'         Sub: close, parseHeader
' 
' 
' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure

Namespace HDF5

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
    ''' </remarks>
    Public Class HDF5Reader

        Private m_reader As BinaryReader

        Private m_filename As String

        Private m_datasetName As String
        Private m_layout As Layout
        Private m_dataTree As DataBTree
        Private m_chunks As List(Of DataChunk)
        Private m_headerLength As Long

        Public Sub New(filename As String, datasetName As String)
            Me.m_filename = filename
            Me.m_reader = New BinaryFileReader(Me.m_filename)
            Me.m_datasetName = datasetName
            Me.m_layout = Nothing
            Me.m_dataTree = Nothing
            Me.m_chunks = New List(Of DataChunk)()
            Me.m_headerLength = 0
        End Sub

        Public Sub New([in] As BinaryReader, datasetName As String)
            Me.m_filename = Nothing
            Me.m_reader = [in]
            Me.m_datasetName = datasetName
            Me.m_layout = Nothing
            Me.m_dataTree = Nothing
            Me.m_chunks = New List(Of DataChunk)()
            Me.m_headerLength = 0
        End Sub

        Public Overridable Sub parseHeader()
            Dim sb As New Superblock(Me.m_reader, 0)
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.m_reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.m_reader, sb, objectFacade)

            Dim objects As List(Of DataObjectFacade) = rootGroup.objects
            For Each dobj As DataObjectFacade In objects
                ' compare dataset name
                If dobj.symbolName.Equals(Me.m_datasetName, StringComparison.CurrentCultureIgnoreCase) Then
                    Dim layout As Layout = dobj.layout
                    Me.m_layout = layout

                    Dim dataTree As New DataBTree(layout)
                    Me.m_dataTree = dataTree

                    Dim iter As DataChunkIterator = dataTree.getChunkIterator(Me.m_reader, sb)

                    While iter.hasNext(Me.m_reader, sb)
                        Dim chunk As DataChunk = iter.[next](Me.m_reader, sb)
                        Me.m_chunks.Add(chunk)
                    End While

                    Exit For
                End If
            Next

            Me.m_headerLength = Me.m_reader.maxOffset
        End Sub

        Public Overridable ReadOnly Property headerSize() As Long
            Get
                Return Me.m_headerLength
            End Get
        End Property

        Public Overridable ReadOnly Property fileName() As String
            Get
                Return Me.m_filename
            End Get
        End Property

        Public Overridable ReadOnly Property reader() As BinaryReader
            Get
                Return Me.m_reader
            End Get
        End Property

        Public Overridable ReadOnly Property datasetName() As String
            Get
                Return Me.m_datasetName
            End Get
        End Property

        Public Overridable ReadOnly Property layout() As Layout
            Get
                Return Me.m_layout
            End Get
        End Property

        Public Overridable ReadOnly Property dataBTree() As DataBTree
            Get
                Return Me.m_dataTree
            End Get
        End Property

        Public Overridable ReadOnly Property chunks() As List(Of DataChunk)
            Get
                Return Me.m_chunks
            End Get
        End Property

        Public Overridable Sub close()
            Me.m_reader.close()
        End Sub
    End Class

End Namespace
