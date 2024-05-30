#Region "Microsoft.VisualBasic::6ff2706777195dec54f86dbec9b542d1, Microsoft.VisualBasic.Core\test\test\group_test.vb"

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

    '   Total Lines: 27
    '    Code Lines: 18 (66.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (33.33%)
    '     File Size: 879 B


    ' Module group_test
    ' 
    '     Sub: RunGroup
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Module group_test

    Sub RunGroup()
        Dim asc_chars = ASCII.AlphaNumericTable.Keys.Select(Function(c) c.ToString).ToArray
        Dim chars = Enumerable.Range(0, 700000).Select(Function(a) randf.Next(asc_chars)).ToArray

        Dim groupBy = chars.GroupBy(Function(s) s).ToDictionary(Function(s) s.Key, Function(s) s.Count)
        Dim dict_group As New Dictionary(Of String, List(Of String))

        For Each c As String In chars
            If Not dict_group.ContainsKey(c) Then
                dict_group.Add(c, New List(Of String))
            End If

            dict_group(c).Add(c)
        Next

        Dim groupByDict = dict_group.ToDictionary(Function(c) c.Key, Function(c) c.Value.Count)


        Pause()
    End Sub

End Module
