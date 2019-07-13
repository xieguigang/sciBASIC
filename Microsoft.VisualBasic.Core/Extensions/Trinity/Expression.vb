#Region "Microsoft.VisualBasic::252bfaa89ee13252e7c43a4b3ae4f611, Microsoft.VisualBasic.Core\Extensions\Trinity\Expression.vb"

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

    '     Module Expression
    ' 
    '         Function: Concatenate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Data.Trinity

    ''' <summary>
    ''' Natural expression builder for AI output
    ''' </summary>
    Public Module Expression

        ''' <summary>
        ''' If <paramref name="list"/> contains no elements or it is nothing, then this function will returns nothing
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="comma$"></param>
        ''' <param name="andalso$"></param>
        ''' <param name="etc$"></param>
        ''' <param name="joinSpace"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Concatenate(list As IEnumerable(Of String), Optional comma$ = ",", Optional andalso$ = "and", Optional etc$ = "etc", Optional joinSpace As Boolean = True) As String
            Dim space As String = "" Or " ".When(joinSpace)
            Dim dataArray As String() = list.SafeQuery.ToArray

            If dataArray.Length = 0 Then
                Return Nothing
            End If

            With dataArray
                If .Length = 1 Then
                    Return .ByRef(0)
                ElseIf .Length < 8 Then
                    Return .Take(.Length - 1).JoinBy(comma & space) & $"{space}{[andalso]}{space}" & .Last
                Else
                    Return .Take(7).JoinBy(comma & space) & $"{space}{[andalso]}{space}" & .ByRef(7) & $"{comma}{space}{etc}"
                End If
            End With
        End Function
    End Module
End Namespace
