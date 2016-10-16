Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer.Prefix
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CommandLine

    Public Class Grouping

        Public Class Groups : Inherits GroupingAttribute

            ''' <summary>
            ''' 这个分组之中的API列表
            ''' </summary>
            ''' <returns></returns>
            Public Property Data As APIEntryPoint()

            Public Sub New(attr As GroupingAttribute)
                MyBase.New(attr.Name)

                Description = attr.Description
            End Sub
        End Class

        Public Property GroupData As Dictionary(Of String, Groups)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CLI">
        ''' 主要是需要从这个类型定义之中得到<see cref="GroupingAttribute"/>数据
        ''' </param>
        Sub New(CLI As Interpreter)
            Dim type As Type = CLI.Type
            Dim gs = From x As Object
                     In type.GetCustomAttributes(GetType(GroupingAttribute), True)
                     Select DirectCast(x, GroupingAttribute)
            Dim api = (From x As APIEntryPoint
                       In CLI.APIList
                       Let g As Group() = x.EntryPoint _
                           .GetCustomAttributes(GetType(Group), True) _
                           .ToArray(Function(o) DirectCast(o, Group))
                       Select If(g.Length = 0, {New Group(undefined)}, g) _
                           .Select(Function(gx) New With {gx, x})) _
                           .MatrixAsIterator _
                           .GroupBy(Function(x) x.gx.Name) _
                           .ToDictionary(Function(x) x.Key,
                                         Function(x) x.ToArray(
                                         Function(o) o.x))

            GroupData = New Dictionary(Of String, Groups)

            For Each g As GroupingAttribute In gs
                If api.ContainsKey(g.Name) Then
                    Dim apiList As APIEntryPoint() = api(g.Name)

                    Call GroupData.Add(
                        g.Name, New Groups(g) With {
                            .Data = apiList
                        })
                    Call api.Remove(g.Name)
                Else
#If DEBUG Then
                    Call $"No data found for grouping {g.GetJson}".Warning
#End If
                End If
            Next

            If api.Count > 0 Then
                For Each g In api
                    Dim gK As New GroupingAttribute(g.Key)

                    Call GroupData.Add(
                        g.Key, New Groups(gK) With {
                            .Data = g.Value
                        })
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return GroupData.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace