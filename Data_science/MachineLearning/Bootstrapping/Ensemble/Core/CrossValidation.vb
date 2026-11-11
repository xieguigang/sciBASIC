Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' K 折划分工具。返回的每一折都是「测试集下标」，训练集是其余下标。
    ''' </summary>
    ''' <remarks>
    ''' 划分使用独立的 <c>Random(seed)</c> 实例，因此相同参数下折分完全一致；
    ''' 分类任务应使用 <see cref="StratifiedKFold"/>，让每一折的类别比例与全集一致。
    ''' </remarks>
    Public NotInheritable Class CrossValidation

        Private Sub New()
        End Sub

        ''' <summary>校验样本数与折数</summary>
        Public Shared Sub Validate(n As Integer, folds As Integer)
            If n <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), $"the sample size must be positive, but got {n}!")
            End If
            If folds < 2 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(folds), $"cross validation requires at least 2 folds, but got {folds}!")
            End If
            If folds > n Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(folds), $"the fold count ({folds}) can not exceed the sample size ({n})!")
            End If
        End Sub

        ''' <summary>普通 K 折：整体洗牌后按顺序切分</summary>
        <Obsolete("use KFold with an explicit seed; this overload uses a fixed seed of 42")>
        Public Shared Function KFold(n As Integer, folds As Integer) As Integer()()
            Return KFold(n, folds, 42)
        End Function

        ''' <summary>普通 K 折：整体洗牌后按顺序切分</summary>
        Public Shared Function KFold(n As Integer, folds As Integer, seed As Integer) As Integer()()
            Call Validate(n, folds)

            Dim order As Integer() = Enumerable.Range(0, n).ToArray()
            Dim rnd As New Random(seed)

            ' Fisher-Yates 洗牌
            For i As Integer = n - 1 To 1 Step -1
                Dim j As Integer = rnd.Next(i + 1)
                Dim swap As Integer = order(i)

                order(i) = order(j)
                order(j) = swap
            Next

            Return Split(order, folds)
        End Function

        ''' <summary>
        ''' 分层 K 折：每个类别内部独立洗牌后轮流分配到各折，
        ''' 使每一折的类别比例与全集尽量一致。
        ''' </summary>
        Public Shared Function StratifiedKFold(yIndex As Integer(), folds As Integer, seed As Integer) As Integer()()
            If yIndex Is Nothing OrElse yIndex.Length = 0 Then
                Throw New ArgumentException("the class index vector can not be empty!", NameOf(yIndex))
            End If

            Call Validate(yIndex.Length, folds)

            Dim groups As New SortedDictionary(Of Integer, List(Of Integer))()

            For i As Integer = 0 To yIndex.Length - 1
                Dim key As Integer = yIndex(i)

                If Not groups.ContainsKey(key) Then
                    groups(key) = New List(Of Integer)()
                End If

                Call groups(key).Add(i)
            Next

            Dim buckets As New List(Of List(Of Integer))()

            For k As Integer = 0 To folds - 1
                buckets.Add(New List(Of Integer)())
            Next

            Dim rnd As New Random(seed)
            Dim offset As Integer = 0

            ' 类别按标签升序遍历，保证不同运行之间的分配顺序稳定
            For Each pair As KeyValuePair(Of Integer, List(Of Integer)) In groups
                Dim members As Integer() = pair.Value.ToArray()

                For i As Integer = members.Length - 1 To 1 Step -1
                    Dim j As Integer = rnd.Next(i + 1)
                    Dim swap As Integer = members(i)

                    members(i) = members(j)
                    members(j) = swap
                Next

                For i As Integer = 0 To members.Length - 1
                    Call buckets((offset + i) Mod folds).Add(members(i))
                Next

                offset = (offset + members.Length) Mod folds
            Next

            Dim result As Integer()() = New Integer(folds - 1)() {}

            For k As Integer = 0 To folds - 1
                result(k) = buckets(k).ToArray()
            Next

            Return result
        End Function

        ''' <summary>
        ''' 给定测试集下标，返回其余下标（即该折的训练集）。
        ''' </summary>
        Public Shared Function Complement(n As Integer, test As Integer()) As Integer()
            If n <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), $"the sample size must be positive, but got {n}!")
            End If
            If test Is Nothing Then
                Throw New ArgumentNullException(NameOf(test))
            End If

            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To test.Length - 1
                Dim slot As Integer = test(i)

                If slot < 0 OrElse slot >= n Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(test), $"the test index {slot} is out of range [0, {n})!")
                End If

                mask(slot) = True
            Next

            Dim rest As New List(Of Integer)()

            For i As Integer = 0 To n - 1
                If Not mask(i) Then
                    Call rest.Add(i)
                End If
            Next

            If rest.Count = 0 Then
                Throw New ArgumentException("the training split is empty; reduce the fold count!", NameOf(test))
            End If

            Return rest.ToArray()
        End Function

        ''' <summary>把排列按折平均切分</summary>
        Private Shared Function Split(order As Integer(), folds As Integer) As Integer()()
            Dim n As Integer = order.Length
            Dim result As Integer()() = New Integer(folds - 1)() {}
            Dim baseSize As Integer = n \ folds
            Dim remainder As Integer = n Mod folds
            Dim cursor As Integer = 0

            For k As Integer = 0 To folds - 1
                Dim size As Integer = baseSize + If(k < remainder, 1, 0)
                Dim part As Integer() = New Integer(size - 1) {}

                Call Array.Copy(order, cursor, part, 0, size)
                cursor += size
                result(k) = part
            Next

            Return result
        End Function

    End Class
End Namespace
