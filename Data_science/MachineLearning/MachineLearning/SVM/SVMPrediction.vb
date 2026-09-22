#Region "Microsoft.VisualBasic::7a1a35d34cac77b832b866a03c97d39b, Data_science\MachineLearning\MachineLearning\SVM\SVMPrediction.vb"

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

    '   Total Lines: 42
    '    Code Lines: 12 (28.57%)
    ' Comment Lines: 25 (59.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.90%)
    '     File Size: 1.55 KB


    '     Structure SVMPrediction
    ' 
    '         Properties: [class], score, unifyValue, vote
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    ''' <summary>
    ''' The prediction result of a support vector machine model.
    ''' </summary>
    Public Structure SVMPrediction

        ''' <summary>
        ''' The predicted class label.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/> class index.</returns>
        Public Property [class] As Integer
        ''' <summary>
        ''' The decision value of the predicted class.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property score As Double
        ''' <summary>
        ''' The unified decision value, which is the sum of the pairwise 
        ''' decision values of the predicted class.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property unifyValue As Double
        ''' <summary>
        ''' The vote value of each class, which is produced by the one-versus-one 
        ''' voting procedure of the multi-class classifier.
        ''' </summary>
        ''' <returns>An array of the vote value of each class.</returns>
        Public Property vote As Double()

        ''' <summary>
        ''' Display this prediction result as a json string.
        ''' </summary>
        ''' <returns>A json text which describes this prediction result.</returns>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Structure
End Namespace
