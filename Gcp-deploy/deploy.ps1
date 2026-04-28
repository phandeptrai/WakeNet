param(
  [Parameter(Mandatory=$false)]
  [string]$Config = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$env:CLOUDSDK_CORE_DISABLE_PROMPTS = "1"
if (Get-Variable -Name PSNativeCommandUseErrorActionPreference -ErrorAction SilentlyContinue) {
  $global:PSNativeCommandUseErrorActionPreference = $false
}

function Read-Json($path) {
  if (!(Test-Path $path)) { throw "Config not found: $path" }
  return (Get-Content $path -Raw | ConvertFrom-Json)
}

function Exec($label, $cmd) {
  Write-Host $label
  & $cmd
  if ($LASTEXITCODE -ne 0) {
    throw "Command failed ($LASTEXITCODE): $label"
  }
}

if ([string]::IsNullOrWhiteSpace($Config)) {
  $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
  $Config = Join-Path $scriptDir "deploy.config.json"
}

$cfg = Read-Json $Config

$projectId = $cfg.projectId
$region = $cfg.region
$zone = $cfg.zone
$vmName = $cfg.vmName
$machineType = $cfg.machineType
$bootDiskGb = [int]$cfg.bootDiskGb
$networkTag = $cfg.networkTag
$firewallRuleName = $cfg.firewallRuleName

$arRegion = $cfg.artifactRegistryRegion
$repoName = $cfg.artifactRepoName
$imageName = $cfg.imageName
$imageTag = $cfg.imageTag

$containerPort = [int]$cfg.containerPort
$publicPort = [int]$cfg.publicPort

if ($machineType -ne "e2-standard-2") {
  Write-Warning "machineType is '$machineType'. 8GB RAM tương ứng e2-standard-2."
}

Write-Host "Using project: $projectId"
Exec "gcloud config set project" { gcloud --quiet config set project $projectId | Out-Null }
Exec "gcloud config set compute/region" { gcloud --quiet config set compute/region $region | Out-Null }
Exec "gcloud config set compute/zone" { gcloud --quiet config set compute/zone $zone | Out-Null }

Exec "Enable required APIs..." { gcloud services enable compute.googleapis.com artifactregistry.googleapis.com --quiet | Out-Null }

Write-Host "Ensure Artifact Registry repo exists..."
$repoExists = $false
$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"
gcloud artifacts repositories describe $repoName --location $arRegion | Out-Null
$ErrorActionPreference = $prevEap
if ($LASTEXITCODE -eq 0) { $repoExists = $true } else { $repoExists = $false }

if (-not $repoExists) {
  Write-Host "Create Artifact Registry repo '$repoName'..."
  $prevEap = $ErrorActionPreference
  $ErrorActionPreference = "Continue"
  $out = & gcloud artifacts repositories create $repoName `
    --repository-format=docker `
    --location=$arRegion `
    --description="WakeNet docker images" 2>&1
  $ErrorActionPreference = $prevEap

  if ($LASTEXITCODE -ne 0) {
    $outText = ($out | Out-String)
    if ($outText -match "ALREADY_EXISTS") {
      Write-Host "Repo already exists. Continuing."
    } else {
      throw "Create Artifact Registry repo failed ($LASTEXITCODE): $outText"
    }
  }
}

$image = "$arRegion-docker.pkg.dev/$projectId/$repoName/$imageName`:$imageTag"

Write-Host "Configure docker auth..."
Exec "Configure docker auth..." { gcloud auth configure-docker "$arRegion-docker.pkg.dev" --quiet | Out-Null }

Write-Host "Build & push image: $image"
Exec "Docker build" { docker build -f .\Gcp-deploy\Dockerfile -t $image . | Out-Null }
Exec "Docker push" { docker push $image | Out-Null }

Write-Host "Ensure firewall rule exists..."
$fwExists = $false
$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"
gcloud compute firewall-rules describe $firewallRuleName | Out-Null
$ErrorActionPreference = $prevEap
if ($LASTEXITCODE -eq 0) { $fwExists = $true } else { $fwExists = $false }

if (-not $fwExists) {
  Exec "Create firewall rule '$firewallRuleName'..." {
    gcloud compute firewall-rules create $firewallRuleName `
      --allow "tcp:80,tcp:443" `
      --target-tags $networkTag `
      --description "Allow HTTP/HTTPS for WakeNet" | Out-Null
  }
}

Write-Host "Ensure VM exists..."
$vmExists = $false
$prevEap = $ErrorActionPreference
$ErrorActionPreference = "Continue"
gcloud compute instances describe $vmName | Out-Null
$ErrorActionPreference = $prevEap
if ($LASTEXITCODE -eq 0) {
  $vmExists = $true
} else {
  # VM may exist in another zone (previous run picked a different zone)
  $vmExists = $false
  foreach ($suffix in @("a","b","c","d")) {
    $z = "$region-$suffix"
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    gcloud compute instances describe $vmName --zone $z | Out-Null
    $ErrorActionPreference = $prevEap
    if ($LASTEXITCODE -eq 0) {
      $vmExists = $true
      $zone = $z
      Exec "gcloud config set compute/zone" { gcloud --quiet config set compute/zone $zone | Out-Null }
      break
    }
  }
}

if (-not $vmExists) {
  $zonesToTry = @($zone)
  foreach ($suffix in @("a","b","c","d")) {
    $z = "$region-$suffix"
    if ($z -ne $zone) { $zonesToTry += $z }
  }

  $created = $false
  foreach ($z in $zonesToTry) {
    Write-Host "Create VM '$vmName' in zone '$z'..."
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    $out = & gcloud compute instances create $vmName `
      --zone $z `
      --machine-type $machineType `
      --boot-disk-size $bootDiskGb `
      --image-family debian-12 `
      --image-project debian-cloud `
      --tags $networkTag `
      --metadata-from-file "startup-script=Gcp-deploy/startup.sh" `
      --metadata "wakenet_image=$image,wakenet_container_port=$containerPort,wakenet_public_port=$publicPort" 2>&1
    $ErrorActionPreference = $prevEap

    if ($LASTEXITCODE -eq 0) {
      $created = $true
      $zone = $z
      Exec "gcloud config set compute/zone" { gcloud --quiet config set compute/zone $zone | Out-Null }
      break
    }

    $outText = ($out | Out-String)
    # Ignore noisy gcloud wrapper warnings
    $outText = ($outText -replace '(?ms)^python\.exe\s*: WARNING:.*?FullyQualifiedErrorId\s*:\s*NativeCommandError\s*$', '').Trim()
    if ([string]::IsNullOrWhiteSpace($outText)) { $outText = ($out | Out-String) }

    if ($outText -match "already exists") {
      $created = $true
      $zone = $z
      Exec "gcloud config set compute/zone" { gcloud --quiet config set compute/zone $zone | Out-Null }
      break
    }
    if ($outText -match "ZONE_RESOURCE_POOL_EXHAUSTED") {
      Write-Warning "Zone '$z' out of capacity. Trying next zone..."
      continue
    }

    throw "Create VM failed in zone '$z': $outText"
  }

  if (-not $created) {
    throw "Create VM failed: no zone available in region '$region'. Try again later or change machineType/region."
  }
} else {
  Write-Host "VM already exists. Updating metadata + restarting..."
  Exec "Update VM metadata..." {
    gcloud compute instances add-metadata $vmName `
      --metadata-from-file "startup-script=Gcp-deploy/startup.sh" `
      --metadata "wakenet_image=$image,wakenet_container_port=$containerPort,wakenet_public_port=$publicPort" | Out-Null
  }
  Exec "Reset VM..." { gcloud compute instances reset $vmName | Out-Null }
}

Write-Host "Done."
Write-Host "Check external IP:"
Exec "Get VM external IP" { gcloud compute instances describe $vmName --zone $zone --format="get(networkInterfaces[0].accessConfigs[0].natIP)" }

