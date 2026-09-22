Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet

Namespace Script

    ''' <summary>
    ''' <c>#include</c> 预编译指令解析器。
    ''' </summary>
    ''' <remarks>
    ''' 支持三类引用目标, 判别顺序为:
    ''' <list type="number">
    ''' <item>按与历史版本一致的搜索顺序(声明该指令的脚本所在目录、<c>App.HOME</c>、
    ''' <c>App.HOME/libs</c>、<c>App.HOME</c> 上级的 <c>libs/</c>)尝试解析为本地文件:
    ''' 扩展名 <c>.vb</c> 视为<b>脚本引用</b>, 其它视为<b>程序集引用</b>;</item>
    ''' <item>本地文件不存在且目标形如 <c>包id</c> 或 <c>包id@版本</c>(不含路径分隔符,
    ''' 且不以程序集/脚本扩展名结尾)时, 视为 <b>nuget 引用</b>;</item>
    ''' <item>仍然无法解析时保持历史宽松语义: 保留原始目标并记录告警。</item>
    ''' </list>
    ''' 被引入的脚本会递归展开(同样支持其自身的 <c>#include</c>), 并接受合法性校验:
    ''' 不得包含元数据指令、魔法方法调用、顶层函数与顶层可执行语句 —— 因为它们的代码
    ''' 是直接复制到主脚本的顶层命名空间之中的。
    ''' </remarks>
    Public Class IncludeResolver

        ''' <summary>
        ''' <c>#include "target"</c> 指令的文本模式。
        ''' 与预处理阶段移除指令行所用的模式保持一致(整行匹配), 因此注释之中的
        ''' <c>#include</c> 示例不会被当作真实指令。
        ''' </summary>
        Public Const DirectivePattern As String = "^\s*#include\s+""(?<target>[^""]+)""\s*$"

        ''' <summary>脚本引擎自身所面向的目标框架(与 VBS.vbproj 保持一致)</summary>
        Public Const ScriptTargetFramework As String = "net10.0"

        ''' <summary>
        ''' 被引入脚本中不允许出现的魔法方法名(魔法方法只允许出现在主脚本中)。
        ''' </summary>
        Private Shared ReadOnly MagicNames As String() = {
            "ScriptDir", "ScriptFile", "ScriptName", "Here", "ScriptText", "ScriptLines",
            "Self", "Package", "Author", "Title", "Version", "Meta", "Includes", "Locate"
        }

        ''' <summary>被视为"程序集/脚本"的扩展名(此类目标不会被当作 nuget 包 id)</summary>
        Private Shared ReadOnly FileLikeExtensions As String() = {".dll", ".exe", ".winmd", ".vb", ".so", ".dylib", ".nupkg"}

        ReadOnly _searchRoots As String()
        ReadOnly _verbose As Boolean

        ' ---- 解析结果 ----
        ReadOnly _set As New IncludeSet()
        ' 已经解析过的本地文件(大小写不敏感去重)
        ReadOnly _files As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ' 已经展开过的脚本绝对路径
        ReadOnly _scripts As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ' 当前展开栈(用于检测循环引用)
        ReadOnly _expanding As New List(Of String)
        ' 已经解析过的 nuget 包(id@version)
        ReadOnly _packages As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>创建一个 <c>#include</c> 解析器</summary>
        ''' <param name="searchRoots">相对路径搜索目录(按优先级排列, 第一项为声明指令的脚本所在目录)</param>
        ''' <param name="verbose">是否输出解析细节</param>
        Public Sub New(searchRoots As IEnumerable(Of String), Optional verbose As Boolean = False)
            Me._searchRoots = searchRoots.Where(Function(dir) Not String.IsNullOrEmpty(dir)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            Me._verbose = verbose
        End Sub

        ''' <summary>
        ''' 解析一段脚本源代码之中出现的全部 <c>#include</c> 指令(含被引入脚本的递归展开)。
        ''' </summary>
        ''' <param name="source">主脚本源代码文本</param>
        ''' <param name="scriptFile">主脚本文件绝对路径</param>
        Public Function Resolve(source As String, scriptFile As String) As IncludeSet
            Dim full As String = Path.GetFullPath(scriptFile)

            Call ResolveSource(source, full)
            Call ApplyNativeLibraryPath()

            Return _set
        End Function

        ''' <summary>解析一段脚本源代码之中的 <c>#include</c> 指令(不递归展开脚本)</summary>
        Private Sub ResolveSource(source As String, declaredIn As String)
            For Each m As Match In Regex.Matches(source, DirectivePattern, RegexOptions.IgnoreCase Or RegexOptions.Multiline)
                Dim raw As String = m.Groups("target").Value.Trim()

                If raw.Length = 0 Then
                    Continue For
                End If

                Call ResolveDirective(raw, declaredIn)
            Next
        End Sub

        Private Sub ResolveDirective(raw As String, declaredIn As String)
            Dim declaredDir As String = Path.GetDirectoryName(declaredIn)
            Dim local As String = Locate(raw, declaredDir)

            If local IsNot Nothing Then
                If String.Equals(Path.GetExtension(local), ".vb", StringComparison.OrdinalIgnoreCase) Then
                    Call IncludeScript(local, declaredIn)
                Else
                    Call AddAssembly(local)
                End If

                Return
            End If

            ' 绝对路径历史上不做存在性检查, 直接交给编译器报错, 这里保持同样的行为
            If Path.IsPathRooted(raw) Then
                Call AddAssembly(raw)
                Return
            End If

            Dim packageId As String = Nothing
            Dim version As String = Nothing

            If TryParseNuGet(raw, packageId, version) Then
                Call IncludeNuGet(packageId, version, declaredIn)
                Return
            End If

            Call _set.Unresolved.Add(raw)
            Call _set.Warnings.Add($"#include 无法解析的目标: ""{raw}"" (声明于 {declaredIn})")
        End Sub

        ''' <summary>
        ''' 按与历史版本一致的顺序把目标解析为本地文件绝对路径; 找不到时返回 Nothing。
        ''' </summary>
        Private Function Locate(raw As String, declaredDir As String) As String
            If Path.IsPathRooted(raw) Then
                Return If(File.Exists(raw), Path.GetFullPath(raw), Nothing)
            End If

            ' 声明该指令的脚本所在目录拥有最高优先级, 之后是与历史版本一致的引擎目录
            For Each dir As String In MergeRoots(declaredDir)
                Dim candidate As String = Path.GetFullPath(Path.Combine(dir, raw))

                If File.Exists(candidate) Then
                    Return candidate
                End If
            Next

            Return Nothing
        End Function

        Private Function MergeRoots(declaredDir As String) As IEnumerable(Of String)
            Dim roots As New List(Of String)

            If Not String.IsNullOrEmpty(declaredDir) Then
                Call roots.Add(declaredDir)
            End If

            For Each dir As String In _searchRoots
                If Not String.IsNullOrEmpty(dir) AndAlso
                    Not roots.Any(Function(p) String.Equals(p, dir, StringComparison.OrdinalIgnoreCase)) Then

                    Call roots.Add(dir)
                End If
            Next

            Return roots
        End Function

        Private Sub AddAssembly(path As String)
            If _files.Add(path) Then
                Call _set.Assemblies.Add(path)

                If _verbose Then
                    Call Console.WriteLine($"    #include assembly -> {path}")
                End If
            End If
        End Sub

        ''' <summary>展开一个被 <c>#include</c> 引入的其它脚本</summary>
        Private Sub IncludeScript(scriptFile As String, declaredIn As String)
            If _scripts.Contains(scriptFile) Then
                ' 同一个脚本被多次引入, 只展开一次(避免类型重复定义)
                Return
            End If

            If _expanding.Any(Function(p) String.Equals(p, scriptFile, StringComparison.OrdinalIgnoreCase)) Then
                Dim chain As New List(Of String)

                For Each p As String In _expanding
                    Call chain.Add(Path.GetFileName(p))
                Next

                Call chain.Add(Path.GetFileName(scriptFile))

                Throw New InvalidOperationException("检测到 #include 循环引用: " & String.Join(" -> ", chain))
            End If

            Dim source As String = File.ReadAllText(scriptFile)

            Call ValidateScript(source, scriptFile)

            Call _expanding.Add(scriptFile)

            Try
                ' 被引入脚本不做向量化改写: 其类型定义会被原样并入主脚本,
                ' 而它自身的文件头并不会带上 SIMD 的 Imports(工程期路径尤其如此)。
                Dim code As String = ScriptRefactor.PreprocessText(source, vectorize:=False)
                Dim syntax As ScriptStructure = ScriptStructure.Scan(code)

                If syntax.Functions.Count > 0 Then
                    Throw New InvalidOperationException(
                        $"被 #include 引入的脚本不支持顶层函数(仅主脚本可以有顶层函数): {scriptFile}{vbCrLf}" &
                        $"违规函数: {String.Join(", ", syntax.Functions.Select(Function(f) f.Name))}")
                End If

                If syntax.Slots.Count > 0 Then
                    Throw New InvalidOperationException(
                        $"被 #include 引入的脚本不支持顶层语句(其代码会被直接复制到主脚本的顶层命名空间): {scriptFile}{vbCrLf}" &
                        $"违规语句: {syntax.Slots(0).Statements(0)}")
                End If

                ' 先登记, 再递归处理本脚本自身的 #include, 保证依赖顺序稳定
                Call _scripts.Add(scriptFile)
                Call _set.Scripts.Add(New IncludedScript With {
                    .FilePath = scriptFile,
                    .DeclaredIn = declaredIn
                })
                Call _set.HeaderImports.AddRange(syntax.Headers)
                Call _set.TypeBlocks.AddRange(syntax.TypeBlocks)

                If _verbose Then
                    Call Console.WriteLine($"    #include script   -> {scriptFile} ({syntax.TypeBlocks.Count} 个类型定义块)")
                End If

                Call ResolveSource(source, scriptFile)
            Finally
                Call _expanding.Remove(scriptFile)
            End Try
        End Sub

        ''' <summary>
        ''' 校验被引入脚本的合法性: 不得声明元数据指令, 不得调用魔法方法。
        ''' </summary>
        Private Shared Sub ValidateScript(source As String, scriptFile As String)
            Dim metadata As ScriptMetadata = ScriptMetadata.Parse(source)

            If Not String.IsNullOrEmpty(metadata.Package) OrElse
               Not String.IsNullOrEmpty(metadata.Author) OrElse
               Not String.IsNullOrEmpty(metadata.Title) OrElse
               Not String.IsNullOrEmpty(metadata.Version) Then

                Throw New InvalidOperationException(
                    $"被 #include 引入的脚本不支持元数据定义指令(#package/#author/#title/#version, 仅主脚本支持): {scriptFile}")
            End If

            For Each name As String In MagicNames
                Dim pattern As String = "(?<![\w.])" & Regex.Escape(name) & "\s*\("

                If Regex.IsMatch(source, pattern, RegexOptions.IgnoreCase) Then
                    Throw New InvalidOperationException(
                        $"被 #include 引入的脚本不允许调用脚本引擎魔法方法 '{name}'(魔法方法仅主脚本可用): {scriptFile}")
                End If
            Next
        End Sub

        ''' <summary>解析并缓存一个 nuget 包(含全部传递依赖)</summary>
        Private Sub IncludeNuGet(packageId As String, version As String, declaredIn As String)
            Dim constraint As String = NormalizeVersionConstraint(version)

            If _verbose Then
                Call Console.WriteLine($"    #include nuget    -> {packageId}{(If(String.IsNullOrEmpty(constraint), " <latest>", " " & constraint))}")
            End If

            Dim result As NuGetResolveResult = NuGetResolver.Resolve(packageId, constraint, ScriptTargetFramework)

            For Each package As NuGetPackage In result.Packages
                Dim key As String = $"{package.Id}@{package.Version}"

                If Not _packages.Add(key) Then
                    Continue For
                End If

                Call _set.NuGetPackages.Add(package)

                For Each dll As String In package.Assemblies
                    Call AddAssembly(dll)
                Next
            Next

            If _verbose Then
                Call Console.WriteLine($"                      {result.AllAssemblies().Length} 个程序集(" & declaredIn & ")")
            End If
        End Sub

        ''' <summary>
        ''' 把 <c>@version</c> 语法规范化为 nuget 版本约束:
        ''' 裸版本号(例如 <c>13.0.3</c>)表示<b>精确版本</b>; 显式的方括号区间
        ''' (例如 <c>[13.0,14.0)</c>)原样保留; 缺省表示最新稳定版。
        ''' </summary>
        Friend Shared Function NormalizeVersionConstraint(version As String) As String
            If String.IsNullOrWhiteSpace(version) Then
                Return Nothing
            End If

            Dim value As String = version.Trim()

            If value.StartsWith("["c) OrElse value.StartsWith("("c) Then
                Return value
            End If

            Return "[" & value & "]"
        End Function

        ''' <summary>
        ''' 判断一个无法在本地定位的目标是否形如 nuget 包引用(<c>id</c> 或 <c>id@version</c>)。
        ''' </summary>
        Friend Shared Function TryParseNuGet(raw As String, ByRef packageId As String, ByRef version As String) As Boolean
            packageId = Nothing
            version = Nothing

            If String.IsNullOrWhiteSpace(raw) Then
                Return False
            End If

            ' 含路径分隔符的一定是本地文件路径
            If raw.IndexOfAny({"/"c, "\"c}) >= 0 Then
                Return False
            End If

            Dim trimmed As String = raw.Trim()
            Dim at As Integer = trimmed.LastIndexOf("@"c)
            Dim idPart As String
            Dim verPart As String = Nothing

            If at > 0 Then
                idPart = trimmed.Substring(0, at).Trim()
                verPart = trimmed.Substring(at + 1).Trim()

                If verPart.Length = 0 Then
                    verPart = Nothing
                End If
            Else
                idPart = trimmed
            End If

            ' 以程序集/脚本扩展名结尾的目标按本地文件处理(即使文件当前不存在)
            Dim dot As Integer = idPart.LastIndexOf("."c)
            Dim ext As String = If(dot >= 0, idPart.Substring(dot), "")

            If FileLikeExtensions.Any(Function(e) String.Equals(e, ext, StringComparison.OrdinalIgnoreCase)) Then
                Return False
            End If

            packageId = idPart
            version = verPart

            Return packageId.Length > 0
        End Function

        ''' <summary>
        ''' 把 nuget 包携带的原生资产目录加入本进程的原生库搜索路径,
        ''' 使脚本通过 <c>DllImport</c> 使用包内原生库时可以被定位。
        ''' </summary>
        Private Sub ApplyNativeLibraryPath()
            Dim dirs As String() = _set.NuGetPackages _
                .SelectMany(Function(p) p.NativeAssets) _
                .Distinct(StringComparer.OrdinalIgnoreCase) _
                .ToArray()

            If dirs.Length = 0 Then
                Return
            End If

            Dim current As String = Environment.GetEnvironmentVariable("PATH")

            If current Is Nothing Then
                current = ""
            End If

            Dim missing As String() = dirs.Where(Function(dir) current.IndexOf(dir, StringComparison.OrdinalIgnoreCase) < 0).ToArray()

            If missing.Length = 0 Then
                Return
            End If

            Dim value As String = String.Join(Path.PathSeparator.ToString(), missing)

            If value.Length > 0 AndAlso current.Length > 0 Then
                value = value & Path.PathSeparator.ToString() & current
            End If

            Call Environment.SetEnvironmentVariable("PATH", value)
        End Sub
    End Class
End Namespace
