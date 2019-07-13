#Region "Microsoft.VisualBasic::879901a8bb9d27ea3c6cc6a202105ce3, mime\text%yaml\1.1\IDictionaryExtensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module IDictionaryExtensions
    ' 
    '         Function: (+3 Overloads) ExportYAML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Grammar11

    Public Module IDictionaryExtensions

        <Extension>
        Public Function ExportYAML(this As IReadOnlyDictionary(Of UInteger, String)) As YAMLNode
            Dim node As New YAMLMappingNode()
            For Each kvp In this
                node.Add(kvp.Key.ToString(), kvp.Value)
            Next
            Return node
        End Function

        <Extension>
        Public Function ExportYAML(this As IReadOnlyDictionary(Of String, String)) As YAMLNode
            Dim node As New YAMLMappingNode()
            For Each kvp In this
                node.Add(kvp.Key, kvp.Value)
            Next
            Return node
        End Function

        <Extension>
        Public Function ExportYAML(this As IReadOnlyDictionary(Of String, Single)) As YAMLNode
            Dim node As New YAMLSequenceNode(SequenceStyle.Block)
            For Each kvp In this
                Dim map As New YAMLMappingNode()
                map.Add(kvp.Key, kvp.Value)
                node.Add(map)
            Next
            Return node
        End Function
    End Module
End Namespace
