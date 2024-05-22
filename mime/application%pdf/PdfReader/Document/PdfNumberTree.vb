#Region "Microsoft.VisualBasic::1bd271ded65fb04825bfa69467b554b6, mime\application%pdf\PdfReader\Document\PdfNumberTree.vb"

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

    '   Total Lines: 112
    '    Code Lines: 89 (79.46%)
    ' Comment Lines: 5 (4.46%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (16.07%)
    '     File Size: 4.18 KB


    '     Class PdfNumberTree
    ' 
    '         Properties: LimitMax, LimitMin
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Load, Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfNumberTree
        Inherits PdfObject

        Private _LimitMin As Integer, _LimitMax As Integer
        Private ReadOnly _root As Boolean
        Private _dictionary As PdfDictionary
        Private _children As List(Of PdfNumberTree)
        Private _nums As Dictionary(Of Integer, PdfObject)

        Public Sub New(dictionary As PdfDictionary, Optional root As Boolean = True)
            MyBase.New(dictionary.Parent)
            _dictionary = dictionary
            _root = root

            If _root Then
                Load()
            Else
                Dim limits = _dictionary.MandatoryValue(Of PdfArray)("Limits")
                LimitMin = CType(limits.Objects(0), PdfInteger).Value
                LimitMax = CType(limits.Objects(1), PdfInteger).Value
            End If
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property LimitMin As Integer
            Get
                Return _LimitMin
            End Get
            Private Set(value As Integer)
                _LimitMin = value
            End Set
        End Property

        Public Property LimitMax As Integer
            Get
                Return _LimitMax
            End Get
            Private Set(value As Integer)
                _LimitMax = value
            End Set
        End Property

        Default Public ReadOnly Property Item(number As Integer) As PdfObject
            Get
                Dim ret As PdfObject = Nothing

                If number >= LimitMin AndAlso number <= LimitMax Then
                    If _nums Is Nothing AndAlso _children Is Nothing Then Load()

                    If _nums IsNot Nothing Then
                        _nums.TryGetValue(number, ret)
                    Else
                        ' Linear search, could improve perf by using a binary search
                        For Each child In _children

                            If number >= child.LimitMin AndAlso number <= child.LimitMax Then
                                ret = child(number)
                                Exit For
                            End If
                        Next
                    End If
                End If

                Return ret
            End Get
        End Property

        Private Sub Load()
            Dim kids = _dictionary.OptionalValue(Of PdfArray)("Kids")

            If kids IsNot Nothing Then
                ' Must load all the children as objects immediately, so we can then calculate the overall limits
                _children = New List(Of PdfNumberTree)()

                For Each reference As PdfObjectReference In kids.Objects
                    _children.Add(New PdfNumberTree(Document.IndirectObjects.MandatoryValue(Of PdfDictionary)(reference), False))
                Next

                ' Only the root calculates the limits by examining the children
                If _root Then
                    LimitMin = _children(0).LimitMin
                    LimitMax = _children(_children.Count - 1).LimitMax
                End If
            Else
                ' Without 'Kids' the 'Nums' is mandatory
                Dim array = _dictionary.MandatoryValue(Of PdfArray)("Nums")
                _nums = New Dictionary(Of Integer, PdfObject)()
                Dim count = array.Objects.Count

                For i = 0 To count - 1 Step 2
                    Dim name = CType(array.Objects(i), PdfInteger)
                    _nums.Add(name.Value, array.Objects(i + 1))

                    ' Only the root calculates the limits by examining the enties
                    If _root Then
                        If i = 0 Then
                            LimitMin = name.Value
                        ElseIf i = count - 1 Then
                            LimitMax = name.Value
                        End If
                    End If
                Next
            End If
        End Sub
    End Class
End Namespace
