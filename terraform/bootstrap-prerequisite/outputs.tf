output "aws_account_id" {
  value = data.aws_caller_identity.current.account_id
}

output "aws_partition" {
  value = data.aws_partition.current.partition
}

output "aws_region" {
  value = var.aws_region
}

output "tf_state_bucket" {
  value = aws_s3_bucket.tf_state.bucket
}

output "tf_lock_table" {
  value = aws_dynamodb_table.tf_lock.name
}

output "bootstrap_role_arn" {
  value = aws_iam_role.github_actions_bootstrap.arn
}

output "bootstrap_role_name" {
  value = aws_iam_role.github_actions_bootstrap.name
}

output "github_oidc_provider_arn" {
  value = local.effective_github_oidc_provider_arn
}

output "github_subject_patterns" {
  value = local.effective_github_subject_patterns
}
