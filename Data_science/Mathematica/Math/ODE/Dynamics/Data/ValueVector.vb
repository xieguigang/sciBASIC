#Region "Microsoft.VisualBasic::0c7c5d6727043c557dae717be2518ceb, Data_science\Mathematica\Math\ODE\Dynamics\Data\ValueVector.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Class ValueVector
    '
    '     Properties: Y
    '
    '     Constructor: (+1 Overloads) Sub New
    '
    '     Function: ToString
    '
    '     Operators: +
    '
    '
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Dynamics.Data

    ''' <summary>
    ''' A cursor over a reference data set: the <see cref="i32"/> base type provides the
    ''' position pointer, and each variable can be accessed by its name.
    ''' (带位置游标的参考数据向量)
    ''' </summary>
    Public Class ValueVector : Inherits i32

        Private _Y As Dictionary(Of NamedCollection(Of Double))
        ''' <summary>
        ''' The resolved column cache, the dictionary lookup and the string conversion of the
        ''' variable name will be skipped after the first access of a given variable.
        ''' </summary>
        Private ReadOnly cached As New Dictionary(Of String, NamedCollection(Of Double))

        ''' <summary>
        ''' The reference data of each variable.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Dictionary(Of NamedCollection(Of Double))
            Get
                Return _Y
            End Get
            Set(value As Dictionary(Of NamedCollection(Of Double)))
                _Y = value
                Call cached.Clear()
            End Set
        End Property

        Default Public Overrides Property Index(name As Object) As Object
            Get
                Return GetColumn(InputHandler.ToString(name)).Value(MyBase.Value)
            End Get
            Set(value As Object)
                GetColumn(InputHandler.ToString(name)).Value(MyBase.Value) = value
            End Set
        End Property

        ''' <summary>
        ''' Get the reference data column of the given variable.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Private Function GetColumn(name As String) As NamedCollection(Of Double)
            Dim column As NamedCollection(Of Double) = Nothing

            If Not cached.TryGetValue(name, column) Then
                column = _Y(name)
                cached(name) = column
            End If

            Return column
        End Function

        Sub New()
            Call MyBase.New(0)
        End Sub

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
End Namespace
