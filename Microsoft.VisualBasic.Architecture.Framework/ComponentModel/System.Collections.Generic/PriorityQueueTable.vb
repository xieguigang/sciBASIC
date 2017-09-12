'Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

'Namespace ComponentModel.Collection

'    ''' <summary>
'    ''' 
'    ''' </summary>
'    ''' <typeparam name="T"></typeparam>
'    ''' <remarks>经过测试发现性能太低了</remarks>
'    Public Class PriorityQueueTable(Of T As {IComparable, IComparable(Of T), IReadOnlyId}) : Inherits PriorityQueue(Of T)

'        Dim index As New Index(Of String)

'        Public Overrides Sub Clear()
'            Call MyBase.Clear()
'            Call index.Clear()
'        End Sub

'        Public Overrides Sub Enqueue(queueItem As T)
'            MyBase.Enqueue(queueItem) ' 需要使用对象的compare方法来进行排序
'            createIndex()
'        End Sub

'        Private Sub createIndex()
'            index = list _
'                .Select(Function(x) x.Identity) _
'                .Indexing
'        End Sub

'        Public Overrides Sub Remove(o As T)
'            Call list.RemoveAt(index(o.Identity))
'            createIndex()
'        End Sub

'        Public Overrides Function Dequeue() As T
'            Dim out As T = MyBase.Dequeue()
'            createIndex()
'            Return out
'        End Function

'        Public Overrides Function Contains(queueItem As T) As Boolean
'            Return index.IndexOf(queueItem.Identity) > -1
'        End Function
'    End Class
'End Namespace