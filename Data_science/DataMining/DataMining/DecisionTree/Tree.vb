Imports System.Runtime.CompilerServices

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