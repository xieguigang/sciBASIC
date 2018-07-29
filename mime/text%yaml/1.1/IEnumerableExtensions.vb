Imports System.Collections.Generic
Imports System.Text

Namespace Exporter.YAML
	Public NotInheritable Class IEnumerableExtensions
		Private Sub New()
		End Sub
		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Boolean)) As YAMLNode
			For Each value As Boolean In _this
				Dim bvalue As Byte = CByte(If(value, 1, 0))
				s_sb.Append(bvalue.ToHexString())
			Next
			Dim node As New YAMLScalarNode(s_sb.ToString())
			s_sb.Length = 0
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Byte)) As YAMLNode
			For Each value As Byte In _this
				s_sb.Append(value.ToHexString())
			Next
			Dim node As New YAMLScalarNode(s_sb.ToString())
			s_sb.Length = 0
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of UShort), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As UShort In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As UShort In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Short), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As Short In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As Short In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of UInteger), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As UInteger In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As UInteger In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Integer), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As Integer In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As Integer In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of ULong), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As ULong In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As ULong In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Long), isRaw As Boolean) As YAMLNode
			If isRaw Then
				For Each value As Long In _this
					s_sb.Append(value.ToHexString())
				Next
				Dim node As New YAMLScalarNode(s_sb.ToString())
				s_sb.Length = 0
				Return node
			Else
				Dim node As New YAMLSequenceNode(SequenceStyle.Block)
				For Each value As Long In _this
					node.Add(value)
				Next
				Return node
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Single)) As YAMLNode
			Dim node As New YAMLSequenceNode(SequenceStyle.Block)
			For Each value As Single In _this
				node.Add(value)
			Next
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of Double)) As YAMLNode
			Dim node As New YAMLSequenceNode(SequenceStyle.Block)
			For Each value As Double In _this
				node.Add(value)
			Next
			Return node
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ExportYAML(_this As IEnumerable(Of String)) As YAMLNode
			Dim node As New YAMLSequenceNode(SequenceStyle.Block)
			For Each value As String In _this
				node.Add(value)
			Next
			Return node
		End Function

		Private Shared ReadOnly s_sb As New StringBuilder()
	End Class
End Namespace
