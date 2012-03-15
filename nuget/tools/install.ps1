param($installPath, $toolsPath, $package, $project)

$postBuildCmds = $project.Properties.Item("PostBuildEvent").Value

$addBuildCmd = '$(TargetDir)\tools\verpatch.exe $(TargetPath) /s "Qlikview Connector" "QvxEventLog"'

$postBuildCmds = $addBuildCmd + "`r`n" + $postBuildCmds
      
$project.Properties.Item("PostBuildEvent").Value = $postBuildCmds


$item = $project.ProjectItems.Item("tools").ProjectItems.Item("licence.txt")
$item.Properties.Item("BuildAction").Value = 0
$item.Properties.Item("CopyToOutputDirectory").Value = 1

$item = $project.ProjectItems.Item("tools").ProjectItems.Item("verpatch.exe")
$item.Properties.Item("BuildAction").Value = 0
$item.Properties.Item("CopyToOutputDirectory").Value = 1