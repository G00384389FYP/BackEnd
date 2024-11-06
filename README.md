# BackEnd ReadMe
## To Run 
1. cd /backend
2. dotnet run

## Installs
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.Identity.Web </br>
*The JWTBearer package is used for authentication:* https://auth0.com/resources/ebooks/jwt-handbook?utm_content=irloidc-jwt-jwthandbookebk&utm_source=google&utm_campaign=emea_uki_irl_all_ciam-all_dg-ao_auth0_search_google_text_kw_OIDC_utm2&utm_medium=cpc&utm_id=aNK4z000000UE5AGAW&gad_source=1&gclid=EAIaIQobChMI59rsnbXHiQMVXKRQBh0mtQO8EAAYASAAEgISVvD_BwE
</br>
*The Web Identity Package is also used for managing Identities:* https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/


## Commands Ran
1. dotnet new webapi -o Backend
2. cd Backend
3. dotnet run
