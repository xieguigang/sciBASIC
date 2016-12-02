#Region "Microsoft.VisualBasic::7873194e2e2f374539f1e4236ce63c52, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\Entities\TransactionTokensItem.vb"

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

Namespace AprioriAlgorithm.Entities

    Public Class TransactionTokensItem : Implements IComparable(Of TransactionTokensItem)

#Region "Public Properties"

        Public Property Name() As String
        Public Property Support() As Double

#End Region

        Public Overrides Function ToString() As String
            Return String.Format("(support={0})  {1}", Support, Name)
        End Function

#Region "IComparable"

        Public Function CompareTo(other As TransactionTokensItem) As Integer Implements IComparable(Of TransactionTokensItem).CompareTo
            Return Name.CompareTo(other.Name)
        End Function
#End Region

    End Class
End Namespace
