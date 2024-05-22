#Region "Microsoft.VisualBasic::3a182abf3402a76242f4d50df7bf5102, Data_science\DataMining\DataMining\DecisionTree\Tree.vb"

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

    '   Total Lines: 91
    '    Code Lines: 49 (53.85%)
    ' Comment Lines: 27 (29.67%)
    '    - Xml Docs: 74.07%
    ' 
    '   Blank Lines: 15 (16.48%)
    '     File Size: 3.33 KB


    '     Class Tree
    ' 
    '         Properties: root
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) CalculateResult, Learn, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data

Namespace DecisionTree

    ''' <summary>
    ''' Implementation of the ID3 to create a decision tree
    ''' 
    ''' > https://github.com/WolfgangOfner/DecisionTree
    ''' </summary>
    Public Class Tree

        Public Property root As TreeNode

        ''' <summary>
        ''' Create a new empty ID3 based decision tree.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create a new decision tree and train for <see cref="root"/>.
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As DataTable)
            root = Tree.Learn(data)
        End Sub

        ''' <summary>
        ''' Load a trained decision tree model
        ''' </summary>
        ''' <param name="model">A trained model which comes from json/xml</param>
        Sub New(model As TreeNode)
            root = model
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CalculateResult(valuesForQuery As IDictionary(Of String, String)) As ClassifyResult
            Return CalculateResult(root, valuesForQuery, New ClassifyResult)
        End Function

        Public Overrides Function ToString() As String
            Return root.attributes.ToString
        End Function

        Private Shared Function CalculateResult(root As TreeNode, valuesForQuery As IDictionary(Of String, String), result As ClassifyResult) As ClassifyResult
            Dim valueFound = False

            result.explains += root.name.ToUpper
            ' 因为在计算的过程之中，函数会删除查询字典之中的一些输入值
            ' 所以为了不修改外部的数据
            ' 在这里需要做一份数据拷贝
            valuesForQuery = New Dictionary(Of String, String)(valuesForQuery)

            If root.isLeaf Then
                ' result.explains += root.edge
                result.result = root.name.ToUpper()
                valueFound = True
            Else
                For Each childNode In root.childNodes
                    For Each entry In valuesForQuery
                        If childNode.edge.ToUpper().Equals(entry.Value.ToUpper()) AndAlso root.name.ToUpper().Equals(entry.Key.ToUpper()) Then
                            valuesForQuery.Remove(entry.Key)
                            result.explains += childNode.edge

                            Return CalculateResult(childNode, valuesForQuery, result)
                        End If
                    Next
                Next
            End If

            ' if the user entered an invalid attribute
            If Not valueFound Then
                result.result = "Attribute not found"
            End If

            Return result
        End Function

        ''' <summary>
        ''' Create tree of <see cref="root"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Learn(data As DataTable) As TreeNode
            Return Algorithm.Learn(data, "")
        End Function
    End Class
End Namespace
