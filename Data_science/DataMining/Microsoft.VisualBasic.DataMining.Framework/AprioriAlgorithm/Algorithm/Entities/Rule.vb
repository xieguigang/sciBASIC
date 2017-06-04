#Region "Microsoft.VisualBasic::30132b829ba2852844698d00a953f3fd, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\Entities\Rule.vb"

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

Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace AprioriAlgorithm.Entities

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Rule : Implements IComparable(Of Rule)

#Region "Member Variables"

        Dim combination As String, remaining As String
        Dim _confidence As Double

#End Region

#Region "Constructor"

        Public Sub New(combination As String, remaining As String, confidence As Double)
            Me.combination = combination
            Me.remaining = remaining
            Me._confidence = confidence
        End Sub

#End Region

#Region "Public Properties"

        <Column("rule.X")> Public ReadOnly Property X() As String
            Get
                Return combination
            End Get
        End Property

        <Column("rule.Y")> Public ReadOnly Property Y() As String
            Get
                Return remaining
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column("confidence")> Public ReadOnly Property Confidence() As Double
            Get
                Return _confidence
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("({0})  {1}  --> {2}", Confidence, X, Y)
        End Function

#End Region

#Region "IComparable<clssRules> Members"

        Public Function CompareTo(other As Rule) As Integer Implements IComparable(Of Rule).CompareTo
            Return X.CompareTo(other.X)
        End Function
#End Region

        Public Overrides Function GetHashCode() As Integer
            Dim sortedXY As String = Apriori.SorterSortTokens(X & Y)
            Return sortedXY.GetHashCode()
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Rule)
            If other Is Nothing Then
                Return False
            End If

            Return other.X = Me.X AndAlso other.Y = Me.Y OrElse other.X = Me.Y AndAlso other.Y = Me.X
        End Function
    End Class
End Namespace
