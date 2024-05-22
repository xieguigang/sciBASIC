#Region "Microsoft.VisualBasic::b4b2eb882bb4c3a7712474830f5b4132, gr\network-visualization\network_layout\Cola\PowerGraph\LinkSet.vb"

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

    '   Total Lines: 80
    '    Code Lines: 60 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (25.00%)
    '     File Size: 2.42 KB


    '     Class LinkSets
    ' 
    '         Properties: count
    ' 
    '         Function: contains, intersection, ToString
    ' 
    '         Sub: add, forAll, forAllModules, remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Cola

    Public Class LinkSets

        Dim sets As New Dictionary(Of String, ModuleSet)

        Public ReadOnly Property count() As Integer

        Public Function contains(id As Integer) As Boolean
            Dim result = False

            Me.forAllModules(Sub(m)
                                 If Not result AndAlso m.id = id Then
                                     result = True
                                 End If
                             End Sub)
            Return result
        End Function

        Public Sub add(linktype As Integer, m As [Module])
            Dim s As ModuleSet

            If Me.sets.ContainsKey(linktype.ToString) Then
                s = Me.sets(linktype.ToString)
            Else
                s = New ModuleSet()
                Me.sets(linktype.ToString) = s
            End If

            s.add(m)
            Me._count += 1
        End Sub

        Public Sub remove(linktype As Integer, m As [Module])
            Dim ms = sets(linktype.ToString)

            ms.remove(m)

            If ms.count() = 0 Then
                Me.sets.Remove(linktype.ToString)
            End If

            Me._count -= 1
        End Sub

        Public Sub forAll(f As Action(Of ModuleSet, Integer))
            For Each linktype As String In Me.sets.Keys
                f(Me.sets(linktype), linktype)
            Next
        End Sub

        Private Sub forAllModules(f As Action(Of [Module]))
            Me.forAll(Sub(ms, lt) Call ms.forAll(f))
        End Sub

        Public Function intersection(other As LinkSets) As LinkSets
            Dim result As New LinkSets()

            Me.forAll(Sub(ms, lt)
                          If other.sets.ContainsKey(lt.ToString) Then
                              Dim i = ms.intersection(other.sets(lt))
                              Dim n = i.count()

                              If n > 0 Then
                                  result.sets(lt) = i
                                  result._count += n
                              End If
                          End If
                      End Sub)

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return sets.KeysJson
        End Function
    End Class
End Namespace
