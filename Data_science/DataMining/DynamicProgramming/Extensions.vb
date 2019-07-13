#Region "Microsoft.VisualBasic::0c0946e47aaace3e949e6a982af0e90b, Data_science\DataMining\DynamicProgramming\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: PopulateAlignments
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch

Public Module Extensions

    ''' <summary>
    ''' This funktion provide a easy way to write a computed alignment into a fasta file
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="nw"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PopulateAlignments(Of T)(nw As Workspace(Of T)) As IEnumerable(Of GlobalAlign(Of T))
        For i As Integer = 0 To nw.NumberOfAlignments - 1
            Yield New GlobalAlign(Of T) With {
                .Score = nw.Score,
                .query = nw.getAligned1(i),
                .subject = nw.getAligned2(i)
            }
        Next
    End Function
End Module
