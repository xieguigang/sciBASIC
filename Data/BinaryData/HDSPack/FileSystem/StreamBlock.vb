﻿#Region "Microsoft.VisualBasic::048a75a51f130a93f1bc19ed5d03ebef, Data\BinaryData\HDSPack\FileSystem\StreamBlock.vb"

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
    '    Code Lines: 46 (51.69%)
    ' Comment Lines: 31 (34.83%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (13.48%)
    '     File Size: 2.96 KB


    '     Class StreamBlock
    ' 
    '         Properties: extensionSuffix, fullName, mimeType, offset, size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetRegion, StreamSpan, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace FileSystem

    ''' <summary>
    ''' A data file reference
    ''' </summary>
    Public Class StreamBlock : Inherits StreamObject

        ''' <summary>
        ''' the byte offset of current file data
        ''' </summary>
        ''' <returns></returns>
        Public Property offset As Long
        ''' <summary>
        ''' the data size
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Long

        ''' <summary>
        ''' get http mime content type of current file object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property mimeType As ContentType
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return referencePath.ToString.FileMimeType
            End Get
        End Property

        ''' <summary>
        ''' get the extension suffix name of current file node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property extensionSuffix As String
            Get
                Return referencePath.FileName.ExtensionSuffix
            End Get
        End Property

        ''' <summary>
        ''' tostring of the <see cref="referencePath"/> object.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property fullName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return referencePath.ToString
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' just create the file path reference of current stream block file
        ''' </summary>
        ''' <param name="filepath"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(filepath As FilePath)
            Call MyBase.New(filepath)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} [offset={offset}, size={StringFormats.Lanudry(size)}] ({mimeType.ToString})"
        End Function

        ''' <summary>
        ''' get data stream region of current file content data
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRegion() As BufferRegion
            Return New BufferRegion With {.size = size, .position = offset}
        End Function

        Public Function StreamSpan(file As Stream) As Stream
            Return New SubStream(file, offset, size)
        End Function

    End Class
End Namespace
