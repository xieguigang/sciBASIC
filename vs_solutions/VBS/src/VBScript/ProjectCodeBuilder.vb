#Region "Microsoft.VisualBasic::f9d226cb79ef637e12af3dbda68d9d6c, vs_solutions\VBS\src\VBScript\ProjectCodeBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 477
    '    Code Lines: 293 (61.43%)
    ' Comment Lines: 90 (18.87%)
    '    - Xml Docs: 82.22%
    ' 
    '   Blank Lines: 94 (19.71%)
    '     File Size: 20.80 KB


    '     Class ProjectCodeBuilder
    ' 
    '         Properties: PromotedVariables
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildField, BuildIncludedSources, BuildMagics, BuildProgram, BuildSources
    '                   ReferencesName, SlotKey, ToModuleSignature
    ' 
    '         Sub: ResolveFields
    '         Class ProjectSourceFile
    ' 
    '             Properties: Code, FileName
    ' 
    '         Class Declaration
    ' 
    '             Properties: Index, Line, Names, Slot
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 把一个脚本的静态结构发射为"正规的" VB.NET 工程源代码。
    ''' </summary>
    ''' <remarks>
    ''' 与运行期发射器(<see cref="ScriptRefactor"/>)的区别:
    ''' <list type="bullet">
    ''' <item>入口是标准的 <c>Public Function Main(argv As String()) As Integer</c>,
    ''' 内部再构造 <c>CommandLine</c> 并赋给模块级字段 <c>args</c>;</item>
    ''' <item>顶层函数被还原为模块内真正的 <c>Private Function/Sub</c>
    ''' (不再是 Main 之中的匿名函数), 因此不再需要落位求解;</item>
    ''' <item>被顶层函数捕获的顶层变量被提升为模块级字段, 否则函数无法访问它们;</item>
    ''' <item>魔法方法物化为独立的源码文件; 被 <c>#include</c> 引入的脚本各自成为独立的源码文件。</item>
    ''' </list>
    ''' </remarks>
    Public Class ProjectCodeBuilder

        ''' <summary>工程内的一个源码文件(<see cref="FileName"/> 相对于 <c>src/</c> 目录)</summary>
        Public Class ProjectSourceFile
            Public Property FileName As String
            Public Property Code As String
        End Class

        ''' <summary>命令行的模块级字段名(<c>?args</c> 语法会展开为 <c>args("...")</c>)</summary>
        Public Const CommandLineField As String = "args"

        ''' <summary>声明行: Dim x [As T] [= v] / Const x = v</summary>
        Private ReadOnly DeclLinePattern As New Regex("^\s*(?<kind>dim|const)\s+(?<decl>.+)$", RegexOptions.IgnoreCase)

        ''' <summary>
        ''' 单个声明项: <c>name[(bounds)] [As [New] type] [= init]</c>
        ''' </summary>
        Private ReadOnly DeclaratorPattern As New Regex(
            "^\s*(?<name>[A-Za-z_]\w*)\s*(?<bounds>\([^)]*\))?\s*(?:as\s+(?<isnew>new\s+)?(?<typename>.+?))?\s*(?:=\s*(?<init>.+))?\s*$",
            RegexOptions.IgnoreCase)

        ''' <summary>被顶层函数捕获(或间接依赖)的顶层变量名</summary>
        ReadOnly _promoted As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>提升之后的模块级字段声明行(不含缩进)</summary>
        ReadOnly _fields As New List(Of String)

        ''' <summary>需要从 Main 之中移除的声明语句, 键为 "槽位:语句序号"</summary>
        ReadOnly _removed As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ReadOnly _parse As ScriptParseResult
        ReadOnly _syntax As ScriptStructure

        Public Sub New(parse As ScriptParseResult)
            Me._parse = parse
            Me._syntax = ScriptStructure.Scan(parse.PreprocessedCode)

            Call ResolveFields()
        End Sub

        ''' <summary>该脚本被提升为模块级字段的顶层变量(只读快照)</summary>
        Public ReadOnly Property PromotedVariables As String()
            Get
                Return _promoted.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 生成工程的全部源码文件: 主脚本、魔法方法、以及每一个被 <c>#include</c> 引入的脚本。
        ''' </summary>
        Public Function BuildSources() As IEnumerable(Of ProjectSourceFile)
            Dim files As New List(Of ProjectSourceFile)

            Call files.Add(New ProjectSourceFile With {
                .FileName = "Program.vb",
                .Code = BuildProgram()
            })
            Call files.Add(New ProjectSourceFile With {
                .FileName = "VBScriptHostMagics.vb",
                .Code = BuildMagics()
            })
            Call files.AddRange(BuildIncludedSources())

            Return files
        End Function

        ' ==================================================================
        ' 主脚本代码
        ' ==================================================================

        ''' <summary>生成主脚本对应的模块代码(程序入口 + 字段 + 顶层函数 + 类型定义)</summary>
        Public Function BuildProgram() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("Option Strict Off")
            Call sb.AppendLine("Option Explicit On")
            Call sb.AppendLine("Option Infer On")
            Call sb.AppendLine()

            For Each header As String In _syntax.Headers
                Call sb.AppendLine(header)
            Next

            If _syntax.Headers.Count > 0 Then
                Call sb.AppendLine()
            End If

            For Each line As String In ScriptRefactor.DefaultImports(_parse.Vectorized)
                Call sb.AppendLine(line)
            Next

            Call sb.AppendLine()
            Call sb.AppendLine($"Namespace {NamespaceName}")
            Call sb.AppendLine($"    Module {ModuleName}")
            Call sb.AppendLine()
            Call sb.AppendLine("        ''' <summary>脚本的命令行参数(?args 语法展开之后引用本字段)</summary>")
            Call sb.AppendLine($"        Private {CommandLineField} As CommandLine")

            For Each field As String In _fields
                Call sb.AppendLine("        " & field)
            Next

            Call sb.AppendLine()

            ' 与运行期发射保持一致: 让生成的工程里也可以直接调用 print(...) 调试数据
            Call ScriptRefactor.AppendPrintForwarder(sb)

            Call sb.AppendLine()
            Call sb.AppendLine("        ''' <summary>由脚本顶层的可执行语句生成的程序入口</summary>")
            Call sb.AppendLine("        Public Function Main(argv As String()) As Integer")
            Call sb.AppendLine("            If argv Is Nothing Then")
            Call sb.AppendLine("                argv = New String() {}")
            Call sb.AppendLine("            End If")
            Call sb.AppendLine()
            ' NoSubCommand:=True: 生成的可执行程序没有"子命令"概念, 全部词元都是参数,
            ' 这样脚本之中的 ?"--x" 与以脚本方式运行时保持一致的取值语义。
            Call sb.AppendLine($"            {CommandLineField} = CommandLine.BuildFromArguments(argv, NoSubCommand:=True)")
            Call sb.AppendLine()

            For slot As Integer = 0 To _syntax.Slots.Count - 1
                For i As Integer = 0 To _syntax.Slots(slot).Statements.Count - 1
                    If _removed.Contains(SlotKey(slot, i)) Then
                        Continue For
                    End If

                    Call sb.AppendLine("            " & _syntax.Slots(slot).Statements(i))
                Next
            Next

            Call sb.AppendLine()
            Call sb.AppendLine("            Return 0")
            Call sb.AppendLine("        End Function")

            For Each func As ScriptFunctionBlock In _syntax.Functions
                Call sb.AppendLine()

                For i As Integer = 0 To func.Lines.Count - 1
                    If i = 0 Then
                        Call sb.AppendLine("        " & ToModuleSignature(func.Signature))
                    ElseIf String.IsNullOrWhiteSpace(func.Lines(i)) Then
                        Call sb.AppendLine()
                    Else
                        Call sb.AppendLine("        " & func.Lines(i))
                    End If
                Next
            Next

            Call sb.AppendLine("    End Module")

            For Each typeBlock As String In _syntax.TypeBlocks
                Call sb.AppendLine()

                Dim lines As String() = typeBlock.Split(vbLf)

                For i As Integer = 0 To lines.Length - 1
                    If String.IsNullOrWhiteSpace(lines(i)) Then
                        Call sb.AppendLine()
                    ElseIf i = 0 Then
                        Call sb.AppendLine("    " & ScriptRefactor.NormalizeTypeAccess(lines(i)))
                    Else
                        Call sb.AppendLine("    " & lines(i))
                    End If
                Next
            Next

            Call sb.AppendLine("End Namespace")

            Return sb.ToString()
        End Function

        ''' <summary>生成魔法方法对应的模块代码</summary>
        Public Function BuildMagics() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("Option Strict Off")
            Call sb.AppendLine("Option Explicit On")
            Call sb.AppendLine("Option Infer On")
            Call sb.AppendLine()
            Call sb.AppendLine($"Namespace {NamespaceName}")
            Call sb.AppendLine("    Module VBScriptHostMagics")

            For Each magic As String In Magics.Build(_parse.ScriptFile, _parse.Metadata, _parse.Imports, _parse.SearchRoots)
                Call sb.AppendLine(magic)
            Next

            Call sb.AppendLine("    End Module")
            Call sb.AppendLine("End Namespace")

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 被 <c>#include</c> 引入的脚本各自成为独立的源码文件:
        ''' 文件头仍旧携带自身的 Imports, 类型定义直接位于工程命名空间之中。
        ''' </summary>
        ''' <remarks>
        ''' 这里刻意**关闭**向量化改写: 运行期路径之中被引入脚本的类型定义是原样复制进生成代码的,
        ''' 而工程期会把每个被引入脚本单独写成一个文件 —— 单独的文件只有它自己的 Imports,
        ''' 无法看到 <c>Microsoft.VisualBasic.Math.SIMD</c>。两条路径都保持"被引入脚本不做向量化",
        ''' 才能保证行为一致。
        ''' </remarks>
        Private Function BuildIncludedSources() As IEnumerable(Of ProjectSourceFile)
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim sources As New List(Of ProjectSourceFile)

            For Each script As IncludedScript In _parse.ScriptIncludes
                Dim original As String = Path.GetFileName(script.FilePath)
                Dim fileName As String = original
                Dim index As Integer = 1

                ' 不同目录下的同名脚本需要改名以避免相互覆盖
                While Not used.Add(fileName)
                    index += 1
                    fileName = Path.GetFileNameWithoutExtension(original) & index & ".vb"
                End While

                Call sources.Add(New ProjectSourceFile With {
                    .FileName = fileName,
                    .Code = ScriptRefactor.PreprocessText(File.ReadAllText(script.FilePath), vectorize:=False)
                })
            Next

            Return sources
        End Function

        ' ==================================================================
        ' 捕获变量提升
        ' ==================================================================

        ''' <summary>
        ''' 求解需要提升为模块级字段的顶层变量。
        ''' </summary>
        ''' <remarks>
        ''' 顶层函数在工程之中变成了模块级方法, 因此它所能引用的顶层变量必须是模块级字段,
        ''' 否则作用域不可见。求解过程分三步:
        ''' <list type="number">
        ''' <item>收集全部顶层声明语句及其变量名;</item>
        ''' <item>找出在顶层函数体之中出现过的变量名(即被函数捕获的变量);</item>
        ''' <item>对"被提升变量的初始化表达式"再做一次引用扫描, 把间接依赖的顶层变量一并提升,
        ''' 否则 <c>Private x = 某个局部变量</c> 会出现编译错误。</item>
        ''' </list>
        ''' 脚本没有任何顶层函数时不做任何提升, Main 之中全部保持为局部变量。
        ''' </remarks>
        Private Sub ResolveFields()
            Dim declarations As New List(Of Declaration)
            Dim index As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

            For slot As Integer = 0 To _syntax.Slots.Count - 1
                Dim statements As List(Of String) = _syntax.Slots(slot).Statements

                For i As Integer = 0 To statements.Count - 1
                    Dim names As String() = ScriptStructure.DeclaredNamesOf(statements(i))

                    If names.Length = 0 Then
                        Continue For
                    End If

                    Dim decl As New Declaration With {
                        .Slot = slot,
                        .Index = i,
                        .Line = statements(i)
                    }

                    Call decl.Names.AddRange(names)

                    For Each name As String In names
                        If Not index.ContainsKey(name) Then
                            index(name) = declarations.Count
                        End If
                    Next

                    Call declarations.Add(decl)
                Next
            Next

            If declarations.Count = 0 OrElse _syntax.Functions.Count = 0 Then
                Return
            End If

            ' 2. 被顶层函数捕获的变量
            Dim pending As New Queue(Of String)

            For Each func As ScriptFunctionBlock In _syntax.Functions
                For Each name As String In index.Keys
                    If ReferencesName(func.Text, name) AndAlso _promoted.Add(name) Then
                        Call pending.Enqueue(name)
                    End If
                Next
            Next

            ' 3. 传递闭包: 被提升变量的声明语句所引用的其它顶层变量
            While pending.Count > 0
                Dim name As String = pending.Dequeue()
                Dim decl As Declaration = declarations(index(name))

                For Each other As String In index.Keys
                    If String.Equals(other, name, StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    If ReferencesName(decl.Line, other) AndAlso _promoted.Add(other) Then
                        Call pending.Enqueue(other)
                    End If
                Next
            End While

            ' 4. 生成字段声明, 并把对应的声明语句从 Main 之中移除
            For Each decl As Declaration In declarations
                If Not decl.Names.Any(Function(n) _promoted.Contains(n)) Then
                    Continue For
                End If

                For Each name As String In decl.Names
                    Dim field As String = BuildField(name, decl.Line)

                    If field IsNot Nothing Then
                        Call _fields.Add(field)
                    End If
                Next

                Call _removed.Add(SlotKey(decl.Slot, decl.Index))
            Next
        End Sub

        ''' <summary>一个顶层声明语句及其声明的变量名</summary>
        Private Class Declaration
            Public Property Slot As Integer
            Public Property Index As Integer
            Public Property Line As String
            Public ReadOnly Property Names As New List(Of String)
        End Class

        Private Shared Function SlotKey(slot As Integer, index As Integer) As String
            Return slot & ":" & index
        End Function

        ''' <summary>判断一段代码之中是否以"词"的形式引用了给定的标识符</summary>
        Private Shared Function ReferencesName(code As String, name As String) As Boolean
            If String.IsNullOrEmpty(name) Then
                Return False
            End If

            Return Regex.IsMatch(code, "\b" & Regex.Escape(name) & "\b", RegexOptions.IgnoreCase)
        End Function

        ''' <summary>
        ''' 把一条顶层声明语句之中的某个变量转换为模块级字段声明。
        ''' </summary>
        ''' <remarks>
        ''' 转换规则(保持类型推断与初始值):
        ''' <code>
        ''' Dim x As Integer = 1   =>  Private x As Integer = 1
        ''' Dim x = 1              =>  Private x = 1
        ''' Dim x As New Foo()     =>  Private x = New Foo()
        ''' Dim x As Integer       =>  Private x As Integer
        ''' Const x As Integer = 1 =>  Private Const x As Integer = 1
        ''' Dim a(10) As Integer   =>  Private a(10) As Integer
        ''' </code>
        ''' </remarks>
        Private Function BuildField(name As String, statement As String) As String
            Dim line As Match = DeclLinePattern.Match(statement)

            If Not line.Success Then
                Return Nothing
            End If

            Dim isConst As Boolean = line.Groups("kind").Value.Equals("const", StringComparison.OrdinalIgnoreCase)
            Dim declarators As String() = ScriptStructure.SplitTopLevel(line.Groups("decl").Value)
            Dim sharedType As String = Nothing

            For Each part As String In declarators
                Dim probe As Match = DeclaratorPattern.Match(part)

                If probe.Success AndAlso Not String.IsNullOrEmpty(probe.Groups("typename").Value) Then
                    sharedType = probe.Groups("typename").Value.Trim()
                    Exit For
                End If
            Next

            For Each part As String In declarators
                Dim m As Match = DeclaratorPattern.Match(part)

                If Not m.Success Then
                    Continue For
                End If

                If Not String.Equals(m.Groups("name").Value, name, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim bounds As String = m.Groups("bounds").Value
                Dim typename As String = m.Groups("typename").Value.Trim()
                Dim init As String = m.Groups("init").Value.Trim()

                If m.Groups("isnew").Success Then
                    ' Dim x As New Foo() => Private x = New Foo()
                    Return $"Private {name}{bounds} = New {typename}"
                End If

                If typename.Length = 0 AndAlso init.Length = 0 AndAlso sharedType IsNot Nothing Then
                    ' Dim a, b As Integer 之中的 a
                    typename = sharedType
                End If

                Dim sb As New StringBuilder()

                Call sb.Append(If(isConst, "Private Const ", "Private ")).Append(name).Append(bounds)

                If typename.Length > 0 Then
                    Call sb.Append(" As ").Append(typename)
                End If

                If init.Length = 0 Then
                    If typename.Length = 0 Then
                        ' 既没有类型也没有初始值, 只能退化为 Object
                        Call sb.Append(" As Object")
                    End If
                Else
                    Call sb.Append(" = ").Append(init)
                End If

                Return sb.ToString()
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 把顶层函数的原始签名改写为工程内的模块级私有方法签名:
        '''   Public Function HelloWorld As String => Private Function HelloWorld() As String
        '''   Public Sub Foo(a As Integer)         => Private Sub Foo(a As Integer)
        ''' </summary>
        Friend Shared Function ToModuleSignature(signature As String) As String
            Dim m As Match = Regex.Match(signature,
                "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+(?<name>[A-Za-z_]\w*)\s*(?<params>\(.*\))?\s*(?<ret>as\s+.+?)?\s*$",
                RegexOptions.IgnoreCase)

            If Not m.Success Then
                Return "Private " & signature
            End If

            Dim name As String = m.Groups("name").Value
            Dim params As String = m.Groups("params").Value
            Dim ret As String = m.Groups("ret").Value

            If String.IsNullOrEmpty(params) Then
                params = "()"
            End If

            If m.Groups("kind").Value.Equals("sub", StringComparison.OrdinalIgnoreCase) Then
                Return $"Private Sub {name}{params}"
            End If

            Return $"Private Function {name}{params} {ret}".Trim()
        End Function
    End Class
End Namespace

