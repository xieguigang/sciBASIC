#Region "Microsoft.VisualBasic::408325d316799f6150b78e1256f04ded, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\FuzzyCMeans\Entity.vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCMeans

    Public Class Entity : Inherits KMeans.Entity

        ''' <summary>
        ''' ``Key``键名和数组的下标一样是从0开始的
        ''' </summary>
        ''' <returns></returns>
        Public Property Memberships As Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Max probably of <see cref="Memberships"/> its key value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProbablyMembership As Integer
            Get
                Return Memberships _
                    .Keys _
                    .Select(Function(i) Memberships(i)) _
                    .MaxIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{uid} --> {Memberships.GetJson}"
        End Function
    End Class
End Namespace
