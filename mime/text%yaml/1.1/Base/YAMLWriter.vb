
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
