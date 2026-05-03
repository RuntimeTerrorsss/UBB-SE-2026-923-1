$files = Get-ChildItem -Path "D:\University\sem4\ISS\Project 923-1\UBB-SE-2026-923-1\BoardGamesApp\BookingBoardGames.Tests" -Filter "*.cs" -Recurse

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    $original = $content
    
    $content = $content -creplace "\bInterfaceRentalsRepository\b", "IRentalRepository"
    $content = $content -creplace "\bInterfaceUsersRepository\b", "IUserRepository"
    $content = $content -creplace "\bIGameRepository\b", "InterfaceGamesRepository"
    $content = $content -creplace "\bConversationDataTransferObject\b", "ConversationDTO"
    
    if ($content -cne $original) {
        Set-Content -Path $f.FullName -Value $content -NoNewline
        Write-Output "Updated $($f.Name)"
    }
}
