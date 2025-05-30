dotnet build C:\Users\timon\Documents\Programmieren\C#\Chess-Challenge\ConsoleBot\ConsoleBot.csproj --runtime ubuntu.22.04-x64
xcopy /Y /i /e "C:\Users\timon\Documents\Programmieren\C#\Chess-Challenge\ConsoleBot\bin\Debug\net6.0\ubuntu.22.04-x64" "\\wsl.localhost\Ubuntu-24.04\home\timon\ChessBots\EvilBot"
dotnet build C:\Users\timon\Documents\Programmieren\C#\Chess-Challenge\MyConsoleBot\MyConsoleBot.csproj --runtime ubuntu.22.04-x64
xcopy /Y /i /e "C:\Users\timon\Documents\Programmieren\C#\Chess-Challenge\MyConsoleBot\bin\Debug\net6.0\ubuntu.22.04-x64" "\\wsl.localhost\Ubuntu-24.04\home\timon\ChessBots\MyBot"
