' Copyright (c) 2017 - presented by Kei Nakai
'
' Original project is developed and published by OpenGamma Inc.
'
' Copyright (C) 2012 - present by OpenGamma Inc. and the OpenGamma group of companies
'
' Please see distribution for license.
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'     http://www.apache.org/licenses/LICENSE-2.0
'     
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.

Namespace FFT

    ''' <summary>
    ''' Core utility class to provide general utility methods
    ''' </summary>
    Friend Class TransformCore
        Public Shared ReadOnly THREADS_BEGIN_N_1D_FFT_2THREADS As Integer = 8192

        Public Shared ReadOnly THREADS_BEGIN_N_1D_FFT_4THREADS As Integer = 65536

        Public Shared ReadOnly THREADS_BEGIN_N_1D As Integer = 32768

        Public Shared ReadOnly THREADS_BEGIN_N_2D As Integer = 65536

        Public Shared ReadOnly THREADS_BEGIN_N_3D As Integer = 65536
    End Class
End Namespace
