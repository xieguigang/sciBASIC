Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace utils

    ''' <summary>
    ''' 计数器
    ''' 最初代码来自Ansj的tree-split包中的love.cq.util;
    ''' @author fangy </summary>
    Public Class Counter(Of tT)
        Private hm As Dictionary(Of tT, CountInteger) = Nothing

        Public Sub New()
            hm = New Dictionary(Of tT, CountInteger)()
        End Sub

        Public Sub New(initialCapacity As Integer)
            hm = New Dictionary(Of tT, CountInteger)(initialCapacity)
        End Sub

        Public Class CountInteger
            Private ReadOnly outerInstance As Counter(Of tT)
            Friend count As Integer

            Public Sub New(outerInstance As Counter(Of tT), initCount As Integer)
                Me.outerInstance = outerInstance
                count = initCount
            End Sub

            Public Sub [set](num As Integer)
                count = num
            End Sub

            Public Function value() As Integer
                Return count
            End Function

            Public Overrides Function ToString() As String
                Return "Count: " & count.ToString()
            End Function
        End Class

        ''' <summary>
        ''' 增加一个元素，并增加其计数 </summary>
        ''' <param name="t"> 元素 </param>
        ''' <param name="n"> 计数 </param>
        Public Sub add(t As tT, n As Integer)
            Dim newCount As CountInteger = New CountInteger(Me, n)
            Dim oldCount As CountInteger

            hm(t) = newCount
            oldCount = newCount

            If oldCount IsNot Nothing Then
                newCount.set(oldCount.value() + n)
            End If
        End Sub

        ''' <summary>
        ''' 增加一个元素，计数默认增加1 </summary>
        ''' <param name="t"> 元素 </param>
        Public Sub add(t As tT)
            add(t, 1)
        End Sub

        ''' <summary>
        ''' 获得某个元素的计数 </summary>
        ''' <param name="t"> 待查询的元素 </param>
        ''' <returns> 数目 </returns>
        Public Function [get](t As tT) As Integer
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
        Public Function size() As Integer
            Return hm.Count
        End Function

        ''' <summary>
        ''' 删除一个元素 </summary>
        ''' <param name="t"> 元素 </param>
        Public Sub remove(t As tT)
            hm.Remove(t)
        End Sub

        ''' <summary>
        ''' 输出已构建好的哈希计数表 </summary>
        ''' <returns> 哈希表 </returns>
        Public Function keySet() As IEnumerable(Of tT)
            Return hm.Keys
        End Function

        ''' <summary>
        ''' 将计数器转换为字符串 </summary>
        ''' <returns> 字符串 </returns>
        Public Overrides Function ToString() As String
            Dim iterator As IEnumerator(Of KeyValuePair(Of tT, CountInteger)) = SetOfKeyValuePairs(Of tT, Counter(Of tT).CountInteger)(hm).GetEnumerator()
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
    End Class
End Namespace
