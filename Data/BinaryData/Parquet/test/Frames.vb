Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.Framework

''' <summary>
''' demo 测试使用的 DataFrame 构造辅助函数。
''' </summary>
Friend Module Frames

    Friend Const NestedTypeAttribute As String = "parquet.type"

    ''' <summary>
    ''' 构造一个包含全部支持类型的扁平表。
    ''' </summary>
    Friend Function MakeFlat(rowCount As Integer) As DataFrame
        Dim df As New DataFrame
        Dim id As Integer() = New Integer(rowCount - 1) {}
        Dim count As Long() = New Long(rowCount - 1) {}
        Dim score As Double() = New Double(rowCount - 1) {}
        Dim weight As Single() = New Single(rowCount - 1) {}
        Dim flag As Boolean() = New Boolean(rowCount - 1) {}
        Dim label As String() = New String(rowCount - 1) {}
        Dim time As Date() = New Date(rowCount - 1) {}

        For i As Integer = 0 To rowCount - 1
            id(i) = i
            count(i) = 1000000000000L + i
            score(i) = i * 0.5R
            weight(i) = CSng(i / 4.0R)
            flag(i) = (i Mod 2 = 0)
            label(i) = If(i Mod 7 = 0, Nothing, $"row_{i}")
            time(i) = DateTime.UnixEpoch.AddSeconds(i)
        Next

        Call df.add(New FeatureVector("id", id))
        Call df.add(New FeatureVector("count", count))
        Call df.add(New FeatureVector("score", score))
        Call df.add(New FeatureVector("weight", weight))
        Call df.add(New FeatureVector("flag", flag))
        Call df.add(New FeatureVector("label", label))
        Call df.add(New FeatureVector("time", time))

        Dim names As String() = New String(rowCount - 1) {}

        For i As Integer = 0 To rowCount - 1
            names(i) = $"r{i}"
        Next

        df.rownames = names

        Return df
    End Function

    ''' <summary>
    ''' 构造用于 pyarrow 对拍的扁平表：包含 nullable 的字符串（区分 null 与空串）、
    ''' <c>NaN</c> 浮点以及 UTC 日期时间列。
    ''' </summary>
    Friend Function MakeInterop() As DataFrame
        Dim df As New DataFrame

        Call df.add(New FeatureVector("id", New Integer() {1, 2, 3, 4, 5, 6}))
        Call df.add(New FeatureVector("count", New Long() {
            1000000000000L, 1000000000001L, 1000000000002L,
            1000000000003L, 1000000000004L, 1000000000005L
        }))
        Call df.add(New FeatureVector("score", New Double() {0.5, -1.25, Double.NaN, 2.0, 0.0, 1.0E-08}))
        Call df.add(New FeatureVector("weight", New Single() {0.0F, 0.25F, 0.5F, 0.75F, 1.0F, 1.25F}))
        Call df.add(New FeatureVector("flag", New Boolean() {True, False, True, False, True, False}))
        Call df.add(New FeatureVector("label", New String() {"alpha", Nothing, "", "delta", "中文", "z"}))
        Call df.add(New FeatureVector("time", New Date() {
            New Date(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            New Date(2020, 1, 1, 1, 0, 0, DateTimeKind.Utc),
            New Date(2020, 1, 1, 2, 0, 0, DateTimeKind.Utc),
            New Date(2020, 1, 1, 3, 0, 0, DateTimeKind.Utc),
            New Date(2020, 1, 1, 4, 0, 0, DateTimeKind.Utc),
            New Date(2020, 1, 1, 5, 0, 0, DateTimeKind.Utc)
        }))

        df.rownames = New String() {"r1", "r2", "r3", "r4", "r5", "r6"}

        Return df
    End Function

    ''' <summary>
    ''' 构造一个包含嵌套类型（list / map / list&lt;struct&gt; / struct 内含 list）的 DataFrame。
    ''' </summary>
    Friend Function MakeNested() As DataFrame
        Dim df As New DataFrame

        ' list<double>
        Dim values As String() = {
            "[1,2,3]",
            "[4.5]",
            "[]",
            Nothing,
            "[6,7]"
        }
        Dim valuesColumn As New FeatureVector("values", values)

        valuesColumn.attributes = New Dictionary(Of String, String) From {
            {NestedTypeAttribute, "{""type"":""list"",""element"":{""type"":""double""}}"}
        }

        ' map<string,int64>
        Dim attrs As String() = {
            "{""a"":1,""b"":2}",
            "{}",
            "{""c"":3}",
            Nothing,
            "{""d"":4,""e"":5}"
        }
        Dim attrsColumn As New FeatureVector("attrs", attrs)

        attrsColumn.attributes = New Dictionary(Of String, String) From {
            {NestedTypeAttribute, "{""type"":""map"",""key"":{""type"":""string""},""value"":{""type"":""int64""}}"}
        }

        ' list<struct<x:int64, y:double>>
        Dim points As String() = {
            "[{""x"":1,""y"":1.5},{""x"":2,""y"":2.5}]",
            "[]",
            "[{""x"":3,""y"":3.5}]",
            Nothing,
            "[{""x"":4,""y"":4.5}]"
        }
        Dim pointsColumn As New FeatureVector("points", points)

        pointsColumn.attributes = New Dictionary(Of String, String) From {
            {NestedTypeAttribute, "{""type"":""list"",""element"":{""type"":""struct"",""fields"":{""x"":{""type"":""int64""},""y"":{""type"":""double""}}}}"}
        }

        ' struct { a:int64, l:list<int64> }
        Dim record As String() = {
            "{""a"":1,""l"":[1,2]}",
            "{""a"":2,""l"":[]}",
            "{""a"":3,""l"":[3]}",
            Nothing,
            "{""a"":5,""l"":[5,6,7]}"
        }
        Dim recordColumn As New FeatureVector("record", record)

        recordColumn.attributes = New Dictionary(Of String, String) From {
            {NestedTypeAttribute, "{""type"":""struct"",""fields"":{""a"":{""type"":""int64""},""l"":{""type"":""list"",""element"":{""type"":""int64""}}}}"}
        }

        Call df.add(valuesColumn)
        Call df.add(attrsColumn)
        Call df.add(pointsColumn)
        Call df.add(recordColumn)

        df.rownames = New String() {"n0", "n1", "n2", "n3", "n4"}

        Return df
    End Function

End Module
