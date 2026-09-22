Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Script

    ''' <summary>
    ''' 脚本所声明类型的**成员表**: 类型简单名 → 成员名 → 成员类型。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>@</c> 数组投影运算符必须知道「<c>list@x</c> 投影出来的元素是什么类型」,
    ''' 才能把结果继续送进 SIMD 向量化。这需要先知道 <c>list</c> 的元素类型
    ''' (来自 <see cref="ValueTypeInfo.ElementName"/>), 再在该类型的成员表里查到 <c>x</c> 的声明类型 ——
    ''' 本类型负责后者。
    ''' </para>
    ''' <para>
    ''' 数据源是 <see cref="ScriptStructure.TypeBlocks"/> 给出的类型定义块原文
    ''' (主脚本自身)以及被 <c>#include</c> 引入脚本所贡献的类型块
    ''' (<see cref="IncludeSet.TypeBlocks"/>)。块内容用 Roslyn 解析,
    ''' 因此对成员修饰符顺序、单行属性(<c>Property x As Double</c>)、
    ''' 带 <c>Get</c>/<c>Set</c> 的属性块(<c>Property BlockSyntax</c>)以及字段声明
    ''' (<c>Dim</c>/<c>Public</c>/<c>Private</c> …)都能正确处理, 不需要自己写正则。
    ''' </para>
    ''' <para>
    ''' <b>保守性</b>: 只登记「带 <c>As</c> 类型子句」的成员 —— 没有类型子句时无法推断投影结果的类型,
    ''' 直接不登记(于是 <c>@</c> 不会展开, 而不是猜一个类型出来)。
    ''' 同名成员被声明为不同类型时, 该成员会被剔除(宁可不展开, 也不选错)。
    ''' </para>
    ''' </remarks>
    Public Class ObjectMemberTable

        ''' <summary>类型简单名 → (成员名 → 成员类型)</summary>
        Private ReadOnly _members As New Dictionary(Of String, Dictionary(Of String, ValueTypeInfo))(StringComparer.OrdinalIgnoreCase)

        ''' <summary>同名成员声明冲突的类型 → 该类型的这些成员被剔除</summary>
        Private ReadOnly _conflicted As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 已登记的类型简单名(供 <c>--verbose</c> 诊断: 可以据此判断某个类型为什么没参与 <c>@</c> 展开)
        ''' </summary>
        Public ReadOnly Property TypeNames As String()
            Get
                Return _members.Keys.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 从若干类型定义块原文构建成员表; 块内容不是类型定义(或解析失败)时自动忽略。
        ''' </summary>
        Public Shared Function FromTypeBlocks(blocks As IEnumerable(Of String)) As ObjectMemberTable
            Dim table As New ObjectMemberTable()

            If blocks IsNot Nothing Then
                For Each block As String In blocks
                    Call table.AddTypeBlock(block)
                Next
            End If

            Return table
        End Function

        ''' <summary>
        ''' 查询某个类型的成员类型; 类型或成员不存在(或该成员声明冲突)时返回 <c>False</c>。
        ''' </summary>
        Public Function TryGetMember(typeName As String, memberName As String, ByRef info As ValueTypeInfo) As Boolean
            info = New ValueTypeInfo()

            If String.IsNullOrEmpty(typeName) OrElse String.IsNullOrEmpty(memberName) Then
                Return False
            End If

            Dim simple As String = VectorType.SimpleTypeName(typeName)
            Dim members As Dictionary(Of String, ValueTypeInfo) = Nothing

            If Not _members.TryGetValue(simple, members) Then
                Return False
            End If

            Dim conflicts As HashSet(Of String) = Nothing

            If _conflicted.TryGetValue(simple, conflicts) AndAlso conflicts.Contains(memberName) Then
                Return False
            End If

            Return members.TryGetValue(memberName, info)
        End Function

        ' ==================================================================
        ' 类型定义块解析
        ' ==================================================================

        Private Sub AddTypeBlock(block As String)
            If String.IsNullOrWhiteSpace(block) Then
                Return
            End If

            Dim tree As SyntaxTree

            Try
                tree = VisualBasicSyntaxTree.ParseText(block)
            Catch ex As Exception
                Return
            End Try

            For Each typeBlock As TypeBlockSyntax In tree.GetRoot().DescendantNodes().OfType(Of TypeBlockSyntax)()
                Dim typeName As String = VectorType.SimpleTypeName(typeBlock.BlockStatement.Identifier.ValueText)

                If typeName Is Nothing Then
                    Continue For
                End If

                Call AddMembers(typeName, typeBlock.Members)
            Next
        End Sub

        Private Sub AddMembers(typeName As String, members As SyntaxList(Of StatementSyntax))
            For Each member As StatementSyntax In members
                ' ---- 单行属性: Property x As Double ----
                Dim [property] As PropertyStatementSyntax = TryCast(member, PropertyStatementSyntax)

                If [property] IsNot Nothing Then
                    Call AddMember(typeName, [property].Identifier.ValueText, [property].AsClause)
                    Continue For
                End If

                ' ---- 带 Get/Set 的属性块 ----
                Dim propertyBlock As PropertyBlockSyntax = TryCast(member, PropertyBlockSyntax)

                If propertyBlock IsNot Nothing AndAlso propertyBlock.PropertyStatement IsNot Nothing Then
                    Call AddMember(typeName,
                                   propertyBlock.PropertyStatement.Identifier.ValueText,
                                   propertyBlock.PropertyStatement.AsClause)
                    Continue For
                End If

                ' ---- 字段声明: Public x As Double / Dim a, b As Integer ----
                Dim field As FieldDeclarationSyntax = TryCast(member, FieldDeclarationSyntax)

                If field IsNot Nothing Then
                    For Each declarator As VariableDeclaratorSyntax In field.Declarators
                        For Each name As ModifiedIdentifierSyntax In declarator.Names
                            Call AddMember(typeName, name.Identifier.ValueText, declarator.AsClause)
                        Next
                    Next
                End If
            Next
        End Sub

        Private Sub AddMember(typeName As String, memberName As String, asClause As AsClauseSyntax)
            If String.IsNullOrEmpty(memberName) Then
                Return
            End If

            Dim info As ValueTypeInfo = ResolveAsClause(asClause)

            If Not info.IsKnown Then
                ' 没有类型子句 / 类型无法识别 => 不登记(投影结果类型不可知, 于是不会展开)
                Return
            End If

            Dim members As Dictionary(Of String, ValueTypeInfo) = Nothing

            If Not _members.TryGetValue(typeName, members) Then
                members = New Dictionary(Of String, ValueTypeInfo)(StringComparer.OrdinalIgnoreCase)
                _members(typeName) = members
            End If

            Dim existing As ValueTypeInfo = Nothing

            If members.TryGetValue(memberName, existing) Then
                If Not String.Equals(existing.ToString(), info.ToString(), StringComparison.OrdinalIgnoreCase) Then
                    ' 同名成员被声明为不同类型: 剔除该成员, 避免选错
                    Call members.Remove(memberName)
                    Call MarkConflict(typeName, memberName)
                End If

                Return
            End If

            Dim conflicts As HashSet(Of String) = Nothing

            If _conflicted.TryGetValue(typeName, conflicts) AndAlso conflicts.Contains(memberName) Then
                Return
            End If

            members(memberName) = info
        End Sub

        Private Sub MarkConflict(typeName As String, memberName As String)
            Dim conflicts As HashSet(Of String) = Nothing

            If Not _conflicted.TryGetValue(typeName, conflicts) Then
                conflicts = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                _conflicted(typeName) = conflicts
            End If

            Call conflicts.Add(memberName)
        End Sub

        ' ==================================================================
        ' 类型语法 → 浅层类型(与改写器共用同一套规则)
        ' ==================================================================

        ''' <summary>
        ''' 解析一个类型语法为浅层类型: 只接受一维数组与标量。
        ''' 数值类型给出 <see cref="NumericKind"/>, 其余具名类型给出
        ''' <see cref="ValueTypeInfo.ElementName"/>。
        ''' </summary>
        Public Shared Function ResolveType(type As TypeSyntax) As ValueTypeInfo
            If type Is Nothing Then
                Return New ValueTypeInfo()
            End If

            Dim array As ArrayTypeSyntax = TryCast(type, ArrayTypeSyntax)

            If array IsNot Nothing Then
                ' 只支持一维数组; 交错数组与多维数组一律放弃
                If array.RankSpecifiers.Count <> 1 OrElse array.RankSpecifiers(0).Rank <> 1 Then
                    Return New ValueTypeInfo()
                End If

                Return ResolveElementType(array.ElementType, isVector:=True)
            End If

            Return ResolveElementType(type, isVector:=False)
        End Function

        ''' <summary>解析 <c>As ...</c> 子句(含 <c>As New ...</c>)给出的类型</summary>
        Public Shared Function ResolveAsClause(asClause As AsClauseSyntax) As ValueTypeInfo
            Dim simple As SimpleAsClauseSyntax = TryCast(asClause, SimpleAsClauseSyntax)

            If simple IsNot Nothing Then
                Return ResolveType(simple.Type)
            End If

            Dim asNew As AsNewClauseSyntax = TryCast(asClause, AsNewClauseSyntax)

            If asNew IsNot Nothing AndAlso asNew.NewExpression IsNot Nothing Then
                Return ResolveType(asNew.NewExpression.Type)
            End If

            Return New ValueTypeInfo()
        End Function

        Private Shared Function ResolveElementType(type As TypeSyntax, isVector As Boolean) As ValueTypeInfo
            Return ParseTypeText(type.ToString(), isVector)
        End Function

        ''' <summary>
        ''' 由类型名**文本**解析浅层类型。
        ''' </summary>
        ''' <remarks>
        ''' 文本入口存在的意义: 语法不完整的声明行(典型是跨行的数组字面量
        ''' <c>Dim list As Foo() = {</c>)无法被 Roslyn 解析成语句, 但仍然可以靠正则取出
        ''' <c>As</c> 子句的文本, 从而登记变量类型 —— 否则后续行上的 <c>@</c> 投影
        ''' 会因为 <see cref="ValueTypeInfo.ElementName"/> 未知而无法展开。
        ''' </remarks>
        Public Shared Function ParseTypeText(raw As String, isVector As Boolean) As ValueTypeInfo
            If String.IsNullOrEmpty(raw) Then
                Return New ValueTypeInfo()
            End If

            ' 去掉类型名内部的空白: CLRObjectType ( ) -> CLRObjectType()
            Dim text As String = System.Text.RegularExpressions.Regex.Replace(raw, "\s", "")
            Dim kind As NumericKind

            If VectorType.TryParseKind(text, kind) Then
                Return New ValueTypeInfo(kind, isVector)
            End If

            Dim name As String = VectorType.SimpleTypeName(text)

            If name Is Nothing Then
                ' 匿名类型/元组类型等无法命名 => 只保留"是数组"这一事实
                Return New ValueTypeInfo(NumericKind.Unknown, isVector)
            End If

            Return New ValueTypeInfo(NumericKind.Unknown, isVector, name)
        End Function
    End Class
End Namespace
