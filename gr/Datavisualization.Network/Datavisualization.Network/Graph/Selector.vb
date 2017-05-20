Imports System.Runtime.CompilerServices

Namespace Graph

    Public Module Selector

        <Extension> Public Function SelectNodeValue(property$) As Func(Of Node, Object)

        End Function

        <Extension> Public Function SelectEdgeValue(property$) As Func(Of Edge, Object)

        End Function
    End Module
End Namespace