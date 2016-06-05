# Canvas for Network visualization

![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/Datavisualization/Datavisualization.Network/tumblr_inline_mqvdlydGCp1qz4rgp.png)
[Reddit Discussion Network Visualization](https://github.com/whichlight/reddit-network-vis)


One of Is using d3js for this data visualization. But as the .NET Webbrowser control is based on the IE browser and not support d3js well, so that I have to changes the .NET Webbrowser control to the opensource Firefox browser or Googl chrome. And by using Firefox in the project, it actually works, but the problem is that we needs deploy our application with the Chrome kernel(~100MB) and Firefox kernel(~50MB). This is not so    for my customer clients.

![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/Datavisualization/Datavisualization.Network/2016-06-05%20(9).png)

This canvas library is majority based on the work of [Mr. Woong Gyu La](http://www.codeproject.com/Articles/833043/EpForceDirectedGraph-cs-A-D-D-force-directed-gra)

This canvas library is consist with 4 parts majority:

1. Force Directed layout engine
2. Canvas control for WinForm
3. InputDevice for user mouse events
4. Renderer for the graphics rendering of the network data

### Performance issue
Both VisualBasic and C# have the performance issue of the gdi+ graphics on large image rendering. As all of the gdi+ graphics work is on the CPU. This tools works well on the small network, but it get stuck when trying to rendering a large scale network data, and the graphics display is not so smoothly.
Planning changes the graphics engine from gdi+ to Microsoft Win2D or OpenGL in the future work.

### Running the Test
![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/Datavisualization/Datavisualization.Network/2016-06-05%20(9).png)
![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/Datavisualization/Datavisualization.Network/net_test/xcb-main.png)

