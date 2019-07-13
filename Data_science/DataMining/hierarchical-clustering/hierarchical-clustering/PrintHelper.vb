#Region "Microsoft.VisualBasic::eaffc540afecb371890fe930727abc5f, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\PrintHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module PrintHelper
    ' 
    '     Function: ToConsoleLine
    ' 
    '     Sub: __consoleLine, Print
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
    <Extension> Public Sub Print(cluster As Cluster, Optional out As StreamWriter = Nothing)

        With out Or New StreamWriter(Console.OpenStandardOutput).AsDefault
            Call .WriteLine(cluster.ToConsoleLine(indent:=Scan0))
            Call .Flush()
        End With

    End Sub

    <Extension>
    Private Function ToConsoleLine(c As Cluster, Optional indent% = Scan0) As String
        Dim sb As New StringBuilder
        Call c.__consoleLine(sb, indent)
        Return sb.ToString
    End Function

    <Extension>
    Private Sub __consoleLine(c As Cluster, sb As StringBuilder, indent%)
        For i As Integer = 0 To indent - 1
            Call sb.Append("  ")
        Next

        With c
            ' 获取当前的cluster的显示文本
            Dim name$ = .Name & (If(.Leaf, " (leaf)", "")) & (If(.Distance IsNot Nothing, "  distance: " & .Distance.ToString, ""))
            Call sb.AppendLine(name)

            ' 然后递归的将所有子节点的文本也生成出来
            For Each child As Cluster In .Children
                Call child.__consoleLine(sb, indent + 1)
            Next
        End With
    End Sub
End Module
