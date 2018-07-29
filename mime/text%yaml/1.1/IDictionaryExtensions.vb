Imports System.Collections.Generic
Imports UtinyRipper.AssetExporters

Namespace Exporter.YAML
	Public NotInheritable Class IDictionaryExtensions
		Private Sub New()
		End Sub
		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IReadOnlyDictionary(Of UInteger, String)) As YAMLNode
			Dim node As New YAMLMappingNode()
			For Each kvp As var In _this
				node.Add(kvp.Key.ToString(), kvp.Value)
			Next
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IReadOnlyDictionary(Of String, String)) As YAMLNode
			Dim node As New YAMLMappingNode()
			For Each kvp As var In _this
				node.Add(kvp.Key, kvp.Value)
			Next
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IReadOnlyDictionary(Of String, Single)) As YAMLNode
			Dim node As New YAMLSequenceNode(SequenceStyle.Block)
			For Each kvp As var In _this
				Dim map As New YAMLMappingNode()
				map.Add(kvp.Key, kvp.Value)
				node.Add(map)
			Next
			Return node
		End Function
	End Class
End Namespace
