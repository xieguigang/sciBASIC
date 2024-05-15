#Region "Microsoft.VisualBasic::99cd40f5ce68d259dc1ed4697b33d09d, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Grouping.vb"

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

    '   Total Lines: 101
    '    Code Lines: 74
    ' Comment Lines: 10
    '   Blank Lines: 17
    '     File Size: 3.90 KB


    '     Class Grouping
    ' 
    '         Properties: GroupData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, ToString
    '         Class Groups
    ' 
    '             Properties: Data
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CommandLine

    Friend Class Grouping : Implements IEnumerable(Of Groups)

        Public Class Groups : Inherits GroupingDefineAttribute

            ''' <summary>
            ''' 这个分组之中的API列表
            ''' </summary>
            ''' <returns></returns>
            Public Property Data As APIEntryPoint()

            Public Sub New(attr As GroupingDefineAttribute)
                MyBase.New(attr.Name)

                Description = attr.Description
            End Sub
        End Class

        Public Property GroupData As Dictionary(Of String, Groups)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CLI">
        ''' 主要是需要从这个类型定义之中得到<see cref="GroupingDefineAttribute"/>数据
        ''' </param>
        Sub New(CLI As Interpreter)
            Dim type As Type = CLI.Type
            Dim gs = From x As Object
                     In type.GetCustomAttributes(GetType(GroupingDefineAttribute), True)
                     Select DirectCast(x, GroupingDefineAttribute)
            Dim api = (From x As APIEntryPoint
                       In CLI.APIList
                       Let g As GroupAttribute() = x.EntryPoint _
                           .GetCustomAttributes(GetType(GroupAttribute), True) _
                           .Select(Function(o) DirectCast(o, GroupAttribute)) _
                           .ToArray
                       Select If(g.Length = 0, {New GroupAttribute(JavaScriptObject.undefined)}, g) _
                           .Select(Function(gx) New With {gx, x})) _
                           .IteratesALL _
                           .GroupBy(Function(x) x.gx.Name) _
                           .ToDictionary(Function(x) x.Key,
                                         Function(x)
                                             Return x.Select(Function(o) o.x).ToArray
                                         End Function)

            GroupData = New Dictionary(Of String, Groups)

            For Each g As GroupingDefineAttribute In gs
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
                    Dim gK As New GroupingDefineAttribute(g.Key)

                    Call GroupData.Add(
                        g.Key, New Groups(gK) With {
                            .Data = g.Value
                        })
                Next

                Call GroupData.SortByKey
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return GroupData.Keys.ToArray.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Groups) Implements IEnumerable(Of Groups).GetEnumerator
            For Each x In GroupData.Values
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
