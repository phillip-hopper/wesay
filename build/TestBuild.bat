@echo off
@if "%INCLUDE%" == "" (
	if not "%VS100COMNTOOLS%" == "" (
		echo "Setting up VisualStudio 2010 Tools..."
		@call "%VS100COMNTOOLS%vsvars32.bat"
		goto build
	)

	if not "%VS110COMNTOOLS%" == "" (
		echo "Setting up Visual Studio Express 2010 Tools..."
		@call "%VS110COMNTOOLS%vsvars32.bat"
		goto build
	)
)
:build
@echo on

msbuild "/target:Clean;Compile" /p:Configuration=Release /p:Platform=x86 /p:RootDir=..  /p:BUILD_NUMBER="0.0.1.abcd" build.win.proj
