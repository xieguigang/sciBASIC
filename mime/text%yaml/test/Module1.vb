#Region "Microsoft.VisualBasic::5a4d1f6d0cdb1f1307a812a683d643ae, mime\text%yaml\test\Module1.vb"

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

    '   Total Lines: 29
    '    Code Lines: 16 (55.17%)
    ' Comment Lines: 5 (17.24%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (27.59%)
    '     File Size: 745 B


    ' Module Module1
    ' 
    '     Sub: Main, v11, v12
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.text.yaml
Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar

Module Module1

    Sub Main()
        Call v12()
        Call v11()

        Pause()
    End Sub

    Sub v11()
        'Dim docs = Grammar11 _
        '    .YamlParser _
        '    .PopulateDocuments("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.1\idle_anim.yaml") _
        '    .ToArray

        'Pause()
    End Sub

    Sub v12()
        Dim doc = YamlParser.Load("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.2\samples\YAML_Sample.yaml")
        Dim singleDoc = YamlParser.Load("X:\Ripped\00d197fedc13a9d41acf88e9375702e9\AnimationClip\rb_yang_passive.anim")

        Pause()
    End Sub

End Module
