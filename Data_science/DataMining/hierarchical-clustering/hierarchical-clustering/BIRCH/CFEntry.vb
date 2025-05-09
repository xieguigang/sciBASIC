Imports System.Text
Imports std = System.Math

' 
'   This file is part of JBIRCH.
' 
'   JBIRCH is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
' 
'   JBIRCH is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
' 
'   You should have received a copy of the GNU General Public License
'   along with JBIRCH.  If not, see <http://www.gnu.org/licenses/>.
' 
' 

' 
'   CFNode.java
'   Copyright (C) 2009 Roberto Perdisci (roberto.perdisci@gmail.com)
' 

Namespace BIRCH

    ''' 
    ''' <summary>
    ''' @author Roberto Perdisci (roberto.perdisci@gmail.com)
    ''' 
    ''' </summary>
    Public Class CFEntry

        Private Shared ReadOnly LINE_SEP As String = vbLf

        Private n As Integer = 0 ' number of patterns summarized by this entry
        Private sumX As Double() = Nothing
        Private sumX2 As Double() = Nothing
        Private childField As CFNode = Nothing
        Private indexListField As List(Of Integer) = Nothing
        Private subclusterIDField As Integer = -1 ' the unique id the describes a subcluster (valid only for leaf entries)

        Public Sub New()
        End Sub

        Public Sub New(x As Double())
            Me.New(x, 0)
        End Sub

        Public Sub New(x As Double(), index As Integer)
            n = 1

            sumX = New Double(x.Length - 1) {}
            For i = 0 To sumX.Length - 1
                sumX(i) = x(i)
            Next

            sumX2 = New Double(x.Length - 1) {}
            For i = 0 To sumX2.Length - 1
                sumX2(i) = x(i) * x(i)
            Next

            indexListField = New List(Of Integer)()
            indexListField.Add(index)
        End Sub

        ''' <summary>
        ''' This makes a deep copy of the CFEntry e.
        ''' WARNING: we do not make a deep copy of the child!!!
        ''' </summary>
        ''' <param name="e"> the entry to be cloned </param>
        Public Sub New(e As CFEntry)
            n = e.n
            sumX = CType(e.sumX.Clone(), Double())
            sumX2 = CType(e.sumX2.Clone(), Double())
            childField = e.childField ' WARNING: we do not make a deep copy of the child!!!
            indexListField = New List(Of Integer)()
            For Each i In e.IndexList ' this makes sure we get a deep copy of the indexList
                indexListField.Add(i)
            Next
        End Sub

        Protected Friend Overridable ReadOnly Property IndexList As List(Of Integer)
            Get
                Return indexListField
            End Get
        End Property

        Protected Friend Overridable Function hasChild() As Boolean
            Return childField IsNot Nothing
        End Function

        Protected Friend Overridable Property Child As CFNode
            Get
                Return childField
            End Get
            Set(value As CFNode)
                childField = value
                indexListField = Nothing ' we don't keep this if this becomes a non-leaf entry
            End Set
        End Property

        Protected Friend Overridable ReadOnly Property ChildSize As Integer
            Get
                Return childField.Entries.Count
            End Get
        End Property


        Protected Friend Overridable Property SubclusterID As Integer
            Set(value As Integer)
                subclusterIDField = value
            End Set
            Get
                Return subclusterIDField
            End Get
        End Property


        Protected Friend Overridable Sub update(e As CFEntry)
            n += e.n

            If sumX Is Nothing Then
                sumX = CType(e.sumX.Clone(), Double())
            Else
                For i = 0 To sumX.Length - 1
                    sumX(i) += e.sumX(i)
                Next
            End If

            If sumX2 Is Nothing Then
                sumX2 = CType(e.sumX2.Clone(), Double())
            Else
                For i = 0 To sumX2.Length - 1
                    sumX2(i) += e.sumX2(i)
                Next
            End If

            If Not hasChild() Then ' we keep indexList only if we are at a leaf
                If indexListField IsNot Nothing AndAlso e.indexListField IsNot Nothing Then
                    indexListField.AddRange(e.indexListField)
                ElseIf indexListField Is Nothing AndAlso e.indexListField IsNot Nothing Then
                    indexListField = New List(Of Integer)(e.indexListField)
                End If
            End If
        End Sub

        Protected Friend Overridable Sub addToChild(e As CFEntry)
            ' adds directly to the child node
            childField.Entries.Add(e)
        End Sub

        Protected Friend Overridable Function isWithinThreshold(e As CFEntry, threshold As Double, distFunction As Integer) As Boolean
            Dim dist = distance(e, distFunction)
            ' System.out.println("Distance = " + dist);

            If dist = 0 OrElse dist <= threshold Then ' read the comments in function d0() about differences with implementation in R
                Return True
            End If

            Return False
        End Function

        ''' 
        ''' <param name="e"> </param>
        ''' <returns> the distance between this entry and e </returns>
        Protected Friend Overridable Function distance(e As CFEntry, distFunction As Integer) As Double
            Dim dist = Double.MaxValue

            Select Case distFunction
                Case CFTree.D0_DIST
                    dist = d0(Me, e)
                Case CFTree.D1_DIST
                    dist = d1(Me, e)
                Case CFTree.D2_DIST
                    dist = d2(Me, e)
                Case CFTree.D3_DIST
                    dist = d3(Me, e)
                Case CFTree.D4_DIST
                    dist = d4(Me, e)
            End Select

            Return dist
        End Function

        Private Function d0(e1 As CFEntry, e2 As CFEntry) As Double
            Dim dist As Double = 0
            For i = 0 To e1.sumX.Length - 1
                Dim diff = e1.sumX(i) / e1.n - e2.sumX(i) / e2.n
                dist += diff * diff
            Next

            If dist < 0 Then
                Console.Error.WriteLine("d0 < 0 !!!")
            End If

            ' notice here that in the R implementation of BIRCH (package birch)
            ' 
            ' the radius parameter is based on the squared distance /dist/
            ' this causes a difference in results.
            ' if we change the line below into 
            '   return dist;
            ' the results produced by the R implementation and this Java implementation
            ' will match perfectly (notice that in the R implementation maxEntries = 100
            ' and merging refinement is not implemented)
            Return std.Sqrt(dist)
        End Function

        Private Function d1(e1 As CFEntry, e2 As CFEntry) As Double
            Dim dist As Double = 0
            For i = 0 To e1.sumX.Length - 1
                Dim diff = std.Abs(e1.sumX(i) / e1.n - e2.sumX(i) / e2.n)
                dist += diff
            Next

            If dist < 0 Then
                Console.Error.WriteLine("d1 < 0 !!!")
            End If

            Return dist
        End Function

        Private Function d2(e1 As CFEntry, e2 As CFEntry) As Double
            Dim dist As Double = 0

            Dim n1 = e1.n
            Dim n2 = e2.n
            For i = 0 To e1.sumX.Length - 1
                Dim diff = (n2 * e1.sumX2(i) - 2 * e1.sumX(i) * e2.sumX(i) + n1 * e2.sumX2(i)) / (n1 * n2)
                dist += diff
            Next

            If dist < 0 Then
                Console.Error.WriteLine("d2 < 0 !!!")
            End If

            Return std.Sqrt(dist)
        End Function

        Private Function d3(e1 As CFEntry, e2 As CFEntry) As Double
            Dim dist As Double = 0

            Dim n1 = e1.n
            Dim n2 = e2.n
            Dim totSumX As Double() = CType(e1.sumX.Clone(), Double())
            Dim totSumX2 As Double() = CType(e1.sumX2.Clone(), Double())
            For i = 0 To e2.sumX.Length - 1
                totSumX(i) += e2.sumX(i)
                totSumX2(i) += e2.sumX2(i)
            Next

            For i = 0 To totSumX.Length - 1
                Dim diff = ((n1 + n2) * totSumX2(i) - 2 * totSumX(i) * totSumX(i) + (n1 + n2) * totSumX2(i)) / ((n1 + n2) * (n1 + n2 - 1))
                dist += diff
            Next

            If dist < 0 Then
                Console.Error.WriteLine("d3 < 0 !!!")
            End If

            Return std.Sqrt(dist)
        End Function

        Private Function d4(e1 As CFEntry, e2 As CFEntry) As Double
            Dim dist As Double = 0

            Dim n1 = e1.n
            Dim n2 = e2.n
            Dim totSumX As Double() = CType(e1.sumX.Clone(), Double())
            Dim totSumX2 As Double() = CType(e1.sumX2.Clone(), Double())
            For i = 0 To e2.sumX.Length - 1
                totSumX(i) += e2.sumX(i)
                totSumX2(i) += e2.sumX2(i)
            Next

            For i = 0 To totSumX.Length - 1
                Dim diff1 = totSumX2(i) - 2 * totSumX(i) * totSumX(i) / (n1 + n2) + (n1 + n2) * (totSumX(i) / (n1 + n2)) * (totSumX(i) / (n1 + n2))
                Dim diff2 = e1.sumX2(i) - 2 * e1.sumX(i) * e1.sumX(i) / n1 + n1 * (e1.sumX(i) / n1) * (e1.sumX(i) / n1)
                Dim diff3 = e2.sumX2(i) - 2 * e2.sumX(i) * e2.sumX(i) / n2 + n2 * (e2.sumX(i) / n2) * (e2.sumX(i) / n2)
                dist += diff1 - diff2 - diff3
            Next

            If dist < 0 Then
                Console.Error.WriteLine("d4 < 0 !!!")
            End If

            Return std.Sqrt(dist)
        End Function

        Public Overrides Function Equals(o As Object) As Boolean
            Dim e = CType(o, CFEntry)

            If n <> e.n Then
                Return False
            End If

            If childField IsNot Nothing AndAlso e.childField Is Nothing Then
                Return False
            End If

            If childField Is Nothing AndAlso e.childField IsNot Nothing Then
                Return False
            End If

            If childField IsNot Nothing AndAlso Not childField.Equals(e.childField) Then
                Return False
            End If

            If indexListField Is Nothing AndAlso e.indexListField IsNot Nothing Then
                Return False
            End If

            If indexListField IsNot Nothing AndAlso e.indexListField Is Nothing Then
                Return False
            End If

            If Not sumX.SequenceEqual(e.sumX) Then
                Return False
            End If

            If Not sumX2.SequenceEqual(e.sumX2) Then
                Return False
            End If

            If indexListField IsNot Nothing AndAlso Not indexListField.SequenceEqual(e.indexListField) Then
                Return False
            End If

            Return True
        End Function

        Public Overrides Function ToString() As String
            Dim buff As StringBuilder = New StringBuilder()
            buff.Append(" ")
            For i = 0 To sumX.Length - 1
                buff.Append(sumX(i) / n.ToString() & " ")
            Next

            If indexListField IsNot Nothing Then
                buff.Append("( ")
                For Each i In indexListField
                    buff.Append(i.ToString() & " ")
                Next
                buff.Append(")")
            End If
            If hasChild() Then
                buff.Append(LINE_SEP)
                buff.Append("||" & LINE_SEP)
                buff.Append("||" & LINE_SEP)
                buff.Append(Child)
            End If


            Return buff.ToString()
        End Function
    End Class

End Namespace
