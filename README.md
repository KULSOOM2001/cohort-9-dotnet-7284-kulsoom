\# cohort-9-dotnet-7284-kulsoom



Cohort 9 — .NET Fullstack (.NET+ReactJS) assignment for Kulsoom Jawed



\## Development Setup



\### JWT Configuration



The API requires a JWT signing key to run.



For local development, configure the `Jwt:Key` setting using .NET User Secrets.



From the `TaskManagement/TaskManagement.API` directory, run:



```bash

dotnet user-secrets init

dotnet user-secrets set "Jwt:Key" "your-development-secret-key-at-least-32-characters-long"

