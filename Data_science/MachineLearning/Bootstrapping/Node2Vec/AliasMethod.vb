#Region "Microsoft.VisualBasic::67a51c6fc9d58e07a359066e123a18d9, Data_science\MachineLearning\Bootstrapping\Node2Vec\AliasMethod.vb"

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

    '   Total Lines: 141
    '    Code Lines: 53
    ' Comment Lines: 67
    '   Blank Lines: 21
    '     File Size: 6.40 KB


    '     Class AliasMethod
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [next]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace node2vec

    ' ****************************************************************************
    ' 	 * File: node2vec.AliasMethod.java
    ' 	 * Author: Keith Schwarz (htiek@cs.stanford.edu)
    ' 	 *
    ' 	 * An implementation of the alias method implemented using Vose's algorithm.
    ' 	 * The alias method allows for efficient sampling of random values from a
    ' 	 * discrete probability distribution (i.e. rolling a loaded die) in O(1) time
    ' 	 * each after O(n) preprocessing time.
    ' 	 *
    ' 	 * For a complete writeup on the alias method, including the intuition and
    ' 	 * important proofs, please see the article "Darts, Dice, and Coins: Smpling
    ' 	 * from a Discrete Distribution" at
    ' 	 *
    ' 	 *                 http://www.keithschwarz.com/darts-dice-coins/
    ' 	 

    Public NotInheritable Class AliasMethod

        ' The probability and alias tables. 
        ReadOnly [alias] As Integer()
        ReadOnly probability As Double()

        ''' <summary>
        ''' Constructs a new node2vec.AliasMethod to sample from a discrete distribution and
        ''' hand back outcomes based on the probability distribution.
        ''' <para>
        ''' Given as input a list of probabilities corresponding to outcomes 0, 1,
        ''' ..., n - 1, along with the random number generator that should be used
        ''' as the underlying generator, this constructor creates the probability
        ''' and alias tables needed to efficiently sample from this distribution.
        ''' 
        ''' </para>
        ''' </summary>
        ''' <param name="probabilities"> The list of probabilities. </param>
        Public Sub New(probabilities As IList(Of Double))
            ' Begin by doing basic structural checks on the inputs. 
            If probabilities Is Nothing Then
                Throw New NullReferenceException()
            End If
            If probabilities.Count = 0 Then
                Throw New ArgumentException("Probability vector must be nonempty.")
            End If

            ' Allocate space for the probability and alias tables. 
            probability = New Double(probabilities.Count - 1) {}
            [alias] = New Integer(probabilities.Count - 1) {}

            ' Compute the average probability and cache it for later use. 
            Dim average = 1.0 / probabilities.Count

            ' Make a copy of the probabilities list, since we will be making
            ' 			 * changes to it.
            ' 			 
            probabilities = New List(Of Double)(probabilities)

            ' Create two stacks to act as worklists as we populate the tables. 
            Dim small As List(Of Integer) = New List(Of Integer)()
            Dim large As List(Of Integer) = New List(Of Integer)()

            ' Populate the stacks with the input probabilities. 
            Dim i = 0

            While i < probabilities.Count
                ' If the probability is below the average probability, then we add
                ' 				 * it to the small list; otherwise we add it to the large list.
                ' 				 
                If probabilities(i) >= average Then
                    large.Add(i)
                Else
                    small.Add(i)
                End If

                Threading.Interlocked.Increment(i)
            End While

            ' As a note: in the mathematical specification of the algorithm, we
            ' 			 * will always exhaust the small list before the big list.  However,
            ' 			 * due to floating point inaccuracies, this is not necessarily true.
            ' 			 * Consequently, this inner loop (which tries to pair small and large
            ' 			 * elements) will have to check that both lists aren't empty.
            ' 			 
            While small.Count > 0 AndAlso large.Count > 0
                ' Get the index of the small and the large probabilities. 
                Dim less As Integer = small.Pop()
                Dim more As Integer = large.Pop()

                ' These probabilities have not yet been scaled up to be such that
                ' 				 * 1/n is given weight 1.0.  We do this here instead.
                ' 				 
                probability(less) = probabilities(less) * probabilities.Count
                [alias](less) = more

                ' Decrease the probability of the larger one by the appropriate
                ' 				 * amount.
                ' 				 
                probabilities(more) = probabilities(more) + probabilities(less) - average

                ' If the new probability is less than the average, add it into the
                ' 				 * small list; otherwise add it to the large list.
                ' 				 
                If probabilities(more) >= 1.0 / probabilities.Count Then
                    large.Add(more)
                Else
                    small.Add(more)
                End If
            End While

            ' At this point, everything is in one list, which means that the
            ' 			 * remaining probabilities should all be 1/n.  Based on this, set them
            ' 			 * appropriately.  Due to numerical issues, we can't be sure which
            ' 			 * stack will hold the entries, so we empty both.
            ' 			 
            While small.Count > 0
                probability(small.Pop()) = 1.0
            End While

            While large.Count > 0
                probability(large.Pop()) = 1.0
            End While
        End Sub

        ''' <summary>
        ''' Samples a value from the underlying distribution.
        ''' </summary>
        ''' <returns> A random value sampled from the underlying distribution. </returns>
        Public Function [next]() As Integer
            ' Generate a fair die roll to determine which column to inspect. 
            Dim column = randf.Next(probability.Length)
            ' Generate a biased coin toss to determine which option to pick. 
            Dim coinToss As Boolean = randf.NextDouble() < probability(column)

            ' Based on the outcome, return either the column or its alias. 
            Return If(coinToss, column, [alias](column))
        End Function
    End Class

End Namespace
