Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Mathematical.Analysis

    Public Interface ITokenCount
        Property Token As String
        Property Id As Integer
        Property Count As Integer
    End Interface

    Public Interface ILink
        Property source As Integer
        Property target As Integer
        Property Count As Integer
    End Interface

    Public Module TokenConnections

        Public Sub Analysis(Of T, Tnode As ITokenCount)(
                               data As IEnumerable(Of T),
                           getValue As Func(Of T, String),
                            nodeNew As Func(Of String, Integer, Tnode),
                        ByRef nodes As Dictionary(Of String, Tnode),
                   Optional ignores As String() = Nothing)

            If ignores Is Nothing Then
                ignores = {}
            Else
                ignores = ignores _
                .Where(Function(s) Not s.IsBlank) _
                .ToArray(Function(s) s.ToLower)
            End If

            For Each x As T In data
                Dim tokens As String() = getValue(x).ToLower.Split
                tokens = tokens.Where( ' 前面已经用ToLower转换为小写了，所以在这里直接使用indexof来判断
                Function(s) Array.IndexOf(ignores, s) = -1 AndAlso
                    Regex.Match(s, "\d+:?").Value <> s).ToArray

                For Each s As String In tokens
                    If Not nodes.ContainsKey(s) Then
                        nodes(s) = nodeNew(s, nodes.Count + 1)
                    End If
                    nodes(s).Count += 1
                Next
            Next
        End Sub

        Public Sub Analysis(Of T, Tnode As ITokenCount,
                              Tlink As ILink)(
                               data As IEnumerable(Of T),
                           getValue As Func(Of T, String),
                            nodeNew As Func(Of String, Integer, Tnode),
                        ByRef nodes As Dictionary(Of String, Tnode),
                   Optional linkNew As Func(Of Integer, Integer, Tlink) = Nothing,
                   Optional ByRef links As Dictionary(Of String, Tlink) = Nothing,
                   Optional ignores As String() = Nothing)

            If ignores Is Nothing Then
                ignores = {}
            Else
                ignores = ignores _
                .Where(Function(s) Not s.IsBlank) _
                .ToArray(Function(s) s.ToLower)
            End If

            For Each x As T In data
                Dim tokens As String() = getValue(x).ToLower.Split
                tokens = tokens.Where( ' 前面已经用ToLower转换为小写了，所以在这里直接使用indexof来判断
                Function(s) Array.IndexOf(ignores, s) = -1 AndAlso
                    Regex.Match(s, "\d+:?").Value <> s).ToArray

                For Each s As String In tokens
                    If Not nodes.ContainsKey(s) Then
                        nodes(s) = nodeNew(s, nodes.Count + 1)
                    End If
                    nodes(s).Count += 1
                Next

                If linkNew Is Nothing Then
                    Continue For
                End If

                For Each s As String In tokens
                    For Each tt As String In tokens

                        If s = tt Then
                            Continue For ' 自己和自己不需要被统计
                        End If

                        Dim o As String = {s, tt}.OrderBy(Function(ss) ss).JoinBy(" --> ")
                        If Not links.ContainsKey(o) Then
                            links(o) = linkNew(nodes(s).Id, nodes(tt).Id)
                        End If
                        links(o).Count += 1
                    Next
                Next
            Next
        End Sub
    End Module
End Namespace