#Region "Microsoft.VisualBasic::0c118f269e92d3295ecf727bc3e30ca3, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Pointer.vb"

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

    '     Class Pointer
    ' 
    '         Operators: (+2 Overloads) -, (+2 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' 进行集合之中的元素的取出操作的帮助类
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Pointer(Of T) : Inherits Pointer

        ''' <summary>
        ''' Returns current line in the array and then pointer moves to next.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(array As T(), i As Pointer(Of T)) As T
            Return array(+i)
        End Operator

        Public Overloads Shared Operator -(array As T(), i As Pointer(Of T)) As T
            Return array(-i)
        End Operator

        Public Overloads Shared Operator +(list As List(Of T), i As Pointer(Of T)) As T
            Return list(+i)
        End Operator

        Public Overloads Shared Operator -(list As List(Of T), i As Pointer(Of T)) As T
            Return list(-i)
        End Operator
    End Class
End Namespace
