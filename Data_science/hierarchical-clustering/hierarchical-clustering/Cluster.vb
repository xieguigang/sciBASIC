Imports System
Imports System.Collections.Generic

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' <p/>
' http://www.apache.org/licenses/LICENSE-2.0
' <p/>
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace com.apporiented.algorithm.clustering


    Public Class Cluster

        Public Sub New(name As String)
            Me.Name = name
            LeafNames = New List(Of String)
        End Sub

        Public Property Distance As Distance

        Public ReadOnly Property WeightValue As Double
            Get
                Return Distance.Weight
            End Get
        End Property

        Public ReadOnly Property DistanceValue As Double
            Get
                Return Distance.Distance
            End Get
        End Property

        Public Property Children As IList(Of Cluster)

        Public Sub addLeafName(lname As String)
            LeafNames.Add(lname)
        End Sub

        Public Sub appendLeafNames(lnames As IList(Of String))
            LeafNames.AddRange(lnames)
        End Sub

        Public ReadOnly Property LeafNames As List(Of String)
        Public Property Parent As Cluster
        Public Property Name As String

        Public Sub addChild(cluster As Cluster)
            Children.Add(cluster)
        End Sub

        Public Function contains(cluster As Cluster) As Boolean
            Return Children.Contains(cluster)
        End Function

        Public Overrides Function ToString() As String
            Return "Cluster " & Name
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then Return True
            If obj Is Nothing Then Return False
            If Me.GetType() IsNot obj.GetType() Then Return False
            Dim other As Cluster = CType(obj, Cluster)
            If Name Is Nothing Then
                If other.Name IsNot Nothing Then Return False
            ElseIf Not Name.Equals(other.Name) Then
                Return False
            End If
            Return True
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return If(Name Is Nothing, 0, Name.GetHashCode())
        End Function

        Public ReadOnly Property Leaf As Boolean
            Get
                Return Children.Count = 0
            End Get
        End Property

        Public Function countLeafs() As Integer
            Return countLeafs(Me, 0)
        End Function

        Public Function countLeafs(node As Cluster, count As Integer) As Integer
            If node.Leaf Then count += 1
            For Each child As Cluster In node.Children
                count += child.countLeafs()
            Next child
            Return count
        End Function

        Public Sub toConsole(indent As Integer)
            For i As Integer = 0 To indent - 1
                Console.Write("  ")
            Next i
            Dim ___name As String = Name + (If(Leaf, " (leaf)", "")) + (If(Distance IsNot Nothing, "  distance: " & Distance.ToString, ""))
            Console.WriteLine(___name)
            For Each child As Cluster In Children
                child.toConsole(indent + 1)
            Next child
        End Sub

        Public ReadOnly Property TotalDistance As Double
            Get
                Dim dist As Double = If(Distance Is Nothing, 0, Distance.Distance)
                If Children.Count > 0 Then dist += Children(0).TotalDistance
                Return dist
            End Get
        End Property
    End Class

End Namespace