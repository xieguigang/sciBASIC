#Region "Microsoft.VisualBasic::75faff2f8409ee57417268200cc341c4, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\PrintHelper.vb"

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

    '   Total Lines: 49
    '    Code Lines: 32
    ' Comment Lines: 9
    '   Blank Lines: 8
    '     File Size: 1.63 KB


    ' Module PrintHelper
    ' 
    '     Function: ToConsoleLine
    ' 
    '     Sub: consoleLine, Print
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

Public Module PrintHelper

    ''' <summary>
    ''' Helper for print the cluster tree
    ''' </summary>
    ''' <param name="cluster"></param>
    ''' <param name="out">
    ''' If this output pointer is nothing, then by default is print onto the <see cref="Console.OpenStandardOutput"/>
    ''' </param>
    <Extension>
    Public Sub Print(cluster As Cluster, Optional out As StreamWriter = Nothing)

        With out Or New StreamWriter(Console.OpenStandardOutput).AsDefault
            Call .WriteLine(cluster.ToConsoleLine(indent:=Scan0))
            Call .Flush()
        End With

    End Sub

    <Extension>
    Public Function ToConsoleLine(c As Cluster, Optional indent% = Scan0) As String
        Dim sb As New StringBuilder
        Call c.consoleLine(sb, indent)
        Return sb.ToString
    End Function

    <Extension>
    Private Sub consoleLine(c As Cluster, sb As StringBuilder, indent%)
        For i As Integer = 0 To indent - 1
            Call sb.Append("  ")
        Next

        With c
            ' 获取当前的cluster的显示文本
            Dim name$ = .Name & (If(.isLeaf, " (leaf)", "")) & (If(.Distance IsNot Nothing, "  " & .Distance.ToString, ""))
            Call sb.AppendLine(name)

            ' 然后递归的将所有子节点的文本也生成出来
            For Each child As Cluster In .Children
                Call child.consoleLine(sb, indent + 1)
            Next
        End With
    End Sub
End Module
