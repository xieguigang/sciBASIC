#Region "Microsoft.VisualBasic::c540d186a51ffd0be60c7b61d85da52d, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\PeriodAnalysis\Variables.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.IEnumerations
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Serials.PeriodAnalysis

    Public Structure SerialsVarialble
        Implements sIdEnumerable

        Public Property Identifier As String Implements sIdEnumerable.Identifier
        Dim SerialsData As Double()

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Public Shared Function Load(source As File) As SerialsVarialble()
            Dim LQuery = LinqAPI.Exec(Of SerialsVarialble) <=
 _
                From Line As RowObject
                In source
                Let Tag As String = Line.First
                Let Data As Double() = (From col As String In Line.Skip(1) Select Val(col)).ToArray
                Select New SerialsVarialble With {
                    .Identifier = Tag,
                    .SerialsData = Data
                }

            Return LQuery
        End Function
    End Structure

    Public Class TimePoint
        Public Property Time As Integer
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} --> {1}", Time, Value)
        End Function

        Friend Shared Function GetData(source As IEnumerable(Of TimePoint), Time As Integer) As TimePoint
            Dim LQuery = (From p In source Where p.Time = Time Select p).First
            Return LQuery
        End Function

        Friend Shared Function GetData(Time As Integer, source As IEnumerable(Of TimePoint)) As Double
            Dim LQuery = (From p In source Where p.Time = Time Select p).ToArray
            If LQuery.IsNullOrEmpty Then
                Return 0
            Else
                Return LQuery.First.Value
            End If
        End Function

        Friend Shared Function CreateBufferObject(ChunkBuffer As Generic.IEnumerable(Of TimePoint)) As SortedDictionary(Of Integer, Double)
            Dim Temp As SortedDictionary(Of Integer, Double) = New SortedDictionary(Of Integer, Double)
            For Each p In ChunkBuffer
                Call Temp.Add(p.Time, p.Value)
            Next

            Return Temp
        End Function
    End Class

    Public Class SamplingData
        Public Property Peaks As List(Of TimePoint)
        Public Property Trough As List(Of TimePoint)
        Public Property FiltedData As List(Of TimePoint)
        Public Property TimePoints As Integer
        Public Property TSerials As TimePoint()
    End Class
End Namespace
