#Region "Microsoft.VisualBasic::1ba6755e81f623301110677e49b38638, ..\visualbasic_App\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\Entities\Output.vb"

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

Namespace AprioriAlgorithm.Entities

    Public Class Output

#Region "Public Properties"

        Public Property StrongRules() As IList(Of Rule)
        Public Property MaximalItemSets() As IList(Of String)
        Public Property ClosedItemSets() As Dictionary(Of String, Dictionary(Of String, Double))
        Public Property FrequentItems() As Dictionary(Of String, TransactionTokensItem)
#End Region

    End Class
End Namespace
