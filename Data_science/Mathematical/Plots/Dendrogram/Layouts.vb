Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree
Imports Microsoft.VisualBasic.Imaging

Namespace Dendrogram

    Public Module Layouts

        ''' <summary>
        ''' 水平放置的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```
        '''      +
        '''      |
        '''      |
        ''' +----+------+
        ''' |           |
        ''' |           |
        ''' +           +
        ''' ```
        ''' </remarks>
        Public Function HorizontalLayout(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double)) As BinaryTree(Of ILayoutedObject(Of T))

        End Function

        ''' <summary>
        ''' 垂直放置的
        ''' </summary>
        ''' <returns></returns>
        Public Function Vertical(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double)) As BinaryTree(Of ILayoutedObject(Of T))

        End Function

        ''' <summary>
        ''' 圆弧状的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/5089030/how-do-i-create-a-radial-cluster-like-the-following-code-example-in-python
        ''' </remarks>
        Public Function Radial(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double)) As BinaryTree(Of ILayoutedObject(Of T))

        End Function
    End Module
End Namespace