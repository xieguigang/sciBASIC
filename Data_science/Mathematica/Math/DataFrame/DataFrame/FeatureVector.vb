#Region "Microsoft.VisualBasic::74aa32d4b6f9435d6daa56814e447a01, sciBASIC#\Data_science\Mathematica\Math\DataFrame\DataFrame\FeatureVector.vb"

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

'   Total Lines: 70
'    Code Lines: 56
' Comment Lines: 0
'   Blank Lines: 14
'     File Size: 2.31 KB


' Class FeatureVector
' 
'     Properties: isScalar, type, vector
' 
'     Constructor: (+7 Overloads) Sub New
'     Function: [TryCast], FromGeneral, ToString
' 
' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ValueTypes

Public Class FeatureVector

    Public ReadOnly Property vector As Array
    Public ReadOnly Property type As Type

    Public ReadOnly Property isScalar As Boolean
        Get
            Return vector Is Nothing OrElse vector.Length = 0 OrElse vector.Length = 1
        End Get
    End Property

    Public ReadOnly Property size As Integer
        Get
            Return vector.Length
        End Get
    End Property

    Sub New(ints As IEnumerable(Of Integer))
        vector = ints.ToArray
        type = GetType(Integer)
    End Sub

    Sub New(factors As IEnumerable(Of String))
        vector = factors.ToArray
        type = GetType(String)
    End Sub

    Sub New(floats As IEnumerable(Of Single))
        vector = floats.ToArray
        type = GetType(Single)
    End Sub

    Sub New(doubles As IEnumerable(Of Double))
        vector = doubles.ToArray
        type = GetType(Double)
    End Sub

    Sub New(int16 As IEnumerable(Of Short))
        vector = int16.ToArray
        type = GetType(Short)
    End Sub

    Sub New(int64 As IEnumerable(Of Long))
        vector = int64.ToArray
        type = GetType(Long)
    End Sub

    Sub New(logicals As IEnumerable(Of Boolean))
        vector = logicals.ToArray
        type = GetType(Boolean)
    End Sub

    Public Function [TryCast](Of T)() As T()
        If GetType(T) Is type Then
            Return DirectCast(vector, T())
        Else
            Select Case GetType(T)
                Case GetType(Double)
                    ' cast value to double [target]
                    Select Case type
                        Case GetType(Integer), GetType(Short), GetType(Long), GetType(Single)
                            Return CObj(vector.AsObjectEnumerator.Select(Function(o) CDbl(o)).ToArray)
                        Case GetType(Boolean)
                            Return CObj(vector.AsObjectEnumerator(Of Boolean).Select(Function(b) If(b, 1.0, 0.0)).ToArray)
                        Case GetType(Date)
                            Return CObj(vector.AsObjectEnumerator(Of Date).Select(Function(d) d.UnixTimeStamp).ToArray)
                    End Select
            End Select

            Throw New NotImplementedException
        End If
    End Function

    Public Overrides Function ToString() As String
        Return $"[{type.Name}] {vector.Length} elements"
    End Function

    Public Shared Function CheckSupports(type As Type) As Boolean
        Static primitiveSupports As Index(Of Type) = {
            GetType(Integer), GetType(Short), GetType(Long),
            GetType(Single), GetType(Double), GetType(Boolean),
            GetType(String)
        }

        Return type Like primitiveSupports
    End Function

    Public Shared Function FromGeneral(vec As Array) As FeatureVector
        Select Case vec.GetType.GetElementType
            Case GetType(Integer) : Return New FeatureVector(DirectCast(vec, Integer()))
            Case GetType(Short) : Return New FeatureVector(DirectCast(vec, Short()))
            Case GetType(Long) : Return New FeatureVector(DirectCast(vec, Long()))
            Case GetType(Single) : Return New FeatureVector(DirectCast(vec, Single()))
            Case GetType(Double) : Return New FeatureVector(DirectCast(vec, Double()))
            Case GetType(Boolean) : Return New FeatureVector(DirectCast(vec, Boolean()))
            Case GetType(String) : Return New FeatureVector(DirectCast(vec, String()))
            Case Else
                Throw New NotImplementedException(vec.GetType.FullName)
        End Select
    End Function

End Class
