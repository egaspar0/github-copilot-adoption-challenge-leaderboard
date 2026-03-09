// Azure Developer CLI main infrastructure template
// This template provisions the basic App Service infrastructure for the Leaderboard App
// Note: Targets an existing resource group. Create the RG and assign Contributor +
// User Access Administrator to the deploying identity before running azd up.

targetScope = 'resourceGroup'

// Required parameters for azd
@minLength(1)
@maxLength(64)
@description('Name of the application (used for resource naming)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
@metadata({
  azd: {
    type: 'location'
  }
})
param location string

@description('Object ID of the Azure AD user that should be configured as the SQL Server administrator.')
param sqlAdminObjectId string

@description('User principal name (UPN) of the SQL Server administrator.')
param sqlAdminLogin string

@description('Client IPv4 address that should be allowed through the SQL Server firewall.')
param sqlAdminClientIp string

@description('Object ID of the AAD group (e.g. PIM-AppServices-DEVX-Support) granted Key Vault Secrets Officer during deployment.')
param kvSecretsGroupObjectId string

// Deploy the main infrastructure
module main 'infra.bicep' = {
  name: 'main'
  params: {
    location: location
    appName: environmentName
    sqlAdminObjectId: sqlAdminObjectId
    sqlAdminLogin: sqlAdminLogin
    sqlAdminClientIp: sqlAdminClientIp
    kvSecretsGroupObjectId: kvSecretsGroupObjectId
  }
}

// Outputs required for azd
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output RESOURCE_GROUP_NAME string = resourceGroup().name
output WEB_APP_NAME string = main.outputs.webAppName
output KEY_VAULT_NAME string = main.outputs.keyVaultName
output APP_SERVICE_HOST string = main.outputs.appServiceHost
output SQL_SERVER_FQDN string = main.outputs.sqlServerFqdn

