# RenameMainWindow
This Visual Studio 2019 addin allows to rename main window. This feature useful when you have several branches of single solution and work with them at the same time
![image](https://user-images.githubusercontent.com/2192524/74261375-34dbfc80-4d0c-11ea-887d-da85a4aa820e.png)


### How to use
0. Compile and install add-in
1. Create configuration file and save to `"%USERPROFILE%\Documents\Visual Studio 2019"` directory as `solution.configuration`
``` xml 
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <Solutions>
    <Solution Path="C:\Projects\SAM5\core-app\core-app.sln" DisplayName="core-app (master)" />
    <Solution Path="C:\Projects\SAM5\core-db\core-db.sln" DisplayName="core-db (master)" />
    <Solution Path="C:\Projects\netcore31\core-app\core-app.sln" DisplayName="core-app (netcore 3.1)" />
    <Solution Path="C:\Users\dykolchev.CORP\source\repos\ConsoleApp1\ConsoleApp1.sln" DisplayName="My Console Application" />
  </Solutions>
</Configuration>
```
2. Add environment variable `VS2019_SOLUTIONS` with full path to configuration file
``` cmd
set VS2019_SOLUTIONS="%USERPROFILE%\Documents\Visual Studio 2019\solution.configuration"
```
3. Start or restart Visual Studio 2019 & open required solution
