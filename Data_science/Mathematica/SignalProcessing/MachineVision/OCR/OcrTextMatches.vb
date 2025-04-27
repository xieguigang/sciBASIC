Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Make text matches of the ocr text data
''' </summary>
Public Module OcrTextMatches

    ''' <summary>
    ''' Get text matches score of the ocr text data
    ''' </summary>
    ''' <param name="group"></param>
    ''' <param name="target"></param>
    ''' <param name="confusion"></param>
    ''' <returns>
    ''' the text similarity matches score, higher is better. The score is calculated by Levenshtein distance, 
    ''' which is a measure of the similarity between two strings.
    ''' </returns>
    ''' <remarks>
    ''' The score is calculated by Levenshtein distance, which is a measure of the similarity between two strings.
    ''' The distance is the number of single-character edits (insertions, deletions, or substitutions) required to change one string into the other.
    ''' </remarks>
    ''' <seealso cref="Similarity.LevenshteinEvaluate(String, String, Func(Of Char, Char, Boolean))"/>
    ''' <seealso cref="ConfusionChars.Check(Char, Char)"/>
    <Extension>
    Public Iterator Function MatchGroup(group As Trajectory, target As String, Optional confusion As ConfusionChars = Nothing) As IEnumerable(Of Double)
        confusion = If(confusion, ConfusionChars.CreateDefaultMatrix)

        For Each frame As Detection In group.objectSet
            Yield Similarity.LevenshteinEvaluate(target, frame.ObjectID, checkChar:=AddressOf confusion.Check)
        Next
    End Function

End Module
