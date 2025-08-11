#Region "Microsoft.VisualBasic::4232f8e1d3cbfafc89b5e7edb8dceda6, Data_science\Mathematica\SignalProcessing\MachineVision\OCR\OcrTextMatches.vb"

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

    '   Total Lines: 34
    '    Code Lines: 11 (32.35%)
    ' Comment Lines: 19 (55.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 1.54 KB


    ' Module OcrTextMatches
    ' 
    '     Function: MatchGroup
    ' 
    ' /********************************************************************************/

#End Region

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
    ''' <seealso cref="Similarity.LevenshteinEvaluate"/>
    ''' <seealso cref="ConfusionChars.Check(Char, Char)"/>
    <Extension>
    Public Iterator Function MatchGroup(group As Trajectory, target As String, Optional confusion As ConfusionChars = Nothing) As IEnumerable(Of Double)
        confusion = If(confusion, ConfusionChars.CreateDefaultMatrix)

        For Each frame As Detection In group.objectSet
            Yield Similarity.LevenshteinEvaluate(target, frame.ObjectID, checkChar:=AddressOf confusion.Check)
        Next
    End Function

End Module
