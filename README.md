# SurfAndSell
A personal project to create a simple e-commerce program

This project runs on .NET Core 6

## How to run project:
1. Have the required packages installed
    1. Running Locally
        1. Dotnet Core Build Tools [https://dotnet.microsoft.com/en-us/download]
        2. MongoDB Community (Run on Docker or Windows Service) [https://www.mongodb.com/try/download/community]
        3. Redis (Run on Docker) [https://redis.io/download/]
        4. RabbitMQ (Optional)
    2. Running on Docker (Docker Desktop Windows or Linux Docker)
        1. Docker Desktop Windows [https://www.docker.com/products/docker-desktop/]
2. Run the solution
    1. Locally
        1. Visual Studio 2022
            1. Open UserEngine.sln in main project folder
            2. Set UserEngine.csproj as main project
            3. Start the project
        2. Command Prompt
            1. dotnet run --project ./UserEngine/UserEngine/UserEngine.csproj
    2. Docker
        1. Open PowerShell
        2. Create a stack registry ("docker service create --name registry --publish published=5000,target=5000 registry:2")
        3. Test Docker Compose on docker-compose.yml
            1. Run ("docker-compose -f docker-compose.yml -d)
            2. Once running head to https://localhost:7922/swagger/index.html
            3. Close it ("docker-compose down --volumes)
        4. Create a docker stack ("docker stack deploy --compose-file docker-compose.yml -c docker-compose.override.yml stackdemo")
        5. Increase the replica of userengine container ("docker service scale stackdemo_userengine=3")
3. Load Testing
    1. Have k6 Installed [https://k6.io/docs/get-started/installation/]
    2. Run scripts.js ("k6 run script.js")
    