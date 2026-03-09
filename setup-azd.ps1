# Azure Developer CLI Setup Script for Leaderboard App
# This script helps set up the environment and provision infrastructure

param(
    [string]$TenantId,
    [string]$SubscriptionId,
    [string]$Location = "eastus2",
    [string]$EnvironmentName = "lbapp",
    [string]$ResourceGroupName,
    [string]$KvSecretsGroupObjectId,
    [switch]$SkipPreview
)

Write-Host "Setting up Azure Developer CLI for Leaderboard App" -ForegroundColor Green

# Check if azd is installed
try {
    $azdVersion = azd version
    Write-Host "Azure Developer CLI is installed: $azdVersion" -ForegroundColor Green
}
catch {
    Write-Host "Azure Developer CLI not found. Please install from: https://aka.ms/azd-install" -ForegroundColor Red
    exit 1
}

# Authenticate with Azure (azd and az CLI are separate auth states — both required)
Write-Host "Authenticating with Azure Developer CLI..." -ForegroundColor Yellow
if ($TenantId) {
    Write-Host "   Using tenant ID $TenantId" -ForegroundColor Gray
    azd auth login --tenant-id $TenantId
}
else {
    azd auth login
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "Authentication failed" -ForegroundColor Red
    exit 1
}

Write-Host "Authenticating Azure CLI (required for user info and SQL token)..." -ForegroundColor Yellow
$azAccount = az account show -o json 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($azAccount)) {
    Write-Host "   No active az CLI session found. Launching device code login..." -ForegroundColor Gray
    if ($TenantId) {
        az login --tenant $TenantId --allow-no-subscriptions --use-device-code
    } else {
        az login --allow-no-subscriptions --use-device-code
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Azure CLI authentication failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   Existing az CLI session detected, skipping login." -ForegroundColor Gray
}

if ($SubscriptionId) {
    az account set --subscription $SubscriptionId
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to set active subscription to '$SubscriptionId'" -ForegroundColor Red
        exit 1
    }
}

# create or select environment
Write-Host "Creating or selecting azd environment '$EnvironmentName'..." -ForegroundColor Yellow
azd env select $EnvironmentName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Environment '$EnvironmentName' not found. Creating a new one..." -ForegroundColor Yellow
    azd env new $EnvironmentName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create new environment '$EnvironmentName'" -ForegroundColor Red
        exit 1
    }
}
# ...existing code...


# Set environment variables
Write-Host "Setting environment name to $EnvironmentName..." -ForegroundColor Yellow
azd env set AZURE_ENV_NAME $EnvironmentName

Write-Host "Setting location to $Location..." -ForegroundColor Yellow
azd env set AZURE_LOCATION $Location

if ($SubscriptionId) {
    Write-Host "Setting subscription ID..." -ForegroundColor Yellow
    azd env set AZURE_SUBSCRIPTION_ID $SubscriptionId
}

if ($TenantId) {
    Write-Host "Setting tenant ID..." -ForegroundColor Yellow
    azd env set AZURE_TENANT_ID $TenantId
}

if ($ResourceGroupName) {
    Write-Host "Targeting existing resource group '$ResourceGroupName'..." -ForegroundColor Yellow
    azd env set AZURE_RESOURCE_GROUP $ResourceGroupName
} else {
    Write-Host "No -ResourceGroupName specified; azd will create a new resource group named '$EnvironmentName-rg'." -ForegroundColor Yellow
    Write-Host "NOTE: This requires subscription-level Contributor permissions." -ForegroundColor Yellow
}

if ([string]::IsNullOrWhiteSpace($KvSecretsGroupObjectId)) {
    Write-Host "Looking up object ID for Key Vault secrets group..." -ForegroundColor Yellow
    $KvSecretsGroupObjectId = az ad group show --group "PIM-AppServices-DEVX-Support" --query id -o tsv 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($KvSecretsGroupObjectId)) {
        Write-Host "Could not resolve group 'PIM-AppServices-DEVX-Support'. Supply the object ID explicitly with -KvSecretsGroupObjectId." -ForegroundColor Red
        exit 1
    }
    Write-Host "   Resolved group object ID: $KvSecretsGroupObjectId" -ForegroundColor Gray
}
azd env set KV_SECRETS_GROUP_OBJECT_ID $KvSecretsGroupObjectId

Write-Host "Retrieving current Azure AD user information from access token..." -ForegroundColor Yellow
# Decode the JWT from az account get-access-token to avoid requiring Microsoft Graph permissions
$rawToken = az account get-access-token --query accessToken -o tsv 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($rawToken)) {
    Write-Host "Failed to acquire access token to identify the signed-in user." -ForegroundColor Red
    exit 1
}
try {
    $jwtPayload = $rawToken.Split('.')[1]
    $padded = $jwtPayload.PadRight($jwtPayload.Length + (4 - $jwtPayload.Length % 4) % 4, '=')
    $tokenData = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($padded)) | ConvertFrom-Json
    $userInfo = [PSCustomObject]@{
        objectId = $tokenData.oid
        upn      = if ($tokenData.upn) { $tokenData.upn } else { $tokenData.preferred_username }
    }
} catch {
    Write-Host "Failed to decode access token to retrieve user identity: $_" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($userInfo.objectId)) {
    Write-Host "Could not extract object ID from access token." -ForegroundColor Red
    exit 1
}
Write-Host "   Signed in as: $($userInfo.upn) ($($userInfo.objectId))" -ForegroundColor Gray

Write-Host "Detecting current public IPv4 address..." -ForegroundColor Yellow
try {
    $clientIp = (Invoke-RestMethod -Uri "https://api.ipify.org")
} catch {
    Write-Host "Failed to retrieve public IPv4 address." -ForegroundColor Red
    exit 1
}

Write-Host "Requesting Azure SQL access token for the current user..." -ForegroundColor Yellow
$sqlAccessToken = az account get-access-token --resource https://database.windows.net --query accessToken -o tsv 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sqlAccessToken)) {
    Write-Host "Failed to acquire access token for https://database.windows.net." -ForegroundColor Red
    exit 1
}

Write-Host "Persisting SQL administrator context in the azd environment..." -ForegroundColor Yellow
azd env set SQL_ADMIN_OBJECT_ID $($userInfo.objectId)
azd env set SQL_ADMIN_LOGIN $($userInfo.upn)
azd env set SQL_CLIENT_IP $clientIp
azd env set SQL_ACCESS_TOKEN $sqlAccessToken

Write-Host "WARNING: The SQL access token expires in approximately one hour. Run 'azd up' before it expires." -ForegroundColor Yellow

# Preview deployment
if ($SkipPreview) {
    Write-Host "Skipping deployment preview (-SkipPreview specified)." -ForegroundColor Yellow
} else {
    Write-Host "Running deployment preview..." -ForegroundColor Yellow
    Write-Host "This will show what resources will be created without actually creating them." -ForegroundColor Gray

    $previewOutput = azd provision --preview 2>&1
    $previewOutput | Write-Host

    if ($LASTEXITCODE -ne 0) {
        $outputStr = $previewOutput -join "`n"
        if ($outputStr -match '403|AuthorizationFailed|whatIf') {
            Write-Host ""
            Write-Host "Preview failed due to insufficient permissions (403 AuthorizationFailed)." -ForegroundColor Yellow
            Write-Host "Re-run with -SkipPreview to skip preview and go straight to 'azd up'." -ForegroundColor Yellow
        } else {
            Write-Host "Preview failed. Please check the error messages above." -ForegroundColor Red
        }
        exit 1
    }
}

Write-Host ""
Write-Host "Setup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the preview above" -ForegroundColor White
Write-Host "2. If everything looks good, run: azd up" -ForegroundColor White
Write-Host ""
Write-Host "For more information, see DEPLOYMENT.md" -ForegroundColor Gray