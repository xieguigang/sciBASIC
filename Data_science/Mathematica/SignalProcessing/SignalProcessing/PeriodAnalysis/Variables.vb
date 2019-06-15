#Region "Microsoft.VisualBasic::63b125db6d085c4d7f4dceb713b35b12, Data_science\DataMining\DataMining\PeriodAnalysis\Variables.vb"

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

    '     Structure SerialsVarialble
    ' 
    '         Properties: Identifier
    ' 
    '         Function: ToString
    ' 
    '     Class TimePoint
    ' 
    '         Properties: Time, Value
    ' 
    '         Function: CreateBufferObject, (+2 Overloads) GetData, ToString
    ' 
    '     Class SamplingData
    ' 
    '         Properties: FiltedData, Peaks, TimePoints, Trough, TSerials
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Serials.PeriodAnalysis

    Public Structure SerialsVarialble
        Implements INamedValue

        Public Property Identifier As String Implements INamedValue.Key
        Dim SerialsData As Double()

        Public Overrides Function ToString() As String
            Return Identifier
        End Function
    End Structure

    ''' <summary>
    ''' 时间点数据
    ''' </summary>
    Public Class TimePoint
        Public Property time As Integer
        Public Property value As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} --> {1}", time, value)
        End Function

        Friend Shared Function GetData(source As IEnumerable(Of TimePoint), Time As Integer) As TimePoint
            Dim LQuery = (From p In source Where p.time = Time Select p).First
            Return LQuery
        End Function

        Public Shared Function GetData(Time As Integer, source As IEnumerable(Of TimePoint)) As Double
            Dim LQuery = (From p In source Where p.time = Time Select p).ToArray
            If LQuery.IsNullOrEmpty Then
                Return 0
            Else
                Return LQuery.First.value
            End If
        End Function

        Friend Shared Function CreateBufferObject(buffer As IEnumerable(Of TimePoint)) As SortedDictionary(Of Integer, Double)
            Dim Temp As New SortedDictionary(Of Integer, Double)
            For Each p In buffer
                Call Temp.Add(p.time, p.value)
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
