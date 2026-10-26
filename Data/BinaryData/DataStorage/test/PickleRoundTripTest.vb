Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Pickle

''' <summary>
''' 演示用的 CLR 对象：覆盖值类型、字符串、日期、枚举、数组、泛型集合、嵌套对象与自引用。
''' </summary>
Public Class PickleDemoNode
    Public Property Name As String
    Public Property Value As Double
    Public Property Flag As Boolean
    Public Property Count As Integer
    Public Property Created As DateTime
    Public Property Kind As PickleDemoKind
    Public Property Ratio As Double?
    Public Property Tags As List(Of String)
    Public Property Scores As Integer()
    Public Property Attributes As Dictionary(Of String, Double)
    Public Property Child As PickleDemoChild
    Public Property NextNode As PickleDemoNode
    Public Property Stamp As Guid
    Public Property Elapsed As TimeSpan
    Public Property OffsetTime As DateTimeOffset
    Public Property Homepage As Uri
    Public Property PayloadType As Type
    Public Property Blob As Byte()
    Public Property Void As Object
    Public Property BigNegative As Long
    Public Property Signal As Func(Of Integer)
    Public Property Origin As PickleDemoPoint
    Public Property Points As List(Of PickleDemoPoint)
    Friend Property FriendChild As PickleDemoFriend
End Class

''' <summary>
''' 演示用的结构体：验证值类型（装箱）也能通过字段回填还原。
''' </summary>
Public Structure PickleDemoPoint
    Public X As Double
    Public Y As Double
    Public Name As String
End Structure

''' <summary>
''' 演示用的 Friend（internal）类型：验证非 public 类型同样可以按类型名解析。
''' </summary>
Friend Class PickleDemoFriend
    Public Property Comment As String
    Public Property Level As Integer
End Class

''' <summary>
''' 只提供参数化构造函数的类型：验证"未初始化对象 + 字段回填"能还原只读字段。
''' </summary>
Public Class PickleDemoChild
    Public ReadOnly Id As Long
    Public ReadOnly IdAsText As String
    Public Property Label As String
    Public Property Weight As Single
    Public Property Hidden As String

    Public Sub New(id As Long, label As String)
        Me.Id = id
        Me.IdAsText = id.ToString()
        Me.Label = label
    End Sub
End Class

''' <summary>
''' 演示用的枚举。
''' </summary>
Public Enum PickleDemoKind As Byte
    Unknown = 0
    Alpha = 1
    Beta = 2
End Enum

''' <summary>
''' PickleSerializer 的 .NET CLR 对象往返测试：验证 <see cref="MinimalPicklePickler"/> 写出的数据
''' 能被 <see cref="MinimalPickleUnpickler"/> 还原成原始 CLR 类型（而不是 <see cref="PythonObject"/> 包装器）。
''' </summary>
Module PickleRoundTripTest

    Dim passCount As Integer = 0
    Dim failCount As Integer = 0

    ''' <summary>
    ''' 执行全部往返测试。
    ''' </summary>
    ''' <returns>进程退出码：0 表示全部通过</returns>
    Function Run() As Integer
        Call Section("pickle CLR object round trip")

        Call TestPrimitivesAndCollections()
        Call TestClrObjectGraph()
        Call TestSharedReferences()
        Call TestLongBoundaries()
        Call TestFileRoundTrip()
        Call TestStructRoundTrip()
        Call TestUnknownPythonObject()

        Call Section("summary")
        Console.WriteLine($"checks passed : {passCount}")
        Console.WriteLine($"checks failed : {failCount}")
        Console.WriteLine(If(failCount = 0, "RESULT: PASS", "RESULT: FAIL"))

        Return If(failCount = 0, 0, 1)
    End Function

#Region "测试用例"

    Private Sub TestPrimitivesAndCollections()
        Call Section("1. primitives and python containers")

        Call Check("string", CStr(RoundTrip("hello") = "hello"))
        Call Check("integer", CLng(RoundTrip(123L)) = 123L)
        Call Check("boolean", CBool(RoundTrip(True)))
        Call Check("double", CDbl(RoundTrip(2.5)) = 2.5)
        Call Check("nothing", RoundTrip(Nothing) Is Nothing)

        Dim numbers As New List(Of Object) From {1L, 2.5, "three", True}
        Dim copyList = DirectCast(RoundTrip(numbers), List(Of Object))

        Call Check("list", copyList.Count = 4 AndAlso CLng(copyList(0)) = 1 AndAlso CStr(copyList(2)) = "three")

        Dim dict As New Dictionary(Of Object, Object) From {
            {"a", 1L},
            {"b", New List(Of Object) From {2L, 3L}}
        }
        Dim copyDict = DirectCast(RoundTrip(dict), Dictionary(Of Object, Object))

        Call Check("dict", copyDict.Count = 2 AndAlso CLng(copyDict("a")) = 1)

        Dim dt As New DateTime(2024, 5, 6, 7, 8, 9, 123)
        Dim copyDate = DirectCast(RoundTrip(dt), DateTime)

        Call Check("datetime", copyDate = dt, copyDate.ToString("o"))

        Dim span As TimeSpan = TimeSpan.FromSeconds(93784.5)
        Dim copySpan = DirectCast(RoundTrip(span), TimeSpan)

        Call Check("timespan", copySpan = span, copySpan.ToString())
    End Sub

    Private Sub TestClrObjectGraph()
        Call Section("2. clr object graph")

        Dim node As New PickleDemoNode With {
            .Name = "root",
            .Value = 3.14159,
            .Flag = True,
            .Count = 7,
            .Created = New DateTime(2023, 12, 31, 23, 59, 58),
            .Kind = PickleDemoKind.Beta,
            .Ratio = 0.618,
            .Tags = New List(Of String) From {"alpha", "beta", "gamma"},
            .Scores = {10, 20, 30, 40},
            .Attributes = New Dictionary(Of String, Double) From {{"auc", 0.91}, {"f1", 0.77}},
            .Child = New PickleDemoChild(20240101L, "nested"),
            .Stamp = New Guid("6f9619ff-8b86-d011-b42d-00c04fc964ff"),
            .Elapsed = TimeSpan.FromMinutes(90),
            .OffsetTime = New DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(8)),
            .Homepage = New Uri("https://github.com/xieguigang/sciBASIC"),
            .PayloadType = GetType(PickleDemoNode),
            .Blob = New Byte() {1, 2, 3, 4, 5},
            .Void = Nothing,
            .BigNegative = -3000000000L,
            .Signal = Function() 1,
            .Origin = New PickleDemoPoint With {.X = 1.5, .Y = -2.5, .Name = "origin"},
            .Points = New List(Of PickleDemoPoint) From {
                New PickleDemoPoint With {.X = 0, .Y = 0, .Name = "p0"},
                New PickleDemoPoint With {.X = 3, .Y = 4, .Name = "p1"}
            },
            .FriendChild = New PickleDemoFriend With {.Comment = "internal type", .Level = 9}
        }
        node.NextNode = ForgeNode("second", 2)

        Dim copy = DirectCast(RoundTrip(node), PickleDemoNode)

        Call Check("clr type is restored", copy IsNot Nothing AndAlso copy.GetType() Is GetType(PickleDemoNode),
            If(copy Is Nothing, "Nothing", copy.GetType().FullName))
        Call Check("string field", copy.Name = node.Name)
        Call Check("double field", copy.Value = node.Value)
        Call Check("boolean field", copy.Flag = node.Flag)
        Call Check("integer field", copy.Count = node.Count)
        Call Check("datetime field", copy.Created = node.Created, copy.Created.ToString("o"))
        Call Check("enum field", copy.Kind = PickleDemoKind.Beta, copy.Kind.ToString())
        Call Check("nullable field", copy.Ratio = 0.618)
        Call Check("list field", copy.Tags.SequenceEqual(node.Tags), String.Join(", ", copy.Tags))
        Call Check("array field", copy.Scores.SequenceEqual(node.Scores), String.Join(", ", copy.Scores))
        Call Check("dictionary field",
            copy.Attributes.Count = 2 AndAlso copy.Attributes("auc") = 0.91 AndAlso copy.Attributes("f1") = 0.77,
            String.Join(", ", copy.Attributes.Select(Function(kv) $"{kv.Key}={kv.Value}")))
        Call Check("guid field", copy.Stamp = node.Stamp, copy.Stamp.ToString())
        Call Check("timespan field", copy.Elapsed = node.Elapsed, copy.Elapsed.ToString())
        Call Check("datetimeoffset field", copy.OffsetTime = node.OffsetTime, copy.OffsetTime.ToString("o"))
        Call Check("uri field", copy.Homepage?.ToString() = node.Homepage.ToString(), copy.Homepage?.ToString())
        Call Check("type field", copy.PayloadType Is GetType(PickleDemoNode), copy.PayloadType?.FullName)
        Call Check("bytes field", copy.Blob.SequenceEqual(node.Blob))
        Call Check("null field", copy.Void Is Nothing)
        Call Check("delegate field is dropped", copy.Signal Is Nothing)

        ' 只带参数化构造函数的类型：只读字段也要还原
        Call Check("nested object type", TypeOf copy.Child Is PickleDemoChild)
        Call Check("nested readonly field", copy.Child.Id = 20240101L, copy.Child.Id.ToString())
        Call Check("nested readonly computed field", copy.Child.IdAsText = "20240101", copy.Child.IdAsText)
        Call Check("nested label", copy.Child.Label = "nested")

        Call Check("second level object", copy.NextNode IsNot Nothing AndAlso copy.NextNode.Name = "second",
            If(copy.NextNode Is Nothing, "Nothing", copy.NextNode.Name))

        ' 结构体字段（装箱后回填）
        Call Check("struct field", copy.Origin.X = 1.5 AndAlso copy.Origin.Y = -2.5 AndAlso copy.Origin.Name = "origin",
            $"{copy.Origin.X}, {copy.Origin.Y}, {copy.Origin.Name}")
        Call Check("list of struct", copy.Points.Count = 2 AndAlso copy.Points(1).X = 3 AndAlso copy.Points(1).Name = "p1",
            $"{copy.Points.Count} items")

        ' Friend（internal）类型
        Call Check("internal type is restored",
            copy.FriendChild IsNot Nothing AndAlso copy.FriendChild.Comment = "internal type" AndAlso copy.FriendChild.Level = 9,
            If(copy.FriendChild Is Nothing, "Nothing", $"{copy.FriendChild.Comment}/{copy.FriendChild.Level}"))
    End Sub

    Private Sub TestSharedReferences()
        Call Section("3. shared reference and cycle")

        Dim first As New PickleDemoNode With {.Name = "first"}
        Dim second As New PickleDemoNode With {.Name = "second"}
        Dim list As New List(Of Object) From {first, second, first}

        Dim copyList = DirectCast(RoundTrip(list), List(Of Object))

        Call Check("list element type", TypeOf copyList(0) Is PickleDemoNode)
        Call Check("shared instance is reused", Object.ReferenceEquals(copyList(0), copyList(2)))

        ' 环形引用
        Dim a As New PickleDemoNode With {.Name = "a"}
        Dim b As New PickleDemoNode With {.Name = "b"}

        a.NextNode = b
        b.NextNode = a

        Dim copyA = DirectCast(RoundTrip(a), PickleDemoNode)

        Call Check("cycle: first level", copyA.NextNode IsNot Nothing AndAlso copyA.NextNode.Name = "b")
        Call Check("cycle: back reference", Object.ReferenceEquals(copyA.NextNode.NextNode, copyA))
    End Sub

    Private Sub TestLongBoundaries()
        Call Section("4. integer boundaries")

        Dim values As Object() = {
            Long.MinValue,
            Long.MaxValue,
            -3000000000L,
            -256L,
            -1L,
            0L,
            255L,
            65536L
        }

        For Each value As Object In values
            Dim copy = RoundTrip(value)
            Dim text As String = value.ToString()

            Call Check($"long {text}", CDec(copy) = CDec(value), $"{copy}")
        Next
    End Sub

    Private Sub TestFileRoundTrip()
        Call Section("5. file round trip")

        Dim file As String = Path.Combine(AppContext.BaseDirectory, "numpy-demo", "clr-object.pickle")
        Dim dir As String = Path.GetDirectoryName(file)

        If Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        Dim node As New PickleDemoNode With {
            .Name = "from-file",
            .Count = 42,
            .Tags = New List(Of String) From {"x", "y"}
        }

        Call MinimalPicklePickler.PickleToFile(node, file)
        Call Check("pickle file is written", System.IO.File.Exists(file), $"{(New FileInfo(file)).Length} bytes")

        Dim copy As PickleDemoNode

        Using stream As Stream = New FileStream(file, FileMode.Open, FileAccess.Read)
            copy = DirectCast(MinimalPickleUnpickler.UnpickleFromStream(stream), PickleDemoNode)
        End Using

        Call Check("object is restored from file", copy IsNot Nothing AndAlso copy.GetType() Is GetType(PickleDemoNode))
        Call Check("object fields are restored from file", copy.Name = "from-file" AndAlso copy.Count = 42,
            $"{copy.Name}/{copy.Count}")
    End Sub

    Private Sub TestStructRoundTrip()
        Call Section("6. struct round trip")

        Dim point As New PickleDemoPoint With {.X = 1.5, .Y = -2.5, .Name = "p"}
        Dim copy = RoundTrip(point)

        Call Check("struct type", copy IsNot Nothing AndAlso copy.GetType() Is GetType(PickleDemoPoint),
            If(copy Is Nothing, "Nothing", copy.GetType().FullName))

        If TypeOf copy Is PickleDemoPoint Then
            Dim restored = DirectCast(copy, PickleDemoPoint)

            Call Check("struct fields", restored.X = 1.5 AndAlso restored.Y = -2.5 AndAlso restored.Name = "p",
                $"{restored.X}, {restored.Y}, {restored.Name}")
        End If
    End Sub

    Private Sub TestUnknownPythonObject()
        Call Section("7. unknown python object is still wrapped")

        ' 模拟 python 的 collections.OrderedDict 之外的未知类：应当继续返回 PythonObject 包装器
        Dim pyObj As New PythonObject("some.python.module", "UnknownClass", New Object() {1L})
        Dim copy = RoundTrip(pyObj)

        Call Check("unknown global is wrapped as PythonObject", TypeOf copy Is PythonObject,
            If(copy Is Nothing, "Nothing", copy.GetType().Name))
    End Sub

#End Region

    Private Function RoundTrip(value As Object) As Object
        Dim bytes As Byte() = MinimalPicklePickler.Pickle(value)

        Return MinimalPickleUnpickler.UnpickleFromBytes(bytes)
    End Function

    Private Function ForgeNode(name As String, count As Integer) As PickleDemoNode
        Return New PickleDemoNode With {.Name = name, .Count = count}
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

End Module
