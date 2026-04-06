# GeneratePlaceholderSprites.ps1
# 실제 픽셀아트 완성 전까지 쓸 플레이스홀더 스프라이트 시트를 생성한다.
# 실행: PowerShell 에서 .\Tools\GeneratePlaceholderSprites.ps1

Add-Type -AssemblyName System.Drawing

$outputBase = Join-Path $PSScriptRoot "..\Assets\Sprites\Pet"

# 클립 정의: (폴더명, 파일명, 프레임 수, R, G, B)
$clips = @(
    @{ dir="idle";  file="pet_idle.png";  frames=4; r=100; g=180; b=255 },  # 파란 계열
    @{ dir="walk";  file="pet_walk.png";  frames=6; r=100; g=220; b=120 },  # 초록 계열
    @{ dir="run";   file="pet_run.png";   frames=6; r=255; g=180; b=80  },  # 주황 계열
    @{ dir="sleep"; file="pet_sleep.png"; frames=4; r=180; g=140; b=220 },  # 보라 계열
    @{ dir="sit";   file="pet_sit.png";   frames=2; r=255; g=220; b=100 },  # 노랑 계열
    @{ dir="react"; file="pet_react.png"; frames=4; r=255; g=100; b=120 }   # 빨강 계열
)

foreach ($clip in $clips) {
    $dir    = Join-Path $outputBase $clip.dir
    $outPath = Join-Path $dir $clip.file
    $frames = $clip.frames
    $W = 128 * $frames
    $H = 128

    $bmp = New-Object System.Drawing.Bitmap($W, $H)
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)

    # 배경: Magenta (colorkey)
    $gfx.Clear([System.Drawing.Color]::FromArgb(255, 0, 255))

    $bodyColor = [System.Drawing.Color]::FromArgb($clip.r, $clip.g, $clip.b)
    $bodyBrush = New-Object System.Drawing.SolidBrush($bodyColor)
    $whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $blackBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Black)
    $darkBrush  = New-Object System.Drawing.SolidBrush(
        [System.Drawing.Color]::FromArgb([int]($clip.r*0.6), [int]($clip.g*0.6), [int]($clip.b*0.6)))

    for ($i = 0; $i -lt $frames; $i++) {
        $ox = $i * 128
        # 상태별 미세 애니메이션 오프셋
        $bounce = if ($i % 2 -eq 0) { 0 } else { 4 }

        # 몸통 (타원)
        $gfx.FillEllipse($bodyBrush,  $ox+24, 30+$bounce, 80, 70)
        # 머리 (원)
        $gfx.FillEllipse($bodyBrush,  $ox+30, 8+$bounce,  68, 60)
        # 귀
        $gfx.FillEllipse($darkBrush,  $ox+30, 4+$bounce,  18, 24)
        $gfx.FillEllipse($darkBrush,  $ox+80, 4+$bounce,  18, 24)
        # 눈 흰자
        $gfx.FillEllipse($whiteBrush, $ox+38, 22+$bounce, 16, 16)
        $gfx.FillEllipse($whiteBrush, $ox+74, 22+$bounce, 16, 16)
        # 눈동자
        $gfx.FillEllipse($blackBrush, $ox+42, 26+$bounce,  8,  8)
        $gfx.FillEllipse($blackBrush, $ox+78, 26+$bounce,  8,  8)
        # 꼬리
        $gfx.FillEllipse($darkBrush,  $ox+90, 70+$bounce, 28, 16)

        # 상태 라벨
        $font  = New-Object System.Drawing.Font("Arial", 8, [System.Drawing.FontStyle]::Bold)
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Black)
        $label = "$($clip.dir)[$i]"
        $gfx.DrawString($label, $font, $brush, ($ox+4), 105)
        $font.Dispose(); $brush.Dispose()
    }

    $bodyBrush.Dispose(); $whiteBrush.Dispose()
    $blackBrush.Dispose(); $darkBrush.Dispose()
    $gfx.Dispose()

    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()

    Write-Host "✅ 생성됨: $outPath ($frames 프레임)"
}

Write-Host ""
Write-Host "완료. Unity Editor에서 Assets > Sprites > Pet 폴더를 확인하세요."
Write-Host "실제 픽셀아트로 교체 후 SpriteAnimationBuilder 메뉴를 실행하세요."
