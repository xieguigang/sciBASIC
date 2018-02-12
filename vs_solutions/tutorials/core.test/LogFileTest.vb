#Region "Microsoft.VisualBasic::32bf033d93b65934e75dc90194c7a96a, ..\core.test\LogFileTest.vb"

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
    ' L
    ' o
    ' g
    ' F
    ' i
    ' l
    ' e
    ' T
    ' e
    ' s
    ' t
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
    ' ,
    '  
    ' P
    ' r
    ' i
    ' n
    ' t
    ' T
    ' e
    ' s
    ' t
    ' ,
    '  
    ' W
    ' r
    ' i
    ' t
    ' e
    ' T
    ' e
    ' s
    ' 

    ' 

    ' 
    ' 

    ' 
    ' 

    ' 
    ' 


#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module LogFileTest

    Sub Main()
        WriteTes()
        PrintTest()
    End Sub

    Const path$ = "./test.log"

    Sub WriteTes()
        Using log As New LogFile(path)
            Call log.writeline("123", "test")
        End Using
    End Sub

    Sub PrintTest()

    End Sub
End Module
