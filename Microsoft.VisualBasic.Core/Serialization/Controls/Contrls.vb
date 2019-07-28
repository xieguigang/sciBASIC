#Region "Microsoft.VisualBasic::8ed2f8e34c36d88c77f3237525a5e88e, Microsoft.VisualBasic.Core\Serialization\Controls\Contrls.vb"

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

    '     Class Button
    ' 
    '         Properties: AutoSizeMode, DialogResult
    ' 
    '     Class ButtonBase
    ' 
    '         Properties: AutoEllipsis, AutoSize, BackColor, FlatAppearance, FlatStyle
    '                     Image, ImageAlign, ImageIndex, ImageKey, ImageList
    '                     Text, TextAlign, TextImageRelation, UseCompatibleTextRendering, UseMnemonic
    '                     UseVisualStyleBackColor
    ' 
    '     Class Control
    ' 
    '         Properties: AccessibilityObject, AccessibleDefaultActionDescription, AccessibleDescription, AccessibleName, AccessibleRole
    '                     AllowDrop, Anchor, AutoScrollOffset, AutoSize, BackColor
    '                     BackgroundImage, BackgroundImageLayout, BindingContext, Bottom, Bounds
    '                     CanEnableIme, CanFocus, CanSelect, Capture, CausesValidation
    '                     ClientRectangle, ClientSize, CompanyName, ContainsFocus, Created
    '                     CreateParams, Cursor, DataBindings, DefaultCursor, DefaultImeMode
    '                     DefaultMargin, DefaultMaximumSize, DefaultMinimumSize, DefaultPadding, DefaultSize
    '                     DisplayRectangle, Disposing, Dock, DoubleBuffered, Enabled
    '                     Focused, Font, FontHeight, ForeColor, Handle
    '                     HasChildren, Height, ImeMode, ImeModeBase, InvokeRequired
    '                     IsAccessible, IsDisposed, IsHandleCreated, IsMirrored, Left
    '                     Location, Margin, MaximumSize, MinimumSize, Name
    '                     Padding, Parent, PreferredSize, ProductName, ProductVersion
    '                     RecreatingHandle, Region, RenderRightToLeft, ResizeRedraw, Right
    '                     RightToLeft, ScaleChildren, ShowFocusCues, ShowKeyboardCues, Size
    '                     TabIndex, TabStop, Tag, Text, Top
    '                     UseWaitCursor, Visible, Width, WindowTarget
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Runtime.InteropServices

Namespace Serialization.Controls

    Public Class Button : Inherits ButtonBase
        '
        ' Summary:
        '     Gets or sets the mode by which the System.Windows.Forms.Button automatically
        '     resizes itself.
        '
        ' Returns:
        '     One of the System.Windows.Forms.AutoSizeMode values. The default value is System.Windows.Forms.AutoSizeMode.GrowOnly.
        <Browsable(True)> <DefaultValue(AutoSizeMode.GrowOnly)> <Localizable(True)>
        Public Property AutoSizeMode As AutoSizeMode
        '
        ' Summary:
        '     Gets or sets a value that is returned to the parent form when the button is clicked.
        '
        ' Returns:
        '     One of the System.Windows.Forms.DialogResult values. The default value is None.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Windows.Forms.DialogResult values.
        <DefaultValue(DialogResult.None)>
        Public Overridable Property DialogResult As DialogResult

    End Class

    Public Class ButtonBase : Inherits Control
        '
        ' Summary:
        '     Gets or sets a value indicating whether the ellipsis character (...) appears
        '     at the right edge of the control, denoting that the control text extends beyond
        '     the specified length of the control.
        '
        ' Returns:
        '     true if the additional label text is to be indicated by an ellipsis; otherwise,
        '     false. The default is true.
        <Browsable(True)> <DefaultValue(False)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property AutoEllipsis As Boolean
        '
        ' Summary:
        '     Gets or sets a value that indicates whether the control resizes based on its
        '     contents.
        '
        ' Returns:
        '     true if the control automatically resizes based on its contents; otherwise, false.
        '     The default is true.
        <Browsable(True)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Overrides Property AutoSize As Boolean
        '
        ' Summary:
        '     Gets or sets the background color of the control.
        '
        ' Returns:
        '     A System.Drawing.Color value representing the background color.
        Public Overrides Property BackColor As Color
        '
        ' Summary:
        '     Gets the appearance of the border and the colors used to indicate check state
        '     and mouse state.
        '
        ' Returns:
        '     One of the System.Windows.Forms.FlatButtonAppearance values.
        <Browsable(True)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        Public Property FlatAppearance As FlatButtonAppearance
        '
        ' Summary:
        '     Gets or sets the flat style appearance of the button control.
        '
        ' Returns:
        '     One of the System.Windows.Forms.FlatStyle values. The default value is Standard.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Windows.Forms.FlatStyle values.
        <DefaultValue(FlatStyle.Standard)> <Localizable(True)>
        Public Property FlatStyle As FlatStyle
        '
        ' Summary:
        '     Gets or sets the image that is displayed on a button control.
        '
        ' Returns:
        '     The System.Drawing.Image displayed on the button control. The default value is
        '     null.
        <Localizable(True)>
        Public Property Image As Image
        '
        ' Summary:
        '     Gets or sets the alignment of the image on the button control.
        '
        ' Returns:
        '     One of the System.Drawing.ContentAlignment values. The default value is MiddleCenter.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Drawing.ContentAlignment values.
        <DefaultValue(ContentAlignment.MiddleCenter)> <Localizable(True)>
        Public Property ImageAlign As ContentAlignment
        '
        ' Summary:
        '     Gets or sets the image list index value of the image displayed on the button
        '     control.
        '
        ' Returns:
        '     A zero-based index, which represents the image position in an System.Windows.Forms.ImageList.
        '     The default is -1.
        '
        ' Exceptions:
        '   T:System.ArgumentOutOfRangeException:
        '     The assigned value is less than the lower bounds of the System.Windows.Forms.ButtonBase.ImageIndex.
        <DefaultValue(-1)> <Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", GetType(UITypeEditor))> <Localizable(True)> <RefreshProperties(RefreshProperties.Repaint)> <TypeConverter(GetType(ImageIndexConverter))>
        Public Property ImageIndex As Integer
        '
        ' Summary:
        '     Gets or sets the key accessor for the image in the System.Windows.Forms.ButtonBase.ImageList.
        '
        ' Returns:
        '     A string representing the key of the image.
        <DefaultValue("")> <Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", GetType(UITypeEditor))> <Localizable(True)> <RefreshProperties(RefreshProperties.Repaint)> <TypeConverter(GetType(ImageKeyConverter))>
        Public Property ImageKey As String
        '
        ' Summary:
        '     Gets or sets the System.Windows.Forms.ImageList that contains the System.Drawing.Image
        '     displayed on a button control.
        '
        ' Returns:
        '     An System.Windows.Forms.ImageList. The default value is null.
        <RefreshProperties(RefreshProperties.Repaint)>
        Public Property ImageList As ImageList

        '
        ' Returns:
        '     The text associated with this control.
        <Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", GetType(UITypeEditor))> <SettingsBindable(True)>
        Public Overrides Property Text As String
        '
        ' Summary:
        '     Gets or sets the alignment of the text on the button control.
        '
        ' Returns:
        '     One of the System.Drawing.ContentAlignment values. The default is MiddleCenter.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Drawing.ContentAlignment values.
        <DefaultValue(ContentAlignment.MiddleCenter)> <Localizable(True)>
        Public Overridable Property TextAlign As ContentAlignment
        '
        ' Summary:
        '     Gets or sets the position of text and image relative to each other.
        '
        ' Returns:
        '     One of the values of System.Windows.Forms.TextImageRelation. The default is System.Windows.Forms.TextImageRelation.Overlay.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value is not one of the System.Windows.Forms.TextImageRelation values.
        <DefaultValue(TextImageRelation.Overlay)> <Localizable(True)>
        Public Property TextImageRelation As TextImageRelation
        '
        ' Summary:
        '     Gets or sets a value that determines whether to use the System.Drawing.Graphics
        '     class (GDI+) or the System.Windows.Forms.TextRenderer class (GDI) to render text.
        '
        ' Returns:
        '     true if the System.Drawing.Graphics class should be used to perform text rendering
        '     for compatibility with versions 1.0 and 1.1. of the .NET Framework; otherwise,
        '     false. The default is false.
        <DefaultValue(False)>
        Public Property UseCompatibleTextRendering As Boolean
        '
        ' Summary:
        '     Gets or sets a value indicating whether the first character that is preceded
        '     by an ampersand (&) is used as the mnemonic key of the control.
        '
        ' Returns:
        '     true if the first character that is preceded by an ampersand (&) is used as the
        '     mnemonic key of the control; otherwise, false. The default is true.
        <DefaultValue(True)>
        Public Property UseMnemonic As Boolean
        '
        ' Summary:
        '     Gets or sets a value that determines if the background is drawn using visual
        '     styles, if supported.
        '
        ' Returns:
        '     true if the background is drawn using visual styles; otherwise, false.
        Public Property UseVisualStyleBackColor As Boolean
    End Class

    Public Class Control
        '
        ' Summary:
        '     Gets the System.Windows.Forms.AccessibleObject assigned to the control.
        '
        ' Returns:
        '     The System.Windows.Forms.AccessibleObject assigned to the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property AccessibilityObject As AccessibleObject
        '
        ' Summary:
        '     Gets or sets the default action description of the control for use by accessibility
        '     client applications.
        '
        ' Returns:
        '     The default action description of the control for use by accessibility client
        '     applications.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property AccessibleDefaultActionDescription As String
        '
        ' Summary:
        '     Gets or sets the description of the control used by accessibility client applications.
        '
        ' Returns:
        '     The description of the control used by accessibility client applications. The
        '     default is null.
        <Localizable(True)>
        Public Property AccessibleDescription As String
        '
        ' Summary:
        '     Gets or sets the name of the control used by accessibility client applications.
        '
        ' Returns:
        '     The name of the control used by accessibility client applications. The default
        '     is null.
        <Localizable(True)>
        Public Property AccessibleName As String
        '
        ' Summary:
        '     Gets or sets the accessible role of the control
        '
        ' Returns:
        '     One of the values of System.Windows.Forms.AccessibleRole. The default is Default.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Windows.Forms.AccessibleRole values.
        <DefaultValue(AccessibleRole.Default)>
        Public Property AccessibleRole As AccessibleRole
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control can accept data that the
        '     user drags onto it.
        '
        ' Returns:
        '     true if drag-and-drop operations are allowed in the control; otherwise, false.
        '     The default is false.
        <DefaultValue(False)>
        Public Overridable Property AllowDrop As Boolean
        '
        ' Summary:
        '     Gets or sets the edges of the container to which a control is bound and determines
        '     how a control is resized with its parent.
        '
        ' Returns:
        '     A bitwise combination of the System.Windows.Forms.AnchorStyles values. The default
        '     is Top and Left.
        <DefaultValue(AnchorStyles.Top Or AnchorStyles.Left)> <Localizable(True)> <RefreshProperties(RefreshProperties.Repaint)>
        Public Overridable Property Anchor As AnchorStyles
        '
        ' Summary:
        '     Gets or sets where this control is scrolled to in System.Windows.Forms.ScrollableControl.ScrollControlIntoView(System.Windows.Forms.Control).
        '
        ' Returns:
        '     A System.Drawing.Point specifying the scroll location. The default is the upper-left
        '     corner of the control.
        <Browsable(False)> <DefaultValue(GetType(Point), "0, 0")> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overridable Property AutoScrollOffset As Point
        '
        ' Summary:
        '     This property is not relevant for this class.
        '
        ' Returns:
        '     true if enabled; otherwise, false.
        <Browsable(False)> <DefaultValue(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Never)> <Localizable(True)> <RefreshProperties(RefreshProperties.All)>
        Public Overridable Property AutoSize As Boolean
        '
        ' Summary:
        '     Gets or sets the background color for the control.
        '
        ' Returns:
        '     A System.Drawing.Color that represents the background color of the control. The
        '     default is the value of the System.Windows.Forms.Control.DefaultBackColor property.
        <DispId(-501)>
        Public Overridable Property BackColor As Color
        '
        ' Summary:
        '     Gets or sets the background image displayed in the control.
        '
        ' Returns:
        '     An System.Drawing.Image that represents the image to display in the background
        '     of the control.
        <Localizable(True)>
        Public Overridable Property BackgroundImage As Image
        '
        ' Summary:
        '     Gets or sets the background image layout as defined in the System.Windows.Forms.ImageLayout
        '     enumeration.
        '
        ' Returns:
        '     One of the values of System.Windows.Forms.ImageLayout (System.Windows.Forms.ImageLayout.Center
        '     , System.Windows.Forms.ImageLayout.None, System.Windows.Forms.ImageLayout.Stretch,
        '     System.Windows.Forms.ImageLayout.Tile, or System.Windows.Forms.ImageLayout.Zoom).
        '     System.Windows.Forms.ImageLayout.Tile is the default value.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The specified enumeration value does not exist.
        <DefaultValue(ImageLayout.Tile)> <Localizable(True)>
        Public Overridable Property BackgroundImageLayout As ImageLayout
        '
        ' Summary:
        '     Gets or sets the System.Windows.Forms.BindingContext for the control.
        '
        ' Returns:
        '     A System.Windows.Forms.BindingContext for the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overridable Property BindingContext As BindingContext
        '
        ' Summary:
        '     Gets the distance, in pixels, between the bottom edge of the control and the
        '     top edge of its container's client area.
        '
        ' Returns:
        '     An System.Int32 representing the distance, in pixels, between the bottom edge
        '     of the control and the top edge of its container's client area.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property Bottom As Integer
        '
        ' Summary:
        '     Gets or sets the size and location of the control including its nonclient elements,
        '     in pixels, relative to the parent control.
        '
        ' Returns:
        '     A System.Drawing.Rectangle in pixels relative to the parent control that represents
        '     the size and location of the control including its nonclient elements.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property Bounds As Rectangle
        '
        ' Summary:
        '     Gets a value indicating whether the control can receive focus.
        '
        ' Returns:
        '     true if the control can receive focus; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property CanFocus As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control can be selected.
        '
        ' Returns:
        '     true if the control can be selected; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property CanSelect As Boolean
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control has captured the mouse.
        '
        ' Returns:
        '     true if the control has captured the mouse; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property Capture As Boolean
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control causes validation to be performed
        '     on any controls that require validation when it receives focus.
        '
        ' Returns:
        '     true if the control causes validation to be performed on any controls requiring
        '     validation when it receives focus; otherwise, false. The default is true.
        <DefaultValue(True)>
        Public Property CausesValidation As Boolean
        '
        ' Summary:
        '     Gets the rectangle that represents the client area of the control.
        '
        ' Returns:
        '     A System.Drawing.Rectangle that represents the client area of the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property ClientRectangle As Rectangle
        '
        ' Summary:
        '     Gets or sets the height and width of the client area of the control.
        '
        ' Returns:
        '     A System.Drawing.Size that represents the dimensions of the client area of the
        '     control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property ClientSize As Size
        '
        ' Summary:
        '     Gets the name of the company or creator of the application containing the control.
        '
        ' Returns:
        '     The company name or creator of the application containing the control.
        <Browsable(False)> <Description("ControlCompanyNameDescr")> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property CompanyName As String
        '
        ' Summary:
        '     Gets a value indicating whether the control, or one of its child controls, currently
        '     has the input focus.
        '
        ' Returns:
        '     true if the control or one of its child controls currently has the input focus;
        '     otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property ContainsFocus As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control has been created.
        '
        ' Returns:
        '     true if the control has been created; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property Created As Boolean
        '
        ' Summary:
        '     Gets or sets the cursor that is displayed when the mouse pointer is over the
        '     control.
        '
        ' Returns:
        '     A System.Windows.Forms.Cursor that represents the cursor to display when the
        '     mouse pointer is over the control.
        Public Overridable Property Cursor As Cursor
        '
        ' Summary:
        '     Gets the data bindings for the control.
        '
        ' Returns:
        '     A System.Windows.Forms.ControlBindingsCollection that contains the System.Windows.Forms.Binding
        '     objects for the control.
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> <ParenthesizePropertyName(True)> <RefreshProperties(RefreshProperties.All)>
        Public ReadOnly Property DataBindings As ControlBindingsCollection
        '
        ' Summary:
        '     Gets the rectangle that represents the display area of the control.
        '
        ' Returns:
        '     A System.Drawing.Rectangle that represents the display area of the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overridable ReadOnly Property DisplayRectangle As Rectangle
        '
        ' Summary:
        '     Gets a value indicating whether the base System.Windows.Forms.Control class is
        '     in the process of disposing.
        '
        ' Returns:
        '     true if the base System.Windows.Forms.Control class is in the process of disposing;
        '     otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property Disposing As Boolean
        '
        ' Summary:
        '     Gets or sets which control borders are docked to its parent control and determines
        '     how a control is resized with its parent.
        '
        ' Returns:
        '     One of the System.Windows.Forms.DockStyle values. The default is System.Windows.Forms.DockStyle.None.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The value assigned is not one of the System.Windows.Forms.DockStyle values.
        <DefaultValue(DockStyle.None)> <Localizable(True)> <RefreshProperties(RefreshProperties.Repaint)>
        Public Overridable Property Dock As DockStyle
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control can respond to user interaction.
        '
        ' Returns:
        '     true if the control can respond to user interaction; otherwise, false. The default
        '     is true.
        <DispId(-514)> <Localizable(True)>
        Public Property Enabled As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control has input focus.
        '
        ' Returns:
        '     true if the control has focus; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overridable ReadOnly Property Focused As Boolean
        '
        ' Summary:
        '     Gets or sets the font of the text displayed by the control.
        '
        ' Returns:
        '     The System.Drawing.Font to apply to the text displayed by the control. The default
        '     is the value of the System.Windows.Forms.Control.DefaultFont property.
        <DispId(-512)> <Localizable(True)>
        Public Overridable Property Font As Font
        '
        ' Summary:
        '     Gets or sets the foreground color of the control.
        '
        ' Returns:
        '     The foreground System.Drawing.Color of the control. The default is the value
        '     of the System.Windows.Forms.Control.DefaultForeColor property.
        <DispId(-513)>
        Public Overridable Property ForeColor As Color
        '
        ' Summary:
        '     Gets the window handle that the control is bound to.
        '
        ' Returns:
        '     An System.IntPtr that contains the window handle (HWND) of the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <DispId(-515)>
        Public ReadOnly Property Handle As IntPtr
        '
        ' Summary:
        '     Gets a value indicating whether the control contains one or more child controls.
        '
        ' Returns:
        '     true if the control contains one or more child controls; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property HasChildren As Boolean
        '
        ' Summary:
        '     Gets or sets the height of the control.
        '
        ' Returns:
        '     The height of the control in pixels.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property Height As Integer
        '
        ' Summary:
        '     Gets or sets the Input Method Editor (IME) mode of the control.
        '
        ' Returns:
        '     One of the System.Windows.Forms.ImeMode values. The default is System.Windows.Forms.ImeMode.Inherit.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The assigned value is not one of the System.Windows.Forms.ImeMode enumeration
        '     values.
        <AmbientValue(ImeMode.Inherit)> <Localizable(True)>
        Public Property ImeMode As ImeMode
        '
        ' Summary:
        '     Gets a value indicating whether the caller must call an invoke method when making
        '     method calls to the control because the caller is on a different thread than
        '     the one the control was created on.
        '
        ' Returns:
        '     true if the control's System.Windows.Forms.Control.Handle was created on a different
        '     thread than the calling thread (indicating that you must make calls to the control
        '     through an invoke method); otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property InvokeRequired As Boolean
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control is visible to accessibility
        '     applications.
        '
        ' Returns:
        '     true if the control is visible to accessibility applications; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property IsAccessible As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control has been disposed of.
        '
        ' Returns:
        '     true if the control has been disposed of; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property IsDisposed As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control has a handle associated with it.
        '
        ' Returns:
        '     true if a handle has been assigned to the control; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property IsHandleCreated As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control is mirrored.
        '
        ' Returns:
        '     true if the control is mirrored; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property IsMirrored As Boolean

        '
        ' Summary:
        '     Gets or sets the distance, in pixels, between the left edge of the control and
        '     the left edge of its container's client area.
        '
        ' Returns:
        '     An System.Int32 representing the distance, in pixels, between the left edge of
        '     the control and the left edge of its container's client area.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property Left As Integer
        '
        ' Summary:
        '     Gets or sets the coordinates of the upper-left corner of the control relative
        '     to the upper-left corner of its container.
        '
        ' Returns:
        '     The System.Drawing.Point that represents the upper-left corner of the control
        '     relative to the upper-left corner of its container.
        <Localizable(True)>
        Public Property Location As Point
        '
        ' Summary:
        '     Gets or sets the space between controls.
        '
        ' Returns:
        '     A System.Windows.Forms.Padding representing the space between controls.
        <Localizable(True)>
        Public Property Margin As Padding
        '
        ' Summary:
        '     Gets or sets the size that is the upper limit that System.Windows.Forms.Control.GetPreferredSize(System.Drawing.Size)
        '     can specify.
        '
        ' Returns:
        '     An ordered pair of type System.Drawing.Size representing the width and height
        '     of a rectangle.
        <AmbientValue(GetType(Size), "0, 0")> <Localizable(True)>
        Public Overridable Property MaximumSize As Size
        '
        ' Summary:
        '     Gets or sets the size that is the lower limit that System.Windows.Forms.Control.GetPreferredSize(System.Drawing.Size)
        '     can specify.
        '
        ' Returns:
        '     An ordered pair of type System.Drawing.Size representing the width and height
        '     of a rectangle.
        <Localizable(True)>
        Public Overridable Property MinimumSize As Size
        '
        ' Summary:
        '     Gets or sets the name of the control.
        '
        ' Returns:
        '     The name of the control. The default is an empty string ("").
        <Browsable(False)>
        Public Property Name As String
        '
        ' Summary:
        '     Gets or sets padding within the control.
        '
        ' Returns:
        '     A System.Windows.Forms.Padding representing the control's internal spacing characteristics.
        <Localizable(True)>
        Public Property Padding As Padding
        '
        ' Summary:
        '     Gets or sets the parent container of the control.
        '
        ' Returns:
        '     A System.Windows.Forms.Control that represents the parent or container control
        '     of the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Parent As Control
        '
        ' Summary:
        '     Gets the size of a rectangular area into which the control can fit.
        '
        ' Returns:
        '     A System.Drawing.Size containing the height and width, in pixels.
        <Browsable(False)>
        Public ReadOnly Property PreferredSize As Size
        '
        ' Summary:
        '     Gets the product name of the assembly containing the control.
        '
        ' Returns:
        '     The product name of the assembly containing the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property ProductName As String
        '
        ' Summary:
        '     Gets the version of the assembly containing the control.
        '
        ' Returns:
        '     The file version of the assembly containing the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property ProductVersion As String
        '
        ' Summary:
        '     Gets a value indicating whether the control is currently re-creating its handle.
        '
        ' Returns:
        '     true if the control is currently re-creating its handle; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property RecreatingHandle As Boolean
        '
        ' Summary:
        '     Gets or sets the window region associated with the control.
        '
        ' Returns:
        '     The window System.Drawing.Region associated with the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property Region As Region
        '
        ' Summary:
        '     Gets the distance, in pixels, between the right edge of the control and the left
        '     edge of its container's client area.
        '
        ' Returns:
        '     An System.Int32 representing the distance, in pixels, between the right edge
        '     of the control and the left edge of its container's client area.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property Right As Integer
        '
        ' Summary:
        '     Gets or sets a value indicating whether control's elements are aligned to support
        '     locales using right-to-left fonts.
        '
        ' Returns:
        '     One of the System.Windows.Forms.RightToLeft values. The default is System.Windows.Forms.RightToLeft.Inherit.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     The assigned value is not one of the System.Windows.Forms.RightToLeft values.
        <AmbientValue(RightToLeft.Inherit)> <Localizable(True)>
        Public Overridable Property RightToLeft As RightToLeft

        '
        ' Summary:
        '     Gets or sets the height and width of the control.
        '
        ' Returns:
        '     The System.Drawing.Size that represents the height and width of the control in
        '     pixels.
        <Localizable(True)>
        Public Property Size As Size
        '
        ' Summary:
        '     Gets or sets the tab order of the control within its container.
        '
        ' Returns:
        '     The index value of the control within the set of controls within its container.
        '     The controls in the container are included in the tab order.
        <Localizable(True)> <MergableProperty(False)>
        Public Property TabIndex As Integer
        '
        ' Summary:
        '     Gets or sets a value indicating whether the user can give the focus to this control
        '     using the TAB key.
        '
        ' Returns:
        '     true if the user can give the focus to the control using the TAB key; otherwise,
        '     false. The default is true.NoteThis property will always return true for an instance
        '     of the System.Windows.Forms.Form class.
        <DefaultValue(True)> <DispId(-516)>
        Public Property TabStop As Boolean
        '
        ' Summary:
        '     Gets or sets the object that contains data about the control.
        '
        ' Returns:
        '     An System.Object that contains data about the control. The default is null.
        <Bindable(True)> <Localizable(False)> <TypeConverter(GetType(StringConverter))>
        Public Property Tag As Object
        '
        ' Summary:
        '     Gets or sets the text associated with this control.
        '
        ' Returns:
        '     The text associated with this control.
        <Bindable(True)> <DispId(-517)> <Localizable(True)>
        Public Overridable Property Text As String
        '
        ' Summary:
        '     Gets or sets the distance, in pixels, between the top edge of the control and
        '     the top edge of its container's client area.
        '
        ' Returns:
        '     An System.Int32 representing the distance, in pixels, between the bottom edge
        '     of the control and the top edge of its container's client area.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property Top As Integer

        '
        ' Summary:
        '     Gets or sets a value indicating whether to use the wait cursor for the current
        '     control and all child controls.
        '
        ' Returns:
        '     true to use the wait cursor for the current control and all child controls; otherwise,
        '     false. The default is false.
        <Browsable(True)> <DefaultValue(False)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property UseWaitCursor As Boolean
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control and all its child controls
        '     are displayed.
        '
        ' Returns:
        '     true if the control and all its child controls are displayed; otherwise, false.
        '     The default is true.
        <Localizable(True)>
        Public Property Visible As Boolean
        '
        ' Summary:
        '     Gets or sets the width of the control.
        '
        ' Returns:
        '     The width of the control in pixels.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property Width As Integer
        '
        ' Summary:
        '     This property is not relevant for this class.
        '
        ' Returns:
        '     The NativeWindow contained within the control.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Never)>
        Public Property WindowTarget As IWindowTarget
        '
        ' Summary:
        '     Gets a value indicating whether the System.Windows.Forms.Control.ImeMode property
        '     can be set to an active value, to enable IME support.
        '
        ' Returns:
        '     true in all cases.
        Protected Overridable ReadOnly Property CanEnableIme As Boolean
        '
        ' Summary:
        '     Gets the required creation parameters when the control handle is created.
        '
        ' Returns:
        '     A System.Windows.Forms.CreateParams that contains the required creation parameters
        '     when the handle to the control is created.
        Protected Overridable ReadOnly Property CreateParams As CreateParams
        '
        ' Summary:
        '     Gets or sets the default cursor for the control.
        '
        ' Returns:
        '     An object of type System.Windows.Forms.Cursor representing the current default
        '     cursor.
        Protected Overridable ReadOnly Property DefaultCursor As Cursor
        '
        ' Summary:
        '     Gets the default Input Method Editor (IME) mode supported by the control.
        '
        ' Returns:
        '     One of the System.Windows.Forms.ImeMode values.
        Protected Overridable ReadOnly Property DefaultImeMode As ImeMode
        '
        ' Summary:
        '     Gets the space, in pixels, that is specified by default between controls.
        '
        ' Returns:
        '     A System.Windows.Forms.Padding that represents the default space between controls.
        Protected Overridable ReadOnly Property DefaultMargin As Padding
        '
        ' Summary:
        '     Gets the length and height, in pixels, that is specified as the default maximum
        '     size of a control.
        '
        ' Returns:
        '     A System.Drawing.Point.#ctor(System.Drawing.Size) representing the size of the
        '     control.
        Protected Overridable ReadOnly Property DefaultMaximumSize As Size
        '
        ' Summary:
        '     Gets the length and height, in pixels, that is specified as the default minimum
        '     size of a control.
        '
        ' Returns:
        '     A System.Drawing.Size representing the size of the control.
        Protected Overridable ReadOnly Property DefaultMinimumSize As Size
        '
        ' Summary:
        '     Gets the internal spacing, in pixels, of the contents of a control.
        '
        ' Returns:
        '     A System.Windows.Forms.Padding that represents the internal spacing of the contents
        '     of a control.
        Protected Overridable ReadOnly Property DefaultPadding As Padding
        '
        ' Summary:
        '     Gets the default size of the control.
        '
        ' Returns:
        '     The default System.Drawing.Size of the control.
        Protected Overridable ReadOnly Property DefaultSize As Size
        '
        ' Summary:
        '     Gets or sets a value indicating whether this control should redraw its surface
        '     using a secondary buffer to reduce or prevent flicker.
        '
        ' Returns:
        '     true if the surface of the control should be drawn using double buffering; otherwise,
        '     false.
        Protected Overridable Property DoubleBuffered As Boolean
        '
        ' Summary:
        '     Gets or sets the height of the font of the control.
        '
        ' Returns:
        '     The height of the System.Drawing.Font of the control in pixels.
        Protected Property FontHeight As Integer
        '
        ' Summary:
        '     Gets or sets the IME mode of a control.
        '
        ' Returns:
        '     The IME mode of the control.
        Protected Overridable Property ImeModeBase As ImeMode
        '
        ' Summary:
        '     Gets or sets a value indicating whether the control redraws itself when resized.
        '
        ' Returns:
        '     true if the control redraws itself when resized; otherwise, false.
        Protected Property ResizeRedraw As Boolean
        '
        ' Summary:
        '     Gets a value that determines the scaling of child controls.
        '
        ' Returns:
        '     true if child controls will be scaled when the System.Windows.Forms.Control.Scale(System.Single)
        '     method on this control is called; otherwise, false. The default is true.
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Overridable ReadOnly Property ScaleChildren As Boolean
        '
        ' Summary:
        '     This property is now obsolete.
        '
        ' Returns:
        '     true if the control is rendered from right to left; otherwise, false. The default
        '     is false.
        <Obsolete("This property has been deprecated. Please use RightToLeft instead. http://go.microsoft.com/fwlink/?linkid=14202")>
        Protected Friend ReadOnly Property RenderRightToLeft As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the control should display focus rectangles.
        '
        ' Returns:
        '     true if the control should display focus rectangles; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Friend Overridable ReadOnly Property ShowFocusCues As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the user interface is in the appropriate state
        '     to show or hide keyboard accelerators.
        '
        ' Returns:
        '     true if the keyboard accelerators are visible; otherwise, false.
        <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Friend Overridable ReadOnly Property ShowKeyboardCues As Boolean
    End Class
End Namespace
