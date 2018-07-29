Namespace Grammar11

    Public NotInheritable Class YAMLDocument

        Friend Sub New()
        End Sub

        Public Function CreateScalarRoot() As YAMLScalarNode
            Dim root As New YAMLScalarNode()
            Me.Root = root
            Return root
        End Function

        Public Function CreateSequenceRoot() As YAMLSequenceNode
            Dim root As New YAMLSequenceNode()
            Me.Root = root
            Return root
        End Function

        Public Function CreateMappingRoot() As YAMLMappingNode
            Dim root As New YAMLMappingNode()
            Me.Root = root
            Return root
        End Function

        Friend Sub Emit(emitter As Emitter, isSeparator As Boolean)
            If isSeparator Then
                emitter.Write("---").WriteWhitespace()
            End If

            Root.Emit(emitter)
        End Sub

        Public Property Root() As YAMLNode

    End Class
End Namespace
