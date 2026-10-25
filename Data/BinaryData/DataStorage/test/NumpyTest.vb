Imports System.Diagnostics
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Numpy

''' <summary>
''' numpy 的 .npy / .npz 读写演示：读取 TCGA COAD 的 r160_subtype.npy（对象数组），
''' 输出头部与内嵌矩阵信息，再把最大的矩阵写成 .npy 与 .npz 并读回做逐元素校验。
''' </summary>
Module NumpyTest

    Const DefaultDataFile As String = "G:\GCModeller\src\GCModeller\annotations\GSEA\data\TCGA_COAD\r160_subtype.npy"

    Dim passCount As Integer = 0
    Dim failCount As Integer = 0

    ''' <summary>
    ''' 演示入口。
    ''' </summary>
    ''' <param name="args">可选的 npy 文件路径（默认使用 TCGA COAD 的 r160_subtype.npy）</param>
    Function Main(args As String()) As Integer
        Dim file As String = If(args IsNot Nothing AndAlso args.Length > 0, args(0), DefaultDataFile)
        Dim workDir As String = Path.Combine(AppContext.BaseDirectory, "numpy-demo")

        Call Section("numpy .npy / .npz storage demo")
        Console.WriteLine($"input file : {file}")

        If Not System.IO.File.Exists(file) Then
            Console.WriteLine("ERROR: input file does not exist!")
            Return 1
        End If

        Dim total As New Stopwatch()

        Call total.Start()

        Dim arrays As New List(Of NpyArray)()

        Try
            If file.EndsWith(".npz", StringComparison.OrdinalIgnoreCase) Then
                Dim archive As NpzFile = NpzFile.Read(file)

                Call Section("1. archive")
                Console.WriteLine($"npz entries : {archive.Count} ({String.Join(", ", archive.Names)})")

                For Each name As String In archive.Names
                    Call arrays.Add(archive(name))
                Next
            Else
                Dim header As NpyHeader = NpyFile.ReadHeader(file)

                Call Section("1. header")
                Call DumpHeader(header, file)
                Call arrays.Add(NpyFile.Read(file))
            End If
        Catch ex As Exception
            Console.WriteLine($"ERROR: failed to load numpy data: {ex.Message}")
            Console.WriteLine(ex.StackTrace)
            Return 1
        End Try

        Call Section("2. array content")

        For Each array As NpyArray In arrays
            Call DumpArray(array, 0)
        Next

        Call total.Stop()

        Console.WriteLine()
        Console.WriteLine($"decode time: {total.ElapsedMilliseconds} ms")

        Call Section("3. write / read round trip")

        If Not Directory.Exists(workDir) Then
            Call Directory.CreateDirectory(workDir)
        End If

        Dim largest As NpyArray = Nothing

        For Each array As NpyArray In arrays
            Dim candidate As NpyArray = FindLargestNumericArray(array)

            If candidate IsNot Nothing AndAlso (largest Is Nothing OrElse candidate.Length > largest.Length) Then
                largest = candidate
            End If
        Next

        If largest Is Nothing Then
            Console.WriteLine("WARN: no numeric sub array was found, skipping the round trip tests.")
            Call Summary()
            Return If(failCount = 0, 0, 1)
        End If

        Console.WriteLine($"payload : {largest}")

        Call TestNpyRoundTrip(largest, Path.Combine(workDir, "r160_subtype.payload.npy"))
        Call TestNpzRoundTrip(largest, Path.Combine(workDir, "r160_subtype.payload.npz"))
        Call TestFortranOrder(Path.Combine(workDir, "fortran-order.npy"))
        Call TestTypedArrays(workDir)

        Call Summary()

        Return If(failCount = 0, 0, 1)
    End Function

    Private Function FormatShape(shape As Integer()) As String
        If shape Is Nothing OrElse shape.Length = 0 Then
            Return "()"
        ElseIf shape.Length = 1 Then
            Return $"({shape(0)},)"
        Else
            Return $"({String.Join(", ", shape)})"
        End If
    End Function

    Private Sub Section(title As String)
        Console.WriteLine()
        Console.WriteLine(New String("="c, 78))
        Console.WriteLine(title)
        Console.WriteLine(New String("="c, 78))
    End Sub

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = Nothing)
        If condition Then
            passCount += 1
            Console.WriteLine($"  [PASS] {name}")
        Else
            failCount += 1
            Console.WriteLine($"  [FAIL] {name}")
        End If

        If Not String.IsNullOrEmpty(detail) Then
            Console.WriteLine($"         {detail}")
        End If
    End Sub

    Private Sub Summary()
        Call Section("summary")
        Console.WriteLine($"checks passed : {passCount}")
        Console.WriteLine($"checks failed : {failCount}")
        Console.WriteLine(If(failCount = 0, "RESULT: PASS", "RESULT: FAIL"))
    End Sub

    Private Sub DumpHeader(header As NpyHeader, file As String)
        Dim info As New FileInfo(file)

        Console.WriteLine($"format version : {header.Version}")
        Console.WriteLine($"descr          : {header.Descr}")
        Console.WriteLine($"dtype kind     : {header.DType.Kind} (item size {header.DType.ItemSize} bytes, endian {header.DType.Endian})")
        Console.WriteLine($"fortran_order  : {header.FortranOrder}")
        Console.WriteLine($"shape          : {FormatShape(header.Shape)}")
        Console.WriteLine($"elements       : {header.Length}")
        Console.WriteLine($"header size    : {header.HeaderSize} bytes (data offset {header.DataOffset})")
        Console.WriteLine($"file size      : {info.Length} bytes")
    End Sub

    Private Sub DumpArray(array As NpyArray, indent As Integer)
        Dim pad As String = New String(" "c, indent)

        Console.WriteLine($"{pad}{array}")

        If array.IsObjectArray Then
            Dim items As Object() = array.AsObjectArray()

            Console.WriteLine($"{pad}object elements: {items.Length}")

            For i As Integer = 0 To items.Length - 1
                Dim item As Object = items(i)

                If TypeOf item Is NpyArray Then
                    Call DumpArray(DirectCast(item, NpyArray), indent + 4)
                Else
                    Console.WriteLine($"{pad}    [{i}] {If(item Is Nothing, "None", item.ToString())} ({If(item Is Nothing, "None", item.GetType().Name)})")
                End If
            Next
        ElseIf array.DType.IsNumeric Then
            Call DumpStatistics(array, indent + 4)
        ElseIf array.DType.IsStringType Then
            Dim values As String() = array.AsString()
            Dim sample As String() = values.Take(8).Select(Function(s) $"'{s}'").ToArray()

            Console.WriteLine($"{pad}    strings: {values.Length}, first: {String.Join(", ", sample)}")
        Else
            Console.WriteLine($"{pad}    raw bytes: {If(array.RawData, New Byte() {}).Length}")
        End If
    End Sub

    Private Sub DumpStatistics(array As NpyArray, indent As Integer)
        Dim pad As String = New String(" "c, indent)
        Dim values As Double() = array.AsDouble()
        Dim min As Double = Double.MaxValue
        Dim max As Double = Double.MinValue
        Dim sum As Double = 0
        Dim nanCount As Integer = 0

        For Each v As Double In values
            If Double.IsNaN(v) Then
                nanCount += 1
                Continue For
            End If

            If v < min Then min = v
            If v > max Then max = v

            sum += v
        Next

        Dim mean As Double = If(values.Length > nanCount, sum / (values.Length - nanCount), Double.NaN)
        Dim sample As String() = Enumerable.Range(0, System.Math.Min(6, values.Length)).Select(Function(i) values(i).ToString("G6")).ToArray()

        Console.WriteLine($"{pad}first values : {String.Join(", ", sample)}")
        Console.WriteLine($"{pad}min/max/mean : {min:G6} / {max:G6} / {mean:G6}" & If(nanCount > 0, $" (NaN count {nanCount})", ""))
    End Sub

    Private Function FindLargestNumericArray(array As NpyArray) As NpyArray
        Dim best As NpyArray = Nothing

        If array.IsObjectArray Then
            For Each item As Object In array.AsObjectArray()
                Dim sub1 As NpyArray = TryCast(item, NpyArray)

                If sub1 IsNot Nothing AndAlso sub1.DType.IsNumeric Then
                    If best Is Nothing OrElse sub1.Length > best.Length Then
                        best = sub1
                    End If
                End If
            Next
        ElseIf array.DType.IsNumeric Then
            best = array
        End If

        Return best
    End Function

    Private Sub TestNpyRoundTrip(array As NpyArray, file As String)
        Console.WriteLine()
        Console.WriteLine($"npy round trip -> {file}")

        Dim source As Double() = array.AsDouble()
        Dim clock As Stopwatch = Stopwatch.StartNew()

        Call NpyFile.Write(file, source, array.Shape)

        clock.Stop()

        Console.WriteLine($"  wrote {new FileInfo(file).Length} bytes in {clock.ElapsedMilliseconds} ms")

        Dim reloaded As NpyArray = NpyFile.Read(file)
        Dim copy As Double() = reloaded.AsDouble()

        Call Check("npy header round trip", reloaded.Shape.SequenceEqual(array.Shape) AndAlso reloaded.DType.Descriptor = "<f8",
            $"shape=({String.Join(", ", reloaded.Shape)}) dtype={reloaded.DType}")
        Call Check("npy element count", copy.Length = source.Length, $"{copy.Length} vs {source.Length}")

        Dim diff As Double = 0

        For i As Integer = 0 To System.Math.Min(copy.Length, source.Length) - 1
            Dim d As Double = System.Math.Abs(copy(i) - source(i))

            If d > diff Then diff = d
        Next

        Call Check("npy payload is bit exact", diff = 0, $"max |diff| = {diff}")

        ' 顺带验证矩阵转换接口
        If array.Rank = 2 Then
            Dim matrix = reloaded.AsMatrix()

            Call Check("npy as matrix", matrix.RowDimension = array.Shape(0) AndAlso matrix.ColumnDimension = array.Shape(1),
                $"{matrix.RowDimension} x {matrix.ColumnDimension}")
        End If
    End Sub

    Private Sub TestNpzRoundTrip(array As NpyArray, file As String)
        Console.WriteLine()
        Console.WriteLine($"npz round trip -> {file}")

        Dim source As Double() = array.AsDouble()
        Dim labelCount As Integer = If(array.Rank >= 2, array.Shape(array.Rank - 1), array.Shape(0))
        Dim labels As String() = Enumerable.Range(0, labelCount).Select(Function(i) $"sample_{i + 1}").ToArray()

        Dim npz As New NpzFile()

        Call npz.Add("expression", New NpyArray With {
            .DType = array.DType,
            .Shape = array.Shape,
            .FortranOrder = array.FortranOrder,
            .RawData = array.RawData
        })
        Call npz.Add("labels", New NpyArray With {
            .DType = NpyDType.Parse("<U" & labels.Max(Function(s) s.Length)),
            .Shape = New Integer() {labels.Length},
            .FortranOrder = False,
            .RawData = EncodeUtf32(labels)
        })

        Call npz.Save(file)

        Dim reloaded As NpzFile = NpzFile.Read(file)

        Call Check("npz entry names", reloaded.Names.SequenceEqual(New String() {"expression", "labels"}),
            String.Join(", ", reloaded.Names))

        Dim matrix As NpyArray = reloaded("expression")
        Dim copy As Double() = matrix.AsDouble()
        Dim diff As Double = 0

        For i As Integer = 0 To System.Math.Min(copy.Length, source.Length) - 1
            Dim d As Double = System.Math.Abs(copy(i) - source(i))

            If d > diff Then diff = d
        Next

        Call Check("npz payload is bit exact", copy.Length = source.Length AndAlso diff = 0,
            $"elements={copy.Length} max |diff| = {diff}")

        Dim restored As String() = reloaded("labels").AsString()

        Call Check("npz string payload", restored.SequenceEqual(labels), String.Join(", ", restored.Take(3)))
        Call Check("npz size", (New FileInfo(file)).Length > 0, $"{(New FileInfo(file)).Length} bytes")
    End Sub

    Private Sub TestFortranOrder(file As String)
        Console.WriteLine()
        Console.WriteLine($"fortran order round trip -> {file}")

        ' 3 x 4，按 C 序给出的元素
        Dim data As Double() = {1, 2, 3, 4, 11, 12, 13, 14, 21, 22, 23, 24}
        Dim shape As Integer() = {3, 4}

        Call NpyFile.Write(file, data, shape, fortranOrder:=True)

        Dim header As NpyHeader = NpyFile.ReadHeader(file)
        Dim reloaded As NpyArray = NpyFile.Read(file)
        Dim copy As Double() = reloaded.AsDouble()

        Call Check("fortran_order flag is written", header.FortranOrder, header.ToString())
        Call Check("fortran order payload matches C order semantics", copy.SequenceEqual(data), String.Join(", ", copy))
    End Sub

    Private Sub TestTypedArrays(workDir As String)
        Console.WriteLine()
        Console.WriteLine("typed array round trip")

        Dim ints As Integer() = {1, -2, 3, -4, 5}
        Dim intFile As String = Path.Combine(workDir, "int32.npy")

        Call NpyFile.Write(intFile, ints)

        Dim intsBack As NpyArray = NpyFile.Read(intFile)

        Call Check("int32 array", intsBack.AsInt32().SequenceEqual(ints), $"{intsBack.DType} -> {String.Join(", ", intsBack.AsInt32())}")

        Dim flags As Boolean() = {True, False, True, True}
        Dim boolFile As String = Path.Combine(workDir, "bool.npy")

        Call NpyFile.Write(boolFile, flags)

        Dim flagsBack As NpyArray = NpyFile.Read(boolFile)

        Call Check("bool array", flagsBack.AsBoolean().SequenceEqual(flags), $"{flagsBack.DType} -> {String.Join(", ", flagsBack.AsBoolean())}")

        Dim names As String() = {"alpha", "beta", "gamma", ""}
        Dim stringFile As String = Path.Combine(workDir, "unicode.npy")

        Call NpyFile.Write(stringFile, names)

        Dim namesBack As NpyArray = NpyFile.Read(stringFile)

        Call Check("unicode array", namesBack.AsString().SequenceEqual(names), $"{namesBack.DType} -> {String.Join(", ", namesBack.AsString())}")
    End Sub

    Private Function EncodeUtf32(data As String()) As Byte()
        Dim capacity As Integer = data.Max(Function(s) s.Length)
        Dim buffer As Byte() = New Byte(data.Length * capacity * 4 - 1) {}

        For i As Integer = 0 To data.Length - 1
            For c As Integer = 0 To data(i).Length - 1
                Dim chunk As Byte() = BitConverter.GetBytes(AscW(data(i)(c)))

                Call Array.Copy(chunk, 0, buffer, (i * capacity + c) * 4, 4)
            Next
        Next

        Return buffer
    End Function

End Module
