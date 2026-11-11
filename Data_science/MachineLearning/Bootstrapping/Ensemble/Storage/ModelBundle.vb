Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 模型包目录：一个 <c>model.json</c> 清单 + 若干 CSV 数值附表。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 目录布局示例：
    ''' <code>
    ''' &lt;model-bundle&gt;/
    ''' ├── model.json                       ' 清单：类型、超参数、类别、特征维度、子模型
    ''' ├── base/0-naive-bayes/tables.csv    ' 条件频率表（长表）
    ''' ├── base/1-svm/support-vectors.csv   ' 支持向量的稀疏三元组
    ''' └── meta/logistic/…                  ' 堆叠的元学习器
    ''' </code>
    ''' 清单中的 <see cref="ModelManifest.data"/> 只保存**相对于包根目录**的 CSV 路径，
    ''' 加载时统一经过 <see cref="ResolvePath"/> 规范化，拒绝绝对路径与 <c>..</c>
    ''' 穿越，避免恶意模型包读写包外的文件。
    ''' </para>
    ''' <para>
    ''' 清单一律先写临时文件再替换，保证不会出现「JSON 写了一半」的半成品包。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ModelBundle

        ''' <summary>清单文件名</summary>
        Public Const ManifestFileName As String = "model.json"

        Private Const TempManifestFileName As String = "model.json.tmp"

        Private ReadOnly _root As String

        Private Sub New(root As String)
            _root = Path.GetFullPath(root)
        End Sub

        ''' <summary>模型包根目录（绝对路径）</summary>
        Public ReadOnly Property Root As String
            Get
                Return _root
            End Get
        End Property

        ''' <summary>清单文件的绝对路径</summary>
        Public ReadOnly Property ManifestPath As String
            Get
                Return Path.Combine(_root, ManifestFileName)
            End Get
        End Property

        ''' <summary>
        ''' 创建（或复用）模型包目录；目录已存在时不会删除其中的旧文件，
        ''' 重名文件在写入时被覆盖。
        ''' </summary>
        ''' <param name="root">模型包根目录</param>
        Public Shared Function Create(root As String) As ModelBundle
            If String.IsNullOrEmpty(root) Then
                Throw New ArgumentException("the model bundle directory can not be empty!", NameOf(root))
            End If

            Dim full As String = Path.GetFullPath(root)

            If File.Exists(full) Then
                Throw New IOException($"the model bundle path '{full}' is an existing file, not a directory!")
            End If
            If Not Directory.Exists(full) Then
                Call Directory.CreateDirectory(full)
            End If

            Return New ModelBundle(full)
        End Function

        ''' <summary>打开已存在的模型包目录（要求 <c>model.json</c> 存在）</summary>
        ''' <param name="root">模型包根目录</param>
        Public Shared Function Open(root As String) As ModelBundle
            If String.IsNullOrEmpty(root) Then
                Throw New ArgumentException("the model bundle directory can not be empty!", NameOf(root))
            End If

            Dim full As String = Path.GetFullPath(root)

            If Not Directory.Exists(full) Then
                Throw New DirectoryNotFoundException($"the model bundle directory '{full}' does not exist!")
            End If
            If Not File.Exists(Path.Combine(full, ManifestFileName)) Then
                Throw New FileNotFoundException(
                    $"the model bundle '{full}' has no '{ManifestFileName}' manifest!", Path.Combine(full, ManifestFileName))
            End If

            Return New ModelBundle(full)
        End Function

        ''' <summary>读取并校验清单</summary>
        Public Function ReadManifest() As ModelManifest
            Dim json As String = File.ReadAllText(ManifestPath, Encoding.UTF8)
            Dim manifest As ModelManifest = json.LoadJSON(Of ModelManifest)()

            If manifest Is Nothing Then
                Throw New InvalidDataException($"the model manifest '{ManifestPath}' can not be deserialized!")
            End If

            Call ValidateManifest(manifest)

            Return manifest
        End Function

        ''' <summary>写入清单（先写临时文件，再原子替换）</summary>
        ''' <param name="manifest">待写入的清单</param>
        Public Sub WriteManifest(manifest As ModelManifest)
            If manifest Is Nothing Then
                Throw New ArgumentNullException(NameOf(manifest))
            End If

            Call manifest.EnsureDictionaries()

            Dim json As String = manifest.GetJson(indent:=True)
            Dim temp As String = Path.Combine(_root, TempManifestFileName)

            Call File.WriteAllText(temp, json, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            If File.Exists(ManifestPath) Then
                Call File.Delete(ManifestPath)
            End If

            Call File.Move(temp, ManifestPath)
        End Sub

        ''' <summary>
        ''' 把一个二维表写到包的子目录，并在清单中登记「逻辑名 → 相对路径」。
        ''' </summary>
        ''' <param name="manifest">接收路径登记的目标清单</param>
        ''' <param name="name">逻辑名（CSV 文件名，不含扩展名）</param>
        ''' <param name="folder">包内的相对目录，例如 <c>base/0-naive-bayes</c>；空值表示包根目录</param>
        ''' <param name="header">表头</param>
        ''' <param name="rows">数据行</param>
        ''' <returns>写入的 CSV 绝对路径</returns>
        Public Function WriteCsv(manifest As ModelManifest,
                                 name As String,
                                 folder As String,
                                 header As String(),
                                 rows As IEnumerable(Of String())) As String
            If manifest Is Nothing Then
                Throw New ArgumentNullException(NameOf(manifest))
            End If
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentException("the data table name can not be empty!", NameOf(name))
            End If

            Call manifest.EnsureDictionaries()

            Dim relative As String = CombineRelative(folder, name & ".csv")
            Dim path As String = ResolvePath(relative)

            Call CsvTable.Save(path, header, rows)
            manifest.data(name) = relative

            Return path
        End Function

        ''' <summary>读取清单中登记的 CSV 附表</summary>
        ''' <param name="manifest">模型清单</param>
        ''' <param name="name">逻辑名</param>
        Public Function ReadCsv(manifest As ModelManifest, name As String) As CsvTable
            If manifest Is Nothing Then
                Throw New ArgumentNullException(NameOf(manifest))
            End If

            Dim relative As String = Nothing

            If manifest.data Is Nothing OrElse Not manifest.data.TryGetValue(name, relative) Then
                Throw New InvalidOperationException(
                    $"the model manifest (modelType='{manifest.modelType}', algorithm='{manifest.algorithm}') " &
                    $"does not reference the data table '{name}'!")
            End If

            Return CsvTable.Load(ResolvePath(relative))
        End Function

        ''' <summary>
        ''' 把包内相对路径解析为绝对路径，并校验结果仍位于包根目录内。
        ''' </summary>
        ''' <param name="relative">形如 <c>base/0-svm/vectors.csv</c> 的相对路径</param>
        Public Function ResolvePath(relative As String) As String
            If String.IsNullOrEmpty(relative) Then
                Throw New ArgumentException("the model data path can not be empty!", NameOf(relative))
            End If

            Dim normalized As String = relative.Replace("\"c, "/"c).Trim()

            If normalized.StartsWith("/", StringComparison.Ordinal) OrElse normalized.IndexOf(":"c) >= 0 Then
                Throw New InvalidDataException(
                    $"the model data path '{relative}' must be relative to the bundle root!")
            End If

            Dim parts As String() = normalized.Split("/"c)
            Dim safe As New List(Of String)()

            For Each part As String In parts
                If part.Length = 0 OrElse part = "." Then
                    Continue For
                End If
                If part = ".." Then
                    Throw New InvalidDataException(
                        $"the model data path '{relative}' escapes the bundle root!")
                End If

                Call safe.Add(part)
            Next

            If safe.Count = 0 Then
                Throw New ArgumentException($"the model data path '{relative}' contains no file name!", NameOf(relative))
            End If

            Dim full As String = Path.GetFullPath(Path.Combine(_root, Path.Combine(safe.ToArray())))
            Dim prefix As String = _root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar

            If Not full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException(
                    $"the model data path '{relative}' resolves outside the bundle root ({full})!")
            End If

            Return full
        End Function

        ''' <summary>拼接包内相对目录与文件名（统一使用 <c>/</c> 作为分隔符）</summary>
        ''' <param name="folder">相对目录，可为空</param>
        ''' <param name="fileName">文件名</param>
        Public Shared Function CombineRelative(folder As String, fileName As String) As String
            Dim left As String = If(folder, "").Replace("\"c, "/"c).Trim("/"c)
            Dim right As String = If(fileName, "").Replace("\"c, "/"c).Trim("/"c)

            If left.Length = 0 Then
                Return right
            End If

            Return left & "/" & right
        End Function

        Public Overrides Function ToString() As String
            Return $"model bundle: {_root}"
        End Function

        ''' <summary>校验清单的格式版本与必填字段</summary>
        ''' <param name="manifest">待校验的清单</param>
        Public Shared Sub ValidateManifest(manifest As ModelManifest)
            If manifest.formatVersion <> ModelManifest.CurrentFormatVersion Then
                Throw New InvalidDataException(
                    $"unsupported model format version '{manifest.formatVersion}', " &
                    $"this build supports '{ModelManifest.CurrentFormatVersion}' only!")
            End If
            If String.IsNullOrEmpty(manifest.kind) Then
                Throw New InvalidDataException("the model manifest has no 'kind' (classifier/regressor)!")
            End If
            If String.IsNullOrEmpty(manifest.modelType) Then
                Throw New InvalidDataException("the model manifest has no 'modelType'!")
            End If
        End Sub

    End Class

End Namespace
