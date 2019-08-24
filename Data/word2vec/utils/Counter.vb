Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace org.nlp.util

    ''' <summary>
    ''' 计数器
    ''' 最初代码来自Ansj的tree-split包中的love.cq.util;
    ''' @author fangy </summary>
    ''' @param <T> 键类型 </param>
    Public Class Counter(Of tT)
        Private hm As Dictionary(Of tT, CountInteger) = Nothing

        Public Sub New()
            hm = New Dictionary(Of tT, CountInteger)()
        End Sub

        Public Sub New(ByVal initialCapacity As Integer)
            hm = New Dictionary(Of tT, CountInteger)(initialCapacity)
        End Sub

        Public Class CountInteger
            Private ReadOnly outerInstance As Counter(Of tT)
            Friend count As Integer

            Public Sub New(ByVal outerInstance As Counter(Of tT), ByVal initCount As Integer)
                Me.outerInstance = outerInstance
                count = initCount
            End Sub

            Public Overridable Sub [set](ByVal num As Integer)
                count = num
            End Sub

            Public Overridable Function value() As Integer
                Return count
            End Function

            Public Overrides Function ToString() As String
                Return "Count: " & count.ToString()
            End Function
        End Class

        ''' <summary>
        ''' 增加一个元素，并增加其计数 </summary>
        ''' <paramname="t"> 元素 </param>
        ''' <paramname="n"> 计数 </param>
        Public Overridable Sub add(ByVal t As tT, ByVal n As Integer)
            Dim newCount As CountInteger = New CountInteger(Me, n)
            Dim oldCount As CountInteger = CSharpImpl.__Assign(hm(t), newCount)

            If oldCount IsNot Nothing Then
                newCount.set(oldCount.value() + n)
            End If
        End Sub

        ''' <summary>
        ''' 增加一个元素，计数默认增加1 </summary>
        ''' <paramname="t"> 元素 </param>
        Public Overridable Sub add(ByVal t As tT)
            add(t, 1)
        End Sub

        ''' <summary>
        ''' 获得某个元素的计数 </summary>
        ''' <paramname="t"> 待查询的元素 </param>
        ''' <returns> 数目 </returns>
        Public Overridable Function [get](ByVal t As tT) As Integer
            Dim count = hm.GetValueOrNull(t)

            If count Is Nothing Then
                Return 0
            Else
                Return count.value()
            End If
        End Function

        ''' <summary>
        ''' 获取哈希表中键的个数 </summary>
        ''' <returns> 键的数量 </returns>
        Public Overridable Function size() As Integer
            Return hm.Count
        End Function

        ''' <summary>
        ''' 删除一个元素 </summary>
        ''' <paramname="t"> 元素 </param>
        Public Overridable Sub remove(ByVal t As tT)
            hm.Remove(t)
        End Sub

        ''' <summary>
        ''' 输出已构建好的哈希计数表 </summary>
        ''' <returns> 哈希表 </returns>
        Public Overridable Function keySet() As ISet(Of tT)
            Return hm.Keys
        End Function

        ''' <summary>
        ''' 将计数器转换为字符串 </summary>
        ''' <returns> 字符串 </returns>
        Public Overrides Function ToString() As String
            Dim iterator As IEnumerator(Of KeyValuePair(Of tT, CountInteger)) = SetOfKeyValuePairs(Of tT, Global.org.nlp.util.Counter(Of tT).CountInteger)(hm).GetEnumerator()
            Dim sb As StringBuilder = New StringBuilder()
            Dim [next] As KeyValuePair(Of tT, CountInteger) = Nothing

            While iterator.MoveNext()
                [next] = iterator.Current
                sb.Append([next].Key)
                sb.Append(vbTab)
                sb.Append([next].Value)
                sb.Append(vbLf)
            End While

            Return sb.ToString()
        End Function

        Public Shared Sub Main(ByVal args As String())
            Dim strKeys = New String() {"1", "2", "3", "1", "2", "1", "3", "3", "3", "1", "2"}
            Dim counter As Counter(Of String) = New Counter(Of String)()

            For Each strKey In strKeys
                counter.add(strKey)
            Next

            For Each strKey As String In counter.Keys
                Console.WriteLine(strKey & " : " & counter.get(strKey))
            Next

            Console.WriteLine(counter.get("9"))
            '        System.out.println(Long.MAX_VALUE);
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace
