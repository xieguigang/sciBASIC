#Region "Microsoft.VisualBasic::08ad26bbb71a44ed23c32a735ba39f8a, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\VectorModel.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Scripting

    Public Class VectorModel(Of T) : Inherits VectorShadows(Of T)

        Default Public Overloads ReadOnly Property Item(name$) As Vector
            Get
                Dim v As Object = Nothing

                If Not MyBase.TryGetMember(name, result:=v) Then
                    Throw New EntryPointNotFoundException(name)
                Else
                    Select Case v.GetType
                        Case GetType(VectorShadows(Of Double))
                            Return DirectCast(v, VectorShadows(Of Double)).AsVector
                        Case GetType(VectorShadows(Of Single))
                            Return DirectCast(v, VectorShadows(Of Single)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Integer))
                            Return DirectCast(v, VectorShadows(Of Integer)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Long))
                            Return DirectCast(v, VectorShadows(Of Long)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Boolean))
                            Return DirectCast(v, VectorShadows(Of Boolean)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Char))
                            Return DirectCast(v, VectorShadows(Of Char)).CharCodes.Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of String))
                            Return DirectCast(v, VectorShadows(Of String)).Select(Function(s) s.ParseDouble).AsVector

                        Case Else

                            Throw New NotSupportedException(v.GetType.FullName)

                    End Select
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(source)
        End Sub
    End Class
End Namespace
