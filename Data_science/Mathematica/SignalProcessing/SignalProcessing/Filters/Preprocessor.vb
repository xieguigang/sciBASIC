﻿' 
'  Copyright [2009] [Marcin Rzeźnicki]
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' 

Namespace mr.go.sgfilter
    ''' <summary>
    ''' This interface represents types which are able to perform data processing in
    ''' place. Useful examples include: eliminating zeros, padding etc.
    ''' 
    ''' @author Marcin Rzeźnicki </summary>
    ''' <seealsocref="SGFilter"/> </seealso>
    Public Interface Preprocessor

        ''' <summary>
        ''' Data processing method. Called on Preprocessor instance when its
        ''' processing is needed
        ''' </summary>
        ''' <paramname="data"> </param>
        Sub apply(data As Double())
    End Interface

End Namespace
