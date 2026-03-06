[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^ou-[a-z0-9-]+$')]
    [string]$OuId,

    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-AccountsForParent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ParentId
    )

    $all = @()
    $nextToken = $null

    do {
        $args = @(
            'organizations', 'list-accounts-for-parent',
            '--parent-id', $ParentId,
            '--output', 'json'
        )

        if ($nextToken) {
            $args += @('--starting-token', $nextToken)
        }

        $raw = aws @args
        if ($LASTEXITCODE -ne 0) {
            throw "aws list-accounts-for-parent failed for parent '$ParentId'."
        }

        $page = $raw | ConvertFrom-Json

        $hasAccountsProperty = $page.PSObject.Properties.Name -contains 'Accounts'
        if ($hasAccountsProperty -and $page.Accounts) {
            $all += $page.Accounts
        }

        $hasNextTokenProperty = $page.PSObject.Properties.Name -contains 'NextToken'
        if ($hasNextTokenProperty -and $page.NextToken) {
            $nextToken = [string]$page.NextToken
        } else {
            $nextToken = $null
        }
    } while ($nextToken)

    return $all
}

if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    throw "AWS CLI ('aws') is not available in PATH."
}

Write-Host "Reading accounts in OU: $OuId"
$accounts = Get-AccountsForParent -ParentId $OuId

if (-not $accounts -or $accounts.Count -eq 0) {
    Write-Host "No accounts found in OU '$OuId'."
    exit 0
}

# CloseAccount is valid only for ACTIVE accounts.
$closable = @(
    $accounts | Where-Object {
        ($_.PSObject.Properties.Name -contains 'Status' -and $_.Status -eq 'ACTIVE') -or
        ($_.PSObject.Properties.Name -contains 'State' -and $_.State -eq 'ACTIVE')
    }
)

if ($closable.Count -eq 0) {
    Write-Host "No ACTIVE accounts to close in OU '$OuId'."
    Write-Host "Current account states:"
    $accounts |
        Select-Object Id, Name, Email, Status, State |
        Format-Table -AutoSize
    exit 0
}

Write-Host ""
Write-Host "Accounts that will be submitted for closure:"
$closable |
    Select-Object Id, Name, Email, Status, State |
    Format-Table -AutoSize

if (-not $Force) {
    $confirmation = Read-Host "Type CLOSE to continue"
    if ($confirmation -ne 'CLOSE') {
        Write-Host "Aborted."
        exit 1
    }
}

$failed = @()

foreach ($account in $closable) {
    $target = "$($account.Name) ($($account.Id))"
    if ($PSCmdlet.ShouldProcess($target, 'Close AWS account')) {
        Write-Host "Submitting CloseAccount for $target ..."
        $result = aws organizations close-account --account-id $account.Id 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Failed to close $target"
            Write-Warning $result
            $failed += [pscustomobject]@{
                Id     = $account.Id
                Name   = $account.Name
                Reason = ($result -join "`n")
            }
        }
    }
}

Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "Done with errors. Failed accounts:"
    $failed | Format-Table -AutoSize
    exit 2
}

Write-Host "Done. CloseAccount requests submitted for all ACTIVE accounts in OU '$OuId'."
Write-Host "Account closure is asynchronous. Status may take time to change."
