Imports System.Diagnostics
Imports System.Text
Imports Microsoft.VisualBasic.Data.MyersDiff

''' <summary>
''' 自测程序：验证 MyersDiff 模块的正确性、最优性与计算性能。
'''
''' 用法：test.exe --test
'''
''' 验证内容：
'''   1. 结构不变量：把差异项按 Delete/Equal 重放必须还原旧序列，按 Equal/Insert
'''      重放必须还原新序列，且每个差异项的索引与取值自洽；
'''   2. 最短编辑脚本：编辑距离（删除数 + 插入数）必须等于 N + M - 2*LCS，
'''      LCS 由一个独立的 O(N*M) 动态规划实现交叉验证；
'''   3. 边界：空序列、空字符串、完全相同、超长公共前后缀、编辑距离上界；
'''   4. 统一差异格式：解析 ToUnifiedDiff 的输出并回放，必须还原新序列；
'''   5. 性能：大文件 + 小改动、完全相同的大文件、完全不相关的大文件。
''' </summary>
Module MyersDiffSelfTest

    Dim passed As Integer = 0
    Dim failed As Integer = 0

    Public ReadOnly Property FailedCount As Integer
        Get
            Return failed
        End Get
    End Property

    Sub Run()
        TestEdgeCases()
        TestBasicEdits()
        TestCharDiff()
        TestUnifiedDiff()
        FuzzSmall()
        FuzzMedium()
        TestMaxEditDistance()
        TestPerformance()

        Console.WriteLine()
        Console.WriteLine($"===== self test: passed={passed}, failed={failed} =====")
    End Sub

    Private Sub Check(name As String, cond As Boolean, Optional detail As String = Nothing)
        If cond Then
            passed += 1
        Else
            failed += 1
            Console.WriteLine($"  [FAIL] {name}: {detail}")
        End If
    End Sub

    ' ------------------------------------------------------------------
    ' 结构不变量：差异项必须能重放出原始的两个序列
    ' ------------------------------------------------------------------
    Private Function Validate(oldLines As String(), newLines As String(), result As DiffResult) As String
        Dim rebuildOld As New List(Of String)()
        Dim rebuildNew As New List(Of String)()

        For Each item As DiffItem In result.Items
            Select Case item.Type
                Case EditType.Equal
                    If item.OldIndex < 0 OrElse item.OldIndex >= oldLines.Length Then Return $"equal oldIndex out of range: {item.OldIndex}"
                    If item.NewIndex < 0 OrElse item.NewIndex >= newLines.Length Then Return $"equal newIndex out of range: {item.NewIndex}"
                    If oldLines(item.OldIndex) <> newLines(item.NewIndex) Then Return $"equal lines are different: '{oldLines(item.OldIndex)}' vs '{newLines(item.NewIndex)}'"
                    If item.Value <> oldLines(item.OldIndex) Then Return "equal value mismatch"

                    rebuildOld.Add(oldLines(item.OldIndex))
                    rebuildNew.Add(newLines(item.NewIndex))

                Case EditType.Delete
                    If item.OldIndex < 0 OrElse item.OldIndex >= oldLines.Length Then Return $"delete oldIndex out of range: {item.OldIndex}"
                    If item.NewIndex <> -1 Then Return "delete newIndex must be -1"
                    If item.Value <> oldLines(item.OldIndex) Then Return "delete value mismatch"

                    rebuildOld.Add(oldLines(item.OldIndex))

                Case EditType.Insert
                    If item.NewIndex < 0 OrElse item.NewIndex >= newLines.Length Then Return $"insert newIndex out of range: {item.NewIndex}"
                    If item.OldIndex <> -1 Then Return "insert oldIndex must be -1"
                    If item.Value <> newLines(item.NewIndex) Then Return "insert value mismatch"

                    rebuildNew.Add(newLines(item.NewIndex))
            End Select
        Next

        If rebuildOld.Count <> oldLines.Length Then Return $"old size {rebuildOld.Count} <> {oldLines.Length}"
        If rebuildNew.Count <> newLines.Length Then Return $"new size {rebuildNew.Count} <> {newLines.Length}"

        For i As Integer = 0 To rebuildOld.Count - 1
            If rebuildOld(i) <> oldLines(i) Then Return $"old line {i} mismatch"
        Next
        For i As Integer = 0 To rebuildNew.Count - 1
            If rebuildNew(i) <> newLines(i) Then Return $"new line {i} mismatch"
        Next

        If result.OldCount <> oldLines.Length Then Return "OldCount mismatch"
        If result.NewCount <> newLines.Length Then Return "NewCount mismatch"

        Return Nothing
    End Function

    ''' <summary>独立的 LCS 实现（O(N*M) 动态规划），用于交叉验证编辑距离</summary>
    Private Function LCS(a As String(), b As String()) As Integer
        Dim n As Integer = a.Length
        Dim m As Integer = b.Length
        Dim dp As Integer()() = New Integer(n)() {}

        For i As Integer = 0 To n
            dp(i) = New Integer(m) {}
        Next

        For i As Integer = 1 To n
            For j As Integer = 1 To m
                If String.Equals(a(i - 1), b(j - 1), StringComparison.Ordinal) Then
                    dp(i)(j) = dp(i - 1)(j - 1) + 1
                ElseIf dp(i - 1)(j) >= dp(i)(j - 1) Then
                    dp(i)(j) = dp(i - 1)(j)
                Else
                    dp(i)(j) = dp(i)(j - 1)
                End If
            Next
        Next

        Return dp(n)(m)
    End Function

    Private Function SeqEqual(a As String(), b As String()) As Boolean
        If a.Length <> b.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            If a(i) <> b(i) Then Return False
        Next

        Return True
    End Function

    ' ------------------------------------------------------------------
    ' 边界用例
    ' ------------------------------------------------------------------
    Private Sub TestEdgeCases()
        Console.WriteLine("── 边界用例")

        Dim diff As New MyersDiff()

        ' 两个空序列
        Dim r = diff.Compare({}, {})

        Check("empty/empty items", r.Items.Count = 0, $"items={r.Items.Count}")
        Check("empty/empty similarity", Math.Abs(r.Similarity - 1.0) < 0.000001, $"sim={r.Similarity}")

        ' 旧为空 -> 全部插入
        r = diff.Compare({}, {"a", "b"})

        Check("empty old: 2 inserts", r.InsertCount = 2 AndAlso r.DeleteCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")
        Check("empty old: invariant", Validate({}, {"a", "b"}, r) Is Nothing, Validate({}, {"a", "b"}, r))

        ' 新为空 -> 全部删除
        r = diff.Compare({"a", "b"}, {})

        Check("empty new: 2 deletes", r.DeleteCount = 2 AndAlso r.InsertCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")
        Check("empty new: invariant", Validate({"a", "b"}, {}, r) Is Nothing, Validate({"a", "b"}, {}, r))

        ' Nothing 输入
        r = diff.Compare(Nothing, {"x"})

        Check("nothing old", r.InsertCount = 1, $"ins={r.InsertCount}")

        ' 完全相同
        Dim lines = {"a", "b", "c", "d", "e"}

        r = diff.Compare(lines, lines)

        Check("identical: all equal", r.EqualCount = 5 AndAlso r.InsertCount = 0 AndAlso r.DeleteCount = 0, $"eq={r.EqualCount}")
        Check("identical: similarity 1", Math.Abs(r.Similarity - 1.0) < 0.000001, $"sim={r.Similarity}")
        Check("identical: no diff output", r.ToUnifiedDiff("a", "b") = "", "unified diff should be empty")
    End Sub

    Private Sub TestBasicEdits()
        Console.WriteLine("── 基本编辑用例")

        Dim diff As New MyersDiff()

        ' 头部插入
        Dim r = diff.Compare({"b", "c"}, {"a", "b", "c"})

        Check("insert head: invariant", Validate({"b", "c"}, {"a", "b", "c"}, r) Is Nothing)
        Check("insert head: 1 insert", r.InsertCount = 1 AndAlso r.DeleteCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")

        ' 中部插入
        r = diff.Compare({"a", "c"}, {"a", "b", "c"})

        Check("insert middle: invariant", Validate({"a", "c"}, {"a", "b", "c"}, r) Is Nothing)
        Check("insert middle: 1 insert", r.InsertCount = 1 AndAlso r.DeleteCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")

        ' 尾部插入
        r = diff.Compare({"a", "b"}, {"a", "b", "c"})

        Check("insert tail: invariant", Validate({"a", "b"}, {"a", "b", "c"}, r) Is Nothing)

        ' 删除
        r = diff.Compare({"a", "b", "c"}, {"a", "c"})

        Check("delete middle: invariant", Validate({"a", "b", "c"}, {"a", "c"}, r) Is Nothing)
        Check("delete middle: 1 delete", r.DeleteCount = 1 AndAlso r.InsertCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")

        ' 替换（1 删 1 插）
        r = diff.Compare({"a", "b", "c"}, {"a", "x", "c"})

        Check("replace: invariant", Validate({"a", "b", "c"}, {"a", "x", "c"}, r) Is Nothing)
        Check("replace: 1+1", r.DeleteCount = 1 AndAlso r.InsertCount = 1, $"ins={r.InsertCount} del={r.DeleteCount}")

        ' 重复元素
        r = diff.Compare({"a", "a", "b"}, {"a", "b"})

        Check("duplicate: invariant", Validate({"a", "a", "b"}, {"a", "b"}, r) Is Nothing)
        Check("duplicate: 1 delete", r.DeleteCount = 1, $"del={r.DeleteCount}")

        ' 顺序颠倒（应为 2 删 2 插）
        r = diff.Compare({"a", "b"}, {"b", "a"})

        Check("swap: invariant", Validate({"a", "b"}, {"b", "a"}, r) Is Nothing)
        Check("swap: distance 2", r.DeleteCount + r.InsertCount = 2, $"d={r.DeleteCount + r.InsertCount}")

        ' 长公共前后缀
        Dim head = New List(Of String)()

        For i As Integer = 0 To 499
            head.Add($"head line {i}")
        Next
        For i As Integer = 0 To 499
            head.Add($"tail line {i}")
        Next

        Dim aLines = head.ToArray()
        Dim bLines = New List(Of String)(head)

        bLines.Insert(500, "new line")

        r = diff.Compare(aLines, bLines.ToArray())

        Check("long prefix/suffix: invariant", Validate(aLines, bLines.ToArray(), r) Is Nothing)
        Check("long prefix/suffix: 1 insert", r.InsertCount = 1 AndAlso r.DeleteCount = 0, $"ins={r.InsertCount} del={r.DeleteCount}")
        Check("long prefix/suffix: similarity", r.Similarity > 0.999, $"sim={r.Similarity}")
    End Sub

    Private Sub TestCharDiff()
        Console.WriteLine("── 字符级比较")

        Dim diff As New MyersDiff()

        ' 空字符串（旧实现会构造 New String(-1) 抛异常）
        Dim r = diff.CompareChars("", "abc")

        Check("char empty old: 3 inserts", r.InsertCount = 3, $"ins={r.InsertCount}")
        Check("char empty old: invariant", Validate({}, {"a", "b", "c"}, r) Is Nothing)

        r = diff.CompareChars("abc", "")

        Check("char empty new: 3 deletes", r.DeleteCount = 3, $"del={r.DeleteCount}")

        r = diff.CompareChars("", "")

        Check("char both empty", r.Items.Count = 0, $"items={r.Items.Count}")

        r = diff.CompareChars("abc", "abc")

        Check("char identical", r.EqualCount = 3, $"eq={r.EqualCount}")

        r = diff.CompareChars("kitten", "sitting")

        Check("char kitten/sitting: distance", r.DeleteCount + r.InsertCount = 5, $"d={r.DeleteCount + r.InsertCount}")

        ' 重放字符级结果
        Dim sb As New StringBuilder()

        For Each item In r.Items
            If item.Type <> EditType.Delete Then sb.Append(item.Value)
        Next

        Check("char replay new", sb.ToString() = "sitting", sb.ToString())
    End Sub

    Private Sub TestUnifiedDiff()
        Console.WriteLine("── 统一差异格式")

        Dim diff As New MyersDiff()
        Dim oldLines = {"Imports System", "", "Public Class A", "    Dim x As Integer", "End Class"}
        Dim newLines = {"Imports System", "Imports System.Text", "", "Public Class A", "    Dim y As String", "End Class"}
        Dim r = diff.Compare(oldLines, newLines)
        Dim text As String = r.ToUnifiedDiff("a.vb", "b.vb", 1)

        Check("unified: has header", text.StartsWith("--- a.vb") AndAlso text.Contains("+++ b.vb"), text)

        ' 回放统一差异：应用 patch 到旧序列应当得到新序列
        Dim patched As New List(Of String)()
        Dim oldPtr As Integer = 0

        For Each line In text.Replace(vbCrLf, vbLf).Split(vbLf)
            If line.Length = 0 Then Continue For
            If line.StartsWith("---") OrElse line.StartsWith("+++") OrElse line.StartsWith("@@") Then Continue For

            Dim tag As Char = line(0)
            Dim body As String = line.Substring(1)

            Select Case tag
                Case " "c
                    patched.Add(body)
                    oldPtr += 1
                Case "-"c
                    oldPtr += 1
                Case "+"c
                    patched.Add(body)
                Case Else
                    ' 忽略
            End Select
        Next

        ' 统一差异只输出变更块（带上下文），所以只校验块内的相对一致性：
        ' patch 结果必须是新序列的一个子序列片段校验 —— 这里改为校验行数统计
        Dim blockLines As Integer = 0

        For Each line In text.Replace(vbCrLf, vbLf).Split(vbLf)
            If line.Length > 0 AndAlso (line(0) = " "c OrElse line(0) = "-"c OrElse line(0) = "+"c) Then
                blockLines += 1
            End If
        Next

        Check("unified: block emitted", blockLines > 0, $"lines={blockLines}")

        ' 上下文为 3 且改动相邻时，整个文件应被输出（含全部行）
        Dim full As String = r.ToUnifiedDiff("a.vb", "b.vb", 100)
        Dim patched2 As New List(Of String)()

        For Each line In full.Replace(vbCrLf, vbLf).Split(vbLf)
            If line.Length = 0 Then Continue For
            If line.StartsWith("---") OrElse line.StartsWith("+++") OrElse line.StartsWith("@@") Then Continue For

            Dim tag As Char = line(0)

            If tag = " "c Then
                patched2.Add(line.Substring(1))
            ElseIf tag = "+"c Then
                patched2.Add(line.Substring(1))
            End If
        Next

        Check("unified: full context replay", SeqEqual(patched2.ToArray(), newLines), String.Join("|", patched2))
    End Sub

    ' ------------------------------------------------------------------
    ' 随机对拍：结构不变量 + 编辑距离最优性
    ' ------------------------------------------------------------------
    Private Sub FuzzSmall()
        Console.WriteLine("── 随机对拍（小规模）")

        Dim rnd As New Random(20261016)
        Dim diff As New MyersDiff()
        Dim bad As Integer = 0

        For nCase As Integer = 1 To 800
            Dim n As Integer = rnd.Next(0, 9)
            Dim m As Integer = rnd.Next(0, 9)
            Dim a As String() = RandomLines(rnd, n, 3)
            Dim b As String() = RandomLines(rnd, m, 3)
            Dim r = diff.Compare(a, b)
            Dim err As String = Validate(a, b, r)

            If err IsNot Nothing Then
                bad += 1
                If bad <= 3 Then Console.WriteLine($"  [FAIL] case {nCase}: {err}")
                Continue For
            End If

            Dim expected As Integer = a.Length + b.Length - 2 * LCS(a, b)
            Dim actual As Integer = r.DeleteCount + r.InsertCount

            If expected <> actual Then
                bad += 1
                If bad <= 3 Then Console.WriteLine($"  [FAIL] case {nCase}: distance {actual} <> {expected}")
            End If
        Next

        Check("fuzz small: 800 cases", bad = 0, $"bad={bad}")
    End Sub

    Private Sub FuzzMedium()
        Console.WriteLine("── 随机对拍（中等规模）")

        Dim rnd As New Random(99991)
        Dim diff As New MyersDiff()
        Dim bad As Integer = 0

        For nCase As Integer = 1 To 120
            Dim n As Integer = rnd.Next(10, 60)
            Dim m As Integer = rnd.Next(10, 60)
            Dim a As String() = RandomLines(rnd, n, 6)
            Dim b As String() = RandomLines(rnd, m, 6)
            Dim r = diff.Compare(a, b)
            Dim err As String = Validate(a, b, r)

            If err IsNot Nothing Then
                bad += 1
                If bad <= 3 Then Console.WriteLine($"  [FAIL] case {nCase}: {err}")
                Continue For
            End If

            Dim expected As Integer = a.Length + b.Length - 2 * LCS(a, b)
            Dim actual As Integer = r.DeleteCount + r.InsertCount

            If expected <> actual Then
                bad += 1
                If bad <= 3 Then Console.WriteLine($"  [FAIL] case {nCase}: distance {actual} <> {expected}")
            End If
        Next

        Check("fuzz medium: 120 cases", bad = 0, $"bad={bad}")
    End Sub

    Private Function RandomLines(rnd As Random, n As Integer, alphabet As Integer) As String()
        Dim list As String() = New String(n - 1) {}

        For i As Integer = 0 To n - 1
            list(i) = "w" & rnd.Next(0, alphabet)
        Next

        Return list
    End Function

    Private Sub TestMaxEditDistance()
        Console.WriteLine("── 编辑距离上界")

        Dim diff As New MyersDiff() With {.MaxEditDistance = 10}
        Dim a As String() = New String(19) {}
        Dim b As String() = New String(19) {}

        For i As Integer = 0 To 19
            a(i) = "a" & i
            b(i) = "b" & i
        Next

        Dim r = diff.Compare(a, b)

        Check("cap: truncated flag", r.Truncated, "should be truncated")
        Check("cap: invariant still holds", Validate(a, b, r) Is Nothing, Validate(a, b, r))

        ' 上界之内则不应退化
        diff.MaxEditDistance = 5000

        r = diff.Compare({"a", "b"}, {"a", "x"})

        Check("cap: not truncated", Not r.Truncated, "should not be truncated")
    End Sub

    Private Sub TestPerformance()
        Console.WriteLine("── 性能")

        Dim diff As New MyersDiff()
        Dim watch As New Stopwatch()

        ' 1) 完全相同的大文件（公共前后缀裁剪）
        Dim big As String() = New String(20000 - 1) {}

        For i As Integer = 0 To big.Length - 1
            big(i) = $"line {i}: the quick brown fox jumps over the lazy dog"
        Next

        watch.Start()
        Dim r = diff.Compare(big, big)
        watch.Stop()

        Console.WriteLine($"   20000 行完全相同: {watch.ElapsedMilliseconds} ms, equal={r.EqualCount}")
        Check("perf identical < 1000ms", watch.ElapsedMilliseconds < 1000, $"{watch.ElapsedMilliseconds}ms")

        ' 2) 大文件 + 少量改动
        Dim touched As String() = CType(big.Clone(), String())
        Dim rnd As New Random(7)

        For k As Integer = 1 To 50
            touched(rnd.Next(0, touched.Length)) = "modified line " & k
        Next

        watch.Restart()
        r = diff.Compare(big, touched)
        watch.Stop()

        Console.WriteLine($"   20000 行 + 50 处改动: {watch.ElapsedMilliseconds} ms, +{r.InsertCount} -{r.DeleteCount}")
        Check("perf big+small edit < 3000ms", watch.ElapsedMilliseconds < 3000, $"{watch.ElapsedMilliseconds}ms")
        Check("perf big+small edit: invariant", Validate(big, touched, r) Is Nothing)

        ' 3) 完全不相关的两个文件（编辑距离 = N + M）
        Dim x As String() = New String(1200 - 1) {}
        Dim y As String() = New String(1200 - 1) {}

        For i As Integer = 0 To x.Length - 1
            x(i) = "x" & i
            y(i) = "y" & i
        Next

        watch.Restart()
        r = diff.Compare(x, y)
        watch.Stop()

        Console.WriteLine($"   1200 x 1200 完全不相关: {watch.ElapsedMilliseconds} ms, d={r.DeleteCount + r.InsertCount}")
        Check("perf disjoint < 5000ms", watch.ElapsedMilliseconds < 5000, $"{watch.ElapsedMilliseconds}ms")
        Check("perf disjoint: invariant", Validate(x, y, r) Is Nothing)

        ' 4) 字符级：长文本
        Dim s1 As String = New String("a"c, 4000)
        Dim s2 As String = New String("a"c, 4000)

        s2 = s2.Remove(2000, 1).Insert(2000, "b")

        watch.Restart()
        Dim rc = diff.CompareChars(s1, s2)
        watch.Stop()

        Console.WriteLine($"   4000 字符比较: {watch.ElapsedMilliseconds} ms, +{rc.InsertCount} -{rc.DeleteCount}")
        Check("perf chars < 3000ms", watch.ElapsedMilliseconds < 3000, $"{watch.ElapsedMilliseconds}ms")
    End Sub
End Module
