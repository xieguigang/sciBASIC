#Region "Microsoft.VisualBasic::c4c036100820a3bb75de4d1580023e26, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework.DynamicProgramming\SmithWaterman\Match.vb"

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

Imports System.Collections.Generic
Imports System.Text
Imports System.Xml.Serialization

''' <summary>
'''  Match class defintion
''' </summary>
Public Class Match

    Sub New()
    End Sub

    Sub New(fA As Integer, tA As Integer, fB As Integer, tB As Integer, s As Double)
        _FromA = fA
        _FromB = fB
        _ToA = tA
        _ToB = tB
        _Score = s
    End Sub

    ''' <summary>
    ''' Returns the value of fromA.
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property FromA As Integer

    ''' <summary>
    ''' Returns the value of fromB.
    ''' </summary>
    <XmlAttribute> Public Property FromB As Integer

    ''' <summary>
    ''' Returns the value of toA.
    ''' </summary>
    <XmlAttribute> Public Property ToA As Integer

    ''' <summary>
    ''' Returns the value of toB.
    ''' </summary>
    <XmlAttribute> Public Property ToB As Integer

    ''' <summary>
    ''' Returns the value of score.
    ''' </summary>
    <XmlAttribute> Public Property Score As Double

    ''' <summary>
    ''' Check whether this Match onecjt overlap with input Match m;
    ''' return true if two objects do not overlap
    ''' </summary>
    ''' <param name="m"></param>
    ''' <returns></returns>
    Public Function notOverlap(m As Match) As Boolean
        Return (m.FromA > _ToA OrElse _FromA > m.ToA) AndAlso (m.FromB > _ToB OrElse _FromB > m.ToB)
    End Function

    Public Function isChainable(m As Match) As Boolean
        Return (m.FromA > _ToA AndAlso m.FromB > _ToB)
    End Function

    Public Overrides Function ToString() As String
        Return $"[query: {FromA}  ===> {ToA}] <---> [subject: {FromB}  ===> {ToB}], score:={Score}"
    End Function

    Public Shared ReadOnly Property FROMA_COMPARATOR As IComparer(Of Match) =
        New ComparatorAnonymousInnerClassHelper()

    Private Class ComparatorAnonymousInnerClassHelper
        Implements IComparer(Of Match)

        Public Sub New()
        End Sub

        Public Function Compare(x As Match, y As Match) As Integer Implements IComparer(Of Match).Compare
            Return x.FromA - y.FromA
        End Function
    End Class
End Class
