#Region "Microsoft.VisualBasic::b221ccc3fc894d61968ceff2d266cfee, Data_science\Mathematica\Math\Gibbs\Gibbs.vb"

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

    '   Total Lines: 195
    '    Code Lines: 115 (58.97%)
    ' Comment Lines: 59 (30.26%)
    '    - Xml Docs: 86.44%
    ' 
    '   Blank Lines: 21 (10.77%)
    '     File Size: 7.80 KB


    ' Class Gibbs
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: calculateP, calculateQ, generateRandomValue, PQ, sample
    ' 
    ' /********************************************************************************/

#End Region

Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' Gibb's Sampling Steps:<br/>
''' 
''' 1. Set every variable to a random value.<br/>
''' 2. Choose a variable to update. <br/>
''' 3. Randomly Select (aka "Sample") a new value for the variable based on the
'''    current conditions. <br/>
''' 4. Repeat from Step 2.
''' </summary>  
Public Class Gibbs

    Dim sequences As String()
    Dim motifLength As Integer

    ''' <summary>
    ''' Constructs and performs Gibb's Sampling in order to find repeated motifs.
    ''' </summary>
    ''' <param name="seqArray">
    '''            A String array of the sequences that will be used. </param>
    ''' <param name="motifLength">
    '''            An Integer that shows the length of the motif or pattern we
    '''            are trying to find, this value is given. </param>
    Public Sub New(seqArray As String(), motifLength As Integer)
        Me.sequences = seqArray
        Me.motifLength = motifLength
    End Sub

    ''' <summary>
    ''' This method is repeated 2000 times.
    ''' </summary>
    Public Function sample(Optional MAXIT As Integer = 1999) As Score()
        Dim rand As Random = randf2.seeds
        Dim start = generateRandomValue()
        Dim scores As Double()

        For j As Integer = 0 To MAXIT
            Dim chosenSeqIndex = rand.Next(sequences.Length)
            Dim chosenSequence As String = sequences(chosenSeqIndex)
            Dim w = chosenSequence.Length - motifLength
            Dim qv As Double() = New Double(w) {}
            Dim pv As Double() = New Double(w) {}

            ' i = possibleStart
            For i As Integer = 0 To w
                Dim tempMotif = chosenSequence.Substring(i, motifLength)
                Dim p = calculateP(tempMotif, chosenSeqIndex)
                Dim q = calculateQ(tempMotif, chosenSeqIndex, i)

                qv(i) = q
                pv(i) = p
            Next

            scores = SIMD.Divide.f64_op_divide_f64(qv, pv)
            scores = SIMD.Divide.f64_op_divide_f64_scalar(scores, scores.Sum)

            Dim random As Double = rand.NextDouble()
            Dim dubsum As Double = 0

            For Each d As Double In scores
                dubsum += d

                If random < dubsum Then
                    'start(chosenSequence).start = scores.IndexOf(d)
                    'start(chosenSequence).p = pv
                    'start(chosenSequence).q = qv
                    'start(chosenSequence).len = motifLength

                    Exit For
                End If
            Next
        Next

        Return start.Values.ToArray
    End Function

    Public Function PQ(chosenSeqIndex As Integer) As (p As Double(), q As Double())
        Dim chosenSequence As String = sequences(chosenSeqIndex)
        Dim w = chosenSequence.Length - motifLength
        Dim qv As Double() = New Double(w) {}
        Dim pv As Double() = New Double(w) {}

        ' i = possibleStart
        For i As Integer = 0 To w
            Dim tempMotif = chosenSequence.Substring(i, motifLength)
            Dim p = calculateP(tempMotif, chosenSeqIndex)
            Dim q = calculateQ(tempMotif, chosenSeqIndex, i)

            qv(i) = q
            pv(i) = p
        Next

        Return (pv, qv)
    End Function

    ''' <summary>
    ''' Calculates the probability of a letter in this position.
    ''' </summary>
    ''' <param name="tempMotif">
    '''            The motif being used for this calculation. </param>
    ''' <param name="chosenSeqIndex">
    '''            The index of the sequence being used for this calculation,
    '''            useful for skipping all of this sequences calculations and
    '''            focusing on the other ones. </param>
    ''' <returns> A double of the probability of a letter in this position. </returns>
    Public Function calculateQ(tempMotif As String,
                                chosenSeqIndex As Integer,
                                possibleStart As Integer) As Double
        Dim q As Double = 1
        Dim start = possibleStart
        Dim [end] = possibleStart + tempMotif.Length
        Dim denominator As Double = sequences.Length - 1
        For Each s As String In sequences
            Dim numerator As Double = 0
            If s = sequences(chosenSeqIndex) Then
                Continue For
            End If
            If [end] > s.Length Then
                q *= 0.01
                Continue For
            End If
            Dim thisMotif = s.Substring(start, [end] - start)
            Dim letters As Char() = tempMotif.ToCharArray()
            Dim seqLetters As Char() = thisMotif.ToCharArray()

            For i = 0 To tempMotif.Length - 1
                If letters(i) = seqLetters(i) Then
                    numerator += 1
                End If
            Next
            If numerator = 0 Then
                q *= 0.01
            Else
                q *= numerator / denominator
            End If
        Next
        Return q
    End Function

    ''' <summary>
    ''' Calculates the probability of a letter randomly selected.
    ''' 
    ''' To find this value, the method loops through each letter of the selected
    ''' temporary motif, and loops through the other sequences. While looping
    ''' through the other sequences, we find the amount of same letters in each
    ''' other sequence, along with the total length of all other sequences. The
    ''' value P is a product of every result, each result being the amount of
    ''' letters of the same kind over the total amount of letters.
    ''' </summary>
    ''' <param name="tempMotif">
    '''            The motif being used for this calculation. </param>
    ''' <param name="chosenSeqIndex">
    '''            The index of the sequence being used for this calculation,
    '''            useful for skipping all of this sequences calculations and
    '''            focusing on the other ones. </param>
    ''' <returns> A double of the probability of a letter randomly selected. </returns>
    Public Function calculateP(tempMotif As String, chosenSeqIndex As Integer) As Double
        Dim p As Double = 1
        For Each c As Char In tempMotif.ToCharArray()
            Dim sameLetters As Double = 0
            Dim totalLength As Double = 0
            For Each s As String In sequences
                If s = sequences(chosenSeqIndex) Then
                    Continue For
                End If
                Dim seqLetters As Char() = s.ToCharArray()
                For Each x As Char In seqLetters
                    If c = x Then
                        sameLetters += 1
                    End If
                Next
                totalLength += s.Length
            Next
            p *= sameLetters / totalLength
        Next
        Return p
    End Function

    ''' <summary>
    ''' Calculates and stores every random value. Generates a random from 0 to a
    ''' value of each individual sequences length subtracted by the motif length.
    ''' </summary>
    ''' <returns> A HashTable containing the sequence as a key, and the random
    '''         integer to be used as the value. </returns>
    Private Function generateRandomValue() As Dictionary(Of String, Score)
        Dim rand As Random = randf2.seeds
        Dim randomValues As New Dictionary(Of String, Score)()
        For Each seq As String In sequences
            Dim randomVal = rand.Next(seq.Length - motifLength)
            randomValues(seq) = New Score With {.start = randomVal, .seq = seq}
        Next
        Return randomValues
    End Function
End Class
