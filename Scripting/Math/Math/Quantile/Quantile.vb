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

    Public Structure Quantile

        Public ReadOnly ___quantile As Double
        Public ReadOnly [error] As Double
        Public ReadOnly u As Double
        Public ReadOnly v As Double

        Public Sub New(quantile As Double, [error] As Double)
            Me.___quantile = quantile
            Me.error = [error]
            Me.u = 2.0 * [error] / (1.0 - quantile)
            Me.v = 2.0 * [error] / quantile
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("Q{{q={0:F3}, eps={1:F3}}})", ___quantile, [error])
        End Function
    End Structure
End Namespace