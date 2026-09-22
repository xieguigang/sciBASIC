#Region "Microsoft.VisualBasic::ae6437172571488786a80c24bd61b709, Data_science\MachineLearning\MachineLearning\SVM\SVMUtilities.vb"

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

    '   Total Lines: 147
    '    Code Lines: 83 (56.46%)
    ' Comment Lines: 45 (30.61%)
    '    - Xml Docs: 97.78%
    ' 
    '   Blank Lines: 19 (12.93%)
    '     File Size: 6.64 KB


    '     Module SVMUtilities
    ' 
    '         Function: CreateMulticlassProblem, CreateRegressionProblem, CreateTwoClassProblem
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM

    ''' <summary>
    ''' svm demo test
    ''' </summary>
    Public Module SVMUtilities

        Const SCALE As Double = 100

        ''' <summary>
        ''' The random seed value which is used for generating the training data.
        ''' </summary>
        Public Const TRAINING_SEED As Integer = 20080524
        ''' <summary>
        ''' The random seed value which is used for generating the testing data.
        ''' </summary>
        Public Const TESTING_SEED As Integer = 20140407

        ''' <summary>
        ''' Create a randomized two class classification problem for the demo test.
        ''' </summary>
        ''' <param name="count">The total number of the samples.</param>
        ''' <param name="isTraining">
        ''' ``True`` to use the <see cref="TRAINING_SEED"/> random seed, 
        ''' ``False`` to use the <see cref="TESTING_SEED"/> random seed.
        ''' </param>
        ''' <returns>
        ''' A <see cref="Problem"/> object in which the points are located on 
        ''' either side of the vertical axis.
        ''' </returns>
        Public Function CreateTwoClassProblem(count As Integer, Optional isTraining As Boolean = True) As Problem
            Dim prob As New Problem() With {
                .maxIndex = 2
            }
            Dim rand As New Random(If(isTraining, TRAINING_SEED, TESTING_SEED))
            ' create points on either side of the vertical axis
            Dim positive As Integer = CInt(count / 2)
            Dim labels As New List(Of String)()
            Dim data As List(Of Node()) = New List(Of Node())()

            For i = 0 To count - 1
                Dim x As Double = rand.NextDouble() * SCALE + 10
                Dim y As Double = rand.NextDouble() * SCALE - SCALE * 0.5
                x = If(i < positive, x, -x)
                data.Add(New Node() {New Node(1, x), New Node(2, y)})
                labels.Add(If(i < positive, 1, -1))
            Next

            prob.X = data.ToArray()
            prob.Y = labels.ClassEncoder.ToArray()
            Return prob
        End Function

        ''' <summary>
        ''' Create a randomized multiclass classification problem for the demo test.
        ''' The points of each class are located in one of the eight octants of 
        ''' the three dimensional space.
        ''' </summary>
        ''' <param name="numberOfClasses">The number of the target classes, this value should be less than 8.</param>
        ''' <param name="count">The total number of the samples.</param>
        ''' <param name="isTraining">
        ''' ``True`` to use the <see cref="TRAINING_SEED"/> random seed, 
        ''' ``False`` to use the <see cref="TESTING_SEED"/> random seed.
        ''' </param>
        ''' <returns>A <see cref="Problem"/> object which contains the randomized samples.</returns>
        ''' <exception cref="ArgumentException">Thrown when the <paramref name="numberOfClasses"/> value is greater than 8.</exception>
        Public Function CreateMulticlassProblem(numberOfClasses As Integer, count As Integer, Optional isTraining As Boolean = True) As Problem
            If numberOfClasses > 8 Then
                Throw New ArgumentException("Number of classes must be < 8")
            End If

            Dim prob As New Problem() With {
                .maxIndex = 3
            }
            Dim samplesPerClass = New Integer(numberOfClasses - 1) {}
            Dim countPerClass As Integer = count / numberOfClasses
            Dim current = countPerClass

            For i = 1 To samplesPerClass.Length - 1
                samplesPerClass(i) = CInt(current)
                current += countPerClass
                samplesPerClass(i - 1) = samplesPerClass(i) - samplesPerClass(i - 1)
            Next

            samplesPerClass(samplesPerClass.Length - 1) = count - samplesPerClass.Last()

            Dim xSigns = New Integer(7) {-1, 1, 1, -1, -1, 1, 1, -1}
            Dim ySigns = New Integer(7) {1, 1, -1, -1, 1, 1, -1, -1}
            Dim zSigns = New Integer(7) {1, 1, 1, 1, -1, -1, -1, -1}
            Dim rand As Random = New Random(If(isTraining, TRAINING_SEED, TESTING_SEED))
            Dim labels As New List(Of String)()
            Dim data As List(Of Node()) = New List(Of Node())()

            For i = 0 To numberOfClasses - 1

                For j = 0 To samplesPerClass(i) - 1
                    Dim x As Double = rand.NextDouble() * SCALE + 10
                    Dim y As Double = rand.NextDouble() * SCALE + 10
                    Dim z As Double = rand.NextDouble() * SCALE + 10
                    x *= xSigns(i)
                    y *= ySigns(i)
                    z *= zSigns(i)
                    data.Add(New Node() {New Node(1, x), New Node(2, y), New Node(3, z)})
                    labels.Add(i)
                Next
            Next

            prob.X = data.ToArray()
            prob.Y = labels.ClassEncoder.ToArray()

            Return prob
        End Function

        ''' <summary>
        ''' Create a randomized support vector regression problem for the demo 
        ''' test: the label value is calculated as <i>x = 2 * y + z</i>.
        ''' </summary>
        ''' <param name="count">The total number of the samples.</param>
        ''' <param name="isTraining">
        ''' ``True`` to use the <see cref="TRAINING_SEED"/> random seed, 
        ''' ``False`` to use the <see cref="TESTING_SEED"/> random seed.
        ''' </param>
        ''' <returns>A <see cref="Problem"/> object which contains the randomized samples.</returns>
        Public Function CreateRegressionProblem(count As Integer, Optional isTraining As Boolean = True) As Problem
            Dim prob As New Problem() With {
                .maxIndex = 2
            }
            Dim rand As Random = New Random(If(isTraining, TRAINING_SEED, TESTING_SEED))
            Dim labels As New List(Of String)()
            Dim data As List(Of Node()) = New List(Of Node())()

            For i = 0 To count - 1
                Dim y As Double = rand.NextDouble() * 10 - 5
                Dim z As Double = rand.NextDouble() * 10 - 5
                Dim x = 2 * y + z
                data.Add(New Node() {New Node(1, y), New Node(2, z)})
                labels.Add(x)
            Next

            prob.X = data.ToArray()
            prob.Y = labels.ClassEncoder.ToArray()
            Return prob
        End Function
    End Module
End Namespace
