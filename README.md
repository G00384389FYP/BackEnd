# BackEnd ReadMe
## To Run 
1. cd /backend
2. dotnet run

## Installs
**JWTBearer:** dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer </br>
*The JWTBearer package is used for authentication.
This middleware is used to authenticate requests to an ASP.NET Core API using JWT tokens issued by a trusted authority, such as AAD*

**Identity Web:** dotnet add package Microsoft.Identity.Web </br>
*The Web Identity Package is also used for managing Identities. Specifically it is an ASP.NET Core library that simplifies integration with Microsoft Identity Platforms (Azure Active Directory or Azure AD B2C - Now known as Entra ID) for handling authentication and authorization in web applications and APIs. It provides additional functionality to make it easier to secure applications using OAuth2 and OpenID Connect* 
</br>
## Links


**JWTBearer:** https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbearerhandler?view=aspnetcore-8.0
**Identity Web:** https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/

## Commands Ran
**Initialization**
1. dotnet new webapi -o Backend
2. cd Backend
3. dotnet run

**Authentication**   
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer </br>
dotnet add package Microsoft.Identity.Web </br>





dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.Azure.Cosmos


**Quick access to Az Query SQL CMDS**

select * from [dbo].[UserData]

delete from [dbo].[UserData] where UserId = 