#Region "Microsoft.VisualBasic::f90127708c0dd871af8142375132f13c, Microsoft.VisualBasic.Core\ComponentModel\Ranges\RangeList.vb"

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

    '     Class RangeList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Select], (+2 Overloads) SelectValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    Public Class RangeList(Of T As IComparable, V) : Inherits List(Of RangeTagValue(Of T, V))

        Sub New()
            MyBase.New(128)
        End Sub

        Public Function [Select](x As T) As RangeTagValue(Of T, V)
            Dim LQuery As RangeTagValue(Of T, V) =
                LinqAPI.DefaultFirst(Of RangeTagValue(Of T, V)) <=
                From r As RangeTagValue(Of T, V)
                In Me.AsQueryable
                Where r.IsInside(x)
                Select r

            Return LQuery
        End Function

        Public Function SelectValue(x As T, Optional [throw] As Boolean = True, Optional ByRef success As Boolean = False) As V
            Dim n As RangeTagValue(Of T, V) = [Select](x)
            If n Is Nothing Then
                If [throw] Then
                    Throw New DataException($"{x.GetJson} is not in any ranges!")
                Else
                    Return Nothing
                End If
            Else
                success = True
                Return n.Value
            End If
        End Function

        Public Function SelectValue(x As T, [default] As Func(Of T, V)) As V
            Dim success As Boolean = False
            Dim v As V = SelectValue(x, [throw]:=False, success:=success)

            If success Then
                Return v
            Else
                Return [default](x)
            End If
        End Function

        Public ReadOnly Iterator Property Values As IEnumerable(Of V)
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x.Value
                Next
            End Get
        End Property

        Public ReadOnly Iterator Property Keys As IEnumerable(Of Range(Of T))
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x
                Next
            End Get
        End Property
    End Class
End Namespace
