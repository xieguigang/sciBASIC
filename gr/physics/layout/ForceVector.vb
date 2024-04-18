Imports System
Imports std = System.Math

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

Namespace layout

    ''' <summary>
    ''' @author Helder Suzuki
    ''' </summary>
    Public Class ForceVector
        Inherits LayoutData

        'JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        Protected Friend x_Conflict As Single
        'JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        Protected Friend y_Conflict As Single

        Public Sub New(vector As ForceVector)
            x_Conflict = vector.x()
            y_Conflict = vector.y()
        End Sub

        Public Sub New(x As Single, y As Single)
            x_Conflict = x
            y_Conflict = y
        End Sub

        Public Sub New()
            x_Conflict = 0
            y_Conflict = 0
        End Sub

        Public Overridable Function x() As Single
            Return x_Conflict
        End Function

        Public Overridable Function y() As Single
            Return y_Conflict
        End Function

        Public Overridable Function z() As Single
            Throw New NotSupportedException("Not supported yet.")
        End Function

        Public Overridable WriteOnly Property XProp As Single
            Set(value As Single)
                x_Conflict = value
            End Set
        End Property

        Public Overridable WriteOnly Property YProp As Single
            Set(value As Single)
                y_Conflict = value
            End Set
        End Property

        Public Overridable Sub add(f As ForceVector)
            If f IsNot Nothing Then
                x_Conflict += f.x()
                y_Conflict += f.y()
            End If
        End Sub

        Public Overridable Sub multiply(s As Single)
            x_Conflict *= s
            y_Conflict *= s
        End Sub

        Public Overridable Sub subtract(f As ForceVector)
            If f IsNot Nothing Then
                x_Conflict -= f.x()
                y_Conflict -= f.y()
            End If
        End Sub

        Public Overridable ReadOnly Property Energy As Single
            Get
                Return x_Conflict * x_Conflict + y_Conflict * y_Conflict
            End Get
        End Property

        Public Overridable ReadOnly Property Norm As Single
            Get
                Return std.Sqrt(Energy)
            End Get
        End Property

        Public Overridable Function normalize() As ForceVector
            Dim norm = Me.Norm
            Return New ForceVector(x_Conflict / norm, y_Conflict / norm)
        End Function

        Public Overrides Function ToString() As String
            Return "(" & x_Conflict.ToString() & ", " & y_Conflict.ToString() & ")"
        End Function
    End Class

End Namespace
