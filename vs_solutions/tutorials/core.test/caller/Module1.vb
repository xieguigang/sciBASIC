#Region "Microsoft.VisualBasic::532761f83fc4f6e6e888b5a96d75fd5b, ..\core.test\caller\Module1.vb"

    ' Author:
    ' 
    '       asuka ()
    '       xieguigang ()
    '       xie ()
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
    ' M
    ' o
    ' d
    ' u
    ' l
    ' e
    '  
    ' M
    ' o
    ' d
    ' u
    ' l
    ' e
    ' 1
    ' 
    ' 

    ' 
    ' 

    '  
    '  
    '  
    '  
    ' S
    ' u
    ' b
    ' :
    '  
    ' M
    ' a
    ' i
    ' n
    ' 

    ' 

    ' 
    ' 

    ' 
    ' 

    ' 
    ' 


#End Region

Module Module1

    Sub Main()

        Call App.Shell(App.HOME & "/child.exe", "/test /123455 sdfgdshgjkfdhgjkdf").Run()

        Pause()
    End Sub

End Module
