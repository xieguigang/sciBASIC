
Namespace Exporter.YAML
	Public NotInheritable Class YAMLDocument
		Friend Sub New()
		End Sub

		Public Function CreateScalarRoot() As YAMLScalarNode
			Dim root__1 As New YAMLScalarNode()
			Root = root__1
			Return root__1
		End Function

		Public Function CreateSequenceRoot() As YAMLSequenceNode
			Dim root__1 As New YAMLSequenceNode()
			Root = root__1
			Return root__1
		End Function

		Public Function CreateMappingRoot() As YAMLMappingNode
			Dim root__1 As New YAMLMappingNode()
			Root = root__1
			Return root__1
		End Function

		Friend Sub Emit(emitter As Emitter, isSeparator As Boolean)
			If isSeparator Then
				emitter.Write("---").WriteWhitespace()
			End If

			Root.Emit(emitter)
		End Sub

		Public Property Root() As YAMLNode
			Get
				Return m_Root
			End Get
			Private Set
				m_Root = Value
			End Set
		End Property
		Private m_Root As YAMLNode
	End Class
End Namespace
