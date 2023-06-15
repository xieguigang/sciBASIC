#Region "Microsoft.VisualBasic::4c1bdc1786721321d87316ae8ab5c7af, sciBASIC#\Data\BinaryData\HDSPack\FileSystem\StreamBlock.vb"

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

'   Total Lines: 43
'    Code Lines: 27
' Comment Lines: 7
'   Blank Lines: 9
'     File Size: 1.21 KB


'     Class StreamBlock
' 
'         Properties: fullName, mimeType, offset, size
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace FileSystem

    ''' <summary>
    ''' A data file
    ''' </summary>
    Public Class StreamBlock : Inherits StreamObject

        Public Property offset As Long
        Public Property size As Long

        Public ReadOnly Property mimeType As ContentType
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return referencePath.ToString.FileMimeType
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(filepath As FilePath)
            Call MyBase.New(filepath)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} [offset={offset}, size={StringFormats.Lanudry(size)}] ({mimeType.ToString})"
        End Function

    End Class
End Namespace
