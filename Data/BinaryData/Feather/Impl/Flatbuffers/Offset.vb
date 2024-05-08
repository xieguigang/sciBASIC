' 
'* Copyright 2014 Google Inc. All rights reserved.
'*
'* Licensed under the Apache License, Version 2.0 (the "License");
'* you may not use this file except in compliance with the License.
'* You may obtain a copy of the License at
'*
'*     http://www.apache.org/licenses/LICENSE-2.0
'*
'* Unless required by applicable law or agreed to in writing, software
'* distributed under the License is distributed on an "AS IS" BASIS,
'* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'* See the License for the specific language governing permissions and
'* limitations under the License.


Namespace FlatBuffers
    ''' <summary>
    ''' Offset class for typesafe assignments.
    ''' </summary>
    Friend Structure Offset(Of T)
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"Offset(Of {GetType(T).Name}): {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure

    Friend Structure StringOffset
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"StringOffset: {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure

    Friend Structure VectorOffset
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"VectorOffset: {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure
End Namespace
