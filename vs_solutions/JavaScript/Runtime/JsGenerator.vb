#Region "Microsoft.VisualBasic::cd68c71482db44471c840d1b7b40e37b, vs_solutions\JavaScript\Runtime\JsGenerator.vb"

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

    '   Total Lines: 71
    '    Code Lines: 39 (54.93%)
    ' Comment Lines: 19 (26.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (18.31%)
    '     File Size: 2.47 KB


    '     Class JsYieldCollector
    ' 
    '         Function: AddValue
    ' 
    '     Class JsGenerator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Next2, Remaining, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' Eager generator collector: a <c>function*</c> body runs to completion on the
    ''' first call and every <c>yield</c> value is recorded here.
    ''' <para>
    ''' A tree-walking interpreter cannot suspend a body mid-flight without a
    ''' continuation or a thread, so generators are materialised eagerly. That is
    ''' observationally equivalent for the usual <c>for...of</c> / <c>next()</c>
    ''' consumption patterns, but an INFINITE generator would never return — a
    ''' documented limitation.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsYieldCollector

        Public ReadOnly Values As New List(Of Object)

        ''' <summary>Record a yielded value; the yield expression evaluates to it.</summary>
        Public Function AddValue(value As Object) As Object
            Values.Add(value)
            Return value
        End Function

    End Class

    ''' <summary>
    ''' The iterator object handed back by a generator function; supports
    ''' <c>next()</c> (returning <c>{value, done}</c>) and for-of/re-spreading
    ''' through <see cref="JsRuntime.JsIterableValues"/>.
    ''' </summary>
    Public NotInheritable Class JsGenerator

        Public ReadOnly Values As List(Of Object)
        Private _index As Integer

        Public Sub New(values As List(Of Object))
            Me.Values = values
        End Sub

        ''' <summary>One iteration step as a {value, done} object.</summary>
        Public Function Next2() As Object
            Dim d As New Dictionary(Of String, Object)
            If _index < Values.Count Then
                d("value") = Values(_index)
                d("done") = False
                _index += 1
            Else
                d("value") = JsRuntime.Undef
                d("done") = True
            End If
            Return d
        End Function

        ''' <summary>Remaining values — used by for-of and spread.</summary>
        Public Function Remaining() As List(Of Object)
            Dim rest As New List(Of Object)
            While _index < Values.Count
                rest.Add(Values(_index))
                _index += 1
            End While
            Return rest
        End Function

        Public Overrides Function ToString() As String
            Return "[object Generator]"
        End Function

    End Class

End Namespace

