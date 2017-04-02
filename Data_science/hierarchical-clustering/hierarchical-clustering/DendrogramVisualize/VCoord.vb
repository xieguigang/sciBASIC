'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
' 

Namespace DendrogramVisualize

    ''' <summary>
    ''' Virtual coordinates.
    ''' </summary>
    Public Class VCoord



        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Property X As Double

        Public Property Y As Double

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is VCoord Then
                Dim other As VCoord = CType(obj, VCoord)
                Return X = other.X AndAlso Y = other.Y
            Else
                Return False
            End If
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("Coord({0:F3},{1:F3})", X, Y)
        End Function

    End Class

End Namespace