Namespace Grammar11

    Public MustInherit Class YAMLNode

        Friend Overridable Sub Emit(emitter As Emitter)
            Dim isWrote As Boolean = False
            If Not CustomTag.IsEmpty Then
                emitter.Write(CustomTag.ToString()).WriteWhitespace()
                isWrote = True
            End If
            If Anchor <> String.Empty Then
                emitter.Write("&").Write(Anchor).WriteWhitespace()
                isWrote = True
            End If

            If isWrote Then
                If IsMultyline Then
                    emitter.WriteLine()
                End If
            End If
        End Sub

        Public MustOverride ReadOnly Property NodeType As YAMLNodeType
        Public MustOverride ReadOnly Property IsMultyline As Boolean
        Public MustOverride ReadOnly Property IsIndent As Boolean

        Public Property Tag() As String
            Get
                Return CustomTag.Content
            End Get
            Set
                CustomTag = New YAMLTag(YAMLWriter.DefaultTagHandle, Value)
            End Set
        End Property

        Public Property CustomTag() As YAMLTag
        Public ReadOnly Property Anchor As String = String.Empty

    End Class
End Namespace
