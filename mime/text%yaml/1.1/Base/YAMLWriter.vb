#Region "Microsoft.VisualBasic::7cea34ee1ce57302b92050762d8136f0, mime\text%yaml\1.1\Base\YAMLWriter.vb"

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

    '   Total Lines: 66
    '    Code Lines: 51 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (22.73%)
    '     File Size: 2.31 KB


    '     Class YAMLWriter
    ' 
    '         Sub: AddDocument, AddTag, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Version = System.Version

Namespace Grammar11

    Public Class YAMLWriter

        Public Sub AddDocument(document As YAMLDocument)
            If document Is Nothing Then
                Throw New ArgumentNullException(NameOf(document))
            End If
            If m_documents.Contains(document) Then
                Throw New ArgumentException($"Document {document} is added already", NameOf(document))
            End If
            m_documents.Add(document)
        End Sub

        Public Sub AddTag(handle As String, content As String)
            If m_tags.Any(Function(t) t.Handle = handle) Then
                Throw New Exception($"Writer already contains tag {handle}")
            End If
            Dim tag As New YAMLTag(handle, content)
            m_tags.Add(tag)
        End Sub

        Public Sub Write(output As TextWriter)
            Dim emitter As New Emitter(output)

            Dim isWriteSeparator As Boolean = False

            If IsWriteVersion Then
                emitter.WriteMeta(MetaType.YAML, Version.ToString())
                isWriteSeparator = True
            End If

            If IsWriteDefaultTag Then
                emitter.WriteMeta(MetaType.TAG, DefaultTag.ToHeaderString())
                isWriteSeparator = True
            End If
            For Each tag As YAMLTag In m_tags
                emitter.WriteMeta(MetaType.TAG, tag.ToHeaderString())
                isWriteSeparator = True
            Next

            For Each doc As YAMLDocument In m_documents
                doc.Emit(emitter, isWriteSeparator)
                isWriteSeparator = True
            Next

            output.Write(ControlChars.Lf)
        End Sub

        Public Shared Version As New Version(1, 1)

        Public Const DefaultTagHandle As String = "!u!"
        Public Const DefaultTagContent As String = "tag:unity3d.com,2011:"

        Public ReadOnly DefaultTag As New YAMLTag(DefaultTagHandle, DefaultTagContent)

        Public IsWriteVersion As Boolean = True
        Public IsWriteDefaultTag As Boolean = True

        Private ReadOnly m_documents As New List(Of YAMLDocument)()
        Private ReadOnly m_tags As New List(Of YAMLTag)()
    End Class
End Namespace
