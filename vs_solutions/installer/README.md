## .NET installer project

![](./Installer/Resources/Microsoft-Visual-Studio-logo-256x256.png)
![](./bootstrap/Resources/logo-microsoft-net.png)

##### Steps for Build install project

1. Open solution file: ``../Microsoft.VisualBasic.Architecture.Framework - ALL.sln``
2. Switch the solution config profile to ``installer_x64`` and selects the ``x64`` platform
3. Rebuild solution
4. Go to directory ``../../.vs/installer``, and then zip all of the output assembly file as a zip package and named this zip file as ``installer.zip``
5. Move the zip file into application resource location: ``./Installer/Resources/installer.zip``
6. Open solution file: ``./vs_installer.sln``
7. Switch the solution config profile to ``installer_x64`` and selects the ``x64`` platform
8. Rebuild solution
9. Go to directory ``./bootstrap/Resources/``
10. Removes the old zip file ``installer.zip`` if it its exists.
11. Zip all of the files in this resource directory into a zip package, and named it as ``installer.zip``
12. Build the current opened ``vs_installer.sln`` solution.
13. Installer program is at location ``./output/sciBASIC_installer.exe``