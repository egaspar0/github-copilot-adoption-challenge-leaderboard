// Parameters
param location string = resourceGroup().location
param appName string = 'leaderboardapp'
param sqlAdminObjectId string
param sqlAdminLogin string
param sqlAdminClientIp string

@description('Object ID of the AAD group granted Key Vault Secrets Officer during deployment.')
param kvSecretsGroupObjectId string

var uniqueSuffix = toLower(substring(uniqueString(resourceGroup().id), 0, 5))


// Resource names
var sqlServerName = toLower('${appName}-sql-${uniqueSuffix}')
var sanitizedAppName = replace(appName, '-', '')
var sanitizedSuffix = replace(uniqueSuffix, '-', '')
var storageAccountName = toLower(take('${sanitizedAppName}${sanitizedSuffix}', 24))

module network 'network.bicep' = {
  name: 'networkModule'
  params: {
    appName: appName
    location: location
  }
}

module utils 'utils.bicep' = {
  name: 'utilsModule'
  params: {
    appName: appName
    uniqueSuffix: uniqueSuffix
    location: location
    vnetId: network.outputs.vnetId
    subnets: network.outputs.subnets
    kvSecretsGroupObjectId: kvSecretsGroupObjectId
  }
}

// Create App Service first to get its managed identity
module appservices 'appservices.bicep' = {
  name: 'appservicesModule'
  params: {
    appName: appName
    location: location
    subnets: network.outputs.subnets
    keyVaultName: utils.outputs.keyVaultName
    sqlConnStringSecret: 'sqlConnString' // Placeholder
    efConnStringSecret: 'efConnString' // Placeholder
    sqlServerFqdn: '${sqlServerName}.${environment().suffixes.sqlServerHostname}' // Placeholder
    sqlDatabaseName: 'leaderboarddb' // Static name
    storageAccountName: storageAccountName
    uniqueSuffix: uniqueSuffix
    allowedIpAddress: sqlAdminClientIp
  }
}

// Create SQL Server with App Service's managed identity as admin
module sql 'sql.bicep' = {
  name: 'sqlModule'
  params: {
    location: location
    appName: appName
    sqlServerName: sqlServerName
    vnetId: network.outputs.vnetId
    subnets: network.outputs.subnets
    keyVaultName: utils.outputs.keyVaultName
    sqlAdminObjectId: sqlAdminObjectId
    sqlAdminLogin: sqlAdminLogin
    clientIpAddress: sqlAdminClientIp
  }
}

// Update App Service with correct SQL connection string
module appservicesUpdate 'appservices.bicep' = {
  name: 'appservicesUpdateModule'
  params: {
    appName: appName
    location: location
    subnets: network.outputs.subnets
    keyVaultName: utils.outputs.keyVaultName
    sqlConnStringSecret: sql.outputs.sqlConnStringSecret
    efConnStringSecret: sql.outputs.efConnStringSecret
    sqlServerFqdn: sql.outputs.sqlServerFqdn
    sqlDatabaseName: sql.outputs.sqlDatabaseName
    storageAccountName: storageAccountName
    uniqueSuffix: uniqueSuffix
    allowedIpAddress: sqlAdminClientIp
  }
}

output webAppName string = appservicesUpdate.outputs.webAppName
output keyVaultName string = utils.outputs.keyVaultName
output appServiceHost string = appservicesUpdate.outputs.appServiceDefaultHostName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
