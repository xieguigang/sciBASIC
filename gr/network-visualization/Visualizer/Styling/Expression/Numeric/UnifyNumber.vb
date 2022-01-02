
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    ''' <summary>
    ''' 所有的节点都统一大小
    ''' </summary>
    Public Class UnifyNumber : Implements IGetSize

        ReadOnly r!

        Sub New(expression As String)
            Me.r! = Val(expression)
        End Sub

        Public Iterator Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            For Each n As Node In nodes
                Yield New Map(Of Node, Single) With {
                    .Key = n,
                    .Maps = r
                }
            Next
        End Function
    End Class
End Namespace