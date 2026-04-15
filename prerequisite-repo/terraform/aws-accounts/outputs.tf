output "ou_id" {
  value = aws_organizations_organizational_unit.app.id
}

output "ou_arn" {
  value = aws_organizations_organizational_unit.app.arn
}

output "ou_name" {
  value = aws_organizations_organizational_unit.app.name
}

output "account_ids" {
  value = {
    for key, account in merge(aws_organizations_account.app_safe, aws_organizations_account.app_debug) : key => account.id
  }
}

output "account_arns" {
  value = {
    for key, account in merge(aws_organizations_account.app_safe, aws_organizations_account.app_debug) : key => account.arn
  }
}

output "account_emails" {
  value = {
    for key, account in merge(aws_organizations_account.app_safe, aws_organizations_account.app_debug) : key => account.email
  }
}
