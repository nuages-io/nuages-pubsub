dotnet publish ./src/Nuages.PubSub.WebSocket/Nuages.PubSub.WebSocket.csproj --configuration Release --framework net6.0 --no-self-contained /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64
dotnet publish ./src/Nuages.PubSub.API/Nuages.PubSub.API.csproj --configuration Release --framework net6.0 --no-self-contained /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64
