Imports System

'
'*****************************************************************************
' Copyright 2015 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace Hierarchy

    Public Class Distance
        Implements IComparable(Of Distance), ICloneable

        Public Property Distance As Double
        Public Property Weight As Double

        Public ReadOnly Property NaN As Boolean
            Get
                Return Double.IsNaN(Distance)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0)
        End Sub

        Public Sub New(distance As Double)
            Me.New(distance, 1.0)
        End Sub

        Public Sub New(distance As Double, weight As Double)
            Me.Distance = distance
            Me.Weight = weight
        End Sub

        Public Function compareTo(distance As Distance) As Integer Implements IComparable(Of Distance).CompareTo
            Return If(distance Is Nothing, 1, Me.Distance.CompareTo(distance.Distance))
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("distance : {0:F2}, weight : {1:F2}", Distance, Weight)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Distance(Distance, Weight)
        End Function
    End Class
End Namespace