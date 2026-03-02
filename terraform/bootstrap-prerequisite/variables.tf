variable "aws_region" {
  description = "AWS region used by the provider."
  type        = string
  default     = "eu-central-1"
}

variable "github_org" {
  description = "GitHub organization that will host repositories created from the template."
  type        = string
  default     = ""
}

variable "github_oidc_role_name" {
  description = "IAM role name assumed by GitHub Actions through OIDC."
  type        = string
  default     = "gha-bootstrap-org"
}

variable "tf_state_bucket_name" {
  description = "S3 bucket name used for Terraform remote state."
  type        = string
  default     = ""
}

variable "tf_lock_table_name" {
  description = "DynamoDB table name used for Terraform state locking."
  type        = string
  default     = ""
}
