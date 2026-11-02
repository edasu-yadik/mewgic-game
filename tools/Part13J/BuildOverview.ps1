Add-Type -AssemblyName System.Drawing
$sheet=[System.Drawing.Bitmap]::new(1200,1710)
$g=[System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::FromArgb(16,16,39))
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$font=[System.Drawing.Font]::new('Arial',12)
try { for($n=1;$n -le 12;$n++) { $x=(($n-1)%4)*300; $y=[Math]::Floor(($n-1)/4)*570; $pic=[System.Drawing.Image]::FromFile((Join-Path (Get-Location) ('docs/ProductionReports/Part13J/Native_{0:00}.png' -f $n)));try { $g.DrawString(('Ada '+$n),$font,[System.Drawing.Brushes]::White,$x+8,$y+5);$g.DrawImage($pic,[int]$x,[int]($y+30),300,533) } finally { $pic.Dispose() } };$sheet.Save((Join-Path (Get-Location) 'docs/ProductionReports/Part13J/OVERVIEW.png'),[System.Drawing.Imaging.ImageFormat]::Png) } finally { $g.Dispose();$font.Dispose();$sheet.Dispose() }
