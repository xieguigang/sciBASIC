'
'   Copyright 2012 Andrew Wang (andrew@umbrant.com)
'
'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may not use this file except in compliance with the License.
'   You may obtain a copy of the License at
'
'       http://www.apache.org/licenses/LICENSE-2.0
'
'   Unless required by applicable law or agreed to in writing, software
'   distributed under the License is distributed on an "AS IS" BASIS,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'   See the License for the specific language governing permissions and
'   limitations under the License.
'

Namespace Quantile

    Public Class X

        Public ReadOnly Property value As Long
        Public Property g As Integer
        Public ReadOnly Property delta As Integer

        Public Sub New(value As Long, lower_delta As Integer, delta As Integer)
            Me.value = value
            Me.g = lower_delta
            Me.delta = delta
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0:D}, {1:D}, {2:D}", value, g, delta)
        End Function
    End Class
End Namespace