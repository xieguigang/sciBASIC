#Region "Microsoft.VisualBasic::699d191daf55f0dbc4851fc04ed40a8f, Data_science\MachineLearning\MachineLearning\SVM\Models\Problem.vb"

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

    '   Total Lines: 95
    '    Code Lines: 33
    ' Comment Lines: 49
    '   Blank Lines: 13
    '     File Size: 3.34 KB


    '     Class Problem
    ' 
    '         Properties: count, dimensionNames, maxIndex, X, Y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    ''' <summary>
    ''' Encapsulates a problem, or set of vectors which must be classified.
    ''' </summary>
    <Serializable> Public Class Problem

        ''' <summary>
        ''' Number of vectors.
        ''' </summary>
        Public ReadOnly Property count As Integer
            Get
                Return X.Length
            End Get
        End Property

        ''' <summary>
        ''' Class labels.
        ''' </summary>
        Public Property Y As ColorClass()

        ''' <summary>
        ''' Vector data.
        ''' </summary>
        Public Property X As Node()()

        ''' <summary>
        ''' Maximum index for a vector. this value is the width of each 
        ''' row in <see cref="X"/> and equals to the length of vector 
        ''' <see cref="dimensionNames"/> 
        ''' </summary>
        Public Property maxIndex As Integer

        ''' <summary>
        ''' the width of each row in <see cref="X"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionNames As String()

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="y">The class labels</param>
        ''' <param name="x">Vector data.</param>
        ''' <param name="maxIndex">Maximum index for a vector</param>
        Public Sub New(y As String(), x As Node()(), maxIndex As Integer)
            Me.Y = y.ClassEncoder.ToArray
            Me.X = x
            Me.maxIndex = maxIndex
        End Sub

        ''' <summary>
        ''' Empty Constructor. 
        ''' </summary>
        ''' <remarks>
        ''' Nothing is initialized.
        ''' </remarks>
        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"dim {dimensionNames.GetJson}, {Y.Length} labels = {Y.Distinct.GetJson}"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other As Problem = TryCast(obj, Problem)
            If other Is Nothing Then Return False
            Return other.count = count AndAlso other.maxIndex = maxIndex AndAlso other.X.IsEqual(X) AndAlso other.Y.IsEqual(Y)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return count.GetHashCode() + maxIndex.GetHashCode() + X.ComputeHashcode2() + Y.ComputeHashcode()
        End Function
    End Class
End Namespace
