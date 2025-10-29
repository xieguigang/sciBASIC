#Region "Microsoft.VisualBasic::475bbcdeba38cbc62842319b1137f8b4, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Enumerable\BucketSet.vb"

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

    '   Total Lines: 53
    '    Code Lines: 42 (79.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (20.75%)
    '     File Size: 1.74 KB


    '     Class BucketSet
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ForEachBucket, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection.Generic

    Public Class BucketSet(Of T) : Implements IEnumerable(Of T)

        ReadOnly buckets As New List(Of T())

        Public ReadOnly Property Count As Long
            Get
                Return Aggregate block As T()
                       In buckets
                       Let lngSize As Long = CLng(block.Length)
                       Into Sum(lngSize)
            End Get
        End Property

        Public ReadOnly Property PackSize As Integer()
            Get
                Return buckets.Select(Function(p) p.Length).ToArray
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(buckets As IEnumerable(Of IEnumerable(Of T)))
            For Each block As IEnumerable(Of T) In buckets
                Call Me.buckets.Add(block.ToArray)
            Next
        End Sub

        ''' <summary>
        ''' populate each pack data from the bucket data
        ''' </summary>
        ''' <returns></returns>
        Public Function ForEachBucket() As IEnumerable(Of T())
            Return buckets.AsEnumerable
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(block As IEnumerable(Of T))
            Call buckets.Add(block.ToArray)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(getData As Func(Of IEnumerable(Of T)))
            Call Add(getData())
        End Sub

        Public Overrides Function ToString() As String
            Return $"with {buckets.Count} buckets data"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each block As T() In buckets
                For Each item As T In block
                    Yield item
                Next
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
