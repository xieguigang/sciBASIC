#Region "Microsoft.VisualBasic::7d9a5619afc08742c40090e55c16e388, ..\sciBASIC#\Data_science\Mathematical\ODE\ODEsSolver\ValueVector.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ValueVector : Inherits int

    Public Property Y As Dictionary(Of NamedCollection(Of Double))

    Default Public Overloads Property Value(name$) As Double
        Get
            Return Y(name).Value(MyBase.value)
        End Get
        Set(value As Double)
            Y(name$).Value(MyBase.value) = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return $"[{value}] " & Y.Keys.ToArray.GetJson
    End Function

    ''' <summary>
    ''' Move pointer value
    ''' </summary>
    ''' <param name="v"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Overloads Shared Operator +(v As ValueVector, n%) As ValueVector
        v.value += n
        Return v
    End Operator
End Class
