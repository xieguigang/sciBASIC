Imports System.Runtime.CompilerServices

Namespace Parallel

    Public Module ParallelExtension

        Public MustInherit Class TaggedGroupData(Of T_TAG)
            Public Property TAG As T_TAG

            Public Overrides Function ToString() As String
                Return TAG.ToString
            End Function
        End Class

        Public Class GroupListNode(Of T, T_TAG) : Inherits ParallelExtension.TaggedGroupData(Of T_TAG)
            Implements System.Collections.Generic.IEnumerable(Of T)

            Dim _Group As List(Of T)

            Public Property Group As List(Of T)
                Get
                    Return _Group
                End Get
                Set(value As List(Of T))
                    _Group = value
                    If value.IsNullOrEmpty Then
                        _InitReads = 0
                    Else
                        _InitReads = value.Count
                    End If
                End Set
            End Property

            Public ReadOnly Property Count As Integer
                Get
                    Return Group.Count
                End Get
            End Property

            ''' <summary>
            ''' 由于<see cref="Group"/>在分组之后的后续的操作的过程之中元素会发生改变，
            ''' 所以在这个属性之中存储了在初始化<see cref="Group"/>列表的时候的原始的列表之中的元素的个数以满足一些其他的算法操作
            ''' </summary>
            ''' <returns></returns>
            Public ReadOnly Property InitReads As Integer

            Public Overrides Function ToString() As String
                Return MyBase.ToString & $" // {NameOf(InitReads)}:={InitReads},  current:={Count}"
            End Function

            Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                For Each obj In Group
                    Yield obj
                Next
            End Function

            Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Yield GetEnumerator()
            End Function
        End Class

        Public Class GroupResult(Of T, T_TAG) : Inherits ParallelExtension.TaggedGroupData(Of T_TAG)
            Implements System.Collections.Generic.IEnumerable(Of T)

            Public Property Group As T()
            Public ReadOnly Property Count As Integer
                Get
                    Return Group.Length
                End Get
            End Property

            Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                For i As Integer = 0 To Group.Length - 1
                    Yield Group(i)
                Next
            End Function

            Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Yield GetEnumerator()
            End Function
        End Class

        ''' <summary>
        ''' 貌似使用LINQ进行Group操作的时候是没有并行化的，灰非常慢，则可以使用这个拓展函数来获取较好的性能
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="T_TAG"></typeparam>
        ''' <param name="Collection"></param>
        ''' <returns></returns>
        <Extension> Public Function ParallelGroup(Of T, T_TAG)(Collection As Generic.IEnumerable(Of T), getGuid As Func(Of T, T_TAG)) As GroupResult(Of T, T_TAG)()
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Generating guid index...")
            Dim TAGS = (From item As T In Collection.AsParallel Select guid = getGuid(item), item).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Start to create lquery partitions...")
            Dim Partitions = TAGS.Split(TAGS.Length / Environment.ProcessorCount)
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Invoke parallel group operations....")
            Call Console.WriteLine($"[DEBUG {Now.ToString}] First groups...")
            Dim FirstGroups = (From Partition In Partitions.AsParallel
                               Select (From obj In (From Token In Partition
                                                    Select Token.guid
                                                    Group guid By guid Into Group).ToArray Select obj.guid).ToArray).ToArray.MatrixToList
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Unique group...")
            Dim UniqueGroup = (From TAG As T_TAG
                               In FirstGroups.AsParallel
                               Select TAG
                               Group TAG By TAG Into Group).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Generating group result....")
            Call Console.WriteLine(" * Cache data....")
            Dim Cache = (From TAG In UniqueGroup.AsParallel Select TAG, List = New List(Of T)).ToArray
            Call Console.WriteLine(" * Address data.....")
            Dim Addressing = (From obj In TAGS.AsParallel Select obj, List = (From item In Cache Where item.TAG.TAG.Equals(obj.guid) Select item.List).FirstOrDefault).ToArray
            For Each Address In Addressing
                Call Address.List.Add(Address.obj.item)
            Next
            Call Console.WriteLine(" * Generate result.....")
            Dim LQuery = (From TAG In Cache.AsParallel Select New GroupResult(Of T, T_TAG)() With {.TAG = TAG.TAG.TAG, .Group = TAG.List.ToArray}).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Parallel group operation job done!")
            Return LQuery
        End Function

        ''' <summary>
        ''' Start a new thread and then returns the background thread task handle.
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns></returns>
        <Extension> Public Function RunTask(start As Threading.ThreadStart) As Threading.Thread
            Dim Thread As New Threading.Thread(start)
            Call Thread.Start()
            Return Thread
        End Function
    End Module
End Namespace