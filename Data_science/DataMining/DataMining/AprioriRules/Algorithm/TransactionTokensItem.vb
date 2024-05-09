#Region "Microsoft.VisualBasic::7341cd314f5984cc56d6cde262f924ef, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//AprioriRules/Algorithm/Entities/TransactionTokensItem.vb"

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

'   Total Lines: 24
'    Code Lines: 16
' Comment Lines: 0
'   Blank Lines: 8
'     File Size: 688 B


'     Class TransactionTokensItem
' 
'         Properties: Name, Support
' 
'         Function: CompareTo, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.AprioriRules.Impl

Namespace AprioriRules.Entities

    Public Class TransactionTokensItem : Implements IComparable(Of TransactionTokensItem)

        Public Property Name() As ItemSet
        Public Property Support() As Double

        Public Overrides Function ToString() As String
            Return String.Format("(support={0})  {1}", Support, Name)
        End Function

        Public Function CompareTo(other As TransactionTokensItem) As Integer Implements IComparable(Of TransactionTokensItem).CompareTo
            Return Name.CompareTo(other.Name)
        End Function

    End Class
End Namespace
