#Region "Microsoft.VisualBasic::879901a8bb9d27ea3c6cc6a202105ce3, mime\text%yaml\1.1\IDictionaryExtensions.vb"

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
