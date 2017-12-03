#Region "Microsoft.VisualBasic::6ab388ad4564fbfe6309622c4d8b8a63, ..\sciBASIC#\Data_science\Mathematica\Math\ODE\ODEsSolver\ValueVector.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ValueVector : Inherits int

    Public Property Y As Dictionary(Of NamedCollection(Of Double))

    Default Public Overloads Property Value(name$) As Double
        Get
            Return Y(name).Value(MyBase.Value)
        End Get
        Set(value As Double)
            Y(name$).Value(MyBase.Value) = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return $"[{Value}] " & Y.Keys.ToArray.GetJson
    End Function

    ''' <summary>
    ''' Move pointer value
    ''' </summary>
    ''' <param name="v"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Overloads Shared Operator +(v As ValueVector, n%) As ValueVector
        v.Value += n
        Return v
    End Operator
End Class
