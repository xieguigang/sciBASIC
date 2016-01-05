http://www.codeproject.com/Articles/20879/WindowsVistaRenderer-A-New-Button-Generation

WindowsVistaRenderer: A New Button Generation

Jose Menendez Póo, 23 Nov 2007 CPOL
 
	   4.86 (135 votes)
Rate:	
vote 1vote 2vote 3vote 4vote 5
 report
ToolStripRenderer that renders Vista like buttons
Download source and demo - 23.5 KB
Screenshot - VistaRenderer.jpg
Introduction
This article demonstrates how to use the WindowsVistaRenderer and how it was created.

Background
Don't you miss the days when a button was drawn with a couple of lines to show a 3D effect? Drawing a button was as simple as drawing the light border and the shadow border. Those days are gone. Vista has arrived and our old apps may need to be renewed.

The first time you look at a button like the ones on Vista look and feel, you must feel dizzy. How to draw buttons like that? Well it turns out that it is not such a great deal. It took me a while to totally understand how these buttons are drawn, but I think the result is a good approach.

Using the Code
Thank God for the ToolStripRenderer technology.

To use this renderer on a ToolStrip you only need one line of code:

Hide   Copy Code
//
// Apply Windows Vista look and feel
//

toolStrip1.Renderer = new Renderers.WindowsVistaRenderer();
The Renderer initializes the necessary properties for the ToolStrip, except for the LayoutStyle property which I recommend to be the default HorizontalStackWithOverflow, otherwise the toolbar may look ugly in some cases.

The source solution contains a project named Renderers. Reference that DLL to your project or copy the source files to your project.

How to Draw a Button Like That
As I said before, the old-school buttons were drawn in a very easy way:

Screenshot - VistaRenderer1.gif Screenshot - VistaRenderer2.gif
The whole idea is to make the user think that he or she is really pushing a button, when clicked shadows were inverted, text pushed one pixel on x and one pixel on y, and the click effect was done.

The Vista buttons are way more complex. I've found different layers on the button drawing.

Borders

Three rounded rectangles are drown as a border, I call them the outer border, the border and the inner border.

Screenshot - VistaRenderer3.gif
Glossy Effect

A glossy effect is drawn on the north of the button. The green color represents an almost transparent color.

Screenshot - VistaRenderer4.gif
Glow

A radial gradient simulates a color glow on the south of the button. The green color represents an almost tansparent color.

Screenshot - VistaRenderer5.gif
Button Fill

Similar to the glossy effect, the inner border area is emphasized with a linear button fill from north to south.

Putting It All Together

Now, the order in which we draw these layers is critical. That order is:

Outer border
Button background color (if button is checked)
Glossy effect
Border
Button fill glossy emphasis
Inner border
Glow
Text and image of the button
Screenshot - VistaRenderer6.jpg
Some Other Details
To create the click effect, the color of the inner border and the button fill are changed. When clicked, the text is not pushed one pixel like in the old days.

When checked, the background color of the button changes. That color was extracted from the tabs on the MediaPlayer.

To make the toolbar a full-Vista-experience component, menus are drawn using the Windows Vista look and feel.

Credits
Thanks to Lukasz Swiatkowski for the methods on creating rounded rectangles and the bottom radial path.

History
15 Oct 2007: Article creation
25 Oct 2007: Subitem initialization by recursion solved (suggested by rvpilot)
21 Nov 2007: Combobox and Textbox support. Better overflow chevron.
23 Nov 2007: MenuStrip support.

License
This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)