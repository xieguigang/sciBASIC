#Region "Microsoft.VisualBasic::7fa7aa666a937a4103fee03934ff74d9, ..\visualbasic_App\CLI_tools\Table\CLI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Linq

Public Module CLI

    <ExportAPI("/Selects", Usage:="/Selects /in <in.Csv> /index <Name> /list <list.key.Csv> [/out <out.Csv>]")>
    Public Function Selects(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim index As String = args.GetValue("/index", "Name")
        Dim list As String = args("/list")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & "." & list.BaseName & ".Csv")
        Dim maps As New Dictionary(Of String, String) From {{index, NameOf(EntityObject.Identifier)}}
        Dim source As EntityObject() = [in].LoadCsv(Of EntityObject)(maps:=maps)
        Dim keys As String() = list.ReadAllLines
        source = source.Where(Function(x) Array.IndexOf(keys, x.Identifier) > -1).ToArray
        Return source.SaveTo(out).CLICode
    End Function

    <ExportAPI("/Associate", Usage:="/Associate /in <in.Csv> /index <Name> /list <list.Csv> /key <Key> /out <out.Csv> /expand")>
    Public Function Associate(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim index As String = args.GetValue("/index", "Name")
        Dim list As String = args("/list")
        Dim listKey As String = args.GetValue("/key", "Key")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & "." & list.BaseName & ".Csv")
        Dim maps As New Dictionary(Of String, String) From {{index, NameOf(EntityObject.Identifier)}}
        Dim source As EntityObject() = [in].LoadCsv(Of EntityObject)(maps:=maps)
        Dim expand As Boolean = args.GetBoolean("/expand")
        maps = New Dictionary(Of String, String) From {{listKey, NameOf(EntityObject.Identifier)}}
        Dim keys As Dictionary(Of String, EntityObject()) =
            list.LoadCsv(Of EntityObject)(maps:=maps) _
                .GroupBy(Function(x) x.Identifier) _
                .ToDictionary(Function(g) g.First.Identifier,
                              Function(g) g.ToArray)

        Dim result As New List(Of EntityObject)

        For Each x As EntityObject In source
            If keys.ContainsKey(x.Identifier) Then
                For Each i In keys(x.Identifier).SeqIterator
                    If expand Then
                        Dim copy As EntityObject = New EntityObject With {
                            .Identifier = x.Identifier,
                            .Properties = New Dictionary(Of String, String)(x.Properties)
                        }
                        For Each p In i.obj.Properties
                            copy.Properties.Add(p.Key, p.Value)
                        Next
                        result += copy
                    Else
                        For Each p In i.obj.Properties
                            x.Properties.Add(p.Key & i.i, p.Value)
                        Next
                    End If
                Next

                If Not expand Then
                    result += x
                End If
            End If
        Next

        Return result.SaveTo(out).CLICode
    End Function
End Module
