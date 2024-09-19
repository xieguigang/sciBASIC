#Region "Microsoft.VisualBasic::5a2973b8c08c305d5e04a52dae63a05a, gr\physics\layout\AbstractForce.vb"

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

'   Total Lines: 57
'    Code Lines: 8 (14.04%)
' Comment Lines: 44 (77.19%)
'    - Xml Docs: 6.82%
' 
'   Blank Lines: 5 (8.77%)
'     File Size: 2.45 KB


'     Class AbstractForce
' 
'         Function: calculateForce
' 
' 
' /********************************************************************************/

#End Region

' 
' Copyright 2008-2010 Gephi
' Authors : Helder Suzuki <heldersuzuki@gephi.org>
' Website : http://www.gephi.org
' 
' This file is part of Gephi.
' 
' DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
' 
' Copyright 2011 Gephi Consortium. All rights reserved.
' 
' The contents of this file are subject to the terms of either the GNU
' General Public License Version 3 only ("GPL") or the Common
' Development and Distribution License("CDDL") (collectively, the
' "License"). You may not use this file except in compliance with the
' License. You can obtain a copy of the License at
' http://gephi.org/about/legal/license-notice/
' or /cddl-1.0.txt and /gpl-3.0.txt. See the License for the
' specific language governing permissions and limitations under the
' License.  When distributing the software, include this License Header
' Notice in each file and include the License files at
' /cddl-1.0.txt and /gpl-3.0.txt. If applicable, add the following below the
' License Header, with the fields enclosed by brackets [] replaced by
' your own identifying information:
' "Portions Copyrighted [year] [name of copyright owner]"
' 
' If you wish your version of this file to be governed by only the CDDL
' or only the GPL Version 3, indicate your decision by adding
' "[Contributor] elects to include this software in this distribution
' under the [CDDL or GPL Version 3] license." If you do not indicate a
' single choice of license, a recipient has the option to distribute
' your version of this file under either the CDDL, the GPL Version 3 or
' to extend the choice of license to its licensees as provided above.
' However, if you add GPL Version 3 code and therefore, elected the GPL
' Version 3 license, then the option applies only if the new code is
' made subject to such option by the copyright holder.
' 
' Contributor(s):
' 
' Portions Copyrighted 2011 Gephi Consortium.
' 

Imports System.Runtime.CompilerServices

Namespace layout

    ''' <summary>
    ''' @author Helder Suzuki
    ''' </summary>
    Public MustInherit Class AbstractForce

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function calculateForce(node1 As Layout2D, node2 As Layout2D) As ForceVector
            Return Me.calculateForce(node1, node2, ForceVectorUtils.distance(node1, node2))
        End Function

        Public MustOverride Function calculateForce(node1 As Layout2D, node2 As Layout2D, distance As Single) As ForceVector
    End Class

End Namespace
