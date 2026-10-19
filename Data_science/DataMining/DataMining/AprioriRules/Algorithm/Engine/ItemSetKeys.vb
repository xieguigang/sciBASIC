Option Strict On

Namespace AprioriRules.Engine

    ''' <summary>
    ''' 引擎内部的项集一律表示为「内部 id 升序排列的 <see cref="Integer"/> 数组」，
    ''' 因此放进字典或者哈希集合的时候必须配套使用这个结构化比较器，
    ''' 否则会退化成引用比较（同一个项集的两份副本会被当成两个不同的键）。
    ''' </summary>
    ''' <remarks>
    ''' 哈希值使用 <see cref="System.HashCode"/> 计算：它内部是无检查算术，
    ''' 不会像手写的 ``31 * hash + x`` 那样在 VB 默认开启的整数溢出检查下
    ''' （本工程的 Debug/Release|AnyCPU 都没有设置 RemoveIntegerChecks）抛出 OverflowException。
    ''' </remarks>
    Friend Class IntSetComparer : Implements IEqualityComparer(Of Integer())

        ''' <summary>
        ''' 比较器是无状态的，全局共用一份即可。
        ''' </summary>
        Public Shared ReadOnly Instance As New IntSetComparer()

        Private Sub New()
        End Sub

        Public Overloads Function GetHashCode(a As Integer()) As Integer Implements IEqualityComparer(Of Integer()).GetHashCode
            If a Is Nothing Then
                Return 0
            End If

            Dim code As New System.HashCode()

            For i As Integer = 0 To a.Length - 1
                code.Add(a(i))
            Next

            Return code.ToHashCode()
        End Function

        Public Overloads Function Equals(a As Integer(), b As Integer()) As Boolean Implements IEqualityComparer(Of Integer()).Equals
            If a Is Nothing OrElse b Is Nothing Then
                Return a Is Nothing AndAlso b Is Nothing
            End If

            If a.Length <> b.Length Then
                Return False
            End If

            For i As Integer = 0 To a.Length - 1
                If a(i) <> b(i) Then
                    Return False
                End If
            Next

            Return True
        End Function
    End Class

    ''' <summary>
    ''' 内部项集容器的构造器助手
    ''' </summary>
    Friend Module IntSetKeys

        ''' <summary>
        ''' 项集 -&gt; 支持度 的查找表
        ''' </summary>
        Public Function NewSupportTable() As Dictionary(Of Integer(), Integer)
            Return New Dictionary(Of Integer(), Integer)(IntSetComparer.Instance)
        End Function

        ''' <summary>
        ''' 项集 -&gt; 任意值 的查找表
        ''' </summary>
        Public Function NewSetTable(Of T)() As Dictionary(Of Integer(), T)
            Return New Dictionary(Of Integer(), T)(IntSetComparer.Instance)
        End Function

        ''' <summary>
        ''' 项集集合
        ''' </summary>
        Public Function NewSetHash() As HashSet(Of Integer())
            Return New HashSet(Of Integer())(IntSetComparer.Instance)
        End Function

        ''' <summary>
        ''' 建立长度为 <paramref name="n"/> 的数组（n 为 0 时也能安全工作）
        ''' </summary>
        ''' <remarks>
        ''' 单独的 <see cref="Array.CreateInstance(Type, Integer)"/> 或者 ``New T(-1) {}``
        ''' 在 n 为 0 时都不好用，这里统一包一层。
        ''' </remarks>
        Public Function NewArray(Of T)(n As Integer) As T()
            If n <= 0 Then
                Return New T() {}
            Else
                Return New T(n - 1) {}
            End If
        End Function
    End Module
End Namespace
