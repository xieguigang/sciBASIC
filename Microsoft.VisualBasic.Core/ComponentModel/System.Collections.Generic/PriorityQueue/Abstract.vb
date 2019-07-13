#Region "Microsoft.VisualBasic::bca0d86dd22bc409d262feb73d47f50b, Microsoft.VisualBasic.Core\ComponentModel\System.Collections.Generic\PriorityQueue\Abstract.vb"

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

    '     Interface IPriorityQueue
    ' 
    '         Function: Peek, Pop, Push
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection

    Public Interface IPriorityQueue(Of T)
        Inherits ICollection
        Inherits ICloneable
        Inherits IList

        Function Push(O As T) As Integer
        Function Pop() As T
        Function Peek() As T

    End Interface
End Namespace
