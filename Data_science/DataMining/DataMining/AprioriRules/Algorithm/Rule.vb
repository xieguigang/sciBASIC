#Region "Microsoft.VisualBasic::a7c7e7b41521fca7dd96a9905ffeb94e, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//AprioriRules/Algorithm/Entities/Rule.vb"

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

    '   Total Lines: 82
    '    Code Lines: 54
    ' Comment Lines: 10
    '   Blank Lines: 18
    '     File Size: 2.50 KB


    '     Class Rule
    ' 
    '         Properties: Confidence, SupportX, SupportXY, X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports std = System.Math

Namespace AprioriRules.Entities

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Rule : Implements IComparable(Of Rule)

        ''' <summary>
        ''' combination 
        ''' </summary>
        ''' <returns></returns>
        <Column(Name:="rule.X")> Public ReadOnly Property X As ItemSet

        ''' <summary>
        ''' remaining
        ''' </summary>
        ''' <returns></returns>
        <Column(Name:="rule.Y")> Public ReadOnly Property Y As ItemSet

        <Column(Name:="support(XY)")>
        Public ReadOnly Property SupportXY As Double
        <Column(Name:="support(X)")>
        Public ReadOnly Property SupportX As Double

        Public ReadOnly Property length As Integer
            Get
                Return X.Length + Y.Length
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column(Name:="confidence")> Public ReadOnly Property Confidence As Double

        Public Sub New(combination As ItemSet, remaining As ItemSet, confidence#, supports As (XY#, X#))
            Me.X = combination
            Me.Y = remaining
            Me.Confidence = confidence
            Me.SupportX = supports.X
            Me.SupportXY = supports.XY
        End Sub

        Public Overrides Function ToString() As String
            Return $"({SupportXY}/{SupportX} = {std.Round(Confidence, 4)}) {X} -> {Y}"
        End Function

        Public Function CompareTo(other As Rule) As Integer Implements IComparable(Of Rule).CompareTo
            Return X.CompareTo(other.X)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim sortedXY As ItemSet = (X & Y).SorterSortTokens
            Dim hash As Integer = sortedXY.GetHashCode()

            Return hash
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Rule)

            If other Is Nothing Then
                Return False
            End If

            Return other.X = X AndAlso other.Y = Y OrElse other.X = Y AndAlso other.Y = X
        End Function
    End Class
End Namespace
