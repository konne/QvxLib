param($installPath, $toolsPath, $package, $project)

$postBuildCmd = $project.Properties.Item("PostBuildEvent").Value

if ($postBuildCmd.Contains('verpatch.exe'))
{
    $postBuildLines = [regex]::Split($postBuildCmd,"\r\n")
    $newPostBuildCmd = ""
   
   foreach($line in $postBuildLines)
   {
        if($line.Length -gt 0 -and !$line.Contains('verpatch.exe')) # remove empty lines
        {
            $newPostBuildCmd += $line + "`r`n"
        }
   }
  
  $project.Properties.Item("PostBuildEvent").Value = $newPostBuildCmd
}